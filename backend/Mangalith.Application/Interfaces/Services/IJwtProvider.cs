using Mangalith.Domain.Entities;
using Mangalith.Application.Contracts.Auth;

namespace Mangalith.Application.Interfaces.Services;

public interface IJwtProvider
{
    /// <summary>
    /// Crea un token JWT con claims de rol y permisos para el usuario
    /// </summary>
    /// <param name="user">Usuario para el cual crear el token</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Respuesta de autenticación con el token JWT</returns>
    Task<AuthResponse> CreateTokenAsync(User user, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Crea un token JWT (método síncrono - obsoleto)
    /// </summary>
    /// <param name="user">Usuario para el cual crear el token</param>
    /// <returns>Respuesta de autenticación con el token JWT</returns>
    [Obsolete("Use CreateTokenAsync instead")]
    AuthResponse CreateToken(User user);
}
