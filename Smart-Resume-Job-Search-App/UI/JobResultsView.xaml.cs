using Smart_Resume_Job_Search_App.Data;
using Smart_Resume_Job_Search_App.Presentation;

namespace Smart_Resume_Job_Search_App.UI
{
    public partial class JobResultsView : ContentPage
    {
        private bool isInitialized;

        public JobResultsView(JobResultsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!isInitialized && BindingContext is JobResultsViewModel vm)
            {
                isInitialized = true;
                await vm.InitializeAsync();
            }
        }

        public void Initialize(List<JobPosting> jobs)
        {
            if (BindingContext is JobResultsViewModel vm)
            {
                vm.Load(jobs);
            }
        }
    }
}
