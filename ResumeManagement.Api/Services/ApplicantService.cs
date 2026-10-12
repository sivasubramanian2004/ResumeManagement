using Microsoft.EntityFrameworkCore;
using ResumeManagement.Api.DTOs;
using ResumeManagement.Api.Entities;
using ResumeManagement.Api.Interfaces;
using ResumeManagement.Api.Repositories;
namespace ResumeManagement.Api.Services
{
    public class ApplicantService:IApplicantService
    {
        private readonly IRepository<Applicant> _applicant;

        private readonly ILogger<ApplicantService> _logger;

        public ApplicantService(IRepository<Applicant> applicant, ILogger<ApplicantService> logger)
        {
            _applicant = applicant;
            _logger = logger;
        }

        public async Task CreateApplicantAsync(CreateApplicantDto dto)
        {
            var existEmail=await _applicant.TableNoTracking.AnyAsync(a => a.Email == dto.Email);
            if (existEmail)
            {
                throw new InvalidOperationException($"Applicant with this email {dto.Email} already exists.");
            }

            var applicant = new Applicant
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Mobile = dto.Mobile,
                Location = dto.Location
            };

            await _applicant.AddAsync(applicant);

            await _applicant.SaveAsync();

            _logger.LogInformation("Applicant created successfully with ID: {ApplicantId}", applicant.Id);
        }
    }
}
