using Application.PixVerse;
using Application.PixVerse.Http;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;
using Infrastructure.Logging;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Http;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Infrastructure.PixVerse
{
    public class ImageToVideoClient(
        IPixVerseRequestHandler requestHandler,
        ILogger<ImageToVideoClient> logger,
        IErrorHandler errorHandler
    ) : IImageToVideoClient
    {
        private readonly IPixVerseRequestHandler _handler = requestHandler;
        private readonly ILogger<ImageToVideoClient> _logger = logger;
        private readonly IErrorHandler _error = errorHandler;

        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public async Task<Operation<JobReceipt>> SubmitAsync(
            ImageToVideo request,
            CancellationToken ct = default)
        {
            var operation = "PixVerse.ImageToVideoClient.SubmitAsync";
            _logger.LogInformation("Starting SubmitImageToVideo");

            try
            {
                request.Validate();

                var payload = JsonSerializer.Serialize(request, JsonOpts);
                 ApiPayloadLogger.LogResponse(
                    _logger,
                    Guid.NewGuid().ToString("N"), 
                    operation,
                    payload
                );

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var resultOp = await _handler.PostAsync<I2VSubmitResp>(Api.ImageToVideoPath, content, ct);

                if (!resultOp.IsSuccessful)
                    return _error.Fail<JobReceipt>(null, resultOp.Message);

                if (resultOp.Data == null || resultOp.Data.VideoId == 0)
                     return _error.Fail<JobReceipt>(null, "Invalid submit response (missing VideoId).");

                var submitted = new JobReceipt
                {
                    JobId = resultOp.Data.VideoId,
                    Message = resultOp.Message
                };

                return Operation<JobReceipt>.Success(submitted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED SubmitImageToVideo");
                return _error.Fail<JobReceipt>(ex, "SubmitImageToVideo failed");
            }
        }
    }
}
