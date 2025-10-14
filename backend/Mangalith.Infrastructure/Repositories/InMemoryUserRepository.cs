using System.Collections.Concurrent;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Entities;

namespace Mangalith.Infrastructure.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _usersByEmail.TryGetValue(email, out var user);
        return Task.FromResult(user);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _usersByEmail.TryAdd(user.Email, user);
        return Task.CompletedTask;
    }
}
