namespace Application.PixVerse.Response
{
    public enum JobState
    {
        Unknown = 0,
        Queued = 1,
        Processing = 2,
        Succeeded = 3,
        Failed = 4,
        Cancelled = 5,
        Pending
    }
}
