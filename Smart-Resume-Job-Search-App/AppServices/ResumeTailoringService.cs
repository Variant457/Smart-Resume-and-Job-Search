using Microsoft.EntityFrameworkCore;
using Smart_Resume_Job_Search_App.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class ResumeTailoringService
    {
        private readonly AppDbContext db;

        public ResumeTailoringService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<TailoredResume> TailorResume(JobPosting job, ParsedResumeRecord baseResume)
        {
            var inputPayload = new
            {
                resume = baseResume.RawJson,
                job = job.Description
            };
            var inputJson = JsonSerializer.Serialize(inputPayload);

            var (outputText, err) = await RunPythonScript(inputJson);
            if (string.IsNullOrWhiteSpace(outputText))
            {
                throw new Exception($"[ResumeTailoringService] Tailoring Failed. Error: {err}");
            }

            var dir = GetTailoredResumeDirectory();
            var safeJobId = string.Join("_", job.JobId.Split(Path.GetInvalidFileNameChars()));
            var fileNameOnly = $"tailored_resume_{safeJobId}.txt";
            var fullPath = Path.Combine(dir, fileNameOnly);
            await File.WriteAllTextAsync(fullPath, outputText, Encoding.UTF8);

            var tailoredResume = new TailoredResume
            {
                ParsedResumeInfoId = baseResume.Id,
                JobPostingId = job.Id,
                JsonResult = outputText,
                CreatedAt = DateTime.UtcNow,
                FileName = fullPath
            };

            db.TailoredResumes.Add(tailoredResume);
            await db.SaveChangesAsync();
            return tailoredResume;
        }

        public async Task<string> GetTailoredResume(int jobPostingId)
        {
            var record = await db.TailoredResumes
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(t => t.JobPostingId == jobPostingId);

            return record.JsonResult;
        }

        private async Task<(string output, string error)> RunPythonScript(string inputJson)
        {
            return await Task.Run(() =>
            {
                var baseDir = AppContext.BaseDirectory;
                var projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.."));
                var scriptPath = Path.Combine(projectRoot, "AI", "tailor_resume.py");

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
                    sw.WriteLine(inputJson);
                }

                string output = process.StandardOutput.ReadToEnd();
                string err = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return (output, err);
            });
        }

        private static string GetExtensionForMime(string mime)
        {
            return mime switch
            {
                "application/pdf" => ".pdf",
                "application/msword" => ".doc",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                "text/plain" => ".txt",
                _ => ".txt"
            };
        }

        private static string GetTailoredResumeDirectory()
        {
            var baseDir = AppContext.BaseDirectory;
            var dir = Path.Combine(baseDir, "TailoredResumes");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
