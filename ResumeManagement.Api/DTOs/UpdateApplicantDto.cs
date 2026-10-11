using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ResumeManagement.Api.DTOs;

public class UpdateApplicantDto
{
    
    [StringLength(100, MinimumLength = 2)]
    public string? FullName { get; set; }

   
    [EmailAddress]
    [StringLength(254)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Mobile { get; set; } 

   
    [StringLength(100)]
    public string? Location { get; set; }

    // Optional: upload a new resume during update.
    public IFormFile? Resume { get; set; }
}