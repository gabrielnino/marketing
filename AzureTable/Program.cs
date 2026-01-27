using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Bootstrapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AzureTable
{
    /// <summary>
    /// Fix principal:
    /// - NO usar el JobId del Image->Video como source_video_id del LipSync.
    /// - Extraer del "result" el VideoMediaId (o media id equivalente) y enviarlo como video_media_id.
    ///
    /// El log muestra que estabas mandando:
    ///   SourceVideoId = {JobId del I2V}
    /// y PixVerse responde ErrCode=500047 "provided media is invalid".
    /// </summary>
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = AppHostBuilder.Create(args).Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            var imageClient = host.Services.GetRequiredService<IImageClient>();
            var balanceClient = host.Services.GetRequiredService<IBalanceClient>();
            var generationClient = host.Services.GetRequiredService<IGenerationClient>();
            var imageToVideoClient = host.Services.GetRequiredService<IImageToVideoClient>();
            var lipSyncClient = host.Services.GetRequiredService<ILipSyncClient>();

            logger.LogInformation("=== START PixVerse IMAGE->VIDEO + LIPSYNC(TTS) (FIXED) ===");

            // -------------------------------------------------
            // 1) Check balance
            // -------------------------------------------------
            var balOp = await balanceClient.GetAsync();
            if (!balOp.IsSuccessful)
            {
                logger.LogError("Balance failed: {Error}", balOp.Error ?? "unknown");
                return;
            }

            logger.LogInformation("Balance OK. Credits={Credits}", balOp.Data?.TotalCredits);

            // -------------------------------------------------
            // 2) Upload image
            // -------------------------------------------------
            var imagePath = @"E:\Marketing-Logs\PixVerse\Inputs\luisNino.jpg"; // AJUSTA
            await using var imgStream = File.OpenRead(imagePath);

            var upOp = await imageClient.UploadAsync(
                imgStream,
                fileName: Path.GetFileName(imagePath),
                contentType: "image/jpeg");

            if (!upOp.IsSuccessful || upOp.Data is null)
            {
                logger.LogError("UploadImage failed: {Error}", upOp.Error ?? "unknown");
                return;
            }

            var imgId = upOp.Data.ImgId;
            logger.LogInformation("Image uploaded. ImgId={ImgId}", imgId);

            // -------------------------------------------------
            // 3) Submit Image->Video
            // -------------------------------------------------
            var i2vReq = new ImageToVideo
            {
                ImgId = imgId,
                Duration = 5,
                Model = "v5",
                Quality = "540p",
                Prompt = "adult guy speaking directly to camera, serious style, expressive mouth, clear face, neutral background",
                NegativePrompt = "blurry, distorted face, artifacts",
                Seed = 0
            };

            var i2vSubmitOp = await imageToVideoClient.SubmitAsync(i2vReq);
            if (!i2vSubmitOp.IsSuccessful || i2vSubmitOp.Data is null)
            {
                logger.LogError("SubmitImageToVideo failed: {Error}", i2vSubmitOp.Error ?? "unknown");
                return;
            }

            var jobId = i2vSubmitOp.Data.JobId;
            logger.LogInformation("Image-to-Video submitted. JobId={JobId}", jobId);

            // -------------------------------------------------
            // 4) Esperar a que el job termine (status)
            // -------------------------------------------------
            GenerationStatus? finalStatus = null;

            for (var attempt = 1; attempt <= 60; attempt++)
            {
                var stOp = await generationClient.GetStatusAsync(jobId);
                if (!stOp.IsSuccessful || stOp.Data is null)
                {
                    logger.LogWarning("GetStatus attempt {Attempt} failed: {Error}", attempt, stOp.Error ?? "unknown");
                }
                else
                {
                    logger.LogInformation("[I2V] attempt={Attempt} state={State}", attempt, stOp.Data.State);

                    if (stOp.Data.IsTerminal)
                    {
                        finalStatus = stOp.Data;
                        break;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            if(finalStatus is null)
            {
                return;
            }
            if (finalStatus is null || finalStatus.State != JobState.Succeeded)
            {
                logger.LogError("I2V job did not succeed. JobId={JobId} State={State}", jobId, finalStatus.State);
                return;
            }

            // -------------------------------------------------
            // 5) Obtener el RESULT y extraer VideoMediaId
            //    FIX: aquí está la corrección clave.
            // -------------------------------------------------
            var resOp = await generationClient.GetResultAsync(jobId);
            if (!resOp.IsSuccessful || resOp.Data is null)
            {
                logger.LogError("GetGenerationResult failed: {Error}", resOp.Error ?? "unknown");
                return;
            }

            // A) Caso ideal: tu modelo YA tiene VideoMediaId (o similar)
            //    Si tu PixVerseGenerationResult no lo tiene, ver B) parsing por JSON abajo.
            long? videoMediaId = TryGetVideoMediaIdFromKnownModel(resOp.Data);

            // B) Fallback: si tu implementación expone RawJson, o si puedes re-serializar el objeto,
            //    intentamos encontrar fields comunes.
            if (videoMediaId is null || videoMediaId <= 0)
            {
                var raw = SafeSerialize(resOp.Data);
                videoMediaId = TryExtractMediaIdFromJson(raw);

                logger.LogInformation(
                    "MediaId extraction: VideoMediaId={VideoMediaId} (from json fallback)",
                    videoMediaId ?? 0);
            }

            if (videoMediaId is null || videoMediaId <= 0)
            {
                // Este log es el que te evita “adivinar” y te dice por qué NO puedes seguir.
                var raw = SafeSerialize(resOp.Data);
                logger.LogError(
                    "Cannot proceed to LipSync because no VideoMediaId could be extracted from result. " +
                    "JobId={JobId}. ResultJson={ResultJson}",
                    jobId,
                    raw);

                return;
            }

            logger.LogInformation("Using VideoMediaId={VideoMediaId} for LipSync (NOT JobId={JobId}).", videoMediaId, jobId);

            // -------------------------------------------------
            // 6) Submit LipSync (TTS) usando video_media_id
            // -------------------------------------------------
            var lipReq = new LipSync
            {
                // FIX: usar VideoMediaId (media id), y NO usar SourceVideoId = jobId
                VideoMediaId = videoMediaId.Value,
                SourceVideoId = null,

                // TTS (no audio_media_id)
                AudioMediaId = null,
                LipSyncTtsSpeakerId = "auto",
                LipSyncTtsContent = "¡Hola Vancouver! Soy Goku. No olviden apoyar al Tricolor Fan Club. ¡Vamos con toda!"
            };

            var lipOp = await lipSyncClient.SubmitJobAsync(lipReq);

            if (!lipOp.IsSuccessful)
            {
                logger.LogError("LipSync submit failed: {Error}", lipOp.Error ?? "unknown");
                return;
            }

            logger.LogInformation("LipSync submitted OK. JobId={LipJobId}", lipOp.Data?.JobId);
        }

        /// <summary>
        /// Si tu PixVerseGenerationResult ya contiene algo tipo VideoMediaId, mapea aquí.
        /// Si no existe, devuelve null y se usa el parsing por JSON.
        /// </summary>
        private static long? TryGetVideoMediaIdFromKnownModel(Generation result)
        {
            // AJUSTA esto a TU modelo real si ya existe un campo:
            // return result.VideoMediaId;
            // o return result.Resp?.VideoMediaId;
            // Por defecto: null (para forzar fallback JSON).
            return null;
        }

        private static string SafeSerialize<T>(T obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj);
            }
            catch
            {
                return "(serialize_failed)";
            }
        }

        /// <summary>
        /// Busca claves comunes en respuestas de APIs: video_media_id, media_id, videoMediaId, etc.
        /// Devuelve null si no encuentra nada.
        /// </summary>
        private static long? TryExtractMediaIdFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "(serialize_failed)")
                return null;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // búsqueda profunda en todo el árbol
                var candidates = new[]
                {
                    "video_media_id",
                    "videoMediaId",
                    "media_id",
                    "mediaId",
                    "video_id",
                    "videoId"
                };

                foreach (var c in candidates)
                {
                    if (TryFindLong(root, c, out var value) && value > 0)
                        return value;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool TryFindLong(JsonElement el, string propertyName, out long value)
        {
            value = 0;

            if (el.ValueKind == JsonValueKind.Object)
            {
                if (el.TryGetProperty(propertyName, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt64(out value))
                        return true;

                    if (prop.ValueKind == JsonValueKind.String && long.TryParse(prop.GetString(), out value))
                        return true;
                }

                foreach (var p in el.EnumerateObject())
                {
                    if (TryFindLong(p.Value, propertyName, out value))
                        return true;
                }
            }
            else if (el.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in el.EnumerateArray())
                {
                    if (TryFindLong(item, propertyName, out value))
                        return true;
                }
            }

            return false;
        }
    }
}
