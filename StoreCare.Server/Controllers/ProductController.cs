using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreCare.Server.Data;
using StoreCare.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace StoreCare.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly StoreCareDbContext _context;
    private readonly ILogger<ProductController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductController(
        StoreCareDbContext context,
        ILogger<ProductController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    // ===============================
    // DTOs
    // ===============================
    public class ProductResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ProductDescription { get; set; }
        public string? ProductImage { get; set; }
        public string? ProductImageUrl { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public bool? IsFeatured { get; set; }
        public int? ViewCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public List<StoreAssignmentDto> AssignedStores { get; set; } = new();
    }

    public class StoreAssignmentDto
    {
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public bool CanManage { get; set; }
    }

    public class ProductCreateDto
    {
        [Required, MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ProductDescription { get; set; }

        [Required]
        public string CategoryId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BrandName { get; set; }

        public bool? IsFeatured { get; set; }

        public IFormFile? ProductImage { get; set; }
    }

    public class ProductUpdateDto
    {
        [MaxLength(200)]
        public string? ProductName { get; set; }

        [MaxLength(1000)]
        public string? ProductDescription { get; set; }

        public string? CategoryId { get; set; }

        [MaxLength(100)]
        public string? BrandName { get; set; }

        public bool? IsFeatured { get; set; }

        public IFormFile? ProductImage { get; set; }

        public int? StatusId { get; set; }
    }

    public class ProductStatusUpdateDto
    {
        [Required]
        public int StatusId { get; set; }
    }

    public class ProductListResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public string? ProductImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public bool? IsFeatured { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public int StoreCount { get; set; }
        public int ItemCount { get; set; }
    }

    // ===============================
    // CREATE PRODUCT (SuperAdmin only)
    // ===============================
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateProduct([FromForm] ProductCreateDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("CreateProduct called by: {UserName}", currentUserName);

        try
        {
            // Validate category
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.Active == true);

            if (category == null)
                return BadRequest(new { message = "Invalid or inactive category." });

            // Validate unique product name within category
            if (await _context.Products.AnyAsync(p =>
                p.ProductName == dto.ProductName.Trim() && p.CategoryId == dto.CategoryId))
            {
                return BadRequest(new { message = "Product with this name already exists in the category." });
            }

            // Get active status
            var activeStatus = await _context.MasterTables
                .FirstOrDefaultAsync(m => m.TableName == "Status" && m.TableValue == "Active");

            if (activeStatus == null)
                return StatusCode(500, new { message = "Active status not configured." });

            // Generate unique product code
            var productCode = "PRD-" + Guid.NewGuid().ToString()[..8].ToUpper();
            var productId = Guid.NewGuid().ToString();

            string? imagePath = null;
            if (dto.ProductImage != null && dto.ProductImage.Length > 0)
            {
                imagePath = await SaveProductImage(dto.ProductImage, productId);
            }

            var product = new Product
            {
                Id = productId,
                ProductCode = productCode,
                ProductName = dto.ProductName.Trim(),
                ProductDescription = dto.ProductDescription?.Trim(),
                ProductImage = imagePath,
                CategoryId = dto.CategoryId,
                BrandName = dto.BrandName?.Trim(),
                IsFeatured = dto.IsFeatured ?? false,
                ViewCount = 0,
                StatusId = activeStatus.Id,
                CreatedBy = currentUserName,
                CreatedDate = GetIndianTime(),
                ModifiedBy = currentUserName,
                ModifiedDate = GetIndianTime(),
                Active = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product created successfully: {ProductName} by {UserName}",
                product.ProductName, currentUserName);

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, new
            {
                success = true,
                message = "Product created successfully.",
                data = new
                {
                    product.Id,
                    product.ProductCode,
                    product.ProductName,
                    CategoryName = category.CategoryName
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateProduct failed");
            return StatusCode(500, new { message = "Failed to create product.", error = ex.Message });
        }
    }

    // ===============================
    // GET ALL PRODUCTS
    // ===============================
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,StoreAdmin,Customer")]
    public async Task<IActionResult> GetAllProducts()
    {
        var currentUserId = GetUserIdFromToken();
        var currentUser = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        _logger.LogInformation("GetAllProducts called by UserId: {UserId}, Role: {Role}",
            currentUserId, currentUser?.Role?.TableValue);

        try
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.StoreProductAssignments)
                .Include(p => p.Items)
                .Where(p => p.Active == true);

            // Role-based filtering
            if (User.IsInRole("StoreAdmin"))
            {
                var userStoreId = currentUser?.StoreId;
                if (string.IsNullOrEmpty(userStoreId))
                    return BadRequest(new { message = "StoreAdmin not associated with any store." });

                query = query.Where(p => p.StoreProductAssignments
                    .Any(sp => sp.StoreId == userStoreId && sp.Active == true));
            }
            else if (User.IsInRole("Customer") || !User.Identity?.IsAuthenticated == true)
            {
                query = query.Where(p => p.Status.TableValue == "Active");
            }

            // First fetch raw data without URL transformation
            var products = await query
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedDate)
                .Select(p => new
                {
                    p.Id,
                    p.ProductCode,
                    p.ProductName,
                    p.ProductImage,
                    CategoryName = p.Category.CategoryName,
                    p.BrandName,
                    p.IsFeatured,
                    Status = p.Status.TableValue,
                    StatusId = p.StatusId,
                    StoreCount = p.StoreProductAssignments.Count(sp => sp.Active == true),
                    ItemCount = p.Items.Count(i => i.Active == true)
                })
                .ToListAsync();

            // Apply URL transformation in memory
            var response = products.Select(p => new ProductListResponseDto
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                ProductImage = p.ProductImage,
                ProductImageUrl = !string.IsNullOrEmpty(p.ProductImage) ? GetFullUrl(p.ProductImage) : null,
                CategoryName = p.CategoryName,
                BrandName = p.BrandName,
                IsFeatured = p.IsFeatured,
                Status = p.Status,
                StatusId = p.StatusId,
                StatusColor = GetStatusColor(p.Status),
                StoreCount = p.StoreCount,
                ItemCount = p.ItemCount
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
            _logger.LogError(ex, "GetAllProducts failed for UserId: {UserId}", currentUserId);
            return StatusCode(500, new { message = "Failed to retrieve products.", error = ex.Message });
        }
    }

    // ===============================
    // GET PRODUCT BY ID
    // ===============================
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,StoreAdmin,Customer")]
    public async Task<IActionResult> GetProductById(string id)
    {
        var currentUserId = GetUserIdFromToken();
        var currentUser = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        _logger.LogInformation("GetProductById called by UserId: {UserId}, ProductId: {ProductId}",
            currentUserId, id);

        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.StoreProductAssignments)
                    .ThenInclude(sp => sp.Store)
                .FirstOrDefaultAsync(p => p.Id == id && p.Active == true);

            if (product == null)
                return NotFound(new { message = "Product not found." });

            // Role-based access check
            if (User.IsInRole("StoreAdmin"))
            {
                var userStoreId = currentUser?.StoreId;
                if (!product.StoreProductAssignments.Any(sp => sp.StoreId == userStoreId && sp.Active == true))
                {
                    _logger.LogWarning("StoreAdmin {UserId} attempted to access unauthorized product {ProductId}",
                        currentUserId, id);
                    return Forbid();
                }
            }
            else if (User.IsInRole("Customer") || !User.Identity?.IsAuthenticated == true)
            {
                if (product.Status.TableValue != "Active")
                {
                    return NotFound(new { message = "Product not found." });
                }
            }

            // Increment view count
            product.ViewCount = (product.ViewCount ?? 0) + 1;
            await _context.SaveChangesAsync();

            var response = new ProductResponseDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                ProductImage = product.ProductImage,
                ProductImageUrl = !string.IsNullOrEmpty(product.ProductImage) ? GetFullUrl(product.ProductImage) : null,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.CategoryName,
                BrandName = product.BrandName,
                IsFeatured = product.IsFeatured,
                ViewCount = product.ViewCount,
                Status = product.Status.TableValue,
                StatusId = product.StatusId,
                StatusColor = GetStatusColor(product.Status.TableValue),
                CreatedBy = product.CreatedBy,
                CreatedDate = product.CreatedDate,
                ModifiedBy = product.ModifiedBy ?? "Not modified",
                ModifiedDate = product.ModifiedDate,
                IsActive = product.Active ?? false,
                AssignedStores = product.StoreProductAssignments
                    .Where(sp => sp.Active == true)
                    .Select(sp => new StoreAssignmentDto
                    {
                        StoreId = sp.StoreId,
                        StoreName = sp.Store.StoreName,
                        CanManage = sp.CanManage ?? false
                    })
                    .ToList()
            };

            return Ok(new { success = true, data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProductById failed for ProductId: {ProductId}", id);
            return StatusCode(500, new { message = "Failed to retrieve product.", error = ex.Message });
        }
    }

    // ===============================
    // GET PRODUCTS BY CATEGORY
    // ===============================
    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductsByCategory(string categoryId)
    {
        try
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.Active == true);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            var products = await _context.Products
                .Include(p => p.Status)
                .Where(p => p.CategoryId == categoryId && p.Active == true && p.Status.TableValue == "Active")
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedDate)
                .Select(p => new
                {
                    p.Id,
                    p.ProductCode,
                    p.ProductName,
                    p.ProductDescription,
                    p.ProductImage,
                    p.BrandName,
                    p.IsFeatured
                })
                .ToListAsync();

            // Apply URL transformation in memory
            var response = products.Select(p => new
            {
                p.Id,
                p.ProductCode,
                p.ProductName,
                p.ProductDescription,
                ProductImageUrl = !string.IsNullOrEmpty(p.ProductImage) ? GetFullUrl(p.ProductImage) : null,
                p.BrandName,
                p.IsFeatured
            }).ToList();

            return Ok(new
            {
                success = true,
                categoryName = category.CategoryName,
                data = response,
                count = response.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProductsByCategory failed for CategoryId: {CategoryId}", categoryId);
            return StatusCode(500, new { message = "Failed to retrieve products.", error = ex.Message });
        }
    }

    // ===============================
    // UPDATE PRODUCT (SuperAdmin only)
    // ===============================
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProduct(string id, [FromForm] ProductUpdateDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("UpdateProduct called by: {UserName}, ProductId: {ProductId}",
            currentUserName, id);

        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.Active == true);

            if (product == null)
                return NotFound(new { message = "Product not found." });

            bool hasChanges = false;

            // Update product name with uniqueness check
            if (!string.IsNullOrWhiteSpace(dto.ProductName) && dto.ProductName.Trim() != product.ProductName)
            {
                if (await _context.Products.AnyAsync(p =>
                    p.ProductName == dto.ProductName.Trim() &&
                    p.CategoryId == product.CategoryId &&
                    p.Id != id))
                {
                    return BadRequest(new { message = "Product with this name already exists in the category." });
                }

                product.ProductName = dto.ProductName.Trim();
                hasChanges = true;
            }

            if (dto.ProductDescription != null && dto.ProductDescription != product.ProductDescription)
            {
                product.ProductDescription = dto.ProductDescription.Trim();
                hasChanges = true;
            }

            // Update category if provided
            if (!string.IsNullOrWhiteSpace(dto.CategoryId) && dto.CategoryId != product.CategoryId)
            {
                var category = await _context.ProductCategories
                    .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.Active == true);

                if (category == null)
                    return BadRequest(new { message = "Invalid category." });

                product.CategoryId = dto.CategoryId;
                hasChanges = true;
            }

            if (dto.BrandName != null && dto.BrandName != product.BrandName)
            {
                product.BrandName = dto.BrandName.Trim();
                hasChanges = true;
            }

            if (dto.IsFeatured.HasValue && dto.IsFeatured != product.IsFeatured)
            {
                product.IsFeatured = dto.IsFeatured;
                hasChanges = true;
            }

            // Handle product image upload
            if (dto.ProductImage != null && dto.ProductImage.Length > 0)
            {
                var newImagePath = await SaveProductImage(dto.ProductImage, product.Id, product.ProductImage);
                if (newImagePath != product.ProductImage)
                {
                    product.ProductImage = newImagePath;
                    hasChanges = true;
                }
            }

            // Update status
            if (dto.StatusId.HasValue && dto.StatusId != product.StatusId)
            {
                var statusExists = await _context.MasterTables
                    .AnyAsync(m => m.Id == dto.StatusId.Value && m.TableName == "Status" && m.Active == true);

                if (!statusExists)
                    return BadRequest(new { message = "Invalid status selected." });

                product.StatusId = dto.StatusId.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                product.ModifiedDate = GetIndianTime();
                product.ModifiedBy = currentUserName;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product updated successfully: {ProductId} by {UserName}",
                    id, currentUserName);
            }

            return await GetProductById(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateProduct failed for ProductId: {ProductId}", id);
            return StatusCode(500, new { message = "Failed to update product.", error = ex.Message });
        }
    }

    // ===============================
    // UPDATE PRODUCT STATUS (SuperAdmin only)
    // ===============================
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateProductStatus(string id, [FromBody] ProductStatusUpdateDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        _logger.LogInformation("UpdateProductStatus called by: {UserName}, ProductId: {ProductId}",
            currentUser?.FullName, id);

        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.Active == true);

            if (product == null)
                return NotFound(new { message = "Product not found." });

            var statusExists = await _context.MasterTables
                .AnyAsync(m => m.Id == dto.StatusId && m.TableName == "Status" && m.Active == true);

            if (!statusExists)
                return BadRequest(new { message = "Invalid status selected." });

            product.StatusId = dto.StatusId;
            product.ModifiedDate = GetIndianTime();
            product.ModifiedBy = currentUser?.FullName ?? "System";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product status updated: {ProductId} to StatusId: {StatusId}",
                id, dto.StatusId);

            return Ok(new
            {
                success = true,
                message = "Product status updated successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateProductStatus failed for ProductId: {ProductId}", id);
            return StatusCode(500, new { message = "Failed to update product status.", error = ex.Message });
        }
    }

    // ===============================
    // DELETE PRODUCT (Soft Delete - SuperAdmin only)
    // ===============================
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        _logger.LogInformation("DeleteProduct called by: {UserName}, ProductId: {ProductId}",
            currentUser?.FullName, id);

        try
        {
            var product = await _context.Products
                .Include(p => p.Items)
                .Include(p => p.StoreProductAssignments)
                .FirstOrDefaultAsync(p => p.Id == id && p.Active == true);

            if (product == null)
                return NotFound(new { message = "Product not found." });

            // Check if product has items
            if (product.Items.Any(i => i.Active == true))
            {
                return BadRequest(new
                {
                    message = "Cannot delete product with active items. Please deactivate or delete items first."
                });
            }

            // Soft delete product
            product.Active = false;
            product.ModifiedDate = GetIndianTime();
            product.ModifiedBy = currentUser?.FullName ?? "System";

            // Soft delete all store product assignments
            foreach (var assignment in product.StoreProductAssignments)
            {
                assignment.Active = false;
                assignment.ModifiedDate = GetIndianTime();
                assignment.ModifiedBy = currentUser?.FullName ?? "System";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product deleted successfully: {ProductId} by {UserName}",
                id, currentUser?.FullName);

            return Ok(new
            {
                success = true,
                message = "Product deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteProduct failed for ProductId: {ProductId}", id);
            return StatusCode(500, new { message = "Failed to delete product.", error = ex.Message });
        }
    }

    // ===============================
    // GET FEATURED PRODUCTS (Public)
    // ===============================
    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeaturedProducts()
    {
        try
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Where(p => p.Active == true &&
                            p.Status.TableValue == "Active" &&
                            p.IsFeatured == true)
                .OrderByDescending(p => p.CreatedDate)
                .Take(8)
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    p.ProductDescription,
                    p.ProductImage,
                    CategoryName = p.Category.CategoryName,
                    p.BrandName
                })
                .ToListAsync();

            var response = products.Select(p => new
            {
                p.Id,
                p.ProductName,
                p.ProductDescription,
                ProductImageUrl = !string.IsNullOrEmpty(p.ProductImage) ? GetFullUrl(p.ProductImage) : null,
                p.CategoryName,
                p.BrandName
            }).ToList();

            return Ok(new { success = true, data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFeaturedProducts failed");
            return StatusCode(500, new { message = "Failed to retrieve featured products.", error = ex.Message });
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

    private async Task<string> SaveProductImage(IFormFile file, string productId, string? oldImagePath = null)
    {
        if (file == null || file.Length == 0)
            return null;

        // Validate file size (max 2MB)
        if (file.Length > 2 * 1024 * 1024)
            throw new InvalidOperationException("Image size exceeds 2MB limit");

        // Validate file type
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Invalid file format. Only images are allowed.");

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImages");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Delete old image if exists
        if (!string.IsNullOrEmpty(oldImagePath))
        {
            var oldFileName = Path.GetFileName(oldImagePath);
            var oldFilePath = Path.Combine(uploadsFolder, oldFileName);
            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);
        }

        var fileName = $"{productId}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/ProductImages/{fileName}";
    }
}