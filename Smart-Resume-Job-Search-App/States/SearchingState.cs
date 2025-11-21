using Smart_Resume_Job_Search_App.AppServices;
using System.Diagnostics;

namespace Smart_Resume_Job_Search_App.States
{
    class SearchingState : IAppState
    {
        private readonly AppWorkflow appWorkflow;
        public SearchingState(AppWorkflow appWorkflow)
        {
            this.appWorkflow = appWorkflow;
            Debug.WriteLine("Searching State initialized.");
        }

        public async void EnterState()
        {
            Debug.WriteLine("Entering Searching State: Searching for job matches.");
            await appWorkflow.SearchJobs();
        }

        public void ExitState()
        {
            Debug.WriteLine("Exiting Searching State: Completed job search.");
        }
    }
}
