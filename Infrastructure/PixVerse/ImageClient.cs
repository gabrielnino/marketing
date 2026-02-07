using Application.PixVerse;
using Application.PixVerse.Http;
using Application.PixVerse.Response;
using Application.Result;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;

namespace Infrastructure.PixVerse
{
    public sealed partial class ImageClient(
        IPixVerseRequestHandler requestHandler,
        ILogger<ImageClient> logger,
        IErrorHandler errorHandler
    ) : IImageClient
    {
        private readonly IPixVerseRequestHandler _handler = requestHandler;
        private readonly ILogger<ImageClient> _logger = logger;
        private readonly IErrorHandler _error = errorHandler;

        public async Task<Operation<ImageResult>> UploadAsync(
            Stream imageStream,
            string fileName,
            string contentType,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Starting UploadImage (file). FileName={FileName} ContentType={ContentType}", fileName, contentType);

            try
            {
                if (imageStream is null) return _error.Business<ImageResult>("imageStream cannot be null.");
                if (!imageStream.CanRead) return _error.Business<ImageResult>("imageStream must be readable.");
                if (string.IsNullOrWhiteSpace(fileName)) return _error.Business<ImageResult>("fileName cannot be null or empty.");
                if (string.IsNullOrWhiteSpace(contentType)) return _error.Business<ImageResult>("contentType cannot be null or empty.");

                if (!Api.AllowedImageMimeTypes.Contains(contentType))
                    return _error.Business<ImageResult>($"Unsupported contentType '{contentType}'. Allowed: {string.Join(", ", Api.AllowedImageMimeTypes)}");

                var ext = Path.GetExtension(fileName);
                if (string.IsNullOrWhiteSpace(ext) || !Api.AllowedExtensions.Contains(ext))
                    return _error.Business<ImageResult>($"Unsupported file extension '{ext}'. Allowed: {string.Join(", ", Api.AllowedExtensions)}");

                if (imageStream.CanSeek)
                {
                    const long maxBytes = 20L * 1024L * 1024L;
                    if (imageStream.Length > maxBytes)
                        return _error.Business<ImageResult>("Image file size must be < 20MB.");
                    imageStream.Position = 0;
                }

                using var form = new MultipartFormDataContent();
                var fileContent = new StreamContent(imageStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                form.Add(fileContent, "image", fileName);

                return await _handler.PostAsync<ImageResult>(Api.UploadImagePath, form, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED UploadImage (file)");
                return _error.Fail<ImageResult>(ex, "Upload image failed");
            }
        }

        public async Task<Operation<ImageResult>> UploadAsync(string imageUrl, CancellationToken ct = default)
        {
            _logger.LogInformation("Starting UploadImage (url). Url={Url}", imageUrl);

            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                    return _error.Business<ImageResult>("imageUrl cannot be null or empty.");

                if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                    return _error.Business<ImageResult>("imageUrl must be a valid http/https absolute URL.");

                using var form = new MultipartFormDataContent
                {
                    { new StringContent(imageUrl, Encoding.UTF8), "image_url" }
                };

                return await _handler.PostAsync<ImageResult>(Api.UploadImagePath, form, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED UploadImage (url)");
                return _error.Fail<ImageResult>(ex, "Upload image failed");
            }
        }
    }
}
