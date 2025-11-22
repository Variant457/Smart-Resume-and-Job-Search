using Smart_Resume_Job_Search_App.Data;
using Smart_Resume_Job_Search_App.Presentation;
using System.Threading.Tasks;

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
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }

        public void Initialize(List<JobPosting> jobs)
        {
            _viewModel.Load(jobs);
        }
    }
}
