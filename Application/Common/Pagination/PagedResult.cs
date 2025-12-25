namespace Application.Common.Pagination
{
    /// <summary>
    /// A page of items of type T plus the cursor to fetch the next page.
    /// </summary>
    public sealed record PagedResult<T>
    {
        /// <summary>
        /// The items in the current page.
        /// </summary>
        public IEnumerable<T> Items { get; init; } = [];

        /// <summary>
        /// The opaque cursor to send back in the next request, or null if there is no more data.
        /// </summary>
        public string? NextCursor { get; init; }

        /// <summary>
        /// Gets the total number of items across all pages.
        /// </summary>
        public int TotalCount { get; init; }
    }
}
