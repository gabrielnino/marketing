namespace Configuration
{
    public class AppConfig
    {
        public Credential LinkedInCredentials { get; set; }
        public SearchConfiguration Search { get; set; }
        public Logging Logging { get; set; }
        public LlmProvider Llm { get; set; }
        public PathsConfig Paths { get; set; }
        public ExecutionOptions Options { get; set; }
    }
}