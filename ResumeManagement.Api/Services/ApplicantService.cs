using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeManagement.Api.DTOs;
using ResumeManagement.Api.Entities;
using ResumeManagement.Api.Helpers;
using ResumeManagement.Api.Interfaces;
using ResumeManagement.Api.Repositories;
using ClosedXML.Excel;

using System.Net.WebSockets;
namespace ResumeManagement.Api.Services
{
    public class ApplicantService : IApplicantService
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
            var existEmail = await _applicant.TableNoTracking.AnyAsync(a => a.Email == dto.Email && a.IsDeleted != true);

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
                _logger.LogError(
                    ex, "Error occurred while creating applicant with email: {Email}", dto.Email);

                // 4. Clean up uploaded file if database operation fails
                try
                {
                    await _fileStorageService.DeleteAsync(uploadResult.FullPath);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Failed to clean up uploaded resume for applicant email: {Email}", dto.Email);

                }

                // 5. Preserve the original exception
                throw;
            }

        }

        public async Task<List<ApplicantResponseDto>> GetAllApplicantsAsync()
        {
            var baseUrl = _urlService.GetBaseUrl();

            var application = await _applicant.TableNoTracking
                .Where(a => a.IsDeleted != true)
                .Select(a => new ApplicantResponseDto
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Email = a.Email,
                    Mobile = a.Mobile,
                    Location = a.Location,
                    ResumeUrl = a.ResumeKey != null
                        ? $"{baseUrl}/api/Applicant/{a.Id}/DownLoadresume" : null

                })
                .OrderBy(a => a.Id)
                .ToListAsync();

            if (application.Count() == 0)
                throw new InvalidOperationException("No applicants found.");

            return application;
        }

        public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadApplicantResumeAsync(int id)
        {
            var applicant = await _applicant.TableNoTracking
                .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted != true);

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

            return (stream, "application/pdf", Path.GetFileName(filePath)

            );
        }


        public async Task DeleteApplicantAsync(int id)
        {
            var applicant = await _applicant.Table
                           .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted != true);

            if (applicant == null)
            {
                throw new KeyNotFoundException($"Applicant with ID {id} not found.");

            }

            var oldResumeKey = applicant.ResumeKey;
            var deletedDate = DateTime.UtcNow;


            // 1. Soft delete applicant
            applicant.IsDeleted = true;
            applicant.DeletedAt = deletedDate;

            // 2. Save database changes
            await _applicant.SaveAsync();


            try
            {

                if (!string.IsNullOrWhiteSpace(oldResumeKey))
                {
                    var resumeFullPath = await _fileStorageService.GetFullPath(oldResumeKey);
                    await _fileStorageService.DeleteAsync(resumeFullPath);
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Applicant {ApplicantId} was soft-deleted, but resume cleanup failed.", id);

                throw new InvalidOperationException(
                    $"Applicant {id} was soft-deleted, but resume file cleanup failed.", ex);
            }

            _logger.LogInformation("Applicant with ID {ApplicantId} marked as deleted at {DeletedAt}.", id, deletedDate);
        }

        public async Task UpdateApplicantAsync(int id, CreateApplicantDto dto)
        {

            var applicant = await _applicant.Table
                           .FirstOrDefaultAsync(a => a.Id == id);

            if (applicant == null)
            {
                throw new KeyNotFoundException($"Applicant with ID {id} not found.");
            }

            var existEmail = await _applicant.TableNoTracking
                            .AnyAsync(a => a.Email == dto.Email && a.Id != id);
            if (existEmail)
            {
                throw new InvalidOperationException($"Another applicant with this email {dto.Email} already exists.");
            }

            var oldResumeKey = applicant.ResumeKey;
            string? newResumeKey = null;
            try
            {
                var uploadResult = await _fileStorageService.UploadAsync(dto.Resume, "Applicants");
                newResumeKey = uploadResult.RelativePath;
                applicant.FullName = dto.FullName;
                applicant.Email = dto.Email;
                applicant.Mobile = dto.Mobile;
                applicant.Location = dto.Location;
                applicant.UpdatedAt = DateTime.UtcNow;
                applicant.ResumeKey = newResumeKey;
                await _applicant.SaveAsync();
            }
            catch (Exception ex)
            {

                _logger.LogError(
                          ex, "Error occurred while updating applicant with ID: {ApplicantId}", id);

                // 4. If database update fails, clean up the newly uploaded file

                if (!string.IsNullOrWhiteSpace(newResumeKey))
                {
                    try
                    {
                        var newResumeFullPath = await _fileStorageService.GetFullPath(newResumeKey);
                        await _fileStorageService.DeleteAsync(newResumeFullPath);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(
                            cleanupEx,
                            "Failed to clean up the new resume for applicant ID: {ApplicantId}",
                            id);
                    }
                }

                throw;

            }
            // 5. Delete the old resume only after database save succeeds
            if (!string.IsNullOrWhiteSpace(oldResumeKey))
            {
                try
                {
                    var oldResumeFullPath = await _fileStorageService.GetFullPath(oldResumeKey);

                    await _fileStorageService.DeleteAsync(oldResumeFullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Applicant {ApplicantId} was updated, but the old resume could not be deleted.",
                        id);
                }
            }

            _logger.LogInformation("Applicant with ID {ApplicantId} updated successfully.", id);

        }

        public async Task<byte[]> ExportApplicantsToExcelAsync()
        {


            var applicants = await _applicant.TableNoTracking
                            .Where(a => !a.IsDeleted)
                            .Select(a => new ApplicantResponseDto
                            {
                                Id = a.Id,
                                FullName = a.FullName,
                                Email = a.Email,
                                Mobile = a.Mobile,
                                Location = a.Location,
                                CreatedAt = a.CreatedAt,

                            })
                            .ToListAsync();

            if (applicants.Count() == 0)
                throw new InvalidOperationException("No applicants found to export.");

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Applications");

            // Header
            worksheet.Cell(1, 1).Value = "ApplicationID";
            worksheet.Cell(1, 2).Value = "First Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Mobile";
            worksheet.Cell(1, 5).Value = "Location";
            worksheet.Cell(1, 6).Value = "Created At";

            for (int i = 0; i < applicants.Count; i++)
            {
                var row = i + 2;
                var applicant = applicants[i];

                worksheet.Cell(row, 1).Value = applicant.Id;
                worksheet.Cell(row, 2).Value = applicant.FullName;
                worksheet.Cell(row, 3).Value = applicant.Email;
                worksheet.Cell(row, 4).Value = applicant.Mobile;
                worksheet.Cell(row, 5).Value = applicant.Location;
                worksheet.Cell(row, 6).Value = applicant.CreatedAt;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();

        }
    }
}
