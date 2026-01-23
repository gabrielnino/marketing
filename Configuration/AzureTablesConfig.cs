namespace Configuration
{
    public sealed class AzureTablesConfig
    {
        public const string SectionName = "AzureTables";

        public string ServiceSasUrl { get; init; } = default!;
    }
}
