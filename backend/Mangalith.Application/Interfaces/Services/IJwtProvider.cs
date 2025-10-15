using Mangalith.Domain.Entities;
using Mangalith.Application.Contracts.Auth;

namespace Mangalith.Application.Interfaces.Services;

public interface IJwtProvider
{
    AuthResponse CreateToken(User user);
}
