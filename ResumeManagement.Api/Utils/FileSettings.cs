namespace ResumeManagement.Api.Utils
{
    public class FileSettings
    {
        public string BasePath { get; set; } = "Uploads/Resumes";

        public int MaxFileSizeInMB { get; set; } = 5;

        public List<string> AllowedExtensions { get; set; } =
            new() { ".pdf" };
    }
}
