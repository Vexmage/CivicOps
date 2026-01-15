using CivicOps.Contracts.Users;

namespace CivicOps.Contracts.Auth;

public sealed record MeResponse(
    Guid Id,
    string Email,
    string DisplayName,
    Role Role,
    bool IsActive
);
