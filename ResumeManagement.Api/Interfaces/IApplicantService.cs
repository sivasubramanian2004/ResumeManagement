using ResumeManagement.Api.DTOs;
namespace ResumeManagement.Api.Interfaces
{
    public interface IApplicantService
    {
        Task CreateApplicantAsync(CreateApplicantDto dto);
    }
}
