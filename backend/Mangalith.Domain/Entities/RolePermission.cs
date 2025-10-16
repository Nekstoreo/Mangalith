namespace Mangalith.Domain.Entities;

public class RolePermission
{
    public UserRole Role { get; private set; }
    public int PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;
    public DateTime GrantedAtUtc { get; private set; }

    private RolePermission()
    {
        GrantedAtUtc = DateTime.UtcNow;
    }

    public RolePermission(UserRole role, int permissionId)
    {
        Role = role;
        PermissionId = permissionId;
        GrantedAtUtc = DateTime.UtcNow;
    }

    public RolePermission(UserRole role, Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        Role = role;
        PermissionId = permission.Id;
        Permission = permission;
        GrantedAtUtc = DateTime.UtcNow;
    }
}