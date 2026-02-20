using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreCare.Server.Data;
using StoreCare.Server.Models;
using StoreCare.Server.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace StoreCare.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly StoreCareDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;
    private readonly IFileService _fileService;

    public AuthController(
        StoreCareDbContext context,
        IConfiguration config,
        ILogger<AuthController> logger,
        IFileService fileService)
    {
        _context = context;
        _config = config;
        _logger = logger;
        _fileService = fileService;
    }

    // ===============================
    // DTOs (unchanged)
    // ===============================
    public class RegisterRequestDto
    {
        [Required, MaxLength(200)] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress, MaxLength(100)] public string Email { get; set; } = string.Empty;
        [Required, RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be 10 digits.")] public string Phone { get; set; } = string.Empty;
        [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
        [Required] public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginRequestDto
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required] public string CurrentPassword { get; set; } = string.Empty;
        [Required, MinLength(8)] public string NewPassword { get; set; } = string.Empty;
        [Required] public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        [MaxLength(200)]
        public string? FullName { get; set; }

        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be 10 digits.")]
        public string? Phone { get; set; }
    }

    public class UploadProfilePictureDto
    {
        [Required]
        public IFormFile ProfileImage { get; set; } = null!;
    }

    public class UserProfileResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public bool Active { get; set; }
        public string? StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    // ===============================
    // 1. REGISTER (Public - Customer only)
    // ===============================
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        _logger.LogInformation("Register attempt for email: {Email}", dto.Email);

        try
        {
            dto.Email = dto.Email.Trim().ToLower();
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                _logger.LogWarning("Registration failed - Email already exists: {Email}", dto.Email);
                return BadRequest(new { message = "Email is already registered." });
            }

            var roleId = await GetMasterId("Role", "Customer");
            var statusId = await GetMasterId("Status", "Active");

            var userId = Guid.NewGuid().ToString();
            var userCode = "CUST-" + Guid.NewGuid().ToString()[..8].ToUpper();
            var fullName = dto.FullName.Trim();

            var user = new User
            {
                Id = userId,
                UserCode = userCode,
                FullName = fullName,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = roleId,
                StatusId = statusId,
                CreatedBy = "System",
                CreatedDate = GetIndianTime(),
                ModifiedBy = fullName,
                ModifiedDate = GetIndianTime(),
                Active = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {Email}, Role: Customer, UserId: {UserId}", dto.Email, user.Id);
            return Ok(new { message = "Registration successful. Please login." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for email: {Email}", dto.Email);
            return BadRequest(new { message = "Registration failed.", error = ex.Message });
        }
    }

    // ===============================
    // 2. LOGIN (Public)
    // ===============================
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        // ... (unchanged, already returns profilePictureUrl)
        _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

        try
        {
            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Status)
                .FirstOrDefaultAsync(u => u.Email == email && u.Active == true);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed - Invalid credentials for email: {Email}", dto.Email);
                await LogLoginAttempt(null, "Failed", "Invalid credentials");
                return Unauthorized(new { message = "Invalid email or password." });
            }

            if (user.Status?.TableValue == "Suspended")
            {
                _logger.LogWarning("Login failed - Account suspended: {Email}", dto.Email);
                return BadRequest(new { message = "Your account is suspended. Contact support." });
            }

            var token = GenerateJwtToken(user);

            user.LastLogin = GetIndianTime();
            await _context.SaveChangesAsync();

            await LogLoginAttempt(user.Id, "Success", null);

            var profilePictureUrl = !string.IsNullOrEmpty(user.ProfilePicture)
                ? _fileService.GetProfileImageUrl(user.ProfilePicture)
                : null;

            _logger.LogInformation("Login successful: {Email}, Role: {Role}, UserId: {UserId}", dto.Email, user.Role.TableValue, user.Id);

            return Ok(new
            {
                token,
                role = user.Role.TableValue,
                fullName = user.FullName,
                email = user.Email,
                storeId = user.StoreId,
                profilePicture = profilePictureUrl,
                message = "Login successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for email: {Email}", dto.Email);
            return BadRequest(new { message = "Login failed.", error = ex.Message });
        }
    }

    // ===============================
    // 3. WHO AM I (Authenticated)
    // ===============================
    [HttpGet("whoami")]
    public async Task<IActionResult> WhoAmI()
    {
        // ... (unchanged, already returns profilePicture)
        try
        {
            var userId = GetUserIdFromToken();
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var profilePictureUrl = user?.ProfilePicture != null
                ? _fileService.GetProfileImageUrl(user.ProfilePicture)
                : null;

            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            _logger.LogInformation("WhoAmI called by UserId: {UserId}, Role: {Role}", userId, user?.Role?.TableValue);

            return Ok(new
            {
                userId,
                email = User.FindFirstValue(ClaimTypes.Email),
                role = User.FindFirstValue(ClaimTypes.Role),
                storeId = User.FindFirstValue("StoreId"),
                profilePicture = profilePictureUrl,
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                authenticationType = User.Identity?.AuthenticationType,
                claims
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WhoAmI failed");
            return BadRequest(new { message = "Failed to get user info.", error = ex.Message });
        }
    }

    // ===============================
    // 4. LOGOUT (Authenticated)
    // ===============================
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // ... (unchanged)
        var userId = GetUserIdFromToken();
        _logger.LogInformation("Logout attempt for UserId: {UserId}", userId);

        try
        {
            var lastLogin = await _context.LoginHistories
                .Where(l => l.UserId == userId && l.Status == "Success")
                .OrderByDescending(l => l.LoginTime)
                .FirstOrDefaultAsync();

            if (lastLogin != null)
            {
                lastLogin.LogoutTime = GetIndianTime();
                await _context.SaveChangesAsync();
                _logger.LogInformation("Logout successful for UserId: {UserId}", userId);
            }

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed for UserId: {UserId}", userId);
            return BadRequest(new { message = "Logout failed.", error = ex.Message });
        }
    }

    // ===============================
    // 5. GET PROFILE (Authenticated - Own profile)
    // ===============================
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // ... (unchanged, already returns profilePictureUrl)
        var userId = GetUserIdFromToken();
        _logger.LogInformation("GetProfile called for UserId: {UserId}", userId);

        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Status)
                .Include(u => u.Store)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("GetProfile failed - User not found: {UserId}", userId);
                return NotFound(new { message = "User not found." });
            }

            var profilePictureUrl = !string.IsNullOrEmpty(user.ProfilePicture)
                ? _fileService.GetProfileImageUrl(user.ProfilePicture)
                : null;

            var response = new UserProfileResponseDto
            {
                Id = user.Id,
                UserCode = user.UserCode,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone ?? "",
                Role = user.Role?.TableValue,
                Status = user.Status?.TableValue ?? "Active",
                Active = user.Active ?? false,
                StoreId = user.StoreId,
                StoreName = user.Store?.StoreName,
                ProfilePicture = user.ProfilePicture,
                ProfilePictureUrl = profilePictureUrl,
                CreatedBy = user.CreatedBy ?? "System",
                CreatedDate = user.CreatedDate,
                ModifiedBy = user.ModifiedBy ?? "System",
                ModifiedDate = user.ModifiedDate,
                LastLogin = user.LastLogin
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProfile failed for UserId: {UserId}", userId);
            return BadRequest(new { message = "Failed to get profile.", error = ex.Message });
        }
    }

    // ===============================
    // 6. UPDATE PROFILE (PUT - Full Update)
    // ===============================
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        // ... (unchanged)
        var userId = GetUserIdFromToken();
        _logger.LogInformation("UpdateProfile called for UserId: {UserId}", userId);

        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("UpdateProfile failed - User not found: {UserId}", userId);
                return NotFound(new { message = "User not found." });
            }

            bool hasChanges = false;

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Phone, @"^[0-9]{10}$"))
                    return BadRequest(new { message = "Phone must be 10 digits." });

                user.Phone = dto.Phone.Trim();
                hasChanges = true;
            }

            if (hasChanges)
            {
                user.ModifiedDate = GetIndianTime();
                user.ModifiedBy = user.FullName;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Profile updated successfully for UserId: {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("No changes detected for UserId: {UserId}", userId);
                return Ok(new { message = "No changes detected." });
            }

            return await GetProfile();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateProfile failed for UserId: {UserId}", userId);
            return BadRequest(new { message = "Failed to update profile.", error = ex.Message });
        }
    }

    // ===============================
    // 7. UPLOAD PROFILE PICTURE
    // ===============================
    [HttpPost("profile-picture")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfilePicture([FromForm] UploadProfilePictureDto dto)
    {
        // ... (unchanged)
        var userId = GetUserIdFromToken();
        _logger.LogInformation("UploadProfilePicture called for UserId: {UserId}", userId);

        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("UploadProfilePicture failed - User not found: {UserId}", userId);
                return NotFound(new { message = "User not found." });
            }

            if (dto.ProfileImage == null || dto.ProfileImage.Length == 0)
                return BadRequest(new { message = "No image file provided." });

            _fileService.DeleteProfileImage(user.ProfilePicture);

            var imagePath = await _fileService.SaveProfileImageAsync(dto.ProfileImage, user.Id);
            user.ProfilePicture = imagePath;
            user.ModifiedDate = GetIndianTime();
            user.ModifiedBy = user.FullName;
            await _context.SaveChangesAsync();

            var profilePictureUrl = _fileService.GetProfileImageUrl(user.ProfilePicture);

            _logger.LogInformation("Profile picture uploaded successfully for UserId: {UserId}", userId);

            return Ok(new
            {
                message = "Profile picture uploaded successfully.",
                profilePicture = user.ProfilePicture,
                profilePictureUrl = profilePictureUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload profile picture for UserId: {UserId}", userId);
            return BadRequest(new { message = "Failed to upload profile picture. Please try again." });
        }
    }

    // ===============================
    // 8. GET PROFILE PICTURE
    // ===============================
    [HttpGet("profile-picture/{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProfilePicture(string userId)
    {
        // ... (unchanged)
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.ProfilePicture))
                return NotFound();

            var fileName = Path.GetFileName(user.ProfilePicture);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProfileImg", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = GetContentType(fileName);

            return File(fileBytes, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get profile picture for UserId: {UserId}", userId);
            return NotFound();
        }
    }

    // ===============================
    // 9. DELETE PROFILE PICTURE
    // ===============================
    [HttpDelete("profile-picture")]
    public async Task<IActionResult> DeleteProfilePicture()
    {
        // ... (unchanged)
        var userId = GetUserIdFromToken();
        _logger.LogInformation("DeleteProfilePicture called for UserId: {UserId}", userId);

        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                _fileService.DeleteProfileImage(user.ProfilePicture);
                user.ProfilePicture = null;
                user.ModifiedDate = GetIndianTime();
                user.ModifiedBy = user.FullName;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Profile picture deleted for UserId: {UserId}", userId);
            }

            return Ok(new { message = "Profile picture deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteProfilePicture failed for UserId: {UserId}", userId);
            return BadRequest(new { message = "Failed to delete profile picture.", error = ex.Message });
        }
    }

    // ===============================
    // 10. CHANGE PASSWORD (Own account)
    // ===============================
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        // ... (unchanged)
        var userId = GetUserIdFromToken();
        _logger.LogInformation("ChangePassword called for UserId: {UserId}", userId);

        try
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                return BadRequest(new { message = "New passwords do not match." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("ChangePassword failed - User not found: {UserId}", userId);
                return NotFound(new { message = "User not found." });
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("ChangePassword failed - Incorrect current password for UserId: {UserId}", userId);
                return BadRequest(new { message = "Current password is incorrect." });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ModifiedDate = GetIndianTime();
            user.ModifiedBy = user.FullName;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password changed successfully for UserId: {UserId}", userId);
            return Ok(new { message = "Password changed successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangePassword failed for UserId: {UserId}", userId);
            return BadRequest(new { message = "Failed to change password.", error = ex.Message });
        }
    }

    // ===============================
    // 11. CREATE STOREADMIN (SuperAdmin only)
    // ===============================
    [HttpPost("storeadmin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateStoreAdmin([FromBody] RegisterRequestDto dto)
    {
        // ... (unchanged, already creates StoreAdmin and store)
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("CreateStoreAdmin called by: {UserName}", currentUserName);

        try
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            dto.Email = dto.Email.Trim().ToLower();
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                _logger.LogWarning("CreateStoreAdmin failed - Email already exists: {Email}", dto.Email);
                return BadRequest(new { message = "Email is already registered." });
            }

            var roleId = await GetMasterId("Role", "StoreAdmin");
            var statusId = await GetMasterId("Status", "Active");

            var storeId = Guid.NewGuid().ToString();
            var storeCode = "ST-" + Guid.NewGuid().ToString()[..5].ToUpper();
            var storeName = $"{dto.FullName.Trim()}'s Store";

            var store = new Store
            {
                Id = storeId,
                StoreCode = storeCode,
                StoreName = storeName,
                StatusId = statusId,
                CreatedDate = GetIndianTime(),
                CreatedBy = currentUserName,
                ModifiedBy = currentUserName,
                ModifiedDate = GetIndianTime(),
                Active = true
            };
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            var userId = Guid.NewGuid().ToString();
            var userCode = "SADM-" + Guid.NewGuid().ToString()[..8].ToUpper();
            var fullName = dto.FullName.Trim();

            var user = new User
            {
                Id = userId,
                UserCode = userCode,
                FullName = fullName,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = roleId,
                StoreId = storeId,
                StatusId = statusId,
                CreatedBy = currentUserName,
                CreatedDate = GetIndianTime(),
                ModifiedBy = currentUserName,
                ModifiedDate = GetIndianTime(),
                Active = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("StoreAdmin created successfully: {Email}, Store: {StoreName}, UserId: {UserId}",
                dto.Email, store.StoreName, user.Id);

            return Ok(new
            {
                message = "StoreAdmin created successfully.",
                storeId = store.Id,
                storeName = store.StoreName,
                userId = user.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateStoreAdmin failed for email: {Email}", dto.Email);
            return BadRequest(new { message = "Failed to create StoreAdmin.", error = ex.Message });
        }
    }

    // ===============================
    // 12. GET ALL USERS (SuperAdmin + StoreAdmin)
    // ===============================
    [HttpGet("users")]
    [Authorize(Roles = "SuperAdmin,StoreAdmin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var currentUserId = GetUserIdFromToken();
        _logger.LogInformation("GetAllUsers called by UserId: {UserId}", currentUserId);

        try
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Status)
                .Include(u => u.Store)
                .AsQueryable();

            // Role-based filtering
            if (User.IsInRole("StoreAdmin"))
            {
                var storeId = User.FindFirst("StoreId")?.Value;
                if (string.IsNullOrEmpty(storeId))
                {
                    _logger.LogWarning("StoreAdmin missing StoreId claim: {UserId}", currentUserId);
                    return Forbid();
                }

                // StoreAdmin sees only customers of their own store
                query = query.Where(u => u.StoreId == storeId && u.Role != null && u.Role.TableValue == "Customer");
            }
            // SuperAdmin sees all users (no extra filter)

            var users = await query
                .Select(u => new
                {
                    u.Id,
                    u.UserCode,
                    u.FullName,
                    u.Email,
                    u.Phone,
                    Role = u.Role != null ? u.Role.TableValue : "Unknown",
                    StoreName = u.Store != null ? u.Store.StoreName : null,
                    u.StoreId,
                    Status = u.Status != null ? u.Status.TableValue : "Unknown",
                    u.Active,
                    u.CreatedBy,
                    u.CreatedDate,
                    u.ModifiedBy,
                    u.ModifiedDate,
                    u.LastLogin,
                    ProfilePicture = !string.IsNullOrEmpty(u.ProfilePicture)
                        ? _fileService.GetProfileImageUrl(u.ProfilePicture)
                        : null
                })
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();

            _logger.LogInformation("GetAllUsers returned {Count} users", users.Count);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllUsers failed for UserId: {UserId}", currentUserId);
            return BadRequest(new { message = "Failed to get users.", error = ex.Message });
        }
    }

    // ===============================
    // 13. GET USER BY ID (SuperAdmin + StoreAdmin)
    // ===============================
    [HttpGet("users/{id}")]
    [Authorize(Roles = "SuperAdmin,StoreAdmin")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var currentUserId = GetUserIdFromToken();
        _logger.LogInformation("GetUserById called by UserId: {UserId}, TargetUserId: {TargetId}", currentUserId, id);

        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Status)
                .Include(u => u.Store)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                _logger.LogWarning("GetUserById failed - User not found: {TargetId}", id);
                return NotFound(new { message = "User not found." });
            }

            // Role-based authorization
            if (User.IsInRole("StoreAdmin"))
            {
                var storeId = User.FindFirst("StoreId")?.Value;
                if (user.StoreId != storeId || user.Role?.TableValue != "Customer")
                {
                    _logger.LogWarning("StoreAdmin {UserId} attempted to access user outside their store: {TargetId}", currentUserId, id);
                    return Forbid();
                }
            }

            var response = new
            {
                user.Id,
                user.UserCode,
                user.FullName,
                user.Email,
                user.Phone,
                Role = user.Role?.TableValue,
                StoreName = user.Store?.StoreName,
                user.StoreId,
                Status = user.Status?.TableValue,
                user.Active,
                user.CreatedBy,
                user.CreatedDate,
                user.ModifiedBy,
                user.ModifiedDate,
                user.LastLogin,
                ProfilePicture = !string.IsNullOrEmpty(user.ProfilePicture)
                    ? _fileService.GetProfileImageUrl(user.ProfilePicture)
                    : null
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserById failed for TargetUserId: {TargetId}", id);
            return BadRequest(new { message = "Failed to get user.", error = ex.Message });
        }
    }

    // ===============================
    // 14. UPDATE USER (SuperAdmin + StoreAdmin)
    // ===============================
    [HttpPut("users/{id}")]
    [Authorize(Roles = "SuperAdmin,StoreAdmin")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateProfileDto dto)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserName = currentUser?.FullName ?? "System";
        var currentUserId = currentUser?.Id;

        _logger.LogInformation("UpdateUser called by: {UserName}, TargetUserId: {TargetId}", currentUserName, id);

        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                _logger.LogWarning("UpdateUser failed - User not found: {TargetId}", id);
                return NotFound(new { message = "User not found." });
            }

            // Role-based authorization
            if (User.IsInRole("StoreAdmin"))
            {
                var storeId = User.FindFirst("StoreId")?.Value;
                if (user.StoreId != storeId || user.Role?.TableValue != "Customer")
                {
                    _logger.LogWarning("StoreAdmin {UserId} attempted to update user outside their store: {TargetId}", currentUserId, id);
                    return Forbid();
                }
            }

            bool hasChanges = false;

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Phone, @"^[0-9]{10}$"))
                    return BadRequest(new { message = "Phone must be 10 digits." });

                user.Phone = dto.Phone.Trim();
                hasChanges = true;
            }

            if (hasChanges)
            {
                user.ModifiedDate = GetIndianTime();
                user.ModifiedBy = currentUserName;
                await _context.SaveChangesAsync();
                _logger.LogInformation("User updated successfully: {TargetId}", id);
            }

            return Ok(new { message = "User updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateUser failed for TargetUserId: {TargetId}", id);
            return BadRequest(new { message = "Failed to update user.", error = ex.Message });
        }
    }

    // ===============================
    // 15. TOGGLE USER STATUS (SuperAdmin + StoreAdmin)
    // ===============================
    [HttpPatch("users/{id}/toggle-status")]
    [Authorize(Roles = "SuperAdmin,StoreAdmin")]
    public async Task<IActionResult> ToggleUserStatus(string id, [FromBody] bool active)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserId = currentUser?.Id;
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("ToggleUserStatus called by: {UserName}, TargetUserId: {TargetId}, SetActive: {Active}",
            currentUserName, id, active);

        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                _logger.LogWarning("ToggleUserStatus failed - User not found: {TargetId}", id);
                return NotFound(new { message = "User not found." });
            }

            // Role-based authorization
            if (User.IsInRole("StoreAdmin"))
            {
                var storeId = User.FindFirst("StoreId")?.Value;
                if (user.StoreId != storeId || user.Role?.TableValue != "Customer")
                {
                    _logger.LogWarning("StoreAdmin {UserId} attempted to toggle status of user outside their store: {TargetId}", currentUserId, id);
                    return Forbid();
                }
            }

            // Prevent SuperAdmin from deactivating themselves
            if (id == currentUserId && !active && User.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("SuperAdmin attempted to deactivate own account: {UserId}", currentUserId);
                return BadRequest(new { message = "You cannot deactivate your own account." });
            }

            user.Active = active;
            user.ModifiedDate = GetIndianTime();
            user.ModifiedBy = currentUserName;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User status toggled successfully: {TargetId}, Active: {Active}", id, active);

            return Ok(new
            {
                message = $"User {(active ? "activated" : "deactivated")} successfully.",
                active = user.Active
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ToggleUserStatus failed for TargetUserId: {TargetId}", id);
            return BadRequest(new { message = "Failed to toggle user status.", error = ex.Message });
        }
    }

    // ===============================
    // 16. DELETE USER (SuperAdmin + StoreAdmin) - Soft Delete
    // ===============================
    [HttpDelete("users/{id}")]
    [Authorize(Roles = "SuperAdmin,StoreAdmin")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var currentUser = await _context.Users.FindAsync(GetUserIdFromToken());
        var currentUserId = currentUser?.Id;
        var currentUserName = currentUser?.FullName ?? "System";

        _logger.LogInformation("DeleteUser called by: {UserName}, TargetUserId: {TargetId}", currentUserName, id);

        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                _logger.LogWarning("DeleteUser failed - User not found: {TargetId}", id);
                return NotFound(new { message = "User not found." });
            }

            // Role-based authorization
            if (User.IsInRole("StoreAdmin"))
            {
                var storeId = User.FindFirst("StoreId")?.Value;
                if (user.StoreId != storeId || user.Role?.TableValue != "Customer")
                {
                    _logger.LogWarning("StoreAdmin {UserId} attempted to delete user outside their store: {TargetId}", currentUserId, id);
                    return Forbid();
                }
            }

            // Prevent SuperAdmin from deleting themselves
            if (id == currentUserId)
            {
                _logger.LogWarning("SuperAdmin attempted to delete own account: {UserId}", currentUserId);
                return BadRequest(new { message = "You cannot delete your own account." });
            }

            // Soft delete
            user.Active = false;
            user.ModifiedDate = GetIndianTime();
            user.ModifiedBy = currentUserName;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User deleted successfully: {TargetId}", id);
            return Ok(new { message = "User deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteUser failed for TargetUserId: {TargetId}", id);
            return BadRequest(new { message = "Failed to delete user.", error = ex.Message });
        }
    }

    // ===============================
    // 17. LOGIN HISTORY (SuperAdmin only) & My Login History (all users)
    // ===============================
    [HttpGet("login-history")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetLoginHistory([FromQuery] string? userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        // ... (unchanged)
        var currentUserId = GetUserIdFromToken();
        _logger.LogInformation("GetLoginHistory called by UserId: {UserId}", currentUserId);

        try
        {
            var query = _context.LoginHistories
                .Include(l => l.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(l => l.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(l => l.LoginTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.LoginTime <= toDate.Value);

            var history = await query
                .OrderByDescending(l => l.LoginTime)
                .Select(l => new
                {
                    l.Id,
                    userId = l.UserId,
                    userName = l.User != null ? l.User.FullName : "Unknown",
                    userEmail = l.User != null ? l.User.Email : "Unknown",
                    l.LoginTime,
                    l.LogoutTime,
                    l.IpAddress,
                    l.Browser,
                    l.Status,
                    l.FailureReason,
                    duration = l.LogoutTime.HasValue && l.LoginTime.HasValue
                        ? (l.LogoutTime.Value - l.LoginTime.Value).TotalMinutes.ToString("F2") + " minutes"
                        : "Still logged in"
                })
                .Take(100)
                .ToListAsync();

            _logger.LogInformation("GetLoginHistory returned {Count} records", history.Count);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLoginHistory failed for UserId: {UserId}", currentUserId);
            return BadRequest(new { message = "Failed to get login history.", error = ex.Message });
        }
    }

    [HttpGet("my-login-history")]
    public async Task<IActionResult> GetMyLoginHistory()
    {
        // ... (unchanged)
        var currentUserId = GetUserIdFromToken();
        try
        {
            var history = await _context.LoginHistories
                .Where(l => l.UserId == currentUserId)
                .OrderByDescending(l => l.LoginTime)
                .Select(l => new
                {
                    l.Id,
                    l.LoginTime,
                    l.LogoutTime,
                    l.IpAddress,
                    l.Browser,
                    l.Status,
                    l.FailureReason,
                    duration = l.LogoutTime.HasValue && l.LoginTime.HasValue
                        ? (l.LogoutTime.Value - l.LoginTime.Value).TotalMinutes.ToString("F2") + " minutes"
                        : "Still logged in"
                })
                .Take(50)
                .ToListAsync();

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMyLoginHistory failed for UserId: {UserId}", currentUserId);
            return BadRequest(new { message = "Failed to get login history.", error = ex.Message });
        }
    }

    // ===============================
    // HELPER METHODS (unchanged)
    // ===============================
    private string GenerateJwtToken(User user)
    {
        // ... unchanged
        var jwtSettings = _config.GetSection("JwtSettings");

        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.TableValue ?? "Customer"),
            new Claim("StoreId", user.StoreId ?? "0"),
            new Claim("FullName", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GetUserIdFromToken()
    {
        return User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    private async Task<int> GetMasterId(string tableName, string value)
    {
        var master = await _context.MasterTables
            .FirstOrDefaultAsync(m => m.TableName == tableName && m.TableValue == value);

        if (master == null)
            throw new Exception($"Master data not found: {tableName} - {value}");

        return master.Id;
    }

    private async Task LogLoginAttempt(string? userId, string status, string? reason)
    {
        // ... unchanged
        try
        {
            string userAgent = Request.Headers["User-Agent"].ToString() ?? "Unknown";
            if (userAgent.Length > 450)
                userAgent = userAgent[..450];

            var log = new LoginHistory
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId ?? string.Empty,
                LoginTime = GetIndianTime(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = userAgent,
                Browser = GetBrowserFromUserAgent(userAgent),
                Status = status,
                FailureReason = reason,
                CreatedDate = GetIndianTime(),
                Active = true
            };

            _context.LoginHistories.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Login attempt logged for UserId: {UserId}, Status: {Status}", userId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log login attempt for UserId: {UserId}", userId);
        }
    }

    private string GetBrowserFromUserAgent(string userAgent)
    {
        if (userAgent.Contains("Chrome")) return "Chrome";
        if (userAgent.Contains("Firefox")) return "Firefox";
        if (userAgent.Contains("Safari")) return "Safari";
        if (userAgent.Contains("Edge")) return "Edge";
        if (userAgent.Contains("MSIE") || userAgent.Contains("Trident")) return "Internet Explorer";
        return "Unknown";
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

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}