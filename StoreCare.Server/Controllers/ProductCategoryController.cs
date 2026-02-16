using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreCare.Server.Data;
using StoreCare.Server.Models;
using StoreCare.Server.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace StoreCare.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductCategoryController : ControllerBase
{
    private readonly StoreCareDbContext _context;
    private readonly ILogger<ProductCategoryController> _logger;
    private readonly IFileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductCategoryController(
        StoreCareDbContext context,
        ILogger<ProductCategoryController> logger,
        IFileService fileService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _fileService = fileService;
        _httpContextAccessor = httpContextAccessor;
    }

    // ===============================
    // DTOs
    // ===============================
    public class CategoryResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryDescription { get; set; }
        public string? CategoryImage { get; set; }
        public string? CategoryImageUrl { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsPopular { get; set; }
        public string? IconClass { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalProducts { get; set; }
        public List<CategoryProductDto> Products { get; set; } = new();
    }

    public class CategoryProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public bool IsFeatured { get; set; }
        public string? ProductImage { get; set; }
        public string? ProductImageUrl { get; set; }
    }

    public class CategoryCreateDto
    {
        [Required, MaxLength(200)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? CategoryDescription { get; set; }

        public int? DisplayOrder { get; set; }

        public bool? IsPopular { get; set; }

        [MaxLength(100)]
        public string? IconClass { get; set; }

        public IFormFile? CategoryImage { get; set; }
    }

    public class CategoryUpdateDto
    {
        [MaxLength(200)]
        public string? CategoryName { get; set; }

        [MaxLength(1000)]
        public string? CategoryDescription { get; set; }

        public int? DisplayOrder { get; set; }

        public bool? IsPopular { get; set; }

        [MaxLength(100)]
        public string? IconClass { get; set; }

        public IFormFile? CategoryImage { get; set; }

        public int? StatusId { get; set; }
    }

    public class CategoryStatusUpdateDto
    {
        [Required]
        public int StatusId { get; set; }
    }

    public class CategoryListResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryDescription { get; set; }
        public string? CategoryImage { get; set; }
        public string? CategoryImageUrl { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsPopular { get; set; }
        public string? IconClass { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
    }

    // ===============================
    // CREATE CATEGORY (SuperAdmin only) with Auto DisplayOrder
    // ===============================
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateCategory([FromForm] CategoryCreateDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("CreateCategory called by: {UserName}", currentUserName);

        try
        {
            // Validate unique category name
            if (await _context.ProductCategories.AnyAsync(c => c.CategoryName == dto.CategoryName.Trim()))
            {
                return BadRequest(new { message = "Category with this name already exists." });
            }

            // Get active status
            var activeStatus = await _context.MasterTables
                .FirstOrDefaultAsync(m => m.TableName == "Status" && m.TableValue == "Active");

            if (activeStatus == null)
                return StatusCode(500, new { message = "Active status not configured." });

            // Auto-generate DisplayOrder if not provided
            int displayOrder = dto.DisplayOrder ?? 0;
            if (displayOrder <= 0)
            {
                // Get max DisplayOrder from active categories and add 1
                var maxDisplayOrder = await _context.ProductCategories
                    .Where(c => c.Active == true)
                    .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;
                displayOrder = maxDisplayOrder + 1;
            }

            // Generate unique category code
            var categoryCode = "CAT-" + Guid.NewGuid().ToString()[..8].ToUpper();
            var categoryId = Guid.NewGuid().ToString();

            string? imagePath = null;
            if (dto.CategoryImage != null && dto.CategoryImage.Length > 0)
            {
                imagePath = await _fileService.SaveCategoryImageAsync(dto.CategoryImage, categoryId);
            }

            var category = new ProductCategory
            {
                Id = categoryId,
                CategoryCode = categoryCode,
                CategoryName = dto.CategoryName.Trim(),
                CategoryDescription = dto.CategoryDescription?.Trim(),
                CategoryImage = imagePath,
                DisplayOrder = displayOrder,
                IsPopular = dto.IsPopular ?? false,
                IconClass = dto.IconClass,
                StatusId = activeStatus.Id,
                CreatedBy = currentUserName,
                CreatedDate = GetIndianTime(),
                ModifiedBy = currentUserName,
                ModifiedDate = GetIndianTime(),
                Active = true
            };

            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category created successfully: {CategoryName} by {UserName} with DisplayOrder: {DisplayOrder}",
                category.CategoryName, currentUserName, displayOrder);

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, new
            {
                success = true,
                message = "Category created successfully.",
                data = new
                {
                    category.Id,
                    category.CategoryCode,
                    category.CategoryName,
                    category.DisplayOrder
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateCategory failed");
            return StatusCode(500, new { message = "Failed to create category.", error = ex.Message });
        }
    }

    // ===============================
    // GET ALL CATEGORIES
    // ===============================
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        _logger.LogInformation("GetAllCategories called");

        try
        {
            IQueryable<ProductCategory> query = _context.ProductCategories
                .Include(c => c.Status)
                .Include(c => c.Products.Where(p => p.Active == true))
                .Where(c => c.Active == true);

            // Role-based filtering
            if (!User.Identity?.IsAuthenticated == true || User.IsInRole("Customer"))
            {
                query = query.Where(c => c.Status.TableValue == "Active");
            }

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .Select(c => new
                {
                    c.Id,
                    c.CategoryCode,
                    c.CategoryName,
                    c.CategoryDescription,
                    c.CategoryImage,
                    c.DisplayOrder,
                    c.IsPopular,
                    c.IconClass,
                    Status = c.Status.TableValue,
                    StatusId = c.StatusId,
                    TotalProducts = c.Products.Count
                })
                .ToListAsync();

            // Build response with URLs
            var response = categories.Select(c => new CategoryListResponseDto
            {
                Id = c.Id,
                CategoryCode = c.CategoryCode,
                CategoryName = c.CategoryName,
                CategoryDescription = c.CategoryDescription,
                CategoryImage = c.CategoryImage,
                CategoryImageUrl = !string.IsNullOrEmpty(c.CategoryImage) ? GetFullUrl(c.CategoryImage) : null,
                DisplayOrder = c.DisplayOrder,
                IsPopular = c.IsPopular,
                IconClass = c.IconClass,
                Status = c.Status,
                StatusId = c.StatusId,
                StatusColor = GetStatusColor(c.Status),
                TotalProducts = c.TotalProducts
            }).ToList();

            return Ok(new
            {
                success = true,
                data = response,
                count = response.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllCategories failed");
            return StatusCode(500, new { message = "Failed to retrieve categories.", error = ex.Message });
        }
    }

    // ===============================
    // GET CATEGORY BY ID
    // ===============================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(string id)
    {
        _logger.LogInformation("GetCategoryById called for CategoryId: {CategoryId}", id);

        try
        {
            var category = await _context.ProductCategories
                .Include(c => c.Status)
                .Include(c => c.Products.Where(p => p.Active == true))
                .FirstOrDefaultAsync(c => c.Id == id && c.Active == true);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            // Role-based access check
            if (!User.Identity?.IsAuthenticated == true || User.IsInRole("Customer"))
            {
                if (category.Status.TableValue != "Active")
                {
                    return NotFound(new { message = "Category not found." });
                }
            }

            var response = new CategoryResponseDto
            {
                Id = category.Id,
                CategoryCode = category.CategoryCode,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                CategoryImage = category.CategoryImage,
                CategoryImageUrl = !string.IsNullOrEmpty(category.CategoryImage) ? GetFullUrl(category.CategoryImage) : null,
                DisplayOrder = category.DisplayOrder,
                IsPopular = category.IsPopular,
                IconClass = category.IconClass,
                Status = category.Status.TableValue,
                StatusId = category.StatusId,
                StatusColor = GetStatusColor(category.Status.TableValue),
                CreatedBy = category.CreatedBy,
                CreatedDate = category.CreatedDate,
                ModifiedBy = category.ModifiedBy ?? "Not modified",
                ModifiedDate = category.ModifiedDate,
                IsActive = category.Active ?? false,
                TotalProducts = category.Products.Count,
                Products = category.Products.Select(p => new CategoryProductDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    BrandName = p.BrandName,
                    IsFeatured = p.IsFeatured ?? false,
                    ProductImage = p.ProductImage,
                    ProductImageUrl = !string.IsNullOrEmpty(p.ProductImage) ? GetFullUrl(p.ProductImage) : null
                }).ToList()
            };

            return Ok(new { success = true, data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCategoryById failed for CategoryId: {CategoryId}", id);
            return StatusCode(500, new { message = "Failed to retrieve category.", error = ex.Message });
        }
    }

    // ===============================
    // UPDATE CATEGORY (SuperAdmin only)
    // ===============================
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateCategory(string id, [FromForm] CategoryUpdateDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("UpdateCategory called by: {UserName}, CategoryId: {CategoryId}",
            currentUserName, id);

        try
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.Active == true);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            bool hasChanges = false;

            // Update name with uniqueness check
            if (!string.IsNullOrWhiteSpace(dto.CategoryName) && dto.CategoryName.Trim() != category.CategoryName)
            {
                if (await _context.ProductCategories.AnyAsync(c =>
                    c.CategoryName == dto.CategoryName.Trim() && c.Id != id))
                {
                    return BadRequest(new { message = "Category with this name already exists." });
                }

                category.CategoryName = dto.CategoryName.Trim();
                hasChanges = true;
            }

            if (dto.CategoryDescription != null && dto.CategoryDescription != category.CategoryDescription)
            {
                category.CategoryDescription = dto.CategoryDescription.Trim();
                hasChanges = true;
            }

            if (dto.DisplayOrder.HasValue && dto.DisplayOrder != category.DisplayOrder)
            {
                category.DisplayOrder = dto.DisplayOrder;
                hasChanges = true;
            }

            if (dto.IsPopular.HasValue && dto.IsPopular != category.IsPopular)
            {
                category.IsPopular = dto.IsPopular;
                hasChanges = true;
            }

            if (dto.IconClass != null && dto.IconClass != category.IconClass)
            {
                category.IconClass = dto.IconClass;
                hasChanges = true;
            }

            // Handle category image upload
            if (dto.CategoryImage != null && dto.CategoryImage.Length > 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(category.CategoryImage))
                        _fileService.DeleteCategoryImage(category.CategoryImage);

                    var imagePath = await _fileService.SaveCategoryImageAsync(dto.CategoryImage, category.Id);
                    category.CategoryImage = imagePath;
                    hasChanges = true;
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            // Update status
            if (dto.StatusId.HasValue && dto.StatusId.Value != category.StatusId)
            {
                var statusExists = await _context.MasterTables
                    .AnyAsync(m => m.Id == dto.StatusId.Value && m.TableName == "Status" && m.Active == true);

                if (!statusExists)
                    return BadRequest(new { message = "Invalid status selected." });

                category.StatusId = dto.StatusId.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                category.ModifiedDate = GetIndianTime();
                category.ModifiedBy = currentUserName;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Category updated successfully: {CategoryId} by {UserName}",
                    id, currentUserName);
            }

            return await GetCategoryById(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateCategory failed for CategoryId: {CategoryId}", id);
            return StatusCode(500, new { message = "Failed to update category.", error = ex.Message });
        }
    }

    // ===============================
    // UPDATE CATEGORY STATUS (SuperAdmin only)
    // ===============================
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateCategoryStatus(string id, [FromBody] CategoryStatusUpdateDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        _logger.LogInformation("UpdateCategoryStatus called by: {UserName}, CategoryId: {CategoryId}",
            currentUser?.FullName, id);

        try
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.Active == true);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            var statusExists = await _context.MasterTables
                .AnyAsync(m => m.Id == dto.StatusId && m.TableName == "Status" && m.Active == true);

            if (!statusExists)
                return BadRequest(new { message = "Invalid status selected." });

            category.StatusId = dto.StatusId;
            category.ModifiedDate = GetIndianTime();
            category.ModifiedBy = currentUser?.FullName ?? "System";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Category status updated: {CategoryId} to StatusId: {StatusId}",
                id, dto.StatusId);

            return Ok(new
            {
                success = true,
                message = "Category status updated successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateCategoryStatus failed for CategoryId: {CategoryId}", id);
            return StatusCode(500, new { message = "Failed to update category status.", error = ex.Message });
        }
    }

    // ===============================
    // DELETE CATEGORY (Soft Delete - SuperAdmin only)
    // ===============================
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        _logger.LogInformation("DeleteCategory called by: {UserName}, CategoryId: {CategoryId}",
            currentUser?.FullName, id);

        try
        {
            var category = await _context.ProductCategories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && c.Active == true);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            // Check if category has active products
            if (category.Products.Any(p => p.Active == true))
            {
                return BadRequest(new
                {
                    message = "Cannot delete category with active products. Please deactivate or delete products first.",
                    activeProducts = category.Products.Count(p => p.Active == true)
                });
            }

            // Soft delete category
            category.Active = false;
            category.ModifiedDate = GetIndianTime();
            category.ModifiedBy = currentUser?.FullName ?? "System";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Category deleted successfully: {CategoryId} by {UserName}",
                id, currentUser?.FullName);

            return Ok(new
            {
                success = true,
                message = "Category deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteCategory failed for CategoryId: {CategoryId}", id);
            return StatusCode(500, new { message = "Failed to delete category.", error = ex.Message });
        }
    }

    // ===============================
    // GET POPULAR CATEGORIES (Public)
    // ===============================
    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPopularCategories()
    {
        try
        {
            var categories = await _context.ProductCategories
                .Include(c => c.Status)
                .Include(c => c.Products.Where(p => p.Active == true))
                .Where(c => c.Active == true && c.Status.TableValue == "Active" && c.IsPopular == true)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new
                {
                    c.Id,
                    c.CategoryCode,
                    c.CategoryName,
                    c.CategoryDescription,
                    c.CategoryImage,
                    c.IconClass,
                    ProductCount = c.Products.Count
                })
                .Take(8)
                .ToListAsync();

            // Build response with URLs
            var response = categories.Select(c => new
            {
                c.Id,
                c.CategoryCode,
                c.CategoryName,
                c.CategoryDescription,
                CategoryImageUrl = !string.IsNullOrEmpty(c.CategoryImage) ? GetFullUrl(c.CategoryImage) : null,
                c.IconClass,
                c.ProductCount
            }).ToList();

            return Ok(new { success = true, data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPopularCategories failed");
            return StatusCode(500, new { message = "Failed to retrieve popular categories.", error = ex.Message });
        }
    }

    // ===============================
    // REORDER CATEGORIES (SuperAdmin only)
    // ===============================
    [HttpPost("reorder")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ReorderCategories([FromBody] Dictionary<string, int> categoryOrders)
    {
        _logger.LogInformation("ReorderCategories called by: {UserName}",
            User.FindFirstValue("FullName") ?? "Unknown");

        try
        {
            var categoryIds = categoryOrders.Keys.ToList();
            var categories = await _context.ProductCategories
                .Where(c => categoryIds.Contains(c.Id) && c.Active == true)
                .ToListAsync();

            if (categories.Count != categoryIds.Count)
                return BadRequest(new { message = "Some categories not found or inactive." });

            foreach (var category in categories)
            {
                if (categoryOrders.TryGetValue(category.Id, out int newOrder))
                {
                    category.DisplayOrder = newOrder;
                    category.ModifiedDate = GetIndianTime();
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Categories reordered successfully");

            return Ok(new
            {
                success = true,
                message = "Categories reordered successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReorderCategories failed");
            return StatusCode(500, new { message = "Failed to reorder categories.", error = ex.Message });
        }
    }

    // ===============================
    // HELPER METHODS
    // ===============================
    private string GetUserIdFromToken()
    {
        return User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    private DateTime GetIndianTime()
    {
        DateTime utcNow = DateTime.UtcNow;
        TimeZoneInfo indianTimeZone;
        try
        {
            indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }
        catch
        {
            indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        }
        return TimeZoneInfo.ConvertTimeFromUtc(utcNow, indianTimeZone);
    }

    private string GetFullUrl(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return null;

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
            return relativePath;

        var baseUrl = $"{request.Scheme}://{request.Host}";
        return $"{baseUrl}{relativePath}";
    }

    private string GetStatusColor(string status)
    {
        return status?.ToLower() switch
        {
            "active" => "green",
            "inactive" => "gray",
            "suspended" => "red",
            "pending" => "orange",
            _ => "blue"
        };
    }
}