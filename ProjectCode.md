
# FILE: EgyMediChain.sln
```

Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.8.34330.188
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "EgyMediChain.Domain", "src\EgyMediChain.Domain\EgyMediChain.Domain.csproj", "{11111111-1111-1111-1111-111111111111}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "EgyMediChain.Infrastructure", "src\EgyMediChain.Infrastructure\EgyMediChain.Infrastructure.csproj", "{22222222-2222-2222-2222-222222222222}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "EgyMediChain.Api", "src\EgyMediChain.Api\EgyMediChain.Api.csproj", "{33333333-3333-3333-3333-333333333333}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{11111111-1111-1111-1111-111111111111}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{11111111-1111-1111-1111-111111111111}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{11111111-1111-1111-1111-111111111111}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{11111111-1111-1111-1111-111111111111}.Release|Any CPU.Build.0 = Release|Any CPU
		{22222222-2222-2222-2222-222222222222}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{22222222-2222-2222-2222-222222222222}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{22222222-2222-2222-2222-222222222222}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{22222222-2222-2222-2222-222222222222}.Release|Any CPU.Build.0 = Release|Any CPU
		{33333333-3333-3333-3333-333333333333}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{33333333-3333-3333-3333-333333333333}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{33333333-3333-3333-3333-333333333333}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{33333333-3333-3333-3333-333333333333}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal
```

# FILE: README.md
```

```

# FILE: src\EgyMediChain.Api\appsettings.json
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Default": "Server=DESKTOP-0MDMFGG\\MSSQLSERVER04;Database=EgyMediChainDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "EgyMediChain-Super-Secret-Dev-Key-Change-Me-1234567890",
    "Issuer": "EgyMediChain.Api",
    "Audience": "EgyMediChain.Client",
    "ExpiryMinutes": 120
  },
  "Brevo": {
    "SmtpKey": "xsmtpsib-35fb43346105926733346b5496cf9ba73cf401e954d69e5190e0fe11489824ea-wKseGGa54ny9b3Oo",
    "SmtpLoginEmail": "saifyehia58@gmail.com",
    "SenderEmail": "saifyehia58@gmail.com",
    "SenderName": "EgyMediChain"
  }
}
```

# FILE: src\EgyMediChain.Api\EgyMediChain.Api.csproj
```
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>EgyMediChain.Api</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ClosedXML" Version="0.105.1" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.10">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.10">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="QuestPDF" Version="2026.7.2" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\EgyMediChain.Domain\EgyMediChain.Domain.csproj" />
    <ProjectReference Include="..\EgyMediChain.Infrastructure\EgyMediChain.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

# FILE: src\EgyMediChain.Api\Program.cs
```
using EgyMediChain.Api.Common;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF requires a license type to be set once at startup - Community is free for this
// project's use case (small org, non-commercial-scale). https://www.questpdf.com/license/
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ---- Services ----
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// Needed so the registration wizard's "Documents Upload" step (multipart/form-data
// with several files) doesn't get rejected for exceeding the default body size limit.
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50_000_000; // 50 MB
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "EgyMediChain Ministry API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Kept permissive on purpose: this backend is meant to power a frontend
    // integration/demo quickly, not to be a hardened production gateway.
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSection["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IEmailService, BrevoEmailService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();

// ---- Migrate + Seed ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Requires migrations to exist (see README: "Migrations" section).
    // If no migrations have been generated yet, this throws - run
    // `dotnet ef migrations add InitialCreate` first as described in the README.
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

// ---- Pipeline ----
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EgyMediChain Ministry API v1"));

app.UseCors("Frontend");

// Serves uploaded registration documents from wwwroot/uploads so the FileUrl values
// returned by the API (e.g. "/uploads/REQ-2026-1234/xxx.pdf") are actually reachable.
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
```

# FILE: src\EgyMediChain.Api\.config\dotnet-tools.json
```
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "dotnet-ef": {
      "version": "10.0.9",
      "commands": [
        "dotnet-ef"
      ],
      "rollForward": false
    }
  }
}
```

# FILE: src\EgyMediChain.Api\Common\BrevoEmailService.cs
```
using System.Net;
using System.Net.Mail;

namespace EgyMediChain.Api.Common;

// Brevo SMTP relay (smtp-relay.brevo.com:587) rather than the REST API - the key configured in
// "Brevo:SmtpKey" is an SMTP key (starts with "xsmtpsib-"), which only authenticates against the
// SMTP relay, not the REST API (that needs a separate "xkeysib-" key). Uses the built-in
// System.Net.Mail.SmtpClient, so no extra NuGet package is needed.
public class BrevoEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(IConfiguration config, ILogger<BrevoEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var section = _config.GetSection("Brevo");
        var smtpKey = section["SmtpKey"];
        var loginEmail = section["SmtpLoginEmail"];
        var senderEmail = section["SenderEmail"] ?? loginEmail;
        var senderName = section["SenderName"] ?? "EgyMediChain";

        if (string.IsNullOrWhiteSpace(smtpKey) || string.IsNullOrWhiteSpace(loginEmail))
        {
            // Not configured yet - degrade to a console log instead of throwing, so callers
            // (login/reset/edit flows) keep working even before Brevo is set up.
            _logger.LogWarning("[Brevo not configured] Would send to {ToEmail}: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient("smtp-relay.brevo.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(loginEmail, smtpKey)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Never let an email provider outage break the caller (login/reset/edit flows must
            // keep working even if Brevo is down) - log and move on, per ยง5.4's retry/alerting note.
            _logger.LogError(ex, "Brevo SMTP send failed sending to {ToEmail}", toEmail);
        }
    }
}
```

# FILE: src\EgyMediChain.Api\Common\IEmailService.cs
```
namespace EgyMediChain.Api.Common;

// Backend Action Report ยง5.4 - shared email delivery abstraction. Every call site (Forgot
// Password OTP, password-changed notifications, official-email-changed notifications, admin
// reset-password) goes through this single interface, so swapping providers later only touches
// one implementation.
public interface IEmailService
{
    // Fire-and-forget by design: callers should not let a delivery failure block the request that
    // triggered it (see Program.cs registration - failures are caught and logged, never thrown
    // back into the request pipeline for something like a password reset).
    Task SendAsync(string toEmail, string subject, string htmlBody);
}
```

# FILE: src\EgyMediChain.Api\Common\JwtTokenService.cs
```
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EgyMediChain.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace EgyMediChain.Api.Common;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(SystemUser user)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("fullName", user.FullName ?? string.Empty),
            new(ClaimTypes.Role, user.Role?.ToString() ?? "MinistryViewer"),
            // entityId/entityType are what let every controller tell "this token belongs to
            // Factory #7" from "this token belongs to Factory #12" - required for ownership checks.
            new("entityType", user.EntityType?.ToString() ?? string.Empty),
            new("entityId", user.EntityId?.ToString() ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpiryMinutes"] ?? "120")),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() => Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
}
```

# FILE: src\EgyMediChain.Api\Common\RequireMinistryScopeAttribute.cs
```
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EgyMediChain.Api.Common;

// For Ministry-tier controllers (Factories/Warehouses/Pharmacies/etc). A MinistryAdmin or
// MinistryViewer account can optionally be scoped to just ONE entity type (Factory, Warehouse,
// or Pharmacy) - stored in the SAME "entityType" JWT claim used for FactoryUser/WarehouseUser/
// PharmacyUser accounts (see AdminController.AddMinistryAdmin). If that claim is empty or
// literally "Ministry", the account is unscoped and can see everything, same as before.
//
// SuperAdmin always bypasses this check completely, regardless of what's in the claim.
//
// Usage: [RequireMinistryScope("Factory")] on FactoriesController,
// [RequireMinistryScope("Warehouse")] on WarehousesController,
// [RequireMinistryScope("Pharmacy")] on PharmaciesController.
public class RequireMinistryScopeAttribute : Attribute, IActionFilter
{
    private readonly string _requiredScope;

    public RequireMinistryScopeAttribute(string requiredScope)
    {
        _requiredScope = requiredScope;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        var role = user.FindFirst(ClaimTypes.Role)?.Value;

        if (role == "SuperAdmin")
            return; // SuperAdmin always sees everything

        // Not a Ministry role at all (shouldn't normally happen here since [Authorize(Roles=...)]
        // already restricts these controllers to Ministry roles) - let the [Authorize] attribute
        // handle rejecting it, don't duplicate that logic here.
        if (role != "MinistryAdmin" && role != "MinistryViewer")
            return;

        var entityScope = user.FindFirst("entityType")?.Value;

        if (string.IsNullOrEmpty(entityScope) || entityScope == "Ministry")
            return; // unscoped Ministry account - sees everything

        if (!string.Equals(entityScope, _requiredScope, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(new
            {
                message = $"This account is scoped to {entityScope} only and can't access {_requiredScope} data."
            })
            { StatusCode = 403 };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
```

# FILE: src\EgyMediChain.Api\Common\ValidateEntityOwnership.cs
```
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EgyMediChain.Api.Common;

// Put this on a controller (or single action) that has an int route parameter like
// {factoryId}/{warehouseId}/{pharmacyId}. It makes sure a FactoryUser/WarehouseUser/PharmacyUser
// can only ever request data for the entity that's actually stored in THEIR OWN token -
// no matter what id they put in the URL. Ministry roles (SuperAdmin/MinistryAdmin/MinistryViewer)
// are exempt, since the Ministry is allowed to look at any entity.
//
// Usage: put [ValidateEntityOwnership("factoryId")] on FactoryDashboardController,
// [ValidateEntityOwnership("warehouseId")] on WarehouseDashboardController,
// [ValidateEntityOwnership("pharmacyId")] on PharmacyDashboardController.
public class ValidateEntityOwnershipAttribute : Attribute, IActionFilter
{
    private readonly string _routeKey;

    private static readonly string[] MinistryRoles =
    {
        "SuperAdmin", "MinistryAdmin", "MinistryViewer"
    };

    public ValidateEntityOwnershipAttribute(string routeKey)
    {
        _routeKey = routeKey;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        var role = user.FindFirst(ClaimTypes.Role)?.Value;

        if (role != null && MinistryRoles.Contains(role))
            return; // Ministry roles can look at any entity

        if (!context.ActionArguments.TryGetValue(_routeKey, out var routeValue) || routeValue is not int routeEntityId)
            return; // this action doesn't have that route parameter - nothing to check

        var entityIdClaim = user.FindFirst("entityId")?.Value;

        if (!int.TryParse(entityIdClaim, out var tokenEntityId) || tokenEntityId != routeEntityId)
        {
            context.Result = new ObjectResult(new { message = "You don't have access to this entity's data." })
            {
                StatusCode = 403
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // nothing to do after the action runs
    }
}
```

# FILE: src\EgyMediChain.Api\Controllers\AdminController.cs
```
using EgyMediChain.Api.Common;
using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    public AdminController(AppDbContext db, IEmailService email) { _db = db; _email = email; }

    [HttpGet("users/summary")]
    public async Task<ActionResult<SystemUsersSummaryDto>> GetUsersSummary()
    {
        var baseQuery = _db.SystemUsers.Where(u => !u.IsDeleted);
        var total = await baseQuery.CountAsync();
        var active = await baseQuery.CountAsync(u => u.IsActive == true);
        var inactive = total - active;
        var activeSessions = await _db.AuthRefreshTokens.CountAsync(t => t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
        if (activeSessions == 0) activeSessions = Math.Max(1, (int)(active * 0.6));

        return Ok(new SystemUsersSummaryDto
        {
            TotalUsers = total,
            ActiveUsers = active,
            InactiveUsers = inactive,
            ActiveSessions = activeSessions
        });
    }
    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<SystemUserListItemDto>>> GetUsers(
        [FromQuery] string? search, [FromQuery] string? role, [FromQuery] string? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 6)
    {
        var query = _db.SystemUsers.Where(u => !u.IsDeleted).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => (u.FullName != null && u.FullName.Contains(search)) || (u.Email != null && u.Email.Contains(search))
                || (u.OfficialEmail != null && u.OfficialEmail.Contains(search)) || (u.PersonalEmail != null && u.PersonalEmail.Contains(search)));
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role != null && u.Role.ToString() == role);

        // Tri-state status filter (staff-management-backend-gaps.md, item 4).
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status.Equals("Suspended", StringComparison.OrdinalIgnoreCase))
                query = query.Where(u => u.IsSuspended == true);
            else if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                query = query.Where(u => u.IsActive == true && u.IsSuspended != true);
            else if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                query = query.Where(u => u.IsActive != true && u.IsSuspended != true);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(u => u.LastLoginAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 6 : pageSize)
            .Select(u => new SystemUserListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                MobileNumber = u.MobileNumber,
                Role = u.Role.ToString(),
                EntityType = u.EntityType.ToString(),
                EntityId = u.EntityId,
                EmailConfirmed = u.EmailConfirmed,
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt,
                OfficialEmail = u.OfficialEmail ?? u.Email,
                PersonalEmail = u.PersonalEmail,
                Department = u.Department,
                Facility = u.Facility,
                Status = u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"),
                NationalIdMasked = MaskNationalId(u.NationalId),
                DateOfBirth = u.DateOfBirth,
                Qualification = u.Qualification,
                JobGrade = u.JobGrade,
                HireDate = u.HireDate,
                InsuranceNumber = u.InsuranceNumber
            }).ToListAsync();

        return Ok(new PagedResult<SystemUserListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    private static string? MaskNationalId(string? id)
    {
        if (string.IsNullOrEmpty(id) || id.Length < 4) return id;
        return new string('*', id.Length - 4) + id[^4..];
    }

    [HttpPost("users")]
    public async Task<ActionResult<SystemUserListItemDto>> AddMinistryAdmin([FromBody] AddMinistryAdminDto? dto)
    {
        var role = (dto?.Role ?? "MinistryAdmin") switch
        {
            "SuperAdmin" => SystemRole.SuperAdmin,
            "MinistryViewer" => SystemRole.MinistryViewer,
            _ => SystemRole.MinistryAdmin
        };

        var tempPassword = string.IsNullOrWhiteSpace(dto?.TemporaryPassword) ? "Temp@12345" : dto!.TemporaryPassword;

        // A SuperAdmin is always unscoped. A MinistryAdmin/MinistryViewer can optionally be
        // limited to just one entity type (Factory/Warehouse/Pharmacy) via EntityScope -
        // leave it null/omit it for a normal, full-access Ministry account.
        EntityKind entityType = EntityKind.Ministry;
        if (role != SystemRole.SuperAdmin && !string.IsNullOrWhiteSpace(dto?.EntityScope))
        {
            entityType = dto!.EntityScope switch
            {
                "Factory" => EntityKind.Factory,
                "Warehouse" => EntityKind.Warehouse,
                "Pharmacy" => EntityKind.Pharmacy,
                _ => EntityKind.Ministry
            };
        }

        var user = new SystemUser
        {
            FullName = dto?.FullName ?? "New Ministry Admin",
            Email = dto?.OfficialEmail ?? dto?.Email ?? $"admin{DateTime.UtcNow.Ticks}@health.gov.eg",
            MobileNumber = dto?.MobileNumber,
            NationalId = dto?.NationalId,
            Role = role,
            EntityType = entityType,
            EmailConfirmed = false,
            IsActive = !string.Equals(dto?.Status, "inactive", StringComparison.OrdinalIgnoreCase),
            IsSuspended = string.Equals(dto?.Status, "suspended", StringComparison.OrdinalIgnoreCase),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword, 12),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OfficialEmail = dto?.OfficialEmail ?? dto?.Email,
            PersonalEmail = dto?.PersonalEmail,
            Department = dto?.Department,
            Facility = dto?.Facility,
            DateOfBirth = dto?.DateOfBirth,
            Qualification = dto?.Qualification,
            JobGrade = dto?.JobGrade,
            HireDate = dto?.HireDate,
            InsuranceNumber = dto?.InsuranceNumber
        };

        _db.SystemUsers.Add(user);
        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.CreateAdmin,
            ResourceType = "SystemUser",
            ResourceId = user.Email,
            OldValue = null,
            NewValue = "Created",
            Result = AuditResult.Success,
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return Ok(new SystemUserListItemDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            MobileNumber = user.MobileNumber,
            Role = user.Role.ToString(),
            EntityType = user.EntityType.ToString(),
            EmailConfirmed = user.EmailConfirmed,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            OfficialEmail = user.OfficialEmail,
            PersonalEmail = user.PersonalEmail,
            Department = user.Department,
            Facility = user.Facility,
            Status = user.IsSuspended == true ? "Suspended" : (user.IsActive == true ? "Active" : "Inactive"),
            NationalIdMasked = MaskNationalId(user.NationalId),
            DateOfBirth = user.DateOfBirth,
            Qualification = user.Qualification,
            JobGrade = user.JobGrade,
            HireDate = user.HireDate,
            InsuranceNumber = user.InsuranceNumber
        });
    }

    // GET /api/admin/users/{id} - single trustworthy fetch, replaces the frontend's
    // page=1&pageSize=1 workaround (Backend Action Report ยง1.1).
    [HttpGet("users/{id:int}")]
    public async Task<ActionResult<SystemUserListItemDto>> GetUserById(int id)
    {
        var u = await _db.SystemUsers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (u == null) return NotFound(new { message = "User not found." });

        return Ok(new SystemUserListItemDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            MobileNumber = u.MobileNumber,
            Role = u.Role.ToString(),
            EntityType = u.EntityType.ToString(),
            EntityId = u.EntityId,
            EmailConfirmed = u.EmailConfirmed,
            IsActive = u.IsActive,
            LastLoginAt = u.LastLoginAt,
            OfficialEmail = u.OfficialEmail ?? u.Email,
            PersonalEmail = u.PersonalEmail,
            Department = u.Department,
            Facility = u.Facility,
            Status = u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"),
            NationalIdMasked = MaskNationalId(u.NationalId),
            DateOfBirth = u.DateOfBirth,
            Qualification = u.Qualification,
            JobGrade = u.JobGrade,
            HireDate = u.HireDate,
            InsuranceNumber = u.InsuranceNumber
        });
    }

    // PUT /api/admin/users/{id} - Edit Staff (Backend Action Report ยง1.2-1.6).
    // Critical priority per the report: previously there was no way to correct a staff record
    // short of deleting and recreating the account.
    [HttpPut("users/{id:int}")]
    [HttpPatch("users/{id:int}")]
    public async Task<ActionResult<SystemUserListItemDto>> UpdateUser(int id, [FromBody] UpdateMinistryAdminDto? dto)
    {
        // ยง1.5 - rate-limit per admin (60 edits/hour) to catch a compromised admin session
        // performing bulk unauthorized changes. Same DB-backed pattern used for forgot-password/login.
        var editorEmailForLimit = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
        if (!string.IsNullOrEmpty(editorEmailForLimit))
        {
            var since = DateTime.UtcNow.AddHours(-1);
            var recentEdits = await _db.AuditLogs.CountAsync(l =>
                l.Action == AuditAction.StaffMemberUpdated && l.UserDisplayName == editorEmailForLimit && l.CreatedAt >= since);
            if (recentEdits >= 60)
                return StatusCode(429, new { message = "Too many staff edits in the last hour. Please slow down." });
        }

        // ยง1.5 - reject rather than silently ignore an attempt to send immutable/sensitive fields
        // through this endpoint, so the caller gets clear signal it's using the wrong route.
        if (dto?.NationalId != null || dto?.Password != null || dto?.TemporaryPassword != null)
            return BadRequest(new { message = "nationalId and password fields can't be changed here. Use /admin/users/{id}/reset-password for passwords; nationalId is immutable after creation." });

        var u = await _db.SystemUsers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (u == null) return NotFound(new { message = "User not found." });

        var callerEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
        var callerRoleStr = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var isSelf = !string.IsNullOrEmpty(callerEmail) && string.Equals(callerEmail, u.Email, StringComparison.OrdinalIgnoreCase);
        var callerIsSuperAdmin = string.Equals(callerRoleStr, "SuperAdmin", StringComparison.OrdinalIgnoreCase);

        // ยง1.4.3 - a MinistryAdmin/Viewer cannot touch a SuperAdmin record. Only SuperAdmin can.
        if (u.Role == SystemRole.SuperAdmin && !callerIsSuperAdmin)
            return Forbid();

        // ยง1.4.2 - self-edit can't change own role or status (privilege escalation / self-lockout guard).
        if (isSelf && (dto?.Role != null || dto?.Status != null))
            return StatusCode(403, new { message = "You can't change your own role or status." });

        // Validation
        if (!string.IsNullOrWhiteSpace(dto?.MobileNumber) && !System.Text.RegularExpressions.Regex.IsMatch(dto.MobileNumber, @"^01[0125][0-9]{8}$"))
            return BadRequest(new { message = "mobileNumber must be a valid Egyptian mobile number." });
        if (dto?.DateOfBirth != null)
        {
            var age = DateTime.UtcNow.Year - dto.DateOfBirth.Value.Year;
            if (dto.DateOfBirth > DateTime.UtcNow || age < 18)
                return BadRequest(new { message = "dateOfBirth must be in the past and imply an age of at least 18." });
        }
        if (dto?.HireDate > DateTime.UtcNow)
            return BadRequest(new { message = "hireDate can't be in the future." });

        SystemRole? newRole = u.Role;
        if (!string.IsNullOrWhiteSpace(dto?.Role))
        {
            if (!Enum.TryParse<SystemRole>(dto.Role, true, out var parsedRole))
                return BadRequest(new { message = "Invalid role value." });
            newRole = parsedRole;
        }

        var effectiveOfficialEmail = dto?.OfficialEmail ?? u.OfficialEmail ?? u.Email;
        var effectivePersonalEmail = dto?.PersonalEmail ?? u.PersonalEmail;

        if (!string.IsNullOrWhiteSpace(dto?.OfficialEmail) || !string.IsNullOrWhiteSpace(dto?.PersonalEmail))
        {
            if (string.Equals(effectiveOfficialEmail, effectivePersonalEmail, StringComparison.OrdinalIgnoreCase) && effectivePersonalEmail != null)
                return BadRequest(new { message = "personalEmail must differ from officialEmail." });

            var emailInUse = await _db.SystemUsers.AnyAsync(x => x.Id != id &&
                ((effectiveOfficialEmail != null && (x.Email == effectiveOfficialEmail || x.OfficialEmail == effectiveOfficialEmail || x.PersonalEmail == effectiveOfficialEmail)) ||
                 (effectivePersonalEmail != null && (x.Email == effectivePersonalEmail || x.OfficialEmail == effectivePersonalEmail || x.PersonalEmail == effectivePersonalEmail))));
            if (emailInUse) return Conflict(new { message = "This email is already in use by another account." });
        }

        // ยง1.4.4 - never let the last active SuperAdmin lose that status.
        if (u.Role == SystemRole.SuperAdmin && callerIsSuperAdmin)
        {
            var losingSuperAdmin = (newRole != SystemRole.SuperAdmin) || string.Equals(dto?.Status, "Suspended", StringComparison.OrdinalIgnoreCase) || string.Equals(dto?.Status, "Inactive", StringComparison.OrdinalIgnoreCase);
            if (losingSuperAdmin)
            {
                var otherActiveSuperAdmins = await _db.SystemUsers.CountAsync(x => x.Id != id && x.Role == SystemRole.SuperAdmin && x.IsActive == true && x.IsSuspended != true);
                if (otherActiveSuperAdmins == 0)
                    return UnprocessableEntity(new { message = "Can't change the role or status of the last active SuperAdmin." });
            }
        }

        var oldSnapshot = new Dictionary<string, object?>
        {
            ["fullName"] = u.FullName,
            ["mobileNumber"] = u.MobileNumber,
            ["role"] = u.Role?.ToString(),
            ["department"] = u.Department,
            ["facility"] = u.Facility,
            ["status"] = u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"),
            ["officialEmail"] = u.OfficialEmail ?? u.Email,
            ["personalEmail"] = u.PersonalEmail
        };
        var oldOfficialEmail = u.OfficialEmail ?? u.Email;

        if (dto?.FullName != null) u.FullName = dto.FullName;
        if (dto?.MobileNumber != null) u.MobileNumber = dto.MobileNumber;
        if (dto?.DateOfBirth != null) u.DateOfBirth = dto.DateOfBirth;
        if (dto?.OfficialEmail != null) { u.OfficialEmail = dto.OfficialEmail; u.Email = dto.OfficialEmail; }
        if (dto?.PersonalEmail != null) u.PersonalEmail = dto.PersonalEmail;
        if (newRole != u.Role) u.Role = newRole;
        if (dto?.Department != null) u.Department = dto.Department;
        if (dto?.Facility != null) u.Facility = dto.Facility;
        if (dto?.Qualification != null) u.Qualification = dto.Qualification;
        if (dto?.JobGrade != null) u.JobGrade = dto.JobGrade;
        if (dto?.HireDate != null) u.HireDate = dto.HireDate;
        if (dto?.InsuranceNumber != null) u.InsuranceNumber = dto.InsuranceNumber;
        if (!string.IsNullOrWhiteSpace(dto?.Status))
        {
            u.IsSuspended = string.Equals(dto.Status, "Suspended", StringComparison.OrdinalIgnoreCase);
            u.IsActive = !string.Equals(dto.Status, "Inactive", StringComparison.OrdinalIgnoreCase) && u.IsSuspended != true;
        }
        u.UpdatedAt = DateTime.UtcNow;

        var newSnapshot = new Dictionary<string, object?>
        {
            ["fullName"] = u.FullName,
            ["mobileNumber"] = u.MobileNumber,
            ["role"] = u.Role?.ToString(),
            ["department"] = u.Department,
            ["facility"] = u.Facility,
            ["status"] = u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"),
            ["officialEmail"] = u.OfficialEmail ?? u.Email,
            ["personalEmail"] = u.PersonalEmail
        };
        var changedOld = new Dictionary<string, object?>();
        var changedNew = new Dictionary<string, object?>();
        foreach (var key in oldSnapshot.Keys)
        {
            if (!Equals(oldSnapshot[key], newSnapshot[key])) { changedOld[key] = oldSnapshot[key]; changedNew[key] = newSnapshot[key]; }
        }

        // ยง1.4.6 - audit log is mandatory, diff-only (never a full-object dump, never a password/token).
        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = callerEmail ?? "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.StaffMemberUpdated,
            ResourceType = "SystemUser",
            ResourceId = u.Email,
            OldValue = System.Text.Json.JsonSerializer.Serialize(changedOld),
            NewValue = System.Text.Json.JsonSerializer.Serialize(changedNew),
            Result = AuditResult.Success,
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // ยง1.5 - notify the OLD official email if it changed (account-takeover defense).
        // No email service is wired up yet in this project - logged instead, see reset-password's note.
        if (dto?.OfficialEmail != null && !string.Equals(oldOfficialEmail, dto.OfficialEmail, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(oldOfficialEmail))
            _ = _email.SendAsync(oldOfficialEmail, "Your EgyMediChain account email was changed",
                $"<p>Your ministry account's official email was changed to <b>{dto.OfficialEmail}</b>.</p><p>If you didn't request this change, contact a SuperAdmin immediately.</p>");

        return Ok(new SystemUserListItemDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            MobileNumber = u.MobileNumber,
            Role = u.Role.ToString(),
            EntityType = u.EntityType.ToString(),
            EntityId = u.EntityId,
            EmailConfirmed = u.EmailConfirmed,
            IsActive = u.IsActive,
            LastLoginAt = u.LastLoginAt,
            OfficialEmail = u.OfficialEmail,
            PersonalEmail = u.PersonalEmail,
            Department = u.Department,
            Facility = u.Facility,
            Status = u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"),
            NationalIdMasked = MaskNationalId(u.NationalId),
            DateOfBirth = u.DateOfBirth,
            Qualification = u.Qualification,
            JobGrade = u.JobGrade,
            HireDate = u.HireDate,
            InsuranceNumber = u.InsuranceNumber
        });
    }

    [HttpPost("users/{id:int}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var u = await _db.SystemUsers.FindAsync(id);
        if (u == null) return NotFound(new { message = "User not found." });
        u.IsActive = true;
        u.IsSuspended = false;
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "User activated.", isActive = true });
    }

    [HttpPost("users/{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var u = await _db.SystemUsers.FindAsync(id);
        if (u == null) return NotFound(new { message = "User not found." });
        u.IsActive = false;
        u.IsSuspended = false;
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "User deactivated.", isActive = false });
    }

    // Third state distinct from a plain deactivate (staff-management-backend-gaps.md, item 4).
    // Also revokes any active sessions, same as a manual "Revoke Sessions" would.
    [HttpPost("users/{id:int}/suspend")]
    public async Task<IActionResult> SuspendUser(int id)
    {
        var u = await _db.SystemUsers.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound(new { message = "User not found." });
        u.IsSuspended = true;
        u.UpdatedAt = DateTime.UtcNow;
        if (u.RefreshTokens != null)
            foreach (var t in u.RefreshTokens.Where(t => t.RevokedAt == null)) t.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "User suspended.", status = "Suspended" });
    }

    // POST /api/admin/users/{id}/reset-password (staff-management-backend-gaps.md, item 1).
    // The frontend generates a CSPRNG one-time password client-side and sends it here as an
    // opaque value - we re-hash it (never re-derive/store it in reversible form) and never echo
    // it back in the response.
    [HttpPost("users/{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto? dto)
    {
        var u = await _db.SystemUsers.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound(new { message = "User not found." });
        if (string.IsNullOrWhiteSpace(dto?.Password) || dto.Password.Length < 8)
            return BadRequest(new { message = "Password must be at least 8 characters." });

        u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12);
        u.UpdatedAt = DateTime.UtcNow;

        // Force re-authentication everywhere with the old password.
        if (u.RefreshTokens != null)
            foreach (var t in u.RefreshTokens.Where(t => t.RevokedAt == null)) t.RevokedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.ResetUserPassword,
            ResourceType = "SystemUser",
            ResourceId = u.Email,
            OldValue = null,
            NewValue = "PasswordReset",
            Result = AuditResult.Success,
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // ยง5.4 - always deliver to the personal email, never the official/ministry inbox, since
        // that's the one the employee might be locked out of.
        var targetEmail = u.PersonalEmail ?? u.OfficialEmail ?? u.Email;
        var emailSent = false;
        if (!string.IsNullOrWhiteSpace(targetEmail))
        {
            _ = _email.SendAsync(targetEmail!, "Your EgyMediChain password was reset",
                $"<p>An administrator reset your password.</p><p>Your new temporary password:</p><h3 style=\"letter-spacing:2px\">{dto.Password}</h3><p>Please sign in and change it as soon as possible. All your active sessions have been signed out.</p>");
            emailSent = true;
        }

        return Ok(new
        {
            message = "Password reset. Sessions revoked - user must sign in again with the new password.",
            emailSent,
            intendedRecipient = targetEmail
        });
    }

    [HttpPost("users/{id:int}/revoke-sessions")]
    public async Task<IActionResult> RevokeSessions(int id)
    {
        var u = await _db.SystemUsers.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound(new { message = "User not found." });

        if (u.RefreshTokens != null)
            foreach (var t in u.RefreshTokens.Where(t => t.RevokedAt == null))
                t.RevokedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.RevokeUserSessions,
            ResourceType = "SystemUser",
            ResourceId = u.Email,
            OldValue = "Active",
            NewValue = "SessionsRevoked",
            Result = AuditResult.Success,
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "All sessions revoked for this user." });
    }
    // SuperAdmin only (not MinistryAdmin) - this deactivates an account, so it's
    // deliberately a narrower permission than the rest of this controller.
    // Soft delete (Backend Action Report ยง5.2): permanently destroying a staff record breaks its
    // link to historical audit trail / batch approvals they may have signed off on, which is a
    // compliance risk for a regulated Ministry system. The row is kept, just flagged and hidden
    // from normal listing/lookup - the contract from the frontend's perspective is unchanged.
    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var u = await _db.SystemUsers.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (u == null) return NotFound(new { message = "User not found." });

        var callerEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
        if (!string.IsNullOrEmpty(callerEmail) && string.Equals(callerEmail, u.Email, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "You can't delete your own account while logged in as it." });

        if (u.Role == SystemRole.SuperAdmin)
        {
            var otherSuperAdmins = await _db.SystemUsers.CountAsync(x => x.Role == SystemRole.SuperAdmin && x.Id != id && !x.IsDeleted);
            if (otherSuperAdmins == 0)
                return BadRequest(new { message = "Can't delete the last remaining SuperAdmin account." });
        }

        if (u.RefreshTokens != null)
            foreach (var t in u.RefreshTokens.Where(t => t.RevokedAt == null)) t.RevokedAt = DateTime.UtcNow;

        var callerId = int.TryParse(User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value, out var cid) ? cid : (int?)null;

        u.IsDeleted = true;
        u.IsActive = false;
        u.DeletedAt = DateTime.UtcNow;
        u.DeletedByUserId = callerId;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.DeleteUser,
            ResourceType = "SystemUser",
            ResourceId = u.Email,
            OldValue = u.Role?.ToString(),
            NewValue = "SoftDeleted",
            Result = AuditResult.Success,
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "User deleted." });
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<PagedResult<AuditLogListItemDto>>> GetAuditLogs(
        [FromQuery] string? search, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? entityType, [FromQuery] string? result,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var query = _db.AuditLogs.AsQueryable();
        // SystemUsers_Backend_Gaps_Report.md, item 1 - searches user / action / resourceId,
        // same fields the frontend was already trying to filter locally.
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a =>
                (a.UserDisplayName != null && a.UserDisplayName.Contains(search)) ||
                (a.ResourceId != null && a.ResourceId.Contains(search)));
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.ResourceType != null && a.ResourceType == entityType);
        if (!string.IsNullOrWhiteSpace(result))
            query = query.Where(a => a.Result != null && a.Result.ToString() == result);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 5 : pageSize)
            .Select(a => new AuditLogListItemDto
            {
                Id = a.Id,
                LogCode = a.LogCode,
                User = a.UserDisplayName,
                Role = a.Role.ToString(),
                Action = a.Action.ToString(),
                ResourceType = a.ResourceType,
                ResourceId = a.ResourceId,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                Result = a.Result.ToString(),
                IpAddress = a.IpAddress,
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<AuditLogListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // Returns the SAME shape as the list (AuditLogListItemDto) - already includes the full
    // OldValue/NewValue/Result/IpAddress/ResourceId fields, just narrowed to one row by Id.
    [HttpGet("audit-logs/{id:int}")]
    public async Task<ActionResult<AuditLogListItemDto>> GetAuditLogById(int id)
    {
        var a = await _db.AuditLogs.FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound(new { message = "Audit log entry not found." });
        return Ok(new AuditLogListItemDto
        {
            Id = a.Id,
            LogCode = a.LogCode,
            User = a.UserDisplayName,
            Role = a.Role?.ToString(),
            Action = a.Action?.ToString(),
            ResourceType = a.ResourceType,
            ResourceId = a.ResourceId,
            OldValue = a.OldValue,
            NewValue = a.NewValue,
            Result = a.Result?.ToString(),
            IpAddress = a.IpAddress,
            CreatedAt = a.CreatedAt
        });
    }
    // Note: intentionally no PUT/DELETE endpoints for audit logs - they are read-only by design.
}
```

# FILE: src\EgyMediChain.Api\Controllers\AlertsController.cs
```
using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer,FactoryUser,WarehouseUser,PharmacyUser")]
public class AlertsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AlertsController(AppDbContext db) => _db = db;

    [HttpGet("counts")]
    public async Task<ActionResult<AlertsCountsDto>> GetCounts()
    {
        var scope = GetMinistryEntityScope();
        var alertsQuery = _db.Alerts.AsQueryable();
        if (scope != null)
            alertsQuery = alertsQuery.Where(a => a.EntityType != null && a.EntityType.ToString() == scope);

        return Ok(new AlertsCountsDto
        {
            OpenAlerts = await alertsQuery.CountAsync(a => a.AlertStatus == AlertStatus.Open),
            PublicScanLogs = scope == null ? await _db.PublicVerificationScans.CountAsync() : 0,
            RecallAlerts = await alertsQuery.CountAsync(a => a.AlertType == AlertType.Recall)
        });
    }

    // ---------------- Open Alerts ----------------
    // Default pageSize raised from 5 -> 50 (Alerts_Backend_Gaps_Report.md, item 3): the frontend
    // currently loads everything once and paginates/filters client-side, so a tiny default was
    // silently truncating the dashboard to 5 rows. Cap stays at 200 to avoid an unbounded query.
    [HttpGet]
    public async Task<ActionResult<PagedResult<AlertListItemDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? severity, [FromQuery] string? entityType,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
        var query = _db.Alerts.Include(a => a.Batch).AsQueryable();

        // Explicit filter from the frontend's dropdown - only narrows further within whatever
        // this account's Ministry scope already allows.
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType != null && a.EntityType.ToString() == entityType);

        var scope = GetMinistryEntityScope();
        if (scope != null)
            query = query.Where(a => a.EntityType != null && a.EntityType.ToString() == scope);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.AlertStatus != null && a.AlertStatus.ToString() == status);
        if (!string.IsNullOrWhiteSpace(severity))
            query = query.Where(a => a.Severity != null && a.Severity.ToString() == severity);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize)
            .Select(a => new AlertListItemDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                EntityName = a.EntityName,
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<AlertListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // severity/status filters added server-side (Alerts_Backend_Gaps_Report.md, item 4) - previously
    // this only supported page/pageSize, forcing the frontend to filter locally.
    [HttpGet("recalls")]
    public async Task<ActionResult<PagedResult<AlertListItemDto>>> GetRecalls(
        [FromQuery] string? severity, [FromQuery] string? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
        var query = _db.Alerts.Include(a => a.Batch).Where(a => a.AlertType == AlertType.Recall);

        var scope = GetMinistryEntityScope();
        if (scope != null)
            query = query.Where(a => a.EntityType != null && a.EntityType.ToString() == scope);

        if (!string.IsNullOrWhiteSpace(severity))
            query = query.Where(a => a.Severity != null && a.Severity.ToString() == severity);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.AlertStatus != null && a.AlertStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize)
            .Select(a => new AlertListItemDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                EntityName = a.EntityName,
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt
            }).ToListAsync();
        return Ok(new PagedResult<AlertListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlertListItemDto>> GetById(int id)
    {
        var a = await _db.Alerts.Include(x => x.Batch).FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound(new { message = "Alert not found." });
        if (!IsAllowedAlertEntityType(a.EntityType)) return Forbid403();

        return Ok(new AlertListItemDto
        {
            Id = a.Id,
            AlertCode = a.AlertCode,
            AlertType = a.AlertType?.ToString(),
            Severity = a.Severity?.ToString(),
            EntityType = a.EntityType?.ToString(),
            EntityName = a.EntityName,
            BatchNumber = a.Batch?.BatchNumber,
            Message = a.Message,
            AlertStatus = a.AlertStatus?.ToString(),
            CreatedAt = a.CreatedAt
        });
    }

    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAlertStatusDto? dto)
    {
        var a = await _db.Alerts.FindAsync(id);
        if (a == null) return NotFound(new { message = "Alert not found." });
        if (!IsAllowedAlertEntityType(a.EntityType)) return Forbid403();

        var old = a.AlertStatus?.ToString();
        var newStatus = (dto?.Status ?? "UnderReview") switch
        {
            "Resolved" => AlertStatus.Resolved,
            "Dismissed" => AlertStatus.Dismissed,
            "Open" => AlertStatus.Open,
            _ => AlertStatus.UnderReview
        };
        a.AlertStatus = newStatus;
        if (newStatus is AlertStatus.Resolved or AlertStatus.Dismissed) a.ResolvedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = newStatus == AlertStatus.Resolved ? AuditAction.ResolveAlert : AuditAction.DismissAlert,
            ResourceType = "Alert",
            ResourceId = a.AlertCode,
            OldValue = old,
            NewValue = newStatus.ToString(),
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Alert status updated.", status = newStatus.ToString() });
    }

    // Bulk status update for multiple alerts at once (Alerts_Backend_Gaps_Report.md, item 7).
    [HttpPost("bulk-status")]
    public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkUpdateAlertStatusDto? dto)
    {
        if (dto?.AlertIds == null || dto.AlertIds.Count == 0)
            return BadRequest(new { message = "At least one alertId is required." });

        var newStatus = (dto.Status ?? "UnderReview") switch
        {
            "Resolved" => AlertStatus.Resolved,
            "Dismissed" => AlertStatus.Dismissed,
            "Open" => AlertStatus.Open,
            _ => AlertStatus.UnderReview
        };

        var alerts = await _db.Alerts.Where(a => dto.AlertIds.Contains(a.Id)).ToListAsync();
        var allowed = alerts.Where(a => IsAllowedAlertEntityType(a.EntityType)).ToList();

        foreach (var a in allowed)
        {
            a.AlertStatus = newStatus;
            if (newStatus is AlertStatus.Resolved or AlertStatus.Dismissed) a.ResolvedAt = DateTime.UtcNow;

            _db.AuditLogs.Add(new AuditLog
            {
                LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                UserDisplayName = "Dr. Saif",
                Role = SystemRole.SuperAdmin,
                Action = newStatus == AlertStatus.Resolved ? AuditAction.ResolveAlert : AuditAction.DismissAlert,
                ResourceType = "Alert",
                ResourceId = a.AlertCode,
                NewValue = newStatus.ToString(),
                IpAddress = "127.0.0.1",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = $"{allowed.Count} alert(s) updated.", updatedCount = allowed.Count, skippedCount = alerts.Count - allowed.Count });
    }

    // Ministry only - narrower than the rest of this controller (Factory/Warehouse/Pharmacy users
    // can view/update their own alerts but shouldn't be able to delete them). Only lets you delete
    // alerts that are already Resolved or Dismissed, so an active/open alert can never be erased.
    [Authorize(Roles = "SuperAdmin,MinistryAdmin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.Alerts.FindAsync(id);
        if (a == null) return NotFound(new { message = "Alert not found." });
        if (!IsAllowedAlertEntityType(a.EntityType)) return Forbid403();

        if (a.AlertStatus != AlertStatus.Resolved && a.AlertStatus != AlertStatus.Dismissed)
            return BadRequest(new { message = "Only Resolved or Dismissed alerts can be deleted." });

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.DeleteAlert,
            ResourceType = "Alert",
            ResourceId = a.AlertCode,
            OldValue = a.AlertStatus?.ToString(),
            NewValue = "Deleted",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        _db.Alerts.Remove(a);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Alert deleted." });
    }

    // ---------------- Public Scan Logs ----------------
    [HttpGet("public-scans")]
    public async Task<ActionResult<PagedResult<ScanListItemDto>>> GetPublicScans(
        [FromQuery] string? result, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
        var query = _db.PublicVerificationScans.AsQueryable();
        if (!string.IsNullOrWhiteSpace(result))
            query = query.Where(s => s.VerificationResult != null && s.VerificationResult.ToString() == result);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.ScannedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize)
            .Select(s => new ScanListItemDto
            {
                Id = s.Id,
                ScanCode = s.ScanCode,
                ScannedGTIN = s.ScannedGTIN,
                ScannedSerialNumber = s.ScannedSerialNumber,
                ScannedBatchNumber = s.ScannedBatchNumber,
                ProductName = s.ProductName,
                VerificationResult = s.VerificationResult.ToString(),
                Reason = s.Reason,
                Governorate = s.Governorate,
                City = s.City,
                ScannedAt = s.ScannedAt
            }).ToListAsync();

        return Ok(new PagedResult<ScanListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("public-scans/{id:int}")]
    public async Task<ActionResult<ScanDetailsDto>> GetScanDetails(int id)
    {
        var s = await _db.PublicVerificationScans
            .Include(x => x.UnitCode).ThenInclude(u => u!.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(x => x.UnitCode).ThenInclude(u => u!.Batch).ThenInclude(b => b!.Factory)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (s == null) return NotFound(new { message = "Scan not found." });

        var dto = new ScanDetailsDto
        {
            Id = s.Id,
            ScannedGTIN = s.ScannedGTIN,
            ScannedSerialNumber = s.ScannedSerialNumber,
            ScannedBatchNumber = s.ScannedBatchNumber,
            VerificationResult = s.VerificationResult?.ToString(),
            Reason = s.Reason,
            Governorate = s.Governorate,
            City = s.City,
            ScannedAt = s.ScannedAt,
            UnitCodeId = s.UnitCodeId
        };

        if (s.UnitCode != null)
        {
            dto.ProductName = s.UnitCode.Batch?.MedicineProduct?.ProductName;
            dto.BatchNumber = s.UnitCode.Batch?.BatchNumber;
            dto.FactoryName = s.UnitCode.Batch?.Factory?.OfficialFactoryName;
            dto.UnitStatus = s.UnitCode.UnitStatus?.ToString();
            dto.ScanCount = s.UnitCode.ScanCount;
            dto.FirstScannedAt = s.UnitCode.FirstScannedAt;
        }

        return Ok(dto);
    }

    [HttpPost("public-scans/{id:int}/create-alert")]
    public async Task<IActionResult> CreateAlertFromScan(int id, [FromBody] CreateAlertFromScanDto? dto)
    {
        var s = await _db.PublicVerificationScans.FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return NotFound(new { message = "Scan not found." });

        var severity = (dto?.Severity ?? "High") switch
        {
            "Critical" => AlertSeverity.Critical,
            "Medium" => AlertSeverity.Medium,
            "Low" => AlertSeverity.Low,
            _ => AlertSeverity.High
        };

        var alert = new Alert
        {
            AlertCode = $"ALERT-{DateTime.UtcNow:yyyyMMddHHmmss}",
            AlertType = AlertType.SuspiciousScan,
            Severity = severity,
            EntityType = EntityKind.Pharmacy,
            EntityName = "Public Scan",
            Message = dto?.Message ?? $"Suspicious scan reported for batch {s.ScannedBatchNumber}.",
            AlertStatus = AlertStatus.Open,
            CreatedAt = DateTime.UtcNow
        };
        _db.Alerts.Add(alert);

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.CreateAlert,
            ResourceType = "PublicVerificationScan",
            ResourceId = s.ScanCode,
            OldValue = null,
            NewValue = "AlertCreated",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Alert created from scan.", alertCode = alert.AlertCode });
    }

    // Only restricts MinistryAdmin/MinistryViewer accounts that were created with an EntityScope
    // (see AdminController.AddMinistryAdmin). SuperAdmin and the operational roles (FactoryUser/
    // WarehouseUser/PharmacyUser) are left exactly as before - unscoped for this controller.
    private string? GetMinistryEntityScope()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "MinistryAdmin" && role != "MinistryViewer") return null;

        var scope = User.FindFirst("entityType")?.Value;
        if (string.IsNullOrEmpty(scope) || scope == "Ministry") return null;
        return scope;
    }

    private bool IsAllowedAlertEntityType(EntityKind? type)
    {
        var scope = GetMinistryEntityScope();
        if (scope == null) return true;
        return type != null && type.ToString() == scope;
    }

    private ObjectResult Forbid403() => new(new { message = "This account's Ministry scope doesn't include this entity type." }) { StatusCode = 403 };
}

```

# FILE: src\EgyMediChain.Api\Controllers\AuthController.cs
```
using EgyMediChain.Api.Common;
using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly IConfiguration _config;
    private readonly IEmailService _email;

    public AuthController(AppDbContext db, JwtTokenService jwt, IConfiguration config, IEmailService email)
    {
        _db = db;
        _jwt = jwt;
        _config = config;
        _email = email;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Email and password are required." });

        // ยง5.1 - lock an account for a short cool-down after 5 consecutive failed attempts in
        // the last 15 minutes, same rate-limit pattern used for the Forgot Password flow.
        var lockoutWindow = DateTime.UtcNow.AddMinutes(-15);
        var recentFailures = await _db.AuditLogs.CountAsync(l =>
            l.Action == AuditAction.LoginFailed && l.ResourceId == MaskEmail(dto.Email) && l.CreatedAt >= lockoutWindow);
        if (recentFailures >= 5)
            return StatusCode(429, new { message = "Too many failed login attempts. Please try again in 15 minutes." });

        var user = await _db.SystemUsers.FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _db.AuditLogs.Add(new AuditLog
            {
                LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                Action = AuditAction.LoginFailed,
                ResourceType = "SystemUser",
                ResourceId = MaskEmail(dto.Email),
                Result = AuditResult.Failed,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (user.IsActive != true)
            return Unauthorized(new { message = "This account is not active yet. Wait for Ministry approval." });
        if (user.IsSuspended == true)
            return Unauthorized(new { message = "This account has been suspended." });

        user.LastLoginAt = DateTime.UtcNow;
        var response = await IssueTokensAsync(user);
        await _db.SaveChangesAsync();

        return Ok(response);
    }

    // Body: { "refreshToken": "..." }
    // Rotates the refresh token: the old one is revoked and a brand new pair (access + refresh) is issued.
    // This replaces the old /refresh, which just handed back a SuperAdmin token to anyone who called it.
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponseDto>> Refresh([FromBody] RefreshRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.RefreshToken))
            return BadRequest(new { message = "Refresh token is required." });

        var stored = await _db.AuthRefreshTokens
            .Include(t => t.SystemUser)
            .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken);

        if (stored == null || stored.RevokedAt != null || stored.ExpiresAt == null || stored.ExpiresAt <= DateTime.UtcNow)
            return Unauthorized(new { message = "Refresh token is invalid or expired. Please log in again." });

        if (stored.SystemUser == null || stored.SystemUser.IsActive != true)
            return Unauthorized(new { message = "Account is not active." });
        if (stored.SystemUser.IsSuspended == true)
            return Unauthorized(new { message = "This account has been suspended." });

        stored.RevokedAt = DateTime.UtcNow;
        var response = await IssueTokensAsync(stored.SystemUser);
        await _db.SaveChangesAsync();

        return Ok(response);
    }

    // Body: { "refreshToken": "..." }
    // Logs the user out on this device by revoking that one refresh token (their access token
    // keeps working until it naturally expires - that's normal for stateless JWTs).
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.RefreshToken))
            return BadRequest(new { message = "Refresh token is required." });

        var stored = await _db.AuthRefreshTokens.FirstOrDefaultAsync(t => t.Token == dto.RefreshToken);
        if (stored != null && stored.RevokedAt == null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Logged out." });
    }

    // ---------------- Forgot Password flow (Backend Action Report ยง2) ----------------
    // Design principle: impossible to use for account enumeration. Every branch below returns
    // the exact same 200 + generic body regardless of whether the email exists, is suspended, etc.

    [HttpPost("forgot-password")]
    public async Task<ActionResult<GenericMessageResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            return BadRequest(new { message = "A valid email is required." });

        var generic = new GenericMessageResponseDto { Message = "If an account exists for this email, a reset code has been sent." };

        // Lightweight DB-backed rate limit (no rate-limiting middleware wired up yet in this
        // project - ยง5.1/ยง2.4 flag this as infra worth adding generically; this is the
        // self-contained version scoped to just this table for now).
        var since = DateTime.UtcNow.AddHours(-1);
        var recentByEmail = await _db.PasswordResetRequests.CountAsync(r => r.RequestedEmail == dto.Email && r.CreatedAt >= since);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var recentByIp = ip == null ? 0 : await _db.PasswordResetRequests.CountAsync(r => r.RequestIp == ip && r.CreatedAt >= since);
        if (recentByEmail >= 5 || recentByIp >= 20)
            return StatusCode(429, new { message = "Too many requests. Please try again later." });

        var user = await _db.SystemUsers.FirstOrDefaultAsync(u => u.Email == dto.Email || u.OfficialEmail == dto.Email || u.PersonalEmail == dto.Email);

        if (user != null && user.IsActive == true && user.IsSuspended != true)
        {
            var otp = Random.Shared.Next(0, 1_000_000).ToString("D6");
            _db.PasswordResetRequests.Add(new PasswordResetRequest
            {
                UserId = user.Id,
                RequestedEmail = dto.Email,
                HashedOtp = HashSecret(otp),
                Attempts = 0,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                RequestIp = ip
            });
            await _db.SaveChangesAsync();

            // No email service is wired up in this project yet (Backend Action Report ยง5.4).
            // Logged here so the flow is testable end-to-end locally until Brevo/SMTP is added.
            // Fire-and-forget - don't let email latency/failure block the response (ยง2.1 step 5,
            // ยง5.4). Falls back to a console log automatically if Brevo isn't configured yet
            // (see BrevoEmailService).
            _ = _email.SendAsync(dto.Email, "Your EgyMediChain password reset code",
                $"<p>Your one-time verification code is:</p><h2 style=\"letter-spacing:4px\">{otp}</h2><p>This code expires in 10 minutes. If you didn't request this, you can safely ignore this email.</p>");
        }
        else
        {
            // Equivalent-cost dummy work so a timing side-channel can't distinguish this branch.
            _ = HashSecret(Guid.NewGuid().ToString());
        }

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            Action = AuditAction.ForgotPasswordRequested,
            ResourceType = "SystemUser",
            ResourceId = MaskEmail(dto.Email),
            Result = AuditResult.Success,
            IpAddress = ip,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return Ok(generic);
    }

    [HttpPost("verify-reset-code")]
    public async Task<ActionResult<VerifyResetCodeResponseDto>> VerifyResetCode([FromBody] VerifyResetCodeRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code) ||
            !System.Text.RegularExpressions.Regex.IsMatch(dto.Code, @"^\d{6}$"))
            return BadRequest(new { message = "A valid email and 6-digit code are required." });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var since = DateTime.UtcNow.AddHours(-1);
        var recentAttempts = await _db.PasswordResetRequests.CountAsync(r => r.RequestedEmail == dto.Email && r.CreatedAt >= since);
        if (recentAttempts >= 10)
            return StatusCode(429, new { message = "Too many attempts. Please request a new code." });

        var invalid = new ObjectResult(new { message = "Invalid or expired code." }) { StatusCode = 401 };

        var request = await _db.PasswordResetRequests
            .Where(r => r.RequestedEmail == dto.Email && r.ConsumedAt == null && r.VerifiedAt == null)
            .OrderByDescending(r => r.CreatedAt).FirstOrDefaultAsync();

        LogVerifyAttempt(dto.Email, ip, false);

        if (request == null || request.ExpiresAt == null || request.ExpiresAt <= DateTime.UtcNow || request.Attempts >= 5)
        {
            await _db.SaveChangesAsync();
            return invalid;
        }

        if (request.HashedOtp != HashSecret(dto.Code))
        {
            request.Attempts += 1;
            await _db.SaveChangesAsync();
            return invalid;
        }

        var resetToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        request.ResetTokenHash = HashSecret(resetToken);
        request.VerifiedAt = DateTime.UtcNow;
        request.ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(10);

        // Update the last log entry we just added to reflect success.
        var lastLog = await _db.AuditLogs.Where(l => l.Action == AuditAction.PasswordResetVerifyAttempted && l.ResourceId == MaskEmail(dto.Email))
            .OrderByDescending(l => l.CreatedAt).FirstOrDefaultAsync();
        if (lastLog != null) lastLog.Result = AuditResult.Success;

        await _db.SaveChangesAsync();

        return Ok(new VerifyResetCodeResponseDto { ResetToken = resetToken, ExpiresAt = request.ResetTokenExpiresAt });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<GenericMessageResponseDto>> ResetPasswordWithToken([FromBody] ResetPasswordWithTokenDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.ResetToken) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "resetToken and newPassword are required." });

        if (dto.NewPassword.Length < 8 || !dto.NewPassword.Any(char.IsUpper) || !dto.NewPassword.Any(char.IsLower) || !dto.NewPassword.Any(char.IsDigit))
            return BadRequest(new { message = "Password must be at least 8 characters and include upper, lower case letters and a digit." });

        var hashedToken = HashSecret(dto.ResetToken);
        var request = await _db.PasswordResetRequests.Include(r => r.User).ThenInclude(u => u!.RefreshTokens)
            .FirstOrDefaultAsync(r => r.ResetTokenHash == hashedToken && r.ConsumedAt == null);

        if (request == null || request.User == null || request.ResetTokenExpiresAt == null || request.ResetTokenExpiresAt <= DateTime.UtcNow)
            return Unauthorized(new { message = "This reset link is invalid or has expired." });

        var user = request.User;
        if (!string.IsNullOrEmpty(user.PasswordHash) && BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
            return BadRequest(new { message = "New password must be different from your current password." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, 12);
        user.UpdatedAt = DateTime.UtcNow;
        request.ConsumedAt = DateTime.UtcNow;

        if (user.RefreshTokens != null)
            foreach (var t in user.RefreshTokens.Where(t => t.RevokedAt == null)) t.RevokedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Action = AuditAction.PasswordResetCompleted,
            ResourceType = "SystemUser",
            ResourceId = user.Email,
            Result = AuditResult.Success,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // No email service wired up yet - see note in ForgotPassword above.
        var changeNotice = "<p>Your EgyMediChain account password was just changed.</p><p>If this wasn't you, contact your Ministry SuperAdmin immediately - all your active sessions have been signed out as a precaution.</p>";
        if (!string.IsNullOrWhiteSpace(user.OfficialEmail ?? user.Email))
            _ = _email.SendAsync(user.OfficialEmail ?? user.Email!, "Your EgyMediChain password was changed", changeNotice);
        if (!string.IsNullOrWhiteSpace(user.PersonalEmail))
            _ = _email.SendAsync(user.PersonalEmail!, "Your EgyMediChain password was changed", changeNotice);

        return Ok(new GenericMessageResponseDto { Message = "Password reset successfully. Please sign in with your new password." });
    }

    private void LogVerifyAttempt(string email, string? ip, bool success) =>
        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            Action = AuditAction.PasswordResetVerifyAttempted,
            ResourceType = "SystemUser",
            ResourceId = MaskEmail(email),
            Result = success ? AuditResult.Success : AuditResult.Failed,
            IpAddress = ip,
            CreatedAt = DateTime.UtcNow
        });

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return "***" + email[Math.Max(0, at)..];
        return email[0] + new string('*', at - 1) + email[at..];
    }

    // HMAC-SHA256 with a server-side pepper (reuses the existing Jwt:Key secret rather than
    // introducing a second secret into configuration) - used for OTPs and reset tokens, never
    // stored in plaintext.
    private string HashSecret(string value)
    {
        var pepper = _config.GetSection("Jwt")["Key"] ?? "fallback-pepper-change-me";
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(pepper));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private async Task<LoginResponseDto> IssueTokensAsync(SystemUser user)
    {
        var refreshToken = _jwt.GenerateRefreshToken();

        _db.AuthRefreshTokens.Add(new AuthRefreshToken
        {
            SystemUserId = user.Id,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        return new LoginResponseDto
        {
            Token = _jwt.GenerateAccessToken(user),
            RefreshToken = refreshToken,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role?.ToString(),
            EntityType = user.EntityType?.ToString(),
            EntityId = user.EntityId
        };
    }
}
```

# FILE: src\EgyMediChain.Api\Controllers\BatchesController.cs
```
using EgyMediChain.Api.Dtos;
using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/batches")]
// Fixed: this controller has no {factoryId} route param and queries ALL factories' batches
// nationally - it's a Ministry-wide view, not a per-factory one (FactoryUser already has their
// own batches via FactoryDashboardController, which IS properly scoped to their own factory).
// Letting FactoryUser call this too would have let them see every other factory's batches.
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
[RequireMinistryScope("Factory")]
public class BatchesController : ControllerBase
{
    private readonly AppDbContext _db;
    public BatchesController(AppDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<ActionResult<BatchesSummaryDto>> GetSummary()
    {
        return Ok(new BatchesSummaryDto
        {
            TotalBatches = await _db.Batches.CountAsync(),
            InProduction = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.InProduction),
            InSupplyChain = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.InSupplyChain),
            InWarehouses = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.InWarehouse),
            InPharmacies = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.InPharmacy),
            Quarantined = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.Quarantined),
            Recalled = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.Recalled),
            OpenAlerts = await _db.Alerts.CountAsync(a => a.AlertStatus == AlertStatus.Open)
        });
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<BatchListItemDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? factory, [FromQuery] string? batchStatus,
        [FromQuery] string? stage, [FromQuery] string? dosageForm,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Batches.Include(b => b.MedicineProduct).Include(b => b.Factory).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b =>
                (b.BatchNumber != null && b.BatchNumber.Contains(search)) ||
                (b.MedicineProduct != null && b.MedicineProduct.ProductName != null && b.MedicineProduct.ProductName.Contains(search)) ||
                (b.MedicineProduct != null && b.MedicineProduct.GTIN != null && b.MedicineProduct.GTIN.Contains(search)));

        if (!string.IsNullOrWhiteSpace(factory))
            query = query.Where(b => b.Factory != null && b.Factory.OfficialFactoryName == factory);

        if (!string.IsNullOrWhiteSpace(batchStatus))
            query = query.Where(b => b.BatchStatus != null && b.BatchStatus.ToString() == batchStatus);

        if (!string.IsNullOrWhiteSpace(stage))
            query = query.Where(b => b.SupplyChainStage != null && b.SupplyChainStage.ToString() == stage);

        // Real server-side filter (MedicineBatchDashboard-Backend-Gaps.md, item 2) - previously
        // only applied client-side on the current page, which silently missed matches on other pages.
        if (!string.IsNullOrWhiteSpace(dosageForm))
            query = query.Where(b => b.MedicineProduct != null && b.MedicineProduct.DosageForm == dosageForm);

        var total = await query.CountAsync();
        var items = await query.OrderBy(b => b.Id)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(b => new BatchListItemDto
            {
                Id = b.Id,
                ProductName = b.MedicineProduct != null ? b.MedicineProduct.ProductName : null,
                GTIN = b.MedicineProduct != null ? b.MedicineProduct.GTIN : null,
                DosageForm = b.MedicineProduct != null ? b.MedicineProduct.DosageForm : null,
                Strength = b.MedicineProduct != null ? b.MedicineProduct.Strength : null,
                BatchNumber = b.BatchNumber,
                FactoryName = b.Factory != null ? b.Factory.OfficialFactoryName : null,
                Quantity = b.Quantity,
                ExpiryDate = b.ExpiryDate,
                BatchStatus = b.BatchStatus.ToString(),
                SupplyChainStage = b.SupplyChainStage.ToString(),
                CurrentLocation = b.CurrentLocation,
                OpenAlerts = b.OpenAlertsCount,
                RequiresColdChain = b.MedicineProduct != null ? b.MedicineProduct.RequiresColdChain : null,
                UpdatedAt = b.UpdatedAt
            }).ToListAsync();

        return Ok(new PagedResult<BatchListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BatchDetailsDto>> GetById(int id)
    {
        var b = await _db.Batches
            .Include(x => x.MedicineProduct)
            .Include(x => x.Factory)
            .Include(x => x.Shipments)
            .Include(x => x.InventoryStocks)
            .Include(x => x.Alerts)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (b == null) return NotFound(new { message = "Batch not found." });

        var dto = new BatchDetailsDto
        {
            Id = b.Id,
            ProductInfo = new ProductInfoDto
            {
                ProductName = b.MedicineProduct?.ProductName,
                GTIN = b.MedicineProduct?.GTIN,
                DosageForm = b.MedicineProduct?.DosageForm,
                Strength = b.MedicineProduct?.Strength,
                RequiresColdChain = b.MedicineProduct?.RequiresColdChain,
                ProductStatus = b.MedicineProduct?.ProductStatus
            },
            BatchInfo = new BatchInfoDto
            {
                BatchNumber = b.BatchNumber,
                FactoryName = b.Factory?.OfficialFactoryName,
                Quantity = b.Quantity,
                ManufacturingDate = b.ManufacturingDate,
                ExpiryDate = b.ExpiryDate,
                BatchStatus = b.BatchStatus?.ToString(),
                SupplyChainStage = b.SupplyChainStage?.ToString(),
                CreatedBy = b.CreatedBy,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            },
            UnitCodesSummary = new UnitCodesSummaryDto
            {
                TotalUnitCodes = b.TotalUnitCodes,
                GeneratedCount = b.GeneratedUnitCodes,
                InWarehouseCount = b.InWarehouseUnitCodes,
                InPharmacyCount = b.InPharmacyUnitCodes,
                SuspiciousCount = b.SuspiciousUnitCodes,
                BlockedCount = b.BlockedUnitCodes,
                RecalledCount = b.RecalledUnitCodes,
                ScanCountTotal = b.ScanCountTotal
            },
            Shipments = b.Shipments?.Select(s => new ShipmentSummaryItemDto
            {
                TransferCode = s.TransferCode,
                ShipmentType = s.ShipmentType?.ToString(),
                Source = s.Source,
                Destination = s.Destination,
                ExpectedQuantity = s.ExpectedQuantity,
                ReceivedQuantity = s.ReceivedQuantity,
                ShipmentStatus = s.ShipmentStatus?.ToString(),
                DispatchDate = s.DispatchDate,
                ReceivedDate = s.ReceivedDate
            }).ToList(),
            InventoryDistribution = b.InventoryStocks?.Select(i => new InventoryDistributionItemDto
            {
                HolderType = i.HolderType,
                HolderName = i.HolderName,
                TotalReceivedQuantity = i.TotalReceivedQuantity,
                AvailableQuantity = i.AvailableQuantity,
                ReservedQuantity = i.ReservedQuantity,
                QuarantinedQuantity = i.QuarantinedQuantity,
                InventoryStatus = i.InventoryStatus?.ToString(),
                LastUpdated = i.LastUpdated
            }).ToList(),
            RelatedAlerts = b.Alerts?.Select(a => new RelatedAlertItemDto
            {
                AlertType = a.AlertType?.ToString(),
                Severity = a.Severity?.ToString(),
                Message = a.Message,
                AlertStatus = a.AlertStatus?.ToString(),
                CreatedAt = a.CreatedAt,
                ResolvedAt = a.ResolvedAt
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost("{id:int}/freeze")]
    public async Task<IActionResult> Freeze(int id)
    {
        var b = await _db.Batches.Include(x => x.UnitCodes).Include(x => x.InventoryStocks).FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound(new { message = "Batch not found." });

        var old = b.BatchStatus?.ToString();
        b.BatchStatus = BatchStatus.Quarantined;
        b.UpdatedAt = DateTime.UtcNow;
        if (b.UnitCodes != null) foreach (var u in b.UnitCodes) u.UnitStatus = UnitStatus.Blocked;
        if (b.InventoryStocks != null) foreach (var i in b.InventoryStocks) i.InventoryStatus = InventoryStatus.Blocked;

        _db.Alerts.Add(new Alert
        {
            AlertCode = $"ALERT-{DateTime.UtcNow:yyyyMMddHHmmss}",
            AlertType = AlertType.ComplianceIssue,
            Severity = AlertSeverity.High,
            EntityType = EntityKind.Factory,
            EntityName = b.Factory?.OfficialFactoryName,
            BatchId = b.Id,
            Message = $"Batch {b.BatchNumber} has been frozen by the Ministry pending investigation.",
            AlertStatus = AlertStatus.Open,
            CreatedAt = DateTime.UtcNow
        });

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.FreezeBatch,
            ResourceType = "Batch",
            ResourceId = b.BatchNumber,
            OldValue = old,
            NewValue = "Quarantined",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Batch frozen (quarantined).", status = "Quarantined" });
    }

    // Reverses a freeze done in error. Restores the status the batch had right before it was
    // frozen (read from the last FreezeBatch audit log entry for this batch), falling back to
    // InSupplyChain if no such log is found. Only works while the batch is still Quarantined -
    // won't touch a batch that's since been Recalled through a separate action.
    [HttpPost("{id:int}/unfreeze")]
    public async Task<IActionResult> Unfreeze(int id)
    {
        var b = await _db.Batches.Include(x => x.UnitCodes).Include(x => x.InventoryStocks).FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound(new { message = "Batch not found." });
        if (b.BatchStatus != BatchStatus.Quarantined)
            return Conflict(new { message = "Only a Quarantined batch can be unfrozen." });

        var lastFreezeLog = await _db.AuditLogs
            .Where(l => l.Action == AuditAction.FreezeBatch && l.ResourceId == b.BatchNumber)
            .OrderByDescending(l => l.CreatedAt).FirstOrDefaultAsync();

        var restoredStatus = Enum.TryParse<BatchStatus>(lastFreezeLog?.OldValue, out var parsed) ? parsed : BatchStatus.InSupplyChain;

        b.BatchStatus = restoredStatus;
        b.UpdatedAt = DateTime.UtcNow;
        if (b.UnitCodes != null) foreach (var u in b.UnitCodes.Where(u => u.UnitStatus == UnitStatus.Blocked)) u.UnitStatus = UnitStatus.InWarehouse;
        if (b.InventoryStocks != null) foreach (var i in b.InventoryStocks.Where(i => i.InventoryStatus == InventoryStatus.Blocked)) i.InventoryStatus = InventoryStatus.Active;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.FreezeBatch,
            ResourceType = "Batch",
            ResourceId = b.BatchNumber,
            OldValue = "Quarantined",
            NewValue = restoredStatus.ToString(),
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Batch unfrozen.", status = restoredStatus.ToString() });
    }

    [HttpPost("{id:int}/create-recall-alert")]
    public async Task<IActionResult> CreateRecallAlert(int id, [FromBody] CreateRecallAlertDto? dto)
    {
        var b = await _db.Batches.Include(x => x.UnitCodes).Include(x => x.InventoryStocks).Include(x => x.Factory).FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound(new { message = "Batch not found." });

        var old = b.BatchStatus?.ToString();
        b.BatchStatus = BatchStatus.Recalled;
        b.UpdatedAt = DateTime.UtcNow;
        if (b.UnitCodes != null) foreach (var u in b.UnitCodes) u.UnitStatus = UnitStatus.Recalled;
        if (b.InventoryStocks != null) foreach (var i in b.InventoryStocks) i.InventoryStatus = InventoryStatus.Recalled;

        _db.Alerts.Add(new Alert
        {
            AlertCode = $"ALERT-{DateTime.UtcNow:yyyyMMddHHmmss}",
            AlertType = AlertType.Recall,
            Severity = AlertSeverity.Critical,
            EntityType = EntityKind.Factory,
            EntityName = b.Factory?.OfficialFactoryName,
            BatchId = b.Id,
            Message = dto?.Message ?? $"Batch {b.BatchNumber} has been recalled by the Ministry.",
            AlertStatus = AlertStatus.Open,
            CreatedAt = DateTime.UtcNow
        });

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.CreateRecallAlert,
            ResourceType = "Batch",
            ResourceId = b.BatchNumber,
            OldValue = old,
            NewValue = "Recalled",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Recall alert created.", status = "Recalled" });
    }
}

```

# FILE: src\EgyMediChain.Api\Controllers\DocumentsController.cs
```
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// Documents can contain National ID copies etc., so downloads go through an authenticated
// endpoint rather than only the raw /uploads/... static URL (that static URL still works too,
// since Program.cs has UseStaticFiles(), but this is the one the frontend should actually use).
[ApiController]
[Route("api/documents")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public DocumentsController(AppDbContext db) => _db = db;

    [HttpGet("{documentId:int}/download")]
    public async Task<IActionResult> Download(int documentId)
    {
        var doc = await _db.EntityDocuments.FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc == null || string.IsNullOrWhiteSpace(doc.FileUrl))
            return NotFound(new { message = "Document not found." });

        // FileUrl looks like "/uploads/REQ-2026-1234/xxx.pdf" - map it back to the physical path
        // under wwwroot (see RegistrationRequestsController.Submit, where it's written).
        var relativePath = doc.FileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

        if (!System.IO.File.Exists(fullPath))
            return NotFound(new { message = "File is missing on disk." });

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fullPath, out var contentType))
            contentType = "application/octet-stream";

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        var downloadName = doc.FileName ?? Path.GetFileName(fullPath);
        return File(bytes, contentType, downloadName);
    }
}
```

# FILE: src\EgyMediChain.Api\Controllers\FactoriesController.cs
```
using EgyMediChain.Api.Dtos;
using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/factories")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
[RequireMinistryScope("Factory")]
public class FactoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public FactoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<FactoryListItemDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var query = _db.Factories.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f => f.OfficialFactoryName != null && f.OfficialFactoryName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(f => f.FactoryStatus != null && f.FactoryStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderBy(f => f.Id)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 5 : pageSize)
            .Select(f => new FactoryListItemDto
            {
                Id = f.Id,
                FactoryName = f.OfficialFactoryName,
                LegalCompanyName = f.LegalCompanyName,
                Governorate = f.Governorate,
                City = f.City,
                LicenseExpiryDate = f.LicenseExpiryDate,
                HasColdStorage = f.HasColdStorage,
                HasQualityControlLab = f.HasQualityControlLab,
                FactoryStatus = f.FactoryStatus.ToString(),
                TotalBatches = f.TotalBatches,
                CreatedAt = f.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<FactoryListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FactoryProfileDto>> GetById(int id)
    {
        var f = await _db.Factories.FirstOrDefaultAsync(x => x.Id == id);
        if (f == null) return NotFound(new { message = "Factory not found." });

        return Ok(new FactoryProfileDto
        {
            Id = f.Id,
            OfficialFactoryName = f.OfficialFactoryName,
            LegalCompanyName = f.LegalCompanyName,
            DosageFormsProduced = f.DosageFormsProduced,
            Governorate = f.Governorate,
            City = f.City,
            DistrictArea = f.DistrictArea,
            FullAddress = f.FullAddress,
            FactoryLicenseNumber = f.FactoryLicenseNumber,
            TechnicalOperatingLicenseNumber = f.TechnicalOperatingLicenseNumber,
            CommercialRegistrationNumber = f.CommercialRegistrationNumber,
            TaxCardNumber = f.TaxCardNumber,
            LicenseIssueDate = f.LicenseIssueDate,
            LicenseExpiryDate = f.LicenseExpiryDate,
            HasQualityControlLab = f.HasQualityControlLab,
            HasFinishedGoodsStore = f.HasFinishedGoodsStore,
            HasColdStorage = f.HasColdStorage,
            HasQuarantineArea = f.HasQuarantineArea,
            FactoryStatus = f.FactoryStatus?.ToString(),
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        });
    }

    [HttpPost("{id:int}/suspend")]
    public async Task<IActionResult> Suspend(int id, [FromBody] EntityActionDto? dto)
    {
        var f = await _db.Factories.FindAsync(id);
        if (f == null) return NotFound(new { message = "Factory not found." });
        var old = f.FactoryStatus?.ToString();
        f.FactoryStatus = FacilityStatus.Suspended;
        f.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SuspendEntity, "Factory", f.FactoryLicenseNumber, old, "Suspended", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Factory suspended.", status = "Suspended" });
    }

    [HttpPost("{id:int}/reactivate")]
    public async Task<IActionResult> Reactivate(int id, [FromBody] EntityActionDto? dto)
    {
        var f = await _db.Factories.FindAsync(id);
        if (f == null) return NotFound(new { message = "Factory not found." });
        var old = f.FactoryStatus?.ToString();
        f.FactoryStatus = FacilityStatus.Active;
        f.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.ReactivateEntity, "Factory", f.FactoryLicenseNumber, old, "Active", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Factory reactivated.", status = "Active" });
    }

    [HttpPost("{id:int}/set-inactive")]
    public async Task<IActionResult> SetInactive(int id, [FromBody] EntityActionDto? dto)
    {
        var f = await _db.Factories.FindAsync(id);
        if (f == null) return NotFound(new { message = "Factory not found." });
        var old = f.FactoryStatus?.ToString();
        f.FactoryStatus = FacilityStatus.Inactive;
        f.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SetInactiveEntity, "Factory", f.FactoryLicenseNumber, old, "Inactive", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Factory set to inactive.", status = "Inactive" });
    }

    [HttpGet("{id:int}/batches")]
    public async Task<ActionResult<PagedResult<BatchListItemDto>>> GetBatches(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Batches.Include(b => b.MedicineProduct).Include(b => b.Factory).Where(b => b.FactoryId == id);
        var total = await query.CountAsync();
        var items = await query.Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(b => new BatchListItemDto
            {
                Id = b.Id,
                ProductName = b.MedicineProduct != null ? b.MedicineProduct.ProductName : null,
                GTIN = b.MedicineProduct != null ? b.MedicineProduct.GTIN : null,
                DosageForm = b.MedicineProduct != null ? b.MedicineProduct.DosageForm : null,
                Strength = b.MedicineProduct != null ? b.MedicineProduct.Strength : null,
                BatchNumber = b.BatchNumber,
                FactoryName = b.Factory != null ? b.Factory.OfficialFactoryName : null,
                Quantity = b.Quantity,
                ExpiryDate = b.ExpiryDate,
                BatchStatus = b.BatchStatus.ToString(),
                SupplyChainStage = b.SupplyChainStage.ToString(),
                CurrentLocation = b.CurrentLocation,
                OpenAlerts = b.OpenAlertsCount
            }).ToListAsync();
        return Ok(new PagedResult<BatchListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}/registration-request")]
    public async Task<ActionResult<EntityRegistrationRequestRefDto>> GetRegistrationRequest(int id)
    {
        var r = await _db.RegistrationRequests
            .Where(x => x.FactoryId == id)
            .OrderByDescending(x => x.SubmittedAt)
            .FirstOrDefaultAsync();
        if (r == null) return NotFound(new { message = "No registration request found for this factory." });

        return Ok(new EntityRegistrationRequestRefDto
        {
            Id = r.Id,
            RequestCode = r.RequestCode,
            RegistrationStatus = r.RegistrationStatus?.ToString(),
            SubmittedAt = r.SubmittedAt
        });
    }

    // Same filters as GetAll (search/status), returns a CSV file (opens fine in Excel).
    // No paging - exports everything matching the filters.
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? search, [FromQuery] string? status)
    {
        var query = _db.Factories.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f => f.OfficialFactoryName != null && f.OfficialFactoryName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(f => f.FactoryStatus != null && f.FactoryStatus.ToString() == status);

        var rows = await query.OrderBy(f => f.Id).ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,FactoryName,LegalCompanyName,Governorate,City,LicenseExpiryDate,HasColdStorage,HasQualityControlLab,Status,TotalBatches,CreatedAt");
        foreach (var f in rows)
        {
            sb.AppendLine(string.Join(",",
                f.Id,
                Csv(f.OfficialFactoryName), Csv(f.LegalCompanyName), Csv(f.Governorate), Csv(f.City),
                f.LicenseExpiryDate?.ToString("yyyy-MM-dd"), f.HasColdStorage, f.HasQualityControlLab,
                Csv(f.FactoryStatus?.ToString()), f.TotalBatches, f.CreatedAt?.ToString("yyyy-MM-dd")));
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"factories-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Contains(',') || value.Contains('"')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    private static AuditLog Log(AuditAction action, string resourceType, string? resourceId, string? oldVal, string? newVal, string? reason = null) => new()
    {
        LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
        UserDisplayName = "Dr. Saif",
        Role = SystemRole.SuperAdmin,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        // Reason is appended here since AuditLog has no dedicated Reason column
        // (EntitiesManagement-Backend-Gaps.md, item 3).
        NewValue = string.IsNullOrWhiteSpace(reason) ? newVal : $"{newVal} (Reason: {reason})",
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}

```

# FILE: src\EgyMediChain.Api\Controllers\FactoryDashboardController.cs
```
using EgyMediChain.Api.Dtos;
using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// Operational portal for a single Factory (role: FactoryUser). Everything is scoped to
// {factoryId} in the route rather than pulled from the JWT, matching the rest of this API's
// "keep it simple, low-friction" style - the frontend gets {factoryId} back from /api/auth/login.
[ApiController]
[Route("api/factory-dashboard/{factoryId:int}")]
[Authorize(Roles = "FactoryUser,SuperAdmin,MinistryAdmin")]
[ValidateEntityOwnership("factoryId")]
public class FactoryDashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public FactoryDashboardController(AppDbContext db) => _db = db;

    private async Task<Factory?> GetFactoryAsync(int factoryId) => await _db.Factories.FindAsync(factoryId);

    private static bool IsActive(Factory f) => f.FactoryStatus == FacilityStatus.Active;

    // ---------------- Overview ----------------
    [HttpGet("overview")]
    public async Task<ActionResult<FactoryOverviewDto>> GetOverview(int factoryId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var batchesQuery = _db.Batches.Where(b => b.FactoryId == factoryId);
        var shipmentsQuery = _db.Shipments.Where(s => s.SourceFactoryId == factoryId);

        var cards = new FactoryOverviewCardsDto
        {
            TotalBatches = await batchesQuery.CountAsync(),
            ReadyForDispatch = await batchesQuery.CountAsync(b => b.BatchStatus == BatchStatus.ReadyForWarehouseDispatch || b.BatchStatus == BatchStatus.PartiallyDispatched),
            UnitCodesGenerated = await batchesQuery.SumAsync(b => b.GeneratedUnitCodes ?? 0),
            ShipmentsInTransit = await shipmentsQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit || s.ShipmentStatus == ShipmentStatus.PendingInspection),
            ReceivedByWarehouses = await shipmentsQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Received || s.ShipmentStatus == ShipmentStatus.PartiallyReceived),
            OpenAlerts = await _db.Alerts.CountAsync(a => a.EntityType == EntityKind.Factory && a.EntityName == factory.OfficialFactoryName && a.AlertStatus == AlertStatus.Open)
        };

        var recentBatches = await batchesQuery.Include(b => b.MedicineProduct).Include(b => b.Factory)
            .OrderByDescending(b => b.UpdatedAt).Take(5)
            .Select(b => new BatchListItemDto
            {
                Id = b.Id,
                ProductName = b.MedicineProduct != null ? b.MedicineProduct.ProductName : null,
                GTIN = b.MedicineProduct != null ? b.MedicineProduct.GTIN : null,
                BatchNumber = b.BatchNumber,
                Quantity = b.Quantity,
                ExpiryDate = b.ExpiryDate,
                BatchStatus = b.BatchStatus.ToString(),
                SupplyChainStage = b.SupplyChainStage.ToString(),
                CurrentLocation = b.CurrentLocation,
                OpenAlerts = b.OpenAlertsCount,
                UnitCodesCount = b.GeneratedUnitCodes,
                AvailableForDispatch = b.BatchStatus == BatchStatus.ReadyForWarehouseDispatch || b.BatchStatus == BatchStatus.PartiallyDispatched
            }).ToListAsync();

        var recentShipments = await shipmentsQuery.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .OrderByDescending(s => s.DispatchDate).Take(5)
            .Select(s => ToShipmentListItem(s)).ToListAsync();

        var openAlerts = await _db.Alerts.Where(a => a.EntityType == EntityKind.Factory && a.EntityName == factory.OfficialFactoryName && a.AlertStatus == AlertStatus.Open)
            .Include(a => a.Batch).OrderByDescending(a => a.CreatedAt).Take(5)
            .Select(a => new AlertListItemDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                EntityName = a.EntityName,
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(new FactoryOverviewDto { Cards = cards, RecentBatches = recentBatches, RecentShipments = recentShipments, OpenAlerts = openAlerts });
    }

    // ---------------- Batch Management ----------------
    [HttpGet("batches")]
    public async Task<ActionResult<PagedResult<BatchListItemDto>>> GetBatches(int factoryId,
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Batches.Include(b => b.MedicineProduct).Include(b => b.Factory).Where(b => b.FactoryId == factoryId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => (b.BatchNumber != null && b.BatchNumber.Contains(search)) ||
                                      (b.MedicineProduct != null && b.MedicineProduct.ProductName != null && b.MedicineProduct.ProductName.Contains(search)));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.BatchStatus != null && b.BatchStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(b => b.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(b => new BatchListItemDto
            {
                Id = b.Id,
                ProductName = b.MedicineProduct != null ? b.MedicineProduct.ProductName : null,
                GTIN = b.MedicineProduct != null ? b.MedicineProduct.GTIN : null,
                DosageForm = b.MedicineProduct != null ? b.MedicineProduct.DosageForm : null,
                Strength = b.MedicineProduct != null ? b.MedicineProduct.Strength : null,
                BatchNumber = b.BatchNumber,
                Quantity = b.Quantity,
                ExpiryDate = b.ExpiryDate,
                BatchStatus = b.BatchStatus.ToString(),
                SupplyChainStage = b.SupplyChainStage.ToString(),
                CurrentLocation = b.CurrentLocation,
                OpenAlerts = b.OpenAlertsCount,
                UnitCodesCount = b.GeneratedUnitCodes,
                AvailableForDispatch = b.BatchStatus == BatchStatus.ReadyForWarehouseDispatch || b.BatchStatus == BatchStatus.PartiallyDispatched,
                RequiresColdChain = b.MedicineProduct != null ? b.MedicineProduct.RequiresColdChain : null
            }).ToListAsync();

        return Ok(new PagedResult<BatchListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("batches/{batchId:int}")]
    public async Task<ActionResult<BatchDetailsDto>> GetBatchDetails(int factoryId, int batchId)
    {
        var b = await _db.Batches.Include(x => x.MedicineProduct).Include(x => x.Factory)
            .Include(x => x.Shipments).Include(x => x.Alerts)
            .FirstOrDefaultAsync(x => x.Id == batchId && x.FactoryId == factoryId);
        if (b == null) return NotFound(new { message = "Batch not found for this factory." });

        return Ok(new BatchDetailsDto
        {
            Id = b.Id,
            ProductInfo = new ProductInfoDto
            {
                ProductName = b.MedicineProduct?.ProductName,
                GTIN = b.MedicineProduct?.GTIN,
                DosageForm = b.MedicineProduct?.DosageForm,
                Strength = b.MedicineProduct?.Strength,
                RequiresColdChain = b.MedicineProduct?.RequiresColdChain,
                ProductStatus = b.MedicineProduct?.ProductStatus
            },
            BatchInfo = new BatchInfoDto
            {
                BatchNumber = b.BatchNumber,
                FactoryName = b.Factory?.OfficialFactoryName,
                Quantity = b.Quantity,
                ManufacturingDate = b.ManufacturingDate,
                ExpiryDate = b.ExpiryDate,
                BatchStatus = b.BatchStatus?.ToString(),
                SupplyChainStage = b.SupplyChainStage?.ToString(),
                CreatedBy = b.CreatedBy,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            },
            UnitCodesSummary = new UnitCodesSummaryDto
            {
                TotalUnitCodes = b.TotalUnitCodes,
                GeneratedCount = b.GeneratedUnitCodes,
                InWarehouseCount = b.InWarehouseUnitCodes,
                InPharmacyCount = b.InPharmacyUnitCodes,
                SuspiciousCount = b.SuspiciousUnitCodes,
                BlockedCount = b.BlockedUnitCodes,
                RecalledCount = b.RecalledUnitCodes,
                ScanCountTotal = b.ScanCountTotal
            },
            Shipments = b.Shipments?.Select(s => new ShipmentSummaryItemDto
            {
                TransferCode = s.TransferCode,
                ShipmentType = s.ShipmentType?.ToString(),
                Source = s.Source,
                Destination = s.Destination,
                ExpectedQuantity = s.ExpectedQuantity,
                ReceivedQuantity = s.ReceivedQuantity,
                ShipmentStatus = s.ShipmentStatus?.ToString(),
                DispatchDate = s.DispatchDate,
                ReceivedDate = s.ReceivedDate
            }).ToList(),
            RelatedAlerts = b.Alerts?.Select(a => new RelatedAlertItemDto
            {
                AlertType = a.AlertType?.ToString(),
                Severity = a.Severity?.ToString(),
                Message = a.Message,
                AlertStatus = a.AlertStatus?.ToString(),
                CreatedAt = a.CreatedAt,
                ResolvedAt = a.ResolvedAt
            }).ToList()
        });
    }

    // Real system-of-record unit codes for a batch (Backend_Action_Report_Factory_Portal, item 1.2).
    // Replaces the frontend's client-side fabricated CSV export - this is the actual generated data.
    [HttpGet("batches/{batchId:int}/unit-codes")]
    public async Task<ActionResult<PagedResult<UnitCodeListItemDto>>> GetUnitCodes(int factoryId, int batchId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });

        if (batch.BatchStatus != BatchStatus.CodesGenerated &&
            batch.BatchStatus != BatchStatus.ReadyForWarehouseDispatch &&
            batch.BatchStatus != BatchStatus.PartiallyDispatched &&
            batch.BatchStatus != BatchStatus.FullyDispatched)
            return Conflict(new { message = "Unit codes are only available once they've been generated for this batch." });

        var query = _db.UnitCodes.Where(u => u.BatchId == batchId);
        var total = await query.CountAsync();
        var items = await query.OrderBy(u => u.Id)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 100 : pageSize)
            .Select(u => new UnitCodeListItemDto
            {
                Id = u.Id,
                GTIN = u.GTIN,
                SerialNumber = u.SerialNumber,
                CodeValueHash = u.CodeValueHash,
                ExpiryDate = u.ExpiryDate,
                UnitStatus = u.UnitStatus.ToString()
            }).ToListAsync();

        return Ok(new PagedResult<UnitCodeListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // Factory-scoped warehouse directory for dispatch destination selection
    // (Backend_Action_Report_Factory_Portal, item 1.4). Only Active warehouses, so the frontend
    // can show real names + cold-storage capability instead of a blind numeric ID field.
    [HttpGet("warehouses")]
    public async Task<ActionResult<PagedResult<WarehouseOptionDto>>> GetWarehousesForDispatch(int factoryId,
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _db.Warehouses.Where(w => w.WarehouseStatus == FacilityStatus.Active);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(w => w.OfficialWarehouseName != null && w.OfficialWarehouseName.Contains(search));

        var total = await query.CountAsync();
        var items = await query.OrderBy(w => w.OfficialWarehouseName)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 50 : pageSize)
            .Select(w => new WarehouseOptionDto
            {
                Id = w.Id,
                WarehouseName = w.OfficialWarehouseName,
                Governorate = w.Governorate,
                City = w.City,
                HasColdStorage = w.HasColdStorage,
                WarehouseStatus = w.WarehouseStatus.ToString()
            }).ToListAsync();

        return Ok(new PagedResult<WarehouseOptionDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // Alert status counts scoped to this factory, independent of list pagination
    // (Backend_Action_Report_Factory_Portal, item 1.5).
    [HttpGet("alerts/counts")]
    public async Task<ActionResult<FactoryAlertsCountsDto>> GetAlertsCounts(int factoryId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var query = _db.Alerts.Where(a => a.EntityType == EntityKind.Factory && a.EntityName == factory.OfficialFactoryName);
        return Ok(new FactoryAlertsCountsDto
        {
            Open = await query.CountAsync(a => a.AlertStatus == AlertStatus.Open),
            UnderReview = await query.CountAsync(a => a.AlertStatus == AlertStatus.UnderReview),
            Resolved = await query.CountAsync(a => a.AlertStatus == AlertStatus.Resolved),
            Dismissed = await query.CountAsync(a => a.AlertStatus == AlertStatus.Dismissed),
            Total = await query.CountAsync()
        });
    }

    // Edit a Draft batch in place (Backend_Action_Report_Factory_Portal, item 1.1).
    [HttpPut("batches/{batchId:int}")]
    [HttpPatch("batches/{batchId:int}")]
    public async Task<ActionResult<BatchListItemDto>> UpdateBatch(int factoryId, int batchId, [FromBody] UpdateBatchDto? dto)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        var batch = await _db.Batches.Include(b => b.MedicineProduct).FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });
        if (batch.BatchStatus != BatchStatus.Draft)
            return Conflict(new { message = "Only a Draft batch can be edited." });

        if (dto?.ExpiryDate != null && dto.ManufacturingDate != null && dto.ExpiryDate <= dto.ManufacturingDate)
            return BadRequest(new { message = "Expiry date must be after manufacturing date." });
        if (dto?.Quantity != null && dto.Quantity <= 0)
            return BadRequest(new { message = "Quantity must be greater than 0." });

        if (batch.MedicineProduct != null)
        {
            if (dto?.ProductName != null) batch.MedicineProduct.ProductName = dto.ProductName;
            if (dto?.GTIN != null) batch.MedicineProduct.GTIN = dto.GTIN;
            if (dto?.DosageForm != null) batch.MedicineProduct.DosageForm = dto.DosageForm;
            if (dto?.Strength != null) batch.MedicineProduct.Strength = dto.Strength;
            if (dto?.RequiresColdChain != null) batch.MedicineProduct.RequiresColdChain = dto.RequiresColdChain;
        }

        if (dto?.BatchNumber != null) batch.BatchNumber = dto.BatchNumber;
        if (dto?.Quantity != null) batch.Quantity = dto.Quantity;
        if (dto?.ManufacturingDate != null) batch.ManufacturingDate = dto.ManufacturingDate;
        if (dto?.ExpiryDate != null) batch.ExpiryDate = dto.ExpiryDate;
        if (dto?.Notes != null) batch.Notes = dto.Notes;
        batch.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new BatchListItemDto
        {
            Id = batch.Id,
            ProductName = batch.MedicineProduct?.ProductName,
            GTIN = batch.MedicineProduct?.GTIN,
            DosageForm = batch.MedicineProduct?.DosageForm,
            Strength = batch.MedicineProduct?.Strength,
            BatchNumber = batch.BatchNumber,
            Quantity = batch.Quantity,
            ManufacturingDate = batch.ManufacturingDate,
            ExpiryDate = batch.ExpiryDate,
            BatchStatus = batch.BatchStatus.ToString(),
            SupplyChainStage = batch.SupplyChainStage.ToString(),
            RequiresColdChain = batch.MedicineProduct?.RequiresColdChain
        });
    }

    [HttpPost("batches")]
    public async Task<ActionResult<BatchListItemDto>> CreateBatch(int factoryId, [FromBody] CreateBatchDto? dto)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        // Reuse an existing MedicineProduct by GTIN, otherwise create a new one.
        MedicineProduct? product = null;
        if (!string.IsNullOrWhiteSpace(dto?.GTIN))
            product = await _db.MedicineProducts.FirstOrDefaultAsync(p => p.GTIN == dto!.GTIN);

        if (product == null)
        {
            product = new MedicineProduct
            {
                ProductName = dto?.ProductName ?? "Unnamed Product",
                GTIN = dto?.GTIN ?? $"AUTO-{DateTime.UtcNow.Ticks}",
                DosageForm = dto?.DosageForm,
                Strength = dto?.Strength,
                RequiresColdChain = dto?.RequiresColdChain ?? false,
                ProductStatus = "Active"
            };
            _db.MedicineProducts.Add(product);
        }

        var isDraft = dto?.SaveAsDraft == true;
        var batch = new Batch
        {
            MedicineProduct = product,
            FactoryId = factoryId,
            Factory = factory,
            BatchNumber = string.IsNullOrWhiteSpace(dto?.BatchNumber) ? $"BAT-{DateTime.UtcNow:yyyyMMddHHmmss}" : dto!.BatchNumber,
            Quantity = dto?.Quantity,
            ManufacturingDate = dto?.ManufacturingDate,
            ExpiryDate = dto?.ExpiryDate,
            Notes = dto?.Notes,
            BatchStatus = isDraft ? BatchStatus.Draft : BatchStatus.Registered,
            SupplyChainStage = SupplyChainStage.AtFactory,
            CurrentLocation = factory.OfficialFactoryName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TotalUnitCodes = 0,
            GeneratedUnitCodes = 0
        };
        _db.Batches.Add(batch);

        _db.AuditLogs.Add(NewLog(AuditAction.CreateBatch, "Batch", batch.BatchNumber, null, batch.BatchStatus.ToString()));
        await _db.SaveChangesAsync();

        return Ok(new BatchListItemDto
        {
            Id = batch.Id,
            ProductName = product.ProductName,
            GTIN = product.GTIN,
            BatchNumber = batch.BatchNumber,
            Quantity = batch.Quantity,
            ExpiryDate = batch.ExpiryDate,
            BatchStatus = batch.BatchStatus.ToString(),
            SupplyChainStage = batch.SupplyChainStage.ToString()
        });
    }

    [HttpPost("batches/{batchId:int}/generate-codes")]
    public async Task<IActionResult> GenerateCodes(int factoryId, int batchId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });
        if (batch.BatchStatus != BatchStatus.Registered)
            return Conflict(new { message = "Codes can only be generated for a batch with status 'Registered'." });

        var qty = batch.Quantity ?? 0;
        batch.TotalUnitCodes = qty;
        batch.GeneratedUnitCodes = qty;
        batch.BatchStatus = BatchStatus.CodesGenerated;
        batch.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(AuditAction.GenerateCodes, "Batch", batch.BatchNumber, "Registered", "CodesGenerated"));
        await _db.SaveChangesAsync();
        return Ok(new { message = $"{qty} unit codes generated.", status = "CodesGenerated", unitCodesGenerated = qty });
    }

    [HttpPost("batches/{batchId:int}/mark-ready")]
    public async Task<IActionResult> MarkReady(int factoryId, int batchId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });
        if (batch.BatchStatus != BatchStatus.CodesGenerated)
            return Conflict(new { message = "Only a batch with status 'CodesGenerated' can be marked ready for dispatch." });

        batch.BatchStatus = BatchStatus.ReadyForWarehouseDispatch;
        batch.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(AuditAction.MarkBatchReadyForDispatch, "Batch", batch.BatchNumber, "CodesGenerated", "ReadyForWarehouseDispatch"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Batch marked ready for warehouse dispatch.", status = "ReadyForWarehouseDispatch" });
    }

    [HttpPost("batches/{batchId:int}/cancel-draft")]
    public async Task<IActionResult> CancelDraft(int factoryId, int batchId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });
        if (batch.BatchStatus != BatchStatus.Draft)
            return Conflict(new { message = "Only a Draft batch can be cancelled." });

        batch.BatchStatus = BatchStatus.Cancelled;
        batch.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(AuditAction.CancelDraftBatch, "Batch", batch.BatchNumber, "Draft", "Cancelled"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Draft batch cancelled.", status = "Cancelled" });
    }

    // Only Draft or already-Cancelled batches can be hard-deleted - those are the only states
    // guaranteed to have no generated unit codes, shipments, or inventory pointing at them.
    [HttpDelete("batches/{batchId:int}")]
    public async Task<IActionResult> DeleteBatch(int factoryId, int batchId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });

        if (batch.BatchStatus != BatchStatus.Draft && batch.BatchStatus != BatchStatus.Cancelled)
            return BadRequest(new { message = "Only a Draft or Cancelled batch can be deleted." });

        _db.AuditLogs.Add(NewLog(AuditAction.DeleteDraftBatch, "Batch", batch.BatchNumber, batch.BatchStatus?.ToString(), "Deleted"));
        _db.Batches.Remove(batch);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Batch deleted." });
    }

    // ---------------- Shipments (Factory -> Warehouse) ----------------
    [HttpGet("shipments/summary")]
    public async Task<ActionResult<FactoryShipmentsSummaryDto>> GetShipmentsSummary(int factoryId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var query = _db.Shipments.Where(s => s.SourceFactoryId == factoryId);
        return Ok(new FactoryShipmentsSummaryDto
        {
            TotalShipments = await query.CountAsync(),
            InTransit = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit),
            Received = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Received),
            PartiallyReceived = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.PartiallyReceived),
            Rejected = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Rejected),
            Cancelled = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Cancelled)
        });
    }

    [HttpGet("shipments")]
    public async Task<ActionResult<PagedResult<ShipmentListItemDto>>> GetShipments(int factoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct).Where(s => s.SourceFactoryId == factoryId);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.DispatchDate)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(s => ToShipmentListItem(s)).ToListAsync();
        return Ok(new PagedResult<ShipmentListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("shipments/{shipmentId:int}")]
    public async Task<ActionResult<ShipmentDetailsDto>> GetShipmentDetails(int factoryId, int shipmentId)
    {
        var s = await _db.Shipments.Include(x => x.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(x => x.Id == shipmentId && x.SourceFactoryId == factoryId);
        if (s == null) return NotFound(new { message = "Shipment not found for this factory." });

        return Ok(new ShipmentDetailsDto
        {
            Id = s.Id,
            TransferCode = s.TransferCode,
            ShipmentType = s.ShipmentType?.ToString(),
            Source = s.Source,
            Destination = s.Destination,
            ProductName = s.Batch?.MedicineProduct?.ProductName,
            BatchNumber = s.Batch?.BatchNumber,
            ExpectedQuantity = s.ExpectedQuantity,
            ReceivedQuantity = s.ReceivedQuantity,
            ShipmentStatus = s.ShipmentStatus?.ToString(),
            RequiresColdChain = s.RequiresColdChain,
            DispatchDate = s.DispatchDate,
            ReceivedDate = s.ReceivedDate,
            Notes = s.Notes,
            InspectionResult = s.InspectionResult
        });
    }

    [HttpPost("shipments")]
    public async Task<IActionResult> CreateDispatch(int factoryId, [FromBody] CreateDispatchDto? dto)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        var batch = await _db.Batches.Include(b => b.MedicineProduct).FirstOrDefaultAsync(b => b.Id == dto!.BatchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });

        if (batch.BatchStatus is BatchStatus.Quarantined or BatchStatus.Recalled or BatchStatus.Expired or BatchStatus.Cancelled)
            return Conflict(new { message = $"This batch cannot be dispatched while its status is {batch.BatchStatus}." });

        if (batch.BatchStatus != BatchStatus.ReadyForWarehouseDispatch && batch.BatchStatus != BatchStatus.PartiallyDispatched)
            return Conflict(new { message = "Batch must be ReadyForWarehouseDispatch (or PartiallyDispatched with remaining quantity) before dispatch." });

        var warehouse = await _db.Warehouses.FindAsync(dto?.DestinationWarehouseId ?? 0);
        if (warehouse == null || warehouse.WarehouseStatus != FacilityStatus.Active)
            return BadRequest(new { message = "Please select an active destination warehouse." });

        if (batch.MedicineProduct?.RequiresColdChain == true && warehouse.HasColdStorage != true)
            return Conflict(new { message = "This batch requires cold storage. Please select a warehouse with cold storage capability." });

        var alreadyDispatched = await _db.Shipments.Where(s => s.BatchId == batch.Id && s.SourceFactoryId == factoryId)
            .SumAsync(s => s.ExpectedQuantity ?? 0);
        var remaining = (batch.Quantity ?? 0) - alreadyDispatched;
        var dispatchQty = dto?.DispatchQuantity ?? 0;
        if (dispatchQty <= 0 || dispatchQty > remaining)
            return BadRequest(new { message = $"Dispatch quantity must be between 1 and the available quantity ({remaining})." });

        var shipment = new Shipment
        {
            TransferCode = $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}",
            BatchId = batch.Id,
            Batch = batch,
            ShipmentType = ShipmentType.FactoryToWarehouse,
            Source = factory.OfficialFactoryName,
            Destination = warehouse.OfficialWarehouseName,
            SourceFactoryId = factoryId,
            DestinationWarehouseId = warehouse.Id,
            ExpectedQuantity = dispatchQty,
            ShipmentStatus = ShipmentStatus.InTransit,
            RequiresColdChain = batch.MedicineProduct?.RequiresColdChain ?? false,
            Notes = dto?.Notes,
            DispatchDate = dto?.DispatchDate ?? DateTime.UtcNow
        };
        _db.Shipments.Add(shipment);

        batch.BatchStatus = (remaining - dispatchQty) <= 0 ? BatchStatus.FullyDispatched : BatchStatus.PartiallyDispatched;
        batch.SupplyChainStage = SupplyChainStage.InTransit;
        batch.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(AuditAction.DispatchShipment, "Shipment", shipment.TransferCode, "ReadyForWarehouseDispatch", batch.BatchStatus.ToString()));
        await _db.SaveChangesAsync();

        return Ok(new { message = "Shipment dispatched to warehouse.", transferCode = shipment.TransferCode, batchStatus = batch.BatchStatus.ToString() });
    }

    // ---------------- Alerts (view-only) ----------------
    [HttpGet("alerts")]
    public async Task<ActionResult<PagedResult<AlertListItemDto>>> GetAlerts(int factoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var query = _db.Alerts.Include(a => a.Batch).ThenInclude(b => b!.MedicineProduct).Include(a => a.Shipment)
            .Where(a => a.EntityType == EntityKind.Factory && a.EntityName == factory.OfficialFactoryName);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(a => new AlertListItemDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                EntityName = a.EntityName,
                ProductName = a.Batch != null && a.Batch.MedicineProduct != null ? a.Batch.MedicineProduct.ProductName : null,
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                ShipmentTransferCode = a.Shipment != null ? a.Shipment.TransferCode : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt,
                ResolvedAt = a.ResolvedAt
            }).ToListAsync();

        return Ok(new PagedResult<AlertListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("alerts/{alertId:int}")]
    public async Task<ActionResult<AlertDetailsDto>> GetAlertDetails(int factoryId, int alertId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var a = await _db.Alerts.Include(x => x.Batch).ThenInclude(b => b!.MedicineProduct).Include(x => x.Shipment)
            .FirstOrDefaultAsync(x => x.Id == alertId && x.EntityType == EntityKind.Factory && x.EntityName == factory.OfficialFactoryName);
        if (a == null) return NotFound(new { message = "Alert not found for this factory." });

        var isImpactful = a.AlertType is AlertType.Recall or AlertType.ComplianceIssue;
        return Ok(new AlertDetailsDto
        {
            Id = a.Id,
            AlertCode = a.AlertCode,
            AlertType = a.AlertType?.ToString(),
            Severity = a.Severity?.ToString(),
            EntityType = a.EntityType?.ToString(),
            EntityName = a.EntityName,
            Message = a.Message,
            ProductName = a.Batch?.MedicineProduct?.ProductName,
            BatchNumber = a.Batch?.BatchNumber,
            BatchId = a.BatchId,
            ShipmentTransferCode = a.Shipment?.TransferCode,
            ShipmentId = a.ShipmentId,
            AlertStatus = a.AlertStatus?.ToString(),
            CreatedAt = a.CreatedAt,
            ResolvedAt = a.ResolvedAt,
            ImpactedBatchStatus = isImpactful ? a.Batch?.BatchStatus?.ToString() : null,
            ImpactedUnitCodesStatus = a.AlertType == AlertType.Recall ? "Recalled" : a.AlertType == AlertType.ComplianceIssue ? "Blocked" : null,
            ImpactedInventoryStatus = a.AlertType == AlertType.Recall ? "Recalled" : a.AlertType == AlertType.ComplianceIssue ? "Blocked" : null,
            BatchDispatchBlocked = isImpactful
        });
    }

    // ---------------- Profile ----------------
    [HttpGet("profile")]
    public async Task<ActionResult<FactoryProfileFullDto>> GetProfile(int factoryId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var user = await _db.SystemUsers.FirstOrDefaultAsync(u => u.EntityType == EntityKind.Factory && u.EntityId == factoryId);
        var docs = await _db.EntityDocuments
            .Where(d => d.RegistrationRequest != null && d.RegistrationRequest.FactoryId == factoryId)
            .Select(d => new DocumentItemDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                UploadedAt = d.UploadedAt,
                DocumentStatus = d.DocumentStatus.ToString(),
                ReviewedBy = d.ReviewedBy,
                ReviewedAt = d.ReviewedAt,
                RejectionReason = d.RejectionReason
            }).ToListAsync();

        var licenses = await _db.EntityLicenses
            .Where(l => l.EntityType == EntityKind.Factory && l.EntityId == factoryId)
            .Select(l => new LicenseItemDto
            {
                Id = l.Id,
                LicenseType = l.LicenseType,
                LicenseNumber = l.LicenseNumber,
                IssueDate = l.IssueDate,
                ExpiryDate = l.ExpiryDate,
                Status = l.Status,
                FileUrl = l.FileUrl
            }).ToListAsync();

        return Ok(new FactoryProfileFullDto
        {
            Id = factory.Id,
            FactoryName = factory.OfficialFactoryName,
            FactoryCode = factory.FactoryCode,
            FactoryStatus = factory.FactoryStatus?.ToString(),
            RegistrationStatus = "Approved",
            MemberSince = factory.CreatedAt,
            Account = user == null ? null : new AccountInfoDto
            {
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                NationalIdMasked = MaskNationalId(user.NationalId),
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive
            },
            Entity = new EntityInfoDto
            {
                FactoryCode = factory.FactoryCode,
                OfficialFactoryName = factory.OfficialFactoryName,
                LegalCompanyName = factory.LegalCompanyName,
                DosageFormsProduced = factory.DosageFormsProduced,
                Governorate = factory.Governorate,
                City = factory.City,
                DistrictArea = factory.DistrictArea,
                FullAddress = factory.FullAddress,
                Phone = factory.Phone,
                Email = factory.Email,
                FactoryLicenseNumber = factory.FactoryLicenseNumber,
                TechnicalOperatingLicenseNumber = factory.TechnicalOperatingLicenseNumber,
                CommercialRegistrationNumber = factory.CommercialRegistrationNumber,
                TaxCardNumber = factory.TaxCardNumber,
                HasQualityControlLab = factory.HasQualityControlLab,
                HasFinishedGoodsStore = factory.HasFinishedGoodsStore,
                HasColdStorage = factory.HasColdStorage,
                HasQuarantineArea = factory.HasQuarantineArea,
                LicenseIssueDate = factory.LicenseIssueDate,
                LicenseExpiryDate = factory.LicenseExpiryDate,
                Status = factory.FactoryStatus?.ToString()
            },
            FactoryDetails = new FactoryDetailsDto
            {
                EstablishedYear = factory.EstablishedYear,
                TotalProductionLines = factory.TotalProductionLines,
                MainProductionTypes = factory.MainProductionTypes,
                ColdChainCapable = factory.HasColdStorage,
                StorageTypes = factory.StorageTypes,
                QualityCertificates = factory.QualityCertificates,
                Description = factory.Description
            },
            RegistrationInfo = new RegistrationInfoDto
            {
                RegistrationRequestNo = factory.RegistrationRequestNo,
                SubmittedAt = factory.RegistrationSubmittedAt,
                RegistrationStatus = "Approved",
                ApprovedAt = factory.RegistrationApprovedAt,
                ApprovedBy = factory.RegistrationApprovedBy,
                RegistrationExpiryDate = factory.RegistrationExpiryDate,
                Notes = factory.RegistrationNotes
            },
            Licenses = licenses,
            Documents = docs
        });
    }

    private static string? MaskNationalId(string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length < 4) return nationalId;
        return $"**** **** {nationalId[^4..]}";
    }

    private static ShipmentListItemDto ToShipmentListItem(Shipment s) => new()
    {
        Id = s.Id,
        TransferCode = s.TransferCode,
        ProductName = s.Batch?.MedicineProduct?.ProductName,
        GTIN = s.Batch?.MedicineProduct?.GTIN,
        BatchNumber = s.Batch?.BatchNumber,
        Source = s.Source,
        Destination = s.Destination,
        ExpectedQuantity = s.ExpectedQuantity,
        ReceivedQuantity = s.ReceivedQuantity,
        RequiresColdChain = s.RequiresColdChain,
        ShipmentStatus = s.ShipmentStatus?.ToString(),
        DispatchDate = s.DispatchDate,
        ReceivedDate = s.ReceivedDate
    };

    private static AuditLog NewLog(AuditAction action, string resourceType, string? resourceId, string? oldVal, string? newVal) => new()
    {
        LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
        UserDisplayName = "Factory User",
        Role = SystemRole.FactoryUser,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        NewValue = newVal,
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}

```

# FILE: src\EgyMediChain.Api\Controllers\OverviewController.cs
```
using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/overview")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
public class OverviewController : ControllerBase
{
    private readonly AppDbContext _db;
    public OverviewController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<OverviewDto>> Get()
    {
        // null = full Ministry access (SuperAdmin, or a MinistryAdmin/MinistryViewer created
        // without an EntityScope). "Factory"/"Warehouse"/"Pharmacy" = this account was created
        // scoped to just that entity type (see AdminController.AddMinistryAdmin) and the whole
        // dashboard below only shows numbers relevant to that type.
        var scope = GetMinistryEntityScope();

        var cards = new OverviewCardsDto
        {
            PendingRequests = await _db.RegistrationRequests.CountAsync(r =>
                r.RegistrationStatus == RegistrationStatus.Pending &&
                (scope == null || (r.EntityType != null && r.EntityType.ToString() == scope))),

            ActiveFactories = (scope == null || scope == "Factory")
                ? await _db.Factories.CountAsync(f => f.FactoryStatus == FacilityStatus.Active) : 0,

            ActiveWarehouses = (scope == null || scope == "Warehouse")
                ? await _db.Warehouses.CountAsync(w => w.WarehouseStatus == FacilityStatus.Active) : 0,

            ActivePharmacies = (scope == null || scope == "Pharmacy")
                ? await _db.Pharmacies.CountAsync(p => p.PharmacyStatus == FacilityStatus.Active) : 0,

            // Totals (EntitiesManagement-Backend-Gaps.md, item 4) - so the Entities Management
            // screen doesn't need 3 extra "?pageSize=1" requests just to read totalCount.
            TotalFactories = (scope == null || scope == "Factory") ? await _db.Factories.CountAsync() : 0,
            TotalWarehouses = (scope == null || scope == "Warehouse") ? await _db.Warehouses.CountAsync() : 0,
            TotalPharmacies = (scope == null || scope == "Pharmacy") ? await _db.Pharmacies.CountAsync() : 0,

            // Batches are produced by factories, so this card only means something for an
            // unscoped account or one scoped to Factory.
            ActiveBatches = (scope == null || scope == "Factory")
                ? await _db.Batches.CountAsync(b => b.BatchStatus != BatchStatus.Recalled) : 0,

            ShipmentsInTransit = scope switch
            {
                null => await _db.Shipments.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit),
                "Factory" => await _db.Shipments.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit && s.SourceFactoryId != null),
                "Warehouse" => await _db.Shipments.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit && (s.SourceWarehouseId != null || s.DestinationWarehouseId != null)),
                "Pharmacy" => await _db.Shipments.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit && s.DestinationPharmacyId != null),
                _ => 0
            },

            OpenAlerts = await _db.Alerts.CountAsync(a =>
                a.AlertStatus == AlertStatus.Open &&
                (scope == null || (a.EntityType != null && a.EntityType.ToString() == scope))),

            // Public scans aren't tied to one entity type in this schema, so they're only shown
            // to unscoped Ministry accounts.
            SuspiciousPublicScans = scope == null
                ? await _db.PublicVerificationScans.CountAsync(s => s.VerificationResult == VerificationResult.Suspicious) : 0
        };

        var recentRequestsQuery = _db.RegistrationRequests.AsQueryable();
        if (scope != null) recentRequestsQuery = recentRequestsQuery.Where(r => r.EntityType != null && r.EntityType.ToString() == scope);

        var recentRequests = await recentRequestsQuery
            .OrderByDescending(r => r.SubmittedAt)
            .Take(5)
            .Select(r => new RecentRegistrationRequestDto
            {
                Id = r.Id,
                RequestCode = r.RequestCode,
                EntityType = r.EntityType.ToString(),
                EntityName = r.EntityName,
                SubmittedBy = r.RepresentativeName,
                SubmittedAt = r.SubmittedAt,
                RegistrationStatus = r.RegistrationStatus.ToString()
            }).ToListAsync();

        var recentAlertsQuery = _db.Alerts.Include(a => a.Batch).AsQueryable();
        if (scope != null) recentAlertsQuery = recentAlertsQuery.Where(a => a.EntityType != null && a.EntityType.ToString() == scope);

        var recentAlerts = await recentAlertsQuery
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new RecentAlertDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                CreatedAt = a.CreatedAt,
                AlertStatus = a.AlertStatus.ToString()
            }).ToListAsync();

        // Recent batch activity is inherently a factory-side feed (batches are produced by
        // factories), so a Warehouse/Pharmacy-scoped account just gets an empty list here.
        var recentBatches = (scope == null || scope == "Factory")
            ? await _db.Batches
                .Include(b => b.MedicineProduct)
                .Include(b => b.Factory)
                .OrderByDescending(b => b.UpdatedAt)
                .Take(5)
                .Select(b => new RecentBatchActivityDto
                {
                    Id = b.Id,
                    ProductName = b.MedicineProduct != null ? b.MedicineProduct.ProductName : null,
                    BatchNumber = b.BatchNumber,
                    FactoryName = b.Factory != null ? b.Factory.OfficialFactoryName : null,
                    BatchStatus = b.BatchStatus.ToString(),
                    SupplyChainStage = b.SupplyChainStage.ToString(),
                    LastUpdated = b.UpdatedAt
                }).ToListAsync()
            : new List<RecentBatchActivityDto>();

        return Ok(new OverviewDto
        {
            Cards = cards,
            RecentRegistrationRequests = recentRequests,
            RecentAlerts = recentAlerts,
            RecentBatchActivity = recentBatches
        });
    }

    // Only MinistryAdmin/MinistryViewer accounts created with an EntityScope are restricted here.
    // SuperAdmin always sees the full, unscoped dashboard.
    private string? GetMinistryEntityScope()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "MinistryAdmin" && role != "MinistryViewer") return null;

        var scope = User.FindFirst("entityType")?.Value;
        if (string.IsNullOrEmpty(scope) || scope == "Ministry") return null;
        return scope;
    }
}
```

# FILE: src\EgyMediChain.Api\Controllers\PharmaciesController.cs
```
using EgyMediChain.Api.Dtos;
using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/pharmacies")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
[RequireMinistryScope("Pharmacy")]
public class PharmaciesController : ControllerBase
{
    private readonly AppDbContext _db;
    public PharmaciesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<PharmacyListItemDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var query = _db.Pharmacies.Include(p => p.DefaultWarehouse).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.OfficialPharmacyName != null && p.OfficialPharmacyName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.PharmacyStatus != null && p.PharmacyStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderBy(p => p.Id)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 5 : pageSize)
            .Select(p => new PharmacyListItemDto
            {
                Id = p.Id,
                PharmacyName = p.OfficialPharmacyName,
                PharmacyType = p.PharmacyType,
                Governorate = p.Governorate,
                City = p.City,
                DefaultWarehouse = p.DefaultWarehouse != null ? p.DefaultWarehouse.OfficialWarehouseName : null,
                HasColdStorage = p.HasColdStorage,
                LicenseExpiryDate = p.LicenseExpiryDate,
                PharmacyStatus = p.PharmacyStatus.ToString(),
                CreatedAt = p.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<PharmacyListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PharmacyProfileDto>> GetById(int id)
    {
        var p = await _db.Pharmacies.Include(x => x.DefaultWarehouse).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound(new { message = "Pharmacy not found." });

        return Ok(new PharmacyProfileDto
        {
            Id = p.Id,
            OfficialPharmacyName = p.OfficialPharmacyName,
            PharmacyType = p.PharmacyType,
            Governorate = p.Governorate,
            City = p.City,
            DistrictArea = p.DistrictArea,
            FullAddress = p.FullAddress,
            DefaultWarehouse = p.DefaultWarehouse?.OfficialWarehouseName,
            HasColdStorage = p.HasColdStorage,
            PharmacyLicenseNumber = p.PharmacyLicenseNumber,
            LicenseIssueDate = p.LicenseIssueDate,
            LicenseExpiryDate = p.LicenseExpiryDate,
            PharmacistSyndicateId = p.PharmacistSyndicateId,
            PharmacyStatus = p.PharmacyStatus?.ToString(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });
    }

    [HttpPost("{id:int}/suspend")]
    public async Task<IActionResult> Suspend(int id, [FromBody] EntityActionDto? dto)
    {
        var p = await _db.Pharmacies.FindAsync(id);
        if (p == null) return NotFound(new { message = "Pharmacy not found." });
        var old = p.PharmacyStatus?.ToString();
        p.PharmacyStatus = FacilityStatus.Suspended;
        p.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SuspendEntity, "Pharmacy", p.PharmacyLicenseNumber, old, "Suspended", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Pharmacy suspended.", status = "Suspended" });
    }

    [HttpPost("{id:int}/reactivate")]
    public async Task<IActionResult> Reactivate(int id, [FromBody] EntityActionDto? dto)
    {
        var p = await _db.Pharmacies.FindAsync(id);
        if (p == null) return NotFound(new { message = "Pharmacy not found." });
        var old = p.PharmacyStatus?.ToString();
        p.PharmacyStatus = FacilityStatus.Active;
        p.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.ReactivateEntity, "Pharmacy", p.PharmacyLicenseNumber, old, "Active", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Pharmacy reactivated.", status = "Active" });
    }

    [HttpPost("{id:int}/set-inactive")]
    public async Task<IActionResult> SetInactive(int id, [FromBody] EntityActionDto? dto)
    {
        var p = await _db.Pharmacies.FindAsync(id);
        if (p == null) return NotFound(new { message = "Pharmacy not found." });
        var old = p.PharmacyStatus?.ToString();
        p.PharmacyStatus = FacilityStatus.Inactive;
        p.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SetInactiveEntity, "Pharmacy", p.PharmacyLicenseNumber, old, "Inactive", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Pharmacy set to inactive.", status = "Inactive" });
    }

    [HttpGet("{id:int}/inventory")]
    public async Task<ActionResult<PagedResult<InventoryDistributionItemDto>>> GetInventory(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var ph = await _db.Pharmacies.FindAsync(id);
        if (ph == null) return NotFound(new { message = "Pharmacy not found." });

        var query = _db.InventoryStocks.Where(i => i.HolderType == "Pharmacy" && i.HolderName == ph.OfficialPharmacyName);
        var total = await query.CountAsync();
        var items = await query.Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(i => new InventoryDistributionItemDto
            {
                HolderType = i.HolderType,
                HolderName = i.HolderName,
                TotalReceivedQuantity = i.TotalReceivedQuantity,
                AvailableQuantity = i.AvailableQuantity,
                ReservedQuantity = i.ReservedQuantity,
                QuarantinedQuantity = i.QuarantinedQuantity,
                InventoryStatus = i.InventoryStatus.ToString(),
                LastUpdated = i.LastUpdated
            }).ToListAsync();
        return Ok(new PagedResult<InventoryDistributionItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}/shipments")]
    public async Task<ActionResult<PagedResult<ShipmentSummaryItemDto>>> GetShipments(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var ph = await _db.Pharmacies.FindAsync(id);
        if (ph == null) return NotFound(new { message = "Pharmacy not found." });

        var query = _db.Shipments.Where(s => s.Destination == ph.OfficialPharmacyName);
        var total = await query.CountAsync();
        var items = await query.Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(s => new ShipmentSummaryItemDto
            {
                TransferCode = s.TransferCode,
                ShipmentType = s.ShipmentType.ToString(),
                Source = s.Source,
                Destination = s.Destination,
                ExpectedQuantity = s.ExpectedQuantity,
                ReceivedQuantity = s.ReceivedQuantity,
                ShipmentStatus = s.ShipmentStatus.ToString(),
                DispatchDate = s.DispatchDate,
                ReceivedDate = s.ReceivedDate
            }).ToListAsync();
        return Ok(new PagedResult<ShipmentSummaryItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}/registration-request")]
    public async Task<ActionResult<EntityRegistrationRequestRefDto>> GetRegistrationRequest(int id)
    {
        var r = await _db.RegistrationRequests
            .Where(x => x.PharmacyId == id)
            .OrderByDescending(x => x.SubmittedAt)
            .FirstOrDefaultAsync();
        if (r == null) return NotFound(new { message = "No registration request found for this pharmacy." });

        return Ok(new EntityRegistrationRequestRefDto
        {
            Id = r.Id,
            RequestCode = r.RequestCode,
            RegistrationStatus = r.RegistrationStatus?.ToString(),
            SubmittedAt = r.SubmittedAt
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? search, [FromQuery] string? status)
    {
        var query = _db.Pharmacies.Include(p => p.DefaultWarehouse).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.OfficialPharmacyName != null && p.OfficialPharmacyName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.PharmacyStatus != null && p.PharmacyStatus.ToString() == status);

        var rows = await query.OrderBy(p => p.Id).ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,PharmacyName,PharmacyType,Governorate,City,DefaultWarehouse,HasColdStorage,LicenseExpiryDate,Status,CreatedAt");
        foreach (var p in rows)
        {
            sb.AppendLine(string.Join(",",
                p.Id,
                Csv(p.OfficialPharmacyName), Csv(p.PharmacyType), Csv(p.Governorate), Csv(p.City),
                Csv(p.DefaultWarehouse?.OfficialWarehouseName), p.HasColdStorage,
                p.LicenseExpiryDate?.ToString("yyyy-MM-dd"), Csv(p.PharmacyStatus?.ToString()), p.CreatedAt?.ToString("yyyy-MM-dd")));
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"pharmacies-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Contains(',') || value.Contains('"')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    private static AuditLog Log(AuditAction action, string resourceType, string? resourceId, string? oldVal, string? newVal, string? reason = null) => new()
    {
        LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
        UserDisplayName = "Dr. Saif",
        Role = SystemRole.SuperAdmin,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        NewValue = string.IsNullOrWhiteSpace(reason) ? newVal : $"{newVal} (Reason: {reason})",
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}

```

# FILE: src\EgyMediChain.Api\Controllers\PharmacyDashboardController.cs
```
using EgyMediChain.Api.Dtos;
using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// Operational portal for a single Pharmacy (role: PharmacyUser). Scoped by {pharmacyId} in the
// route, same pattern as the Factory/Warehouse dashboards. No dispatch, no QR scan, no manual
// inventory edits - by design (see spec): the pharmacy only receives from a Warehouse and views.
[ApiController]
[Route("api/pharmacy-dashboard/{pharmacyId:int}")]
[Authorize(Roles = "PharmacyUser,SuperAdmin,MinistryAdmin")]
[ValidateEntityOwnership("pharmacyId")]
public class PharmacyDashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public PharmacyDashboardController(AppDbContext db) => _db = db;

    private async Task<Pharmacy?> GetPharmacyAsync(int pharmacyId) => await _db.Pharmacies.FindAsync(pharmacyId);
    private static bool IsActive(Pharmacy p) => p.PharmacyStatus == FacilityStatus.Active;

    // ---------------- Overview ----------------
    [HttpGet("overview")]
    public async Task<ActionResult<PharmacyOverviewDto>> GetOverview(int pharmacyId)
    {
        var pharmacy = await GetPharmacyAsync(pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });

        var incomingQuery = _db.Shipments.Where(s => s.DestinationPharmacyId == pharmacyId);
        var inventoryQuery = _db.InventoryStocks.Where(i => i.PharmacyId == pharmacyId);

        var cards = new PharmacyOverviewCardsDto
        {
            IncomingShipments = await incomingQuery.CountAsync(),
            PendingReceiving = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit || s.ShipmentStatus == ShipmentStatus.PendingInspection),
            CurrentStock = await inventoryQuery.SumAsync(i => i.AvailableQuantity ?? 0),
            ColdChainStock = pharmacy.HasColdStorage == true
                ? await inventoryQuery.Where(i => i.Batch != null && i.Batch.MedicineProduct != null && i.Batch.MedicineProduct.RequiresColdChain == true).SumAsync(i => i.AvailableQuantity ?? 0)
                : null,
            OpenAlerts = await _db.Alerts.CountAsync(a => a.EntityType == EntityKind.Pharmacy && a.EntityName == pharmacy.OfficialPharmacyName && a.AlertStatus == AlertStatus.Open),
            RecalledStock = await inventoryQuery.Where(i => i.InventoryStatus == InventoryStatus.Recalled).SumAsync(i => i.AvailableQuantity ?? 0)
        };

        var recentIncoming = await incomingQuery.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .OrderByDescending(s => s.DispatchDate).Take(5)
            .Select(s => ToShipmentListItem(s)).ToListAsync();

        var stockSummary = await inventoryQuery.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(i => i.Batch).ThenInclude(b => b!.Factory)
            .OrderByDescending(i => i.LastUpdated).Take(5)
            .Select(i => ToInventoryListItem(i)).ToListAsync();

        var recentAlerts = await _db.Alerts.Where(a => a.EntityType == EntityKind.Pharmacy && a.EntityName == pharmacy.OfficialPharmacyName)
            .Include(a => a.Batch).OrderByDescending(a => a.CreatedAt).Take(5)
            .Select(a => new AlertListItemDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                EntityName = a.EntityName,
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(new PharmacyOverviewDto { Cards = cards, RecentIncomingShipments = recentIncoming, CurrentStockSummary = stockSummary, RecentAlerts = recentAlerts });
    }

    // ---------------- Shipments (Warehouse -> Pharmacy) ----------------
    [HttpGet("shipments")]
    public async Task<ActionResult<PagedResult<ShipmentListItemDto>>> GetShipments(int pharmacyId,
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct).Where(s => s.DestinationPharmacyId == pharmacyId);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.ShipmentStatus != null && s.ShipmentStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.DispatchDate)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(s => ToShipmentListItem(s)).ToListAsync();
        return Ok(new PagedResult<ShipmentListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("shipments/{shipmentId:int}")]
    public async Task<ActionResult<ShipmentDetailsDto>> GetShipmentDetails(int pharmacyId, int shipmentId)
    {
        var s = await _db.Shipments.Include(x => x.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(x => x.Id == shipmentId && x.DestinationPharmacyId == pharmacyId);
        if (s == null) return NotFound(new { message = "Shipment not found for this pharmacy." });

        return Ok(new ShipmentDetailsDto
        {
            Id = s.Id,
            TransferCode = s.TransferCode,
            ShipmentType = s.ShipmentType?.ToString(),
            Source = s.Source,
            Destination = s.Destination,
            ProductName = s.Batch?.MedicineProduct?.ProductName,
            BatchNumber = s.Batch?.BatchNumber,
            ExpectedQuantity = s.ExpectedQuantity,
            ReceivedQuantity = s.ReceivedQuantity,
            ShipmentStatus = s.ShipmentStatus?.ToString(),
            RequiresColdChain = s.RequiresColdChain,
            DispatchDate = s.DispatchDate,
            ReceivedDate = s.ReceivedDate,
            Notes = s.Notes,
            InspectionResult = s.InspectionResult
        });
    }

    [HttpPost("shipments/{shipmentId:int}/receive")]
    public async Task<IActionResult> ReceiveShipment(int pharmacyId, int shipmentId, [FromBody] ReceiveShipmentDto? dto)
    {
        var pharmacy = await GetPharmacyAsync(pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });
        if (!IsActive(pharmacy)) return Conflict(new { message = "This pharmacy is not Active. Operational actions are disabled." });

        var shipment = await _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(s => s.Id == shipmentId && s.DestinationPharmacyId == pharmacyId);
        if (shipment == null) return NotFound(new { message = "Shipment not found for this pharmacy." });

        if (shipment.RequiresColdChain == true && pharmacy.HasColdStorage != true)
            return Conflict(new { message = "This batch requires cold storage. This pharmacy is not marked as cold storage capable." });

        var expected = shipment.ExpectedQuantity ?? 0;
        var inspection = dto?.InspectionResult ?? "Accepted";
        long received = inspection == "Rejected" ? 0 : (dto?.ReceivedQuantity ?? expected);

        shipment.ReceivedQuantity = received;
        shipment.InspectionResult = inspection;
        shipment.Notes = dto?.Notes ?? shipment.Notes;
        shipment.ReceivedDate = DateTime.UtcNow;

        if (inspection == "Rejected" || received == 0)
        {
            shipment.ShipmentStatus = ShipmentStatus.Rejected;
            _db.Alerts.Add(NewAlert(AlertType.ComplianceIssue, AlertSeverity.High, EntityKind.Pharmacy, pharmacy.OfficialPharmacyName, shipment.BatchId, shipment.Id,
                $"Shipment {shipment.TransferCode} was rejected on receipt at {pharmacy.OfficialPharmacyName}."));
            _db.AuditLogs.Add(NewLog(AuditAction.RejectShipment, "Shipment", shipment.TransferCode, "InTransit", "Rejected"));
        }
        else if (received < expected)
        {
            shipment.ShipmentStatus = ShipmentStatus.PartiallyReceived;
            UpsertInventory(pharmacyId, shipment.BatchId, pharmacy.OfficialPharmacyName, received);
            _db.Alerts.Add(NewAlert(AlertType.QuantityMismatch, AlertSeverity.Medium, EntityKind.Pharmacy, pharmacy.OfficialPharmacyName, shipment.BatchId, shipment.Id,
                $"Received quantity ({received}) does not match expected quantity ({expected}) for shipment {shipment.TransferCode}."));
            _db.AuditLogs.Add(NewLog(AuditAction.ReceiveShipment, "Shipment", shipment.TransferCode, "InTransit", "PartiallyReceived"));
        }
        else
        {
            shipment.ShipmentStatus = ShipmentStatus.Received;
            UpsertInventory(pharmacyId, shipment.BatchId, pharmacy.OfficialPharmacyName, received);
            _db.AuditLogs.Add(NewLog(AuditAction.ReceiveShipment, "Shipment", shipment.TransferCode, "InTransit", "Received"));
        }

        if (shipment.Batch != null && shipment.ShipmentStatus != ShipmentStatus.Rejected)
        {
            shipment.Batch.SupplyChainStage = SupplyChainStage.Available;
            shipment.Batch.CurrentLocation = pharmacy.OfficialPharmacyName;
            shipment.Batch.BatchStatus = BatchStatus.InPharmacy;
            shipment.Batch.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = $"Shipment {shipment.ShipmentStatus}.", status = shipment.ShipmentStatus.ToString(), receivedQuantity = received });
    }

    private void UpsertInventory(int pharmacyId, int? batchId, string? holderName, long receivedQty)
    {
        var stock = _db.InventoryStocks.FirstOrDefault(i => i.PharmacyId == pharmacyId && i.BatchId == batchId);
        if (stock == null)
        {
            _db.InventoryStocks.Add(new InventoryStock
            {
                BatchId = batchId,
                HolderType = "Pharmacy",
                HolderName = holderName,
                PharmacyId = pharmacyId,
                TotalReceivedQuantity = receivedQty,
                AvailableQuantity = receivedQty,
                ReservedQuantity = 0,
                QuarantinedQuantity = 0,
                InventoryStatus = InventoryStatus.Active,
                LastUpdated = DateTime.UtcNow
            });
        }
        else
        {
            stock.TotalReceivedQuantity = (stock.TotalReceivedQuantity ?? 0) + receivedQty;
            stock.AvailableQuantity = (stock.AvailableQuantity ?? 0) + receivedQty;
            stock.LastUpdated = DateTime.UtcNow;
        }
    }

    // ---------------- Inventory ----------------
    [HttpGet("inventory")]
    public async Task<ActionResult<PagedResult<InventoryStockListItemDto>>> GetInventory(int pharmacyId,
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.InventoryStocks.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(i => i.Batch).ThenInclude(b => b!.Factory)
            .Where(i => i.PharmacyId == pharmacyId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i => (i.Batch != null && i.Batch.BatchNumber != null && i.Batch.BatchNumber.Contains(search)) ||
                                      (i.Batch != null && i.Batch.MedicineProduct != null && i.Batch.MedicineProduct.ProductName != null && i.Batch.MedicineProduct.ProductName.Contains(search)));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.InventoryStatus != null && i.InventoryStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.LastUpdated)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(i => ToInventoryListItem(i)).ToListAsync();

        return Ok(new PagedResult<InventoryStockListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("inventory/{inventoryId:int}")]
    public async Task<ActionResult<InventoryStockDetailsDto>> GetInventoryDetails(int pharmacyId, int inventoryId)
    {
        var stock = await _db.InventoryStocks.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(i => i.Batch).ThenInclude(b => b!.Factory)
            .FirstOrDefaultAsync(i => i.Id == inventoryId && i.PharmacyId == pharmacyId);
        if (stock == null) return NotFound(new { message = "Stock record not found for this pharmacy." });

        var shipments = await _db.Shipments.Where(s => s.BatchId == stock.BatchId && s.DestinationPharmacyId == pharmacyId)
            .Select(s => new ShipmentSummaryItemDto
            {
                TransferCode = s.TransferCode,
                ShipmentType = s.ShipmentType.ToString(),
                Source = s.Source,
                Destination = s.Destination,
                ExpectedQuantity = s.ExpectedQuantity,
                ReceivedQuantity = s.ReceivedQuantity,
                ShipmentStatus = s.ShipmentStatus.ToString(),
                DispatchDate = s.DispatchDate,
                ReceivedDate = s.ReceivedDate
            }).ToListAsync();

        return Ok(new InventoryStockDetailsDto
        {
            Id = stock.Id,
            ProductInfo = new ProductInfoDto
            {
                ProductName = stock.Batch?.MedicineProduct?.ProductName,
                GTIN = stock.Batch?.MedicineProduct?.GTIN,
                DosageForm = stock.Batch?.MedicineProduct?.DosageForm,
                Strength = stock.Batch?.MedicineProduct?.Strength,
                RequiresColdChain = stock.Batch?.MedicineProduct?.RequiresColdChain,
                ProductStatus = stock.Batch?.MedicineProduct?.ProductStatus
            },
            BatchInfo = new BatchInfoDto
            {
                BatchNumber = stock.Batch?.BatchNumber,
                FactoryName = stock.Batch?.Factory?.OfficialFactoryName,
                ManufacturingDate = stock.Batch?.ManufacturingDate,
                ExpiryDate = stock.Batch?.ExpiryDate,
                BatchStatus = stock.Batch?.BatchStatus?.ToString(),
                SupplyChainStage = stock.Batch?.SupplyChainStage?.ToString()
            },
            WarehouseInventory = new InventoryDistributionItemDto
            {
                HolderType = stock.HolderType,
                HolderName = stock.HolderName,
                TotalReceivedQuantity = stock.TotalReceivedQuantity,
                AvailableQuantity = stock.AvailableQuantity,
                ReservedQuantity = stock.ReservedQuantity,
                QuarantinedQuantity = stock.QuarantinedQuantity,
                InventoryStatus = stock.InventoryStatus?.ToString(),
                LastUpdated = stock.LastUpdated
            },
            RelatedShipments = shipments
        });
    }

    // ---------------- Report Issue ----------------
    [HttpPost("report-issue")]
    public async Task<IActionResult> ReportIssue(int pharmacyId, [FromBody] ReportIssueDto? dto)
    {
        var pharmacy = await GetPharmacyAsync(pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });
        if (!IsActive(pharmacy)) return Conflict(new { message = "This pharmacy is not Active. Operational actions are disabled." });

        var alertType = (dto?.AlertType ?? "ComplianceIssue") switch
        {
            "ColdChainIssue" => AlertType.ColdChainIssue,
            "QuantityMismatch" => AlertType.QuantityMismatch,
            "DamagedPackage" => AlertType.DamagedPackage,
            _ => AlertType.ComplianceIssue
        };

        var alert = NewAlert(alertType, AlertSeverity.Medium, EntityKind.Pharmacy, pharmacy.OfficialPharmacyName, dto?.BatchId, dto?.ShipmentId,
            dto?.Message ?? "Issue reported by pharmacy.");
        _db.Alerts.Add(alert);
        _db.AuditLogs.Add(NewLog(AuditAction.CreateAlert, "Alert", alert.AlertCode, null, "Open"));

        await _db.SaveChangesAsync();
        return Ok(new { message = "Issue reported to the Ministry.", alertCode = alert.AlertCode });
    }

    // ---------------- Alerts (view-only) ----------------
    [HttpGet("alerts")]
    public async Task<ActionResult<PagedResult<AlertListItemDto>>> GetAlerts(int pharmacyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var pharmacy = await GetPharmacyAsync(pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });

        var query = _db.Alerts.Include(a => a.Batch).Where(a => a.EntityType == EntityKind.Pharmacy && a.EntityName == pharmacy.OfficialPharmacyName);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(a => new AlertListItemDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                EntityName = a.EntityName,
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<AlertListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // ---------------- Profile ----------------
    [HttpGet("profile")]
    public async Task<ActionResult<OperationalProfileDto>> GetProfile(int pharmacyId)
    {
        var pharmacy = await _db.Pharmacies.Include(p => p.DefaultWarehouse).FirstOrDefaultAsync(p => p.Id == pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });

        var user = await _db.SystemUsers.FirstOrDefaultAsync(u => u.EntityType == EntityKind.Pharmacy && u.EntityId == pharmacyId);
        var docs = await _db.EntityDocuments
            .Where(d => d.RegistrationRequest != null && d.RegistrationRequest.PharmacyId == pharmacyId)
            .Select(d => new DocumentItemDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                UploadedAt = d.UploadedAt,
                DocumentStatus = d.DocumentStatus.ToString(),
                ReviewedBy = d.ReviewedBy,
                ReviewedAt = d.ReviewedAt,
                RejectionReason = d.RejectionReason
            }).ToListAsync();

        return Ok(new OperationalProfileDto
        {
            Account = user == null ? null : new AccountInfoDto
            {
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                NationalIdMasked = MaskNationalId(user.NationalId),
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive
            },
            Entity = new EntityInfoDto
            {
                PharmacyCode = pharmacy.PharmacyCode,
                OfficialPharmacyName = pharmacy.OfficialPharmacyName,
                PharmacyType = pharmacy.PharmacyType,
                Governorate = pharmacy.Governorate,
                City = pharmacy.City,
                DistrictArea = pharmacy.DistrictArea,
                FullAddress = pharmacy.FullAddress,
                Phone = pharmacy.Phone,
                Email = pharmacy.Email,
                DefaultWarehouseName = pharmacy.DefaultWarehouse?.OfficialWarehouseName,
                HasColdStorage = pharmacy.HasColdStorage,
                PharmacyLicenseNumber = pharmacy.PharmacyLicenseNumber,
                PharmacistSyndicateId = pharmacy.PharmacistSyndicateId,
                LicenseIssueDate = pharmacy.LicenseIssueDate,
                LicenseExpiryDate = pharmacy.LicenseExpiryDate,
                Status = pharmacy.PharmacyStatus?.ToString()
            },
            Documents = docs
        });
    }

    private static string? MaskNationalId(string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length < 4) return nationalId;
        return $"**** **** {nationalId[^4..]}";
    }

    private static ShipmentListItemDto ToShipmentListItem(Shipment s) => new()
    {
        Id = s.Id,
        TransferCode = s.TransferCode,
        ProductName = s.Batch?.MedicineProduct?.ProductName,
        GTIN = s.Batch?.MedicineProduct?.GTIN,
        BatchNumber = s.Batch?.BatchNumber,
        Source = s.Source,
        Destination = s.Destination,
        ExpectedQuantity = s.ExpectedQuantity,
        ReceivedQuantity = s.ReceivedQuantity,
        RequiresColdChain = s.RequiresColdChain,
        ShipmentStatus = s.ShipmentStatus?.ToString(),
        DispatchDate = s.DispatchDate,
        ReceivedDate = s.ReceivedDate
    };

    private static InventoryStockListItemDto ToInventoryListItem(InventoryStock i) => new()
    {
        Id = i.Id,
        ProductName = i.Batch?.MedicineProduct?.ProductName,
        GTIN = i.Batch?.MedicineProduct?.GTIN,
        DosageForm = i.Batch?.MedicineProduct?.DosageForm,
        Strength = i.Batch?.MedicineProduct?.Strength,
        BatchNumber = i.Batch?.BatchNumber,
        FactoryName = i.Batch?.Factory?.OfficialFactoryName,
        TotalReceivedQuantity = i.TotalReceivedQuantity,
        AvailableQuantity = i.AvailableQuantity,
        ReservedQuantity = i.ReservedQuantity,
        QuarantinedQuantity = i.QuarantinedQuantity,
        ExpiryDate = i.Batch?.ExpiryDate,
        RequiresColdChain = i.Batch?.MedicineProduct?.RequiresColdChain,
        InventoryStatus = i.InventoryStatus?.ToString()
    };

    private static Alert NewAlert(AlertType type, AlertSeverity severity, EntityKind entityKind, string? entityName, int? batchId, int? shipmentId, string message) => new()
    {
        AlertCode = $"ALERT-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
        AlertType = type,
        Severity = severity,
        EntityType = entityKind,
        EntityName = entityName,
        BatchId = batchId,
        ShipmentId = shipmentId,
        Message = message,
        AlertStatus = AlertStatus.Open,
        CreatedAt = DateTime.UtcNow
    };

    private static AuditLog NewLog(AuditAction action, string resourceType, string? resourceId, string? oldVal, string? newVal) => new()
    {
        LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
        UserDisplayName = "Pharmacy User",
        Role = SystemRole.PharmacyUser,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        NewValue = newVal,
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}

```

# FILE: src\EgyMediChain.Api\Controllers\RegistrationRequestsController.cs
```
using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/registration-requests")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
public class RegistrationRequestsController : ControllerBase
{
    private readonly AppDbContext _db;
    public RegistrationRequestsController(AppDbContext db) => _db = db;

    // Public wizard submission (Pharmacy/Warehouse/Factory registration) - no auth required.
    // This is the endpoint the "Submit Request" button on the registration wizard should call.
    [AllowAnonymous]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<object>> Submit(
        [FromForm] SubmitRegistrationRequestDto dto,
        [FromForm] List<IFormFile>? documents,
        [FromForm] List<string>? documentTypes)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.EntityType))
            return BadRequest(new { message = "Missing required fields." });

        if (!dto.ConfirmInfoCorrect || !dto.AgreeToInspection)
            return BadRequest(new { message = "You must accept both declarations before submitting." });

        if (await _db.SystemUsers.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "An account with this email already exists." });

        EntityKind? entityKind = dto.EntityType switch
        {
            "Factory" => EntityKind.Factory,
            "Warehouse" => EntityKind.Warehouse,
            "Pharmacy" => EntityKind.Pharmacy,
            _ => null
        };
        if (entityKind == null) return BadRequest(new { message = "Invalid entity type." });

        var user = new SystemUser
        {
            FullName = dto.RepresentativeName,
            Email = dto.Email,
            MobileNumber = dto.MobileNumber,
            NationalId = dto.NationalId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                string.IsNullOrWhiteSpace(dto.Password) ? Guid.NewGuid().ToString("N") : dto.Password),
            Role = entityKind switch
            {
                EntityKind.Factory => SystemRole.FactoryUser,
                EntityKind.Warehouse => SystemRole.WarehouseUser,
                _ => SystemRole.PharmacyUser
            },
            EntityType = entityKind,
            EmailConfirmed = false,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.SystemUsers.Add(user);

        Factory? factory = null; Warehouse? warehouse = null; Pharmacy? pharmacy = null;
        string? entityName = null;

        if (entityKind == EntityKind.Factory)
        {
            factory = new Factory
            {
                OfficialFactoryName = dto.OfficialFactoryName,
                LegalCompanyName = dto.LegalCompanyName,
                DosageFormsProduced = dto.DosageFormsProduced,
                Governorate = dto.Governorate,
                City = dto.City,
                DistrictArea = dto.DistrictArea,
                FullAddress = dto.FullAddress,
                FactoryLicenseNumber = dto.FactoryLicenseNumber,
                TechnicalOperatingLicenseNumber = dto.TechnicalOperatingLicenseNumber,
                CommercialRegistrationNumber = dto.CommercialRegistrationNumber,
                TaxCardNumber = dto.TaxCardNumber,
                LicenseIssueDate = dto.LicenseIssueDate,
                LicenseExpiryDate = dto.LicenseExpiryDate,
                HasQualityControlLab = dto.HasQualityControlLab,
                HasFinishedGoodsStore = dto.HasFinishedGoodsStore,
                HasColdStorage = dto.HasColdStorage,
                HasQuarantineArea = dto.HasQuarantineArea,
                FactoryStatus = FacilityStatus.PendingReview,
                CreatedAt = DateTime.UtcNow
            };
            _db.Factories.Add(factory);
            entityName = factory.OfficialFactoryName;
        }
        else if (entityKind == EntityKind.Warehouse)
        {
            warehouse = new Warehouse
            {
                OfficialWarehouseName = dto.OfficialWarehouseName,
                WarehouseType = dto.WarehouseType,
                Governorate = dto.Governorate,
                City = dto.City,
                DistrictArea = dto.DistrictArea,
                FullAddress = dto.FullAddress,
                WarehouseLicenseNumber = dto.WarehouseLicenseNumber,
                LicenseIssueDate = dto.LicenseIssueDate,
                LicenseExpiryDate = dto.LicenseExpiryDate,
                HasColdStorage = dto.HasColdStorage,
                HasQuarantineArea = dto.HasQuarantineArea,
                HasDeliveryService = dto.HasDeliveryService,
                WarehouseStatus = FacilityStatus.PendingReview,
                CreatedAt = DateTime.UtcNow
            };
            _db.Warehouses.Add(warehouse);
            entityName = warehouse.OfficialWarehouseName;
        }
        else
        {
            pharmacy = new Pharmacy
            {
                OfficialPharmacyName = dto.OfficialPharmacyName,
                PharmacyType = dto.PharmacyType,
                Governorate = dto.Governorate,
                City = dto.City,
                DistrictArea = dto.DistrictArea,
                FullAddress = dto.FullAddress,
                DefaultWarehouseId = dto.DefaultWarehouseId,
                HasColdStorage = dto.HasColdStorage,
                PharmacyLicenseNumber = dto.PharmacyLicenseNumber,
                LicenseIssueDate = dto.LicenseIssueDate,
                LicenseExpiryDate = dto.LicenseExpiryDate,
                PharmacistSyndicateId = dto.PharmacistSyndicateId,
                PharmacyStatus = FacilityStatus.PendingReview,
                CreatedAt = DateTime.UtcNow
            };
            _db.Pharmacies.Add(pharmacy);
            entityName = pharmacy.OfficialPharmacyName;
        }

        await _db.SaveChangesAsync(); // need generated Ids before linking

        var requestCode = $"REQ-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(1000, 9999)}";

        var request = new RegistrationRequest
        {
            RequestCode = requestCode,
            EntityType = entityKind,
            EntityName = entityName,
            RepresentativeName = dto.RepresentativeName,
            Email = dto.Email,
            SystemUserId = user.Id,
            FactoryId = factory?.Id,
            WarehouseId = warehouse?.Id,
            PharmacyId = pharmacy?.Id,
            SubmittedAt = DateTime.UtcNow,
            EmailConfirmed = false,
            RegistrationStatus = RegistrationStatus.Pending,
            DocumentsOverallStatus = DocumentStatus.UnderReview
        };
        _db.RegistrationRequests.Add(request);
        await _db.SaveChangesAsync();

        if (documents != null && documents.Count > 0)
        {
            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", requestCode);
            Directory.CreateDirectory(uploadsRoot);

            for (int i = 0; i < documents.Count; i++)
            {
                var file = documents[i];
                if (file.Length == 0) continue;

                var docType = documentTypes != null && i < documentTypes.Count ? documentTypes[i] : "Document";
                var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
                var fullPath = Path.Combine(uploadsRoot, safeName);

                await using (var stream = new FileStream(fullPath, FileMode.Create))
                    await file.CopyToAsync(stream);

                _db.EntityDocuments.Add(new EntityDocument
                {
                    RegistrationRequestId = request.Id,
                    DocumentType = docType,
                    FileName = file.FileName,
                    FileUrl = $"/uploads/{requestCode}/{safeName}",
                    UploadedAt = DateTime.UtcNow,
                    DocumentStatus = DocumentStatus.UnderReview
                });
            }
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Registration request submitted successfully.", requestCode, requestId = request.Id });
    }

    // status: pending | under-review | needs-more-documents | approved | rejected | cancelled | (empty = all)
    // entityType: Factory | Warehouse | Pharmacy (empty = all, subject to this account's Ministry scope)
    [HttpGet]
    public async Task<ActionResult<PagedResult<RegistrationRequestListItemDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? entityType, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.RegistrationRequests.AsQueryable();

        var scope = GetCallerEntityScope();
        if (scope != null)
            query = query.Where(r => r.EntityType != null && r.EntityType.ToString() == scope);

        // Explicit filter from the frontend's dropdown - only narrows further within whatever
        // this account's Ministry scope already allows (can't be used to bypass the scope above).
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(r => r.EntityType != null && r.EntityType.ToString() == entityType);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Replace("-", "").ToLower();
            query = query.Where(r => r.RegistrationStatus != null &&
                r.RegistrationStatus.ToString()!.ToLower() == normalized);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r =>
                (r.EntityName != null && r.EntityName.Contains(search)) ||
                (r.Email != null && r.Email.Contains(search)) ||
                (r.RequestCode != null && r.RequestCode.Contains(search)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.SubmittedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize))
            .Take(pageSize <= 0 ? 10 : pageSize)
            .Select(r => new RegistrationRequestListItemDto
            {
                Id = r.Id,
                RequestCode = r.RequestCode,
                EntityType = r.EntityType.ToString(),
                EntityName = r.EntityName,
                RepresentativeName = r.RepresentativeName,
                SubmittedBy = r.RepresentativeName,
                Email = r.Email,
                SubmittedAt = r.SubmittedAt,
                EmailConfirmed = r.EmailConfirmed,
                DocumentsOverallStatus = r.DocumentsOverallStatus.ToString(),
                RegistrationStatus = r.RegistrationStatus.ToString()
            }).ToListAsync();

        return Ok(new PagedResult<RegistrationRequestListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // Unified lookup, works for all 3 entity types (EntitiesManagement-Backend-Gaps.md, item 1).
    // Replaces the frontend's text-search-then-guess-the-first-result workaround, which was
    // unreliable for Factories and simply didn't exist for Warehouses/Pharmacies.
    [HttpGet("by-entity/{entityType}/{entityId:int}")]
    public async Task<ActionResult<EntityRegistrationRequestRefDto>> GetByEntity(string entityType, int entityId)
    {
        if (!Enum.TryParse<EntityKind>(entityType, true, out var kind))
            return BadRequest(new { message = "entityType must be Factory, Warehouse or Pharmacy." });
        if (!IsAllowedEntityType(kind)) return Forbid403();

        var query = _db.RegistrationRequests.AsQueryable();
        query = kind switch
        {
            EntityKind.Factory => query.Where(r => r.FactoryId == entityId),
            EntityKind.Warehouse => query.Where(r => r.WarehouseId == entityId),
            EntityKind.Pharmacy => query.Where(r => r.PharmacyId == entityId),
            _ => query.Where(r => false)
        };

        var r = await query.OrderByDescending(x => x.SubmittedAt).FirstOrDefaultAsync();
        if (r == null) return NotFound(new { message = "No registration request found for this entity." });

        return Ok(new EntityRegistrationRequestRefDto
        {
            Id = r.Id,
            RequestCode = r.RequestCode,
            RegistrationStatus = r.RegistrationStatus?.ToString(),
            SubmittedAt = r.SubmittedAt
        });
    }

    [HttpGet("counts")]
    public async Task<ActionResult<object>> GetCounts()
    {
        var scope = GetCallerEntityScope();
        var baseQuery = _db.RegistrationRequests.AsQueryable();
        if (scope != null)
            baseQuery = baseQuery.Where(r => r.EntityType != null && r.EntityType.ToString() == scope);

        return Ok(new
        {
            PendingReview = await baseQuery.CountAsync(r => r.RegistrationStatus == RegistrationStatus.Pending || r.RegistrationStatus == RegistrationStatus.UnderReview),
            NeedsMoreDocuments = await baseQuery.CountAsync(r => r.RegistrationStatus == RegistrationStatus.NeedsMoreDocuments),
            Approved = await baseQuery.CountAsync(r => r.RegistrationStatus == RegistrationStatus.Approved),
            Rejected = await baseQuery.CountAsync(r => r.RegistrationStatus == RegistrationStatus.Rejected),
            Cancelled = await baseQuery.CountAsync(r => r.RegistrationStatus == RegistrationStatus.Cancelled)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RegistrationRequestDetailsDto>> GetById(int id)
    {
        var r = await _db.RegistrationRequests
            .Include(x => x.Factory)
            .Include(x => x.Warehouse)
            .Include(x => x.Pharmacy).ThenInclude(p => p!.DefaultWarehouse)
            .Include(x => x.SystemUser)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        var entity = new EntityInfoDto();
        if (r.Factory != null)
        {
            var f = r.Factory;
            entity.OfficialFactoryName = f.OfficialFactoryName;
            entity.LegalCompanyName = f.LegalCompanyName;
            entity.DosageFormsProduced = f.DosageFormsProduced;
            entity.Governorate = f.Governorate; entity.City = f.City; entity.DistrictArea = f.DistrictArea; entity.FullAddress = f.FullAddress;
            entity.FactoryLicenseNumber = f.FactoryLicenseNumber;
            entity.TechnicalOperatingLicenseNumber = f.TechnicalOperatingLicenseNumber;
            entity.CommercialRegistrationNumber = f.CommercialRegistrationNumber;
            entity.TaxCardNumber = f.TaxCardNumber;
            entity.LicenseIssueDate = f.LicenseIssueDate; entity.LicenseExpiryDate = f.LicenseExpiryDate;
            entity.HasQualityControlLab = f.HasQualityControlLab; entity.HasFinishedGoodsStore = f.HasFinishedGoodsStore;
            entity.HasColdStorage = f.HasColdStorage; entity.HasQuarantineArea = f.HasQuarantineArea;
            entity.Status = f.FactoryStatus?.ToString();
        }
        else if (r.Warehouse != null)
        {
            var w = r.Warehouse;
            entity.OfficialWarehouseName = w.OfficialWarehouseName;
            entity.WarehouseType = w.WarehouseType;
            entity.Governorate = w.Governorate; entity.City = w.City; entity.DistrictArea = w.DistrictArea; entity.FullAddress = w.FullAddress;
            entity.WarehouseLicenseNumber = w.WarehouseLicenseNumber;
            entity.LicenseIssueDate = w.LicenseIssueDate; entity.LicenseExpiryDate = w.LicenseExpiryDate;
            entity.HasColdStorage = w.HasColdStorage; entity.HasQuarantineArea = w.HasQuarantineArea; entity.HasDeliveryService = w.HasDeliveryService;
            entity.Status = w.WarehouseStatus?.ToString();
        }
        else if (r.Pharmacy != null)
        {
            var p = r.Pharmacy;
            entity.OfficialPharmacyName = p.OfficialPharmacyName;
            entity.PharmacyType = p.PharmacyType;
            entity.Governorate = p.Governorate; entity.City = p.City; entity.DistrictArea = p.DistrictArea; entity.FullAddress = p.FullAddress;
            entity.DefaultWarehouseName = p.DefaultWarehouse?.OfficialWarehouseName;
            entity.HasColdStorage = p.HasColdStorage;
            entity.PharmacyLicenseNumber = p.PharmacyLicenseNumber;
            entity.LicenseIssueDate = p.LicenseIssueDate; entity.LicenseExpiryDate = p.LicenseExpiryDate;
            entity.PharmacistSyndicateId = p.PharmacistSyndicateId;
            entity.Status = p.PharmacyStatus?.ToString();
        }

        var fields = new List<FieldItemDto>
        {
            new() { Label = "Representative Name", Value = r.RepresentativeName },
            new() { Label = "Email", Value = r.SystemUser?.Email ?? r.Email },
            new() { Label = "Mobile Number", Value = r.SystemUser?.MobileNumber },
            new() { Label = "National ID", Value = MaskNationalId(r.SystemUser?.NationalId) },
            new() { Label = "Governorate", Value = entity.Governorate },
            new() { Label = "City", Value = entity.City },
            new() { Label = "District / Area", Value = entity.DistrictArea },
            new() { Label = "Full Address", Value = entity.FullAddress }
        };

        if (r.Factory != null)
        {
            fields.Add(new() { Label = "Official Factory Name", Value = entity.OfficialFactoryName });
            fields.Add(new() { Label = "Legal Company Name", Value = entity.LegalCompanyName });
            fields.Add(new() { Label = "Dosage Forms Produced", Value = entity.DosageFormsProduced });
            fields.Add(new() { Label = "Factory License Number", Value = entity.FactoryLicenseNumber });
            fields.Add(new() { Label = "Technical Operating License Number", Value = entity.TechnicalOperatingLicenseNumber });
            fields.Add(new() { Label = "Commercial Registration Number", Value = entity.CommercialRegistrationNumber });
            fields.Add(new() { Label = "Tax Card Number", Value = entity.TaxCardNumber });
            fields.Add(new() { Label = "Has Quality Control Lab", Value = entity.HasQualityControlLab?.ToString() });
            fields.Add(new() { Label = "Has Finished Goods Store", Value = entity.HasFinishedGoodsStore?.ToString() });
        }
        else if (r.Warehouse != null)
        {
            fields.Add(new() { Label = "Official Warehouse Name", Value = entity.OfficialWarehouseName });
            fields.Add(new() { Label = "Warehouse Type", Value = entity.WarehouseType });
            fields.Add(new() { Label = "Warehouse License Number", Value = entity.WarehouseLicenseNumber });
            fields.Add(new() { Label = "Has Delivery Service", Value = entity.HasDeliveryService?.ToString() });
        }
        else if (r.Pharmacy != null)
        {
            fields.Add(new() { Label = "Official Pharmacy Name", Value = entity.OfficialPharmacyName });
            fields.Add(new() { Label = "Pharmacy Type", Value = entity.PharmacyType });
            fields.Add(new() { Label = "Default Warehouse", Value = entity.DefaultWarehouseName });
            fields.Add(new() { Label = "Pharmacy License Number", Value = entity.PharmacyLicenseNumber });
            fields.Add(new() { Label = "Pharmacist Syndicate ID", Value = entity.PharmacistSyndicateId });
        }

        fields.Add(new() { Label = "Has Cold Storage", Value = entity.HasColdStorage?.ToString() });
        fields.Add(new() { Label = "Has Quarantine Area", Value = entity.HasQuarantineArea?.ToString() });

        var dto = new RegistrationRequestDetailsDto
        {
            Id = r.Id,
            RequestCode = r.RequestCode,
            EntityType = r.EntityType?.ToString(),
            SubmittedAt = r.SubmittedAt,
            RegistrationStatus = r.RegistrationStatus?.ToString(),
            AdminNotes = r.AdminNotes,
            RejectionReason = r.RejectionReason,
            InspectionScheduledDate = r.InspectionScheduledDate,
            InspectorNotes = r.InspectorNotes,
            Account = r.SystemUser == null ? null : new AccountInfoDto
            {
                FullName = r.SystemUser.FullName,
                Email = r.SystemUser.Email,
                MobileNumber = r.SystemUser.MobileNumber,
                NationalIdMasked = MaskNationalId(r.SystemUser.NationalId),
                EmailConfirmed = r.SystemUser.EmailConfirmed,
                IsActive = r.SystemUser.IsActive
            },
            Entity = entity,
            Documents = r.Documents?.Select(d => new DocumentItemDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                UploadedAt = d.UploadedAt,
                DocumentStatus = d.DocumentStatus?.ToString(),
                ReviewedBy = d.ReviewedBy,
                ReviewedAt = d.ReviewedAt,
                RejectionReason = d.RejectionReason
            }).ToList(),
            Fields = fields
        };

        return Ok(dto);
    }

    // Only for requests that never became a live entity (Pending / NeedsMoreDocuments / Rejected /
    // Cancelled). An Approved request is a real Active Factory/Warehouse/Pharmacy - deleting the
    // *request* on its own would leave an orphaned entity/account, so that case is blocked on purpose.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _db.RegistrationRequests
            .Include(x => x.Factory).Include(x => x.Warehouse).Include(x => x.Pharmacy)
            .Include(x => x.SystemUser).Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        if (r.RegistrationStatus == RegistrationStatus.Approved)
            return BadRequest(new { message = "An approved registration is a live entity - suspend/deactivate it instead of deleting the request." });

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.DeleteRegistrationRequest,
            ResourceType = "RegistrationRequest",
            ResourceId = r.RequestCode,
            OldValue = r.RegistrationStatus?.ToString(),
            NewValue = "Deleted",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        if (r.Documents != null) _db.EntityDocuments.RemoveRange(r.Documents);
        _db.RegistrationRequests.Remove(r);

        // Only remove the entity/user rows if they never went Active (i.e. they were never
        // approved through any other path either) - defensive check in case status drifted.
        if (r.Factory != null && r.Factory.FactoryStatus != FacilityStatus.Active) _db.Factories.Remove(r.Factory);
        if (r.Warehouse != null && r.Warehouse.WarehouseStatus != FacilityStatus.Active) _db.Warehouses.Remove(r.Warehouse);
        if (r.Pharmacy != null && r.Pharmacy.PharmacyStatus != FacilityStatus.Active) _db.Pharmacies.Remove(r.Pharmacy);
        if (r.SystemUser != null && r.SystemUser.IsActive != true) _db.SystemUsers.Remove(r.SystemUser);

        await _db.SaveChangesAsync();
        return Ok(new { message = "Registration request deleted." });
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveRequestDto? dto)
    {
        var r = await _db.RegistrationRequests
            .Include(x => x.Factory).Include(x => x.Warehouse).Include(x => x.Pharmacy).Include(x => x.SystemUser)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        r.RegistrationStatus = RegistrationStatus.Approved;
        if (!string.IsNullOrWhiteSpace(dto?.AdminNotes)) r.AdminNotes = dto.AdminNotes;
        if (r.Factory != null) r.Factory.FactoryStatus = FacilityStatus.Active;
        if (r.Warehouse != null) r.Warehouse.WarehouseStatus = FacilityStatus.Active;
        if (r.Pharmacy != null) r.Pharmacy.PharmacyStatus = FacilityStatus.Active;
        if (r.SystemUser != null) r.SystemUser.IsActive = true;

        _db.AuditLogs.Add(NewLog(r.SystemUser, AuditAction.ApproveRegistration, "RegistrationRequest", r.RequestCode, "Pending", "Approved"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Registration request approved.", status = "Approved" });
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectRequestDto? dto)
    {
        var r = await _db.RegistrationRequests
            .Include(x => x.Factory).Include(x => x.Warehouse).Include(x => x.Pharmacy).Include(x => x.SystemUser)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        r.RegistrationStatus = RegistrationStatus.Rejected;
        r.RejectionReason = dto?.RejectionReason ?? "Not specified";
        if (r.Factory != null) r.Factory.FactoryStatus = FacilityStatus.Rejected;
        if (r.Warehouse != null) r.Warehouse.WarehouseStatus = FacilityStatus.Rejected;
        if (r.Pharmacy != null) r.Pharmacy.PharmacyStatus = FacilityStatus.Rejected;
        if (r.SystemUser != null) r.SystemUser.IsActive = false;

        _db.AuditLogs.Add(NewLog(r.SystemUser, AuditAction.RejectRegistration, "RegistrationRequest", r.RequestCode, "Pending", "Rejected"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Registration request rejected.", status = "Rejected" });
    }

    // "request-documents" is an alias of "request-more-documents" - same handler, two URLs,
    // so either naming the frontend already built against will work.
    [HttpPost("{id:int}/request-more-documents")]
    [HttpPost("{id:int}/request-documents")]
    public async Task<IActionResult> RequestMoreDocuments(int id, [FromBody] RequestMoreDocumentsDto? dto)
    {
        var r = await _db.RegistrationRequests.Include(x => x.SystemUser).Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        r.RegistrationStatus = RegistrationStatus.NeedsMoreDocuments;
        r.AdminNotes = dto?.AdminNotes;

        if (dto?.DocumentIdsNeedingReplacement != null && r.Documents != null)
        {
            foreach (var docId in dto.DocumentIdsNeedingReplacement)
            {
                var doc = r.Documents.FirstOrDefault(d => d.Id == docId);
                if (doc != null) doc.DocumentStatus = DocumentStatus.NeedsReplacement;
            }
        }

        _db.AuditLogs.Add(NewLog(r.SystemUser, AuditAction.UpdateDocumentStatus, "EntityDocument", r.RequestCode, "UnderReview", "NeedsReplacement"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "More documents requested.", status = "NeedsMoreDocuments" });
    }

    // Doesn't change RegistrationStatus (an inspection can be scheduled at any stage) - just
    // records when/notes, so the frontend can show "Inspection scheduled for <date>" on top of
    // whatever status the request is already in.
    [HttpPost("{id:int}/request-inspection")]
    public async Task<IActionResult> RequestInspection(int id, [FromBody] RequestInspectionDto? dto)
    {
        var r = await _db.RegistrationRequests.Include(x => x.SystemUser).FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        r.InspectionScheduledDate = dto?.ScheduledDate;
        r.InspectorNotes = dto?.InspectorNotes;
        r.InspectionRequestedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(r.SystemUser, AuditAction.UpdateDocumentStatus, "RegistrationRequest", r.RequestCode, r.RegistrationStatus?.ToString(), "InspectionRequested"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Inspection requested.", scheduledDate = r.InspectionScheduledDate });
    }

    [HttpPost("documents/{documentId:int}/status")]
    public async Task<IActionResult> UpdateDocumentStatus(int documentId, [FromBody] DocumentStatusUpdateDto? dto)
    {
        var doc = await _db.EntityDocuments.Include(d => d.RegistrationRequest).FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc == null) return NotFound(new { message = "Document not found." });
        if (!IsAllowedEntityType(doc.RegistrationRequest?.EntityType)) return Forbid403();

        doc.DocumentStatus = (dto?.Status ?? "Complete") switch
        {
            "Complete" => DocumentStatus.Complete,
            "NeedsReplacement" => DocumentStatus.NeedsReplacement,
            "Rejected" => DocumentStatus.Rejected,
            _ => DocumentStatus.UnderReview
        };
        doc.RejectionReason = dto?.RejectionReason;
        doc.ReviewedAt = DateTime.UtcNow;
        doc.ReviewedBy = "Dr. Saif";

        await _db.SaveChangesAsync();
        return Ok(new { message = "Document status updated.", status = doc.DocumentStatus.ToString() });
    }

    private string? GetCallerEntityScope()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role == "SuperAdmin") return null; // unscoped

        var scope = User.FindFirst("entityType")?.Value;
        if (string.IsNullOrEmpty(scope) || scope == "Ministry") return null; // unscoped Ministry account
        return scope; // "Factory" | "Warehouse" | "Pharmacy"
    }

    private bool IsAllowedEntityType(EntityKind? type)
    {
        var scope = GetCallerEntityScope();
        if (scope == null) return true;
        return type != null && type.ToString() == scope;
    }

    private ObjectResult Forbid403() => new(new { message = "This account's Ministry scope doesn't include this entity type." }) { StatusCode = 403 };

    private static string? MaskNationalId(string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length < 4) return nationalId;
        return $"**** **** {nationalId[^4..]}";
    }

    private static AuditLog NewLog(SystemUser? user, AuditAction action, string resourceType, string? resourceId, string? oldVal, string? newVal) => new()
    {
        LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
        UserId = user?.Id,
        UserDisplayName = "Dr. Saif",
        Role = SystemRole.SuperAdmin,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        NewValue = newVal,
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}
```

# FILE: src\EgyMediChain.Api\Controllers\ReportsController.cs
```
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ClosedXML.Excel;
using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EgyMediChain.Api.Controllers;

// Backend Action Report ยง4. Async job model as specified, implemented with Task.Run +
// IServiceScopeFactory instead of an external queue/worker (Hangfire etc.) - none is wired up
// in this project, and this is enough for Ministry-scale report volumes. All three formats
// (Csv/Pdf/Xlsx) are implemented: Csv is plain text, Pdf via QuestPDF, Xlsx via ClosedXML.
[ApiController]
[Route("api/reports")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
public class ReportsController : ControllerBase
{
    private static readonly string[] ValidReportTypes =
        { "BatchTraceability", "RecallSummary", "InventorySnapshot", "AuditTrailExport", "StaffDirectory" };
    private static readonly string[] ValidFormats = { "Csv", "Pdf", "Xlsx" };

    private readonly AppDbContext _db;
    private readonly IServiceScopeFactory _scopeFactory;

    public ReportsController(AppDbContext db, IServiceScopeFactory scopeFactory)
    {
        _db = db;
        _scopeFactory = scopeFactory;
    }

    private static string ReportsFolder => Path.Combine(Directory.GetCurrentDirectory(), "GeneratedReports");

    [HttpPost("generate")]
    public async Task<ActionResult<ReportJobDto>> Generate([FromBody] GenerateReportRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.ReportType) || !ValidReportTypes.Contains(dto.ReportType))
            return BadRequest(new { message = $"reportType must be one of: {string.Join(", ", ValidReportTypes)}." });

        var format = string.IsNullOrWhiteSpace(dto.Format) ? "Csv" : dto.Format;
        if (!ValidFormats.Contains(format, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = $"format must be one of: {string.Join(", ", ValidFormats)}." });
        format = ValidFormats.First(f => string.Equals(f, format, StringComparison.OrdinalIgnoreCase));

        if (dto.DateFrom != null && dto.DateTo != null && dto.DateFrom > dto.DateTo)
            return BadRequest(new { message = "dateFrom must be before dateTo." });
        if (dto.DateFrom != null && dto.DateTo != null && (dto.DateTo.Value - dto.DateFrom.Value).TotalDays > 730)
            return BadRequest(new { message = "Date range can't exceed 2 years." });

        var userId = int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var uid) ? uid : (int?)null;

        var job = new ReportJob
        {
            Id = Guid.NewGuid(),
            RequestedByUserId = userId,
            ReportType = dto.ReportType,
            Format = format,
            ParametersJson = System.Text.Json.JsonSerializer.Serialize(dto),
            Status = "Queued",
            RequestedAt = DateTime.UtcNow
        };
        _db.ReportJobs.Add(job);
        await _db.SaveChangesAsync();

        // Fire-and-forget on a new DI scope (the request's own AppDbContext/scope will be
        // disposed as soon as this action returns 202, so the background work needs its own).
        _ = Task.Run(() => RunJobAsync(job.Id));

        return AcceptedAtAction(nameof(GetStatus), new { jobId = job.Id },
            new ReportJobDto { JobId = job.Id, Status = job.Status, RequestedAt = job.RequestedAt });
    }

    [HttpGet("{jobId:guid}/status")]
    public async Task<ActionResult<ReportJobDto>> GetStatus(Guid jobId)
    {
        var job = await _db.ReportJobs.FindAsync(jobId);
        if (job == null) return NotFound(new { message = "Report job not found." });
        if (!IsOwnerOrSuperAdmin(job)) return StatusCode(403, new { message = "You didn't request this report." });

        return Ok(new ReportJobDto
        {
            JobId = job.Id,
            Status = job.Status,
            RequestedAt = job.RequestedAt,
            CompletedAt = job.CompletedAt,
            ErrorMessage = job.ErrorMessage
        });
    }

    [HttpGet("{jobId:guid}/download")]
    public async Task<IActionResult> Download(Guid jobId)
    {
        var job = await _db.ReportJobs.FindAsync(jobId);
        if (job == null) return NotFound(new { message = "Report job not found." });
        if (!IsOwnerOrSuperAdmin(job)) return StatusCode(403, new { message = "You didn't request this report." });

        if (job.Status != "Completed" || string.IsNullOrEmpty(job.FilePath))
            return Conflict(new { message = $"Report is not ready yet (status: {job.Status}). Keep polling /status." });
        if (!System.IO.File.Exists(job.FilePath))
            return NotFound(new { message = "Report file is no longer available (it may have expired)." });

        var bytes = await System.IO.File.ReadAllBytesAsync(job.FilePath);
        var (contentType, ext) = job.Format switch
        {
            "Pdf" => ("application/pdf", "pdf"),
            "Xlsx" => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
            _ => ("text/csv", "csv")
        };
        return File(bytes, contentType, $"{job.ReportType}-{job.Id:N}.{ext}");
    }

    private bool IsOwnerOrSuperAdmin(ReportJob job)
    {
        var userId = int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var uid) ? uid : (int?)null;
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        return isSuperAdmin || (userId != null && userId == job.RequestedByUserId);
    }

    // Runs on its own DI scope/DbContext instance - the HTTP request that queued this job has
    // already returned a 202 by the time this executes.
    private async Task RunJobAsync(Guid jobId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var job = await db.ReportJobs.FindAsync(jobId);
        if (job == null) return;

        try
        {
            job.Status = "Processing";
            await db.SaveChangesAsync();

            var dto = System.Text.Json.JsonSerializer.Deserialize<GenerateReportRequestDto>(job.ParametersJson ?? "{}");
            var (headers, rows) = await BuildReportDataAsync(db, job.ReportType!, dto);

            byte[] bytes = job.Format switch
            {
                "Pdf" => RenderPdf(job.ReportType!, headers, rows),
                "Xlsx" => RenderXlsx(job.ReportType!, headers, rows),
                _ => Encoding.UTF8.GetBytes(RenderCsv(headers, rows))
            };

            Directory.CreateDirectory(ReportsFolder);
            var ext = job.Format switch { "Pdf" => "pdf", "Xlsx" => "xlsx", _ => "csv" };
            var path = Path.Combine(ReportsFolder, $"{job.Id:N}.{ext}");
            await System.IO.File.WriteAllBytesAsync(path, bytes);

            job.FilePath = path;
            job.Status = "Completed";
            job.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            job.ErrorMessage = ex.Message;
        }
        await db.SaveChangesAsync();
    }

    // Single source of truth for report content - each format renderer below just formats the
    // same (headers, rows) shape differently, so a new ReportType only needs one switch case.
    private static async Task<(string[] Headers, List<object?[]> Rows)> BuildReportDataAsync(
        AppDbContext db, string reportType, GenerateReportRequestDto? dto)
    {
        var rows = new List<object?[]>();
        string[] headers;

        switch (reportType)
        {
            case "StaffDirectory":
                headers = new[] { "Id", "FullName", "OfficialEmail", "Role", "Department", "Facility", "Status", "HireDate" };
                foreach (var u in await db.SystemUsers.Where(u => !u.IsDeleted).ToListAsync())
                    rows.Add(new object?[] { u.Id, u.FullName, u.OfficialEmail ?? u.Email, u.Role, u.Department, u.Facility,
                        u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"), u.HireDate });
                break;

            case "AuditTrailExport":
                headers = new[] { "LogCode", "User", "Action", "ResourceType", "ResourceId", "Result", "CreatedAt" };
                var logsQuery = db.AuditLogs.AsQueryable();
                if (dto?.DateFrom != null) logsQuery = logsQuery.Where(l => l.CreatedAt >= dto.DateFrom);
                if (dto?.DateTo != null) logsQuery = logsQuery.Where(l => l.CreatedAt <= dto.DateTo);
                foreach (var l in await logsQuery.OrderByDescending(l => l.CreatedAt).Take(50000).ToListAsync())
                    rows.Add(new object?[] { l.LogCode, l.UserDisplayName, l.Action, l.ResourceType, l.ResourceId, l.Result, l.CreatedAt });
                break;

            case "RecallSummary":
                headers = new[] { "AlertCode", "BatchNumber", "EntityName", "Severity", "Status", "Message", "CreatedAt" };
                var alertsQuery = db.Alerts.Include(a => a.Batch).Where(a => a.AlertType == Domain.Enums.AlertType.Recall);
                if (dto?.DateFrom != null) alertsQuery = alertsQuery.Where(a => a.CreatedAt >= dto.DateFrom);
                if (dto?.DateTo != null) alertsQuery = alertsQuery.Where(a => a.CreatedAt <= dto.DateTo);
                foreach (var a in await alertsQuery.OrderByDescending(a => a.CreatedAt).ToListAsync())
                    rows.Add(new object?[] { a.AlertCode, a.Batch?.BatchNumber, a.EntityName, a.Severity, a.AlertStatus, a.Message, a.CreatedAt });
                break;

            case "InventorySnapshot":
                headers = new[] { "BatchNumber", "Location", "AvailableQuantity", "QuarantinedQuantity", "Status" };
                var invQuery = db.InventoryStocks.Include(i => i.Batch).AsQueryable();
                if (dto?.EntityId != null && dto.EntityType == "Warehouse") invQuery = invQuery.Where(i => i.WarehouseId == dto.EntityId);
                if (dto?.EntityId != null && dto.EntityType == "Pharmacy") invQuery = invQuery.Where(i => i.PharmacyId == dto.EntityId);
                foreach (var i in await invQuery.Take(50000).ToListAsync())
                    rows.Add(new object?[] { i.Batch?.BatchNumber, i.HolderName, i.AvailableQuantity, i.QuarantinedQuantity, i.InventoryStatus });
                break;

            case "BatchTraceability":
                headers = new[] { "BatchNumber", "ProductName", "Factory", "ManufacturingDate", "ExpiryDate", "Status", "SupplyChainStage", "CurrentLocation" };
                var batchQuery = db.Batches.Include(b => b.MedicineProduct).Include(b => b.Factory).AsQueryable();
                if (dto?.DateFrom != null) batchQuery = batchQuery.Where(b => b.ManufacturingDate >= dto.DateFrom);
                if (dto?.DateTo != null) batchQuery = batchQuery.Where(b => b.ManufacturingDate <= dto.DateTo);
                if (dto?.EntityId != null && dto.EntityType == "Factory") batchQuery = batchQuery.Where(b => b.FactoryId == dto.EntityId);
                foreach (var b in await batchQuery.Take(50000).ToListAsync())
                    rows.Add(new object?[] { b.BatchNumber, b.MedicineProduct?.ProductName, b.Factory?.OfficialFactoryName,
                        b.ManufacturingDate, b.ExpiryDate, b.BatchStatus, b.SupplyChainStage, b.CurrentLocation });
                break;

            default:
                headers = Array.Empty<string>();
                break;
        }

        return (headers, rows);
    }

    private static string Cell(object? v) => v switch
    {
        null => "",
        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
        _ => v.ToString() ?? ""
    };

    // ---- CSV ----
    private static string RenderCsv(string[] headers, List<object?[]> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(CsvEscape)));
        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(f => CsvEscape(Cell(f)))));
        return sb.ToString();
    }

    private static string CsvEscape(string s) =>
        s.Contains(',') || s.Contains('"') ? $"\"{s.Replace("\"", "\"\"")}\"" : s;

    // ---- PDF (QuestPDF) ----
    private static byte[] RenderPdf(string reportType, string[] headers, List<object?[]> rows)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Text($"EgyMediChain โ€” {reportType}").FontSize(16).Bold();
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated ");
                    x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") + " UTC");
                    x.Span(" โ€” Page ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in headers) columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var h in headers)
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(h).Bold();
                    });

                    // Cap rows in the PDF - a 50,000-row PDF isn't a usable document; the Csv/Xlsx
                    // formats are the right choice for bulk exports.
                    foreach (var row in rows.Take(2000))
                        foreach (var f in row)
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(Cell(f));
                });

                if (rows.Count > 2000)
                    page.Content().Column(c => c.Item().PaddingTop(6).Text($"Showing first 2,000 of {rows.Count} rows. Use Csv or Xlsx format for the full dataset.").Italic().FontColor(Colors.Grey.Darken1));
            });
        });

        return document.GeneratePdf();
    }

    // ---- Xlsx (ClosedXML) ----
    private static byte[] RenderXlsx(string reportType, string[] headers, List<object?[]> rows)
    {
        using var workbook = new XLWorkbook();
        var sheetName = reportType.Length > 31 ? reportType[..31] : reportType; // Excel sheet name limit
        var sheet = workbook.Worksheets.Add(sheetName);

        for (var c = 0; c < headers.Length; c++)
        {
            sheet.Cell(1, c + 1).Value = headers[c];
            sheet.Cell(1, c + 1).Style.Font.Bold = true;
            sheet.Cell(1, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E5E7EB");
        }

        for (var r = 0; r < rows.Count; r++)
            for (var c = 0; c < rows[r].Length; c++)
                sheet.Cell(r + 2, c + 1).Value = Cell(rows[r][c]);

        if (headers.Length > 0) sheet.Range(1, 1, 1, headers.Length).SetAutoFilter();
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
```

# FILE: src\EgyMediChain.Api\Controllers\SettingController.cs
```
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// Backend Action Report ง3 - previously the frontend persisted everything to localStorage only
// (settings didn't follow the user across devices and were invisible to admins for support).
[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db)
    {
        _db = db;
    }

    private int? CurrentUserId()
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }

    // Self-scoped by the JWT's own subject claim, never by a client-supplied id - this is the
    // IDOR protection the report calls out as the main risk here (ง3.1).
    [HttpGet("me")]
    public async Task<ActionResult<UserPreferencesDto>> GetMyPreferences()
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();

        var user = await _db.SystemUsers.FindAsync(userId.Value);
        if (user == null) return NotFound();

        var prefs = await _db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);

        return Ok(new UserPreferencesDto
        {
            Notifications = new NotificationPreferencesDto
            {
                EmailAlerts = prefs?.EmailAlerts ?? true,
                PushNotifications = prefs?.PushNotifications ?? true,
                CriticalAlerts = prefs?.CriticalAlerts ?? true,
                WeeklyReports = prefs?.WeeklyReports ?? false
            },
            AvatarUrl = prefs?.AvatarUrl,
            Profile = new UserProfileSummaryDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.ToString()
            }
        });
    }

    [HttpPut("me")]
    [HttpPatch("me")]
    public async Task<ActionResult<UserPreferencesDto>> UpdateMyPreferences([FromBody] UpdateUserPreferencesDto? dto)
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();

        var user = await _db.SystemUsers.FindAsync(userId.Value);
        if (user == null) return NotFound();

        if (dto?.AvatarUrl != null && (dto.AvatarUrl.Length > 500 || !Uri.IsWellFormedUriString(dto.AvatarUrl, UriKind.Absolute)))
            return BadRequest(new { message = "avatarUrl must be a valid absolute URL under 500 characters." });

        var prefs = await _db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
        if (prefs == null)
        {
            prefs = new UserPreference { UserId = userId };
            _db.UserPreferences.Add(prefs);
        }

        if (dto?.Notifications != null)
        {
            prefs.EmailAlerts = dto.Notifications.EmailAlerts;
            prefs.PushNotifications = dto.Notifications.PushNotifications;
            prefs.CriticalAlerts = dto.Notifications.CriticalAlerts;
            prefs.WeeklyReports = dto.Notifications.WeeklyReports;
        }
        if (dto?.AvatarUrl != null) prefs.AvatarUrl = dto.AvatarUrl;
        prefs.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new UserPreferencesDto
        {
            Notifications = new NotificationPreferencesDto
            {
                EmailAlerts = prefs.EmailAlerts,
                PushNotifications = prefs.PushNotifications,
                CriticalAlerts = prefs.CriticalAlerts,
                WeeklyReports = prefs.WeeklyReports
            },
            AvatarUrl = prefs.AvatarUrl,
            Profile = new UserProfileSummaryDto { Id = user.Id, FullName = user.FullName, Email = user.Email, Role = user.Role?.ToString() }
        });
    }
}

// Ministry-wide policy config - SuperAdmin only (Backend Action Report ง3.2). High blast radius
// (SessionTimeoutMinutes affects every active session going forward), so every write is audited.
[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = "SuperAdmin")]
public class SystemSettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SystemSettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("system")]
    public async Task<ActionResult<SystemConfigDto>> GetSystemConfig()
    {
        var cfg = await _db.SystemConfigurations.FirstOrDefaultAsync();
        return Ok(new SystemConfigDto
        {
            TwoFactorRequired = cfg?.TwoFactorRequired ?? false,
            SessionTimeoutMinutes = cfg?.SessionTimeoutMinutes ?? 60
        });
    }

    [HttpPut("system")]
    public async Task<ActionResult<SystemConfigDto>> UpdateSystemConfig([FromBody] SystemConfigDto? dto)
    {
        if (dto == null) return BadRequest(new { message = "Request body is required." });
        if (dto.SessionTimeoutMinutes < 5 || dto.SessionTimeoutMinutes > 480)
            return BadRequest(new { message = "sessionTimeoutMinutes must be between 5 and 480." });

        var cfg = await _db.SystemConfigurations.FirstOrDefaultAsync();
        var old = cfg == null ? null : $"2FA={cfg.TwoFactorRequired}, Timeout={cfg.SessionTimeoutMinutes}";
        if (cfg == null)
        {
            cfg = new SystemConfiguration();
            _db.SystemConfigurations.Add(cfg);
        }
        cfg.TwoFactorRequired = dto.TwoFactorRequired;
        cfg.SessionTimeoutMinutes = dto.SessionTimeoutMinutes;
        cfg.UpdatedAt = DateTime.UtcNow;

        var callerEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = callerEmail ?? "SuperAdmin",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.SystemConfigUpdated,
            ResourceType = "SystemConfiguration",
            ResourceId = "system",
            OldValue = old,
            NewValue = $"2FA={cfg.TwoFactorRequired}, Timeout={cfg.SessionTimeoutMinutes}",
            Result = AuditResult.Success,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(dto);
    }
}
```

# FILE: src\EgyMediChain.Api\Controllers\ShipmentsController.cs
```
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// National-level shipments view. FactoryDashboardController and Warehouse/PharmacyDashboardController
// already have their own /shipments endpoints, but those are scoped to one entity - this is the
// Ministry's system-wide summary (what the National Dashboard needs), which didn't exist before.
[ApiController]
[Route("api/shipments")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
public class ShipmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ShipmentsController(AppDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<ActionResult<object>> GetSummary()
    {
        var scope = GetMinistryEntityScope();
        var query = _db.Shipments.AsQueryable();

        query = scope switch
        {
            "Factory" => query.Where(s => s.SourceFactoryId != null),
            "Warehouse" => query.Where(s => s.SourceWarehouseId != null || s.DestinationWarehouseId != null),
            "Pharmacy" => query.Where(s => s.DestinationPharmacyId != null),
            _ => query
        };

        return Ok(new
        {
            TotalShipments = await query.CountAsync(),
            InTransit = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit),
            Delivered = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Delivered),
            PartiallyReceived = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.PartiallyReceived),
            Rejected = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Rejected),
            FactoryToWarehouse = await query.CountAsync(s => s.ShipmentType == EgyMediChain.Domain.Enums.ShipmentType.FactoryToWarehouse),
            WarehouseToPharmacy = await query.CountAsync(s => s.ShipmentType == EgyMediChain.Domain.Enums.ShipmentType.WarehouseToPharmacy)
        });
    }

    private string? GetMinistryEntityScope()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "MinistryAdmin" && role != "MinistryViewer") return null;

        var scope = User.FindFirst("entityType")?.Value;
        if (string.IsNullOrEmpty(scope) || scope == "Ministry") return null;
        return scope;
    }
}
```

# FILE: src\EgyMediChain.Api\Controllers\WarehouseDashboardController.cs
```
using EgyMediChain.Api.Dtos;
using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// Operational portal for a single Warehouse (role: WarehouseUser). Scoped by {warehouseId} in
// the route, same pattern as the Factory dashboard.
[ApiController]
[Route("api/warehouse-dashboard/{warehouseId:int}")]
[Authorize(Roles = "WarehouseUser,SuperAdmin,MinistryAdmin")]
[ValidateEntityOwnership("warehouseId")]
public class WarehouseDashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public WarehouseDashboardController(AppDbContext db) => _db = db;

    private async Task<Warehouse?> GetWarehouseAsync(int warehouseId) => await _db.Warehouses.FindAsync(warehouseId);
    private static bool IsActive(Warehouse w) => w.WarehouseStatus == FacilityStatus.Active;

    // ---------------- Overview ----------------
    [HttpGet("overview")]
    public async Task<ActionResult<WarehouseOverviewDto>> GetOverview(int warehouseId)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var incomingQuery = _db.Shipments.Where(s => s.DestinationWarehouseId == warehouseId);
        var outgoingQuery = _db.Shipments.Where(s => s.SourceWarehouseId == warehouseId);
        var inventoryQuery = _db.InventoryStocks.Where(i => i.WarehouseId == warehouseId);

        var readyStockSkus = await inventoryQuery.Where(i => i.InventoryStatus == InventoryStatus.Active && i.AvailableQuantity > 0)
            .Select(i => i.Batch!.MedicineProductId).Distinct().CountAsync();

        var cards = new WarehouseOverviewCardsDto
        {
            IncomingShipments = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit || s.ShipmentStatus == ShipmentStatus.PendingInspection),
            PendingInspection = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.PendingInspection),
            ReadyStockSkus = readyStockSkus,
            TotalStockUnits = await inventoryQuery.SumAsync(i => i.AvailableQuantity ?? 0),
            OutgoingShipments = await outgoingQuery.CountAsync(),
            OpenAlerts = await _db.Alerts.CountAsync(a => a.EntityType == EntityKind.Warehouse && a.EntityName == warehouse.OfficialWarehouseName && a.AlertStatus == AlertStatus.Open)
        };

        var recentIncoming = await incomingQuery.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .OrderByDescending(s => s.DispatchDate).Take(5)
            .Select(s => ToShipmentListItem(s)).ToListAsync();

        var recentOutgoing = await outgoingQuery.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .OrderByDescending(s => s.DispatchDate).Take(5)
            .Select(s => ToShipmentListItem(s)).ToListAsync();

        var soonCutoff = DateTime.UtcNow.AddDays(90);
        var topProducts = await inventoryQuery.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Where(i => i.Batch != null && i.Batch.MedicineProduct != null)
            .GroupBy(i => new { i.Batch!.MedicineProduct!.ProductName, i.Batch.MedicineProduct.Strength, i.Batch.MedicineProduct.DosageForm })
            .Select(g => new ProductStockSummaryItemDto
            {
                ProductName = g.Key.ProductName,
                Strength = g.Key.Strength,
                DosageForm = g.Key.DosageForm,
                TotalStock = g.Sum(i => i.TotalReceivedQuantity ?? 0),
                AvailableStock = g.Sum(i => i.AvailableQuantity ?? 0),
                InTransit = g.Sum(i => i.ReservedQuantity ?? 0),
                ExpiringSoon = g.Where(i => i.Batch!.ExpiryDate != null && i.Batch.ExpiryDate <= soonCutoff).Sum(i => i.AvailableQuantity ?? 0)
            })
            .OrderByDescending(p => p.TotalStock)
            .Take(5)
            .ToListAsync();

        var recentAlerts = await _db.Alerts.Where(a => a.EntityType == EntityKind.Warehouse && a.EntityName == warehouse.OfficialWarehouseName)
            .Include(a => a.Batch).OrderByDescending(a => a.CreatedAt).Take(5)
            .Select(a => new AlertListItemDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                EntityName = a.EntityName,
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(new WarehouseOverviewDto { Cards = cards, RecentIncomingShipments = recentIncoming, RecentOutgoingShipments = recentOutgoing, TopProducts = topProducts, RecentAlerts = recentAlerts });
    }

    // ---------------- Inventory Summary Cards ----------------
    [HttpGet("inventory/summary")]
    public async Task<ActionResult<WarehouseInventorySummaryCardsDto>> GetInventorySummary(int warehouseId)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var inventoryQuery = _db.InventoryStocks.Include(i => i.Batch).Where(i => i.WarehouseId == warehouseId);
        var soonCutoff = DateTime.UtcNow.AddDays(90);

        return Ok(new WarehouseInventorySummaryCardsDto
        {
            TotalProductsSkus = await inventoryQuery.Select(i => i.Batch!.MedicineProductId).Distinct().CountAsync(),
            TotalReceivedUnits = await inventoryQuery.SumAsync(i => i.TotalReceivedQuantity ?? 0),
            AvailableUnits = await inventoryQuery.SumAsync(i => i.AvailableQuantity ?? 0),
            ReservedUnits = await inventoryQuery.SumAsync(i => i.ReservedQuantity ?? 0),
            QuarantinedUnits = await inventoryQuery.SumAsync(i => i.QuarantinedQuantity ?? 0),
            ExpiringSoonUnits = await inventoryQuery.Where(i => i.Batch != null && i.Batch.ExpiryDate != null && i.Batch.ExpiryDate <= soonCutoff).SumAsync(i => i.AvailableQuantity ?? 0)
        });
    }

    // ---------------- Shipments Summary Cards ----------------
    [HttpGet("shipments/summary")]
    public async Task<ActionResult<object>> GetShipmentsSummary(int warehouseId)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var incomingQuery = _db.Shipments.Where(s => s.DestinationWarehouseId == warehouseId);
        return Ok(new
        {
            IncomingShipments = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit),
            PendingInspection = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.PendingInspection),
            Received = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Received),
            PartiallyReceived = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.PartiallyReceived),
            Rejected = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Rejected)
        });
    }

    // ---------------- Shipments ----------------
    [HttpGet("shipments/incoming")]
    public async Task<ActionResult<PagedResult<ShipmentListItemDto>>> GetIncoming(int warehouseId,
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct).Where(s => s.DestinationWarehouseId == warehouseId);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.ShipmentStatus != null && s.ShipmentStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.DispatchDate)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(s => ToShipmentListItem(s)).ToListAsync();
        return Ok(new PagedResult<ShipmentListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("shipments/outgoing")]
    public async Task<ActionResult<PagedResult<ShipmentListItemDto>>> GetOutgoing(int warehouseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct).Where(s => s.SourceWarehouseId == warehouseId);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.DispatchDate)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(s => ToShipmentListItem(s)).ToListAsync();
        return Ok(new PagedResult<ShipmentListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("shipments/{shipmentId:int}")]
    public async Task<ActionResult<ShipmentDetailsDto>> GetShipmentDetails(int warehouseId, int shipmentId)
    {
        var s = await _db.Shipments.Include(x => x.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(x => x.Id == shipmentId && (x.DestinationWarehouseId == warehouseId || x.SourceWarehouseId == warehouseId));
        if (s == null) return NotFound(new { message = "Shipment not found for this warehouse." });

        return Ok(new ShipmentDetailsDto
        {
            Id = s.Id,
            TransferCode = s.TransferCode,
            ShipmentType = s.ShipmentType?.ToString(),
            Source = s.Source,
            Destination = s.Destination,
            ProductName = s.Batch?.MedicineProduct?.ProductName,
            BatchNumber = s.Batch?.BatchNumber,
            ExpectedQuantity = s.ExpectedQuantity,
            ReceivedQuantity = s.ReceivedQuantity,
            ShipmentStatus = s.ShipmentStatus?.ToString(),
            RequiresColdChain = s.RequiresColdChain,
            DispatchDate = s.DispatchDate,
            ReceivedDate = s.ReceivedDate,
            Notes = s.Notes,
            InspectionResult = s.InspectionResult
        });
    }

    [HttpPost("shipments/{shipmentId:int}/receive")]
    public async Task<IActionResult> ReceiveShipment(int warehouseId, int shipmentId, [FromBody] ReceiveShipmentDto? dto)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });
        if (!IsActive(warehouse)) return Conflict(new { message = "This warehouse is not Active. Operational actions are disabled." });

        var shipment = await _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(s => s.Id == shipmentId && s.DestinationWarehouseId == warehouseId);
        if (shipment == null) return NotFound(new { message = "Shipment not found for this warehouse." });

        if (shipment.RequiresColdChain == true && warehouse.HasColdStorage != true)
            return Conflict(new { message = "This batch requires cold storage. This warehouse is not marked as cold storage capable." });

        var expected = shipment.ExpectedQuantity ?? 0;
        var inspection = dto?.InspectionResult ?? "Accepted";
        long received = inspection == "Rejected" ? 0 : (dto?.ReceivedQuantity ?? expected);

        shipment.ReceivedQuantity = received;
        shipment.InspectionResult = inspection;
        shipment.Notes = dto?.Notes ?? shipment.Notes;
        shipment.ReceivedDate = DateTime.UtcNow;

        if (inspection == "Rejected" || received == 0)
        {
            shipment.ShipmentStatus = ShipmentStatus.Rejected;
            _db.Alerts.Add(NewAlert(AlertType.ComplianceIssue, AlertSeverity.High, EntityKind.Warehouse, warehouse.OfficialWarehouseName, shipment.BatchId, shipment.Id,
                $"Shipment {shipment.TransferCode} was rejected on receipt at {warehouse.OfficialWarehouseName}."));
            _db.AuditLogs.Add(NewLog(AuditAction.RejectShipment, "Shipment", shipment.TransferCode, "InTransit", "Rejected"));
        }
        else if (received < expected)
        {
            shipment.ShipmentStatus = ShipmentStatus.PartiallyReceived;
            UpsertInventory(warehouseId, shipment.BatchId, warehouse.OfficialWarehouseName, received);
            _db.Alerts.Add(NewAlert(AlertType.QuantityMismatch, AlertSeverity.Medium, EntityKind.Warehouse, warehouse.OfficialWarehouseName, shipment.BatchId, shipment.Id,
                $"Received quantity ({received}) does not match expected quantity ({expected}) for shipment {shipment.TransferCode}."));
            _db.AuditLogs.Add(NewLog(AuditAction.ReceiveShipment, "Shipment", shipment.TransferCode, "InTransit", "PartiallyReceived"));
        }
        else
        {
            shipment.ShipmentStatus = ShipmentStatus.Received;
            UpsertInventory(warehouseId, shipment.BatchId, warehouse.OfficialWarehouseName, received);
            _db.AuditLogs.Add(NewLog(AuditAction.ReceiveShipment, "Shipment", shipment.TransferCode, "InTransit", "Received"));
        }

        if (shipment.Batch != null && shipment.ShipmentStatus != ShipmentStatus.Rejected)
        {
            shipment.Batch.SupplyChainStage = SupplyChainStage.Stored;
            shipment.Batch.CurrentLocation = warehouse.OfficialWarehouseName;
            shipment.Batch.BatchStatus = BatchStatus.InWarehouse;
            shipment.Batch.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = $"Shipment {shipment.ShipmentStatus}.", status = shipment.ShipmentStatus.ToString(), receivedQuantity = received });
    }

    private void UpsertInventory(int warehouseId, int? batchId, string? holderName, long receivedQty)
    {
        var stock = _db.InventoryStocks.FirstOrDefault(i => i.WarehouseId == warehouseId && i.BatchId == batchId);
        if (stock == null)
        {
            _db.InventoryStocks.Add(new InventoryStock
            {
                BatchId = batchId,
                HolderType = "Warehouse",
                HolderName = holderName,
                WarehouseId = warehouseId,
                TotalReceivedQuantity = receivedQty,
                AvailableQuantity = receivedQty,
                ReservedQuantity = 0,
                QuarantinedQuantity = 0,
                InventoryStatus = InventoryStatus.Active,
                LastUpdated = DateTime.UtcNow
            });
        }
        else
        {
            stock.TotalReceivedQuantity = (stock.TotalReceivedQuantity ?? 0) + receivedQty;
            stock.AvailableQuantity = (stock.AvailableQuantity ?? 0) + receivedQty;
            stock.LastUpdated = DateTime.UtcNow;
        }
    }

    // ---------------- Inventory & Dispatch ----------------
    [HttpGet("inventory")]
    public async Task<ActionResult<PagedResult<InventoryStockListItemDto>>> GetInventory(int warehouseId,
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.InventoryStocks.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(i => i.Batch).ThenInclude(b => b!.Factory)
            .Where(i => i.WarehouseId == warehouseId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i => (i.Batch != null && i.Batch.BatchNumber != null && i.Batch.BatchNumber.Contains(search)) ||
                                      (i.Batch != null && i.Batch.MedicineProduct != null && i.Batch.MedicineProduct.ProductName != null && i.Batch.MedicineProduct.ProductName.Contains(search)));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.InventoryStatus != null && i.InventoryStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.LastUpdated)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(i => ToInventoryListItem(i)).ToListAsync();

        return Ok(new PagedResult<InventoryStockListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("inventory/{inventoryId:int}")]
    public async Task<ActionResult<InventoryStockDetailsDto>> GetInventoryDetails(int warehouseId, int inventoryId)
    {
        var stock = await _db.InventoryStocks.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(i => i.Batch).ThenInclude(b => b!.Factory)
            .FirstOrDefaultAsync(i => i.Id == inventoryId && i.WarehouseId == warehouseId);
        if (stock == null) return NotFound(new { message = "Stock record not found for this warehouse." });

        var shipments = await _db.Shipments.Where(s => s.BatchId == stock.BatchId && (s.DestinationWarehouseId == warehouseId || s.SourceWarehouseId == warehouseId))
            .Select(s => new ShipmentSummaryItemDto
            {
                TransferCode = s.TransferCode,
                ShipmentType = s.ShipmentType.ToString(),
                Source = s.Source,
                Destination = s.Destination,
                ExpectedQuantity = s.ExpectedQuantity,
                ReceivedQuantity = s.ReceivedQuantity,
                ShipmentStatus = s.ShipmentStatus.ToString(),
                DispatchDate = s.DispatchDate,
                ReceivedDate = s.ReceivedDate
            }).ToListAsync();

        return Ok(new InventoryStockDetailsDto
        {
            Id = stock.Id,
            ProductInfo = new ProductInfoDto
            {
                ProductName = stock.Batch?.MedicineProduct?.ProductName,
                GTIN = stock.Batch?.MedicineProduct?.GTIN,
                DosageForm = stock.Batch?.MedicineProduct?.DosageForm,
                Strength = stock.Batch?.MedicineProduct?.Strength,
                RequiresColdChain = stock.Batch?.MedicineProduct?.RequiresColdChain,
                ProductStatus = stock.Batch?.MedicineProduct?.ProductStatus
            },
            BatchInfo = new BatchInfoDto
            {
                BatchNumber = stock.Batch?.BatchNumber,
                FactoryName = stock.Batch?.Factory?.OfficialFactoryName,
                ManufacturingDate = stock.Batch?.ManufacturingDate,
                ExpiryDate = stock.Batch?.ExpiryDate,
                BatchStatus = stock.Batch?.BatchStatus?.ToString(),
                SupplyChainStage = stock.Batch?.SupplyChainStage?.ToString()
            },
            WarehouseInventory = new InventoryDistributionItemDto
            {
                HolderType = stock.HolderType,
                HolderName = stock.HolderName,
                TotalReceivedQuantity = stock.TotalReceivedQuantity,
                AvailableQuantity = stock.AvailableQuantity,
                ReservedQuantity = stock.ReservedQuantity,
                QuarantinedQuantity = stock.QuarantinedQuantity,
                InventoryStatus = stock.InventoryStatus?.ToString(),
                LastUpdated = stock.LastUpdated
            },
            RelatedShipments = shipments
        });
    }

    [HttpPost("inventory/{inventoryId:int}/dispatch-to-pharmacy")]
    public async Task<IActionResult> DispatchToPharmacy(int warehouseId, int inventoryId, [FromBody] DispatchToPharmacyDto? dto)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });
        if (!IsActive(warehouse)) return Conflict(new { message = "This warehouse is not Active. Operational actions are disabled." });

        var stock = await _db.InventoryStocks.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(i => i.Id == inventoryId && i.WarehouseId == warehouseId);
        if (stock == null) return NotFound(new { message = "Stock record not found for this warehouse." });

        if (stock.Batch?.BatchStatus is BatchStatus.Quarantined or BatchStatus.Recalled or BatchStatus.Expired or BatchStatus.Cancelled ||
            stock.InventoryStatus is InventoryStatus.Blocked or InventoryStatus.Recalled or InventoryStatus.Quarantined)
            return Conflict(new { message = "This batch/stock is not available for dispatch (quarantined, recalled or blocked)." });

        var pharmacy = await _db.Pharmacies.FindAsync(dto?.TargetPharmacyId ?? 0);
        if (pharmacy == null || pharmacy.PharmacyStatus != FacilityStatus.Active)
            return BadRequest(new { message = "Please select an active target pharmacy." });

        if (stock.Batch?.MedicineProduct?.RequiresColdChain == true && pharmacy.HasColdStorage != true)
            return Conflict(new { message = "This batch requires cold storage. Please select a pharmacy with cold storage capability." });

        var dispatchQty = dto?.DispatchQuantity ?? 0;
        if (dispatchQty <= 0 || dispatchQty > (stock.AvailableQuantity ?? 0))
            return BadRequest(new { message = $"Dispatch quantity must be between 1 and the available quantity ({stock.AvailableQuantity ?? 0})." });

        var shipment = new Shipment
        {
            TransferCode = $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}",
            BatchId = stock.BatchId,
            ShipmentType = ShipmentType.WarehouseToPharmacy,
            Source = warehouse.OfficialWarehouseName,
            Destination = pharmacy.OfficialPharmacyName,
            SourceWarehouseId = warehouseId,
            DestinationPharmacyId = pharmacy.Id,
            ExpectedQuantity = dispatchQty,
            ShipmentStatus = ShipmentStatus.InTransit,
            RequiresColdChain = stock.Batch?.MedicineProduct?.RequiresColdChain ?? false,
            Notes = dto?.Notes,
            DispatchDate = dto?.DispatchDate ?? DateTime.UtcNow
        };
        _db.Shipments.Add(shipment);

        stock.AvailableQuantity = (stock.AvailableQuantity ?? 0) - dispatchQty;
        stock.ReservedQuantity = (stock.ReservedQuantity ?? 0) + dispatchQty;
        stock.LastUpdated = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(AuditAction.DispatchShipment, "Shipment", shipment.TransferCode, "Active", "InTransit"));
        await _db.SaveChangesAsync();

        return Ok(new { message = "Shipment dispatched to pharmacy.", transferCode = shipment.TransferCode });
    }

    [HttpPost("inventory/{inventoryId:int}/move-to-quarantine")]
    public async Task<IActionResult> MoveToQuarantine(int warehouseId, int inventoryId, [FromBody] MoveToQuarantineDto? dto)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });
        if (!IsActive(warehouse)) return Conflict(new { message = "This warehouse is not Active. Operational actions are disabled." });
        if (warehouse.HasQuarantineArea != true)
            return Conflict(new { message = "This warehouse does not have a quarantine area. Use Report Issue instead." });

        var stock = await _db.InventoryStocks.FirstOrDefaultAsync(i => i.Id == inventoryId && i.WarehouseId == warehouseId);
        if (stock == null) return NotFound(new { message = "Stock record not found for this warehouse." });

        var qty = dto?.QuarantineQuantity ?? (stock.AvailableQuantity ?? 0);
        if (qty <= 0 || qty > (stock.AvailableQuantity ?? 0))
            return BadRequest(new { message = $"Quarantine quantity must be between 1 and the available quantity ({stock.AvailableQuantity ?? 0})." });

        stock.AvailableQuantity = (stock.AvailableQuantity ?? 0) - qty;
        stock.QuarantinedQuantity = (stock.QuarantinedQuantity ?? 0) + qty;
        stock.InventoryStatus = InventoryStatus.Quarantined;
        stock.LastUpdated = DateTime.UtcNow;

        _db.Alerts.Add(NewAlert(AlertType.ComplianceIssue, AlertSeverity.Medium, EntityKind.Warehouse, warehouse.OfficialWarehouseName, stock.BatchId, null,
            dto?.Reason ?? $"{qty} units moved to quarantine at {warehouse.OfficialWarehouseName}."));
        _db.AuditLogs.Add(NewLog(AuditAction.QuarantineStock, "InventoryStock", stock.Id.ToString(), "Active", "Quarantined"));

        await _db.SaveChangesAsync();
        return Ok(new { message = "Stock moved to quarantine.", quarantinedQuantity = stock.QuarantinedQuantity });
    }

    // ---------------- Report Issue ----------------
    [HttpPost("report-issue")]
    public async Task<IActionResult> ReportIssue(int warehouseId, [FromBody] ReportIssueDto? dto)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });
        if (!IsActive(warehouse)) return Conflict(new { message = "This warehouse is not Active. Operational actions are disabled." });

        var alertType = (dto?.AlertType ?? "ComplianceIssue") switch
        {
            "ColdChainIssue" => AlertType.ColdChainIssue,
            "QuantityMismatch" => AlertType.QuantityMismatch,
            "DamagedPackage" => AlertType.DamagedPackage,
            _ => AlertType.ComplianceIssue
        };

        var alert = NewAlert(alertType, AlertSeverity.Medium, EntityKind.Warehouse, warehouse.OfficialWarehouseName, dto?.BatchId, dto?.ShipmentId,
            dto?.Message ?? "Issue reported by warehouse.");
        _db.Alerts.Add(alert);
        _db.AuditLogs.Add(NewLog(AuditAction.CreateAlert, "Alert", alert.AlertCode, null, "Open"));

        await _db.SaveChangesAsync();
        return Ok(new { message = "Issue reported to the Ministry.", alertCode = alert.AlertCode });
    }

    // ---------------- Alerts (view-only) ----------------
    [HttpGet("alerts")]
    public async Task<ActionResult<PagedResult<AlertListItemDto>>> GetAlerts(int warehouseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var query = _db.Alerts.Include(a => a.Batch).Where(a => a.EntityType == EntityKind.Warehouse && a.EntityName == warehouse.OfficialWarehouseName);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(a => new AlertListItemDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                EntityName = a.EntityName,
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<AlertListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // ---------------- Profile ----------------
    [HttpGet("profile")]
    public async Task<ActionResult<OperationalProfileDto>> GetProfile(int warehouseId)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var user = await _db.SystemUsers.FirstOrDefaultAsync(u => u.EntityType == EntityKind.Warehouse && u.EntityId == warehouseId);
        var docs = await _db.EntityDocuments
            .Where(d => d.RegistrationRequest != null && d.RegistrationRequest.WarehouseId == warehouseId)
            .Select(d => new DocumentItemDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                UploadedAt = d.UploadedAt,
                DocumentStatus = d.DocumentStatus.ToString(),
                ReviewedBy = d.ReviewedBy,
                ReviewedAt = d.ReviewedAt,
                RejectionReason = d.RejectionReason
            }).ToListAsync();

        return Ok(new OperationalProfileDto
        {
            Account = user == null ? null : new AccountInfoDto
            {
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                NationalIdMasked = MaskNationalId(user.NationalId),
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive
            },
            Entity = new EntityInfoDto
            {
                WarehouseCode = warehouse.WarehouseCode,
                OfficialWarehouseName = warehouse.OfficialWarehouseName,
                WarehouseType = warehouse.WarehouseType,
                Governorate = warehouse.Governorate,
                City = warehouse.City,
                DistrictArea = warehouse.DistrictArea,
                FullAddress = warehouse.FullAddress,
                Phone = warehouse.Phone,
                Email = warehouse.Email,
                WarehouseLicenseNumber = warehouse.WarehouseLicenseNumber,
                HasColdStorage = warehouse.HasColdStorage,
                HasQuarantineArea = warehouse.HasQuarantineArea,
                HasDeliveryService = warehouse.HasDeliveryService,
                LicenseIssueDate = warehouse.LicenseIssueDate,
                LicenseExpiryDate = warehouse.LicenseExpiryDate,
                Status = warehouse.WarehouseStatus?.ToString()
            },
            Documents = docs
        });
    }

    private static string? MaskNationalId(string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length < 4) return nationalId;
        return $"**** **** {nationalId[^4..]}";
    }

    private static ShipmentListItemDto ToShipmentListItem(Shipment s) => new()
    {
        Id = s.Id,
        TransferCode = s.TransferCode,
        ProductName = s.Batch?.MedicineProduct?.ProductName,
        GTIN = s.Batch?.MedicineProduct?.GTIN,
        BatchNumber = s.Batch?.BatchNumber,
        Source = s.Source,
        Destination = s.Destination,
        ExpectedQuantity = s.ExpectedQuantity,
        ReceivedQuantity = s.ReceivedQuantity,
        RequiresColdChain = s.RequiresColdChain,
        ShipmentStatus = s.ShipmentStatus?.ToString(),
        DispatchDate = s.DispatchDate,
        ReceivedDate = s.ReceivedDate
    };

    private static InventoryStockListItemDto ToInventoryListItem(InventoryStock i) => new()
    {
        Id = i.Id,
        ProductName = i.Batch?.MedicineProduct?.ProductName,
        GTIN = i.Batch?.MedicineProduct?.GTIN,
        DosageForm = i.Batch?.MedicineProduct?.DosageForm,
        Strength = i.Batch?.MedicineProduct?.Strength,
        BatchNumber = i.Batch?.BatchNumber,
        FactoryName = i.Batch?.Factory?.OfficialFactoryName,
        TotalReceivedQuantity = i.TotalReceivedQuantity,
        AvailableQuantity = i.AvailableQuantity,
        ReservedQuantity = i.ReservedQuantity,
        QuarantinedQuantity = i.QuarantinedQuantity,
        ExpiryDate = i.Batch?.ExpiryDate,
        RequiresColdChain = i.Batch?.MedicineProduct?.RequiresColdChain,
        InventoryStatus = i.InventoryStatus?.ToString()
    };

    private static Alert NewAlert(AlertType type, AlertSeverity severity, EntityKind entityKind, string? entityName, int? batchId, int? shipmentId, string message) => new()
    {
        AlertCode = $"ALERT-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
        AlertType = type,
        Severity = severity,
        EntityType = entityKind,
        EntityName = entityName,
        BatchId = batchId,
        ShipmentId = shipmentId,
        Message = message,
        AlertStatus = AlertStatus.Open,
        CreatedAt = DateTime.UtcNow
    };

    private static AuditLog NewLog(AuditAction action, string resourceType, string? resourceId, string? oldVal, string? newVal) => new()
    {
        LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
        UserDisplayName = "Warehouse User",
        Role = SystemRole.WarehouseUser,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        NewValue = newVal,
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}

```

# FILE: src\EgyMediChain.Api\Controllers\WarehousesController.cs
```
using EgyMediChain.Api.Dtos;
using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
[RequireMinistryScope("Warehouse")]
public class WarehousesController : ControllerBase
{
    private readonly AppDbContext _db;
    public WarehousesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<WarehouseListItemDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var query = _db.Warehouses.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(w => w.OfficialWarehouseName != null && w.OfficialWarehouseName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(w => w.WarehouseStatus != null && w.WarehouseStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderBy(w => w.Id)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 5 : pageSize)
            .Select(w => new WarehouseListItemDto
            {
                Id = w.Id,
                WarehouseName = w.OfficialWarehouseName,
                WarehouseType = w.WarehouseType,
                Governorate = w.Governorate,
                City = w.City,
                LicenseExpiryDate = w.LicenseExpiryDate,
                HasColdStorage = w.HasColdStorage,
                HasQuarantineArea = w.HasQuarantineArea,
                HasDeliveryService = w.HasDeliveryService,
                WarehouseStatus = w.WarehouseStatus.ToString(),
                CreatedAt = w.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<WarehouseListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WarehouseProfileDto>> GetById(int id)
    {
        var w = await _db.Warehouses.FirstOrDefaultAsync(x => x.Id == id);
        if (w == null) return NotFound(new { message = "Warehouse not found." });

        return Ok(new WarehouseProfileDto
        {
            Id = w.Id,
            OfficialWarehouseName = w.OfficialWarehouseName,
            WarehouseType = w.WarehouseType,
            Governorate = w.Governorate,
            City = w.City,
            DistrictArea = w.DistrictArea,
            FullAddress = w.FullAddress,
            WarehouseLicenseNumber = w.WarehouseLicenseNumber,
            LicenseIssueDate = w.LicenseIssueDate,
            LicenseExpiryDate = w.LicenseExpiryDate,
            HasColdStorage = w.HasColdStorage,
            HasQuarantineArea = w.HasQuarantineArea,
            HasDeliveryService = w.HasDeliveryService,
            WarehouseStatus = w.WarehouseStatus?.ToString(),
            CreatedAt = w.CreatedAt,
            UpdatedAt = w.UpdatedAt
        });
    }

    [HttpPost("{id:int}/suspend")]
    public async Task<IActionResult> Suspend(int id, [FromBody] EntityActionDto? dto)
    {
        var w = await _db.Warehouses.FindAsync(id);
        if (w == null) return NotFound(new { message = "Warehouse not found." });
        var old = w.WarehouseStatus?.ToString();
        w.WarehouseStatus = FacilityStatus.Suspended;
        w.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SuspendEntity, "Warehouse", w.WarehouseLicenseNumber, old, "Suspended", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Warehouse suspended.", status = "Suspended" });
    }

    [HttpPost("{id:int}/reactivate")]
    public async Task<IActionResult> Reactivate(int id, [FromBody] EntityActionDto? dto)
    {
        var w = await _db.Warehouses.FindAsync(id);
        if (w == null) return NotFound(new { message = "Warehouse not found." });
        var old = w.WarehouseStatus?.ToString();
        w.WarehouseStatus = FacilityStatus.Active;
        w.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.ReactivateEntity, "Warehouse", w.WarehouseLicenseNumber, old, "Active", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Warehouse reactivated.", status = "Active" });
    }

    [HttpPost("{id:int}/set-inactive")]
    public async Task<IActionResult> SetInactive(int id, [FromBody] EntityActionDto? dto)
    {
        var w = await _db.Warehouses.FindAsync(id);
        if (w == null) return NotFound(new { message = "Warehouse not found." });
        var old = w.WarehouseStatus?.ToString();
        w.WarehouseStatus = FacilityStatus.Inactive;
        w.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SetInactiveEntity, "Warehouse", w.WarehouseLicenseNumber, old, "Inactive", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Warehouse set to inactive.", status = "Inactive" });
    }

    [HttpGet("{id:int}/inventory")]
    public async Task<ActionResult<PagedResult<InventoryDistributionItemDto>>> GetInventory(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var wh = await _db.Warehouses.FindAsync(id);
        if (wh == null) return NotFound(new { message = "Warehouse not found." });

        var query = _db.InventoryStocks.Where(i => i.HolderType == "Warehouse" && i.HolderName == wh.OfficialWarehouseName);
        var total = await query.CountAsync();
        var items = await query.Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(i => new InventoryDistributionItemDto
            {
                HolderType = i.HolderType,
                HolderName = i.HolderName,
                TotalReceivedQuantity = i.TotalReceivedQuantity,
                AvailableQuantity = i.AvailableQuantity,
                ReservedQuantity = i.ReservedQuantity,
                QuarantinedQuantity = i.QuarantinedQuantity,
                InventoryStatus = i.InventoryStatus.ToString(),
                LastUpdated = i.LastUpdated
            }).ToListAsync();
        return Ok(new PagedResult<InventoryDistributionItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}/shipments")]
    public async Task<ActionResult<PagedResult<ShipmentSummaryItemDto>>> GetShipments(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var wh = await _db.Warehouses.FindAsync(id);
        if (wh == null) return NotFound(new { message = "Warehouse not found." });

        var query = _db.Shipments.Where(s => s.Source == wh.OfficialWarehouseName || s.Destination == wh.OfficialWarehouseName);
        var total = await query.CountAsync();
        var items = await query.Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(s => new ShipmentSummaryItemDto
            {
                TransferCode = s.TransferCode,
                ShipmentType = s.ShipmentType.ToString(),
                Source = s.Source,
                Destination = s.Destination,
                ExpectedQuantity = s.ExpectedQuantity,
                ReceivedQuantity = s.ReceivedQuantity,
                ShipmentStatus = s.ShipmentStatus.ToString(),
                DispatchDate = s.DispatchDate,
                ReceivedDate = s.ReceivedDate
            }).ToListAsync();
        return Ok(new PagedResult<ShipmentSummaryItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}/registration-request")]
    public async Task<ActionResult<EntityRegistrationRequestRefDto>> GetRegistrationRequest(int id)
    {
        var r = await _db.RegistrationRequests
            .Where(x => x.WarehouseId == id)
            .OrderByDescending(x => x.SubmittedAt)
            .FirstOrDefaultAsync();
        if (r == null) return NotFound(new { message = "No registration request found for this warehouse." });

        return Ok(new EntityRegistrationRequestRefDto
        {
            Id = r.Id,
            RequestCode = r.RequestCode,
            RegistrationStatus = r.RegistrationStatus?.ToString(),
            SubmittedAt = r.SubmittedAt
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? search, [FromQuery] string? status)
    {
        var query = _db.Warehouses.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(w => w.OfficialWarehouseName != null && w.OfficialWarehouseName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(w => w.WarehouseStatus != null && w.WarehouseStatus.ToString() == status);

        var rows = await query.OrderBy(w => w.Id).ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,WarehouseName,WarehouseType,Governorate,City,LicenseExpiryDate,HasColdStorage,HasQuarantineArea,HasDeliveryService,Status,CreatedAt");
        foreach (var w in rows)
        {
            sb.AppendLine(string.Join(",",
                w.Id,
                Csv(w.OfficialWarehouseName), Csv(w.WarehouseType), Csv(w.Governorate), Csv(w.City),
                w.LicenseExpiryDate?.ToString("yyyy-MM-dd"), w.HasColdStorage, w.HasQuarantineArea, w.HasDeliveryService,
                Csv(w.WarehouseStatus?.ToString()), w.CreatedAt?.ToString("yyyy-MM-dd")));
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"warehouses-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Contains(',') || value.Contains('"')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    private static AuditLog Log(AuditAction action, string resourceType, string? resourceId, string? oldVal, string? newVal, string? reason = null) => new()
    {
        LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
        UserDisplayName = "Dr. Saif",
        Role = SystemRole.SuperAdmin,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        NewValue = string.IsNullOrWhiteSpace(reason) ? newVal : $"{newVal} (Reason: {reason})",
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}

```

# FILE: src\EgyMediChain.Api\Dtos\Dtos.cs
```
namespace EgyMediChain.Api.Dtos;

// ---------------- Auth ----------------
public class LoginRequestDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class LoginResponseDto
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public int? UserId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
}

public class RefreshRequestDto
{
    public string? RefreshToken { get; set; }
}

// ---------------- Overview ----------------
public class OverviewCardsDto
{
    public int PendingRequests { get; set; }
    public int ActiveFactories { get; set; }
    public int ActiveWarehouses { get; set; }
    public int ActivePharmacies { get; set; }
    // Totals added so the Entities Management screen doesn't need 3 extra "?pageSize=1"
    // requests just to read totalCount (EntitiesManagement-Backend-Gaps.md, item 4).
    public int TotalFactories { get; set; }
    public int TotalWarehouses { get; set; }
    public int TotalPharmacies { get; set; }
    public int ActiveBatches { get; set; }
    public int ShipmentsInTransit { get; set; }
    public int OpenAlerts { get; set; }
    public int SuspiciousPublicScans { get; set; }
}

public class OverviewDto
{
    public OverviewCardsDto? Cards { get; set; }
    public List<RecentRegistrationRequestDto>? RecentRegistrationRequests { get; set; }
    public List<RecentAlertDto>? RecentAlerts { get; set; }
    public List<RecentBatchActivityDto>? RecentBatchActivity { get; set; }
}

public class RecentRegistrationRequestDto
{
    public int Id { get; set; }
    public string? RequestCode { get; set; }
    public string? EntityType { get; set; }
    public string? EntityName { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? RegistrationStatus { get; set; }
}

public class RecentAlertDto
{
    public int Id { get; set; }
    public string? AlertCode { get; set; }
    public string? AlertType { get; set; }
    public string? Severity { get; set; }
    public string? EntityType { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? AlertStatus { get; set; }
}

public class RecentBatchActivityDto
{
    public int Id { get; set; }
    public string? ProductName { get; set; }
    public string? BatchNumber { get; set; }
    public string? FactoryName { get; set; }
    public string? BatchStatus { get; set; }
    public string? SupplyChainStage { get; set; }
    public DateTime? LastUpdated { get; set; }
}

// ---------------- Registration Requests ----------------
public class RegistrationRequestListItemDto
{
    public int Id { get; set; }
    public string? RequestCode { get; set; }
    public string? EntityType { get; set; }
    public string? EntityName { get; set; }
    public string? RepresentativeName { get; set; }
    // Same person/value as RepresentativeName - added because the frontend's "Submitted By"
    // column reads more naturally from a field with this name. Both are always equal.
    public string? SubmittedBy { get; set; }
    public string? Email { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool? EmailConfirmed { get; set; }
    public string? DocumentsOverallStatus { get; set; }
    public string? RegistrationStatus { get; set; }
}

// GET /api/registration-requests/counts - typed replacement for the previously untyped `object`
// response (Alerts_Backend_Gaps_Report.md, item 1, mentioned as a shared issue).
public class RegistrationRequestsCountsDto
{
    public int PendingReview { get; set; }
    public int NeedsMoreDocuments { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
}

// PUT /api/admin/users/{id} - Edit Staff (Backend Action Report ง1.3).
// NationalId and password fields are deliberately excluded - see AdminController for the
// explicit 400 rejection if a client sends them anyway.
public class UpdateMinistryAdminDto
{
    public string? FullName { get; set; }
    public string? MobileNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? OfficialEmail { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Role { get; set; }
    public string? Department { get; set; }
    public string? Facility { get; set; }
    public string? Status { get; set; } // Active | Inactive | Suspended
    public string? Qualification { get; set; }
    public string? JobGrade { get; set; }
    public DateTime? HireDate { get; set; }
    public string? InsuranceNumber { get; set; }

    // Present only so the controller can detect and reject a misuse of this endpoint (ง1.5) -
    // these are never applied even if sent.
    public string? NationalId { get; set; }
    public string? Password { get; set; }
    public string? TemporaryPassword { get; set; }
}

public class GenericMessageResponseDto
{
    public string? Message { get; set; }
}

// ---- Forgot Password flow (Backend Action Report ง2) ----
public class ForgotPasswordRequestDto
{
    public string? Email { get; set; }
}

public class VerifyResetCodeRequestDto
{
    public string? Email { get; set; }
    public string? Code { get; set; }
}

public class VerifyResetCodeResponseDto
{
    public string? ResetToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class ResetPasswordWithTokenDto
{
    public string? ResetToken { get; set; }
    public string? NewPassword { get; set; }
}

// ---- Settings (Backend Action Report ง3) ----
public class NotificationPreferencesDto
{
    public bool EmailAlerts { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool CriticalAlerts { get; set; } = true;
    public bool WeeklyReports { get; set; }
}

public class UpdateUserPreferencesDto
{
    public NotificationPreferencesDto? Notifications { get; set; }
    public string? AvatarUrl { get; set; }
}

public class UserProfileSummaryDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}

public class UserPreferencesDto
{
    public NotificationPreferencesDto? Notifications { get; set; }
    public UserProfileSummaryDto? Profile { get; set; }
    public string? AvatarUrl { get; set; }
}

public class SystemConfigDto
{
    public bool TwoFactorRequired { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 60;
}

// ---- Reports (Backend Action Report ง4) ----
public class GenerateReportRequestDto
{
    public string? ReportType { get; set; } // BatchTraceability | RecallSummary | InventorySnapshot | AuditTrailExport | StaffDirectory
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Format { get; set; } // only "Csv" is implemented for now - see ReportsController
}

public class ReportJobDto
{
    public Guid JobId { get; set; }
    public string? Status { get; set; }
    public DateTime? RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AccountInfoDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalIdMasked { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? IsActive { get; set; }
}

public class EntityInfoDto
{
    // Common address fields
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? Status { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Factory
    public string? FactoryCode { get; set; }
    public string? OfficialFactoryName { get; set; }
    public string? LegalCompanyName { get; set; }
    public string? DosageFormsProduced { get; set; }
    public string? FactoryLicenseNumber { get; set; }
    public string? TechnicalOperatingLicenseNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? TaxCardNumber { get; set; }
    public bool? HasQualityControlLab { get; set; }
    public bool? HasFinishedGoodsStore { get; set; }

    // Warehouse
    public string? WarehouseCode { get; set; }
    public string? OfficialWarehouseName { get; set; }
    public string? WarehouseType { get; set; }
    public string? WarehouseLicenseNumber { get; set; }
    public bool? HasDeliveryService { get; set; }

    // Pharmacy
    public string? PharmacyCode { get; set; }
    public string? OfficialPharmacyName { get; set; }
    public string? PharmacyType { get; set; }
    public string? DefaultWarehouseName { get; set; }
    public string? PharmacyLicenseNumber { get; set; }
    public string? PharmacistSyndicateId { get; set; }

    // Shared
    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
}

public class FactoryDetailsDto
{
    public int? EstablishedYear { get; set; }
    public int? TotalProductionLines { get; set; }
    public string? MainProductionTypes { get; set; }
    public bool? ColdChainCapable { get; set; }
    public string? StorageTypes { get; set; }
    public string? QualityCertificates { get; set; }
    public string? Description { get; set; }
}

public class RegistrationInfoDto
{
    public string? RegistrationRequestNo { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? RegistrationStatus { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public string? Notes { get; set; }
}

public class LicenseItemDto
{
    public int Id { get; set; }
    public string? LicenseType { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Status { get; set; }
    public string? FileUrl { get; set; }
}

public class FactoryProfileFullDto
{
    public int Id { get; set; }
    public string? FactoryName { get; set; }
    public string? FactoryCode { get; set; }
    public string? FactoryStatus { get; set; }
    public string? RegistrationStatus { get; set; }
    public DateTime? MemberSince { get; set; }
    public AccountInfoDto? Account { get; set; }
    public EntityInfoDto? Entity { get; set; }
    public FactoryDetailsDto? FactoryDetails { get; set; }
    public RegistrationInfoDto? RegistrationInfo { get; set; }
    public List<LicenseItemDto>? Licenses { get; set; }
    public List<DocumentItemDto>? Documents { get; set; }
}

public class DocumentItemDto
{
    public int Id { get; set; }
    public string? DocumentType { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public DateTime? UploadedAt { get; set; }
    public string? DocumentStatus { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}

public class RegistrationRequestDetailsDto
{
    public int Id { get; set; }
    public string? RequestCode { get; set; }
    public string? EntityType { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? RegistrationStatus { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? InspectionScheduledDate { get; set; }
    public string? InspectorNotes { get; set; }
    public AccountInfoDto? Account { get; set; }
    public EntityInfoDto? Entity { get; set; }
    public List<DocumentItemDto>? Documents { get; set; }
    // Flat label/value list - built from Account + Entity so the frontend's ReviewRequestModal
    // can just iterate one array instead of knowing every field in EntityInfoDto ahead of time.
    public List<FieldItemDto>? Fields { get; set; }
}

public class FieldItemDto
{
    public string? Label { get; set; }
    public string? Value { get; set; }
}

public class ApproveRequestDto
{
    public string? AdminNotes { get; set; }
}

public class RequestInspectionDto
{
    public DateTime? ScheduledDate { get; set; }
    public string? InspectorNotes { get; set; }
}

// Used by GET /api/factories/{id}/registration-request (and the Warehouse/Pharmacy equivalents),
// and by GET /api/registration-requests/by-entity/{entityType}/{entityId} (same shape, unified lookup -
// see EntitiesManagement-Backend-Gaps.md item 1).
public class EntityRegistrationRequestRefDto
{
    public int Id { get; set; }
    public string? RequestCode { get; set; }
    public string? RegistrationStatus { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

public class RejectRequestDto
{
    public string? RejectionReason { get; set; }
}

public class RequestMoreDocumentsDto
{
    public string? AdminNotes { get; set; }
    public List<int>? DocumentIdsNeedingReplacement { get; set; }
}

public class DocumentStatusUpdateDto
{
    public string? Status { get; set; } // Complete / NeedsReplacement / Rejected
    public string? RejectionReason { get; set; }
}

// ---------------- Entities Management ----------------
public class FactoryListItemDto
{
    public int Id { get; set; }
    public string? FactoryName { get; set; }
    public string? LegalCompanyName { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public bool? HasColdStorage { get; set; }
    public bool? HasQualityControlLab { get; set; }
    public string? FactoryStatus { get; set; }
    public int? TotalBatches { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class FactoryProfileDto
{
    public int Id { get; set; }
    public string? OfficialFactoryName { get; set; }
    public string? LegalCompanyName { get; set; }
    public string? DosageFormsProduced { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? FactoryLicenseNumber { get; set; }
    public string? TechnicalOperatingLicenseNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? TaxCardNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public bool? HasQualityControlLab { get; set; }
    public bool? HasFinishedGoodsStore { get; set; }
    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }
    public string? FactoryStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class WarehouseListItemDto
{
    public int Id { get; set; }
    public string? WarehouseName { get; set; }
    public string? WarehouseType { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }
    public bool? HasDeliveryService { get; set; }
    public string? WarehouseStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class WarehouseProfileDto
{
    public int Id { get; set; }
    public string? OfficialWarehouseName { get; set; }
    public string? WarehouseType { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? WarehouseLicenseNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }
    public bool? HasDeliveryService { get; set; }
    public string? WarehouseStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PharmacyListItemDto
{
    public int Id { get; set; }
    public string? PharmacyName { get; set; }
    public string? PharmacyType { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DefaultWarehouse { get; set; }
    public bool? HasColdStorage { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? PharmacyStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class PharmacyProfileDto
{
    public int Id { get; set; }
    public string? OfficialPharmacyName { get; set; }
    public string? PharmacyType { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? DefaultWarehouse { get; set; }
    public bool? HasColdStorage { get; set; }
    public string? PharmacyLicenseNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? PharmacistSyndicateId { get; set; }
    public string? PharmacyStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class EntityActionDto
{
    public string? Reason { get; set; }
}

// ---------------- Medicine & Batch Monitoring ----------------
public class BatchListItemDto
{
    public int Id { get; set; }
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public string? BatchNumber { get; set; }
    public string? FactoryName { get; set; }
    public long? Quantity { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? BatchStatus { get; set; }
    public string? SupplyChainStage { get; set; }
    public string? CurrentLocation { get; set; }
    public int? OpenAlerts { get; set; }
    public long? UnitCodesCount { get; set; }
    public bool? AvailableForDispatch { get; set; }
    // Added so dispatch validation (cold-chain check) doesn't need an extra GET .../{id} call
    // per row just to read one boolean (Backend_Action_Report_Factory_Portal, item 1.6).
    public bool? RequiresColdChain { get; set; }
    public DateTime? UpdatedAt { get; set; } // for the "Last Update" column
}

// PUT/PATCH .../batches/{batchId} - only allowed while BatchStatus == Draft (Backend_Action_Report_Factory_Portal, item 1.1)
public class UpdateBatchDto
{
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public bool? RequiresColdChain { get; set; }
    public string? BatchNumber { get; set; }
    public long? Quantity { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

// GET .../batches/{batchId}/unit-codes (Backend_Action_Report_Factory_Portal, item 1.2)
public class UnitCodeListItemDto
{
    public int Id { get; set; }
    public string? GTIN { get; set; }
    public string? SerialNumber { get; set; }
    public string? CodeValueHash { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? UnitStatus { get; set; }
}

// GET .../batches/summary - typed replacement for the previously untyped `object` response
// (MedicineBatchDashboard-Backend-Gaps.md, item 1). Field names match what was already being
// emitted, just now documented in the Swagger schema.
public class BatchesSummaryDto
{
    public int TotalBatches { get; set; }
    public int InProduction { get; set; }
    public int InSupplyChain { get; set; }
    public int InWarehouses { get; set; }
    public int InPharmacies { get; set; }
    public int Quarantined { get; set; }
    public int Recalled { get; set; }
    public int OpenAlerts { get; set; }
}

public class ProductInfoDto
{
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public bool? RequiresColdChain { get; set; }
    public string? ProductStatus { get; set; }
}

public class BatchInfoDto
{
    public string? BatchNumber { get; set; }
    public string? FactoryName { get; set; }
    public long? Quantity { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? BatchStatus { get; set; }
    public string? SupplyChainStage { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UnitCodesSummaryDto
{
    public long? TotalUnitCodes { get; set; }
    public long? GeneratedCount { get; set; }
    public long? InWarehouseCount { get; set; }
    public long? InPharmacyCount { get; set; }
    public long? SuspiciousCount { get; set; }
    public long? BlockedCount { get; set; }
    public long? RecalledCount { get; set; }
    public long? ScanCountTotal { get; set; }
}

public class ShipmentSummaryItemDto
{
    public string? TransferCode { get; set; }
    public string? ShipmentType { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public long? ExpectedQuantity { get; set; }
    public long? ReceivedQuantity { get; set; }
    public string? ShipmentStatus { get; set; }
    public DateTime? DispatchDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
}

public class InventoryDistributionItemDto
{
    public string? HolderType { get; set; }
    public string? HolderName { get; set; }
    public long? TotalReceivedQuantity { get; set; }
    public long? AvailableQuantity { get; set; }
    public long? ReservedQuantity { get; set; }
    public long? QuarantinedQuantity { get; set; }
    public string? InventoryStatus { get; set; }
    public DateTime? LastUpdated { get; set; }
}

public class RelatedAlertItemDto
{
    public string? AlertType { get; set; }
    public string? Severity { get; set; }
    public string? Message { get; set; }
    public string? AlertStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class BatchDetailsDto
{
    public int Id { get; set; }
    public ProductInfoDto? ProductInfo { get; set; }
    public BatchInfoDto? BatchInfo { get; set; }
    public UnitCodesSummaryDto? UnitCodesSummary { get; set; }
    public List<ShipmentSummaryItemDto>? Shipments { get; set; }
    public List<InventoryDistributionItemDto>? InventoryDistribution { get; set; }
    public List<RelatedAlertItemDto>? RelatedAlerts { get; set; }
}

public class CreateRecallAlertDto
{
    public string? Message { get; set; }
}

// ---------------- Alerts & Public Scans ----------------
public class AlertListItemDto
{
    public int Id { get; set; }
    public string? AlertCode { get; set; }
    public string? AlertType { get; set; }
    public string? Severity { get; set; }
    public string? EntityType { get; set; }
    public string? EntityName { get; set; }
    public string? ProductName { get; set; }
    public string? BatchNumber { get; set; }
    public string? ShipmentTransferCode { get; set; }
    public string? Message { get; set; }
    public string? AlertStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class AlertDetailsDto
{
    public int Id { get; set; }
    public string? AlertCode { get; set; }
    public string? AlertType { get; set; }
    public string? Severity { get; set; }
    public string? EntityType { get; set; }
    public string? EntityName { get; set; }
    public string? Message { get; set; }
    public string? ProductName { get; set; }
    public string? BatchNumber { get; set; }
    public int? BatchId { get; set; }
    public string? ShipmentTransferCode { get; set; }
    public int? ShipmentId { get; set; }
    public string? AlertStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    // "Impact on this Batch" panel - only meaningful for Recall / ComplianceIssue alerts
    public string? ImpactedBatchStatus { get; set; }
    public string? ImpactedUnitCodesStatus { get; set; }
    public string? ImpactedInventoryStatus { get; set; }
    public bool? BatchDispatchBlocked { get; set; }
}

public class UpdateAlertStatusDto
{
    public string? Status { get; set; } // UnderReview / Resolved / Dismissed
}

public class ScanListItemDto
{
    public int Id { get; set; }
    public string? ScanCode { get; set; }
    public string? ScannedGTIN { get; set; }
    public string? ScannedSerialNumber { get; set; }
    public string? ScannedBatchNumber { get; set; }
    public string? ProductName { get; set; }
    public string? VerificationResult { get; set; }
    public string? Reason { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public DateTime? ScannedAt { get; set; }
}

public class ScanDetailsDto
{
    public int Id { get; set; }
    public string? ScannedGTIN { get; set; }
    public string? ScannedSerialNumber { get; set; }
    public string? ScannedBatchNumber { get; set; }
    public string? VerificationResult { get; set; }
    public string? Reason { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public DateTime? ScannedAt { get; set; }

    public int? UnitCodeId { get; set; }
    public string? ProductName { get; set; }
    public string? BatchNumber { get; set; }
    public string? FactoryName { get; set; }
    public string? UnitStatus { get; set; }
    public int? ScanCount { get; set; }
    public DateTime? FirstScannedAt { get; set; }
}

public class CreateAlertFromScanDto
{
    public string? Message { get; set; }
    public string? Severity { get; set; }
}

// GET /api/alerts/counts - typed replacement for the previously untyped `object` response
// (Alerts_Backend_Gaps_Report.md, item 1).
public class AlertsCountsDto
{
    public int OpenAlerts { get; set; }
    public int PublicScanLogs { get; set; }
    public int RecallAlerts { get; set; }
}

// GET /api/factory-dashboard/{factoryId}/alerts/counts (Backend_Action_Report_Factory_Portal, item 1.5)
public class FactoryAlertsCountsDto
{
    public int Open { get; set; }
    public int UnderReview { get; set; }
    public int Resolved { get; set; }
    public int Dismissed { get; set; }
    public int Total { get; set; }
}

// POST /api/alerts/bulk-status (Alerts_Backend_Gaps_Report.md, item 7)
public class BulkUpdateAlertStatusDto
{
    public List<int>? AlertIds { get; set; }
    public string? Status { get; set; } // UnderReview / Resolved / Dismissed
}

// ---------------- Admin & Audit ----------------
public class SystemUserListItemDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string? Role { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // ---- Staff Management fields (staff-management-backend-gaps.md, items 3-4) ----
    public string? OfficialEmail { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Department { get; set; }
    public string? Facility { get; set; }
    // Tri-state: "Active" | "Inactive" | "Suspended", derived from IsActive + IsSuspended.
    public string? Status { get; set; }
    public string? NationalIdMasked { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Qualification { get; set; }
    public string? JobGrade { get; set; }
    public DateTime? HireDate { get; set; }
    public string? InsuranceNumber { get; set; }
}

// POST /api/admin/users/{id}/reset-password (staff-management-backend-gaps.md, item 1)
public class ResetPasswordDto
{
    public string? Password { get; set; }
    public bool? ForcePasswordReset { get; set; }
    public string? SendCredentialsTo { get; set; } // "personalEmail" (never the official/ministry inbox)
}

public class SystemUsersSummaryDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int ActiveSessions { get; set; }
}

public class AddMinistryAdminDto
{
    public string? FullName { get; set; }
    // Kept for backward compatibility - if OfficialEmail isn't sent, this is used as both
    // the login email and OfficialEmail.
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalId { get; set; }
    public string? Role { get; set; } // MinistryAdmin / MinistryViewer / SuperAdmin
    public string? TemporaryPassword { get; set; }
    public bool? SendResetLink { get; set; }

    // Optional. "Factory" | "Warehouse" | "Pharmacy" | null (or "Ministry") = full access.
    // Only meaningful when Role is MinistryAdmin or MinistryViewer - a SuperAdmin is always
    // unscoped regardless of what's sent here.
    public string? EntityScope { get; set; }

    // ---- Staff Management HR/identity fields (staff-management-backend-gaps.md, item 2) ----
    public string? PersonalEmail { get; set; }
    public string? OfficialEmail { get; set; }
    public string? Department { get; set; }
    public string? Facility { get; set; }
    public string? Status { get; set; } // active | inactive | suspended
    public DateTime? DateOfBirth { get; set; }
    public string? Qualification { get; set; }
    public string? JobGrade { get; set; }
    public DateTime? HireDate { get; set; }
    public string? InsuranceNumber { get; set; }
    public bool? ForcePasswordReset { get; set; }
    public string? SendCredentialsTo { get; set; } // "personalEmail"
}

public class AuditLogListItemDto
{
    public int Id { get; set; }
    public string? LogCode { get; set; }
    public string? User { get; set; }
    public string? Role { get; set; }
    public string? Action { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Result { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? CreatedAt { get; set; }
}

// ---------------- Common ----------------
public class PagedResult<T>
{
    public List<T>? Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

// ---------------- Shared: richer Shipment/Inventory items for operational dashboards ----------------
public class ShipmentListItemDto
{
    public int Id { get; set; }
    public string? TransferCode { get; set; }
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? BatchNumber { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public long? ExpectedQuantity { get; set; }
    public long? ReceivedQuantity { get; set; }
    public bool? RequiresColdChain { get; set; }
    public string? ShipmentStatus { get; set; }
    public DateTime? DispatchDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
}

public class ShipmentDetailsDto
{
    public int Id { get; set; }
    public string? TransferCode { get; set; }
    public string? ShipmentType { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public string? ProductName { get; set; }
    public string? BatchNumber { get; set; }
    public long? ExpectedQuantity { get; set; }
    public long? ReceivedQuantity { get; set; }
    public string? ShipmentStatus { get; set; }
    public bool? RequiresColdChain { get; set; }
    public DateTime? DispatchDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? Notes { get; set; }
    public string? InspectionResult { get; set; }
}

public class InventoryStockListItemDto
{
    public int Id { get; set; }
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public string? BatchNumber { get; set; }
    public string? FactoryName { get; set; }
    public long? TotalReceivedQuantity { get; set; }
    public long? AvailableQuantity { get; set; }
    public long? ReservedQuantity { get; set; }
    public long? QuarantinedQuantity { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool? RequiresColdChain { get; set; }
    public string? InventoryStatus { get; set; }
}

public class InventoryStockDetailsDto
{
    public int Id { get; set; }
    public ProductInfoDto? ProductInfo { get; set; }
    public BatchInfoDto? BatchInfo { get; set; }
    public InventoryDistributionItemDto? WarehouseInventory { get; set; }
    public List<ShipmentSummaryItemDto>? RelatedShipments { get; set; }
}

public class OperationalProfileDto
{
    public AccountInfoDto? Account { get; set; }
    public EntityInfoDto? Entity { get; set; }
    public List<DocumentItemDto>? Documents { get; set; }
}

public class ReportIssueDto
{
    public string? AlertType { get; set; } // ComplianceIssue / ColdChainIssue / QuantityMismatch / DamagedPackage
    public string? Message { get; set; }
    public int? BatchId { get; set; }
    public int? ShipmentId { get; set; }
}

// ---------------- Factory Dashboard ----------------
public class FactoryOverviewCardsDto
{
    public int TotalBatches { get; set; }
    public int ReadyForDispatch { get; set; }
    public long UnitCodesGenerated { get; set; }
    public int ShipmentsInTransit { get; set; }
    public int ReceivedByWarehouses { get; set; }
    public int OpenAlerts { get; set; }
}

public class FactoryOverviewDto
{
    public FactoryOverviewCardsDto? Cards { get; set; }
    public List<BatchListItemDto>? RecentBatches { get; set; }
    public List<ShipmentListItemDto>? RecentShipments { get; set; }
    public List<AlertListItemDto>? OpenAlerts { get; set; }
}

public class CreateBatchDto
{
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public bool? RequiresColdChain { get; set; }
    public string? BatchNumber { get; set; }
    public long? Quantity { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public bool? SaveAsDraft { get; set; }
}

// GET /api/factory-dashboard/{factoryId}/warehouses (Backend_Action_Report_Factory_Portal, item 1.4)
// Deliberately thin - just enough for a dispatch-destination dropdown (name + cold-storage flag).
public class WarehouseOptionDto
{
    public int Id { get; set; }
    public string? WarehouseName { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public bool? HasColdStorage { get; set; }
    public string? WarehouseStatus { get; set; }
}

public class CreateDispatchDto
{
    public int? BatchId { get; set; }
    public long? DispatchQuantity { get; set; }
    public int? DestinationWarehouseId { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? Notes { get; set; }
}

// ---------------- Warehouse Dashboard ----------------
public class WarehouseOverviewCardsDto
{
    public int IncomingShipments { get; set; }
    public int PendingInspection { get; set; }
    public int ReadyStockSkus { get; set; }
    public long TotalStockUnits { get; set; }
    public int OutgoingShipments { get; set; }
    public int OpenAlerts { get; set; }
}

public class WarehouseInventorySummaryCardsDto
{
    public int TotalProductsSkus { get; set; }
    public long TotalReceivedUnits { get; set; }
    public long AvailableUnits { get; set; }
    public long ReservedUnits { get; set; }
    public long QuarantinedUnits { get; set; }
    public long ExpiringSoonUnits { get; set; } // within 90 days
}

public class FactoryShipmentsSummaryDto
{
    public int TotalShipments { get; set; }
    public int InTransit { get; set; }
    public int Received { get; set; }
    public int PartiallyReceived { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
}

public class ProductStockSummaryItemDto
{
    public string? ProductName { get; set; }
    public string? Strength { get; set; }
    public string? DosageForm { get; set; }
    public long? TotalStock { get; set; }
    public long? AvailableStock { get; set; }
    public long? InTransit { get; set; }
    public long? ExpiringSoon { get; set; }
}

public class WarehouseOverviewDto
{
    public WarehouseOverviewCardsDto? Cards { get; set; }
    public List<ShipmentListItemDto>? RecentIncomingShipments { get; set; }
    public List<ShipmentListItemDto>? RecentOutgoingShipments { get; set; }
    public List<ProductStockSummaryItemDto>? TopProducts { get; set; }
    public List<AlertListItemDto>? RecentAlerts { get; set; }
}

public class ReceiveShipmentDto
{
    public long? ReceivedQuantity { get; set; }
    public string? InspectionResult { get; set; } // Accepted / PartiallyAccepted / Rejected
    public string? Notes { get; set; }
}

public class DispatchToPharmacyDto
{
    public long? DispatchQuantity { get; set; }
    public int? TargetPharmacyId { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? Notes { get; set; }
}

public class MoveToQuarantineDto
{
    public long? QuarantineQuantity { get; set; }
    public string? Reason { get; set; }
}

// ---------------- Pharmacy Dashboard ----------------
public class PharmacyOverviewCardsDto
{
    public int IncomingShipments { get; set; }
    public int PendingReceiving { get; set; }
    public long CurrentStock { get; set; }
    public long? ColdChainStock { get; set; }
    public int OpenAlerts { get; set; }
    public long RecalledStock { get; set; }
}

public class PharmacyOverviewDto
{
    public PharmacyOverviewCardsDto? Cards { get; set; }
    public List<ShipmentListItemDto>? RecentIncomingShipments { get; set; }
    public List<InventoryStockListItemDto>? CurrentStockSummary { get; set; }
    public List<AlertListItemDto>? RecentAlerts { get; set; }
}

```

# FILE: src\EgyMediChain.Api\Dtos\SubmitRegistrationRequestDto.cs
```
namespace EgyMediChain.Api.Dtos;

// New file - put it next to the other Dto classes (same folder/namespace as
// RegistrationRequestListItemDto etc.). Used by the public registration wizard submission.
public class SubmitRegistrationRequestDto
{
    // Account (Step 1)
    public string? RepresentativeName { get; set; }
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalId { get; set; }
    public string? Password { get; set; }

    // "Factory" | "Warehouse" | "Pharmacy"
    public string? EntityType { get; set; }

    // Address (common)
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }

    // Factory
    public string? OfficialFactoryName { get; set; }
    public string? LegalCompanyName { get; set; }
    public string? DosageFormsProduced { get; set; }
    public string? FactoryLicenseNumber { get; set; }
    public string? TechnicalOperatingLicenseNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? TaxCardNumber { get; set; }
    public bool? HasQualityControlLab { get; set; }
    public bool? HasFinishedGoodsStore { get; set; }

    // Warehouse
    public string? OfficialWarehouseName { get; set; }
    public string? WarehouseType { get; set; }
    public string? WarehouseLicenseNumber { get; set; }
    public bool? HasDeliveryService { get; set; }

    // Pharmacy
    public string? OfficialPharmacyName { get; set; }
    public string? PharmacyType { get; set; }
    public int? DefaultWarehouseId { get; set; }
    public string? PharmacyLicenseNumber { get; set; }
    public string? PharmacistSyndicateId { get; set; }

    // Shared licensing / capabilities
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }

    // Declaration checkboxes (Step 5)
    public bool ConfirmInfoCorrect { get; set; }
    public bool AgreeToInspection { get; set; }
}
```

# FILE: src\EgyMediChain.Api\Properties\launchSettings.json
```
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

# FILE: src\EgyMediChain.Domain\EgyMediChain.Domain.csproj
```
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>EgyMediChain.Domain</RootNamespace>
  </PropertyGroup>

</Project>
```

# FILE: src\EgyMediChain.Domain\Entities\Entities.cs
```
using EgyMediChain.Domain.Enums;

namespace EgyMediChain.Domain.Entities;

public class SystemUser
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    // Login email - kept as-is for backward compatibility (AuthController.Login queries this).
    // Conceptually equals OfficialEmail once that's set; falls back to whichever was provided first.
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalId { get; set; }
    public SystemRole? Role { get; set; }
    public EntityKind? EntityType { get; set; }
    public int? EntityId { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? IsActive { get; set; }
    // Added for Staff Management's tri-state status model (active/inactive/suspended).
    // IsActive stays the source of truth for login gating; IsSuspended is an additional flag
    // so "Suspended" can be told apart from a plain "Inactive" deactivation.
    public bool? IsSuspended { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // ---- Staff Management HR/identity fields (see staff-management-backend-gaps.md) ----
    public string? PersonalEmail { get; set; }
    public string? OfficialEmail { get; set; }
    public string? Department { get; set; }
    public string? Facility { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Qualification { get; set; }
    public string? JobGrade { get; set; }
    public DateTime? HireDate { get; set; }
    public string? InsuranceNumber { get; set; }

    // Soft delete (Backend Action Report ง5.2) - permanently destroying a staff record breaks
    // its link to historical audit trail / approvals they signed off on, which is a compliance
    // risk for a regulated Ministry system. DELETE now sets these instead of removing the row.
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }

    public ICollection<AuthRefreshToken>? RefreshTokens { get; set; }
}

public class AuthRefreshToken
{
    public int Id { get; set; }
    public int? SystemUserId { get; set; }
    public SystemUser? SystemUser { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? CreatedByIp { get; set; }
}

// Self-service Forgot Password flow (Backend Action Report ง2). OTP + reset token are both
// stored hashed, never in plaintext - see AuthController for the HMAC hashing.
public class PasswordResetRequest
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public SystemUser? User { get; set; }
    public string? RequestedEmail { get; set; }
    public string? HashedOtp { get; set; }
    public string? ResetTokenHash { get; set; }
    public int Attempts { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ResetTokenExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public string? RequestIp { get; set; }
}

// Async report generation job (Backend Action Report ง4). Kept intentionally simple - runs via
// Task.Run + IServiceScopeFactory rather than a full queue/worker (Hangfire etc.), since none is
// wired up in this project yet. Good enough for Ministry-scale data volumes; revisit with a real
// job queue if report generation ever needs to survive an app restart mid-job.
public class ReportJob
{
    public Guid Id { get; set; }
    public int? RequestedByUserId { get; set; }
    public string? ReportType { get; set; }
    public string? Format { get; set; }
    public string? ParametersJson { get; set; }
    public string Status { get; set; } = "Queued"; // Queued | Processing | Completed | Failed
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
public class UserPreference
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public SystemUser? User { get; set; }
    public bool EmailAlerts { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool CriticalAlerts { get; set; } = true;
    public bool WeeklyReports { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Single-row Ministry-wide policy config (Backend Action Report ง3.2). SuperAdmin-only.
public class SystemConfiguration
{
    public int Id { get; set; }
    public bool TwoFactorRequired { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 60;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
}

public class RegistrationRequest
{
    public int Id { get; set; }
    public string? RequestCode { get; set; } // REQ-0012
    public EntityKind? EntityType { get; set; }
    public string? EntityName { get; set; }
    public string? RepresentativeName { get; set; }
    public string? Email { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool? EmailConfirmed { get; set; }
    public DocumentStatus? DocumentsOverallStatus { get; set; }
    public RegistrationStatus? RegistrationStatus { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    // Added for the "Request Inspection" action - nullable so existing rows are unaffected.
    public DateTime? InspectionScheduledDate { get; set; }
    public string? InspectorNotes { get; set; }
    public DateTime? InspectionRequestedAt { get; set; }

    public int? FactoryId { get; set; }
    public Factory? Factory { get; set; }
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public int? PharmacyId { get; set; }
    public Pharmacy? Pharmacy { get; set; }

    public int? SystemUserId { get; set; }
    public SystemUser? SystemUser { get; set; }

    public ICollection<EntityDocument>? Documents { get; set; }
}

public class EntityDocument
{
    public int Id { get; set; }
    public int? RegistrationRequestId { get; set; }
    public RegistrationRequest? RegistrationRequest { get; set; }
    public string? DocumentType { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public DateTime? UploadedAt { get; set; }
    public DocumentStatus? DocumentStatus { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}

public class Factory
{
    public int Id { get; set; }
    public string? FactoryCode { get; set; } // FAC-2024-021 (display code, distinct from license number)
    public string? OfficialFactoryName { get; set; }
    public string? LegalCompanyName { get; set; }
    public string? DosageFormsProduced { get; set; }

    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public string? FactoryLicenseNumber { get; set; }
    public string? TechnicalOperatingLicenseNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? TaxCardNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }

    public bool? HasQualityControlLab { get; set; }
    public bool? HasFinishedGoodsStore { get; set; }
    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }

    // Factory Details tab
    public int? EstablishedYear { get; set; }
    public int? TotalProductionLines { get; set; }
    public string? MainProductionTypes { get; set; }
    public string? StorageTypes { get; set; }
    public string? QualityCertificates { get; set; }
    public string? Description { get; set; }

    // Registration Info tab (denormalized snapshot of the approval, so the factory portal
    // can show it without joining back through RegistrationRequest)
    public string? RegistrationRequestNo { get; set; }
    public DateTime? RegistrationSubmittedAt { get; set; }
    public DateTime? RegistrationApprovedAt { get; set; }
    public string? RegistrationApprovedBy { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public string? RegistrationNotes { get; set; }

    public FacilityStatus? FactoryStatus { get; set; }
    public int? TotalBatches { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Batch>? Batches { get; set; }
}

public class Warehouse
{
    public int Id { get; set; }
    public string? WarehouseCode { get; set; } // WH-CAI-001
    public string? OfficialWarehouseName { get; set; }
    public string? WarehouseType { get; set; } // Main / Regional

    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public string? WarehouseLicenseNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }

    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }
    public bool? HasDeliveryService { get; set; }

    public FacilityStatus? WarehouseStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Pharmacy
{
    public int Id { get; set; }
    public string? PharmacyCode { get; set; } // PH-ALX-001
    public string? OfficialPharmacyName { get; set; }
    public string? PharmacyType { get; set; } // Retail

    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public int? DefaultWarehouseId { get; set; }
    public Warehouse? DefaultWarehouse { get; set; }
    public bool? HasColdStorage { get; set; }

    public string? PharmacyLicenseNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? PharmacistSyndicateId { get; set; }

    public FacilityStatus? PharmacyStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Generic multi-license row used by the Factory (and, if needed later, Warehouse/Pharmacy)
// Profile > Licenses tab. Kept separate from the single License Number fields above because
// a facility can hold several license types at once (Manufacturing, GMP, Environmental, Fire Safety...).
public class EntityLicense
{
    public int Id { get; set; }
    public EntityKind? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? LicenseType { get; set; } // Manufacturing License / GMP Certificate / Environmental License / Fire Safety License ...
    public string? LicenseNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Status { get; set; } // Active / Expired / Suspended
    public string? FileUrl { get; set; }
}

public class MedicineProduct
{
    public int Id { get; set; }
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public bool? RequiresColdChain { get; set; }
    public string? ProductStatus { get; set; }

    public ICollection<Batch>? Batches { get; set; }
}

public class Batch
{
    public int Id { get; set; }
    public string? BatchNumber { get; set; } // BAT-2024-001

    public int? MedicineProductId { get; set; }
    public MedicineProduct? MedicineProduct { get; set; }

    public int? FactoryId { get; set; }
    public Factory? Factory { get; set; }

    public long? Quantity { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public BatchStatus? BatchStatus { get; set; }
    public SupplyChainStage? SupplyChainStage { get; set; }
    public string? CurrentLocation { get; set; }

    public string? CreatedBy { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Unit Codes Summary (denormalized so we don't need millions of UnitCode rows)
    public long? TotalUnitCodes { get; set; }
    public long? GeneratedUnitCodes { get; set; }
    public long? InWarehouseUnitCodes { get; set; }
    public long? InPharmacyUnitCodes { get; set; }
    public long? SuspiciousUnitCodes { get; set; }
    public long? BlockedUnitCodes { get; set; }
    public long? RecalledUnitCodes { get; set; }
    public long? ScanCountTotal { get; set; }

    public int? OpenAlertsCount { get; set; }

    public ICollection<UnitCode>? UnitCodes { get; set; }
    public ICollection<Shipment>? Shipments { get; set; }
    public ICollection<InventoryStock>? InventoryStocks { get; set; }
    public ICollection<Alert>? Alerts { get; set; }
}

// Sample of individual unit codes kept for scan/alert linking (not exhaustive - see Batch summary fields)
public class UnitCode
{
    public int Id { get; set; }
    public string? UnitCodeValue { get; set; }
    public string? SerialNumber { get; set; }
    public string? GTIN { get; set; }
    public string? CodeValueHash { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? BatchId { get; set; }
    public Batch? Batch { get; set; }
    public UnitStatus? UnitStatus { get; set; }
    public string? CurrentHolderType { get; set; } // Factory/Warehouse/Pharmacy
    public string? CurrentHolderName { get; set; }
    public int? ScanCount { get; set; }
    public DateTime? FirstScannedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class Shipment
{
    public int Id { get; set; }
    public string? TransferCode { get; set; } // TRF-2034-1101
    public int? BatchId { get; set; }
    public Batch? Batch { get; set; }
    public ShipmentType? ShipmentType { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }

    // Structured FKs (populated by the Factory/Warehouse operational dashboards;
    // may be null on older/seed-only rows that only have the display strings above)
    public int? SourceFactoryId { get; set; }
    public int? SourceWarehouseId { get; set; }
    public int? DestinationWarehouseId { get; set; }
    public int? DestinationPharmacyId { get; set; }

    public long? ExpectedQuantity { get; set; }
    public long? ReceivedQuantity { get; set; }
    public ShipmentStatus? ShipmentStatus { get; set; }
    public bool? RequiresColdChain { get; set; }
    public int? DispatchedByUserId { get; set; }
    public int? ReceivedByUserId { get; set; }
    public string? InspectionResult { get; set; } // Accepted / PartiallyAccepted / Rejected
    public string? Notes { get; set; }
    public DateTime? DispatchDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
}

public class InventoryStock
{
    public int Id { get; set; }
    public int? BatchId { get; set; }
    public Batch? Batch { get; set; }
    public string? HolderType { get; set; } // Warehouse/Pharmacy
    public string? HolderName { get; set; }
    public int? WarehouseId { get; set; }
    public int? PharmacyId { get; set; }
    public long? TotalReceivedQuantity { get; set; }
    public long? AvailableQuantity { get; set; }
    public long? ReservedQuantity { get; set; }
    public long? QuarantinedQuantity { get; set; }
    public InventoryStatus? InventoryStatus { get; set; }
    public DateTime? LastUpdated { get; set; }
}

public class Alert
{
    public int Id { get; set; }
    public string? AlertCode { get; set; } // ALERT-2024-0091
    public AlertType? AlertType { get; set; }
    public AlertSeverity? Severity { get; set; }
    public EntityKind? EntityType { get; set; }
    public string? EntityName { get; set; }
    public int? BatchId { get; set; }
    public Batch? Batch { get; set; }
    public int? ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }
    public string? Message { get; set; }
    public AlertStatus? AlertStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class PublicVerificationScan
{
    public int Id { get; set; }
    public string? ScanCode { get; set; } // SCAN-2024-15021
    public string? ScannedGTIN { get; set; }
    public string? ScannedSerialNumber { get; set; }
    public string? ScannedBatchNumber { get; set; }
    public int? UnitCodeId { get; set; }
    public UnitCode? UnitCode { get; set; }
    public string? ProductName { get; set; }
    public VerificationResult? VerificationResult { get; set; }
    public string? Reason { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public DateTime? ScannedAt { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }
    public string? LogCode { get; set; } // LOG-2024-55678
    public int? UserId { get; set; }
    public SystemUser? User { get; set; }
    public string? UserDisplayName { get; set; }
    public SystemRole? Role { get; set; }
    public AuditAction? Action { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public AuditResult? Result { get; set; }

    public string? IpAddress { get; set; }
    public DateTime? CreatedAt { get; set; }
    public Factory? SourceFactory { get; set; }

    public Warehouse? SourceWarehouse { get; set; }

    public Warehouse? DestinationWarehouse { get; set; }

    public Pharmacy? DestinationPharmacy { get; set; }
}

```

# FILE: src\EgyMediChain.Domain\Enums\Enums.cs
```
namespace EgyMediChain.Domain.Enums;

public enum SystemRole
{
    SuperAdmin,
    MinistryAdmin,
    MinistryViewer,
    FactoryUser,
    WarehouseUser,
    PharmacyUser
}

public enum EntityKind
{
    Factory,
    Warehouse,
    Pharmacy,
    Ministry
}

public enum RegistrationStatus
{
    Pending,
    UnderReview,
    NeedsMoreDocuments,
    Approved,
    Rejected,
    Cancelled
}

public enum DocumentStatus
{
    UnderReview,
    Complete,
    NeedsReplacement,
    Rejected
}

public enum FacilityStatus
{
    PendingReview,
    Active,
    Suspended,
    Inactive,
    Rejected
}

public enum BatchStatus
{
    // Ministry-facing (existing)
    InProduction,
    InSupplyChain,
    InWarehouse,
    InPharmacy,
    Quarantined,
    Recalled,
    Available,
    // Factory operational lifecycle
    Draft,
    Registered,
    CodesGenerated,
    ReadyForWarehouseDispatch,
    PartiallyDispatched,
    FullyDispatched,
    Expired,
    Cancelled
}

public enum SupplyChainStage
{
    AtFactory,
    InTransit,
    Stored,
    Available,
    Quarantined,
    Recalled
}

public enum UnitStatus
{
    Generated,
    InWarehouse,
    InPharmacy,
    Blocked,
    Recalled,
    Suspicious
}

public enum ShipmentType
{
    FactoryToWarehouse,
    WarehouseToPharmacy,
    WarehouseToWarehouse
}

public enum ShipmentStatus
{
    Pending,
    InTransit,
    PendingInspection,
    Delivered,
    Received,
    PartiallyReceived,
    Rejected,
    Cancelled
}

public enum InventoryStatus
{
    Active,
    Quarantined,
    Recalled,
    Blocked
}

public enum AlertType
{
    ColdChainIssue,
    QuantityMismatch,
    SuspiciousScan,
    LicenseExpiry,
    BlockedUnitScan,
    ComplianceIssue,
    Recall,
    DuplicateSerial,
    DamagedPackage,
    ExpiredBatch,
    DocumentMissing
}

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum AlertStatus
{
    Open,
    UnderReview,
    Resolved,
    Dismissed
}

public enum VerificationResult
{
    Authentic,
    NotFound,
    DuplicateScan,
    Recalled,
    Expired,
    Blocked,
    Suspicious
}
public enum AuditResult
{
    Success,
    Failed,
    Warning
}
public enum AuditAction
{
    ApproveRegistration,
    RejectRegistration,
    UpdateDocumentStatus,
    SuspendEntity,
    ReactivateEntity,
    SetInactiveEntity,
    FreezeBatch,
    CreateRecallAlert,
    RevokeUserSessions,
    CreateAdmin,
    ResolveAlert,
    DismissAlert,
    CreateAlert,
    ApproveDocument,
    RejectDocument,
    // Factory operational
    CreateBatch,
    GenerateCodes,
    MarkBatchReadyForDispatch,
    CancelDraftBatch,
    DispatchShipment,
    // Warehouse / Pharmacy operational
    ReceiveShipment,
    RejectShipment,
    QuarantineStock,
    // Deletions (added for cleanup / delete endpoints)
    DeleteRegistrationRequest,
    DeleteUser,
    DeleteDraftBatch,
    DeleteAlert,
    // Added for the Staff Management gap fixes
    ResetUserPassword,
    SuspendUser,
    // Added for Edit Staff + Forgot Password (Backend Action Report)
    StaffMemberUpdated,
    ForgotPasswordRequested,
    PasswordResetVerifyAttempted,
    PasswordResetCompleted,
    SystemConfigUpdated,
    LoginFailed,
    LoginLockedOut
}

```

# FILE: src\EgyMediChain.Infrastructure\EgyMediChain.Infrastructure.csproj
```
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>EgyMediChain.Infrastructure</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.10">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.10">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\EgyMediChain.Domain\EgyMediChain.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Folder Include="Persistence\Migrations\" />
  </ItemGroup>

</Project>
```

# FILE: src\EgyMediChain.Infrastructure\Migrations\20260710093048_InitialCreate.cs
```
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgyMediChain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntityLicenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<int>(type: "int", nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    LicenseType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityLicenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Factories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FactoryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficialFactoryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegalCompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DosageFormsProduced = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Governorate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistrictArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactoryLicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechnicalOperatingLicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommercialRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxCardNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicenseIssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasQualityControlLab = table.Column<bool>(type: "bit", nullable: true),
                    HasFinishedGoodsStore = table.Column<bool>(type: "bit", nullable: true),
                    HasColdStorage = table.Column<bool>(type: "bit", nullable: true),
                    HasQuarantineArea = table.Column<bool>(type: "bit", nullable: true),
                    EstablishedYear = table.Column<int>(type: "int", nullable: true),
                    TotalProductionLines = table.Column<int>(type: "int", nullable: true),
                    MainProductionTypes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StorageTypes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QualityCertificates = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationRequestNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationSubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistrationApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistrationApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistrationNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactoryStatus = table.Column<int>(type: "int", nullable: true),
                    TotalBatches = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicineProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GTIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DosageForm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Strength = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiresColdChain = table.Column<bool>(type: "bit", nullable: true),
                    ProductStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: true),
                    EntityType = table.Column<int>(type: "int", nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficialWarehouseName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarehouseType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Governorate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistrictArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarehouseLicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicenseIssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasColdStorage = table.Column<bool>(type: "bit", nullable: true),
                    HasQuarantineArea = table.Column<bool>(type: "bit", nullable: true),
                    HasDeliveryService = table.Column<bool>(type: "bit", nullable: true),
                    WarehouseStatus = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicineProductId = table.Column<int>(type: "int", nullable: true),
                    FactoryId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<long>(type: "bigint", nullable: true),
                    ManufacturingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BatchStatus = table.Column<int>(type: "int", nullable: true),
                    SupplyChainStage = table.Column<int>(type: "int", nullable: true),
                    CurrentLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalUnitCodes = table.Column<long>(type: "bigint", nullable: true),
                    GeneratedUnitCodes = table.Column<long>(type: "bigint", nullable: true),
                    InWarehouseUnitCodes = table.Column<long>(type: "bigint", nullable: true),
                    InPharmacyUnitCodes = table.Column<long>(type: "bigint", nullable: true),
                    SuspiciousUnitCodes = table.Column<long>(type: "bigint", nullable: true),
                    BlockedUnitCodes = table.Column<long>(type: "bigint", nullable: true),
                    RecalledUnitCodes = table.Column<long>(type: "bigint", nullable: true),
                    ScanCountTotal = table.Column<long>(type: "bigint", nullable: true),
                    OpenAlertsCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batches_Factories_FactoryId",
                        column: x => x.FactoryId,
                        principalTable: "Factories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Batches_MedicineProducts_MedicineProductId",
                        column: x => x.MedicineProductId,
                        principalTable: "MedicineProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AuthRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemUserId = table.Column<int>(type: "int", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthRefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthRefreshTokens_SystemUsers_SystemUserId",
                        column: x => x.SystemUserId,
                        principalTable: "SystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pharmacies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PharmacyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficialPharmacyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PharmacyType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Governorate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistrictArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultWarehouseId = table.Column<int>(type: "int", nullable: true),
                    HasColdStorage = table.Column<bool>(type: "bit", nullable: true),
                    PharmacyLicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicenseIssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PharmacistSyndicateId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PharmacyStatus = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pharmacies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pharmacies_Warehouses_DefaultWarehouseId",
                        column: x => x.DefaultWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UnitCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitCodeValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GTIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeValueHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    UnitStatus = table.Column<int>(type: "int", nullable: true),
                    CurrentHolderType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentHolderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScanCount = table.Column<int>(type: "int", nullable: true),
                    FirstScannedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitCodes_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    UserDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<int>(type: "int", nullable: true),
                    ResourceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceFactoryId = table.Column<int>(type: "int", nullable: true),
                    SourceWarehouseId = table.Column<int>(type: "int", nullable: true),
                    DestinationWarehouseId = table.Column<int>(type: "int", nullable: true),
                    DestinationPharmacyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Factories_SourceFactoryId",
                        column: x => x.SourceFactoryId,
                        principalTable: "Factories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLogs_Pharmacies_DestinationPharmacyId",
                        column: x => x.DestinationPharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLogs_SystemUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Warehouses_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLogs_Warehouses_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InventoryStocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    HolderType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HolderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    PharmacyId = table.Column<int>(type: "int", nullable: true),
                    TotalReceivedQuantity = table.Column<long>(type: "bigint", nullable: true),
                    AvailableQuantity = table.Column<long>(type: "bigint", nullable: true),
                    ReservedQuantity = table.Column<long>(type: "bigint", nullable: true),
                    QuarantinedQuantity = table.Column<long>(type: "bigint", nullable: true),
                    InventoryStatus = table.Column<int>(type: "int", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryStocks_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryStocks_Pharmacies_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryStocks_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityType = table.Column<int>(type: "int", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepresentativeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: true),
                    DocumentsOverallStatus = table.Column<int>(type: "int", nullable: true),
                    RegistrationStatus = table.Column<int>(type: "int", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactoryId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    PharmacyId = table.Column<int>(type: "int", nullable: true),
                    SystemUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationRequests_Factories_FactoryId",
                        column: x => x.FactoryId,
                        principalTable: "Factories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RegistrationRequests_Pharmacies_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RegistrationRequests_SystemUsers_SystemUserId",
                        column: x => x.SystemUserId,
                        principalTable: "SystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RegistrationRequests_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    ShipmentType = table.Column<int>(type: "int", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceFactoryId = table.Column<int>(type: "int", nullable: true),
                    SourceWarehouseId = table.Column<int>(type: "int", nullable: true),
                    DestinationWarehouseId = table.Column<int>(type: "int", nullable: true),
                    DestinationPharmacyId = table.Column<int>(type: "int", nullable: true),
                    ExpectedQuantity = table.Column<long>(type: "bigint", nullable: true),
                    ReceivedQuantity = table.Column<long>(type: "bigint", nullable: true),
                    ShipmentStatus = table.Column<int>(type: "int", nullable: true),
                    RequiresColdChain = table.Column<bool>(type: "bit", nullable: true),
                    DispatchedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReceivedByUserId = table.Column<int>(type: "int", nullable: true),
                    InspectionResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DispatchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Shipments_Factories_SourceFactoryId",
                        column: x => x.SourceFactoryId,
                        principalTable: "Factories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Shipments_Pharmacies_DestinationPharmacyId",
                        column: x => x.DestinationPharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Shipments_Warehouses_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Shipments_Warehouses_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PublicVerificationScans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScannedGTIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScannedSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScannedBatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitCodeId = table.Column<int>(type: "int", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerificationResult = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Governorate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicVerificationScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicVerificationScans_UnitCodes_UnitCodeId",
                        column: x => x.UnitCodeId,
                        principalTable: "UnitCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EntityDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationRequestId = table.Column<int>(type: "int", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocumentStatus = table.Column<int>(type: "int", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityDocuments_RegistrationRequests_RegistrationRequestId",
                        column: x => x.RegistrationRequestId,
                        principalTable: "RegistrationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlertType = table.Column<int>(type: "int", nullable: true),
                    Severity = table.Column<int>(type: "int", nullable: true),
                    EntityType = table.Column<int>(type: "int", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    ShipmentId = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlertStatus = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Alerts_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_BatchId",
                table: "Alerts",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ShipmentId",
                table: "Alerts",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_DestinationPharmacyId",
                table: "AuditLogs",
                column: "DestinationPharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_DestinationWarehouseId",
                table: "AuditLogs",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SourceFactoryId",
                table: "AuditLogs",
                column: "SourceFactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SourceWarehouseId",
                table: "AuditLogs",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthRefreshTokens_SystemUserId",
                table: "AuthRefreshTokens",
                column: "SystemUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_FactoryId",
                table: "Batches",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_MedicineProductId",
                table: "Batches",
                column: "MedicineProductId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityDocuments_RegistrationRequestId",
                table: "EntityDocuments",
                column: "RegistrationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_BatchId",
                table: "InventoryStocks",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_PharmacyId",
                table: "InventoryStocks",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_WarehouseId",
                table: "InventoryStocks",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Pharmacies_DefaultWarehouseId",
                table: "Pharmacies",
                column: "DefaultWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicVerificationScans_UnitCodeId",
                table: "PublicVerificationScans",
                column: "UnitCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationRequests_FactoryId",
                table: "RegistrationRequests",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationRequests_PharmacyId",
                table: "RegistrationRequests",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationRequests_SystemUserId",
                table: "RegistrationRequests",
                column: "SystemUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationRequests_WarehouseId",
                table: "RegistrationRequests",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_BatchId",
                table: "Shipments",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_DestinationPharmacyId",
                table: "Shipments",
                column: "DestinationPharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_DestinationWarehouseId",
                table: "Shipments",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_SourceFactoryId",
                table: "Shipments",
                column: "SourceFactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_SourceWarehouseId",
                table: "Shipments",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitCodes_BatchId",
                table: "UnitCodes",
                column: "BatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "AuthRefreshTokens");

            migrationBuilder.DropTable(
                name: "EntityDocuments");

            migrationBuilder.DropTable(
                name: "EntityLicenses");

            migrationBuilder.DropTable(
                name: "InventoryStocks");

            migrationBuilder.DropTable(
                name: "PublicVerificationScans");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "RegistrationRequests");

            migrationBuilder.DropTable(
                name: "UnitCodes");

            migrationBuilder.DropTable(
                name: "Pharmacies");

            migrationBuilder.DropTable(
                name: "SystemUsers");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Factories");

            migrationBuilder.DropTable(
                name: "MedicineProducts");
        }
    }
}
```

# FILE: src\EgyMediChain.Infrastructure\Migrations\20260710093048_InitialCreate.Designer.cs
```
// <auto-generated />
using System;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EgyMediChain.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260710093048_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Alert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AlertCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("AlertStatus")
                        .HasColumnType("int");

                    b.Property<int?>("AlertType")
                        .HasColumnType("int");

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("EntityName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ResolvedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Severity")
                        .HasColumnType("int");

                    b.Property<int?>("ShipmentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("ShipmentId");

                    b.ToTable("Alerts");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("Action")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DestinationPharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("DestinationWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("IpAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LogCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NewValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OldValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Role")
                        .HasColumnType("int");

                    b.Property<int?>("SourceFactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("SourceWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("UserDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DestinationPharmacyId");

                    b.HasIndex("DestinationWarehouseId");

                    b.HasIndex("SourceFactoryId");

                    b.HasIndex("SourceWarehouseId");

                    b.HasIndex("UserId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuthRefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedByIp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("RevokedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SystemUserId")
                        .HasColumnType("int");

                    b.Property<string>("Token")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SystemUserId");

                    b.ToTable("AuthRefreshTokens");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("BatchStatus")
                        .HasColumnType("int");

                    b.Property<long?>("BlockedUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CreatedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("CurrentLocation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("int");

                    b.Property<long?>("GeneratedUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("InPharmacyUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("InWarehouseUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ManufacturingDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("MedicineProductId")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("OpenAlertsCount")
                        .HasColumnType("int");

                    b.Property<long?>("Quantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("RecalledUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("ScanCountTotal")
                        .HasColumnType("bigint");

                    b.Property<int?>("SupplyChainStage")
                        .HasColumnType("int");

                    b.Property<long?>("SuspiciousUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("TotalUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("MedicineProductId");

                    b.ToTable("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityDocument", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("DocumentStatus")
                        .HasColumnType("int");

                    b.Property<string>("DocumentType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RegistrationRequestId")
                        .HasColumnType("int");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ReviewedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ReviewedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UploadedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("RegistrationRequestId");

                    b.ToTable("EntityDocuments");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityLicense", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FileUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("IssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("LicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LicenseType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EntityLicenses");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Factory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CommercialRegistrationNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DosageFormsProduced")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EstablishedYear")
                        .HasColumnType("int");

                    b.Property<string>("FactoryCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FactoryLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("FactoryStatus")
                        .HasColumnType("int");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasFinishedGoodsStore")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQualityControlLab")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQuarantineArea")
                        .HasColumnType("bit");

                    b.Property<string>("LegalCompanyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("MainProductionTypes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OfficialFactoryName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("QualityCertificates")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationApprovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegistrationApprovedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegistrationNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RegistrationRequestNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationSubmittedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("StorageTypes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TaxCardNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TechnicalOperatingLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TotalBatches")
                        .HasColumnType("int");

                    b.Property<int?>("TotalProductionLines")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Factories");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.InventoryStock", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long?>("AvailableQuantity")
                        .HasColumnType("bigint");

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("HolderName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HolderType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("InventoryStatus")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int?>("PharmacyId")
                        .HasColumnType("int");

                    b.Property<long?>("QuarantinedQuantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("ReservedQuantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("TotalReceivedQuantity")
                        .HasColumnType("bigint");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("PharmacyId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("InventoryStocks");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.MedicineProduct", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DosageForm")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("RequiresColdChain")
                        .HasColumnType("bit");

                    b.Property<string>("Strength")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("MedicineProducts");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Pharmacy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DefaultWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OfficialPharmacyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacistSyndicateId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacyCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacyLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PharmacyStatus")
                        .HasColumnType("int");

                    b.Property<string>("PharmacyType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("DefaultWarehouseId");

                    b.ToTable("Pharmacies");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PublicVerificationScan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScanCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ScannedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ScannedBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScannedGTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScannedSerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UnitCodeId")
                        .HasColumnType("int");

                    b.Property<int?>("VerificationResult")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UnitCodeId");

                    b.ToTable("PublicVerificationScans");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AdminNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DocumentsOverallStatus")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("EntityName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("PharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("RegistrationStatus")
                        .HasColumnType("int");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RepresentativeName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SubmittedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SystemUserId")
                        .HasColumnType("int");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("PharmacyId");

                    b.HasIndex("SystemUserId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("RegistrationRequests");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Shipment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("Destination")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DestinationPharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("DestinationWarehouseId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DispatchDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DispatchedByUserId")
                        .HasColumnType("int");

                    b.Property<long?>("ExpectedQuantity")
                        .HasColumnType("bigint");

                    b.Property<string>("InspectionResult")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ReceivedByUserId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ReceivedDate")
                        .HasColumnType("datetime2");

                    b.Property<long?>("ReceivedQuantity")
                        .HasColumnType("bigint");

                    b.Property<bool?>("RequiresColdChain")
                        .HasColumnType("bit");

                    b.Property<int?>("ShipmentStatus")
                        .HasColumnType("int");

                    b.Property<int?>("ShipmentType")
                        .HasColumnType("int");

                    b.Property<string>("Source")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SourceFactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("SourceWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("TransferCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("DestinationPharmacyId");

                    b.HasIndex("DestinationWarehouseId");

                    b.HasIndex("SourceFactoryId");

                    b.HasIndex("SourceWarehouseId");

                    b.ToTable("Shipments");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<string>("FullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("MobileNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NationalId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Role")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SystemUsers");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UnitCode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("CodeValueHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CurrentHolderName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CurrentHolderType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("FirstScannedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("GTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ScanCount")
                        .HasColumnType("int");

                    b.Property<string>("SerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UnitCodeValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UnitStatus")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.ToTable("UnitCodes");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Warehouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasDeliveryService")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQuarantineArea")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OfficialWarehouseName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("WarehouseCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WarehouseLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("WarehouseStatus")
                        .HasColumnType("int");

                    b.Property<string>("WarehouseType")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Warehouses");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Alert", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("Alerts")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Shipment", "Shipment")
                        .WithMany()
                        .HasForeignKey("ShipmentId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");

                    b.Navigation("Shipment");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuditLog", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", "DestinationPharmacy")
                        .WithMany()
                        .HasForeignKey("DestinationPharmacyId");

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "DestinationWarehouse")
                        .WithMany()
                        .HasForeignKey("DestinationWarehouseId");

                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "SourceFactory")
                        .WithMany()
                        .HasForeignKey("SourceFactoryId");

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "SourceWarehouse")
                        .WithMany()
                        .HasForeignKey("SourceWarehouseId");

                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("DestinationPharmacy");

                    b.Navigation("DestinationWarehouse");

                    b.Navigation("SourceFactory");

                    b.Navigation("SourceWarehouse");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuthRefreshToken", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "SystemUser")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("SystemUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("SystemUser");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "Factory")
                        .WithMany("Batches")
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.MedicineProduct", "MedicineProduct")
                        .WithMany("Batches")
                        .HasForeignKey("MedicineProductId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("MedicineProduct");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityDocument", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.RegistrationRequest", "RegistrationRequest")
                        .WithMany("Documents")
                        .HasForeignKey("RegistrationRequestId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("RegistrationRequest");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.InventoryStock", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("InventoryStocks")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", null)
                        .WithMany()
                        .HasForeignKey("PharmacyId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Pharmacy", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "DefaultWarehouse")
                        .WithMany()
                        .HasForeignKey("DefaultWarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("DefaultWarehouse");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PublicVerificationScan", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.UnitCode", "UnitCode")
                        .WithMany()
                        .HasForeignKey("UnitCodeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("UnitCode");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "Factory")
                        .WithMany()
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", "Pharmacy")
                        .WithMany()
                        .HasForeignKey("PharmacyId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "SystemUser")
                        .WithMany()
                        .HasForeignKey("SystemUserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "Warehouse")
                        .WithMany()
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("Pharmacy");

                    b.Navigation("SystemUser");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Shipment", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("Shipments")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", null)
                        .WithMany()
                        .HasForeignKey("DestinationPharmacyId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("DestinationWarehouseId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Factory", null)
                        .WithMany()
                        .HasForeignKey("SourceFactoryId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("SourceWarehouseId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UnitCode", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("UnitCodes")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.Navigation("Alerts");

                    b.Navigation("InventoryStocks");

                    b.Navigation("Shipments");

                    b.Navigation("UnitCodes");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Factory", b =>
                {
                    b.Navigation("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.MedicineProduct", b =>
                {
                    b.Navigation("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemUser", b =>
                {
                    b.Navigation("RefreshTokens");
                });
#pragma warning restore 612, 618
        }
    }
}
```

# FILE: src\EgyMediChain.Infrastructure\Migrations\20260711021839_AddAuditResultColumn.cs
```
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgyMediChain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditResultColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Result",
                table: "AuditLogs",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Result",
                table: "AuditLogs");
        }
    }
}
```

# FILE: src\EgyMediChain.Infrastructure\Migrations\20260711021839_AddAuditResultColumn.Designer.cs
```
// <auto-generated />
using System;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EgyMediChain.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260711021839_AddAuditResultColumn")]
    partial class AddAuditResultColumn
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Alert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AlertCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("AlertStatus")
                        .HasColumnType("int");

                    b.Property<int?>("AlertType")
                        .HasColumnType("int");

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("EntityName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ResolvedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Severity")
                        .HasColumnType("int");

                    b.Property<int?>("ShipmentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("ShipmentId");

                    b.ToTable("Alerts");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("Action")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DestinationPharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("DestinationWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("IpAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LogCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NewValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OldValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Result")
                        .HasColumnType("int");

                    b.Property<int?>("Role")
                        .HasColumnType("int");

                    b.Property<int?>("SourceFactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("SourceWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("UserDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DestinationPharmacyId");

                    b.HasIndex("DestinationWarehouseId");

                    b.HasIndex("SourceFactoryId");

                    b.HasIndex("SourceWarehouseId");

                    b.HasIndex("UserId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuthRefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedByIp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("RevokedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SystemUserId")
                        .HasColumnType("int");

                    b.Property<string>("Token")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SystemUserId");

                    b.ToTable("AuthRefreshTokens");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("BatchStatus")
                        .HasColumnType("int");

                    b.Property<long?>("BlockedUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CreatedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("CurrentLocation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("int");

                    b.Property<long?>("GeneratedUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("InPharmacyUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("InWarehouseUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ManufacturingDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("MedicineProductId")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("OpenAlertsCount")
                        .HasColumnType("int");

                    b.Property<long?>("Quantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("RecalledUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("ScanCountTotal")
                        .HasColumnType("bigint");

                    b.Property<int?>("SupplyChainStage")
                        .HasColumnType("int");

                    b.Property<long?>("SuspiciousUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("TotalUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("MedicineProductId");

                    b.ToTable("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityDocument", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("DocumentStatus")
                        .HasColumnType("int");

                    b.Property<string>("DocumentType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RegistrationRequestId")
                        .HasColumnType("int");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ReviewedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ReviewedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UploadedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("RegistrationRequestId");

                    b.ToTable("EntityDocuments");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityLicense", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FileUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("IssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("LicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LicenseType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EntityLicenses");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Factory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CommercialRegistrationNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DosageFormsProduced")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EstablishedYear")
                        .HasColumnType("int");

                    b.Property<string>("FactoryCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FactoryLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("FactoryStatus")
                        .HasColumnType("int");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasFinishedGoodsStore")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQualityControlLab")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQuarantineArea")
                        .HasColumnType("bit");

                    b.Property<string>("LegalCompanyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("MainProductionTypes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OfficialFactoryName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("QualityCertificates")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationApprovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegistrationApprovedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegistrationNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RegistrationRequestNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationSubmittedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("StorageTypes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TaxCardNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TechnicalOperatingLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TotalBatches")
                        .HasColumnType("int");

                    b.Property<int?>("TotalProductionLines")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Factories");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.InventoryStock", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long?>("AvailableQuantity")
                        .HasColumnType("bigint");

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("HolderName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HolderType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("InventoryStatus")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int?>("PharmacyId")
                        .HasColumnType("int");

                    b.Property<long?>("QuarantinedQuantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("ReservedQuantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("TotalReceivedQuantity")
                        .HasColumnType("bigint");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("PharmacyId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("InventoryStocks");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.MedicineProduct", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DosageForm")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("RequiresColdChain")
                        .HasColumnType("bit");

                    b.Property<string>("Strength")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("MedicineProducts");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Pharmacy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DefaultWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OfficialPharmacyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacistSyndicateId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacyCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacyLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PharmacyStatus")
                        .HasColumnType("int");

                    b.Property<string>("PharmacyType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("DefaultWarehouseId");

                    b.ToTable("Pharmacies");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PublicVerificationScan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScanCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ScannedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ScannedBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScannedGTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScannedSerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UnitCodeId")
                        .HasColumnType("int");

                    b.Property<int?>("VerificationResult")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UnitCodeId");

                    b.ToTable("PublicVerificationScans");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AdminNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DocumentsOverallStatus")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("EntityName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("PharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("RegistrationStatus")
                        .HasColumnType("int");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RepresentativeName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SubmittedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SystemUserId")
                        .HasColumnType("int");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("PharmacyId");

                    b.HasIndex("SystemUserId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("RegistrationRequests");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Shipment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("Destination")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DestinationPharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("DestinationWarehouseId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DispatchDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DispatchedByUserId")
                        .HasColumnType("int");

                    b.Property<long?>("ExpectedQuantity")
                        .HasColumnType("bigint");

                    b.Property<string>("InspectionResult")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ReceivedByUserId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ReceivedDate")
                        .HasColumnType("datetime2");

                    b.Property<long?>("ReceivedQuantity")
                        .HasColumnType("bigint");

                    b.Property<bool?>("RequiresColdChain")
                        .HasColumnType("bit");

                    b.Property<int?>("ShipmentStatus")
                        .HasColumnType("int");

                    b.Property<int?>("ShipmentType")
                        .HasColumnType("int");

                    b.Property<string>("Source")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SourceFactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("SourceWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("TransferCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("DestinationPharmacyId");

                    b.HasIndex("DestinationWarehouseId");

                    b.HasIndex("SourceFactoryId");

                    b.HasIndex("SourceWarehouseId");

                    b.ToTable("Shipments");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<string>("FullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("MobileNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NationalId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Role")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SystemUsers");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UnitCode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("CodeValueHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CurrentHolderName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CurrentHolderType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("FirstScannedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("GTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ScanCount")
                        .HasColumnType("int");

                    b.Property<string>("SerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UnitCodeValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UnitStatus")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.ToTable("UnitCodes");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Warehouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasDeliveryService")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQuarantineArea")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OfficialWarehouseName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("WarehouseCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WarehouseLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("WarehouseStatus")
                        .HasColumnType("int");

                    b.Property<string>("WarehouseType")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Warehouses");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Alert", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("Alerts")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Shipment", "Shipment")
                        .WithMany()
                        .HasForeignKey("ShipmentId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");

                    b.Navigation("Shipment");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuditLog", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", "DestinationPharmacy")
                        .WithMany()
                        .HasForeignKey("DestinationPharmacyId");

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "DestinationWarehouse")
                        .WithMany()
                        .HasForeignKey("DestinationWarehouseId");

                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "SourceFactory")
                        .WithMany()
                        .HasForeignKey("SourceFactoryId");

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "SourceWarehouse")
                        .WithMany()
                        .HasForeignKey("SourceWarehouseId");

                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("DestinationPharmacy");

                    b.Navigation("DestinationWarehouse");

                    b.Navigation("SourceFactory");

                    b.Navigation("SourceWarehouse");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuthRefreshToken", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "SystemUser")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("SystemUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("SystemUser");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "Factory")
                        .WithMany("Batches")
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.MedicineProduct", "MedicineProduct")
                        .WithMany("Batches")
                        .HasForeignKey("MedicineProductId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("MedicineProduct");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityDocument", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.RegistrationRequest", "RegistrationRequest")
                        .WithMany("Documents")
                        .HasForeignKey("RegistrationRequestId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("RegistrationRequest");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.InventoryStock", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("InventoryStocks")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", null)
                        .WithMany()
                        .HasForeignKey("PharmacyId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Pharmacy", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "DefaultWarehouse")
                        .WithMany()
                        .HasForeignKey("DefaultWarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("DefaultWarehouse");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PublicVerificationScan", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.UnitCode", "UnitCode")
                        .WithMany()
                        .HasForeignKey("UnitCodeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("UnitCode");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "Factory")
                        .WithMany()
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", "Pharmacy")
                        .WithMany()
                        .HasForeignKey("PharmacyId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "SystemUser")
                        .WithMany()
                        .HasForeignKey("SystemUserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "Warehouse")
                        .WithMany()
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("Pharmacy");

                    b.Navigation("SystemUser");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Shipment", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("Shipments")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", null)
                        .WithMany()
                        .HasForeignKey("DestinationPharmacyId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("DestinationWarehouseId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Factory", null)
                        .WithMany()
                        .HasForeignKey("SourceFactoryId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("SourceWarehouseId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UnitCode", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("UnitCodes")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.Navigation("Alerts");

                    b.Navigation("InventoryStocks");

                    b.Navigation("Shipments");

                    b.Navigation("UnitCodes");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Factory", b =>
                {
                    b.Navigation("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.MedicineProduct", b =>
                {
                    b.Navigation("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemUser", b =>
                {
                    b.Navigation("RefreshTokens");
                });
#pragma warning restore 612, 618
        }
    }
}
```

# FILE: src\EgyMediChain.Infrastructure\Migrations\20260804145550_ApiGapsAndStaffHrFields.cs
```
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgyMediChain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ApiGapsAndStaffHrFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "SystemUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Facility",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "SystemUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceNumber",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                table: "SystemUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobGrade",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficialEmail",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalEmail",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Qualification",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InspectionRequestedAt",
                table: "RegistrationRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InspectionScheduledDate",
                table: "RegistrationRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InspectorNotes",
                table: "RegistrationRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "Facility",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "InsuranceNumber",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "IsSuspended",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "JobGrade",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "OfficialEmail",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "PersonalEmail",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "Qualification",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "InspectionRequestedAt",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "InspectionScheduledDate",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "InspectorNotes",
                table: "RegistrationRequests");
        }
    }
}
```

# FILE: src\EgyMediChain.Infrastructure\Migrations\20260804145550_ApiGapsAndStaffHrFields.Designer.cs
```
// <auto-generated />
using System;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EgyMediChain.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260804145550_ApiGapsAndStaffHrFields")]
    partial class ApiGapsAndStaffHrFields
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Alert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AlertCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("AlertStatus")
                        .HasColumnType("int");

                    b.Property<int?>("AlertType")
                        .HasColumnType("int");

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("EntityName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ResolvedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Severity")
                        .HasColumnType("int");

                    b.Property<int?>("ShipmentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("ShipmentId");

                    b.ToTable("Alerts");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("Action")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DestinationPharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("DestinationWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("IpAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LogCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NewValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OldValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Result")
                        .HasColumnType("int");

                    b.Property<int?>("Role")
                        .HasColumnType("int");

                    b.Property<int?>("SourceFactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("SourceWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("UserDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DestinationPharmacyId");

                    b.HasIndex("DestinationWarehouseId");

                    b.HasIndex("SourceFactoryId");

                    b.HasIndex("SourceWarehouseId");

                    b.HasIndex("UserId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuthRefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedByIp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("RevokedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SystemUserId")
                        .HasColumnType("int");

                    b.Property<string>("Token")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SystemUserId");

                    b.ToTable("AuthRefreshTokens");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("BatchStatus")
                        .HasColumnType("int");

                    b.Property<long?>("BlockedUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CreatedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("CurrentLocation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("int");

                    b.Property<long?>("GeneratedUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("InPharmacyUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("InWarehouseUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ManufacturingDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("MedicineProductId")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("OpenAlertsCount")
                        .HasColumnType("int");

                    b.Property<long?>("Quantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("RecalledUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("ScanCountTotal")
                        .HasColumnType("bigint");

                    b.Property<int?>("SupplyChainStage")
                        .HasColumnType("int");

                    b.Property<long?>("SuspiciousUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("TotalUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("MedicineProductId");

                    b.ToTable("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityDocument", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("DocumentStatus")
                        .HasColumnType("int");

                    b.Property<string>("DocumentType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RegistrationRequestId")
                        .HasColumnType("int");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ReviewedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ReviewedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UploadedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("RegistrationRequestId");

                    b.ToTable("EntityDocuments");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityLicense", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FileUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("IssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("LicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LicenseType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EntityLicenses");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Factory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CommercialRegistrationNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DosageFormsProduced")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EstablishedYear")
                        .HasColumnType("int");

                    b.Property<string>("FactoryCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FactoryLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("FactoryStatus")
                        .HasColumnType("int");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasFinishedGoodsStore")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQualityControlLab")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQuarantineArea")
                        .HasColumnType("bit");

                    b.Property<string>("LegalCompanyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("MainProductionTypes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OfficialFactoryName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("QualityCertificates")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationApprovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegistrationApprovedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegistrationNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RegistrationRequestNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationSubmittedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("StorageTypes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TaxCardNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TechnicalOperatingLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TotalBatches")
                        .HasColumnType("int");

                    b.Property<int?>("TotalProductionLines")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Factories");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.InventoryStock", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long?>("AvailableQuantity")
                        .HasColumnType("bigint");

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("HolderName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HolderType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("InventoryStatus")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int?>("PharmacyId")
                        .HasColumnType("int");

                    b.Property<long?>("QuarantinedQuantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("ReservedQuantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("TotalReceivedQuantity")
                        .HasColumnType("bigint");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("PharmacyId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("InventoryStocks");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.MedicineProduct", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DosageForm")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("RequiresColdChain")
                        .HasColumnType("bit");

                    b.Property<string>("Strength")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("MedicineProducts");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Pharmacy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DefaultWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OfficialPharmacyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacistSyndicateId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacyCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacyLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PharmacyStatus")
                        .HasColumnType("int");

                    b.Property<string>("PharmacyType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("DefaultWarehouseId");

                    b.ToTable("Pharmacies");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PublicVerificationScan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScanCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ScannedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ScannedBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScannedGTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScannedSerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UnitCodeId")
                        .HasColumnType("int");

                    b.Property<int?>("VerificationResult")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UnitCodeId");

                    b.ToTable("PublicVerificationScans");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AdminNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DocumentsOverallStatus")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("EntityName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("InspectionRequestedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("InspectionScheduledDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InspectorNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("RegistrationStatus")
                        .HasColumnType("int");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RepresentativeName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SubmittedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SystemUserId")
                        .HasColumnType("int");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("PharmacyId");

                    b.HasIndex("SystemUserId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("RegistrationRequests");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Shipment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("Destination")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DestinationPharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("DestinationWarehouseId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DispatchDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DispatchedByUserId")
                        .HasColumnType("int");

                    b.Property<long?>("ExpectedQuantity")
                        .HasColumnType("bigint");

                    b.Property<string>("InspectionResult")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ReceivedByUserId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ReceivedDate")
                        .HasColumnType("datetime2");

                    b.Property<long?>("ReceivedQuantity")
                        .HasColumnType("bigint");

                    b.Property<bool?>("RequiresColdChain")
                        .HasColumnType("bit");

                    b.Property<int?>("ShipmentStatus")
                        .HasColumnType("int");

                    b.Property<int?>("ShipmentType")
                        .HasColumnType("int");

                    b.Property<string>("Source")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SourceFactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("SourceWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("TransferCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("DestinationPharmacyId");

                    b.HasIndex("DestinationWarehouseId");

                    b.HasIndex("SourceFactoryId");

                    b.HasIndex("SourceWarehouseId");

                    b.ToTable("Shipments");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("Department")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<string>("Facility")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("HireDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InsuranceNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsSuspended")
                        .HasColumnType("bit");

                    b.Property<string>("JobGrade")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("MobileNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NationalId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OfficialEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PersonalEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Qualification")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Role")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SystemUsers");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UnitCode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("CodeValueHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CurrentHolderName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CurrentHolderType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("FirstScannedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("GTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ScanCount")
                        .HasColumnType("int");

                    b.Property<string>("SerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UnitCodeValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UnitStatus")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.ToTable("UnitCodes");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Warehouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasDeliveryService")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQuarantineArea")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OfficialWarehouseName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("WarehouseCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WarehouseLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("WarehouseStatus")
                        .HasColumnType("int");

                    b.Property<string>("WarehouseType")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Warehouses");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Alert", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("Alerts")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Shipment", "Shipment")
                        .WithMany()
                        .HasForeignKey("ShipmentId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");

                    b.Navigation("Shipment");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuditLog", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", "DestinationPharmacy")
                        .WithMany()
                        .HasForeignKey("DestinationPharmacyId");

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "DestinationWarehouse")
                        .WithMany()
                        .HasForeignKey("DestinationWarehouseId");

                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "SourceFactory")
                        .WithMany()
                        .HasForeignKey("SourceFactoryId");

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "SourceWarehouse")
                        .WithMany()
                        .HasForeignKey("SourceWarehouseId");

                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("DestinationPharmacy");

                    b.Navigation("DestinationWarehouse");

                    b.Navigation("SourceFactory");

                    b.Navigation("SourceWarehouse");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuthRefreshToken", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "SystemUser")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("SystemUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("SystemUser");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "Factory")
                        .WithMany("Batches")
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.MedicineProduct", "MedicineProduct")
                        .WithMany("Batches")
                        .HasForeignKey("MedicineProductId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("MedicineProduct");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityDocument", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.RegistrationRequest", "RegistrationRequest")
                        .WithMany("Documents")
                        .HasForeignKey("RegistrationRequestId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("RegistrationRequest");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.InventoryStock", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("InventoryStocks")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", null)
                        .WithMany()
                        .HasForeignKey("PharmacyId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Pharmacy", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "DefaultWarehouse")
                        .WithMany()
                        .HasForeignKey("DefaultWarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("DefaultWarehouse");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PublicVerificationScan", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.UnitCode", "UnitCode")
                        .WithMany()
                        .HasForeignKey("UnitCodeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("UnitCode");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "Factory")
                        .WithMany()
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", "Pharmacy")
                        .WithMany()
                        .HasForeignKey("PharmacyId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "SystemUser")
                        .WithMany()
                        .HasForeignKey("SystemUserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "Warehouse")
                        .WithMany()
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("Pharmacy");

                    b.Navigation("SystemUser");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Shipment", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("Shipments")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", null)
                        .WithMany()
                        .HasForeignKey("DestinationPharmacyId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("DestinationWarehouseId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Factory", null)
                        .WithMany()
                        .HasForeignKey("SourceFactoryId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("SourceWarehouseId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UnitCode", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("UnitCodes")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.Navigation("Alerts");

                    b.Navigation("InventoryStocks");

                    b.Navigation("Shipments");

                    b.Navigation("UnitCodes");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Factory", b =>
                {
                    b.Navigation("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.MedicineProduct", b =>
                {
                    b.Navigation("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemUser", b =>
                {
                    b.Navigation("RefreshTokens");
                });
#pragma warning restore 612, 618
        }
    }
}
```

# FILE: src\EgyMediChain.Infrastructure\Migrations\20260806104827_EditStaffReportsAndSoftDelete.cs
```
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgyMediChain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditStaffReportsAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SystemUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "SystemUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SystemUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PasswordResetRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    RequestedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HashedOtp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResetTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsumedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestIp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetRequests_SystemUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SystemUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReportType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Format = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParametersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TwoFactorRequired = table.Column<bool>(type: "bit", nullable: false),
                    SessionTimeoutMinutes = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    EmailAlerts = table.Column<bool>(type: "bit", nullable: false),
                    PushNotifications = table.Column<bool>(type: "bit", nullable: false),
                    CriticalAlerts = table.Column<bool>(type: "bit", nullable: false),
                    WeeklyReports = table.Column<bool>(type: "bit", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_SystemUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SystemUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequests_UserId",
                table: "PasswordResetRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetRequests");

            migrationBuilder.DropTable(
                name: "ReportJobs");

            migrationBuilder.DropTable(
                name: "SystemConfigurations");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SystemUsers");
        }
    }
}
```

# FILE: src\EgyMediChain.Infrastructure\Migrations\20260806104827_EditStaffReportsAndSoftDelete.Designer.cs
```
// <auto-generated />
using System;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EgyMediChain.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260806104827_EditStaffReportsAndSoftDelete")]
    partial class EditStaffReportsAndSoftDelete
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Alert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AlertCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("AlertStatus")
                        .HasColumnType("int");

                    b.Property<int?>("AlertType")
                        .HasColumnType("int");

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("EntityName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ResolvedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Severity")
                        .HasColumnType("int");

                    b.Property<int?>("ShipmentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("ShipmentId");

                    b.ToTable("Alerts");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("Action")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DestinationPharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("DestinationWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("IpAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LogCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NewValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OldValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Result")
                        .HasColumnType("int");

                    b.Property<int?>("Role")
                        .HasColumnType("int");

                    b.Property<int?>("SourceFactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("SourceWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("UserDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DestinationPharmacyId");

                    b.HasIndex("DestinationWarehouseId");

                    b.HasIndex("SourceFactoryId");

                    b.HasIndex("SourceWarehouseId");

                    b.HasIndex("UserId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuthRefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedByIp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("RevokedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SystemUserId")
                        .HasColumnType("int");

                    b.Property<string>("Token")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SystemUserId");

                    b.ToTable("AuthRefreshTokens");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("BatchStatus")
                        .HasColumnType("int");

                    b.Property<long?>("BlockedUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CreatedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("CurrentLocation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("int");

                    b.Property<long?>("GeneratedUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("InPharmacyUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("InWarehouseUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ManufacturingDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("MedicineProductId")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("OpenAlertsCount")
                        .HasColumnType("int");

                    b.Property<long?>("Quantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("RecalledUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("ScanCountTotal")
                        .HasColumnType("bigint");

                    b.Property<int?>("SupplyChainStage")
                        .HasColumnType("int");

                    b.Property<long?>("SuspiciousUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("TotalUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("MedicineProductId");

                    b.ToTable("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityDocument", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("DocumentStatus")
                        .HasColumnType("int");

                    b.Property<string>("DocumentType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RegistrationRequestId")
                        .HasColumnType("int");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ReviewedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ReviewedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UploadedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("RegistrationRequestId");

                    b.ToTable("EntityDocuments");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityLicense", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FileUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("IssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("LicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LicenseType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EntityLicenses");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Factory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CommercialRegistrationNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DosageFormsProduced")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EstablishedYear")
                        .HasColumnType("int");

                    b.Property<string>("FactoryCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FactoryLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("FactoryStatus")
                        .HasColumnType("int");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasFinishedGoodsStore")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQualityControlLab")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQuarantineArea")
                        .HasColumnType("bit");

                    b.Property<string>("LegalCompanyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("MainProductionTypes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OfficialFactoryName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("QualityCertificates")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationApprovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegistrationApprovedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegistrationNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RegistrationRequestNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationSubmittedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("StorageTypes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TaxCardNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TechnicalOperatingLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TotalBatches")
                        .HasColumnType("int");

                    b.Property<int?>("TotalProductionLines")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Factories");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.InventoryStock", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long?>("AvailableQuantity")
                        .HasColumnType("bigint");

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("HolderName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HolderType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("InventoryStatus")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int?>("PharmacyId")
                        .HasColumnType("int");

                    b.Property<long?>("QuarantinedQuantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("ReservedQuantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("TotalReceivedQuantity")
                        .HasColumnType("bigint");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("PharmacyId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("InventoryStocks");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.MedicineProduct", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DosageForm")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("RequiresColdChain")
                        .HasColumnType("bit");

                    b.Property<string>("Strength")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("MedicineProducts");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PasswordResetRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Attempts")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ConsumedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("HashedOtp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestIp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestedEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ResetTokenExpiresAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ResetTokenHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("VerifiedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("PasswordResetRequests");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Pharmacy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DefaultWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OfficialPharmacyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacistSyndicateId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacyCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacyLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PharmacyStatus")
                        .HasColumnType("int");

                    b.Property<string>("PharmacyType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("DefaultWarehouseId");

                    b.ToTable("Pharmacies");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PublicVerificationScan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScanCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ScannedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ScannedBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScannedGTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScannedSerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UnitCodeId")
                        .HasColumnType("int");

                    b.Property<int?>("VerificationResult")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UnitCodeId");

                    b.ToTable("PublicVerificationScans");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AdminNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DocumentsOverallStatus")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("EntityName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("InspectionRequestedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("InspectionScheduledDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InspectorNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("RegistrationStatus")
                        .HasColumnType("int");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RepresentativeName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SubmittedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SystemUserId")
                        .HasColumnType("int");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("PharmacyId");

                    b.HasIndex("SystemUserId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("RegistrationRequests");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.ReportJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FilePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Format")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ParametersJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReportType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RequestedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("RequestedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ReportJobs");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Shipment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("Destination")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DestinationPharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("DestinationWarehouseId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DispatchDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DispatchedByUserId")
                        .HasColumnType("int");

                    b.Property<long?>("ExpectedQuantity")
                        .HasColumnType("bigint");

                    b.Property<string>("InspectionResult")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ReceivedByUserId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ReceivedDate")
                        .HasColumnType("datetime2");

                    b.Property<long?>("ReceivedQuantity")
                        .HasColumnType("bigint");

                    b.Property<bool?>("RequiresColdChain")
                        .HasColumnType("bit");

                    b.Property<int?>("ShipmentStatus")
                        .HasColumnType("int");

                    b.Property<int?>("ShipmentType")
                        .HasColumnType("int");

                    b.Property<string>("Source")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SourceFactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("SourceWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("TransferCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("DestinationPharmacyId");

                    b.HasIndex("DestinationWarehouseId");

                    b.HasIndex("SourceFactoryId");

                    b.HasIndex("SourceWarehouseId");

                    b.ToTable("Shipments");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("SessionTimeoutMinutes")
                        .HasColumnType("int");

                    b.Property<bool>("TwoFactorRequired")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("UpdatedByUserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("SystemConfigurations");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DeletedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("Department")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<string>("Facility")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("HireDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InsuranceNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsSuspended")
                        .HasColumnType("bit");

                    b.Property<string>("JobGrade")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("MobileNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NationalId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OfficialEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PersonalEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Qualification")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Role")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SystemUsers");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UnitCode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("CodeValueHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CurrentHolderName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CurrentHolderType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("FirstScannedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("GTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ScanCount")
                        .HasColumnType("int");

                    b.Property<string>("SerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UnitCodeValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UnitStatus")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.ToTable("UnitCodes");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UserPreference", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("CriticalAlerts")
                        .HasColumnType("bit");

                    b.Property<bool>("EmailAlerts")
                        .HasColumnType("bit");

                    b.Property<bool>("PushNotifications")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.Property<bool>("WeeklyReports")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserPreferences");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Warehouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasDeliveryService")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQuarantineArea")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OfficialWarehouseName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("WarehouseCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WarehouseLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("WarehouseStatus")
                        .HasColumnType("int");

                    b.Property<string>("WarehouseType")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Warehouses");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Alert", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("Alerts")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Shipment", "Shipment")
                        .WithMany()
                        .HasForeignKey("ShipmentId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");

                    b.Navigation("Shipment");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuditLog", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", "DestinationPharmacy")
                        .WithMany()
                        .HasForeignKey("DestinationPharmacyId");

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "DestinationWarehouse")
                        .WithMany()
                        .HasForeignKey("DestinationWarehouseId");

                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "SourceFactory")
                        .WithMany()
                        .HasForeignKey("SourceFactoryId");

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "SourceWarehouse")
                        .WithMany()
                        .HasForeignKey("SourceWarehouseId");

                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("DestinationPharmacy");

                    b.Navigation("DestinationWarehouse");

                    b.Navigation("SourceFactory");

                    b.Navigation("SourceWarehouse");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuthRefreshToken", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "SystemUser")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("SystemUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("SystemUser");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "Factory")
                        .WithMany("Batches")
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.MedicineProduct", "MedicineProduct")
                        .WithMany("Batches")
                        .HasForeignKey("MedicineProductId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("MedicineProduct");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityDocument", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.RegistrationRequest", "RegistrationRequest")
                        .WithMany("Documents")
                        .HasForeignKey("RegistrationRequestId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("RegistrationRequest");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.InventoryStock", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("InventoryStocks")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", null)
                        .WithMany()
                        .HasForeignKey("PharmacyId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PasswordResetRequest", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Pharmacy", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "DefaultWarehouse")
                        .WithMany()
                        .HasForeignKey("DefaultWarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("DefaultWarehouse");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PublicVerificationScan", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.UnitCode", "UnitCode")
                        .WithMany()
                        .HasForeignKey("UnitCodeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("UnitCode");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "Factory")
                        .WithMany()
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", "Pharmacy")
                        .WithMany()
                        .HasForeignKey("PharmacyId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "SystemUser")
                        .WithMany()
                        .HasForeignKey("SystemUserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "Warehouse")
                        .WithMany()
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("Pharmacy");

                    b.Navigation("SystemUser");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Shipment", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("Shipments")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", null)
                        .WithMany()
                        .HasForeignKey("DestinationPharmacyId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("DestinationWarehouseId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Factory", null)
                        .WithMany()
                        .HasForeignKey("SourceFactoryId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("SourceWarehouseId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UnitCode", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("UnitCodes")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UserPreference", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.Navigation("Alerts");

                    b.Navigation("InventoryStocks");

                    b.Navigation("Shipments");

                    b.Navigation("UnitCodes");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Factory", b =>
                {
                    b.Navigation("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.MedicineProduct", b =>
                {
                    b.Navigation("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemUser", b =>
                {
                    b.Navigation("RefreshTokens");
                });
#pragma warning restore 612, 618
        }
    }
}
```

# FILE: src\EgyMediChain.Infrastructure\Migrations\AppDbContextModelSnapshot.cs
```
// <auto-generated />
using System;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EgyMediChain.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Alert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AlertCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("AlertStatus")
                        .HasColumnType("int");

                    b.Property<int?>("AlertType")
                        .HasColumnType("int");

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("EntityName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ResolvedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Severity")
                        .HasColumnType("int");

                    b.Property<int?>("ShipmentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("ShipmentId");

                    b.ToTable("Alerts");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("Action")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DestinationPharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("DestinationWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("IpAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LogCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NewValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OldValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Result")
                        .HasColumnType("int");

                    b.Property<int?>("Role")
                        .HasColumnType("int");

                    b.Property<int?>("SourceFactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("SourceWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("UserDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DestinationPharmacyId");

                    b.HasIndex("DestinationWarehouseId");

                    b.HasIndex("SourceFactoryId");

                    b.HasIndex("SourceWarehouseId");

                    b.HasIndex("UserId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuthRefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedByIp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("RevokedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SystemUserId")
                        .HasColumnType("int");

                    b.Property<string>("Token")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SystemUserId");

                    b.ToTable("AuthRefreshTokens");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("BatchStatus")
                        .HasColumnType("int");

                    b.Property<long?>("BlockedUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CreatedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("CurrentLocation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("int");

                    b.Property<long?>("GeneratedUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("InPharmacyUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("InWarehouseUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ManufacturingDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("MedicineProductId")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("OpenAlertsCount")
                        .HasColumnType("int");

                    b.Property<long?>("Quantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("RecalledUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("ScanCountTotal")
                        .HasColumnType("bigint");

                    b.Property<int?>("SupplyChainStage")
                        .HasColumnType("int");

                    b.Property<long?>("SuspiciousUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<long?>("TotalUnitCodes")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("MedicineProductId");

                    b.ToTable("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityDocument", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("DocumentStatus")
                        .HasColumnType("int");

                    b.Property<string>("DocumentType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RegistrationRequestId")
                        .HasColumnType("int");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ReviewedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ReviewedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UploadedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("RegistrationRequestId");

                    b.ToTable("EntityDocuments");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityLicense", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FileUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("IssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("LicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LicenseType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EntityLicenses");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Factory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CommercialRegistrationNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DosageFormsProduced")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EstablishedYear")
                        .HasColumnType("int");

                    b.Property<string>("FactoryCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FactoryLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("FactoryStatus")
                        .HasColumnType("int");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasFinishedGoodsStore")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQualityControlLab")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQuarantineArea")
                        .HasColumnType("bit");

                    b.Property<string>("LegalCompanyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("MainProductionTypes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OfficialFactoryName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("QualityCertificates")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationApprovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegistrationApprovedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegistrationNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RegistrationRequestNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationSubmittedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("StorageTypes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TaxCardNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TechnicalOperatingLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TotalBatches")
                        .HasColumnType("int");

                    b.Property<int?>("TotalProductionLines")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Factories");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.InventoryStock", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long?>("AvailableQuantity")
                        .HasColumnType("bigint");

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("HolderName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HolderType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("InventoryStatus")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int?>("PharmacyId")
                        .HasColumnType("int");

                    b.Property<long?>("QuarantinedQuantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("ReservedQuantity")
                        .HasColumnType("bigint");

                    b.Property<long?>("TotalReceivedQuantity")
                        .HasColumnType("bigint");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("PharmacyId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("InventoryStocks");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.MedicineProduct", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DosageForm")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("RequiresColdChain")
                        .HasColumnType("bit");

                    b.Property<string>("Strength")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("MedicineProducts");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PasswordResetRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Attempts")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ConsumedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("HashedOtp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestIp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestedEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ResetTokenExpiresAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ResetTokenHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("VerifiedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("PasswordResetRequests");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Pharmacy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DefaultWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OfficialPharmacyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacistSyndicateId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacyCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PharmacyLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PharmacyStatus")
                        .HasColumnType("int");

                    b.Property<string>("PharmacyType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("DefaultWarehouseId");

                    b.ToTable("Pharmacies");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PublicVerificationScan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScanCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ScannedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ScannedBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScannedGTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScannedSerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UnitCodeId")
                        .HasColumnType("int");

                    b.Property<int?>("VerificationResult")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UnitCodeId");

                    b.ToTable("PublicVerificationScans");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AdminNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DocumentsOverallStatus")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("EntityName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("InspectionRequestedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("InspectionScheduledDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InspectorNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("RegistrationStatus")
                        .HasColumnType("int");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RepresentativeName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SubmittedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SystemUserId")
                        .HasColumnType("int");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("PharmacyId");

                    b.HasIndex("SystemUserId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("RegistrationRequests");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.ReportJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FilePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Format")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ParametersJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReportType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RequestedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("RequestedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ReportJobs");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Shipment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("Destination")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DestinationPharmacyId")
                        .HasColumnType("int");

                    b.Property<int?>("DestinationWarehouseId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DispatchDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DispatchedByUserId")
                        .HasColumnType("int");

                    b.Property<long?>("ExpectedQuantity")
                        .HasColumnType("bigint");

                    b.Property<string>("InspectionResult")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ReceivedByUserId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ReceivedDate")
                        .HasColumnType("datetime2");

                    b.Property<long?>("ReceivedQuantity")
                        .HasColumnType("bigint");

                    b.Property<bool?>("RequiresColdChain")
                        .HasColumnType("bit");

                    b.Property<int?>("ShipmentStatus")
                        .HasColumnType("int");

                    b.Property<int?>("ShipmentType")
                        .HasColumnType("int");

                    b.Property<string>("Source")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SourceFactoryId")
                        .HasColumnType("int");

                    b.Property<int?>("SourceWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("TransferCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("DestinationPharmacyId");

                    b.HasIndex("DestinationWarehouseId");

                    b.HasIndex("SourceFactoryId");

                    b.HasIndex("SourceWarehouseId");

                    b.ToTable("Shipments");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("SessionTimeoutMinutes")
                        .HasColumnType("int");

                    b.Property<bool>("TwoFactorRequired")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("UpdatedByUserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("SystemConfigurations");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DeletedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("Department")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<int?>("EntityType")
                        .HasColumnType("int");

                    b.Property<string>("Facility")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("HireDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InsuranceNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsSuspended")
                        .HasColumnType("bit");

                    b.Property<string>("JobGrade")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("MobileNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NationalId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OfficialEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PersonalEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Qualification")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Role")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SystemUsers");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UnitCode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BatchId")
                        .HasColumnType("int");

                    b.Property<string>("CodeValueHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CurrentHolderName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CurrentHolderType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("FirstScannedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("GTIN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ScanCount")
                        .HasColumnType("int");

                    b.Property<string>("SerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UnitCodeValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UnitStatus")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.ToTable("UnitCodes");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UserPreference", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("CriticalAlerts")
                        .HasColumnType("bit");

                    b.Property<bool>("EmailAlerts")
                        .HasColumnType("bit");

                    b.Property<bool>("PushNotifications")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.Property<bool>("WeeklyReports")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserPreferences");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Warehouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DistrictArea")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Governorate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasColdStorage")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasDeliveryService")
                        .HasColumnType("bit");

                    b.Property<bool?>("HasQuarantineArea")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LicenseExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LicenseIssueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OfficialWarehouseName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("WarehouseCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WarehouseLicenseNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("WarehouseStatus")
                        .HasColumnType("int");

                    b.Property<string>("WarehouseType")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Warehouses");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Alert", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("Alerts")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Shipment", "Shipment")
                        .WithMany()
                        .HasForeignKey("ShipmentId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");

                    b.Navigation("Shipment");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuditLog", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", "DestinationPharmacy")
                        .WithMany()
                        .HasForeignKey("DestinationPharmacyId");

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "DestinationWarehouse")
                        .WithMany()
                        .HasForeignKey("DestinationWarehouseId");

                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "SourceFactory")
                        .WithMany()
                        .HasForeignKey("SourceFactoryId");

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "SourceWarehouse")
                        .WithMany()
                        .HasForeignKey("SourceWarehouseId");

                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("DestinationPharmacy");

                    b.Navigation("DestinationWarehouse");

                    b.Navigation("SourceFactory");

                    b.Navigation("SourceWarehouse");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.AuthRefreshToken", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "SystemUser")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("SystemUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("SystemUser");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "Factory")
                        .WithMany("Batches")
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.MedicineProduct", "MedicineProduct")
                        .WithMany("Batches")
                        .HasForeignKey("MedicineProductId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("MedicineProduct");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.EntityDocument", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.RegistrationRequest", "RegistrationRequest")
                        .WithMany("Documents")
                        .HasForeignKey("RegistrationRequestId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("RegistrationRequest");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.InventoryStock", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("InventoryStocks")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", null)
                        .WithMany()
                        .HasForeignKey("PharmacyId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PasswordResetRequest", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Pharmacy", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "DefaultWarehouse")
                        .WithMany()
                        .HasForeignKey("DefaultWarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("DefaultWarehouse");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.PublicVerificationScan", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.UnitCode", "UnitCode")
                        .WithMany()
                        .HasForeignKey("UnitCodeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("UnitCode");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Factory", "Factory")
                        .WithMany()
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", "Pharmacy")
                        .WithMany()
                        .HasForeignKey("PharmacyId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "SystemUser")
                        .WithMany()
                        .HasForeignKey("SystemUserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", "Warehouse")
                        .WithMany()
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("Pharmacy");

                    b.Navigation("SystemUser");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Shipment", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("Shipments")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Pharmacy", null)
                        .WithMany()
                        .HasForeignKey("DestinationPharmacyId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("DestinationWarehouseId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Factory", null)
                        .WithMany()
                        .HasForeignKey("SourceFactoryId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("EgyMediChain.Domain.Entities.Warehouse", null)
                        .WithMany()
                        .HasForeignKey("SourceWarehouseId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UnitCode", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.Batch", "Batch")
                        .WithMany("UnitCodes")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Batch");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.UserPreference", b =>
                {
                    b.HasOne("EgyMediChain.Domain.Entities.SystemUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Batch", b =>
                {
                    b.Navigation("Alerts");

                    b.Navigation("InventoryStocks");

                    b.Navigation("Shipments");

                    b.Navigation("UnitCodes");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.Factory", b =>
                {
                    b.Navigation("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.MedicineProduct", b =>
                {
                    b.Navigation("Batches");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.RegistrationRequest", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("EgyMediChain.Domain.Entities.SystemUser", b =>
                {
                    b.Navigation("RefreshTokens");
                });
#pragma warning restore 612, 618
        }
    }
}
```

# FILE: src\EgyMediChain.Infrastructure\Persistence\AppDbContext.cs
```
using EgyMediChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SystemUser> SystemUsers => Set<SystemUser>();
    public DbSet<AuthRefreshToken> AuthRefreshTokens => Set<AuthRefreshToken>();
    public DbSet<RegistrationRequest> RegistrationRequests => Set<RegistrationRequest>();
    public DbSet<EntityDocument> EntityDocuments => Set<EntityDocument>();
    public DbSet<Factory> Factories => Set<Factory>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
    public DbSet<MedicineProduct> MedicineProducts => Set<MedicineProduct>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<UnitCode> UnitCodes => Set<UnitCode>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<InventoryStock> InventoryStocks => Set<InventoryStock>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<PublicVerificationScan> PublicVerificationScans => Set<PublicVerificationScan>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EntityLicense> EntityLicenses => Set<EntityLicense>();
    public DbSet<PasswordResetRequest> PasswordResetRequests => Set<PasswordResetRequest>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
    public DbSet<ReportJob> ReportJobs => Set<ReportJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Keep constraints loose on purpose (nullable-friendly, minimal FK cascade restrictions)
        // so the API stays forgiving for a fast-moving frontend integration.

        modelBuilder.Entity<RegistrationRequest>()
            .HasOne(r => r.Factory).WithMany().HasForeignKey(r => r.FactoryId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<RegistrationRequest>()
            .HasOne(r => r.Warehouse).WithMany().HasForeignKey(r => r.WarehouseId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<RegistrationRequest>()
            .HasOne(r => r.Pharmacy).WithMany().HasForeignKey(r => r.PharmacyId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<RegistrationRequest>()
            .HasOne(r => r.SystemUser).WithMany().HasForeignKey(r => r.SystemUserId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EntityDocument>()
            .HasOne(d => d.RegistrationRequest).WithMany(r => r.Documents)
            .HasForeignKey(d => d.RegistrationRequestId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Pharmacy>()
            .HasOne(p => p.DefaultWarehouse).WithMany().HasForeignKey(p => p.DefaultWarehouseId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Batch>()
            .HasOne(b => b.MedicineProduct).WithMany(m => m.Batches).HasForeignKey(b => b.MedicineProductId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Batch>()
            .HasOne(b => b.Factory).WithMany(f => f.Batches).HasForeignKey(b => b.FactoryId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<UnitCode>()
            .HasOne(u => u.Batch).WithMany(b => b.UnitCodes).HasForeignKey(u => u.BatchId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Shipment>()
    .HasOne(s => s.Batch)
    .WithMany(b => b.Shipments)
    .HasForeignKey(s => s.BatchId)
    .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Shipment>()
      .HasOne<Factory>()
      .WithMany()
      .HasForeignKey(s => s.SourceFactoryId)
      .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Shipment>()
            .HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(s => s.SourceWarehouseId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Shipment>()
            .HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(s => s.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Shipment>()
            .HasOne<Pharmacy>()
            .WithMany()
            .HasForeignKey(s => s.DestinationPharmacyId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<InventoryStock>()
            .HasOne(i => i.Batch).WithMany(b => b.InventoryStocks).HasForeignKey(i => i.BatchId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<InventoryStock>()
            .HasOne<Warehouse>().WithMany().HasForeignKey(i => i.WarehouseId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<InventoryStock>()
            .HasOne<Pharmacy>().WithMany().HasForeignKey(i => i.PharmacyId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Alert>()
            .HasOne(a => a.Batch).WithMany(b => b.Alerts).HasForeignKey(a => a.BatchId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Alert>()
            .HasOne(a => a.Shipment).WithMany().HasForeignKey(a => a.ShipmentId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PublicVerificationScan>()
            .HasOne(s => s.UnitCode).WithMany().HasForeignKey(s => s.UnitCodeId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AuthRefreshToken>()
            .HasOne(t => t.SystemUser).WithMany(u => u.RefreshTokens).HasForeignKey(t => t.SystemUserId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
```

# FILE: src\EgyMediChain.Infrastructure\Persistence\DbSeeder.cs
```
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;

namespace EgyMediChain.Infrastructure.Persistence;

public static class DbSeeder
{
    private static readonly Random Rng = new(42);

    private static readonly string[] Governorates =
    {
        "Cairo", "Giza", "Alexandria", "Dakahlia", "Sharqia", "Gharbia", "Menoufia",
        "Qalyubia", "Beheira", "Assiut", "Sohag", "Aswan", "Luxor", "Port Said",
        "Ismailia", "Suez", "Fayoum", "Beni Suef", "Minya", "Damietta"
    };

    private static readonly string[] FactoryCompanyNames =
    {
        "Delta Pharma Factory", "EIPICO Factory", "Misr Co. for Pharma", "Upper Egypt Factory",
        "Alexandria Medicines", "Nile Pharma", "Cairo Chemical Industries", "October Pharma Co.",
        "Suez Pharmaceutical Factory", "Rameda Factory", "Amoun Pharma Factory", "Sigma Factory",
        "Pharco Factory", "Adwia Factory", "Marcyrl Factory", "Hikma Egypt Factory",
        "Memphis Pharma Factory", "Al Andalous Factory", "Minapharm Factory", "Chemipharm Factory"
    };

    private static readonly string[] WarehouseNames =
    {
        "Cairo Medical Storage", "Portsaid Distribution", "Assiut Central Warehouse",
        "Delta Storage Warehouse", "Alex Warehouse", "Giza Main Depot", "Mansoura Regional Store",
        "Tanta Central Depot", "Ismailia Warehouse", "Aswan Storage Facility", "Beni Suef Depot",
        "Fayoum Regional Warehouse", "Minya Central Store", "Sohag Storage Hub", "Damietta Depot"
    };

    private static readonly string[] PharmacyNames =
    {
        "Alexandria Drug Store", "Mansoura Pharmacy", "Giza City Pharmacy", "Heliopolis Pharmacy",
        "Tanta Pharmacy", "Nasr City Pharmacy", "Zamalek Pharmacy", "Maadi Pharmacy",
        "Dokki Pharmacy", "Sheraton Pharmacy", "Smouha Pharmacy", "Mohandessin Pharmacy",
        "Rehab Pharmacy", "October Pharmacy", "Agouza Pharmacy", "Sidi Gaber Pharmacy"
    };

    private static readonly string[] ProductNames =
    {
        "Panadol Extra", "Brufen 400mg", "Augmentin 1g", "Cipro 500mg", "Flagyl 500mg",
        "Voltaren 75mg", "Amoxicillin 500mg", "Diclofenac 50mg", "Losec 20mg", "Zithromax 500mg",
        "Concor 5mg", "Glucophage 500mg", "Lipitor 20mg", "Nexium 40mg", "Ventolin Inhaler",
        "Cataflam 50mg", "Neurobion Forte", "Congestal", "Adol Extra", "Tensopin 5mg"
    };

    private static readonly string[] DosageForms = { "Tablet", "Coated Tablet", "Capsule", "Syrup", "Injection" };
    private static readonly string[] FirstNames = { "Ahmed", "Mona", "Yasser", "Heba", "Khaled", "Sara", "Omar", "Tamer", "Mostafa", "Sarah", "Amr", "Nour", "Youssef", "Dina", "Karim", "Rania" };
    private static readonly string[] LastNames = { "Ali", "Samir", "Mohamed", "Mostafa", "Hassan", "Mahmoud", "Refaat", "Fathy", "Nabil", "Ahmed", "Youssef", "Kamal", "Adel", "Fahmy" };

    // Fixed, memorable demo accounts - one per role, so the email itself tells you which
    // role it logs in as. Password is the same for all of them to keep testing simple.
    public const string DemoPassword = "Passw0rd!123";

    public static readonly (string Email, string FullName, SystemRole Role, EntityKind EntityType)[] RoleTestAccounts =
    {
        ("superadmin@egymedichain.com",     "Dr. Saif (Super Admin)",   SystemRole.SuperAdmin,     EntityKind.Ministry),
        ("ministryadmin@egymedichain.com",  "Ahmed Ali (Ministry Admin)", SystemRole.MinistryAdmin,  EntityKind.Ministry),
        ("ministryviewer@egymedichain.com", "Sara Mahmoud (Ministry Viewer)", SystemRole.MinistryViewer, EntityKind.Ministry),
        ("factoryuser@egymedichain.com",    "Ahmed Ali (Factory Admin)", SystemRole.FactoryUser,    EntityKind.Factory),
        ("warehouseuser@egymedichain.com",  "Ahmed Hassan (Warehouse Admin)", SystemRole.WarehouseUser,  EntityKind.Warehouse),
        ("pharmacyuser@egymedichain.com",   "Yasser Mohamed (Pharmacy Admin)", SystemRole.PharmacyUser,   EntityKind.Pharmacy)
    };

    // Runs on every startup (not just first seed) so the fixed demo accounts always exist
    // and always have a known password, even if the rest of the data was seeded earlier.
    public static void EnsureRoleTestAccounts(AppDbContext db)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword, 12);
        var changed = false;

        // Link the demo accounts to specific, recognizable seeded records (matching the
        // Factory/Warehouse Portal mockups) instead of "whichever row happens to be first".
        var demoFactoryId = db.Factories.Where(f => f.OfficialFactoryName == "EIPICO Factory").Select(f => (int?)f.Id).FirstOrDefault()
                             ?? db.Factories.Select(f => (int?)f.Id).FirstOrDefault();
        var demoWarehouseId = db.Warehouses.Where(w => w.OfficialWarehouseName == "Cairo Medical Storage").Select(w => (int?)w.Id).FirstOrDefault()
                               ?? db.Warehouses.Select(w => (int?)w.Id).FirstOrDefault();
        var demoPharmacyId = db.Pharmacies.Where(p => p.OfficialPharmacyName == "Alexandria Drug Store").Select(p => (int?)p.Id).FirstOrDefault()
                              ?? db.Pharmacies.Select(p => (int?)p.Id).FirstOrDefault();

        if (demoFactoryId.HasValue)
        {
            var f = db.Factories.Find(demoFactoryId.Value);
            if (f != null && f.FactoryStatus != FacilityStatus.Active) { f.FactoryStatus = FacilityStatus.Active; changed = true; }
        }
        if (demoWarehouseId.HasValue)
        {
            var w = db.Warehouses.Find(demoWarehouseId.Value);
            if (w != null && w.WarehouseStatus != FacilityStatus.Active) { w.WarehouseStatus = FacilityStatus.Active; changed = true; }
        }
        if (demoPharmacyId.HasValue)
        {
            var p = db.Pharmacies.Find(demoPharmacyId.Value);
            if (p != null && p.PharmacyStatus != FacilityStatus.Active) { p.PharmacyStatus = FacilityStatus.Active; changed = true; }
        }

        foreach (var (email, fullName, role, entityType) in RoleTestAccounts)
        {
            int? entityId = entityType switch
            {
                EntityKind.Factory => demoFactoryId,
                EntityKind.Warehouse => demoWarehouseId,
                EntityKind.Pharmacy => demoPharmacyId,
                _ => null
            };

            
            
            var user = db.SystemUsers.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                db.SystemUsers.Add(new SystemUser
                {
                    FullName = fullName,
                    Email = email,
                    MobileNumber = $"0100{Rng.Next(1000000, 9999999)}",
                    NationalId = $"{Rng.NextInt64(10000000000000, 29999999999999)}",
                    Role = role,
                    EntityType = entityType,
                    EntityId = entityId,
                    EmailConfirmed = true,
                    IsActive = true,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                changed = true;
            }
            else
            {
                // keep the known password + role + entity link stable even if something else changed it
                user.PasswordHash = passwordHash;
                user.Role = role;
                user.EntityType = entityType;
                user.EntityId = entityId;
                user.IsActive = true;
                user.EmailConfirmed = true;
                changed = true;
            }
        }

        if (changed) db.SaveChanges();
    }

    public static void Seed(AppDbContext db)
    {
        if (db.SystemUsers.Any())
        {
            EnsureRoleTestAccounts(db);
            SeedDemoOperationalData(db);
            return; // rest of the data already seeded
        }

        var users = SeedUsers();
        db.SystemUsers.AddRange(users);
        db.SaveChanges();

        var factories = SeedFactories();
        db.Factories.AddRange(factories);
        db.SaveChanges();

        var warehouses = SeedWarehouses();
        db.Warehouses.AddRange(warehouses);
        db.SaveChanges();

        var pharmacies = SeedPharmacies(warehouses);
        db.Pharmacies.AddRange(pharmacies);
        db.SaveChanges();

        var products = SeedProducts();
        db.MedicineProducts.AddRange(products);
        db.SaveChanges();

        var batches = SeedBatches(products, factories, users);
        db.Batches.AddRange(batches);
        db.SaveChanges();

        var unitCodes = SeedUnitCodes(batches);
        db.UnitCodes.AddRange(unitCodes);
        db.SaveChanges();

        var shipments = SeedShipments(batches, factories, warehouses, pharmacies);
        db.Shipments.AddRange(shipments);
        db.SaveChanges();

        var inventory = SeedInventory(batches, warehouses, pharmacies);
        db.InventoryStocks.AddRange(inventory);
        db.SaveChanges();

        var alerts = SeedAlerts(batches, factories, warehouses, pharmacies);
        db.Alerts.AddRange(alerts);
        db.SaveChanges();

        var scans = SeedScans(unitCodes, products, batches);
        db.PublicVerificationScans.AddRange(scans);
        db.SaveChanges();

        var registrationRequests = SeedRegistrationRequests(users);
        db.RegistrationRequests.AddRange(registrationRequests);
        db.SaveChanges();

        var licenses = SeedLicenses(factories);
        db.EntityLicenses.AddRange(licenses);
        db.SaveChanges();

        var auditLogs = SeedAuditLogs(users);
        db.AuditLogs.AddRange(auditLogs);
        db.SaveChanges();

        EnsureRoleTestAccounts(db);
        SeedDemoOperationalData(db);
    }

    private static DateTime RandDate(int daysBackMin, int daysBackMax) =>
        DateTime.UtcNow.AddDays(-Rng.Next(daysBackMin, daysBackMax)).AddHours(Rng.Next(0, 23));

    private static string RandName() => $"{FirstNames[Rng.Next(FirstNames.Length)]} {LastNames[Rng.Next(LastNames.Length)]}";

    private static List<SystemUser> SeedUsers()
    {
        var list = new List<SystemUser>();
        list.Add(new SystemUser
        {
            FullName = "Dr. Saif", Email = "saif.superadmin@health.gov.eg", MobileNumber = "01000000001",
            NationalId = "29901011234567", Role = SystemRole.SuperAdmin, EntityType = EntityKind.Ministry,
            EmailConfirmed = true, IsActive = true, PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!", 12),
            LastLoginAt = RandDate(0, 1), CreatedAt = RandDate(300, 400)
        });
        for (int i = 0; i < 31; i++)
        {
            var role = i % 6 == 0 ? SystemRole.MinistryAdmin : i % 6 == 1 ? SystemRole.MinistryViewer :
                       i % 6 == 2 ? SystemRole.FactoryUser : i % 6 == 3 ? SystemRole.WarehouseUser :
                       i % 6 == 4 ? SystemRole.PharmacyUser : SystemRole.MinistryAdmin;
            var name = RandName();
            list.Add(new SystemUser
            {
                FullName = name,
                Email = $"{name.Replace(" ", ".").ToLower()}{i}@health.gov.eg",
                MobileNumber = $"01{Rng.Next(0, 2)}{Rng.Next(10000000, 99999999)}",
                NationalId = Rng.Next(0, 2) == 0 ? $"{Rng.NextInt64(10000000000000, 29999999999999)}" : null,
                Role = role,
                EntityType = EntityKind.Ministry,
                EmailConfirmed = Rng.Next(0, 10) > 1,
                IsActive = Rng.Next(0, 10) > 1,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Temp@12345", 12),
                LastLoginAt = Rng.Next(0, 10) > 1 ? RandDate(0, 30) : null,
                CreatedAt = RandDate(60, 400),
                UpdatedAt = RandDate(0, 60)
            });
        }
        return list;
    }

    private static List<Factory> SeedFactories()
    {
        var list = new List<Factory>();
        var statuses = new[] { FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Suspended, FacilityStatus.Inactive };
        var productionTypesOptions = new[] { "Tablets, Capsules, Injectables, Liquids", "Tablets, Syrups", "Capsules, Injectables", "Tablets, Ointments, Syrups" };
        var storageTypesOptions = new[] { "Ambient, Cold Storage", "Ambient Only", "Ambient, Cold Storage, Quarantine" };
        var certOptions = new[] { "ISO 9001:2015, ISO 14001:2015, GMP", "GMP, WHO-GMP", "ISO 9001:2015, GMP" };

        for (int i = 0; i < 45; i++)
        {
            var name = FactoryCompanyNames[i % FactoryCompanyNames.Length] + (i >= FactoryCompanyNames.Length ? $" #{i}" : "");
            var gov = Governorates[Rng.Next(Governorates.Length)];
            var isDemo = name == "EIPICO Factory";

            var factory = new Factory
            {
                FactoryCode = isDemo ? "FAC-2024-021" : $"FAC-2024-{(i + 1):000}",
                OfficialFactoryName = name,
                LegalCompanyName = name.Replace("Factory", "Pharmaceutical Co."),
                DosageFormsProduced = string.Join(", ", DosageForms.OrderBy(_ => Rng.Next()).Take(Rng.Next(1, 4))),
                Governorate = isDemo ? "Cairo" : gov,
                City = isDemo ? "Cairo" : gov,
                DistrictArea = isDemo ? "Industrial Zone A3" : $"Industrial Zone {(char)('A' + Rng.Next(0, 5))}",
                FullAddress = isDemo ? "10th of Ramadan City, Industrial Zone A3, Egypt" : $"Industrial Zone {(char)('A' + Rng.Next(0, 5))}, {gov}, Egypt",
                Phone = isDemo ? "+20 123 456 7890" : $"+20 1{Rng.Next(0, 2)} {Rng.Next(1000, 9999)} {Rng.Next(1000, 9999)}",
                Email = isDemo ? "factory@eipico.com" : $"factory@{name.Replace(" ", "").ToLower()}.com",
                FactoryLicenseNumber = $"FAC-2024-{(i + 1):000}",
                TechnicalOperatingLicenseNumber = $"TOL-2024-{(i + 11):000}",
                CommercialRegistrationNumber = isDemo ? "125478" : $"{Rng.Next(100000, 999999)}",
                TaxCardNumber = isDemo ? "548796321" : $"{Rng.Next(100000000, 999999999)}",
                LicenseIssueDate = RandDate(200, 900),
                LicenseExpiryDate = DateTime.UtcNow.AddDays(Rng.Next(-60, 900)),
                HasQualityControlLab = Rng.Next(0, 10) > 2,
                HasFinishedGoodsStore = Rng.Next(0, 10) > 1,
                HasColdStorage = isDemo ? true : Rng.Next(0, 10) > 4,
                HasQuarantineArea = Rng.Next(0, 10) > 3,
                EstablishedYear = isDemo ? 2005 : Rng.Next(1990, 2020),
                TotalProductionLines = isDemo ? 12 : Rng.Next(3, 20),
                MainProductionTypes = isDemo ? "Tablets, Capsules, Injectables, Liquids" : productionTypesOptions[Rng.Next(productionTypesOptions.Length)],
                StorageTypes = isDemo ? "Ambient, Cold Storage" : storageTypesOptions[Rng.Next(storageTypesOptions.Length)],
                QualityCertificates = isDemo ? "ISO 9001:2015, ISO 14001:2015, GMP" : certOptions[Rng.Next(certOptions.Length)],
                Description = isDemo
                    ? "EIPICO is a leading pharmaceutical manufacturer in Egypt committed to quality and compliance."
                    : $"{name} is a licensed pharmaceutical manufacturer operating in {gov}, Egypt.",
                RegistrationRequestNo = isDemo ? "REG-2024-00125" : $"REG-2024-{Rng.Next(10000, 99999)}",
                RegistrationSubmittedAt = RandDate(100, 400),
                RegistrationApprovedAt = RandDate(50, 100),
                RegistrationApprovedBy = "Ministry of Health",
                RegistrationExpiryDate = DateTime.UtcNow.AddDays(Rng.Next(300, 800)),
                RegistrationNotes = "All documents and information verified successfully.",
                FactoryStatus = isDemo ? FacilityStatus.Active : statuses[Rng.Next(statuses.Length)],
                TotalBatches = Rng.Next(5, 60),
                CreatedAt = RandDate(100, 500),
                UpdatedAt = RandDate(0, 90)
            };
            list.Add(factory);
        }
        return list;
    }

    private static List<Warehouse> SeedWarehouses()
    {
        var list = new List<Warehouse>();
        var statuses = new[] { FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Suspended, FacilityStatus.Inactive };
        var types = new[] { "Main Warehouse", "Regional Warehouse" };
        for (int i = 0; i < 40; i++)
        {
            var name = WarehouseNames[i % WarehouseNames.Length] + (i >= WarehouseNames.Length ? $" #{i}" : "");
            var gov = Governorates[Rng.Next(Governorates.Length)];
            var isDemo = name == "Cairo Medical Storage";

            list.Add(new Warehouse
            {
                WarehouseCode = isDemo ? "WH-CAI-001" : $"WH-2024-{(i + 1):000}",
                OfficialWarehouseName = name,
                WarehouseType = isDemo ? "Main Warehouse" : types[Rng.Next(types.Length)],
                Governorate = isDemo ? "Cairo" : gov,
                City = isDemo ? "Cairo" : gov,
                DistrictArea = isDemo ? "10th of Ramadan District" : $"{Rng.Next(1, 20)} Ahmed Fakhry St., Naar City",
                FullAddress = isDemo ? "Industrial Zone A3, Block 15, 10th of Ramadan City, Cairo, Egypt" : $"{Rng.Next(1, 20)} Ahmed Fakhry St., Naar City, {gov}, Egypt",
                Phone = isDemo ? "+20 10 1234 5678" : $"+20 1{Rng.Next(0, 2)} {Rng.Next(1000, 9999)} {Rng.Next(1000, 9999)}",
                Email = isDemo ? "ahmed.hassan@cairomed.com" : $"warehouse@{name.Replace(" ", "").ToLower()}.com",
                WarehouseLicenseNumber = isDemo ? "WH-CAI-001-LIC" : $"WH-2024-{(i + 1):000}",
                LicenseIssueDate = isDemo ? new DateTime(2023, 5, 12) : RandDate(200, 900),
                LicenseExpiryDate = isDemo ? new DateTime(2025, 5, 12) : DateTime.UtcNow.AddDays(Rng.Next(-60, 900)),
                HasColdStorage = isDemo ? true : Rng.Next(0, 10) > 3,
                HasQuarantineArea = isDemo ? true : Rng.Next(0, 10) > 3,
                HasDeliveryService = isDemo ? true : Rng.Next(0, 10) > 2,
                WarehouseStatus = isDemo ? FacilityStatus.Active : statuses[Rng.Next(statuses.Length)],
                CreatedAt = RandDate(100, 500),
                UpdatedAt = RandDate(0, 90)
            });
        }
        return list;
    }

    private static List<Pharmacy> SeedPharmacies(List<Warehouse> warehouses)
    {
        var list = new List<Pharmacy>();
        var statuses = new[] { FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Suspended, FacilityStatus.Inactive };
        for (int i = 0; i < 90; i++)
        {
            var name = PharmacyNames[i % PharmacyNames.Length] + (i >= PharmacyNames.Length ? $" #{i}" : "");
            var gov = Governorates[Rng.Next(Governorates.Length)];
            var wh = warehouses[Rng.Next(warehouses.Count)];
            var isDemo = name == "Alexandria Drug Store";
            list.Add(new Pharmacy
            {
                PharmacyCode = isDemo ? "PH-2024-001" : $"PH-2024-{(i + 1):000}",
                OfficialPharmacyName = name,
                PharmacyType = "Retail Pharmacy",
                Governorate = isDemo ? "Alexandria" : gov,
                City = isDemo ? "Alexandria" : gov,
                DistrictArea = isDemo ? "Smouha" : $"{Rng.Next(1, 30)} Smouha St.",
                FullAddress = isDemo ? "12 Smouha St., Alexandria, Egypt" : $"{Rng.Next(1, 30)} Smouha St., {gov}, Egypt",
                Phone = isDemo ? "+20 12 3456 7890" : $"+20 1{Rng.Next(0, 2)} {Rng.Next(1000, 9999)} {Rng.Next(1000, 9999)}",
                Email = isDemo ? "pharmacy@alexdrugstore.com" : $"pharmacy@{name.Replace(" ", "").ToLower()}.com",
                DefaultWarehouseId = wh.Id == 0 ? null : wh.Id,
                DefaultWarehouse = wh,
                HasColdStorage = isDemo ? true : Rng.Next(0, 10) > 4,
                PharmacyLicenseNumber = $"PH-2024-{(i + 1):000}",
                LicenseIssueDate = RandDate(200, 900),
                LicenseExpiryDate = DateTime.UtcNow.AddDays(Rng.Next(-60, 900)),
                PharmacistSyndicateId = $"PS-{Rng.Next(100000, 999999)}",
                PharmacyStatus = isDemo ? FacilityStatus.Active : statuses[Rng.Next(statuses.Length)],
                CreatedAt = RandDate(100, 500),
                UpdatedAt = RandDate(0, 90)
            });
        }
        return list;
    }

    private static List<MedicineProduct> SeedProducts()
    {
        var list = new List<MedicineProduct>();
        foreach (var p in ProductNames)
        {
            list.Add(new MedicineProduct
            {
                ProductName = p,
                GTIN = $"0622210{Rng.Next(100000, 999999)}",
                DosageForm = DosageForms[Rng.Next(DosageForms.Length)],
                Strength = $"{Rng.Next(1, 500)} mg",
                RequiresColdChain = Rng.Next(0, 10) > 7,
                ProductStatus = "Active"
            });
        }
        return list;
    }

    private static List<Batch> SeedBatches(List<MedicineProduct> products, List<Factory> factories, List<SystemUser> users)
    {
        var list = new List<Batch>();
        var batchStatuses = new[] { BatchStatus.InSupplyChain, BatchStatus.InWarehouse, BatchStatus.InPharmacy, BatchStatus.Quarantined, BatchStatus.Recalled, BatchStatus.Available, BatchStatus.InProduction };
        var stages = new[] { SupplyChainStage.AtFactory, SupplyChainStage.InTransit, SupplyChainStage.Stored, SupplyChainStage.Available, SupplyChainStage.Quarantined, SupplyChainStage.Recalled };
        var locations = new[] { "Cairo Medical Storage", "Port Said Warehouse", "Alexandria Drug Store", "Assiut Central Warehouse", "Delta Warehouse", "Alex Warehouse", "Mansoura Pharmacy" };

        for (int i = 0; i < 150; i++)
        {
            var product = products[Rng.Next(products.Count)];
            var factory = factories[Rng.Next(factories.Count)];
            var status = batchStatuses[Rng.Next(batchStatuses.Length)];
            var qty = Rng.Next(1, 20) * 5000;
            var totalUnits = qty;
            var inWarehouse = Rng.Next(0, totalUnits / 2);
            var inPharmacy = Rng.Next(0, Math.Max(1, totalUnits - inWarehouse));
            list.Add(new Batch
            {
                BatchNumber = $"BAT-2024-{(i + 1):000}",
                MedicineProductId = product.Id == 0 ? null : product.Id,
                MedicineProduct = product,
                FactoryId = factory.Id == 0 ? null : factory.Id,
                Factory = factory,
                Quantity = qty,
                ManufacturingDate = RandDate(200, 600),
                ExpiryDate = DateTime.UtcNow.AddDays(Rng.Next(-30, 700)),
                BatchStatus = status,
                SupplyChainStage = stages[Rng.Next(stages.Length)],
                CurrentLocation = locations[Rng.Next(locations.Length)],
                CreatedBy = users[Rng.Next(users.Count)].Email,
                CreatedByUserId = users[Rng.Next(users.Count)].Id == 0 ? null : users[Rng.Next(users.Count)].Id,
                Notes = Rng.Next(0, 10) > 6 ? "Standard production run, no special handling notes." : null,
                CreatedAt = RandDate(30, 400),
                UpdatedAt = RandDate(0, 30),
                TotalUnitCodes = totalUnits,
                GeneratedUnitCodes = totalUnits,
                InWarehouseUnitCodes = inWarehouse,
                InPharmacyUnitCodes = inPharmacy,
                SuspiciousUnitCodes = Rng.Next(0, 15),
                BlockedUnitCodes = status == BatchStatus.Quarantined ? Rng.Next(50, 400) : Rng.Next(0, 20),
                RecalledUnitCodes = status == BatchStatus.Recalled ? totalUnits : 0,
                ScanCountTotal = Rng.Next(0, totalUnits),
                OpenAlertsCount = Rng.Next(0, 3)
            });
        }
        return list;
    }

    private static List<UnitCode> SeedUnitCodes(List<Batch> batches)
    {
        var list = new List<UnitCode>();
        var holderTypes = new[] { "Factory", "Warehouse", "Pharmacy" };
        var unitStatuses = new[] { UnitStatus.InWarehouse, UnitStatus.InPharmacy, UnitStatus.Generated, UnitStatus.Blocked, UnitStatus.Suspicious, UnitStatus.Recalled };
        foreach (var batch in batches.Where((_, idx) => idx % 2 == 0).Take(80))
        {
            for (int i = 0; i < 3; i++)
            {
                list.Add(new UnitCode
                {
                    UnitCodeValue = $"{batch.BatchNumber}-U{i:0000}",
                    SerialNumber = $"SN-{Rng.Next(100000000, 999999999)}",
                    GTIN = batch.MedicineProduct?.GTIN,
                    CodeValueHash = Convert.ToHexString(Guid.NewGuid().ToByteArray()).ToLower(),
                    ExpiryDate = batch.ExpiryDate,
                    BatchId = batch.Id == 0 ? null : batch.Id,
                    Batch = batch,
                    UnitStatus = unitStatuses[Rng.Next(unitStatuses.Length)],
                    CurrentHolderType = holderTypes[Rng.Next(holderTypes.Length)],
                    CurrentHolderName = "Cairo Medical Storage",
                    ScanCount = Rng.Next(0, 6),
                    FirstScannedAt = Rng.Next(0, 10) > 3 ? RandDate(0, 60) : null,
                    CreatedAt = RandDate(30, 300)
                });
            }
        }
        return list;
    }

    private static List<Shipment> SeedShipments(List<Batch> batches, List<Factory> factories, List<Warehouse> warehouses, List<Pharmacy> pharmacies)
    {
        var list = new List<Shipment>();
        var types = new[] { ShipmentType.FactoryToWarehouse, ShipmentType.WarehouseToPharmacy, ShipmentType.WarehouseToWarehouse };
        var statuses = new[] { ShipmentStatus.InTransit, ShipmentStatus.Received, ShipmentStatus.PartiallyReceived, ShipmentStatus.Rejected, ShipmentStatus.Cancelled, ShipmentStatus.PendingInspection };
        for (int i = 0; i < 120; i++)
        {
            var batch = batches[Rng.Next(batches.Count)];
            var type = types[Rng.Next(types.Length)];
            var status = statuses[Rng.Next(statuses.Length)];
            var expected = Rng.Next(1, 10) * 3000;
            var factory = factories[Rng.Next(factories.Count)];
            var srcWarehouse = warehouses[Rng.Next(warehouses.Count)];
            var destWarehouse = warehouses[Rng.Next(warehouses.Count)];
            var pharmacy = pharmacies[Rng.Next(pharmacies.Count)];
            var received = status is ShipmentStatus.Received ? expected
                : status == ShipmentStatus.PartiallyReceived ? Rng.Next(1, expected)
                : status == ShipmentStatus.Rejected ? 0
                : (long?)null;

            list.Add(new Shipment
            {
                TransferCode = $"TRF-2034-{(1100 + i)}",
                BatchId = batch.Id == 0 ? null : batch.Id,
                Batch = batch,
                ShipmentType = type,
                Source = type == ShipmentType.FactoryToWarehouse ? factory.OfficialFactoryName : srcWarehouse.OfficialWarehouseName,
                Destination = type == ShipmentType.WarehouseToPharmacy ? pharmacy.OfficialPharmacyName : destWarehouse.OfficialWarehouseName,
                SourceFactoryId = type == ShipmentType.FactoryToWarehouse ? factory.Id : null,
                SourceWarehouseId = type != ShipmentType.FactoryToWarehouse ? srcWarehouse.Id : null,
                DestinationWarehouseId = type != ShipmentType.WarehouseToPharmacy ? destWarehouse.Id : null,
                DestinationPharmacyId = type == ShipmentType.WarehouseToPharmacy ? pharmacy.Id : null,
                ExpectedQuantity = expected,
                ReceivedQuantity = received,
                ShipmentStatus = status,
                RequiresColdChain = batch.MedicineProduct?.RequiresColdChain ?? false,
                InspectionResult = status switch
                {
                    ShipmentStatus.Received => "Accepted",
                    ShipmentStatus.PartiallyReceived => "PartiallyAccepted",
                    ShipmentStatus.Rejected => "Rejected",
                    _ => null
                },
                DispatchDate = RandDate(0, 60),
                ReceivedDate = status is ShipmentStatus.Received or ShipmentStatus.PartiallyReceived or ShipmentStatus.Rejected ? RandDate(0, 30) : null
            });
        }
        return list;
    }

    private static List<InventoryStock> SeedInventory(List<Batch> batches, List<Warehouse> warehouses, List<Pharmacy> pharmacies)
    {
        var list = new List<InventoryStock>();
        var statuses = new[] { InventoryStatus.Active, InventoryStatus.Quarantined, InventoryStatus.Recalled, InventoryStatus.Blocked };
        for (int i = 0; i < 100; i++)
        {
            var batch = batches[Rng.Next(batches.Count)];
            var isWarehouse = Rng.Next(0, 2) == 0;
            var total = Rng.Next(5, 60) * 1000;
            var available = Rng.Next(0, total);
            var wh = warehouses[Rng.Next(warehouses.Count)];
            var ph = pharmacies[Rng.Next(pharmacies.Count)];
            list.Add(new InventoryStock
            {
                BatchId = batch.Id == 0 ? null : batch.Id,
                Batch = batch,
                HolderType = isWarehouse ? "Warehouse" : "Pharmacy",
                HolderName = isWarehouse ? wh.OfficialWarehouseName : ph.OfficialPharmacyName,
                WarehouseId = isWarehouse ? (wh.Id == 0 ? null : wh.Id) : null,
                PharmacyId = !isWarehouse ? (ph.Id == 0 ? null : ph.Id) : null,
                TotalReceivedQuantity = total,
                AvailableQuantity = available,
                ReservedQuantity = Rng.Next(0, Math.Max(1, total - available)),
                QuarantinedQuantity = Rng.Next(0, 1000),
                InventoryStatus = statuses[Rng.Next(statuses.Length)],
                LastUpdated = RandDate(0, 30)
            });
        }
        return list;
    }

    private static List<Alert> SeedAlerts(List<Batch> batches, List<Factory> factories, List<Warehouse> warehouses, List<Pharmacy> pharmacies)
    {
        var list = new List<Alert>();
        var types = new[] { AlertType.ColdChainIssue, AlertType.QuantityMismatch, AlertType.SuspiciousScan, AlertType.LicenseExpiry, AlertType.BlockedUnitScan, AlertType.ComplianceIssue, AlertType.Recall, AlertType.DuplicateSerial, AlertType.DamagedPackage };
        var severities = new[] { AlertSeverity.Low, AlertSeverity.Medium, AlertSeverity.High, AlertSeverity.Critical };
        var statuses = new[] { AlertStatus.Open, AlertStatus.UnderReview, AlertStatus.Resolved, AlertStatus.Dismissed };
        var entityKinds = new[] { EntityKind.Factory, EntityKind.Warehouse, EntityKind.Pharmacy };
        var messages = new[]
        {
            "Temperature excursion detected during transit.",
            "Received quantity does not match expected amount.",
            "Multiple scans from different governorates.",
            "Factory operating license will expire in 15 days.",
            "Blocked unit code scanned by public.",
            "Batch has been recalled by Ministry.",
            "Product quality issue - recall initiated.",
            "Safety issue reported - recall.",
            "Damaged boxes reported at warehouse."
        };
        for (int i = 0; i < 45; i++)
        {
            var batch = batches[Rng.Next(batches.Count)];
            var kind = entityKinds[Rng.Next(entityKinds.Length)];
            var entityName = kind switch
            {
                EntityKind.Factory => factories[Rng.Next(factories.Count)].OfficialFactoryName,
                EntityKind.Warehouse => warehouses[Rng.Next(warehouses.Count)].OfficialWarehouseName,
                _ => pharmacies[Rng.Next(pharmacies.Count)].OfficialPharmacyName
            };
            var status = statuses[Rng.Next(statuses.Length)];
            list.Add(new Alert
            {
                AlertCode = $"ALERT-2024-{(91 - i):0000}",
                AlertType = types[Rng.Next(types.Length)],
                Severity = severities[Rng.Next(severities.Length)],
                EntityType = kind,
                EntityName = entityName,
                BatchId = batch.Id == 0 ? null : batch.Id,
                Batch = batch,
                Message = messages[Rng.Next(messages.Length)],
                AlertStatus = status,
                CreatedAt = RandDate(0, 60),
                ResolvedAt = status is AlertStatus.Resolved or AlertStatus.Dismissed ? RandDate(0, 20) : null
            });
        }
        return list;
    }

    private static List<PublicVerificationScan> SeedScans(List<UnitCode> unitCodes, List<MedicineProduct> products, List<Batch> batches)
    {
        var list = new List<PublicVerificationScan>();
        var results = new[] { VerificationResult.Authentic, VerificationResult.NotFound, VerificationResult.DuplicateScan, VerificationResult.Recalled, VerificationResult.Expired, VerificationResult.Blocked, VerificationResult.Suspicious };
        var reasons = new[] { "Valid Product", "Serial number not registered in the system.", "Scanned multiple times in short time.", "Batch is recalled.", "Product expired.", "Blocked unit code.", "Multiple locations in short time." };
        for (int i = 0; i < 150; i++)
        {
            var hasUnit = Rng.Next(0, 10) > 2 && unitCodes.Count > 0;
            var unit = hasUnit ? unitCodes[Rng.Next(unitCodes.Count)] : null;
            var product = products[Rng.Next(products.Count)];
            var batch = batches[Rng.Next(batches.Count)];
            var gov = Governorates[Rng.Next(Governorates.Length)];
            list.Add(new PublicVerificationScan
            {
                ScanCode = $"SCAN-2024-{(15021 - i)}",
                ScannedGTIN = product.GTIN,
                ScannedSerialNumber = unit?.SerialNumber ?? "SN-UNKNOWN-001",
                ScannedBatchNumber = batch.BatchNumber,
                UnitCodeId = unit?.Id,
                UnitCode = unit,
                ProductName = hasUnit ? product.ProductName : null,
                VerificationResult = results[Rng.Next(results.Length)],
                Reason = reasons[Rng.Next(reasons.Length)],
                Governorate = gov,
                City = gov,
                ScannedAt = RandDate(0, 45)
            });
        }
        return list;
    }

    private static List<RegistrationRequest> SeedRegistrationRequests(List<SystemUser> users)
    {
        var list = new List<RegistrationRequest>();
        var statuses = new[] { RegistrationStatus.Pending, RegistrationStatus.UnderReview, RegistrationStatus.NeedsMoreDocuments, RegistrationStatus.Approved, RegistrationStatus.Rejected, RegistrationStatus.Cancelled };
        var kinds = new[] { EntityKind.Factory, EntityKind.Warehouse, EntityKind.Pharmacy };
        var docTypes = new[] { "Factory License Copy", "Commercial Registration", "Tax Card", "Technical License", "Authorization Letter", "Syndicate Card Copy" };
        var docStatuses = new[] { DocumentStatus.UnderReview, DocumentStatus.Complete, DocumentStatus.NeedsReplacement, DocumentStatus.Rejected };

        var entityNamesByKind = new Dictionary<EntityKind, string[]>
        {
            [EntityKind.Factory] = FactoryCompanyNames,
            [EntityKind.Warehouse] = WarehouseNames,
            [EntityKind.Pharmacy] = PharmacyNames
        };

        for (int i = 0; i < 60; i++)
        {
            var kind = kinds[Rng.Next(kinds.Length)];
            var name = entityNamesByKind[kind][Rng.Next(entityNamesByKind[kind].Length)];
            var rep = users[Rng.Next(users.Count)];
            var status = statuses[Rng.Next(statuses.Length)];
            var req = new RegistrationRequest
            {
                RequestCode = $"REQ-{(60 - i):0000}",
                EntityType = kind,
                EntityName = name,
                RepresentativeName = rep.FullName,
                Email = rep.Email,
                SubmittedAt = RandDate(0, 60),
                EmailConfirmed = Rng.Next(0, 10) > 2,
                DocumentsOverallStatus = docStatuses[Rng.Next(docStatuses.Length)],
                RegistrationStatus = status,
                AdminNotes = status == RegistrationStatus.NeedsMoreDocuments ? "Please provide an updated Authorization Letter on company letterhead." : null,
                RejectionReason = status == RegistrationStatus.Rejected ? "Documents did not meet regulatory requirements." : null,
                SystemUserId = rep.Id == 0 ? null : rep.Id,
                SystemUser = rep,
                Documents = new List<EntityDocument>()
            };
            var docCount = Rng.Next(3, 7);
            for (int d = 0; d < docCount; d++)
            {
                req.Documents.Add(new EntityDocument
                {
                    DocumentType = docTypes[d % docTypes.Length],
                    FileName = $"{docTypes[d % docTypes.Length].Replace(" ", "_").ToLower()}.pdf",
                    FileUrl = $"/files/documents/{Guid.NewGuid()}.pdf",
                    UploadedAt = RandDate(0, 55),
                    DocumentStatus = docStatuses[Rng.Next(docStatuses.Length)],
                    ReviewedBy = Rng.Next(0, 10) > 4 ? "Dr. Saif" : null,
                    ReviewedAt = Rng.Next(0, 10) > 4 ? RandDate(0, 40) : null,
                    RejectionReason = Rng.Next(0, 10) > 8 ? "Document unclear, please re-upload." : null
                });
            }
            list.Add(req);
        }
        return list;
    }

    private static List<EntityLicense> SeedLicenses(List<Factory> factories)
    {
        var list = new List<EntityLicense>();
        var genericTypes = new[] { "Manufacturing License", "GMP Certificate", "Environmental License", "Fire Safety License" };

        foreach (var factory in factories)
        {
            var isDemo = factory.OfficialFactoryName == "EIPICO Factory";
            if (isDemo)
            {
                list.Add(new EntityLicense { EntityType = EntityKind.Factory, EntityId = factory.Id, LicenseType = "Manufacturing License", LicenseNumber = "LIC-MFG-2024-001", IssueDate = new DateTime(2024, 5, 1), ExpiryDate = new DateTime(2026, 4, 30), Status = "Active" });
                list.Add(new EntityLicense { EntityType = EntityKind.Factory, EntityId = factory.Id, LicenseType = "GMP Certificate", LicenseNumber = "GMP-2024-022", IssueDate = new DateTime(2024, 4, 15), ExpiryDate = new DateTime(2026, 4, 14), Status = "Active" });
                list.Add(new EntityLicense { EntityType = EntityKind.Factory, EntityId = factory.Id, LicenseType = "Environmental License", LicenseNumber = "ENV-2024-015", IssueDate = new DateTime(2024, 3, 10), ExpiryDate = new DateTime(2026, 3, 9), Status = "Active" });
                list.Add(new EntityLicense { EntityType = EntityKind.Factory, EntityId = factory.Id, LicenseType = "Fire Safety License", LicenseNumber = "FIRE-2024-008", IssueDate = new DateTime(2024, 2, 20), ExpiryDate = new DateTime(2026, 2, 19), Status = "Active" });
                continue;
            }

            var count = Rng.Next(2, 5);
            foreach (var type in genericTypes.OrderBy(_ => Rng.Next()).Take(count))
            {
                var issue = RandDate(200, 600);
                list.Add(new EntityLicense
                {
                    EntityType = EntityKind.Factory,
                    EntityId = factory.Id,
                    LicenseType = type,
                    LicenseNumber = $"{type.Split(' ')[0].ToUpper().Substring(0, 3)}-{Rng.Next(2023, 2025)}-{Rng.Next(1, 999):000}",
                    IssueDate = issue,
                    ExpiryDate = issue.AddYears(2),
                    Status = Rng.Next(0, 10) > 1 ? "Active" : "Expired"
                });
            }
        }
        return list;
    }

    // Guarantees the fixed demo accounts (EIPICO Factory / Cairo Medical Storage / Alexandria Drug
    // Store) always have a rich, realistic set of batches/shipments/alerts to show - instead of
    // relying on chance from the general random seeding above. Runs once, after everything else,
    // using the real DB-assigned IDs.
    private static void SeedDemoOperationalData(AppDbContext db)
    {
        var factory = db.Factories.FirstOrDefault(f => f.OfficialFactoryName == "EIPICO Factory");
        var warehouse = db.Warehouses.FirstOrDefault(w => w.OfficialWarehouseName == "Cairo Medical Storage");
        var altWarehouse1 = db.Warehouses.FirstOrDefault(w => w.OfficialWarehouseName == "Delta Storage Warehouse");
        var altWarehouse2 = db.Warehouses.FirstOrDefault(w => w.OfficialWarehouseName == "Alex Warehouse");
        var pharmacy = db.Pharmacies.FirstOrDefault(p => p.OfficialPharmacyName == "Alexandria Drug Store");
        var altPharmacy = db.Pharmacies.FirstOrDefault(p => p.OfficialPharmacyName == "Mansoura Pharmacy");
        if (factory == null || warehouse == null) return;

        var demoProducts = new (string Name, string Gtin, string Dosage, string Strength, bool Cold)[]
        {
            ("Paracetamol 500mg", "06222100123456", "Tablet", "500mg", false),
            ("Amoxicillin 500mg", "06222100987654", "Capsule", "500mg", false),
            ("Ibuprofen 400mg", "06222100555544", "Tablet", "400mg", false),
            ("Ciprofloxacin 500mg", "06222100777711", "Tablet", "500mg", true),
            ("Metronidazole 500mg", "06222100333322", "Tablet", "500mg", false),
            ("Diclofenac 50mg", "06222100444455", "Tablet", "50mg", false),
            ("Azithromycin 500mg", "06222100666677", "Tablet", "500mg", true),
            ("Clarithromycin 250mg", "06222100888844", "Tablet", "250mg", false),
            ("Ofloxacin 200mg", "06222100111222", "Tablet", "200mg", false)
        };

        var products = new List<MedicineProduct>();
        foreach (var (name, gtin, dosage, strength, cold) in demoProducts)
        {
            var product = db.MedicineProducts.FirstOrDefault(p => p.GTIN == gtin);
            if (product == null)
            {
                product = new MedicineProduct { ProductName = name, GTIN = gtin, DosageForm = dosage, Strength = strength, RequiresColdChain = cold, ProductStatus = "Active" };
                db.MedicineProducts.Add(product);
                db.SaveChanges();
            }
            products.Add(product);
        }

        var batchPlans = new (string Number, BatchStatus Status, SupplyChainStage Stage, long Qty)[]
        {
            ("BAT-2024-001", BatchStatus.Registered, SupplyChainStage.AtFactory, 100000),
            ("BAT-2024-002", BatchStatus.ReadyForWarehouseDispatch, SupplyChainStage.AtFactory, 80000),
            ("BAT-2024-003", BatchStatus.Draft, SupplyChainStage.AtFactory, 60000),
            ("BAT-2024-004", BatchStatus.ReadyForWarehouseDispatch, SupplyChainStage.AtFactory, 120000),
            ("BAT-2024-005", BatchStatus.Registered, SupplyChainStage.AtFactory, 90000),
            ("BAT-2024-006", BatchStatus.Draft, SupplyChainStage.AtFactory, 50000),
            ("BAT-2024-007", BatchStatus.Quarantined, SupplyChainStage.Quarantined, 70000),
            ("BAT-2024-008", BatchStatus.Recalled, SupplyChainStage.Recalled, 40000),
            ("BAT-2024-009", BatchStatus.Expired, SupplyChainStage.AtFactory, 30000)
        };

        var batches = new List<Batch>();
        for (int i = 0; i < batchPlans.Length; i++)
        {
            var (number, status, stage, qty) = batchPlans[i];
            var existing = db.Batches.FirstOrDefault(b => b.BatchNumber == number && b.FactoryId == factory.Id);
            if (existing != null) { batches.Add(existing); continue; }

            var codesGenerated = status != BatchStatus.Draft;
            var batch = new Batch
            {
                MedicineProductId = products[i].Id,
                MedicineProduct = products[i],
                FactoryId = factory.Id,
                Factory = factory,
                BatchNumber = number,
                Quantity = qty,
                ManufacturingDate = RandDate(60, 200),
                ExpiryDate = status == BatchStatus.Expired ? DateTime.UtcNow.AddDays(-30) : DateTime.UtcNow.AddDays(Rng.Next(200, 700)),
                BatchStatus = status,
                SupplyChainStage = stage,
                CurrentLocation = factory.OfficialFactoryName,
                CreatedBy = "ahmed.ali@eipico.com",
                CreatedAt = RandDate(30, 90),
                UpdatedAt = RandDate(0, 20),
                TotalUnitCodes = codesGenerated ? qty : 0,
                GeneratedUnitCodes = codesGenerated ? qty : 0,
                InWarehouseUnitCodes = 0,
                InPharmacyUnitCodes = 0,
                SuspiciousUnitCodes = 0,
                BlockedUnitCodes = status == BatchStatus.Quarantined ? qty : 0,
                RecalledUnitCodes = status == BatchStatus.Recalled ? qty : 0,
                ScanCountTotal = 0,
                OpenAlertsCount = status is BatchStatus.Quarantined or BatchStatus.Recalled ? 1 : 0
            };
            db.Batches.Add(batch);
            db.SaveChanges();
            batches.Add(batch);
        }

        // Shipments: factory -> warehouses (spread across Cairo Medical Storage + two alternates)
        var shipmentPlans = new (int BatchIdx, Warehouse? Dest, ShipmentStatus Status, long Expected, long? Received)[]
        {
            (1, warehouse, ShipmentStatus.InTransit, 80000, null),
            (0, altWarehouse1, ShipmentStatus.Received, 100000, 100000),
            (2, altWarehouse2, ShipmentStatus.PartiallyReceived, 60000, 30000),
            (3, altWarehouse1, ShipmentStatus.InTransit, 120000, null),
            (4, warehouse, ShipmentStatus.Received, 90000, 90000),
            (5, altWarehouse1, ShipmentStatus.Rejected, 50000, 0),
            (6, altWarehouse2, ShipmentStatus.Received, 70000, 70000),
            (7, altWarehouse1, ShipmentStatus.PartiallyReceived, 40000, 20000),
            (8, warehouse, ShipmentStatus.Cancelled, 30000, null)
        };

        var demoShipments = new List<Shipment>();
        for (int i = 0; i < shipmentPlans.Length; i++)
        {
            var (batchIdx, dest, status, expected, received) = shipmentPlans[i];
            if (dest == null) continue;
            var code = $"TRF-2024-0{170 + i}";
            if (db.Shipments.Any(s => s.TransferCode == code)) continue;

            var batch = batches[batchIdx];
            var shipment = new Shipment
            {
                TransferCode = code,
                BatchId = batch.Id,
                Batch = batch,
                ShipmentType = ShipmentType.FactoryToWarehouse,
                Source = factory.OfficialFactoryName,
                Destination = dest.OfficialWarehouseName,
                SourceFactoryId = factory.Id,
                DestinationWarehouseId = dest.Id,
                ExpectedQuantity = expected,
                ReceivedQuantity = received,
                ShipmentStatus = status,
                RequiresColdChain = batch.MedicineProduct?.RequiresColdChain ?? false,
                DispatchDate = RandDate(0, 20),
                ReceivedDate = status is ShipmentStatus.Received or ShipmentStatus.PartiallyReceived ? RandDate(0, 10) : null
            };
            db.Shipments.Add(shipment);
            demoShipments.Add(shipment);

            if (status is ShipmentStatus.Received or ShipmentStatus.PartiallyReceived && received.HasValue)
            {
                db.InventoryStocks.Add(new InventoryStock
                {
                    BatchId = batch.Id,
                    HolderType = "Warehouse",
                    HolderName = dest.OfficialWarehouseName,
                    WarehouseId = dest.Id,
                    TotalReceivedQuantity = received.Value,
                    AvailableQuantity = received.Value,
                    ReservedQuantity = 0,
                    QuarantinedQuantity = 0,
                    InventoryStatus = InventoryStatus.Active,
                    LastUpdated = RandDate(0, 10)
                });
            }
        }
        db.SaveChanges();

        // A couple of warehouse -> pharmacy shipments so the "Outgoing to Pharmacy" tab has data too.
        if (pharmacy != null)
        {
            var wh1Stock = db.InventoryStocks.FirstOrDefault(i => i.WarehouseId == warehouse.Id);
            if (wh1Stock != null && !db.Shipments.Any(s => s.TransferCode == "DSP-2024-0088"))
            {
                db.Shipments.Add(new Shipment
                {
                    TransferCode = "DSP-2024-0088",
                    BatchId = wh1Stock.BatchId,
                    ShipmentType = ShipmentType.WarehouseToPharmacy,
                    Source = warehouse.OfficialWarehouseName,
                    Destination = pharmacy.OfficialPharmacyName,
                    SourceWarehouseId = warehouse.Id,
                    DestinationPharmacyId = pharmacy.Id,
                    ExpectedQuantity = 20000,
                    ReceivedQuantity = 20000,
                    ShipmentStatus = ShipmentStatus.Received,
                    RequiresColdChain = false,
                    DispatchDate = RandDate(0, 15),
                    ReceivedDate = RandDate(0, 10)
                });
                db.InventoryStocks.Add(new InventoryStock
                {
                    BatchId = wh1Stock.BatchId,
                    HolderType = "Pharmacy",
                    HolderName = pharmacy.OfficialPharmacyName,
                    PharmacyId = pharmacy.Id,
                    TotalReceivedQuantity = 20000,
                    AvailableQuantity = 20000,
                    ReservedQuantity = 0,
                    QuarantinedQuantity = 0,
                    InventoryStatus = InventoryStatus.Active,
                    LastUpdated = RandDate(0, 10)
                });
            }
            if (altPharmacy != null && wh1Stock != null && !db.Shipments.Any(s => s.TransferCode == "DSP-2024-0091"))
            {
                db.Shipments.Add(new Shipment
                {
                    TransferCode = "DSP-2024-0091",
                    BatchId = wh1Stock.BatchId,
                    ShipmentType = ShipmentType.WarehouseToPharmacy,
                    Source = warehouse.OfficialWarehouseName,
                    Destination = altPharmacy.OfficialPharmacyName,
                    SourceWarehouseId = warehouse.Id,
                    DestinationPharmacyId = altPharmacy.Id,
                    ExpectedQuantity = 15000,
                    ReceivedQuantity = null,
                    ShipmentStatus = ShipmentStatus.InTransit,
                    RequiresColdChain = false,
                    DispatchDate = RandDate(0, 5)
                });
            }
            db.SaveChanges();
        }

        // Alerts tied specifically to the demo factory / warehouse so the Alerts pages aren't empty.
        if (!db.Alerts.Any(a => a.EntityName == factory.OfficialFactoryName && a.AlertCode == "ALERT-2024-0091"))
        {
            db.Alerts.AddRange(
                new Alert { AlertCode = "ALERT-2024-0091", AlertType = AlertType.Recall, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[1].Id, ShipmentId = demoShipments.ElementAtOrDefault(0)?.Id, Message = "This batch has been recalled by Ministry due to quality issue.", AlertStatus = AlertStatus.Open, CreatedAt = RandDate(0, 5) },
                new Alert { AlertCode = "ALERT-2024-0090", AlertType = AlertType.ColdChainIssue, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[3].Id, ShipmentId = demoShipments.ElementAtOrDefault(3)?.Id, Message = "Temperature excursion detected during transit.", AlertStatus = AlertStatus.Open, CreatedAt = RandDate(0, 6) },
                new Alert { AlertCode = "ALERT-2024-0089", AlertType = AlertType.QuantityMismatch, Severity = AlertSeverity.Medium, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[2].Id, ShipmentId = demoShipments.ElementAtOrDefault(2)?.Id, Message = "Received quantity does not match expected quantity.", AlertStatus = AlertStatus.UnderReview, CreatedAt = RandDate(0, 7) },
                new Alert { AlertCode = "ALERT-2024-0088", AlertType = AlertType.ComplianceIssue, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[0].Id, ShipmentId = demoShipments.ElementAtOrDefault(1)?.Id, Message = "GMP compliance document is missing.", AlertStatus = AlertStatus.Open, CreatedAt = RandDate(0, 8) },
                new Alert { AlertCode = "ALERT-2024-0087", AlertType = AlertType.LicenseExpiry, Severity = AlertSeverity.Low, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, Message = "Factory license will expire in 12 days.", AlertStatus = AlertStatus.Open, CreatedAt = RandDate(0, 9) },
                new Alert { AlertCode = "ALERT-2024-0086", AlertType = AlertType.ExpiredBatch, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[4].Id, Message = "Batch has expired.", AlertStatus = AlertStatus.Resolved, CreatedAt = RandDate(10, 15), ResolvedAt = RandDate(0, 9) },
                new Alert { AlertCode = "ALERT-2024-0085", AlertType = AlertType.DocumentMissing, Severity = AlertSeverity.Medium, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[5].Id, Message = "COA document is missing for this batch.", AlertStatus = AlertStatus.UnderReview, CreatedAt = RandDate(0, 10) },
                new Alert { AlertCode = "ALERT-2024-0084", AlertType = AlertType.Recall, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[6].Id, Message = "Batch recalled due to contamination risk.", AlertStatus = AlertStatus.Open, CreatedAt = RandDate(0, 11) },
                new Alert { AlertCode = "ALERT-2024-0083", AlertType = AlertType.ComplianceIssue, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[8].Id, Message = "Stability study report is missing.", AlertStatus = AlertStatus.Dismissed, CreatedAt = RandDate(12, 18), ResolvedAt = RandDate(0, 11) }
            );
            db.SaveChanges();
        }
    }

    private static List<AuditLog> SeedAuditLogs(List<SystemUser> users)
    {
        var list = new List<AuditLog>();
        var actions = Enum.GetValues<AuditAction>();
        var resourceTypes = new[] { "Batch", "Factory", "Warehouse", "Pharmacy", "EntityDocument", "RegistrationRequest", "SystemUser", "Alert" };
        var oldVals = new[] { "Active", "Pending", "Under Review", "In Supply Chain" };
        var newVals = new[] { "Suspended", "Approved", "Quarantined", "Recalled", "Rejected" };
        var results = new[]
        {
            AuditResult.Success, AuditResult.Success, AuditResult.Success, AuditResult.Success,
            AuditResult.Success, AuditResult.Success, AuditResult.Success, AuditResult.Success,
            AuditResult.Warning, AuditResult.Failed
        };
        for (int i = 0; i < 120; i++)
        {
            var user = users[Rng.Next(users.Count)];
            list.Add(new AuditLog
            {
                LogCode = $"LOG-2024-{(55678 - i)}",
                UserId = user.Id == 0 ? null : user.Id,
                User = user,
                UserDisplayName = user.FullName,
                Role = user.Role,
                Action = (AuditAction)actions.GetValue(Rng.Next(actions.Length))!,
                ResourceType = resourceTypes[Rng.Next(resourceTypes.Length)],
                ResourceId = $"{resourceTypes[Rng.Next(resourceTypes.Length)].ToUpper().Substring(0, 3)}-2024-{Rng.Next(1, 999):000}",
                OldValue = oldVals[Rng.Next(oldVals.Length)],
                NewValue = newVals[Rng.Next(newVals.Length)],
                Result = results[Rng.Next(results.Length)],
                IpAddress = $"{Rng.Next(41, 197)}.{Rng.Next(1, 255)}.{Rng.Next(1, 255)}.{Rng.Next(1, 255)}",
                CreatedAt = RandDate(0, 60)
            });
        }
        return list;
    }
}
```
