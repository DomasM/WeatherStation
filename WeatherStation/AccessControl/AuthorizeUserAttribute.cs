using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WeatherStation;

/// <summary>
/// throws if method requires authorization but userId has not been extracted to http context
/// </summary>
[AttributeUsage (AttributeTargets.Class | AttributeTargets.Method)]
public class AssertUserAuthorizedAttribute : Attribute, IAuthorizationFilter {
    public void OnAuthorization (AuthorizationFilterContext context) {
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute> ().Any ()) return;
        var userId = context.HttpContext.Items["User"] as string;
        if (string.IsNullOrWhiteSpace (userId)) {
            // not logged in
            throw new InvalidCredentialsException ();
        }
    }
}

