using Smart_Resume_Job_Search_App.Models;
using Smart_Resume_Job_Search_App.Presentation;

namespace Smart_Resume_Job_Search_App.UI
{
    public partial class JobResultsView : ContentPage
    {
        private readonly JobResultsViewModel _viewModel;

        public JobResultsView(JobResultsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void Initialize(IEnumerable<JobPosting> jobs)
        {
            _viewModel.Load(jobs);
        }
    }
}
