namespace ResumeManagement.Api.Entities
{
   
    public class Applicant
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        // AWS S3 object key, not the actual PDF file.
        public string? ResumeKey { get; set; }

        public bool IsDeleted { get; set; } 

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
