namespace Domain.OpenAI
{
    public class Prompt
    {
        public required string SystemContent { get; set; }
        public required string UserContent { get; set; }
    }
}
