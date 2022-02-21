using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WeatherStation;

/// <summary>
/// add correct method-level authorization info in swagger definition
/// </summary>
public class SwaggerSecurityScheme : IOperationFilter {
    OpenApiSecurityRequirement SecurityReq { get; }
    public SwaggerSecurityScheme (OpenApiSecurityRequirement securityReq) {
        SecurityReq = securityReq;
    }
    public void Apply (OpenApiOperation operation, OperationFilterContext context) {
        if (!context.MethodInfo.GetCustomAttributes (true).Any (x => x is AllowAnonymousAttribute) &&
        !context.MethodInfo.DeclaringType.GetCustomAttributes (true).Any (x => x is AllowAnonymousAttribute) &&
        (context.MethodInfo.GetCustomAttributes (true).Any (x => x is AssertUserAuthorizedAttribute) || context.MethodInfo.DeclaringType.GetCustomAttributes (true).Any (x => x is AssertUserAuthorizedAttribute))
        ) {
            operation.Security.Add (SecurityReq);
        } else {
            //allow anon
        }

    }
}
