using System.Net;

namespace GK.Talks.Core.Exceptions;
public class SpeakerRegistrationException(string title, string message, int statusCode = 400, object? details = null, IDictionary<string, string[]?>? errors = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
    public object? Details { get; } = details;
    public IDictionary<string, string[]?>? Errors { get; } = errors;

    public static SpeakerRegistrationException FromArgument(ArgumentException ex)
        => new(
            title: "Invalid argument",
            message: ex.Message,
            statusCode: (int)HttpStatusCode.BadRequest,
            details: new { ex.ParamName });

    public static SpeakerRegistrationException FromInvalidOperation(InvalidOperationException ex)
        => new(
            title: "Invalid operation",
            message: ex.Message,
            statusCode: (int)HttpStatusCode.Conflict);

    public static SpeakerRegistrationException FromValidation(IDictionary<string, string[]?> errors)
        => new(
            title: "Validation failed",
            message: "One or more validation errors occurred.",
            statusCode: (int)HttpStatusCode.BadRequest,
            errors: errors);

    public static SpeakerRegistrationException FromUnexpected(Exception ex)
        => new(
            title: "Unexpected error",
            message: ex.Message,
            statusCode: (int)HttpStatusCode.InternalServerError);
}