using Bootstrapper;
using Microsoft.Extensions.DependencyInjection;
using Services.Abstractions.OpenAI.news;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = AppHostBuilder.Create(args).Build();
        var runner = host.Services.GetRequiredService<IJsonPromptRunner>();
        await runner.RunStrictJsonAsync();

        ////var trackedLink = host.Services.GetRequiredService<ITrackedLink>();
        ////await trackedLink.UpsertAsync("google", "https://www.google.com/");

        //using var httpClient = new HttpClient();
        //httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);



    }
}