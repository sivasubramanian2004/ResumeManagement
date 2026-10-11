namespace ResumeManagement.Api.DTOs;

public class ApplicantResponseDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public bool HasResume { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}