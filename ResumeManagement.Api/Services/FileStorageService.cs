
 
   using Microsoft.Extensions.Options;
   using ResumeManagement.Api.Interfaces;
   using ResumeManagement.Api.Utils;

namespace ResumeManagement.Api.Services;

public class FileStorageService : IFileStorageService
    {
        private readonly string _contentRootPath;
        private readonly FileSettings _fileSettings;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(
            IWebHostEnvironment env,
            IOptions<FileSettings> fileSettings,
            ILogger<FileStorageService> logger)
        {
            _contentRootPath = env.ContentRootPath;
            _fileSettings = fileSettings.Value;
            _logger = logger;
        }

        public async Task<FileUploadResult> UploadAsync(
            IFormFile file,
            string folder)
        {
            // 1. Validate file

            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file was uploaded.");
            }

            if (_fileSettings.MaxFileSizeInMB <= 0)
            {
                throw new InvalidOperationException(
                    "Maximum file size configuration is invalid.");
            }

            var maxBytes = _fileSettings.MaxFileSizeInMB * 1024L * 1024L;

            if (file.Length > maxBytes)
            {
                throw new ArgumentException(
                    $"File size cannot exceed {_fileSettings.MaxFileSizeInMB} MB.");
            }

            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !_fileSettings.AllowedExtensions.Contains(
                    extension,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"File type is not allowed. Allowed types: " +
                    $"{string.Join(", ", _fileSettings.AllowedExtensions)}");
            }

            // 2. Resolve private storage directory

            var baseDirectory = Path.GetFullPath(
                Path.Combine(_contentRootPath, _fileSettings.BasePath));

            // Restrict folder to a single safe folder name
            if (string.IsNullOrWhiteSpace(folder) ||
                folder != Path.GetFileName(folder) ||
                folder.Contains(".."))
            {
                throw new ArgumentException("Invalid storage folder.");
            }

            var uploadFolder = Path.GetFullPath(
                Path.Combine(baseDirectory, folder));

            if (!uploadFolder.StartsWith(
                    baseDirectory + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid storage path.");
            }

            Directory.CreateDirectory(uploadFolder);

            // 3. Generate a unique filename

            var uniqueFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

            var fullPath = Path.Combine(uploadFolder, uniqueFileName);

            try
            {
                // 4. Save file outside wwwroot

                await using var stream = new FileStream(
                    fullPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None);

                await file.CopyToAsync(stream);

                // 5. Return a private storage key

                var relativePath = Path.Combine(
                    _fileSettings.BasePath,
                    folder,
                    uniqueFileName).Replace("\\", "/");

                return new FileUploadResult
                {
                    FileName = uniqueFileName,
                    OriginalFileName = Path.GetFileName(file.FileName),
                    RelativePath = relativePath,
                    FullPath = fullPath,
                    FileSize = file.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to save private file. Folder: {Folder}",
                    folder);

                // Remove partially written file if one exists
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                throw new InvalidOperationException(
                    "Failed to save the uploaded file.",
                    ex);
            }
        }

        public Task DeleteAsync(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return Task.CompletedTask;
            }

            var normalizedPath = Path.GetFullPath(fullPath);

            var baseDirectory = Path.GetFullPath(
                Path.Combine(_contentRootPath, _fileSettings.BasePath));

            if (!normalizedPath.StartsWith(
                    baseDirectory + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "The file path is outside the private storage directory.");
            }

            if (File.Exists(normalizedPath))
            {
                File.Delete(normalizedPath);
            }

            return Task.CompletedTask;
        }

        public Task<string> GetFullPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Task.FromResult(string.Empty);
            }

            var baseDirectory = Path.GetFullPath(
                Path.Combine(_contentRootPath, _fileSettings.BasePath));

            var normalizedPath = relativePath.Replace(
                '/',
                Path.DirectorySeparatorChar);

            var fullPath = Path.GetFullPath(
                Path.Combine(_contentRootPath, normalizedPath));

            if (!fullPath.StartsWith(
                    baseDirectory + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Invalid private file path.");
            }

            return Task.FromResult(fullPath);
        }
    }
