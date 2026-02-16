using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace StoreCare.Server.Services
{
    public interface IFileService
    {
        Task<string> SaveProfileImageAsync(IFormFile file, string userId);
        Task<string> SaveStoreLogoAsync(IFormFile file, string storeId);
        Task<string> SaveCategoryImageAsync(IFormFile file, string categoryId);
        Task<string> SaveProductImageAsync(IFormFile file, string productId);
        void DeleteProfileImage(string? imagePath);
        void DeleteStoreLogo(string? imagePath);
        void DeleteCategoryImage(string? imagePath);
        void DeleteProductImage(string? imagePath);
        string? GetProfileImageUrl(string? imagePath);
        string? GetStoreLogoUrl(string? imagePath);
        string? GetCategoryImageUrl(string? imagePath);
        string? GetProductImageUrl(string? imagePath);
    }
}