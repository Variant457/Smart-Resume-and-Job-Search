using System.Collections.ObjectModel;
using System.Windows.Input;
using Smart_Resume_Job_Search_App.Data;
using Smart_Resume_Job_Search_App.AppServices;
using Smart_Resume_Job_Search_App.UINav;
using System.Threading.Tasks;

namespace Smart_Resume_Job_Search_App.Presentation
{
    public class JobResultsViewModel
    {
        public ObservableCollection<JobItemViewModel> Jobs { get; } = new();
        public ICommand OpenJobCommand { get; }
        public ICommand BackCommand { get; }

        private readonly ApplicationFacade facade;
        private readonly IAppFlow appFlow;

        public JobResultsViewModel(ApplicationFacade facade, IAppFlow appFlow)
        {
            this.facade = facade;
            this.appFlow = appFlow;

            OpenJobCommand = new Command<JobPosting>(async job =>
            {
                if (job == null || string.IsNullOrWhiteSpace(job.Url)) return;

                await Launcher.OpenAsync(job.Url);
            });

            BackCommand = new HomeViewModel.DelegateCommand(
                async _ => await appFlow.BackToHome()
            );
        }

        public async Task InitializeAsync()
        {
            var jobPostings = await facade.GetJobPostings();
            var latestResume = await facade.GetLatestParsedResume();

            Jobs.Clear();
            foreach (var job in jobPostings)
            {
                var tailored = await facade.GetLatestTailoredResume(job.Id);
                Jobs.Add(new JobItemViewModel(job, facade, tailored, latestResume));
            }
        }

        public async Task Load(List<JobPosting> jobs)
        {
            Jobs.Clear();
            if (jobs == null) return;

            var latestResume = await facade.GetLatestParsedResume();
            foreach (var job in jobs)
            {
                var tailored = await facade.GetLatestTailoredResume(job.Id);
                Jobs.Add(new JobItemViewModel(job, facade, tailored, latestResume));
            }
        }
    }

    public class JobItemViewModel : BindableObject
    {
        private readonly ApplicationFacade facade;
        public JobPosting Job { get; }

        public Color StatusColor { get; private set; }
        public bool ShowTailorButton { get; private set; }
        public bool ShowViewButton { get; private set; }
        public bool IsBusy { get; private set; }

        public ICommand OpenJobCommand { get; }
        public ICommand TailorCommand { get; }
        public ICommand ViewTailoredCommand { get; }

        public JobItemViewModel(JobPosting job, ApplicationFacade facade, TailoredResume? existingTailored, ParsedResumeRecord latestParsed)
        {
            Job = job;
            this.facade = facade;

            OpenJobCommand = new Command(async () =>
            {
                if (!string.IsNullOrWhiteSpace(Job.Url))
                {
                    await Launcher.OpenAsync(Job.Url);
                }
            });

            TailorCommand = new Command(async () => await TailorAsync(), () => !IsBusy);

            ViewTailoredCommand = new Command(async () => await ViewAsync());
            DetermineStatus(existingTailored, latestParsed);
        }

        private void DetermineStatus(TailoredResume? tailored, ParsedResumeRecord latestParsed)
        {
            if (tailored == null)
            {
                StatusColor = Colors.Red;
                ShowTailorButton = true;
                ShowViewButton = false;
            }
            else if (tailored.CreatedAt < latestParsed.ParsedAt)
            {
                StatusColor = Colors.Orange;
                ShowTailorButton = true;
                ShowViewButton = true;
            }
            else
            {
                StatusColor = Colors.Green;
                ShowTailorButton = false;
                ShowViewButton = true;
            }

            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(ShowTailorButton));
            OnPropertyChanged(nameof(ShowViewButton));
        }

        private async Task TailorAsync()
        {
            IsBusy = true;
            OnPropertyChanged(nameof(IsBusy));
            ((Command)TailorCommand).ChangeCanExecute();

            try
            {
                await facade.TailorResume(Job);

                var latestParsed = await facade.GetLatestParsedResume();
                var tailored = await facade.GetLatestTailoredResume(Job.Id);
                DetermineStatus(tailored, latestParsed);
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(IsBusy));
                ((Command)TailorCommand).ChangeCanExecute();
            }
        }

        private async Task ViewAsync()
        {
            var tailored = await facade.GetLatestTailoredResume(Job.Id);
            if (tailored != null)
            {
                await Application.Current.MainPage.DisplayAlert("Tailored Resume",
                    tailored.JsonResult.Substring(0, Math.Min(500, tailored.JsonResult.Length)) + "...", "OK");
            }
        }
    }
}
