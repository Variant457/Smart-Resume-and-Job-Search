using Smart_Resume_Job_Search_App.Data;
using Smart_Resume_Job_Search_App.Presentation;

namespace Smart_Resume_Job_Search_App.UI
{
    public partial class JobResultsView : ContentPage
    {
        public JobResultsView(JobResultsViewModel viewModel)
        {
            InitializeComponent();
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is JobResultsViewModel vm)
            {
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
