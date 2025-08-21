using GK.Talks.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GK.Talks.Api.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var sre = ex as SpeakerRegistrationException ?? SpeakerRegistrationException.FromUnexpected(ex);

            int status;

            if (ex is SpeakerRegistrationException domainEx)
            {
                sre = domainEx;
                status = sre.StatusCode;
            }
            else if (ex is ArgumentException argEx)
            {
                sre = SpeakerRegistrationException.FromArgument(argEx);
                status = StatusCodes.Status422UnprocessableEntity;
            }
            else if (ex is InvalidOperationException invEx)
            {
                sre = SpeakerRegistrationException.FromInvalidOperation(invEx);
                status = sre.StatusCode;
            }
            else if (ex is ValidationException valEx && valEx.Value is IDictionary<string, string[]?> errors)
            {
                sre = SpeakerRegistrationException.FromValidation(errors);
                status = sre.StatusCode;
            }
            else
            {
                sre = SpeakerRegistrationException.FromUnexpected(ex);
                status = sre.StatusCode;
            }

            var pd = new ProblemDetails
            {
                Title = sre.Title,
                Detail = sre.Message,
                Status = status
            };

            if (sre.Errors != null)
                pd.Extensions["errors"] = sre.Errors;
            if (sre.Details != null)
                pd.Extensions["details"] = sre.Details;

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(pd));
        }
    }
}