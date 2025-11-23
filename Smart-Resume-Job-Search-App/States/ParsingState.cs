using Smart_Resume_Job_Search_App.AppServices;
using System.Diagnostics;

namespace Smart_Resume_Job_Search_App.States
{
    class ParsingState : IAppState
    {
        private readonly AppWorkflow appWorkflow;

        public ParsingState(AppWorkflow appWorkflow)
        {
            this.appWorkflow = appWorkflow;
            Debug.WriteLine("Parsing State initialized.");
        }

        public async Task EnterState()
        {
            Debug.WriteLine("Entering Parsing State: Parsing resumes and job descriptions.");

            await appWorkflow.ParseResume();
        }

        public void ExitState()
        {
            Debug.WriteLine("Exiting Parsing State: Completed parsing.");
        }
    }
}
