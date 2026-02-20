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
public class StoreController : ControllerBase
{
    private readonly StoreCareDbContext _context;
    private readonly ILogger<StoreController> _logger;
    private readonly IFileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StoreController(
        StoreCareDbContext context,
        ILogger<StoreController> logger,
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
    public class StoreResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string StoreCode { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? StoreLogo { get; set; }
        public string? StoreLogoUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalProducts { get; set; }
        public List<StoreProductSummaryDto> Products { get; set; } = new();
    }

    public class StoreProductSummaryDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool CanManage { get; set; }
    }

    public class StoreCreateDto
    {
        [Required, MaxLength(200)]
        public string StoreName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Contact number must be 10 digits.")]
        public string? ContactNumber { get; set; }

        [EmailAddress, MaxLength(100)]
        public string? Email { get; set; }

        public IFormFile? StoreLogo { get; set; }
    }

    public class StoreUpdateDto
    {
        [MaxLength(200)]
        public string? StoreName { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Contact number must be 10 digits.")]
        public string? ContactNumber { get; set; }

        [EmailAddress, MaxLength(100)]
        public string? Email { get; set; }

        public IFormFile? StoreLogo { get; set; }

        public int? StatusId { get; set; }
    }

    public class StoreStatusUpdateDto
    {
        [Required]
        public int StatusId { get; set; }
    }

    public class StoreListResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string StoreCode { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? StoreLogo { get; set; }
        public string? StoreLogoUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalProducts { get; set; }
    }

    // ===============================
    // GET ALL STORES (SuperAdmin sees all; StoreAdmin sees only own)
    // ===============================
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,StoreAdmin")]
    public async Task<IActionResult> GetAllStores()
    {
        var currentUserId = GetUserIdFromToken();
        var currentUser = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (currentUser == null)
            return Unauthorized(new { message = "User not found." });

        _logger.LogInformation("GetAllStores called by UserId: {UserId}, Role: {Role}",
            currentUserId, currentUser.Role?.TableValue);

        try
        {
            IQueryable<Store> query = _context.Stores
                .Include(s => s.Status)
                .Include(s => s.Users)
                .Include(s => s.StoreProductAssignments)
                    .ThenInclude(sp => sp.Product)
                .Where(s => s.Active == true);

            // Role‑based filtering
            if (User.IsInRole("StoreAdmin"))
            {
                var userStoreId = currentUser.StoreId;
                if (string.IsNullOrEmpty(userStoreId))
                    return BadRequest(new { message = "StoreAdmin not associated with any store." });

                query = query.Where(s => s.Id == userStoreId);
            }

            var stores = await query
                .OrderByDescending(s => s.CreatedDate)
                .Select(s => new
                {
                    s.Id,
                    s.StoreCode,
                    s.StoreName,
                    s.Address,
                    s.ContactNumber,
                    s.Email,
                    s.StoreLogo,
                    Status = s.Status.TableValue,
                    StatusId = s.StatusId,
                    s.CreatedBy,
                    s.CreatedDate,
                    IsActive = s.Active ?? false,
                    TotalEmployees = s.Users.Count(u => u.Active == true),
                    TotalProducts = s.StoreProductAssignments.Count(sp => sp.Active == true)
                })
                .ToListAsync();

            var response = stores.Select(s => new StoreListResponseDto
            {
                Id = s.Id,
                StoreCode = s.StoreCode,
                StoreName = s.StoreName,
                Address = s.Address,
                ContactNumber = s.ContactNumber,
                Email = s.Email,
                StoreLogo = s.StoreLogo,
                StoreLogoUrl = !string.IsNullOrEmpty(s.StoreLogo) ? GetFullUrl(s.StoreLogo) : null,
                Status = s.Status,
                StatusId = s.StatusId,
                StatusColor = GetStatusColor(s.Status),
                CreatedBy = s.CreatedBy,
                CreatedDate = s.CreatedDate,
                IsActive = s.IsActive,
                TotalEmployees = s.TotalEmployees,
                TotalProducts = s.TotalProducts
            }).ToList();

            return Ok(new { success = true, data = response, count = response.Count, role = currentUser.Role?.TableValue });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllStores failed for UserId: {UserId}", currentUserId);
            return StatusCode(500, new { message = "Failed to retrieve stores.", error = ex.Message });
        }
    }

    // ===============================
    // GET STORE BY ID
    // ===============================
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,StoreAdmin")]
    public async Task<IActionResult> GetStoreById(string id)
    {
        var currentUserId = GetUserIdFromToken();
        var currentUser = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (currentUser == null)
            return Unauthorized(new { message = "User not found." });

        _logger.LogInformation("GetStoreById called by UserId: {UserId}, StoreId: {StoreId}", currentUserId, id);

        try
        {
            // Role‑based access check
            if (User.IsInRole("StoreAdmin") && currentUser.StoreId != id)
            {
                _logger.LogWarning("StoreAdmin {UserId} attempted to access unauthorized store {StoreId}", currentUserId, id);
                return Forbid();
            }

            var store = await _context.Stores
                .Include(s => s.Status)
                .Include(s => s.Users.Where(u => u.Active == true))
                .Include(s => s.StoreProductAssignments.Where(sp => sp.Active == true))
                    .ThenInclude(sp => sp.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(s => s.Id == id && s.Active == true);

            if (store == null)
                return NotFound(new { message = "Store not found." });

            var response = new StoreResponseDto
            {
                Id = store.Id,
                StoreCode = store.StoreCode,
                StoreName = store.StoreName,
                Address = store.Address,
                ContactNumber = store.ContactNumber,
                Email = store.Email,
                StoreLogo = store.StoreLogo,
                StoreLogoUrl = !string.IsNullOrEmpty(store.StoreLogo) ? GetFullUrl(store.StoreLogo) : null,
                Status = store.Status.TableValue,
                StatusId = store.StatusId,
                StatusColor = GetStatusColor(store.Status.TableValue),
                CreatedBy = store.CreatedBy,
                CreatedDate = store.CreatedDate,
                ModifiedBy = store.ModifiedBy ?? "Not modified",
                ModifiedDate = store.ModifiedDate,
                IsActive = store.Active ?? false,
                TotalEmployees = store.Users.Count,
                TotalProducts = store.StoreProductAssignments.Count,
                Products = store.StoreProductAssignments.Select(sp => new StoreProductSummaryDto
                {
                    ProductId = sp.ProductId,
                    ProductName = sp.Product.ProductName,
                    ProductCode = sp.Product.ProductCode,
                    CategoryName = sp.Product.Category?.CategoryName ?? "Unknown",
                    CanManage = sp.CanManage ?? false
                }).ToList()
            };

            return Ok(new { success = true, data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStoreById failed for StoreId: {StoreId}", id);
            return StatusCode(500, new { message = "Failed to retrieve store.", error = ex.Message });
        }
    }

    // ===============================
    // UPDATE STORE (SuperAdmin or the StoreAdmin of that store)
    // ===============================
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,StoreAdmin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateStore(string id, [FromForm] StoreUpdateDto dto)
    {
        var currentUserId = GetUserIdFromToken();
        var currentUser = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (currentUser == null)
            return Unauthorized(new { message = "User not found." });

        _logger.LogInformation("UpdateStore called by UserId: {UserId}, StoreId: {StoreId}", currentUserId, id);

        try
        {
            // Role‑based access check
            if (User.IsInRole("StoreAdmin") && currentUser.StoreId != id)
            {
                _logger.LogWarning("StoreAdmin {UserId} attempted to update unauthorized store {StoreId}", currentUserId, id);
                return Forbid();
            }

            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == id && s.Active == true);

            if (store == null)
                return NotFound(new { message = "Store not found." });

            bool hasChanges = false;

            // Update basic fields
            if (!string.IsNullOrWhiteSpace(dto.StoreName) && dto.StoreName != store.StoreName)
            {
                store.StoreName = dto.StoreName.Trim();
                hasChanges = true;
            }

            if (dto.Address != null && dto.Address != store.Address)
            {
                store.Address = dto.Address.Trim();
                hasChanges = true;
            }

            if (dto.ContactNumber != null && dto.ContactNumber != store.ContactNumber)
            {
                store.ContactNumber = dto.ContactNumber.Trim();
                hasChanges = true;
            }

            if (dto.Email != null && dto.Email != store.Email)
            {
                store.Email = dto.Email.Trim().ToLower();
                hasChanges = true;
            }

            // Handle store logo upload
            if (dto.StoreLogo != null && dto.StoreLogo.Length > 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(store.StoreLogo))
                        _fileService.DeleteStoreLogo(store.StoreLogo);

                    var logoPath = await _fileService.SaveStoreLogoAsync(dto.StoreLogo, store.Id);
                    store.StoreLogo = logoPath;
                    hasChanges = true;

                    _logger.LogInformation("Store logo updated for StoreId: {StoreId}", id);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            // Update status (SuperAdmin only)
            if (User.IsInRole("SuperAdmin") && dto.StatusId.HasValue && dto.StatusId.Value != store.StatusId)
            {
                var statusExists = await _context.MasterTables
                    .AnyAsync(m => m.Id == dto.StatusId.Value && m.TableName == "Status" && m.Active == true);
                if (!statusExists)
                    return BadRequest(new { message = "Invalid status selected." });

                store.StatusId = dto.StatusId.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                store.ModifiedDate = GetIndianTime();
                store.ModifiedBy = currentUser.FullName;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Store updated successfully: {StoreId} by {UserName}", id, currentUser.FullName);
            }

            return await GetStoreById(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateStore failed for StoreId: {StoreId}", id);
            return StatusCode(500, new { message = "Failed to update store.", error = ex.Message });
        }
    }

    // ===============================
    // UPDATE STORE STATUS (SuperAdmin only)
    // ===============================
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateStoreStatus(string id, [FromBody] StoreStatusUpdateDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        _logger.LogInformation("UpdateStoreStatus called by: {UserName}, StoreId: {StoreId}", currentUser?.FullName, id);

        try
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == id && s.Active == true);

            if (store == null)
                return NotFound(new { message = "Store not found." });

            var statusExists = await _context.MasterTables
                .AnyAsync(m => m.Id == dto.StatusId && m.TableName == "Status" && m.Active == true);
            if (!statusExists)
                return BadRequest(new { message = "Invalid status selected." });

            store.StatusId = dto.StatusId;
            store.ModifiedDate = GetIndianTime();
            store.ModifiedBy = currentUser?.FullName ?? "System";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Store status updated: {StoreId} to StatusId: {StatusId}", id, dto.StatusId);

            return Ok(new { success = true, message = "Store status updated successfully.", statusId = dto.StatusId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateStoreStatus failed for StoreId: {StoreId}", id);
            return StatusCode(500, new { message = "Failed to update store status.", error = ex.Message });
        }
    }

    // ===============================
    // DELETE STORE (Soft Delete - SuperAdmin only)
    // ===============================
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteStore(string id)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        _logger.LogInformation("DeleteStore called by: {UserName}, StoreId: {StoreId}", currentUser?.FullName, id);

        try
        {
            var store = await _context.Stores
                .Include(s => s.Users)
                .Include(s => s.StoreProductAssignments)
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id && s.Active == true);

            if (store == null)
                return NotFound(new { message = "Store not found." });

            // Soft delete store
            store.Active = false;
            store.ModifiedDate = GetIndianTime();
            store.ModifiedBy = currentUser?.FullName ?? "System";

            // Soft delete all users in this store
            foreach (var user in store.Users)
            {
                user.Active = false;
                user.ModifiedDate = GetIndianTime();
                user.ModifiedBy = currentUser?.FullName ?? "System";
            }

            // Soft delete all store product assignments
            foreach (var assignment in store.StoreProductAssignments)
            {
                assignment.Active = false;
                assignment.ModifiedDate = GetIndianTime();
                assignment.ModifiedBy = currentUser?.FullName ?? "System";
            }

            // Soft delete all items in this store
            foreach (var item in store.Items)
            {
                item.Active = false;
                item.ModifiedDate = GetIndianTime();
                item.ModifiedBy = currentUser?.FullName ?? "System";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Store deleted successfully: {StoreId} by {UserName}", id, currentUser?.FullName);

            return Ok(new { success = true, message = "Store and all associated records deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteStore failed for StoreId: {StoreId}", id);
            return StatusCode(500, new { message = "Failed to delete store.", error = ex.Message });
        }
    }

    // ===============================
    // GET STORE STATISTICS (SuperAdmin only)
    // ===============================
    [HttpGet("statistics")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetStoreStatistics()
    {
        _logger.LogInformation("GetStoreStatistics called by: {UserName}", User.FindFirstValue("FullName") ?? "Unknown");

        try
        {
            var totalStores = await _context.Stores.CountAsync(s => s.Active == true);
            var activeStores = await _context.Stores
                .CountAsync(s => s.Active == true && s.Status.TableValue == "Active");
            var suspendedStores = await _context.Stores
                .CountAsync(s => s.Active == true && s.Status.TableValue == "Suspended");

            var storePerformance = await _context.Stores
                .Where(s => s.Active == true)
                .Select(s => new
                {
                    storeId = s.Id,
                    storeName = s.StoreName,
                    totalEmployees = s.Users.Count(u => u.Active == true),
                    totalProducts = s.StoreProductAssignments.Count(sp => sp.Active == true),
                    totalOrders = s.Orders.Count(o => o.Active == true),
                    status = s.Status.TableValue
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    overview = new
                    {
                        totalStores,
                        activeStores,
                        suspendedStores,
                        inactiveStores = totalStores - activeStores - suspendedStores
                    },
                    storePerformance
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStoreStatistics failed");
            return StatusCode(500, new { message = "Failed to retrieve store statistics.", error = ex.Message });
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