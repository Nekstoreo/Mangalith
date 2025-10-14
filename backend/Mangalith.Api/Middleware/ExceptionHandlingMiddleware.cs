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
            await WriteProblemAsync(context, HttpStatusCode.Unauthorized, unauthorizedException.Code, unauthorizedException.Message);
        }
        catch (ConflictAppException conflictException)
        {
            await WriteProblemAsync(context, HttpStatusCode.Conflict, conflictException.Code, conflictException.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "internal_error", "An unexpected error has occurred.");
        }
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
