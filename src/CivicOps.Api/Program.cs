using CivicOps.Contracts.Auth;
using CivicOps.Contracts.Users;
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
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),

            // 👇 Explicit identity semantics
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role",
        };
    });

builder.Services.AddAuthorization();

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
        var signingKey =
            jwt["SigningKey"] ?? throw new InvalidOperationException("Missing Jwt:SigningKey");

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
        var claims = user.Claims
            .Select(c => new { c.Type, c.Value })
            .ToList();

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
// Root
// --------------------------------------------------
app.MapGet("/", () =>
    Results.Ok("CivicOps API is running. POST /api/dev/token, then GET /api/me"));

app.Run();
