using Configuration;
using Domain.WhatsApp.OpenAI;
using Services.WhatsApp.Abstractions.OpenAI;
using System.Net.Http.Json;

namespace Services.WhatsApp.OpenAI
{
    public class OpenAIClient(OpenAIConfig openAI, HttpClient httpClient) : IOpenAIClient
    {
        private readonly OpenAIConfig _openAI= openAI;
        private readonly HttpClient _httpClient = httpClient;

        public async Task<string> GetChatCompletionAsync(Prompt prompt)
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

            var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI API request failed with status code {response.StatusCode}: {errorContent}");
            }

            var responseData = await response.Content.ReadFromJsonAsync<OpenAIChatResponse>();

            if (responseData?.Choices == null || responseData.Choices.Count == 0)
                throw new Exception("No response received from OpenAI API.");

            return responseData.Choices[0].Message.Content.Trim();
        }
    }
}
