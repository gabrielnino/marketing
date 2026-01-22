namespace Common.StringExtensions
{
    public static class JsonExtractionExtensions
    {
        /// <summary>
        /// Extracts JSON content from an LLM response.
        /// Supports:
        ///  - Plain JSON
        ///  - ```json fenced blocks
        ///  - ``` generic fenced blocks
        /// If no fences are found, returns the trimmed input.
        /// Never throws.
        /// </summary>
        public static string ExtractJsonContent(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            const string JsonFence = "```json";
            const string Fence = "```";

            // Fast path: no fences at all
            if (!input.Contains(Fence, StringComparison.Ordinal))
                return input.Trim();

            // Case 1: ```json fence (case-insensitive)
            var startIndex = input.IndexOf(JsonFence, StringComparison.OrdinalIgnoreCase);
            if (startIndex >= 0)
            {
                startIndex += JsonFence.Length;

                var endIndex = input.IndexOf(Fence, startIndex, StringComparison.Ordinal);
                if (endIndex < 0)
                    endIndex = input.Length;

                return input[startIndex..endIndex].Trim();
            }

            // Case 2: generic ``` fence
            startIndex = input.IndexOf(Fence, StringComparison.Ordinal);
            if (startIndex < 0)
                return input.Trim();

            startIndex += Fence.Length;

            var genericEndIndex = input.IndexOf(Fence, startIndex, StringComparison.Ordinal);
            if (genericEndIndex < 0)
                genericEndIndex = input.Length;

            return input[startIndex..genericEndIndex].Trim();
        }
    }
}
