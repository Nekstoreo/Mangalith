namespace Mangalith.Domain.Entities;

/// <summary>
/// Entidad para rastrear el rate limiting por usuario
/// </summary>
public class RateLimitEntry
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Endpoint { get; private set; }
    public int RequestCount { get; private set; }
    public DateTime WindowStartUtc { get; private set; }
    public DateTime LastRequestUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private RateLimitEntry()
    {
        Id = Guid.Empty;
        UserId = Guid.Empty;
        Endpoint = string.Empty;
        RequestCount = 0;
        WindowStartUtc = DateTime.UtcNow;
        LastRequestUtc = DateTime.UtcNow;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public RateLimitEntry(Guid userId, string endpoint)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Endpoint = endpoint;
        RequestCount = 1;
        WindowStartUtc = DateTime.UtcNow;
        LastRequestUtc = DateTime.UtcNow;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Incrementa el contador de requests
    /// </summary>
    public void IncrementRequest()
    {
        RequestCount++;
        LastRequestUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Resetea la ventana de rate limiting
    /// </summary>
    public void ResetWindow()
    {
        RequestCount = 1;
        WindowStartUtc = DateTime.UtcNow;
        LastRequestUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica si la ventana actual ha expirado (más de 1 minuto)
    /// </summary>
    public bool IsWindowExpired()
    {
        return DateTime.UtcNow.Subtract(WindowStartUtc).TotalMinutes >= 1.0;
    }

    /// <summary>
    /// Verifica si el usuario ha excedido el límite para su rol
    /// </summary>
    public bool HasExceededLimit(UserRole role)
    {
        var limit = Constants.QuotaLimits.GetApiCallLimit(role);
        return RequestCount > limit;
    }

    /// <summary>
    /// Obtiene el número de requests restantes en la ventana actual
    /// </summary>
    public int GetRemainingRequests(UserRole role)
    {
        var limit = Constants.QuotaLimits.GetApiCallLimit(role);
        return Math.Max(0, limit - RequestCount);
    }

    /// <summary>
    /// Obtiene el tiempo restante hasta que se resetee la ventana
    /// </summary>
    public TimeSpan GetTimeUntilReset()
    {
        var elapsed = DateTime.UtcNow.Subtract(WindowStartUtc);
        var remaining = TimeSpan.FromMinutes(1) - elapsed;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}