using Configuration;
using Domain.WhatsApp.OpenAI;
using Services.WhatsApp.Abstractions.OpenAI;
using System.Net;
using System.Net.Http.Json;

namespace Services.WhatsApp.OpenAI
{
    public class OpenAIClient(OpenAIConfig openAI, HttpClient httpClient) : IOpenAIClient
    {
        private readonly OpenAIConfig _openAI = openAI;
        private readonly HttpClient _httpClient = httpClient;

        public async Task<string> GetChatCompletionAsync(Prompt prompt, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(prompt);

            if (string.IsNullOrWhiteSpace(prompt.SystemContent))
                throw new ArgumentException("SystemContent cannot be null or whitespace.", nameof(prompt.SystemContent));

            if (string.IsNullOrWhiteSpace(prompt.UserContent))
                throw new ArgumentException("UserContent cannot be null or whitespace.", nameof(prompt.UserContent));

            var request = new OpenAIChatRequest(_openAI.Model)
            {
                Messages =
                [
                    new() { Role = "system", Content = prompt.SystemContent },
                    new() { Role = "user",   Content = prompt.UserContent }
                ]
            };

            // Minimal resilience: retry a few times on transient failures (429/503/504)
            const int maxAttempts = 3;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            var response = await _httpClient.PostAsync(requestUri, content, ct);
                using var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", request, ct);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<OpenAIChatResponse>(cancellationToken: ct);

                    if (responseData?.Choices == null || responseData.Choices.Count == 0)
                        throw new Exception("No response received from OpenAI API.");

                    var content = responseData.Choices[0]?.Message?.Content;
                    if (string.IsNullOrWhiteSpace(content))
                        throw new Exception("OpenAI API returned an empty completion.");

                    return content.Trim();
                }

                var status = response.StatusCode;

                // Retry only on clearly transient statuses
                var isTransient =
                    status == (HttpStatusCode)429 ||
                    status == HttpStatusCode.ServiceUnavailable ||   // 503
                    status == HttpStatusCode.GatewayTimeout;         // 504

                if (!isTransient || attempt == maxAttempts)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(ct);
                    throw new Exception($"OpenAI API request failed with status code {status}: {errorContent}");
                }

                // Respect Retry-After if present; otherwise exponential backoff
                TimeSpan delay;
                if (response.Headers.RetryAfter?.Delta is { } retryAfterDelta)
                    delay = retryAfterDelta;
                else
                    delay = TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt - 1)); // 250ms, 500ms, 1000ms

                await Task.Delay(delay, ct);
            }

            // Unreachable, but keeps compiler happy
            throw new Exception("Unexpected failure executing OpenAI request.");
        }
    }
}
