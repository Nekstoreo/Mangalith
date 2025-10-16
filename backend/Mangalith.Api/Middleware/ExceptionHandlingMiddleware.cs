using System.Net;
using System.Text.Json;
using Mangalith.Api.Contracts;
using Mangalith.Application.Common.Exceptions;

namespace Mangalith.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationAppException validationException)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, validationException.Code, validationException.Message, validationException.Failures);
        }
        catch (UnauthorizedAppException unauthorizedException)
        {
            LogSecurityEvent(context, "Unauthorized access attempt", unauthorizedException.Message);
            await WriteProblemAsync(context, HttpStatusCode.Unauthorized, unauthorizedException.Code, unauthorizedException.Message);
        }
        catch (ForbiddenAppException forbiddenException)
        {
            LogSecurityEvent(context, "Forbidden access attempt", $"Permission: {forbiddenException.RequiredPermission}, Resource: {forbiddenException.Resource}");
            await WriteProblemAsync(context, HttpStatusCode.Forbidden, forbiddenException.Code, forbiddenException.Message);
        }
        catch (InvitationExpiredException invitationExpiredException)
        {
            _logger.LogWarning("Attempt to use expired invitation. Expired at: {ExpiredAt}", invitationExpiredException.ExpiredAt);
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, invitationExpiredException.Code, invitationExpiredException.Message);
        }
        catch (InvitationAlreadyAcceptedException invitationAcceptedException)
        {
            _logger.LogWarning("Attempt to use already accepted invitation. Accepted at: {AcceptedAt} by user: {UserId}", 
                invitationAcceptedException.AcceptedAt, invitationAcceptedException.AcceptedByUserId);
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, invitationAcceptedException.Code, invitationAcceptedException.Message);
        }
        catch (InvitationNotFoundException invitationNotFoundException)
        {
            LogSecurityEvent(context, "Invalid invitation token used", $"Token: {invitationNotFoundException.Token}");
            await WriteProblemAsync(context, HttpStatusCode.NotFound, invitationNotFoundException.Code, invitationNotFoundException.Message);
        }
        catch (InsufficientPrivilegesForInvitationException insufficientPrivilegesException)
        {
            LogSecurityEvent(context, "Insufficient privileges for invitation creation", 
                $"Required: {insufficientPrivilegesException.RequiredRole}, Current: {insufficientPrivilegesException.UserRole}");
            await WriteProblemAsync(context, HttpStatusCode.Forbidden, insufficientPrivilegesException.Code, insufficientPrivilegesException.Message);
        }
        catch (ConflictAppException conflictException)
        {
            await WriteProblemAsync(context, HttpStatusCode.Conflict, conflictException.Code, conflictException.Message);
        }
        catch (QuotaExceededException quotaException)
        {
            await WriteProblemAsync(context, HttpStatusCode.TooManyRequests, quotaException.Code, quotaException.Message);
        }
        catch (RateLimitExceededException rateLimitException)
        {
            context.Response.Headers["Retry-After"] = rateLimitException.RetryAfter.TotalSeconds.ToString("F0");
            await WriteProblemAsync(context, HttpStatusCode.TooManyRequests, rateLimitException.Code, rateLimitException.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "internal_error", "An unexpected error has occurred.");
        }
    }

    private void LogSecurityEvent(HttpContext context, string eventType, string details)
    {
        var userId = context.User?.Identity?.Name ?? "Anonymous";
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var endpoint = $"{context.Request.Method} {context.Request.Path}";

        _logger.LogWarning("Security Event: {EventType} | User: {UserId} | IP: {IpAddress} | Endpoint: {Endpoint} | Details: {Details} | UserAgent: {UserAgent}",
            eventType, userId, ipAddress, endpoint, details, userAgent);
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string code, string message, IDictionary<string, string[]>? errors = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = new ErrorResponse(code, message, errors);
        await JsonSerializer.SerializeAsync(context.Response.Body, payload, SerializerOptions);
    }
}
