using System.Text.Json;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Smart_Resume_Job_Search_App.AppServices;
using Smart_Resume_Job_Search_App.Data;
using Smart_Resume_Job_Search_App.UINav;

namespace Smart_Resume_Job_Search_App.Presentation
{
    public class TailoredResumeViewModel : BindableObject
    {
        private readonly ApplicationFacade facade;
        private readonly IAppFlow appFlow;
        private TailoredResume? tailored;

        public string JobTitle { get; private set; } = "";
        public string Company { get; private set; } = "";
        public string ResumeText { get; private set; } = "";

        public ICommand BackCommand { get; }
        public ICommand DownloadCommand { get; }

        public TailoredResumeViewModel(ApplicationFacade facade, IAppFlow appFlow)
        {
            this.facade = facade;
            this.appFlow = appFlow;

            DownloadCommand = new Command(async () => await DownloadAsync());
            BackCommand = new Command(async () => await appFlow.BackToResults());
        }

        public void Initialize(TailoredResume tailoredResume)
        {
            tailored = tailoredResume;
            JobTitle = tailoredResume.JobPosting?.Title ?? "Job";
            Company = tailoredResume.JobPosting?.Company ?? "";
            ResumeText = tailoredResume.JsonResult ?? string.Empty;

            OnPropertyChanged(nameof(JobTitle));
            OnPropertyChanged(nameof(Company));
            OnPropertyChanged(nameof(ResumeText));
        }

        private async Task DownloadAsync()
        {
            if(tailored == null || string.IsNullOrWhiteSpace(tailored.FileName))
            {
                return; 
            }

            if(!File.Exists(tailored.FileName))
            {
                return; 
            }

            var downloadRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            Directory.CreateDirectory(downloadRoot);
            var fileName = Path.GetFileName(tailored.FileName);
            var destPath = Path.Combine(downloadRoot, fileName);

            File.Copy(tailored.FileName, destPath, overwrite: true);

            await Application.Current.MainPage.DisplayAlert(
                "Tailored Resume Saved",
                $"Saved to:\n{destPath}",
                "OK");
        }
    }
}
