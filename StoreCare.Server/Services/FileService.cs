using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StoreCare.Server.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SaveProfileImageAsync(IFormFile file, string userId)
        {
            return await SaveImageAsync(file, userId, "ProfileImg");
        }

        public async Task<string> SaveStoreLogoAsync(IFormFile file, string storeId)
        {
            return await SaveImageAsync(file, storeId, "StoreLogos");
        }

        public async Task<string> SaveCategoryImageAsync(IFormFile file, string categoryId)
        {
            return await SaveImageAsync(file, categoryId, "CategoryImages");
        }

        public async Task<string> SaveProductImageAsync(IFormFile file, string productId)
        {
            return await SaveImageAsync(file, productId, "ProductImages");
        }

        private async Task<string> SaveImageAsync(IFormFile file, string id, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            // Validate file size (max 5MB for profile, 2MB for others)
            long maxSize = folderName == "ProfileImg" ? 5 * 1024 * 1024 : 2 * 1024 * 1024;
            if (file.Length > maxSize)
                throw new InvalidOperationException($"File size exceeds {(maxSize / (1024 * 1024))}MB limit");

            // Create directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath, folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Get file extension and validate
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid file format. Only images are allowed.");

            // Generate filename with ID
            var fileName = $"{id}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Delete existing file if it exists
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Save new file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{folderName}/{fileName}";
        }

        public void DeleteProfileImage(string? imagePath)
        {
            DeleteImage(imagePath, "ProfileImg");
        }

        public void DeleteStoreLogo(string? imagePath)
        {
            DeleteImage(imagePath, "StoreLogos");
        }

        public void DeleteCategoryImage(string? imagePath)
        {
            DeleteImage(imagePath, "CategoryImages");
        }

        public void DeleteProductImage(string? imagePath)
        {
            DeleteImage(imagePath, "ProductImages");
        }

        private void DeleteImage(string? imagePath, string folderName)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            try
            {
                var fileName = Path.GetFileName(imagePath);
                if (string.IsNullOrWhiteSpace(fileName))
                    return;

                var fullPath = Path.Combine(_environment.WebRootPath, folderName, fileName);

                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete image: {ex.Message}");
            }
        }

        public string? GetProfileImageUrl(string? imagePath)
        {
            return GetImageUrl(imagePath);
        }

        public string? GetStoreLogoUrl(string? imagePath)
        {
            return GetImageUrl(imagePath);
        }

        public string? GetCategoryImageUrl(string? imagePath)
        {
            return GetImageUrl(imagePath);
        }

        public string? GetProductImageUrl(string? imagePath)
        {
            return GetImageUrl(imagePath);
        }

        private string? GetImageUrl(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return imagePath;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}{imagePath}";
        }
    }
}