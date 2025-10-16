using Microsoft.Extensions.Logging;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Services;

/// <summary>
/// Servicio para gestión del sistema de invitaciones con validaciones de seguridad
/// </summary>
public class UserInvitationService : IUserInvitationService
{
    private readonly IUserInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<UserInvitationService> _logger;

    // Configuración de invitaciones
    private static readonly TimeSpan DefaultExpirationPeriod = TimeSpan.FromDays(7);
    private static readonly TimeSpan MaxExpirationPeriod = TimeSpan.FromDays(30);

    public UserInvitationService(
        IUserInvitationRepository invitationRepository,
        IUserRepository userRepository,
        IPermissionService permissionService,
        ILogger<UserInvitationService> logger)
    {
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<UserInvitation> CreateInvitationAsync(
        string email,
        UserRole targetRole,
        Guid invitedByUserId,
        TimeSpan? expirationPeriod = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        if (invitedByUserId == Guid.Empty)
            throw new ArgumentException("InvitedByUserId cannot be empty", nameof(invitedByUserId));

        try
        {
            // Validar que el usuario invitador puede crear invitaciones para este rol
            var canCreateInvitation = await CanUserCreateInvitationAsync(invitedByUserId, targetRole, cancellationToken);
            if (!canCreateInvitation)
            {
                _logger.LogWarning("User {InviterId} attempted to create invitation for role {TargetRole} without permission",
                    invitedByUserId, targetRole);
                throw new UnauthorizedAccessException($"User does not have permission to create invitations for role {targetRole}");
            }

            // Verificar si ya existe una invitación pendiente
            var hasPendingInvitation = await HasPendingInvitationAsync(email, targetRole, cancellationToken);
            if (hasPendingInvitation)
            {
                _logger.LogWarning("Attempted to create duplicate invitation for email {Email} and role {TargetRole}",
                    email, targetRole);
                throw new InvalidOperationException($"A pending invitation already exists for {email} with role {targetRole}");
            }

            // Verificar si el usuario ya existe
            var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogWarning("Attempted to create invitation for existing user {Email}", email);
                throw new InvalidOperationException($"User with email {email} already exists");
            }

            // Validar período de expiración
            var validatedExpirationPeriod = ValidateExpirationPeriod(expirationPeriod);

            // Crear la invitación
            var invitation = new UserInvitation(email, targetRole, invitedByUserId, validatedExpirationPeriod);
            var createdInvitation = await _invitationRepository.CreateAsync(invitation, cancellationToken);

            _logger.LogInformation("Created invitation {InvitationId} for email {Email} with role {TargetRole} by user {InviterId}",
                createdInvitation.Id, email, targetRole, invitedByUserId);

            return createdInvitation;
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error creating invitation for email {Email} with role {TargetRole}", email, targetRole);
            throw;
        }
    }

    public async Task<UserInvitation?> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var invitation = await _invitationRepository.GetByTokenAsync(token, cancellationToken);
            
            if (invitation == null)
            {
                _logger.LogWarning("Invitation token {Token} not found", token);
            }

            return invitation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invitation by token");
            return null;
        }
    }

    public async Task<UserInvitation?> GetInvitationByIdAsync(Guid invitationId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invitation {InvitationId}", invitationId);
            return null;
        }
    }

    public async Task<bool> AcceptInvitationAsync(string token, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token) || userId == Guid.Empty)
            return false;

        try
        {
            var invitation = await _invitationRepository.GetByTokenAsync(token, cancellationToken);
            if (invitation == null)
            {
                _logger.LogWarning("Attempted to accept non-existent invitation with token {Token}", token);
                return false;
            }

            if (!invitation.IsValid)
            {
                _logger.LogWarning("Attempted to accept invalid invitation {InvitationId} (expired: {IsExpired}, accepted: {IsAccepted})",
                    invitation.Id, invitation.IsExpired, invitation.IsAccepted);
                return false;
            }

            // Verificar que el usuario que acepta coincide con el email de la invitación
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null || !string.Equals(user.Email, invitation.Email, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("User {UserId} attempted to accept invitation {InvitationId} for different email",
                    userId, invitation.Id);
                return false;
            }

            // Aceptar la invitación
            var accepted = invitation.AcceptInvitation(userId);
            if (!accepted)
            {
                _logger.LogWarning("Failed to accept invitation {InvitationId} for user {UserId}", invitation.Id, userId);
                return false;
            }

            // Actualizar el rol del usuario
            user.UpdateRole(invitation.TargetRole);
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Actualizar la invitación
            await _invitationRepository.UpdateAsync(invitation, cancellationToken);

            // Invalidar caché de permisos del usuario
            await _permissionService.InvalidateUserPermissionsAsync(userId, cancellationToken);

            _logger.LogInformation("User {UserId} accepted invitation {InvitationId} and was assigned role {TargetRole}",
                userId, invitation.Id, invitation.TargetRole);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation with token {Token} for user {UserId}", token, userId);
            return false;
        }
    }

    public async Task<IEnumerable<UserInvitation>> GetPendingInvitationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _invitationRepository.GetPendingAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending invitations");
            return Enumerable.Empty<UserInvitation>();
        }
    }

    public async Task<IEnumerable<UserInvitation>> GetPendingInvitationsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Enumerable.Empty<UserInvitation>();

        try
        {
            return await _invitationRepository.GetByEmailAsync(email, onlyPending: true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending invitations for email {Email}", email);
            return Enumerable.Empty<UserInvitation>();
        }
    }

    public async Task<IEnumerable<UserInvitation>> GetInvitationsByInviterAsync(Guid invitedByUserId, bool includeExpired = false, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _invitationRepository.GetByInviterAsync(invitedByUserId, includeExpired, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invitations by inviter {InviterId}", invitedByUserId);
            return Enumerable.Empty<UserInvitation>();
        }
    }

    public async Task<bool> ExpireInvitationAsync(Guid invitationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invitation = await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);
            if (invitation == null)
            {
                _logger.LogWarning("Attempted to expire non-existent invitation {InvitationId}", invitationId);
                return false;
            }

            if (invitation.IsAccepted)
            {
                _logger.LogWarning("Attempted to expire already accepted invitation {InvitationId}", invitationId);
                return false;
            }

            invitation.ExpireInvitation();
            await _invitationRepository.UpdateAsync(invitation, cancellationToken);

            _logger.LogInformation("Manually expired invitation {InvitationId}", invitationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring invitation {InvitationId}", invitationId);
            return false;
        }
    }

    public async Task<bool> ExtendInvitationAsync(Guid invitationId, TimeSpan additionalTime, CancellationToken cancellationToken = default)
    {
        try
        {
            var invitation = await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);
            if (invitation == null)
            {
                _logger.LogWarning("Attempted to extend non-existent invitation {InvitationId}", invitationId);
                return false;
            }

            if (invitation.IsAccepted)
            {
                _logger.LogWarning("Attempted to extend already accepted invitation {InvitationId}", invitationId);
                return false;
            }

            // Validar que la extensión no exceda el período máximo
            var newExpirationDate = invitation.ExpiresAtUtc.Add(additionalTime);
            var maxAllowedDate = invitation.CreatedAtUtc.Add(MaxExpirationPeriod);
            
            if (newExpirationDate > maxAllowedDate)
            {
                _logger.LogWarning("Attempted to extend invitation {InvitationId} beyond maximum allowed period", invitationId);
                return false;
            }

            invitation.ExtendExpiration(additionalTime);
            await _invitationRepository.UpdateAsync(invitation, cancellationToken);

            _logger.LogInformation("Extended invitation {InvitationId} by {AdditionalTime}", invitationId, additionalTime);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending invitation {InvitationId}", invitationId);
            return false;
        }
    }

    public async Task<long> CleanupExpiredInvitationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var deletedCount = await _invitationRepository.DeleteExpiredAsync(cancellationToken);
            
            if (deletedCount > 0)
            {
                _logger.LogInformation("Cleaned up {DeletedCount} expired invitations", deletedCount);
            }

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired invitations");
            return 0;
        }
    }

    public async Task<bool> CanUserCreateInvitationAsync(Guid inviterUserId, UserRole targetRole, CancellationToken cancellationToken = default)
    {
        try
        {
            // Los usuarios solo pueden invitar a roles iguales o inferiores al suyo
            var inviterRole = await _permissionService.GetUserRoleAsync(inviterUserId, cancellationToken);
            if (!inviterRole.HasValue)
            {
                _logger.LogWarning("Cannot determine role for user {InviterId}", inviterUserId);
                return false;
            }

            // Verificar que el usuario tiene permisos para invitar
            var canInvite = await _permissionService.HasPermissionAsync(inviterUserId, "user.invite", cancellationToken);
            if (!canInvite)
            {
                return false;
            }

            // Los usuarios no pueden invitar a roles superiores al suyo
            if (targetRole > inviterRole.Value)
            {
                _logger.LogWarning("User {InviterId} with role {InviterRole} attempted to create invitation for higher role {TargetRole}",
                    inviterUserId, inviterRole.Value, targetRole);
                return false;
            }

            // Restricciones adicionales para roles específicos
            switch (targetRole)
            {
                case UserRole.Administrator:
                    // Solo administradores pueden invitar a otros administradores
                    return inviterRole.Value == UserRole.Administrator;
                
                case UserRole.Moderator:
                    // Solo moderadores y administradores pueden invitar moderadores
                    return inviterRole.Value >= UserRole.Moderator;
                
                default:
                    return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {InviterId} can create invitation for role {TargetRole}",
                inviterUserId, targetRole);
            return false;
        }
    }

    public async Task<bool> HasPendingInvitationAsync(string email, UserRole targetRole, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            return await _invitationRepository.ExistsPendingAsync(email, targetRole, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking pending invitation for email {Email} and role {TargetRole}", email, targetRole);
            return false;
        }
    }

    public async Task<InvitationStatistics> GetInvitationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _invitationRepository.GetStatisticsAsync(fromDate, toDate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invitation statistics");
            return new InvitationStatistics
            {
                FromDate = fromDate,
                ToDate = toDate,
                GeneratedAtUtc = DateTime.UtcNow
            };
        }
    }

    private static TimeSpan ValidateExpirationPeriod(TimeSpan? expirationPeriod)
    {
        if (!expirationPeriod.HasValue)
            return DefaultExpirationPeriod;

        var period = expirationPeriod.Value;
        
        // Mínimo 1 hora, máximo 30 días
        if (period < TimeSpan.FromHours(1))
            return TimeSpan.FromHours(1);
        
        if (period > MaxExpirationPeriod)
            return MaxExpirationPeriod;

        return period;
    }
}