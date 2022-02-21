using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using WeatherStation;
using WeatherStation.DTO;
using WeatherStation.Models;
using WeatherStation.Models.LocationProviders;
using WeatherStation.Models.WeatherProviders;

var builder = WebApplication.CreateBuilder (args);


var openWeatherAPIKey = builder.Configuration.GetValue<string> ("OpenWeather:APIKey");

var jwtHandler = new JWTHandler (new InMemoryTokenReplayCache (), Temp.CreateJWTConfig ());
var services = builder.Services;

services.AddSingleton (typeof (JWTHandler), jwtHandler);
services.AddSingleton (typeof (UsersModel), new UsersModel (jwtHandler));

services.AddSingleton (typeof (ILocationProvider), new OpenWeatherLocationProvider (openWeatherAPIKey));

services.AddSingleton (typeof (IWeatherProvider), new OpenWeatherWeatherProvider (openWeatherAPIKey));
services.AddSingleton (typeof (IWeatherProvider), typeof (AstrologyWeatherProvider));
//register other weather providers here, none others are implemented atm

services.AddSingleton (typeof (WeatherModel));
services.AddApplicationInsightsTelemetry (); //todo add "ApplicationInsights": { "InstrumentationKey": "putinstrumentationkeyhere" }, to appsettings.json

services.AddMvc ().AddFluentValidation (d => d.RegisterValidatorsFromAssemblyContaining<CreateUserValidator> ());
services.AddFluentValidationRulesToSwagger ();

services.AddControllers ();

services.AddHttpLogging (logging => {
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add ("X-Request-Header");
    logging.ResponseHeaders.Add ("X-Response-Header");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});


services.AddEndpointsApiExplorer ();

services.AddSwaggerGen (options => {
    // Set the comments path for the XmlComments file.
    string xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Name}.xml";
    string xmlPath = Path.Combine (AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments (xmlPath);
    options.ExampleFilters ();
    options.EnableAnnotations ();
    options.AddSecurityDefinition ("Bearer", new OpenApiSecurityScheme {
        Description =
        "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.OperationFilter<SwaggerSecurityScheme> (new OpenApiSecurityRequirement ()
    {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            },
            Scheme = "oauth2",
            Name = "Bearer",
            In = ParameterLocation.Header,
        },
        new List<string>()
    }
});
});

services.AddSwaggerExamplesFromAssemblyOf<ProblemDetailsLocationNotFoundExample> ();


var app = builder.Build ();




if (app.Environment.IsDevelopment ()) {
    app.UseSwagger ();
    app.UseSwaggerUI ();
}


//same exception handler for both dev and prod, todo dev would like to have stack traces in responses
app.UseExceptionHandler (exceptionHandlerApp => {
    exceptionHandlerApp.Run (async context => await new ExceptionHander ().HandleException (context));
});


app.UseMiddleware<JWTExtractUserMiddleware> (jwtHandler);//extract user id from jwt and put in httpcontext.Items["User"]


app.UseHttpsRedirection ();

app.UseAuthorization ();

app.MapControllers ();

app.UseHsts ();

app.Run ();


public partial class Program { }
