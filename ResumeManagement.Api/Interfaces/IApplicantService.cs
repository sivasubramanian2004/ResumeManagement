using ResumeManagement.Api.DTOs;
namespace ResumeManagement.Api.Interfaces
{
    public interface IApplicantService
    {
        Task CreateApplicantAsync(CreateApplicantDto dto);

        Task<List<ApplicantResponseDto>> GetAllApplicantsAsync();

        Task<(Stream FileStream, string ContentType, string FileName)> DownloadApplicantResumeAsync(int id);

    }
}
