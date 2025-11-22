using System.Collections.ObjectModel;
using System.Windows.Input;
using Smart_Resume_Job_Search_App.Data;
using Smart_Resume_Job_Search_App.AppServices;

namespace Smart_Resume_Job_Search_App.Presentation
{
    public class JobResultsViewModel
    {
        public ObservableCollection<JobPosting> Jobs { get; } = new();
        public ICommand OpenJobCommand { get; }

        private readonly ApplicationFacade facade;

        public JobResultsViewModel(ApplicationFacade facade)
        {
            this.facade = facade;

            OpenJobCommand = new Command<JobPosting>(async job =>
            {
                if (job == null || string.IsNullOrWhiteSpace(job.Url)) return;

                await Launcher.OpenAsync(job.Url);
            });
        }

        public async Task InitializeAsync()
        {
            var jobs = await facade.GetJobPostings();
            Load(jobs);
        }

        public void Load(List<JobPosting> jobs)
        {
            Jobs.Clear();
            if (jobs == null) return;

            foreach (var job in jobs)
            {
                Jobs.Add(job);
            }
        }
    }
}
