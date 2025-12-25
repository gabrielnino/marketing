namespace Services.Interfaces
{
    public interface IUtil
    {
        Task<bool> WaitForPageLoadAsync(int timeoutInSeconds = 30);
        void ScrollMove();
        void ScrollToTop();
        void ScrollToExperienceSection();
        Task<bool> NavigateToNextPageAsync();
    }
}
