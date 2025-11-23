

using Smart_Resume_Job_Search_App.Domain;

namespace Smart_Resume_Job_Search_App.UINav
{
    public interface IAppFlow
    {
        Task Start();
        Task StartSearch(InputBlob blob);
        Task ShowResults();
        Task StartOptimizing();
        Task ShowOptimized();
        Task BackToHome();
        Task BackToResults();
    }
}
