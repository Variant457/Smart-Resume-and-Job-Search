using DocumentFormat.OpenXml.Office.CustomUI;
using Smart_Resume_Job_Search_App.Domain;
using Smart_Resume_Job_Search_App.States;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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

            if (process.ExitCode != 0)
            {
                Debug.WriteLine($"[AppWorkflow] Parsing process exited with code {process.ExitCode}. Error: {err}");
                throw new Exception("Parsing process failed.");
            }

            json = json.Trim();
            Debug.WriteLine($"[AppWorkflow] Parsing completed successfully. Output: \n{json}");

            var parsedResume = new ParsedResume(inputBlob.Sha256, json);
        }
    }

    public class ParsedResumeDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("emails")]
        public List<string>? Emails { get; set; }

        [JsonPropertyName("phones")]
        public List<string>? Phones { get; set; }

        [JsonPropertyName("profiles")]
        public List<ProfileDto>? Profiles { get; set; }

        [JsonPropertyName("headline")]
        public string? Headline { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("skills")]
        public SkillsDto? Skills { get; set; }

        [JsonPropertyName("experience")]
        public List<ExperienceDto>? Experience { get; set; }

        [JsonPropertyName("education")]
        public List<EducationDto>? Education { get; set; }

        [JsonPropertyName("certifications")]
        public List<CertificationDto>? Certifications { get; set; }

        [JsonPropertyName("projects")]
        public List<ProjectDto>? Projects { get; set; }

        [JsonPropertyName("awards")]
        public List<AwardDto>? Awards { get; set; }

        [JsonPropertyName("publications")]
        public List<PublicationDto>? Publications { get; set; }

        [JsonPropertyName("languages")]
        public List<LanguageDto>? Languages { get; set; }

        [JsonPropertyName("interests")]
        public List<string>? Interests { get; set; }
    }

    public class ProfileDto
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }
    }

    public class SkillsDto
    {
        [JsonPropertyName("by_category")]
        public List<SkillCategoryDto>? ByCategory { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }
    }

    public class SkillCategoryDto
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("items")]
        public List<string>? Items { get; set; }
    }

    public class ExperienceDto
    {
        [JsonPropertyName("job_title")]
        public string? JobTitle { get; set; }

        [JsonPropertyName("organization")]
        public string? Organization { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("start_date")]
        public string? StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string? EndDate { get; set; }

        [JsonPropertyName("employment_type")]
        public string? EmploymentType { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("highlights")]
        public List<string>? Highlights { get; set; }

        [JsonPropertyName("skills")]
        public List<string>? Skills { get; set; }
    }

    public class EducationDto
    {
        [JsonPropertyName("degree")]
        public string? Degree { get; set; }

        [JsonPropertyName("field_of_study")]
        public string? FieldOfStudy { get; set; }

        [JsonPropertyName("institution")]
        public string? Institution { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("start_date")]
        public string? StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string? EndDate { get; set; }

        [JsonPropertyName("graduation_year")]
        public string? GraduationYear { get; set; }
    }

    public class CertificationDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }

        [JsonPropertyName("issue_date")]
        public string? IssueDate { get; set; }

        [JsonPropertyName("expiry_date")]
        public string? ExpiryDate { get; set; }

        [JsonPropertyName("credential_id")]
        public string? CredentialId { get; set; }
    }

    public class ProjectDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("highlights")]
        public List<string>? Highlights { get; set; }

        [JsonPropertyName("skills")]
        public List<string>? Skills { get; set; }
    }

    public class AwardDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }

        [JsonPropertyName("year")]
        public string? Year { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class PublicationDto
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("venue")]
        public string? Venue { get; set; }

        [JsonPropertyName("year")]
        public string? Year { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public class LanguageDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("proficiency")]
        public string? Proficiency { get; set; }
    }
}
