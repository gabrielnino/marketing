using System.Text.Json.Serialization;

namespace Application.PixVerse.response
{
    public sealed class Balance
    {
        [JsonPropertyName("account_id")]
        public long AccountId { get; init; }

        [JsonPropertyName("credit_monthly")]
        public int CreditMonthly { get; init; }

        [JsonPropertyName("credit_package")]
        public int CreditPackage { get; init; }

        // Optional convenience property
        [JsonIgnore]
        public int TotalCredits => CreditMonthly + CreditPackage;

        [JsonIgnore]
        public DateTimeOffset RetrievedAtUtc { get; init; } = DateTimeOffset.UtcNow;

        public bool HasAtLeast(int minimum) => TotalCredits >= minimum;
    }
}
