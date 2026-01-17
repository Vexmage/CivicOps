using CivicOps.Api.WorkItems;
using CivicOps.Contracts.Auth;
using CivicOps.Contracts.Users;
using CivicOps.Contracts.WorkItems;
using CivicOps.Domain.WorkItems;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

// Use raw JWT claim names (sub, email, role, etc.)
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

// JSON config (enum as string)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        var signingKey = jwt["SigningKey"];

        if (string.IsNullOrWhiteSpace(signingKey))
            throw new InvalidOperationException("Missing Jwt:SigningKey");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwt["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),

            // Explicit identity semantics
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role",
        };
    });

builder.Services.AddAuthorization();

// Work Items (in-memory repo for MVP)
builder.Services.AddSingleton<IWorkItemRepository, InMemoryWorkItemRepository>();

var app = builder.Build();

// Avoid HTTPS redirect warning during local HTTP dev
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");

// --------------------------------------------------
// DEV endpoints (Development only)
// --------------------------------------------------
if (app.Environment.IsDevelopment())
{
    // Issue a dev JWT
    api.MapPost("/dev/token", (IConfiguration config) =>
    {
        var jwt = config.GetSection("Jwt");

        var issuer = jwt["Issuer"] ?? "CivicOps";
        var audience = jwt["Audience"] ?? "CivicOps";
        var signingKey = jwt["SigningKey"] ?? throw new InvalidOperationException("Missing Jwt:SigningKey");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "11111111-1111-1111-1111-111111111111"),
            new(JwtRegisteredClaimNames.Email, "dev@civicops.local"),
            new("displayName", "Dev User"),
            new("role", "Admin"),
            new("isActive", "true"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Results.Ok(new { accessToken = tokenString });
    });

    // Inspect claims after auth (debugging)
    api.MapGet("/dev/claims", (ClaimsPrincipal user) =>
    {
        var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Results.Ok(claims);
    })
    .RequireAuthorization();
}

// --------------------------------------------------
// Protected endpoints
// --------------------------------------------------
api.MapGet("/me", (ClaimsPrincipal user) =>
{
    var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? "00000000-0000-0000-0000-000000000000";

    var email = user.FindFirstValue(JwtRegisteredClaimNames.Email) ?? "";
    var displayName = user.FindFirstValue("displayName") ?? "";
    var roleText = user.FindFirstValue("role") ?? "Staff";
    var isActiveText = user.FindFirstValue("isActive") ?? "true";

    _ = Guid.TryParse(sub, out var id);
    _ = Enum.TryParse<Role>(roleText, ignoreCase: true, out var role);
    _ = bool.TryParse(isActiveText, out var isActive);

    return Results.Ok(new MeResponse(
        Id: id,
        Email: email,
        DisplayName: displayName,
        Role: role,
        IsActive: isActive
    ));
})
.RequireAuthorization();

// --------------------------------------------------
// Work Items MVP (Iteration 2A)
// --------------------------------------------------

static WorkItemResponse ToResponse(WorkItem wi) =>
    new(
        wi.Id,
        wi.Title,
        wi.Description,
        (CivicOps.Contracts.WorkItems.WorkItemStatus)(int)wi.Status,
        wi.CreatedAt,
        wi.UpdatedAt
    );

var work = api.MapGroup("/work-items").RequireAuthorization();

// Create
work.MapPost("/", (CreateWorkItemRequest req, IWorkItemRepository repo) =>
{
    try
    {
        var wi = new WorkItem(req.Title, req.Description);
        repo.Add(wi);

        return Results.Created($"/api/work-items/{wi.Id}", ToResponse(wi));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// List (optional ?status=Todo|InProgress|Blocked|Done)
work.MapGet("/", (IWorkItemRepository repo, string? status) =>
{
    var items = repo.List().AsEnumerable();

    if (!string.IsNullOrWhiteSpace(status) &&
        Enum.TryParse<CivicOps.Contracts.WorkItems.WorkItemStatus>(status, ignoreCase: true, out var parsed))
    {
        items = items.Where(x => (int)x.Status == (int)parsed);
    }

    return Results.Ok(items.Select(ToResponse));
});

// Get by id
work.MapGet("/{id:guid}", (Guid id, IWorkItemRepository repo) =>
{
    var wi = repo.Get(id);
    return wi is null ? Results.NotFound() : Results.Ok(ToResponse(wi));
});

// Patch/update
work.MapPatch("/{id:guid}", (Guid id, UpdateWorkItemRequest req, IWorkItemRepository repo) =>
{
    try
    {
        var ok = repo.TryUpdate(id, wi =>
        {
            CivicOps.Domain.WorkItems.WorkItemStatus? domainStatus = null;

            if (req.Status is not null)
                domainStatus = (CivicOps.Domain.WorkItems.WorkItemStatus)(int)req.Status.Value;

            wi.Update(req.Title, req.Description, domainStatus);
        });

        if (!ok) return Results.NotFound();

        var updated = repo.Get(id)!;
        return Results.Ok(ToResponse(updated));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Delete
work.MapDelete("/{id:guid}", (Guid id, IWorkItemRepository repo) =>
{
    return repo.Delete(id) ? Results.NoContent() : Results.NotFound();
});

// --------------------------------------------------
// Root
// --------------------------------------------------
app.MapGet("/", () =>
    Results.Ok("CivicOps API is running. POST /api/dev/token, then GET /api/me"));

app.Run();
