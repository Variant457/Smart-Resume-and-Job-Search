using Smart_Resume_Job_Search_App.States;

namespace Smart_Resume_Job_Search_App.AppServices
{
    class AppWorkflow
    {
        IAppState curState;
        public void SetState()
        {
            curState = new ParsingState(this);
            curState.EnterState();
        }
        
        public void ParseResume()
        {
            ;
        }
    }
}
