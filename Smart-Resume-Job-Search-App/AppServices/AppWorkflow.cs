using Smart_Resume_Job_Search_App.States;
using Smart_Resume_Job_Search_App.AI;
using Smart_Resume_Job_Search_App.Domain;
using System.Diagnostics;

namespace Smart_Resume_Job_Search_App.AppServices
{
    class AppWorkflow
    {
        private IAppState? curState;
        private InputBlob? currentInput;

        public void SetState(InputBlob input)
        {
            currentInput = input;
            curState = new ParsingState(this);
            curState.EnterState();
        }
        
        public async Task ParseResumeAsync()
        {
            if (currentInput is null)
            {
                Debug.WriteLine("[AppWorkflow] ParseResumeAsync called with null input");
                return;
            }

            Debug.WriteLine($"[AppWorkflow] Parsing resume: {currentInput.FileName} ({currentInput.MimeType}), {currentInput.Content?.Length} bytes");

            var parser = new ResumeParser(new ParsingAdapter());
            // Pass the full InputBlob so we can extract text from PDFs
            var entities = await parser.ParseAsync(currentInput);

            Debug.WriteLine($"[AppWorkflow] Parsed {entities.Count} entities");
            // TODO: propagate results to next state / domain model as needed
        }
    }
}
