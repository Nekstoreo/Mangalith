using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Contracts.Auth;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;

namespace Mangalith.Infrastructure.Auth;

public class JwtProvider : IJwtProvider
{
    private readonly JwtSettings _settings;
    private readonly IPermissionService _permissionService;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtProvider(IOptions<JwtSettings> settings, IPermissionService permissionService)
    {
        _settings = settings.Value;
        _permissionService = permissionService;
    }

    public async Task<AuthResponse> CreateTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        var expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Agregar permisos del usuario al token
        try
        {
            var userPermissions = await _permissionService.GetUserPermissionsAsync(user.Id, cancellationToken);
            var permissionsArray = userPermissions.ToArray();
            
            // Comprimir permisos para mantener el tamaño del token manejable
            // Usar una sola claim con permisos separados por comas
            if (permissionsArray.Length > 0)
            {
                var permissionsString = string.Join(",", permissionsArray);
                claims.Add(new Claim("permissions", permissionsString));
            }
        }
        catch (Exception)
        {
            // Si falla la obtención de permisos, continuar sin ellos
            // El sistema puede funcionar con verificación de permisos en el backend
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var accessToken = _tokenHandler.WriteToken(token);
        return new AuthResponse(accessToken, expires, user.Email, user.FullName);
    }

    [Obsolete("Use CreateTokenAsync instead")]
    public AuthResponse CreateToken(User user)
    {
        return CreateTokenAsync(user).GetAwaiter().GetResult();
    }
}
