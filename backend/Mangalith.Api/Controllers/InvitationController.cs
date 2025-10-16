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
/// Controlador para gestión del sistema de invitaciones de usuarios
/// </summary>
[ApiController]
[Route("api/admin/invitations")]
[Authorize]
public class InvitationController : ControllerBase
{
    private readonly IUserInvitationService _invitationService;
    private readonly IAuditService _auditService;
    private readonly ILogger<InvitationController> _logger;

    public InvitationController(
        IUserInvitationService invitationService,
        IAuditService auditService,
        ILogger<InvitationController> logger)
    {
        _invitationService = invitationService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene una lista paginada de invitaciones con filtros
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.User.Invite)]
    [ProducesResponseType(typeof(PagedResult<InvitationSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInvitations([FromQuery] InvitationFilterRequest filter, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} requesting invitations list with filter: {@Filter}", currentUserId, filter);

            // Obtener invitaciones (implementación básica)
            var allInvitations = await _invitationService.GetPendingInvitationsAsync(cancellationToken);
            var invitationsList = allInvitations.ToList();

            // Aplicar filtros básicos
            var filteredInvitations = invitationsList.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Email))
            {
                filteredInvitations = filteredInvitations.Where(i => i.Email.Contains(filter.Email, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.TargetRole.HasValue)
            {
                filteredInvitations = filteredInvitations.Where(i => i.TargetRole == filter.TargetRole.Value);
            }

            if (filter.InvitedByUserId.HasValue)
            {
                filteredInvitations = filteredInvitations.Where(i => i.InvitedByUserId == filter.InvitedByUserId.Value);
            }

            if (!filter.IncludeExpired)
            {
                filteredInvitations = filteredInvitations.Where(i => !i.IsExpired);
            }

            // Aplicar paginación
            var totalCount = filteredInvitations.Count();
            var invitations = filteredInvitations
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(i => new InvitationSummaryResponse
                {
                    Id = i.Id,
                    Email = i.Email,
                    TargetRole = i.TargetRole,
                    InvitedBy = new InvitationUserInfo
                    {
                        Id = i.InvitedBy.Id,
                        Email = i.InvitedBy.Email,
                        FullName = i.InvitedBy.FullName,
                        Role = i.InvitedBy.Role
                    },
                    CreatedAtUtc = i.CreatedAtUtc,
                    ExpiresAtUtc = i.ExpiresAtUtc,
                    Status = DetermineInvitationStatus(i),
                    IsExpired = i.IsExpired,
                    IsAccepted = i.IsAccepted,
                    HoursUntilExpiration = i.IsExpired ? null : (i.ExpiresAtUtc - DateTime.UtcNow).TotalHours
                })
                .ToList();

            var result = new PagedResult<InvitationSummaryResponse>(invitations, totalCount, filter.Page, filter.PageSize);

            // Registrar acceso a la lista de invitaciones
            await _auditService.LogSuccessAsync(
                currentUserId,
                "invitation.list",
                "invitation_system",
                GetClientIpAddress(),
                details: $"Retrieved {invitations.Count} invitations (page {filter.Page})",
                cancellationToken: cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invitations list");
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving invitations list"));
        }
    }

    /// <summary>
    /// Obtiene información detallada de una invitación específica
    /// </summary>
    [HttpGet("{invitationId:guid}")]
    [RequirePermission(Permissions.User.Invite)]
    [ProducesResponseType(typeof(InvitationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInvitationDetail(Guid invitationId, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} requesting details for invitation {InvitationId}", currentUserId, invitationId);

            var invitation = await _invitationService.GetInvitationByIdAsync(invitationId, cancellationToken);
            if (invitation == null)
            {
                return NotFound(new ErrorResponse("invitation_not_found", "Invitation not found"));
            }

            var response = new InvitationDetailResponse
            {
                Id = invitation.Id,
                Email = invitation.Email,
                TargetRole = invitation.TargetRole,
                Token = invitation.Token,
                InvitedBy = new InvitationUserInfo
                {
                    Id = invitation.InvitedBy.Id,
                    Email = invitation.InvitedBy.Email,
                    FullName = invitation.InvitedBy.FullName,
                    Role = invitation.InvitedBy.Role
                },
                CreatedAtUtc = invitation.CreatedAtUtc,
                ExpiresAtUtc = invitation.ExpiresAtUtc,
                AcceptedAtUtc = invitation.AcceptedAtUtc,
                AcceptedBy = invitation.AcceptedBy != null ? new InvitationUserInfo
                {
                    Id = invitation.AcceptedBy.Id,
                    Email = invitation.AcceptedBy.Email,
                    FullName = invitation.AcceptedBy.FullName,
                    Role = invitation.AcceptedBy.Role
                } : null,
                Status = DetermineInvitationStatus(invitation),
                IsExpired = invitation.IsExpired,
                IsAccepted = invitation.IsAccepted,
                TimeUntilExpiration = invitation.IsExpired ? null : invitation.ExpiresAtUtc - DateTime.UtcNow,
                EmailSendAttempts = 1, // TODO: Implementar tracking real de envíos
                LastEmailSentAtUtc = invitation.CreatedAtUtc // TODO: Implementar tracking real
            };

            // Registrar acceso a detalles de invitación
            await _auditService.LogSuccessAsync(
                currentUserId,
                "invitation.view_details",
                "invitation",
                GetClientIpAddress(),
                invitationId.ToString(),
                $"Viewed details for invitation to {invitation.Email}",
                cancellationToken: cancellationToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invitation detail for invitation {InvitationId}", invitationId);
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving invitation details"));
        }
    }

    /// <summary>
    /// Crea una nueva invitación de usuario
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.User.Invite)]
    [ProducesResponseType(typeof(InvitationDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} creating invitation for {Email} with role {TargetRole}", 
                currentUserId, request.Email, request.TargetRole);

            // Validar si el usuario puede crear invitaciones para este rol
            var canCreate = await _invitationService.CanUserCreateInvitationAsync(currentUserId, request.TargetRole, cancellationToken);
            if (!canCreate)
            {
                return Forbid("You don't have permission to create invitations for this role");
            }

            // Verificar si ya existe una invitación pendiente
            var hasPending = await _invitationService.HasPendingInvitationAsync(request.Email, request.TargetRole, cancellationToken);
            if (hasPending)
            {
                return Conflict(new ErrorResponse("invitation_exists", "A pending invitation already exists for this email and role"));
            }

            // Crear la invitación
            var expirationPeriod = request.ExpirationHours.HasValue 
                ? (TimeSpan?)TimeSpan.FromHours(Math.Max(1, Math.Min(request.ExpirationHours.Value, 8760))) // Entre 1 hora y 1 año
                : (TimeSpan?)null;

            var invitation = await _invitationService.CreateInvitationAsync(
                request.Email, 
                request.TargetRole, 
                currentUserId, 
                expirationPeriod, 
                cancellationToken);

            var response = new InvitationDetailResponse
            {
                Id = invitation.Id,
                Email = invitation.Email,
                TargetRole = invitation.TargetRole,
                Token = invitation.Token,
                InvitedBy = new InvitationUserInfo
                {
                    Id = invitation.InvitedBy.Id,
                    Email = invitation.InvitedBy.Email,
                    FullName = invitation.InvitedBy.FullName,
                    Role = invitation.InvitedBy.Role
                },
                CreatedAtUtc = invitation.CreatedAtUtc,
                ExpiresAtUtc = invitation.ExpiresAtUtc,
                Status = InvitationStatus.Pending,
                IsExpired = false,
                IsAccepted = false,
                TimeUntilExpiration = invitation.ExpiresAtUtc - DateTime.UtcNow,
                Message = request.Message,
                EmailSendAttempts = request.SendEmail ? 1 : 0,
                LastEmailSentAtUtc = request.SendEmail ? DateTime.UtcNow : null
            };

            // TODO: Enviar email de invitación si se solicita
            if (request.SendEmail)
            {
                _logger.LogInformation("Email invitation would be sent to {Email} (not implemented)", request.Email);
            }

            return CreatedAtAction(nameof(GetInvitationDetail), new { invitationId = invitation.Id }, response);
        }
        catch (ConflictAppException ex)
        {
            return Conflict(new ErrorResponse("conflict", ex.Message));
        }
        catch (ValidationAppException ex)
        {
            return BadRequest(new ErrorResponse("validation_error", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invitation");
            return StatusCode(500, new ErrorResponse("internal_error", "Error creating invitation"));
        }
    }

    /// <summary>
    /// Acepta una invitación usando su token
    /// </summary>
    [HttpPost("accept")]
    [AllowAnonymous] // Permitir acceso anónimo para aceptar invitaciones
    [ProducesResponseType(typeof(AcceptInvitationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status410Gone)]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Attempting to accept invitation with token {Token}", request.Token[..8] + "...");

            // Validar la invitación
            var invitation = await _invitationService.GetInvitationByTokenAsync(request.Token, cancellationToken);
            if (invitation == null)
            {
                return NotFound(new ErrorResponse("invitation_not_found", "Invalid invitation token"));
            }

            if (invitation.IsExpired)
            {
                return StatusCode(410, new ErrorResponse("invitation_expired", "This invitation has expired"));
            }

            if (invitation.IsAccepted)
            {
                return BadRequest(new ErrorResponse("invitation_already_accepted", "This invitation has already been accepted"));
            }

            // Para aceptar una invitación, el usuario debe estar autenticado
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Unauthorized(new ErrorResponse("authentication_required", "You must be logged in to accept an invitation"));
            }

            var currentUserId = GetCurrentUserId();

            // Aceptar la invitación
            var success = await _invitationService.AcceptInvitationAsync(request.Token, currentUserId, cancellationToken);
            if (!success)
            {
                return BadRequest(new ErrorResponse("acceptance_failed", "Failed to accept invitation"));
            }

            // Obtener la invitación actualizada
            var updatedInvitation = await _invitationService.GetInvitationByTokenAsync(request.Token, cancellationToken);

            var response = new AcceptInvitationResponse
            {
                Success = true,
                Message = "Invitation accepted successfully",
                NewRole = updatedInvitation?.TargetRole,
                AcceptedAtUtc = updatedInvitation?.AcceptedAtUtc,
                User = updatedInvitation?.AcceptedBy != null ? new InvitationUserInfo
                {
                    Id = updatedInvitation.AcceptedBy.Id,
                    Email = updatedInvitation.AcceptedBy.Email,
                    FullName = updatedInvitation.AcceptedBy.FullName,
                    Role = updatedInvitation.AcceptedBy.Role
                } : null
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation");
            return StatusCode(500, new ErrorResponse("internal_error", "Error accepting invitation"));
        }
    }

    /// <summary>
    /// Valida una invitación por su token sin aceptarla
    /// </summary>
    [HttpGet("validate/{token}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ValidateInvitationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateInvitation(string token, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Validating invitation token {Token}", token[..8] + "...");

            var invitation = await _invitationService.GetInvitationByTokenAsync(token, cancellationToken);
            
            if (invitation == null)
            {
                return Ok(new ValidateInvitationResponse
                {
                    IsValid = false,
                    ErrorCode = "invitation_not_found",
                    ErrorMessage = "Invalid invitation token"
                });
            }

            if (invitation.IsExpired)
            {
                return Ok(new ValidateInvitationResponse
                {
                    IsValid = false,
                    ErrorCode = "invitation_expired",
                    ErrorMessage = "This invitation has expired"
                });
            }

            if (invitation.IsAccepted)
            {
                return Ok(new ValidateInvitationResponse
                {
                    IsValid = false,
                    ErrorCode = "invitation_already_accepted",
                    ErrorMessage = "This invitation has already been accepted"
                });
            }

            return Ok(new ValidateInvitationResponse
            {
                IsValid = true,
                Invitation = new InvitationDetailResponse
                {
                    Id = invitation.Id,
                    Email = invitation.Email,
                    TargetRole = invitation.TargetRole,
                    InvitedBy = new InvitationUserInfo
                    {
                        Id = invitation.InvitedBy.Id,
                        Email = invitation.InvitedBy.Email,
                        FullName = invitation.InvitedBy.FullName,
                        Role = invitation.InvitedBy.Role
                    },
                    CreatedAtUtc = invitation.CreatedAtUtc,
                    ExpiresAtUtc = invitation.ExpiresAtUtc,
                    Status = InvitationStatus.Pending,
                    IsExpired = false,
                    IsAccepted = false,
                    TimeUntilExpiration = invitation.ExpiresAtUtc - DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating invitation token");
            return Ok(new ValidateInvitationResponse
            {
                IsValid = false,
                ErrorCode = "validation_error",
                ErrorMessage = "Error validating invitation"
            });
        }
    }

    /// <summary>
    /// Extiende la fecha de expiración de una invitación
    /// </summary>
    [HttpPatch("{invitationId:guid}/extend")]
    [RequirePermission(Permissions.User.Invite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExtendInvitation(Guid invitationId, [FromBody] ExtendInvitationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} extending invitation {InvitationId} by {Hours} hours", 
                currentUserId, invitationId, request.AdditionalHours);

            var additionalTime = TimeSpan.FromHours(request.AdditionalHours);
            var success = await _invitationService.ExtendInvitationAsync(invitationId, additionalTime, cancellationToken);

            if (!success)
            {
                return NotFound(new ErrorResponse("invitation_not_found", "Invitation not found or cannot be extended"));
            }

            // Registrar extensión en auditoría
            await _auditService.LogSuccessAsync(
                currentUserId,
                "invitation.extend",
                "invitation",
                GetClientIpAddress(),
                invitationId.ToString(),
                $"Extended invitation by {request.AdditionalHours} hours. Reason: {request.Reason}",
                cancellationToken: cancellationToken);

            return Ok(new { message = $"Invitation extended by {request.AdditionalHours} hours successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending invitation {InvitationId}", invitationId);
            return StatusCode(500, new ErrorResponse("internal_error", "Error extending invitation"));
        }
    }

    /// <summary>
    /// Cancela/expira una invitación manualmente
    /// </summary>
    [HttpPatch("{invitationId:guid}/cancel")]
    [RequirePermission(Permissions.User.Invite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelInvitation(Guid invitationId, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} cancelling invitation {InvitationId}", currentUserId, invitationId);

            var success = await _invitationService.ExpireInvitationAsync(invitationId, cancellationToken);
            if (!success)
            {
                return NotFound(new ErrorResponse("invitation_not_found", "Invitation not found"));
            }

            // Registrar cancelación en auditoría
            await _auditService.LogSuccessAsync(
                currentUserId,
                "invitation.cancel",
                "invitation",
                GetClientIpAddress(),
                invitationId.ToString(),
                "Invitation cancelled manually",
                cancellationToken: cancellationToken);

            return Ok(new { message = "Invitation cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invitation {InvitationId}", invitationId);
            return StatusCode(500, new ErrorResponse("internal_error", "Error cancelling invitation"));
        }
    }

    /// <summary>
    /// Obtiene estadísticas del sistema de invitaciones
    /// </summary>
    [HttpGet("statistics")]
    [RequirePermission(Permissions.System.Monitor)]
    [ProducesResponseType(typeof(InvitationStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInvitationStatistics(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} requesting invitation statistics", currentUserId);

            var statistics = await _invitationService.GetInvitationStatisticsAsync(fromDate, toDate, cancellationToken);

            var response = new InvitationStatisticsResponse
            {
                TotalInvitations = statistics.TotalInvitations,
                PendingInvitations = statistics.PendingInvitations,
                AcceptedInvitations = statistics.AcceptedInvitations,
                ExpiredInvitations = statistics.ExpiredInvitations,
                InvitationsByRole = statistics.InvitationsByRole,
                TopInviters = statistics.TopInviters,
                FromDate = statistics.FromDate,
                ToDate = statistics.ToDate,
                GeneratedAtUtc = statistics.GeneratedAtUtc,
                
                // Campos adicionales específicos del response
                InvitationsByStatus = new Dictionary<InvitationStatus, long>
                {
                    [InvitationStatus.Pending] = statistics.PendingInvitations,
                    [InvitationStatus.Accepted] = statistics.AcceptedInvitations,
                    [InvitationStatus.Expired] = statistics.ExpiredInvitations
                },
                InvitationsByMonth = new Dictionary<string, long>(), // TODO: Implementar
                AverageAcceptanceTimeHours = 0, // TODO: Implementar
                ExpiringInNext7Days = 0 // TODO: Implementar
            };

            // Registrar acceso a estadísticas
            await _auditService.LogSuccessAsync(
                currentUserId,
                "invitation.view_statistics",
                "invitation_system",
                GetClientIpAddress(),
                details: "Viewed invitation system statistics",
                cancellationToken: cancellationToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invitation statistics");
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving invitation statistics"));
        }
    }

    /// <summary>
    /// Limpia invitaciones expiradas automáticamente
    /// </summary>
    [HttpPost("cleanup")]
    [RequirePermission(Permissions.System.Maintenance)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CleanupExpiredInvitations(CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} initiating invitation cleanup", currentUserId);

            var deletedCount = await _invitationService.CleanupExpiredInvitationsAsync(cancellationToken);

            // Registrar limpieza
            await _auditService.LogSuccessAsync(
                currentUserId,
                "invitation.cleanup",
                "invitation_system",
                GetClientIpAddress(),
                details: $"Cleaned up {deletedCount} expired invitations",
                cancellationToken: cancellationToken);

            return Ok(new
            {
                message = "Expired invitations cleanup completed successfully",
                deletedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired invitations");
            return StatusCode(500, new ErrorResponse("internal_error", "Error cleaning up expired invitations"));
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

    private static InvitationStatus DetermineInvitationStatus(UserInvitation invitation)
    {
        if (invitation.IsAccepted)
            return InvitationStatus.Accepted;
        
        if (invitation.IsExpired)
            return InvitationStatus.Expired;
        
        return InvitationStatus.Pending;
    }
}