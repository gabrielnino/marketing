using Application.PixVerse;
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
    public class TextToVideoClient(
        IPixVerseRequestHandler requestHandler,
        ILogger<TextToVideoClient> logger,
        IErrorHandler errorHandler
    ) : ITextToVideoClient
    {
        private readonly IPixVerseRequestHandler _handler = requestHandler;
        private readonly ILogger<TextToVideoClient> _logger = logger;
        private readonly IErrorHandler _error = errorHandler;

        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public async Task<Operation<JobReceipt>> SubmitTAsync(
            TextToVideo request,
            CancellationToken ct = default)
        {
            var operation = "PixVerse.TextToVideoClient.SubmitTAsync";
            _logger.LogInformation("Starting SubmitTextToVideo");

            try
            {
                request.Validate();

                var payload = JsonSerializer.Serialize(request, JsonOpts);
                 ApiPayloadLogger.LogResponse(
                    _logger,
                    Guid.NewGuid().ToString("N"), // Create a temp ID for logging if needed, or update logger signature
                    operation,
                    payload
                );

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var resultOp = await _handler.PostAsync<SubmitResp>(Api.TextToVideoPath, content, ct);

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
                _logger.LogError(ex, "FAILED SubmitTextToVideo");
                return _error.Fail<JobReceipt>(ex, "Submit failed");
            }
        }
    }
}
