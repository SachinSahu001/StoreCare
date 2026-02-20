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
[Authorize(Roles = "SuperAdmin")]
public class StoreProductAssignmentController : ControllerBase
{
    private readonly StoreCareDbContext _context;
    private readonly ILogger<StoreProductAssignmentController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StoreProductAssignmentController(
        StoreCareDbContext context,
        ILogger<StoreProductAssignmentController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    // ===============================
    // DTOs
    // ===============================
    public class AssignmentResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool CanManage { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class AssignmentCreateDto
    {
        [Required]
        public string StoreId { get; set; } = string.Empty;

        [Required]
        public string ProductId { get; set; } = string.Empty;

        public bool CanManage { get; set; } = true;
    }

    public class AssignmentUpdateDto
    {
        public bool? CanManage { get; set; }
        public int? StatusId { get; set; }
    }

    public class BulkAssignmentDto
    {
        [Required]
        public string StoreId { get; set; } = string.Empty;

        [Required]
        public List<string> ProductIds { get; set; } = new();

        public bool CanManage { get; set; } = true;
    }

    public class CategoryProductAssignmentDto
    {
        [Required]
        public string StoreId { get; set; } = string.Empty;

        [Required]
        public string CategoryId { get; set; } = string.Empty;

        [Required]
        public List<string> ProductIds { get; set; } = new();

        public bool CanManage { get; set; } = true;
    }

    public class AssignmentListResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool CanManage { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
    }

    public class AvailableProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsAssigned { get; set; }
    }

    // ===============================
    // CREATE ASSIGNMENT (with reactivation)
    // ===============================
    [HttpPost]
    public async Task<IActionResult> CreateAssignment([FromBody] AssignmentCreateDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("CreateAssignment called by: {UserName}", currentUserName);

        try
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == dto.StoreId && s.Active == true);
            if (store == null)
                return BadRequest(new { message = "Invalid or inactive store." });

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.Active == true);
            if (product == null)
                return BadRequest(new { message = "Invalid or inactive product." });

            var existing = await _context.StoreProductAssignments
                .FirstOrDefaultAsync(sp => sp.StoreId == dto.StoreId && sp.ProductId == dto.ProductId);

            var activeStatus = await _context.MasterTables
                .FirstOrDefaultAsync(m => m.TableName == "Status" && m.TableValue == "Active")
                ?? throw new Exception("Active status not configured.");

            if (existing != null)
            {
                if (existing.Active == true)
                {
                    return Ok(new { success = false, message = "Product already assigned to this store." });
                }
                else
                {
                    // Reactivate
                    existing.Active = true;
                    existing.CanManage = dto.CanManage;
                    existing.StatusId = activeStatus.Id;
                    existing.ModifiedBy = currentUserName;
                    existing.ModifiedDate = GetIndianTime();

                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Product assignment reactivated successfully.",
                        data = new { existing.Id, store.StoreName, product.ProductName, product.Category?.CategoryName }
                    });
                }
            }

            var assignment = new StoreProductAssignment
            {
                Id = Guid.NewGuid().ToString(),
                StoreId = dto.StoreId,
                ProductId = dto.ProductId,
                CanManage = dto.CanManage,
                StatusId = activeStatus.Id,
                CreatedBy = currentUserName,
                CreatedDate = GetIndianTime(),
                ModifiedBy = currentUserName,
                ModifiedDate = GetIndianTime(),
                Active = true
            };

            _context.StoreProductAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assignment created: Store {StoreName} - Product {ProductName}", store.StoreName, product.ProductName);

            return Ok(new
            {
                success = true,
                message = "Product assigned to store successfully.",
                data = new { assignment.Id, store.StoreName, product.ProductName, product.Category?.CategoryName }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateAssignment failed");
            return StatusCode(500, new { message = "Failed to create assignment.", error = ex.Message });
        }
    }

    // ===============================
    // BULK CREATE / REACTIVATE ASSIGNMENTS
    // ===============================
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreateAssignments([FromBody] BulkAssignmentDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("BulkCreateAssignments called by: {UserName} for Store: {StoreId}", currentUserName, dto.StoreId);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == dto.StoreId && s.Active == true);
            if (store == null)
                return BadRequest(new { message = "Invalid or inactive store." });

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => dto.ProductIds.Contains(p.Id) && p.Active == true)
                .ToListAsync();

            if (products.Count != dto.ProductIds.Count)
            {
                var invalid = dto.ProductIds.Except(products.Select(p => p.Id)).ToList();
                return BadRequest(new { message = "Some products are invalid or inactive.", invalidProductIds = invalid });
            }

            var activeStatus = await _context.MasterTables
                .FirstOrDefaultAsync(m => m.TableName == "Status" && m.TableValue == "Active")
                ?? throw new Exception("Active status not configured.");

            var existingAssignments = await _context.StoreProductAssignments
                .Where(sp => sp.StoreId == dto.StoreId && dto.ProductIds.Contains(sp.ProductId))
                .ToListAsync();

            var toAdd = new List<StoreProductAssignment>();
            int reactivated = 0, skipped = 0;

            foreach (var productId in dto.ProductIds)
            {
                var existing = existingAssignments.FirstOrDefault(x => x.ProductId == productId);

                if (existing != null)
                {
                    if (existing.Active == true)
                    {
                        skipped++;
                        continue;
                    }
                    else
                    {
                        existing.Active = true;
                        existing.CanManage = dto.CanManage;
                        existing.StatusId = activeStatus.Id;
                        existing.ModifiedBy = currentUserName;
                        existing.ModifiedDate = GetIndianTime();
                        reactivated++;
                    }
                }
                else
                {
                    toAdd.Add(new StoreProductAssignment
                    {
                        Id = Guid.NewGuid().ToString(),
                        StoreId = dto.StoreId,
                        ProductId = productId,
                        CanManage = dto.CanManage,
                        StatusId = activeStatus.Id,
                        CreatedBy = currentUserName,
                        CreatedDate = GetIndianTime(),
                        ModifiedBy = currentUserName,
                        ModifiedDate = GetIndianTime(),
                        Active = true
                    });
                }
            }

            if (toAdd.Any())
                _context.StoreProductAssignments.AddRange(toAdd);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var productNames = products.Select(p => $"{p.ProductName} ({p.Category?.CategoryName})").ToList();

            _logger.LogInformation("Bulk assignments processed: {Added} new, {Reactivated} reactivated, {Skipped} skipped for Store: {StoreName}",
                toAdd.Count, reactivated, skipped, store.StoreName);

            return Ok(new
            {
                success = true,
                message = $"{toAdd.Count + reactivated} products assigned successfully ({reactivated} reactivated).",
                assignedCount = toAdd.Count + reactivated,
                skippedCount = skipped,
                assignedProducts = productNames
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "BulkCreateAssignments failed");
            return StatusCode(500, new { message = "Failed to create assignments.", error = ex.Message });
        }
    }

    // ===============================
    // ASSIGN PRODUCTS BY CATEGORY (with reactivation)
    // ===============================
    [HttpPost("by-category")]
    public async Task<IActionResult> AssignProductsByCategory([FromBody] CategoryProductAssignmentDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("AssignProductsByCategory called by: {UserName} for Store: {StoreId}, Category: {CategoryId}",
            currentUserName, dto.StoreId, dto.CategoryId);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == dto.StoreId && s.Active == true);
            if (store == null)
                return BadRequest(new { message = "Invalid or inactive store." });

            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.Active == true);
            if (category == null)
                return BadRequest(new { message = "Invalid or inactive category." });

            var activeStatus = await _context.MasterTables
                .FirstOrDefaultAsync(m => m.TableName == "Status" && m.TableValue == "Active")
                ?? throw new Exception("Active status not configured.");

            int assigned = 0, reactivated = 0;

            foreach (var productId in dto.ProductIds)
            {
                var existing = await _context.StoreProductAssignments
                    .FirstOrDefaultAsync(x => x.StoreId == dto.StoreId && x.ProductId == productId);

                if (existing != null)
                {
                    if (existing.Active == false)
                    {
                        existing.Active = true;
                        existing.CanManage = dto.CanManage;
                        existing.StatusId = activeStatus.Id;
                        existing.ModifiedBy = currentUserName;
                        existing.ModifiedDate = GetIndianTime();
                        reactivated++;
                    }
                }
                else
                {
                    _context.StoreProductAssignments.Add(new StoreProductAssignment
                    {
                        Id = Guid.NewGuid().ToString(),
                        StoreId = dto.StoreId,
                        ProductId = productId,
                        CanManage = dto.CanManage,
                        StatusId = activeStatus.Id,
                        CreatedBy = currentUserName,
                        CreatedDate = GetIndianTime(),
                        ModifiedBy = currentUserName,
                        ModifiedDate = GetIndianTime(),
                        Active = true
                    });
                    assigned++;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                success = true,
                message = $"Processed successfully. Assigned: {assigned}, Reactivated: {reactivated}",
                assignedCount = assigned,
                reactivatedCount = reactivated
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "AssignProductsByCategory failed");
            return StatusCode(500, new { message = "Failed to assign products.", error = ex.Message });
        }
    }

    // ===============================
    // UNASSIGN (Soft Delete) - SuperAdmin & StoreAdmin with CanManage
    // ===============================
    [HttpDelete("{id}")]
    public async Task<IActionResult> UnassignProduct(string id)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("UnassignProduct called by: {UserName}, AssignmentId: {AssignmentId}", currentUserName, id);

        try
        {
            var assignment = await _context.StoreProductAssignments
                .Include(sp => sp.Store)
                .Include(sp => sp.Product)
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.Active == true);

            if (assignment == null)
                return NotFound(new { message = "Assignment not found." });

            // Check permission if StoreAdmin
            if (User.IsInRole("StoreAdmin"))
            {
                var storeId = User.FindFirst("StoreId")?.Value;
                if (assignment.StoreId != storeId || assignment.CanManage != true)
                {
                    _logger.LogWarning("StoreAdmin {UserId} attempted unauthorized unassign of Assignment {AssignmentId}", currentUser?.Id, id);
                    return Forbid();
                }
            }

            // Soft delete
            assignment.Active = false;
            assignment.ModifiedDate = GetIndianTime();
            assignment.ModifiedBy = currentUserName;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Assignment unassigned: Store {StoreName} - Product {ProductName}",
                assignment.Store.StoreName, assignment.Product.ProductName);

            return Ok(new { success = true, message = "Product unassigned from store successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UnassignProduct failed for AssignmentId: {AssignmentId}", id);
            return StatusCode(500, new { message = "Failed to unassign product.", error = ex.Message });
        }
    }

    // ===============================
    // GET STORE PRODUCTS (StoreAdmin View)
    // ===============================
    [HttpGet("store-products")]
    [Authorize(Roles = "StoreAdmin")]
    public async Task<IActionResult> GetStoreProducts()
    {
        try
        {
            var storeId = User.FindFirst("StoreId")?.Value;
            if (string.IsNullOrEmpty(storeId))
                return BadRequest(new { message = "Store ID not found in token." });

            var assignments = await _context.StoreProductAssignments
                .Where(x => x.StoreId == storeId && x.Active == true)
                .Include(x => x.Product)
                    .ThenInclude(p => p.Category)
                .Select(x => new
                {
                    x.Product.Id,
                    x.Product.ProductName,
                    x.Product.ProductCode,
                    Category = x.Product.Category.CategoryName ?? "Uncategorized",
                    x.CanManage,
                    ProductImage = x.Product.ProductImage,
                    ProductImageUrl = !string.IsNullOrEmpty(x.Product.ProductImage) ? GetFullUrl(x.Product.ProductImage) : null
                })
                .ToListAsync();

            return Ok(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStoreProducts failed");
            return StatusCode(500, new { message = "Failed to retrieve store products.", error = ex.Message });
        }
    }

    // ===============================
    // GET ALL ASSIGNMENTS (SuperAdmin only)
    // ===============================
    [HttpGet]
    public async Task<IActionResult> GetAllAssignments([FromQuery] string? storeId, [FromQuery] string? productId)
    {
        _logger.LogInformation("GetAllAssignments called by: {UserName}", User.FindFirstValue("FullName") ?? "Unknown");

        try
        {
            var query = _context.StoreProductAssignments
                .Include(sp => sp.Store)
                .Include(sp => sp.Product)
                    .ThenInclude(p => p.Category)
                .Include(sp => sp.Status)
                .Where(sp => sp.Active == true)
                .AsQueryable();

            if (!string.IsNullOrEmpty(storeId))
                query = query.Where(sp => sp.StoreId == storeId);

            if (!string.IsNullOrEmpty(productId))
                query = query.Where(sp => sp.ProductId == productId);

            var assignments = await query
                .OrderByDescending(sp => sp.CreatedDate)
                .Select(sp => new
                {
                    sp.Id,
                    StoreName = sp.Store.StoreName,
                    ProductName = sp.Product.ProductName,
                    ProductCode = sp.Product.ProductCode,
                    CategoryName = sp.Product.Category.CategoryName,
                    sp.CanManage,
                    Status = sp.Status.TableValue,
                    StatusId = sp.StatusId,
                    sp.CreatedDate
                })
                .ToListAsync();

            var response = assignments.Select(a => new AssignmentListResponseDto
            {
                Id = a.Id,
                StoreName = a.StoreName,
                ProductName = a.ProductName,
                ProductCode = a.ProductCode,
                CategoryName = a.CategoryName ?? "Uncategorized",
                CanManage = a.CanManage ?? false,
                Status = a.Status,
                StatusId = a.StatusId,
                StatusColor = GetStatusColor(a.Status),
                CreatedDate = a.CreatedDate
            }).ToList();

            return Ok(new { success = true, data = response, count = response.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAssignments failed");
            return StatusCode(500, new { message = "Failed to retrieve assignments.", error = ex.Message });
        }
    }

    // ===============================
    // GET PRODUCTS BY CATEGORY FOR ASSIGNMENT (SuperAdmin)
    // ===============================
    [HttpGet("products-by-category/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory(string categoryId, [FromQuery] string? storeId)
    {
        _logger.LogInformation("GetProductsByCategory called for CategoryId: {CategoryId}, StoreId: {StoreId}", categoryId, storeId ?? "all");

        try
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.Active == true);
            if (category == null)
                return BadRequest(new { message = "Invalid or inactive category." });

            var productsQuery = _context.Products
                .Include(p => p.Status)
                .Where(p => p.CategoryId == categoryId && p.Active == true && p.Status.TableValue == "Active");

            List<string> assignedProductIds = new();
            if (!string.IsNullOrEmpty(storeId))
            {
                assignedProductIds = await _context.StoreProductAssignments
                    .Where(sp => sp.StoreId == storeId && sp.Active == true)
                    .Select(sp => sp.ProductId)
                    .ToListAsync();
            }

            var products = await productsQuery
                .OrderBy(p => p.ProductName)
                .Select(p => new
                {
                    p.Id,
                    p.ProductCode,
                    p.ProductName,
                    p.BrandName,
                    p.IsFeatured,
                    p.ProductImage,
                    IsAssigned = assignedProductIds.Contains(p.Id)
                })
                .ToListAsync();

            var response = products.Select(p => new
            {
                p.Id,
                p.ProductCode,
                p.ProductName,
                p.BrandName,
                p.IsFeatured,
                ProductImageUrl = !string.IsNullOrEmpty(p.ProductImage) ? GetFullUrl(p.ProductImage) : null,
                p.IsAssigned
            }).ToList();

            return Ok(new
            {
                success = true,
                categoryName = category.CategoryName,
                data = response,
                totalCount = response.Count,
                assignedCount = response.Count(p => p.IsAssigned)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProductsByCategory failed for CategoryId: {CategoryId}", categoryId);
            return StatusCode(500, new { message = "Failed to retrieve products.", error = ex.Message });
        }
    }

    // ===============================
    // GET AVAILABLE PRODUCTS FOR STORE (Grouped by Category)
    // ===============================
    [HttpGet("available-products/{storeId}")]
    public async Task<IActionResult> GetAvailableProductsForStore(string storeId, [FromQuery] string? categoryId)
    {
        _logger.LogInformation("GetAvailableProductsForStore called for StoreId: {StoreId}", storeId);

        try
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == storeId && s.Active == true);
            if (store == null)
                return BadRequest(new { message = "Invalid or inactive store." });

            var assignedProductIds = await _context.StoreProductAssignments
                .Where(sp => sp.StoreId == storeId && sp.Active == true)
                .Select(sp => sp.ProductId)
                .ToListAsync();

            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Active == true && p.Status.TableValue == "Active")
                .AsQueryable();

            if (!string.IsNullOrEmpty(categoryId))
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);

            var products = await productsQuery
                .OrderBy(p => p.Category.CategoryName)
                .ThenBy(p => p.ProductName)
                .Select(p => new AvailableProductDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.CategoryName,
                    BrandName = p.BrandName,
                    IsFeatured = p.IsFeatured ?? false,
                    IsAssigned = assignedProductIds.Contains(p.Id)
                })
                .ToListAsync();

            var grouped = products
                .GroupBy(p => new { p.CategoryId, p.CategoryName })
                .Select(g => new
                {
                    categoryId = g.Key.CategoryId,
                    categoryName = g.Key.CategoryName,
                    products = g.ToList()
                })
                .ToList();

            return Ok(new
            {
                success = true,
                data = grouped,
                assignedCount = assignedProductIds.Count,
                totalAvailable = products.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAvailableProductsForStore failed for StoreId: {StoreId}", storeId);
            return StatusCode(500, new { message = "Failed to retrieve available products.", error = ex.Message });
        }
    }

    // ===============================
    // GET ASSIGNMENT BY ID
    // ===============================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssignmentById(string id)
    {
        _logger.LogInformation("GetAssignmentById called for AssignmentId: {AssignmentId}", id);

        try
        {
            var assignment = await _context.StoreProductAssignments
                .Include(sp => sp.Store)
                .Include(sp => sp.Product)
                    .ThenInclude(p => p.Category)
                .Include(sp => sp.Status)
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.Active == true);

            if (assignment == null)
                return NotFound(new { message = "Assignment not found." });

            var response = new AssignmentResponseDto
            {
                Id = assignment.Id,
                StoreId = assignment.StoreId,
                StoreName = assignment.Store.StoreName,
                ProductId = assignment.ProductId,
                ProductName = assignment.Product.ProductName,
                ProductCode = assignment.Product.ProductCode,
                CategoryId = assignment.Product.CategoryId,
                CategoryName = assignment.Product.Category?.CategoryName ?? "Unknown",
                CanManage = assignment.CanManage ?? false,
                Status = assignment.Status.TableValue,
                StatusId = assignment.StatusId,
                StatusColor = GetStatusColor(assignment.Status.TableValue),
                CreatedBy = assignment.CreatedBy,
                CreatedDate = assignment.CreatedDate,
                ModifiedBy = assignment.ModifiedBy ?? "Not modified",
                ModifiedDate = assignment.ModifiedDate,
                IsActive = assignment.Active ?? false
            };

            return Ok(new { success = true, data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAssignmentById failed for AssignmentId: {AssignmentId}", id);
            return StatusCode(500, new { message = "Failed to retrieve assignment.", error = ex.Message });
        }
    }

    // ===============================
    // GET ASSIGNMENTS BY STORE (SuperAdmin) – for quick overview
    // ===============================
    [HttpGet("store/{storeId}")]
    public async Task<IActionResult> GetAssignmentsByStore(string storeId)
    {
        _logger.LogInformation("GetAssignmentsByStore called for StoreId: {StoreId}", storeId);

        try
        {
            var assignments = await _context.StoreProductAssignments
                .Include(sp => sp.Product)
                    .ThenInclude(p => p.Category)
                .Include(sp => sp.Status)
                .Where(sp => sp.StoreId == storeId && sp.Active == true)
                .OrderByDescending(sp => sp.CreatedDate)
                .Select(sp => new
                {
                    sp.Id,
                    sp.ProductId,
                    ProductName = sp.Product.ProductName,
                    ProductCode = sp.Product.ProductCode,
                    ProductImage = sp.Product.ProductImage,
                    CategoryName = sp.Product.Category.CategoryName,
                    sp.CanManage,
                    Status = sp.Status.TableValue,
                    StatusId = sp.StatusId
                })
                .ToListAsync();

            var response = assignments.Select(a => new
            {
                a.Id,
                a.ProductId,
                a.ProductName,
                a.ProductCode,
                ProductImageUrl = !string.IsNullOrEmpty(a.ProductImage) ? GetFullUrl(a.ProductImage) : null,
                a.CategoryName,
                a.CanManage,
                a.Status,
                a.StatusId,
                StatusColor = GetStatusColor(a.Status)
            }).ToList();

            return Ok(new { success = true, storeId, data = response, count = response.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAssignmentsByStore failed for StoreId: {StoreId}", storeId);
            return StatusCode(500, new { message = "Failed to retrieve assignments.", error = ex.Message });
        }
    }

    // ===============================
    // UPDATE ASSIGNMENT (SuperAdmin only – can change CanManage or Status)
    // ===============================
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssignment(string id, [FromBody] AssignmentUpdateDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("UpdateAssignment called by: {UserName}, AssignmentId: {AssignmentId}", currentUserName, id);

        try
        {
            var assignment = await _context.StoreProductAssignments
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.Active == true);

            if (assignment == null)
                return NotFound(new { message = "Assignment not found." });

            bool hasChanges = false;

            if (dto.CanManage.HasValue && dto.CanManage != assignment.CanManage)
            {
                assignment.CanManage = dto.CanManage;
                hasChanges = true;
            }

            if (dto.StatusId.HasValue && dto.StatusId.Value != assignment.StatusId)
            {
                var statusExists = await _context.MasterTables
                    .AnyAsync(m => m.Id == dto.StatusId.Value && m.TableName == "Status" && m.Active == true);
                if (!statusExists)
                    return BadRequest(new { message = "Invalid status selected." });

                assignment.StatusId = dto.StatusId.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                assignment.ModifiedDate = GetIndianTime();
                assignment.ModifiedBy = currentUserName;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Assignment updated successfully: {AssignmentId} by {UserName}", id, currentUserName);
            }

            return await GetAssignmentById(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAssignment failed for AssignmentId: {AssignmentId}", id);
            return StatusCode(500, new { message = "Failed to update assignment.", error = ex.Message });
        }
    }

    // ===============================
    // DELETE ASSIGNMENTS BY STORE (Soft Delete)
    // ===============================
    [HttpDelete("store/{storeId}")]
    public async Task<IActionResult> DeleteAssignmentsByStore(string storeId)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("DeleteAssignmentsByStore called by: {UserName}, StoreId: {StoreId}", currentUserName, storeId);

        try
        {
            var assignments = await _context.StoreProductAssignments
                .Where(sp => sp.StoreId == storeId && sp.Active == true)
                .ToListAsync();

            if (!assignments.Any())
                return NotFound(new { message = "No active assignments found for this store." });

            foreach (var a in assignments)
            {
                a.Active = false;
                a.ModifiedDate = GetIndianTime();
                a.ModifiedBy = currentUserName;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("All assignments deleted for Store: {StoreId} by {UserName}", storeId, currentUserName);

            return Ok(new { success = true, message = $"All {assignments.Count} assignments deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAssignmentsByStore failed for StoreId: {StoreId}", storeId);
            return StatusCode(500, new { message = "Failed to delete assignments.", error = ex.Message });
        }
    }

    // ===============================
    // GET ASSIGNMENT STATISTICS (SuperAdmin only)
    // ===============================
    [HttpGet("statistics")]
    public async Task<IActionResult> GetAssignmentStatistics()
    {
        _logger.LogInformation("GetAssignmentStatistics called by: {UserName}", User.FindFirstValue("FullName") ?? "Unknown");

        try
        {
            var totalAssignments = await _context.StoreProductAssignments
                .CountAsync(sp => sp.Active == true);

            var storeStats = await _context.StoreProductAssignments
                .Include(sp => sp.Store)
                .Where(sp => sp.Active == true)
                .GroupBy(sp => new { sp.StoreId, sp.Store.StoreName })
                .Select(g => new
                {
                    storeId = g.Key.StoreId,
                    storeName = g.Key.StoreName,
                    productCount = g.Count()
                })
                .OrderByDescending(x => x.productCount)
                .Take(10)
                .ToListAsync();

            var categoryStats = await _context.StoreProductAssignments
                .Include(sp => sp.Product)
                    .ThenInclude(p => p.Category)
                .Where(sp => sp.Active == true && sp.Product.Category != null)
                .GroupBy(sp => new { sp.Product.CategoryId, sp.Product.Category.CategoryName })
                .Select(g => new
                {
                    categoryId = g.Key.CategoryId,
                    categoryName = g.Key.CategoryName,
                    assignmentCount = g.Count(),
                    storeCount = g.Select(x => x.StoreId).Distinct().Count()
                })
                .OrderByDescending(x => x.assignmentCount)
                .Take(10)
                .ToListAsync();

            var productStats = await _context.StoreProductAssignments
                .Include(sp => sp.Product)
                .Where(sp => sp.Active == true)
                .GroupBy(sp => new { sp.ProductId, sp.Product.ProductName })
                .Select(g => new
                {
                    productId = g.Key.ProductId,
                    productName = g.Key.ProductName,
                    storeCount = g.Count()
                })
                .OrderByDescending(x => x.storeCount)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    totalAssignments,
                    topStores = storeStats,
                    topCategories = categoryStats,
                    topProducts = productStats
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAssignmentStatistics failed");
            return StatusCode(500, new { message = "Failed to retrieve statistics.", error = ex.Message });
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