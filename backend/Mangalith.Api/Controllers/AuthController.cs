using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Api.Authorization;
using Mangalith.Api.Contracts;
using Mangalith.Application.Contracts.Auth;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Constants;
using Mangalith.Domain.Entities;

namespace Mangalith.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement role-based registration restrictions
        // For now, allow public registration with Reader role by default
        // In the future, elevated roles should require invitation or admin approval
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return Created("api/profile/me", response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Endpoint de ejemplo para demostrar autorización basada en permisos
    /// </summary>
    [HttpGet("test-permission")]
    [RequirePermission(Permissions.User.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult TestPermission()
    {
        return Ok(new { message = "You have the 'user.read' permission!" });
    }

    /// <summary>
    /// Endpoint de ejemplo para demostrar autorización basada en roles
    /// </summary>
    [HttpGet("test-role")]
    [RequireRole(UserRole.Moderator)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult TestRole()
    {
        return Ok(new { message = "You have at least Moderator role!" });
    }
}
