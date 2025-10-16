namespace Mangalith.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando un usuario no tiene permisos para realizar una acción
/// </summary>
public class ForbiddenAppException : AppException
{
    public string? RequiredPermission { get; }
    public string? Resource { get; }

    public ForbiddenAppException(string message = "Access forbidden")
        : base("forbidden", message)
    {
    }

    public ForbiddenAppException(string requiredPermission, string message)
        : base("forbidden", message)
    {
        RequiredPermission = requiredPermission;
    }

    public ForbiddenAppException(string requiredPermission, string resource, string message)
        : base("forbidden", message)
    {
        RequiredPermission = requiredPermission;
        Resource = resource;
    }

    public static ForbiddenAppException ForPermission(string permission)
    {
        return new ForbiddenAppException(permission, $"You do not have the required permission: {permission}");
    }

    public static ForbiddenAppException ForResource(string permission, string resource)
    {
        return new ForbiddenAppException(permission, resource, $"You do not have permission to access {resource}. Required permission: {permission}");
    }
}