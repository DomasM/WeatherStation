using Microsoft.AspNetCore.Authorization;

namespace WeatherStation;

/// <summary>
/// validates access token from Authorization header and extracts userId to HttpContext.Items["User"]
/// </summary>
public class JWTExtractUserMiddleware {
    public JWTExtractUserMiddleware (RequestDelegate next, JWTHandler accessControl) {
        _next = next;
        JWTHandler = accessControl;
    }

    public async Task Invoke (HttpContext context) {
        var endpoint = context.GetEndpoint ();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous> () is object == false) {

            (var isValid, var token) = JWTHandler.AuthAccessToken (context.Request);
            if (isValid) {
                var user = JWTHandler.GetClaim (token, "sub");
                if (string.IsNullOrWhiteSpace (user) == false) context.Items["User"] = user;
            }

        }
        await _next (context);
    }
    private readonly RequestDelegate _next;
    JWTHandler JWTHandler { get; }
}

