using CivicOps.Contracts.Auth;
using CivicOps.Contracts.Users;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseHttpsRedirection();

var api = app.MapGroup("/api");

api.MapGet("/me", () =>
{
    var me = new MeResponse(
        Id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Email: "dev@civicops.local",
        DisplayName: "Dev User",
        Role: Role.Admin,
        IsActive: true
    );

    return Results.Ok(me);
});

app.MapGet("/", () => Results.Ok("CivicOps API is running. Try GET /api/me"));


app.Run();
