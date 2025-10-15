namespace Mangalith.Application.Contracts.Auth;

public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, string Email, string FullName);
