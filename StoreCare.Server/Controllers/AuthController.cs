using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StoreCare.Server.Data;
using StoreCare.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StoreCare.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly StoreCareDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(StoreCareDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // ===============================
    // DTOs
    // ===============================
    public class RegisterRequestDto
    {
        [Required, MaxLength(200)] public string FullName { get; set; }
        [Required, EmailAddress, MaxLength(100)] public string Email { get; set; }
        [Required, RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be 10 digits.")] public string Phone { get; set; }
        [Required, MinLength(8)] public string Password { get; set; }
        [Required] public string ConfirmPassword { get; set; }
    }

    public class LoginRequestDto
    {
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string Password { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required] public string CurrentPassword { get; set; }
        [Required, MinLength(8)] public string NewPassword { get; set; }
        [Required] public string ConfirmNewPassword { get; set; }
    }

    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public int? StoreId { get; set; }
        public bool Active { get; set; }
        public string Status { get; set; }
    }

    // ===============================
    // 1. REGISTER (Customer only)
    // ===============================
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        try
        {
            dto.Email = dto.Email.Trim().ToLower();
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email is already registered." });

            var roleId = await GetMasterId("Role", "Customer");
            var statusId = await GetMasterId("Status", "Active");

            var user = new User
            {
                UserCode = "CUST-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                FullName = dto.FullName.Trim(),
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = roleId,
                StatusId = statusId,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful. Please login." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Registration failed.", error = ex.Message });
        }
    }

    // ===============================
    // 2. LOGIN (All roles)
    // ===============================
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        try
        {
            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Status)
                .FirstOrDefaultAsync(u => u.Email == email && u.Active == true);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                await LogLoginAttempt(null, "Failed", "Invalid credentials");
                return Unauthorized(new { message = "Invalid email or password." });
            }

            if (user.Status?.TableValue == "Suspended")
                return BadRequest(new { message = "Your account is suspended. Contact support." });

            // Generate token
            var token = GenerateJwtToken(user);

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Log successful login
            await LogLoginAttempt(user.Id, "Success", null);

            return Ok(new
            {
                token = token,
                role = user.Role.TableValue,
                fullName = user.FullName,
                email = user.Email,
                storeId = user.StoreId,
                message = "Login successful"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Login failed.", error = ex.Message });
        }
    }

    // ===============================
    // 3. LOGOUT (All authenticated users)
    // ===============================
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = GetUserIdFromToken();
            var lastLogin = await _context.LoginHistories
                .Where(l => l.UserId == userId && l.Status == "Success")
                .OrderByDescending(l => l.LoginTime)
                .FirstOrDefaultAsync();

            if (lastLogin != null)
            {
                lastLogin.LogoutTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Logout failed.", error = ex.Message });
        }
    }

    // ===============================
    // 4. GET PROFILE (All authenticated users)
    // ===============================
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetUserIdFromToken();
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Status)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                phone = user.Phone ?? "",
                role = user.Role?.TableValue,
                status = user.Status?.TableValue ?? "Active",
                active = user.Active,
                storeId = user.StoreId,
                lastLogin = user.LastLogin
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Failed to get profile.", error = ex.Message });
        }
    }

    // ===============================
    // 5. UPDATE PROFILE (All authenticated users)
    // ===============================
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.FullName = dto.FullName;
            user.Phone = dto.Phone;
            user.ModifiedDate = DateTime.UtcNow;
            user.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Failed to update profile.", error = ex.Message });
        }
    }

    // ===============================
    // 6. CHANGE PASSWORD (All authenticated users)
    // ===============================
    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                return BadRequest(new { message = "New passwords do not match." });

            var userId = GetUserIdFromToken();
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest(new { message = "Current password is incorrect." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ModifiedDate = DateTime.UtcNow;
            user.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Failed to change password.", error = ex.Message });
        }
    }

    // ===============================
    // 7. CREATE STOREADMIN (SuperAdmin only)
    // ===============================
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("storeadmin")]
    public async Task<IActionResult> CreateStoreAdmin(RegisterRequestDto dto)
    {
        try
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            dto.Email = dto.Email.Trim().ToLower();
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email is already registered." });

            var currentUserId = GetUserIdFromToken();
            var roleId = await GetMasterId("Role", "StoreAdmin");
            var statusId = await GetMasterId("Status", "Active");

            // Create store first
            var store = new Store
            {
                StoreCode = "ST-" + Guid.NewGuid().ToString()[..5].ToUpper(),
                StoreName = $"{dto.FullName}'s Store",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = currentUserId,
                Active = true
            };
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            // Create store admin
            var user = new User
            {
                UserCode = "SADM-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                FullName = dto.FullName.Trim(),
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = roleId,
                StoreId = store.Id,
                StatusId = statusId,
                CreatedBy = currentUserId,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

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
            return BadRequest(new { message = "Failed to create StoreAdmin.", error = ex.Message });
        }
    }

    // ===============================
    // 8. LOGIN HISTORY (SuperAdmin only)
    // ===============================
    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("login-history")]
    public async Task<IActionResult> GetLoginHistory([FromQuery] int? userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var query = _context.LoginHistories
                .Include(l => l.User)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId.Value);

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
                    userName = l.User.FullName,
                    userEmail = l.User.Email,
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

            return Ok(history);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Failed to get login history.", error = ex.Message });
        }
    }

    // ===============================
    // DTO for Update Profile
    // ===============================
    public class UpdateProfileDto
    {
        [Required, MaxLength(200)]
        public string FullName { get; set; }

        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be 10 digits.")]
        public string Phone { get; set; }
    }

    // ===============================
    // HELPERS
    // ===============================
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.TableValue), // This is critical for role authorization
            new Claim("role", user.Role.TableValue), // Add both formats for compatibility
            new Claim("StoreId", user.StoreId?.ToString() ?? "0"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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

    private int GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    private async Task<int> GetMasterId(string tableName, string value)
    {
        var master = await _context.MasterTables
            .FirstOrDefaultAsync(m => m.TableName == tableName && m.TableValue == value);

        if (master == null)
            throw new Exception($"Master data not found: {tableName} - {value}");

        return master.Id;
    }

    private async Task LogLoginAttempt(int? userId, string status, string? reason)
    {
        try
        {
            string userAgent = Request.Headers["User-Agent"].ToString() ?? "Unknown";
            if (userAgent.Length > 450)
                userAgent = userAgent[..450];

            var log = new LoginHistory
            {
                UserId = userId ?? 0,
                LoginTime = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                Browser = userAgent,
                Status = status,
                FailureReason = reason,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            _context.LoginHistories.Add(log);
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Silently fail - don't break login flow for logging errors
        }
    }
}