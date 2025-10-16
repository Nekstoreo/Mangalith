using Microsoft.EntityFrameworkCore;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;
using Mangalith.Infrastructure.Data;

namespace Mangalith.Infrastructure.Repositories;

/// <summary>
/// Repositorio Entity Framework para invitaciones de usuarios
/// </summary>
public class UserInvitationRepository : IUserInvitationRepository
{
    private readonly MangalithDbContext _context;

    public UserInvitationRepository(MangalithDbContext context)
    {
        _context = context;
    }

    public async Task<UserInvitation> CreateAsync(UserInvitation invitation, CancellationToken cancellationToken = default)
    {
        _context.UserInvitations.Add(invitation);
        await _context.SaveChangesAsync(cancellationToken);
        return invitation;
    }

    public async Task<UserInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserInvitations
            .Include(i => i.InvitedBy)
            .Include(i => i.AcceptedBy)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.UserInvitations
            .Include(i => i.InvitedBy)
            .Include(i => i.AcceptedBy)
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<UserInvitation>> GetByEmailAsync(string email, bool onlyPending = true, CancellationToken cancellationToken = default)
    {
        var query = _context.UserInvitations
            .Where(i => i.Email == email.ToLowerInvariant());

        if (onlyPending)
        {
            var now = DateTime.UtcNow;
            query = query.Where(i => !i.AcceptedAtUtc.HasValue && i.ExpiresAtUtc > now);
        }

        return await query
            .Include(i => i.InvitedBy)
            .Include(i => i.AcceptedBy)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserInvitation>> GetByInviterAsync(Guid invitedByUserId, bool includeExpired = false, CancellationToken cancellationToken = default)
    {
        var query = _context.UserInvitations
            .Where(i => i.InvitedByUserId == invitedByUserId);

        if (!includeExpired)
        {
            var now = DateTime.UtcNow;
            query = query.Where(i => i.ExpiresAtUtc > now || i.AcceptedAtUtc.HasValue);
        }

        return await query
            .Include(i => i.InvitedBy)
            .Include(i => i.AcceptedBy)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserInvitation>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        return await _context.UserInvitations
            .Where(i => !i.AcceptedAtUtc.HasValue && i.ExpiresAtUtc > now)
            .Include(i => i.InvitedBy)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserInvitation> UpdateAsync(UserInvitation invitation, CancellationToken cancellationToken = default)
    {
        _context.UserInvitations.Update(invitation);
        await _context.SaveChangesAsync(cancellationToken);
        return invitation;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invitation = await _context.UserInvitations.FindAsync(new object[] { id }, cancellationToken);
        if (invitation == null)
            return false;

        _context.UserInvitations.Remove(invitation);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<long> DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiredInvitations = _context.UserInvitations
            .Where(i => !i.AcceptedAtUtc.HasValue && i.ExpiresAtUtc <= now);

        var count = await expiredInvitations.CountAsync(cancellationToken);
        
        if (count > 0)
        {
            _context.UserInvitations.RemoveRange(expiredInvitations);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return count;
    }

    public async Task<bool> ExistsPendingAsync(string email, UserRole targetRole, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        return await _context.UserInvitations
            .AnyAsync(i => i.Email == email.ToLowerInvariant() 
                          && i.TargetRole == targetRole 
                          && !i.AcceptedAtUtc.HasValue 
                          && i.ExpiresAtUtc > now, 
                     cancellationToken);
    }

    public async Task<InvitationStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.UserInvitations.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(i => i.CreatedAtUtc >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.CreatedAtUtc <= toDate.Value);

        var now = DateTime.UtcNow;

        var totalInvitations = await query.CountAsync(cancellationToken);
        var acceptedInvitations = await query.CountAsync(i => i.AcceptedAtUtc.HasValue, cancellationToken);
        var pendingInvitations = await query.CountAsync(i => !i.AcceptedAtUtc.HasValue && i.ExpiresAtUtc > now, cancellationToken);
        var expiredInvitations = await query.CountAsync(i => !i.AcceptedAtUtc.HasValue && i.ExpiresAtUtc <= now, cancellationToken);

        // Invitaciones por rol
        var invitationsByRole = await query
            .GroupBy(i => i.TargetRole)
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Role, x => (long)x.Count, cancellationToken);

        // Top invitadores
        var topInviters = await query
            .Include(i => i.InvitedBy)
            .GroupBy(i => new { i.InvitedByUserId, i.InvitedBy.Email })
            .Select(g => new { InviterEmail = g.Key.Email ?? g.Key.InvitedByUserId.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToDictionaryAsync(x => x.InviterEmail, x => (long)x.Count, cancellationToken);

        return new InvitationStatistics
        {
            TotalInvitations = totalInvitations,
            AcceptedInvitations = acceptedInvitations,
            PendingInvitations = pendingInvitations,
            ExpiredInvitations = expiredInvitations,
            InvitationsByRole = invitationsByRole,
            TopInviters = topInviters,
            FromDate = fromDate,
            ToDate = toDate,
            GeneratedAtUtc = DateTime.UtcNow
        };
    }
}