namespace Mangalith.Domain.Entities;

public class Permission
{
    public int Id { get; private set; }
    public string Resource { get; private set; }
    public string Action { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // Propiedad computada para el nombre completo del permiso
    public string Name => $"{Resource}.{Action}";

    // Propiedades de navegación
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Permission()
    {
        Resource = string.Empty;
        Action = string.Empty;
        Description = string.Empty;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Permission(string resource, string action, string description)
    {
        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentException("Resource cannot be null or empty", nameof(resource));
        
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or empty", nameof(action));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));

        Resource = resource.ToLowerInvariant();
        Action = action.ToLowerInvariant();
        Description = description;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));
        
        Description = description;
    }
}