using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ResumeManagement.Api.DTOs;

public class CreateApplicantDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string Mobile { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public IFormFile Resume { get; set; } = default!;
}