namespace Services.Abstractions.Check
{
    public interface ISecurityCheck
    {
        bool IsSecurityCheck();
        Task TryStartPuzzle();
        Task HandleSecurityPage();
        Task HandleUnexpectedPage();
    }
}
