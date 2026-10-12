using ResumeManagement.Api.Utils;
namespace ResumeManagement.Api.Interfaces
{
    public interface IFileStorageService
    {
        Task<FileUploadResult> UploadAsync(
            IFormFile file,
            string folder);

        Task DeleteAsync(string fullPath);

        Task<string> GetFullPath(string relativePath);
    }
}
