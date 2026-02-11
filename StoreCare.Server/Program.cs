using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StoreCare.Server.Data;
using StoreCare.Server.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==============================
// 1. DATABASE CONFIGURATION
// ==============================
builder.Services.AddDbContext<StoreCareDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==============================
// 2. JWT AUTHENTICATION - FIXED
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
    options.RequireHttpsMetadata = false;
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
        // CRITICAL FIX: Map both role claim types
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };

    // FIXED: Better token extraction
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authorization = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authorization) &&
                authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = authorization["Bearer ".Length..].Trim();
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        }
    };
});

// ==============================
// 3. AUTHORIZATION POLICIES
// ==============================
builder.Services.AddAuthorization(options =>
{
    // Define role-based policies as per your table
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
    options.AddPolicy("StoreAdmin", policy => policy.RequireRole("StoreAdmin"));
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("All", policy => policy.RequireAuthenticatedUser());
});

// ==============================
// 4. CORS CONFIGURATION
// ==============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// ==============================
// 5. CONTROLLERS & SWAGGER
// ==============================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// FIXED: Swagger with proper JWT configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StoreCare API",
        Version = "v1",
        Description = "StoreCare Authentication API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token. Example: 'Bearer eyJhbGciOiJIUzI1NiIs...'",
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
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ==============================
// 6. SEED DATABASE
// ==============================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StoreCareDbContext>();
    await SeedData(context);
}

// ==============================
// 7. MIDDLEWARE PIPELINE
// ==============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication(); // IMPORTANT: This must be before Authorization
app.UseAuthorization();

app.MapControllers();

// Test endpoint to verify API is running
app.MapGet("/", () => "StoreCare API is running!");

app.Run();

// ==============================
// SEEDING METHOD - FIXED
// ==============================
async Task SeedData(StoreCareDbContext context)
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
            new() { TableName = "Status", TableValue = "Inactive", Active = true }
        };

        context.MasterTables.AddRange(masterData);
        await context.SaveChangesAsync();
        Console.WriteLine("Master tables seeded.");
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
                CreatedBy = 0,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = 0,
                Active = true
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            Console.WriteLine("=================================");
            Console.WriteLine("SuperAdmin seeded successfully!");
            Console.WriteLine("Email: admin@storecare.com");
            Console.WriteLine("Password: Admin@123");
            Console.WriteLine("=================================");
        }
    }
}