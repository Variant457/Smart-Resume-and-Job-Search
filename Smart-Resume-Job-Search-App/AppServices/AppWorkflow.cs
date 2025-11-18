using Smart_Resume_Job_Search_App.Domain;
using Smart_Resume_Job_Search_App.States;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class EntityDto
    {
        public string text { get; set; }
        public string label { get; set; }
        public int start { get; set; }
    }

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

            var baseDir = AppContext.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.."));
            var scriptPath = Path.Combine(projectRoot, "AI", "ParsingModel.py");
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            using (var sw = process.StandardInput)
            {
                sw.WriteLine(text);
            }

            string json = process.StandardOutput.ReadToEnd();
            string err = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Debug.WriteLine($"[AppWorkflow] Raw Parsing Output:\n{json}");

            if (process.ExitCode != 0)
            {
                Debug.WriteLine($"[AppWorkflow] Parsing process exited with code {process.ExitCode}. Error: {err}");
                throw new Exception("Parsing process failed.");
            }

            json = json.Trim();
            var entities = JsonSerializer.Deserialize<List<EntityDto>>(json);
            Debug.WriteLine($"[AppWorkflow] Parsing completed successfully. Output: \n{json}");
        }
    }
}
