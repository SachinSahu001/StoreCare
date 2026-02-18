using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StoreCare.Server.Data;
using StoreCare.Server.Middlewares;
using StoreCare.Server.Models;
using StoreCare.Server.Services;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==============================
// 1. DATABASE CONFIGURATION
// ==============================
builder.Services.AddDbContext<StoreCareDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==============================
// 2. JWT AUTHENTICATION - FIXED FOR BOTH HTTP/HTTPS
// ==============================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ??
    "YourSuperSecretKeyForJWTTokenGenerationThatIsAtLeast32BytesLong!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to false to allow HTTP
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Check for token in Authorization header
            var authorization = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authorization) &&
                authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = authorization["Bearer ".Length..].Trim();
            }

            // Also check for token in query string (for WebSockets or Swagger HTTP)
            if (string.IsNullOrEmpty(context.Token))
            {
                context.Token = context.Request.Query["access_token"];
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
            var roleClaims = claimsIdentity?.FindAll(ClaimTypes.Role).ToList();

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token validated on {Scheme}://{Host}",
                context.HttpContext.Request.Scheme,
                context.HttpContext.Request.Host);

            if (roleClaims != null)
            {
                foreach (var claim in roleClaims)
                {
                    logger.LogInformation("- Role: {Role}", claim.Value);
                }
            }

            // Verify user exists and is active
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<StoreCareDbContext>();
            var userIdClaim = context.Principal?.FindFirst("UserId") ??
                 context.Principal?.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null)
            {
                var userId = userIdClaim.Value;
                var user = await dbContext.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Active == true);

                if (user == null)
                {
                    logger.LogWarning("User {UserId} not found or inactive", userId);
                    context.Fail("User not found or inactive");
                    return;
                }
            }
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "Authentication failed on {Scheme}://{Host}: {Message}",
                context.HttpContext.Request.Scheme,
                context.HttpContext.Request.Host,
                context.Exception.Message);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Authentication challenge on {Scheme}://{Host}: {Error}",
                context.HttpContext.Request.Scheme,
                context.HttpContext.Request.Host,
                context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

// ==============================
// 3. AUTHORIZATION
// ==============================
builder.Services.AddAuthorization(options =>
{
    // Simple role-based authorization
});

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

// ==============================
// 4. CORS CONFIGURATION - FIXED FOR BOTH ORIGINS
// ==============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .SetIsOriginAllowed(origin => true) // Allow any origin
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()); // Important for authentication
});

// ==============================
// 5. CONTROLLERS & SWAGGER - FIXED FOR MULTIPLE URLs
// ==============================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with multiple URL support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StoreCare API",
        Version = "v1",
        Description = "StoreCare Authentication API with Role-Based Authorization"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer eyJhbGciOiJIUzI1NiIs...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Support multiple server URLs in Swagger
    c.AddServer(new OpenApiServer
    {
        Url = "https://localhost:7066",
        Description = "HTTPS"
    });
    c.AddServer(new OpenApiServer
    {
        Url = "http://localhost:5041",
        Description = "HTTP"
    });
});

// ==============================
// FILE SERVICE REGISTRATION
// ==============================
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddHttpContextAccessor();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// ==============================
// 6. SEED DATABASE
// ==============================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StoreCareDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await SeedData(context, logger);
}

// ==============================
// 7. MIDDLEWARE PIPELINE
// ==============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StoreCare API V1");
        c.RoutePrefix = "swagger";
    });
    app.UseDeveloperExceptionPage();
}

// Don't force HTTPS in development for Swagger HTTP access
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseCors("AllowAll");

// CRITICAL: Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Test endpoint
app.MapGet("/", () => "StoreCare API is running!");

app.Run();

// ==============================
// SEEDING METHOD
// ==============================
async Task SeedData(StoreCareDbContext context, ILogger logger)
{
    await context.Database.EnsureCreatedAsync();

    // Seed Master Tables
    if (!await context.MasterTables.AnyAsync())
    {
        var masterData = new List<MasterTable>
        {
            // Roles
            new() { TableName = "Role", TableValue = "SuperAdmin", Active = true },
            new() { TableName = "Role", TableValue = "StoreAdmin", Active = true },
            new() { TableName = "Role", TableValue = "Customer", Active = true },
            
            // Statuses
            new() { TableName = "Status", TableValue = "Active", Active = true },
            new() { TableName = "Status", TableValue = "Suspended", Active = true },
            new() { TableName = "Status", TableValue = "Inactive", Active = true },
            new() { TableName = "Status", TableValue = "Blocked", Active = true }
        };

        context.MasterTables.AddRange(masterData);
        await context.SaveChangesAsync();
        logger.LogInformation("Master tables seeded.");
    }

    // Seed SuperAdmin
    var superAdminRole = await context.MasterTables
        .FirstOrDefaultAsync(m => m.TableName == "Role" && m.TableValue == "SuperAdmin");

    var activeStatus = await context.MasterTables
        .FirstOrDefaultAsync(m => m.TableName == "Status" && m.TableValue == "Active");

    if (superAdminRole != null && activeStatus != null)
    {
        if (!await context.Users.AnyAsync(u => u.Email == "admin@storecare.com"))
        {
            var admin = new User
            {
                UserCode = "SA-001",
                FullName = "Super Administrator",
                Email = "admin@storecare.com",
                Phone = "1234567890",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                RoleId = superAdminRole.Id,
                StatusId = activeStatus.Id,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System",
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = "System",
                Active = true
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            logger.LogInformation("=================================");
            logger.LogInformation("SuperAdmin seeded successfully!");
            logger.LogInformation("Email: admin@storecare.com");
            logger.LogInformation("Password: Admin@123");
            logger.LogInformation("=================================");
        }
    }
}