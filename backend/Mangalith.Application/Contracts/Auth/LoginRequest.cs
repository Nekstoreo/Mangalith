using System.ComponentModel.DataAnnotations;

namespace Mangalith.Application.Contracts.Auth;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}
