namespace Tool.Program
{
    internal class Services
    {
        protected static void ConfigureServices(WebApplicationBuilder builder)
        {
            var provider = builder.Configuration.GetValue<string>("DatabaseProvider");
            if (provider == null)
            {
                return;
            }

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });
            builder.Services.AddHttpClient();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDistributedMemoryCache();
        }
    }
}