using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.PixVerse
{
    public sealed class PixVerseBalance
    {
        /// <summary>
        /// Available balance units/credits (as reported by PixVerse).
        /// </summary>
        public required decimal Available { get; init; }

        /// <summary>
        /// Optional: currency or unit label if PixVerse provides it (e.g., "USD", "credits").
        /// </summary>
        public string? Unit { get; init; }

        /// <summary>
        /// Optional: server-side timestamp or retrieval time if you want to attach it.
        /// </summary>
        public DateTimeOffset? RetrievedAtUtc { get; init; } = DateTimeOffset.UtcNow;

        public bool HasAtLeast(decimal minimum) => Available >= minimum;
    }
}
