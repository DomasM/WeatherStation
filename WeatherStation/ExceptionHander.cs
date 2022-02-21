using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace WeatherStation;

internal class LocationNotFoundException : Exception {

    public LocationNotFoundException () { }
    public LocationNotFoundException (string message) : base (message) { }
}

internal class InvalidCredentialsException : Exception { }

internal class DuplicateUserException : Exception { }


public class ExceptionHander {
    public async Task HandleException (HttpContext context) {
        if (context.Response.StatusCode == StatusCodes.Status400BadRequest) return;


        context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;


        var exPath = context.Features.Get<IExceptionHandlerPathFeature> ();

        switch (exPath?.Error) {
            case LocationNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                break;

            case InvalidCredentialsException:
            case Microsoft.IdentityModel.Tokens.SecurityTokenException:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                break;

            case DuplicateUserException:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                break;
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        if (exPath?.Error != null) await WriteProblemDetails (context, exPath.Error);

    }

    private static async Task WriteProblemDetails (HttpContext context, Exception ex) {
        var msg = (Activator.CreateInstance (ex.GetType ()) as Exception)?.Message == ex.Message ? null : ex.Message;
        await context.Response.WriteAsJsonAsync (new ProblemDetails {
            Title = ReasonPhrases.GetReasonPhrase (context.Response.StatusCode),
            Status = context.Response.StatusCode,
            Detail = msg
        }); ;
    }
}
