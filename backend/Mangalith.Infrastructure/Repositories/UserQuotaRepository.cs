using Microsoft.EntityFrameworkCore;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Constants;
using Mangalith.Domain.Entities;
using Mangalith.Infrastructure.Data;

namespace Mangalith.Infrastructure.Repositories;

public class UserQuotaRepository : IUserQuotaRepository
{
    private readonly MangalithDbContext _context;

    public UserQuotaRepository(MangalithDbContext context)
    {
        _context = context;
    }

    public async Task<UserQuota?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserQuotas
            .Include(uq => uq.User)
            .FirstOrDefaultAsync(uq => uq.UserId == userId, cancellationToken);
    }

    public async Task<UserQuota> CreateAsync(UserQuota userQuota, CancellationToken cancellationToken = default)
    {
        _context.UserQuotas.Add(userQuota);
        await _context.SaveChangesAsync(cancellationToken);
        return userQuota;
    }

    public async Task<UserQuota> UpdateAsync(UserQuota userQuota, CancellationToken cancellationToken = default)
    {
        _context.UserQuotas.Update(userQuota);
        await _context.SaveChangesAsync(cancellationToken);
        return userQuota;
    }

    public async Task<IEnumerable<UserQuota>> GetUsersNearLimitAsync(double thresholdPercentage = 80.0, CancellationToken cancellationToken = default)
    {
        var userQuotas = await _context.UserQuotas
            .Include(uq => uq.User)
            .ToListAsync(cancellationToken);

        return userQuotas.Where(uq => 
        {
            var percentage = uq.GetStorageUsagePercentage(uq.User.Role);
            return percentage >= thresholdPercentage && percentage < 100.0;
        });
    }

    public async Task<IEnumerable<UserQuota>> GetUsersExceedingLimitAsync(CancellationToken cancellationToken = default)
    {
        var userQuotas = await _context.UserQuotas
            .Include(uq => uq.User)
            .ToListAsync(cancellationToken);

        return userQuotas.Where(uq => uq.HasExceededStorageQuota(uq.User.Role));
    }

    public async Task<long> GetTotalStorageUsageAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserQuotas
            .SumAsync(uq => uq.StorageUsedBytes, cancellationToken);
    }

    public async Task<Dictionary<UserRole, long>> GetStorageUsageByRoleAsync(CancellationToken cancellationToken = default)
    {
        var result = await _context.UserQuotas
            .Include(uq => uq.User)
            .GroupBy(uq => uq.User.Role)
            .Select(g => new { Role = g.Key, TotalStorage = g.Sum(uq => uq.StorageUsedBytes) })
            .ToListAsync(cancellationToken);

        var dictionary = new Dictionary<UserRole, long>();
        
        // Inicializar todos los roles con 0
        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            dictionary[role] = 0;
        }

        // Llenar con los valores reales
        foreach (var item in result)
        {
            dictionary[item.Role] = item.TotalStorage;
        }

        return dictionary;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserQuotas.CountAsync(cancellationToken);
    }
}