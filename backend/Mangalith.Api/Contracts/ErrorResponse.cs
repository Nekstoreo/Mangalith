namespace Mangalith.Api.Contracts;

public record ErrorResponse(string Code, string Message, IDictionary<string, string[]>? Errors = null);
