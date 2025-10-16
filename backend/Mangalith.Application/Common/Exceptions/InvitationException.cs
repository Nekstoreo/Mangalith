namespace Mangalith.Application.Common.Exceptions;

/// <summary>
/// Excepción base para errores relacionados con invitaciones
/// </summary>
public abstract class InvitationException : AppException
{
    protected InvitationException(string code, string message) : base(code, message)
    {
    }
}

/// <summary>
/// Excepción lanzada cuando una invitación ha expirado
/// </summary>
public class InvitationExpiredException : InvitationException
{
    public DateTime ExpiredAt { get; }

    public InvitationExpiredException(DateTime expiredAt)
        : base("invitation_expired", $"The invitation expired on {expiredAt:yyyy-MM-dd HH:mm:ss} UTC")
    {
        ExpiredAt = expiredAt;
    }

    public InvitationExpiredException()
        : base("invitation_expired", "The invitation has expired")
    {
        ExpiredAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Excepción lanzada cuando una invitación ya ha sido aceptada
/// </summary>
public class InvitationAlreadyAcceptedException : InvitationException
{
    public DateTime AcceptedAt { get; }
    public Guid AcceptedByUserId { get; }

    public InvitationAlreadyAcceptedException(DateTime acceptedAt, Guid acceptedByUserId)
        : base("invitation_already_accepted", $"The invitation was already accepted on {acceptedAt:yyyy-MM-dd HH:mm:ss} UTC")
    {
        AcceptedAt = acceptedAt;
        AcceptedByUserId = acceptedByUserId;
    }

    public InvitationAlreadyAcceptedException()
        : base("invitation_already_accepted", "The invitation has already been accepted")
    {
        AcceptedAt = DateTime.UtcNow;
        AcceptedByUserId = Guid.Empty;
    }
}

/// <summary>
/// Excepción lanzada cuando una invitación no se encuentra
/// </summary>
public class InvitationNotFoundException : InvitationException
{
    public string Token { get; }

    public InvitationNotFoundException(string token)
        : base("invitation_not_found", "The invitation was not found or is invalid")
    {
        Token = token;
    }
}

/// <summary>
/// Excepción lanzada cuando un usuario intenta crear una invitación sin permisos suficientes
/// </summary>
public class InsufficientPrivilegesForInvitationException : InvitationException
{
    public string RequiredRole { get; }
    public string UserRole { get; }

    public InsufficientPrivilegesForInvitationException(string requiredRole, string userRole)
        : base("insufficient_privileges_for_invitation", $"You need at least {requiredRole} role to create invitations for this role. Your current role: {userRole}")
    {
        RequiredRole = requiredRole;
        UserRole = userRole;
    }
}