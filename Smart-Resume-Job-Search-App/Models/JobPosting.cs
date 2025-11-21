namespace Smart_Resume_Job_Search_App.Models
{
    public class JobPosting
    {
        public string Title { get; set; } = "";
        public string Company { get; set; } = "";
        public string Location { get; set; } = "";
        public string Url { get; set; } = "";
        public double? MinSalary { get; set; }
        public double? MaxSalary { get; set; }
        public string Description { get; set; } = "";
    }
}
