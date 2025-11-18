using Smart_Resume_Job_Search_App.Domain;
using Smart_Resume_Job_Search_App.States;

namespace Smart_Resume_Job_Search_App.AppServices
{
    class AppWorkflow
    {
        private readonly InputBlob inputBlob;
        private readonly TextExtractionService textExtractionService;
        IAppState curState;

        public AppWorkflow(InputBlob inputBlob, TextExtractionService textExtractionService)
        {
            this.inputBlob = inputBlob;
            this.textExtractionService = textExtractionService;
        }

        public void SetState()
        {
            curState = new ParsingState(this);
            curState.EnterState();
        }
        
        public void ParseResume()
        {
            var text = textExtractionService.Extract(inputBlob);
        }
    }
}
