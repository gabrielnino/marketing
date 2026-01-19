using System.Text.RegularExpressions;
using Azure.Data.Tables;
using Application.TrackedLinks;

namespace Infrastructure.AzureTables;

public sealed partial class TrackedLink : ITrackedLink
{
    private const string TableName = "TrackedLinks";
    private const string PartitionKey = "urls";

    private static readonly Regex IdRegex = BuildIdRegex();

    private readonly TableClient _table;

    public TrackedLink(string serviceSasUrl)
    {
        if (string.IsNullOrWhiteSpace(serviceSasUrl))
            throw new InvalidOperationException("AzureTables:ServiceSasUrl is missing.");

        var serviceClient = new TableServiceClient(new Uri(serviceSasUrl));
        _table = serviceClient.GetTableClient(TableName);
    }

    public async Task UpsertAsync(string id, string targetUrl, CancellationToken ct = default)
    {
        ValidateId(id);

        if (string.IsNullOrWhiteSpace(targetUrl))
            throw new ArgumentException("targetUrl is required.", nameof(targetUrl));

        var entity = new TableEntity(PartitionKey, id)
        {
            ["TargetUrl"] = targetUrl.Trim(),
            ["CreatedUtc"] = DateTime.UtcNow,
            ["Source"] = "marketing"
        };

        await _table.UpsertEntityAsync(entity, TableUpdateMode.Merge, ct);
    }

    private static void ValidateId(string id)
    {
        if (!IdRegex.IsMatch(id))
            throw new ArgumentException(
                "Id must be exactly 15 alphanumeric characters (A–Z, a–z, 0–9).",
                nameof(id));
    }

    [GeneratedRegex(@"^[A-Za-z0-9]{15}$", RegexOptions.Compiled)]
    private static partial Regex BuildIdRegex();
}
