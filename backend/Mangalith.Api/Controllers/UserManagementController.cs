using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Api.Authorization;
using Mangalith.Api.Contracts;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Contracts.Admin;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Constants;
using Mangalith.Domain.Entities;

namespace Mangalith.Api.Controllers;

/// <summary>
/// Controlador para gestión administrativa de usuarios
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly IAuditService _auditService;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        IUserManagementService userManagementService,
        IAuditService auditService,
        ILogger<UserManagementController> logger)
    {
        _userManagementService = userManagementService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene una lista paginada de usuarios con filtros
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.User.Manage)]
    [ProducesResponseType(typeof(PagedResult<UserSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers([FromQuery] UserFilterRequest filter, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} requesting user list with filter: {@Filter}", currentUserId, filter);

            var result = await _userManagementService.GetUsersAsync(filter, cancellationToken);

            // Registrar acceso a la lista de usuarios
            await _auditService.LogSuccessAsync(
                currentUserId,
                "user.list",
                "user_management",
                GetClientIpAddress(),
                details: $"Retrieved {result.Items.Count()} users (page {result.Page})",
                cancellationToken: cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user list");
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving user list"));
        }
    }

    /// <summary>
    /// Obtiene información detallada de un usuario específico
    /// </summary>
    [HttpGet("{userId:guid}")]
    [RequirePermission(Permissions.User.Manage)]
    [ProducesResponseType(typeof(UserDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserDetail(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} requesting details for user {UserId}", currentUserId, userId);

            var userDetail = await _userManagementService.GetUserDetailAsync(userId, cancellationToken);
            if (userDetail == null)
            {
                return NotFound(new ErrorResponse("user_not_found", "User not found"));
            }

            // Registrar acceso a detalles de usuario
            await _auditService.LogSuccessAsync(
                currentUserId,
                "user.view_details",
                "user",
                GetClientIpAddress(),
                userId.ToString(),
                $"Viewed details for user {userDetail.Email}",
                cancellationToken: cancellationToken);

            return Ok(userDetail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user detail for user {UserId}", userId);
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving user details"));
        }
    }

    /// <summary>
    /// Actualiza información de un usuario
    /// </summary>
    [HttpPut("{userId:guid}")]
    [RequirePermission(Permissions.User.Manage)]
    [ProducesResponseType(typeof(UserDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} updating user {UserId}", currentUserId, userId);

            var updatedUser = await _userManagementService.UpdateUserAsync(userId, request, currentUserId, cancellationToken);
            return Ok(updatedUser);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ErrorResponse("user_not_found", ex.Message));
        }
        catch (ConflictAppException ex)
        {
            return Conflict(new ErrorResponse("conflict", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return StatusCode(500, new ErrorResponse("internal_error", "Error updating user"));
        }
    }

    /// <summary>
    /// Activa o desactiva un usuario
    /// </summary>
    [HttpPatch("{userId:guid}/status")]
    [RequirePermission(Permissions.User.Manage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetUserStatus(Guid userId, [FromBody] SetUserStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} setting status of user {UserId} to {IsActive}", currentUserId, userId, request.IsActive);

            var success = await _userManagementService.SetUserActiveStatusAsync(userId, request.IsActive, currentUserId, request.Reason, cancellationToken);
            if (!success)
            {
                return NotFound(new ErrorResponse("user_not_found", "User not found"));
            }

            return Ok(new { message = $"User {(request.IsActive ? "activated" : "deactivated")} successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting status for user {UserId}", userId);
            return StatusCode(500, new ErrorResponse("internal_error", "Error updating user status"));
        }
    }

    /// <summary>
    /// Cambia el rol de un usuario
    /// </summary>
    [HttpPatch("{userId:guid}/role")]
    [RequirePermission(Permissions.User.Manage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeUserRole(Guid userId, [FromBody] ChangeUserRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} changing role of user {UserId} to {NewRole}", currentUserId, userId, request.NewRole);

            var success = await _userManagementService.ChangeUserRoleAsync(userId, request.NewRole, currentUserId, request.Reason, cancellationToken);
            if (!success)
            {
                return NotFound(new ErrorResponse("user_not_found", "User not found"));
            }

            return Ok(new { message = $"User role changed to {request.NewRole} successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing role for user {UserId}", userId);
            return StatusCode(500, new ErrorResponse("internal_error", "Error changing user role"));
        }
    }

    /// <summary>
    /// Elimina un usuario del sistema
    /// </summary>
    [HttpDelete("{userId:guid}")]
    [RequirePermission(Permissions.User.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(Guid userId, [FromBody] DeleteUserRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} deleting user {UserId}", currentUserId, userId);

            var success = await _userManagementService.DeleteUserAsync(userId, currentUserId, request?.Reason, cancellationToken);
            if (!success)
            {
                return NotFound(new ErrorResponse("user_not_found", "User not found"));
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return StatusCode(500, new ErrorResponse("internal_error", "Error deleting user"));
        }
    }

    /// <summary>
    /// Realiza operaciones en lote sobre múltiples usuarios
    /// </summary>
    [HttpPost("bulk")]
    [RequirePermission(Permissions.User.Manage)]
    [ProducesResponseType(typeof(BulkUserOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkOperation([FromBody] BulkUserOperationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} performing bulk operation {Operation} on {UserCount} users", 
                currentUserId, request.Operation, request.UserIds.Count);

            if (!request.UserIds.Any())
            {
                return BadRequest(new ErrorResponse("invalid_request", "No users specified for bulk operation"));
            }

            if (request.Operation == BulkUserOperation.ChangeRole && !request.NewRole.HasValue)
            {
                return BadRequest(new ErrorResponse("invalid_request", "NewRole is required for ChangeRole operation"));
            }

            var result = await _userManagementService.BulkOperationAsync(request, currentUserId, cancellationToken);

            // Registrar operación en lote
            await _auditService.LogSuccessAsync(
                currentUserId,
                "user.bulk_operation",
                "user_management",
                GetClientIpAddress(),
                resourceId: null,
                details: $"Bulk {request.Operation}: {result.SuccessCount} successful, {result.FailureCount} failed",
                cancellationToken: cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation");
            return StatusCode(500, new ErrorResponse("internal_error", "Error performing bulk operation"));
        }
    }

    /// <summary>
    /// Obtiene estadísticas generales del sistema de usuarios
    /// </summary>
    [HttpGet("statistics")]
    [RequirePermission(Permissions.System.Monitor)]
    [ProducesResponseType(typeof(UserSystemStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserStatistics(CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} requesting user statistics", currentUserId);

            var statistics = await _userManagementService.GetUserStatisticsAsync(cancellationToken);

            // Registrar acceso a estadísticas
            await _auditService.LogSuccessAsync(
                currentUserId,
                "user.view_statistics",
                "user_management",
                GetClientIpAddress(),
                resourceId: null,
                details: "Viewed user system statistics",
                cancellationToken: cancellationToken);

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user statistics");
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving user statistics"));
        }
    }

    /// <summary>
    /// Obtiene la actividad reciente de un usuario
    /// </summary>
    [HttpGet("{userId:guid}/activity")]
    [RequirePermission(Permissions.User.Manage)]
    [ProducesResponseType(typeof(IEnumerable<UserActivityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserActivity(Guid userId, CancellationToken cancellationToken, [FromQuery] int limit = 20)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} requesting activity for user {UserId}", currentUserId, userId);

            var activity = await _userManagementService.GetUserRecentActivityAsync(userId, limit, cancellationToken);
            return Ok(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity for user {UserId}", userId);
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving user activity"));
        }
    }

    /// <summary>
    /// Obtiene el historial de cambios de rol de un usuario
    /// </summary>
    [HttpGet("{userId:guid}/role-history")]
    [RequirePermission(Permissions.User.Manage)]
    [ProducesResponseType(typeof(IEnumerable<UserRoleChangeHistory>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserRoleHistory(Guid userId, CancellationToken cancellationToken, [FromQuery] int limit = 10)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} requesting role history for user {UserId}", currentUserId, userId);

            var roleHistory = await _userManagementService.GetUserRoleHistoryAsync(userId, limit, cancellationToken);
            return Ok(roleHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role history for user {UserId}", userId);
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving role history"));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    private string GetClientIpAddress()
    {
        // Intentar obtener la IP real del cliente
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }
}

/// <summary>
/// Request para cambiar el estado activo de un usuario
/// </summary>
public class SetUserStatusRequest
{
    /// <summary>
    /// Nuevo estado activo
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Razón del cambio
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Request para cambiar el rol de un usuario
/// </summary>
public class ChangeUserRoleRequest
{
    /// <summary>
    /// Nuevo rol
    /// </summary>
    public UserRole NewRole { get; set; }

    /// <summary>
    /// Razón del cambio
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Request para eliminar un usuario
/// </summary>
public class DeleteUserRequest
{
    /// <summary>
    /// Razón de la eliminación
    /// </summary>
    public string? Reason { get; set; }
}