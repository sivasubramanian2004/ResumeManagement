using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeManagement.Api.DTOs;
using ResumeManagement.Api.Entities;
using ResumeManagement.Api.Helpers;
using ResumeManagement.Api.Interfaces;
using ResumeManagement.Api.Repositories;
namespace ResumeManagement.Api.Services
{
    public class ApplicantService:IApplicantService
    {
        private readonly IRepository<Applicant> _applicant;

        private readonly IFileStorageService _fileStorageService;

        private readonly UrlService _urlService;

        private readonly ILogger<ApplicantService> _logger;

        public ApplicantService(IRepository<Applicant> applicant, IFileStorageService fileStorageService, UrlService urlService, ILogger<ApplicantService> logger)
        {
            _applicant = applicant;
            _fileStorageService = fileStorageService;
            _urlService = urlService;
            _logger = logger;
        }

        public async Task CreateApplicantAsync(CreateApplicantDto dto)
        {
            var existEmail=await _applicant.TableNoTracking.AnyAsync(a => a.Email == dto.Email);

            if (existEmail)
            {
                throw new InvalidOperationException($"Applicant with this email {dto.Email} already exists.");
            }

            var uploadResult = await _fileStorageService.UploadAsync(dto.Resume, "Applicants");
            try
            {
                var applicant = new Applicant
                {
                FullName = dto.FullName,
                Email = dto.Email,
                Mobile = dto.Mobile,
                Location = dto.Location,
                ResumeKey = uploadResult.RelativePath
                };
           
                await _applicant.AddAsync(applicant);

                await _applicant.SaveAsync();

                _logger.LogInformation("Applicant created successfully with ID: {ApplicantId}", applicant.Id);
            }
            catch (Exception ex)
            {
                await _fileStorageService.DeleteAsync(uploadResult.FullPath);

                _logger.LogError(ex, "Error occurred while creating applicant with email: {Email}", dto.Email);
                throw;
            
            }

        }

        public async Task<List<ApplicantResponseDto>> GetAllApplicantsAsync()
        {
            var baseUrl = _urlService.GetBaseUrl();

            return await _applicant.TableNoTracking
                .Select(a => new ApplicantResponseDto
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Email = a.Email,
                    Mobile = a.Mobile,
                    Location = a.Location,

                    ResumeUrl = a.ResumeKey != null
                        ? $"{baseUrl}/api/Applicant/{a.Id}/resume"
                        : null
                })
                .OrderBy(a => a.Id)
                .ToListAsync();
        }

        public async Task<(Stream FileStream, string ContentType, string FileName)>
     DownloadApplicantResumeAsync(int id)
        {
            var applicant = await _applicant.TableNoTracking
                .FirstOrDefaultAsync(a => a.Id == id);

            if (applicant == null)
            {
                throw new KeyNotFoundException(
                    $"Applicant with ID {id} not found.");
            }

            if (string.IsNullOrWhiteSpace(applicant.ResumeKey))
            {
                throw new FileNotFoundException(
                    $"Resume not found for applicant ID {id}.");
            }

            var filePath = await _fileStorageService
                .GetFullPath(applicant.ResumeKey);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"Resume file not found for applicant ID {id}.");
            }

            var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            return (
                stream,
                "application/pdf",
                Path.GetFileName(filePath)
            );
        }
    }
}
