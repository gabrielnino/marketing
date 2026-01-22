namespace Services.Abstractions.Check
{
    public interface IDirectoryCheck
    {
        void EnsureDirectoryExists(string path);
    }
}
