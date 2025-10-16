using Microsoft.Extensions.Logging;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Contracts.Admin;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Services;

/// <summary>
/// Implementación del servicio de gestión administrativa de usuarios
/// </summary>
public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IUserRepository userRepository,
        IAuditService auditService,
        ILogger<UserManagementService> logger)
    {
        _userRepository = userRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<PagedResult<UserSummaryResponse>> GetUsersAsync(UserFilterRequest filter, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting users with filter: {@Filter}", filter);

        // Por ahora implementamos una versión básica
        // En una implementación completa, esto debería usar un repositorio con filtros y paginación
        var allUsers = await _userRepository.GetAllAsync(cancellationToken);
        
        var filteredUsers = allUsers.AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(filter.Email))
        {
            filteredUsers = filteredUsers.Where(u => u.Email.Contains(filter.Email, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(filter.FullName))
        {
            filteredUsers = filteredUsers.Where(u => u.FullName.Contains(filter.FullName, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.Role.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.Role == filter.Role.Value);
        }

        if (filter.IsActive.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.IsActive == filter.IsActive.Value);
        }

        if (filter.CreatedFrom.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.CreatedAtUtc >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.CreatedAtUtc <= filter.CreatedTo.Value);
        }

        // Aplicar ordenamiento
        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            var isDescending = filter.SortDirection?.ToLower() == "desc";
            
            filteredUsers = filter.SortBy.ToLower() switch
            {
                "email" => isDescending ? filteredUsers.OrderByDescending(u => u.Email) : filteredUsers.OrderBy(u => u.Email),
                "fullname" => isDescending ? filteredUsers.OrderByDescending(u => u.FullName) : filteredUsers.OrderBy(u => u.FullName),
                "role" => isDescending ? filteredUsers.OrderByDescending(u => u.Role) : filteredUsers.OrderBy(u => u.Role),
                "createdat" => isDescending ? filteredUsers.OrderByDescending(u => u.CreatedAtUtc) : filteredUsers.OrderBy(u => u.CreatedAtUtc),
                _ => filteredUsers.OrderBy(u => u.CreatedAtUtc)
            };
        }
        else
        {
            filteredUsers = filteredUsers.OrderBy(u => u.CreatedAtUtc);
        }

        var totalCount = filteredUsers.Count();
        var users = filteredUsers
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => new UserSummaryResponse
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAtUtc = u.CreatedAtUtc,
                LastLoginAtUtc = u.LastLoginAtUtc,
                TotalUploads = 0 // TODO: Implementar conteo real de uploads
            })
            .ToList();

        return new PagedResult<UserSummaryResponse>(users, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<UserDetailResponse?> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user detail for user {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return null;
        }

        // Obtener estadísticas de actividad
        var activityStats = new UserActivityStats
        {
            TotalUploads = 0, // TODO: Implementar conteo real
            TotalMangaCreated = 0, // TODO: Implementar conteo real
            TotalChaptersCreated = 0, // TODO: Implementar conteo real
            StorageUsed = 0, // TODO: Implementar cálculo real
            RecentAuditActions = 0, // TODO: Implementar conteo real
            LastActivityAtUtc = user.LastLoginAtUtc
        };

        // Obtener historial de roles (simulado por ahora)
        var roleHistory = new List<UserRoleChangeHistory>();

        return new UserDetailResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc,
            LastLoginAtUtc = user.LastLoginAtUtc,
            ActivityStats = activityStats,
            RoleHistory = roleHistory
        };
    }

    public async Task<UserDetailResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request, Guid updatedByUserId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user {UserId} by user {UpdatedByUserId}", userId, updatedByUserId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found");
        }

        var changes = new List<string>();

        // Actualizar campos si se proporcionan
        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            // Verificar que el email no esté en uso
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new ConflictAppException("Email is already in use by another user");
            }

            changes.Add($"Email: {user.Email} -> {request.Email}");
            // Note: Email update would need a new method in User entity
            // For now, we'll skip email updates as it's not implemented in the entity
        }

        if (!string.IsNullOrEmpty(request.FullName) && request.FullName != user.FullName)
        {
            changes.Add($"FullName: {user.FullName} -> {request.FullName}");
            user.UpdateProfile(request.FullName, user.Username, user.Bio);
        }

        if (request.Role.HasValue && request.Role.Value != user.Role)
        {
            changes.Add($"Role: {user.Role} -> {request.Role.Value}");
            user.UpdateRole(request.Role.Value);
        }

        if (request.IsActive.HasValue && request.IsActive.Value != user.IsActive)
        {
            changes.Add($"IsActive: {user.IsActive} -> {request.IsActive.Value}");
            user.SetActive(request.IsActive.Value);
        }

        if (changes.Any())
        {
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Registrar en auditoría
            var details = $"Changes: {string.Join(", ", changes)}";
            if (!string.IsNullOrEmpty(request.Reason))
            {
                details += $". Reason: {request.Reason}";
            }

            await _auditService.LogSuccessAsync(
                updatedByUserId,
                "user.update",
                "user",
                "127.0.0.1", // TODO: Obtener IP real
                userId.ToString(),
                details,
                cancellationToken: cancellationToken);

            _logger.LogInformation("User {UserId} updated successfully. Changes: {Changes}", userId, string.Join(", ", changes));
        }

        return await GetUserDetailAsync(userId, cancellationToken) 
               ?? throw new NotFoundException($"User with ID {userId} not found after update");
    }

    public async Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive, Guid updatedByUserId, string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting user {UserId} active status to {IsActive} by user {UpdatedByUserId}", userId, isActive, updatedByUserId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return false;
        }

        if (user.IsActive == isActive)
        {
            return true; // No hay cambio necesario
        }

        user.SetActive(isActive);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Registrar en auditoría
        var action = isActive ? "user.activate" : "user.deactivate";
        var details = $"User {(isActive ? "activated" : "deactivated")}";
        if (!string.IsNullOrEmpty(reason))
        {
            details += $". Reason: {reason}";
        }

        await _auditService.LogSuccessAsync(
            updatedByUserId,
            action,
            "user",
            "127.0.0.1", // TODO: Obtener IP real
            userId.ToString(),
            details,
            cancellationToken: cancellationToken);

        return true;
    }

    public async Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole, Guid updatedByUserId, string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Changing user {UserId} role to {NewRole} by user {UpdatedByUserId}", userId, newRole, updatedByUserId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return false;
        }

        if (user.Role == newRole)
        {
            return true; // No hay cambio necesario
        }

        var oldRole = user.Role;
        user.UpdateRole(newRole);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Registrar en auditoría
        var details = $"Role changed from {oldRole} to {newRole}";
        if (!string.IsNullOrEmpty(reason))
        {
            details += $". Reason: {reason}";
        }

        await _auditService.LogSuccessAsync(
            updatedByUserId,
            "user.role_change",
            "user",
            "127.0.0.1", // TODO: Obtener IP real
            userId.ToString(),
            details,
            cancellationToken: cancellationToken);

        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId, Guid deletedByUserId, string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user {UserId} by user {DeletedByUserId}", userId, deletedByUserId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return false;
        }

        // Registrar en auditoría antes de eliminar
        var details = $"User {user.Email} ({user.FullName}) deleted";
        if (!string.IsNullOrEmpty(reason))
        {
            details += $". Reason: {reason}";
        }

        await _auditService.LogSuccessAsync(
            deletedByUserId,
            "user.delete",
            "user",
            "127.0.0.1", // TODO: Obtener IP real
            userId.ToString(),
            details,
            cancellationToken: cancellationToken);

        await _userRepository.DeleteAsync(userId, cancellationToken);
        return true;
    }

    public async Task<BulkUserOperationResponse> BulkOperationAsync(BulkUserOperationRequest request, Guid operatedByUserId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Performing bulk operation {Operation} on {UserCount} users by user {OperatedByUserId}", 
            request.Operation, request.UserIds.Count, operatedByUserId);

        var response = new BulkUserOperationResponse();
        var errors = new List<BulkOperationError>();

        foreach (var userId in request.UserIds)
        {
            try
            {
                var success = request.Operation switch
                {
                    BulkUserOperation.Activate => await SetUserActiveStatusAsync(userId, true, operatedByUserId, request.Reason, cancellationToken),
                    BulkUserOperation.Deactivate => await SetUserActiveStatusAsync(userId, false, operatedByUserId, request.Reason, cancellationToken),
                    BulkUserOperation.ChangeRole when request.NewRole.HasValue => await ChangeUserRoleAsync(userId, request.NewRole.Value, operatedByUserId, request.Reason, cancellationToken),
                    BulkUserOperation.Delete => await DeleteUserAsync(userId, operatedByUserId, request.Reason, cancellationToken),
                    _ => false
                };

                if (success)
                {
                    response.SuccessCount++;
                }
                else
                {
                    response.FailureCount++;
                    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                    errors.Add(new BulkOperationError
                    {
                        UserId = userId,
                        UserEmail = user?.Email ?? "Unknown",
                        ErrorMessage = "Operation failed"
                    });
                }
            }
            catch (Exception ex)
            {
                response.FailureCount++;
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                errors.Add(new BulkOperationError
                {
                    UserId = userId,
                    UserEmail = user?.Email ?? "Unknown",
                    ErrorMessage = ex.Message
                });

                _logger.LogError(ex, "Error performing bulk operation on user {UserId}", userId);
            }
        }

        response.Errors = errors;
        response.Message = $"Operation completed. {response.SuccessCount} successful, {response.FailureCount} failed.";

        return response;
    }

    public async Task<UserSystemStatistics> GetUserStatisticsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user system statistics");

        var allUsers = await _userRepository.GetAllAsync(cancellationToken);
        var usersList = allUsers.ToList();

        var stats = new UserSystemStatistics
        {
            TotalUsers = usersList.Count,
            ActiveUsers = usersList.Count(u => u.IsActive),
            InactiveUsers = usersList.Count(u => !u.IsActive),
            NewUsersLast30Days = usersList.Count(u => u.CreatedAtUtc >= DateTime.UtcNow.AddDays(-30)),
            ActiveUsersLast30Days = usersList.Count(u => u.LastLoginAtUtc >= DateTime.UtcNow.AddDays(-30)),
            TotalStorageUsed = 0, // TODO: Implementar cálculo real
            GeneratedAtUtc = DateTime.UtcNow
        };

        // Distribución por roles
        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            stats.UsersByRole[role] = usersList.Count(u => u.Role == role);
        }

        return stats;
    }

    public async Task<IEnumerable<UserActivityResponse>> GetUserRecentActivityAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting recent activity for user {UserId}", userId);

        // TODO: Implementar usando el servicio de auditoría
        var recentActivity = await _auditService.GetRecentUserActivityAsync(userId, limit, cancellationToken);
        
        return recentActivity.Select(a => new UserActivityResponse
        {
            Id = a.Id,
            Action = a.Action,
            Resource = a.Resource,
            ResourceId = a.ResourceId,
            Details = a.Details,
            TimestampUtc = a.TimestampUtc,
            Success = a.Success,
            IpAddress = a.IpAddress
        });
    }

    public async Task<IEnumerable<UserRoleChangeHistory>> GetUserRoleHistoryAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting role history for user {UserId}", userId);

        // TODO: Implementar historial de roles real basado en logs de auditoría
        // Por ahora retornamos una lista vacía
        return new List<UserRoleChangeHistory>();
    }

    public async Task<bool> CanUserPerformOperationAsync(Guid operatorUserId, Guid targetUserId, UserManagementOperation operation, CancellationToken cancellationToken = default)
    {
        var operatorUser = await _userRepository.GetByIdAsync(operatorUserId, cancellationToken);
        var targetUser = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);

        if (operatorUser == null || targetUser == null)
        {
            return false;
        }

        // Los administradores pueden realizar cualquier operación
        if (operatorUser.Role == UserRole.Administrator)
        {
            return true;
        }

        // Los moderadores pueden gestionar usuarios con roles menores
        if (operatorUser.Role == UserRole.Moderator)
        {
            return targetUser.Role < UserRole.Moderator;
        }

        // Otros roles no pueden gestionar usuarios
        return false;
    }
}