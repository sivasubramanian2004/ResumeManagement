namespace ResumeManagement.Api.Utils
{
    
    public class FileUploadResult
    {
        public string FileName { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        // Store this relative key in the database
        public string RelativePath { get; set; } = string.Empty;

        // Internal server path; do not return this to the client
        public string FullPath { get; set; } = string.Empty;

        public long FileSize { get; set; }
    }
}
