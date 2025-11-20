using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Smart_Resume_Job_Search_App.Models;

namespace Smart_Resume_Job_Search_App.Presentation
{
    public class JobResultsViewModel
    {
        public ObservableCollection<JobPosting> Jobs { get; } = new();

        public ICommand OpenJobCommand { get; }

        public JobResultsViewModel()
        {
            OpenJobCommand = new Command<JobPosting>(async job =>
            {
                if (job == null) return;
                if (string.IsNullOrWhiteSpace(job.Url)) return;

                await Launcher.OpenAsync(job.Url);
            });
        }

        public void Load(IEnumerable<JobPosting> jobs)
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
