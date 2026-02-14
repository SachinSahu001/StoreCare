using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StoreCare.Server.Services
{
    public interface IFileService
    {
        Task<string> SaveProfileImageAsync(IFormFile file, string userId);
        void DeleteProfileImage(string? imagePath);
        string? GetProfileImageUrl(string? imagePath);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _profileImagesFolder = "ProfileImg";

        public FileService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SaveProfileImageAsync(IFormFile file, string userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("File size exceeds 5MB limit");

            // Create directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath, _profileImagesFolder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Get file extension and validate
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid file format. Only images are allowed (jpg, jpeg, png, gif, bmp, webp)");

            // Generate filename with user ID
            var fileName = $"{userId}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save new file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{_profileImagesFolder}/{fileName}";
        }

        public void DeleteProfileImage(string? imagePath)
        {
            // Early return if no image path provided
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            try
            {
                var fileName = Path.GetFileName(imagePath);
                if (string.IsNullOrWhiteSpace(fileName))
                    return;

                var fullPath = Path.Combine(_environment.WebRootPath, _profileImagesFolder, fileName);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - we don't want to fail the request if delete fails
                Console.WriteLine($"Failed to delete profile image: {ex.Message}");
            }
        }

        public string? GetProfileImageUrl(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return imagePath;

            // Generate full URL
            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}{imagePath}";
        }
    }
}