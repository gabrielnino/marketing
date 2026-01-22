namespace Services.Abstractions.Check
{ 
    public interface ICaptureSnapshot
    {
        Task<string> CaptureArtifactsAsync(string executionFolder, string stage);
    }
}
