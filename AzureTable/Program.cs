using Application.TrackedLinks;
using Bootstrapper;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = AppHostBuilder.Create(args).Build();
        var trackedLink = host.Services.GetRequiredService<ITrackedLink>();
        await trackedLink.UpsertAsync("google", "https://www.google.com/");
    }
}