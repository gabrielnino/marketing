using Bootstrapper;
using Microsoft.Extensions.DependencyInjection;
using Services.Abstractions.OpenAI.news;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = AppHostBuilder.Create(args).Build();
        ////var trackedLink = host.Services.GetRequiredService<ITrackedLink>();
        ////await trackedLink.UpsertAsync("google", "https://www.google.com/");

        var runner = host.Services.GetRequiredService<IJsonPromptRunner>();
        await runner.RunStrictJsonAsync();

        //string apiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");
        //if (string.IsNullOrEmpty(apiKey))
        //{
        //    Console.WriteLine("Please set the OPENAI_API_KEY environment variable.");
        // //   return string.Empty;
        //}

        //using var httpClient = new HttpClient();
        //httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        //var requestBody = new
        //{
        //    model = "deepseek-chat",//_openAI.Value.Model,
        //    messages = new[]
        //    {
        //        new { role = "system", content = "You are a helpful assistant." },
        //        new { role = "user", content = "write note to introduce you." }
        //    },
        //    stream = false
        //};

        //string jsonBody = JsonConvert.SerializeObject(requestBody);
        //var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        //var response = await httpClient.PostAsync("https://api.deepseek.com/v1/chat/completions", content);

        //if (response.IsSuccessStatusCode)
        //{
        //    string responseBody = await response.Content.ReadAsStringAsync();
        //    dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
        //    Console.WriteLine(jsonResponse.choices[0].message.content);
        //}
        //else
        //{
        //    Console.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        //}


    }
}