

using Smart_Resume_Job_Search_App.Data;
using Smart_Resume_Job_Search_App.Domain;

namespace Smart_Resume_Job_Search_App.UINav
{
    public interface IAppFlow
    {
        Task Start();
        Task StartSearch(InputBlob blob);
        Task ShowResults();
        Task<TailoredResume?> TailorResume(JobPosting job);
        Task StartSearchFromLatestResume();
        Task ShowTailoredResume(JobPosting job);
        Task BackToHome();
        Task BackToResults();
    }
}
