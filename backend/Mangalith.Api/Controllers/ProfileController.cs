using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Api.Contracts;

namespace Mangalith.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    [HttpGet("me", Name = nameof(GetCurrentProfile))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentProfile()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var name = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        return Ok(new
        {
            id = subject,
            email,
            fullName = name
        });
    }
}
