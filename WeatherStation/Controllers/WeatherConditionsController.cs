using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using WeatherStation.DTO;
using WeatherStation.Models;

namespace WeatherStation.Controllers;

[ApiController]
[AllowAnonymous]
[Route ("[controller]")]
[Consumes ("application/json"), Produces ("application/json")]
public class WeatherConditionsController : ControllerBase {

    private readonly ILogger<WeatherConditionsController> _logger;
    WeatherModel WeatherModel { get; }

    public WeatherConditionsController (ILogger<WeatherConditionsController> logger, WeatherModel weatherModel) {
        _logger = logger;
        WeatherModel = weatherModel;
    }



    //[HttpGet ("Locate")]
    //[ProducesResponseType (StatusCodes.Status200OK)]
    //[ProducesResponseType (StatusCodes.Status404NotFound, Type = typeof (ProblemDetails))]
    //public async Task<ActionResult<Location>> LocateAsync (string location) {
    //    var resolvedLocation = await new OpenWeatherLocationProvider ().Locate (location);
    //    return Ok (resolvedLocation);
    //}


    [HttpGet ("Conditions")]
    [SwaggerOperation ("Returns current weather conditions at free-form specified location. Weather conditions are aggregated from multiple weather providers. Some providers might be unable to provide weather conditions for specific location or due downtime, weather property will be null for them.")]
    [ProducesResponseType (StatusCodes.Status200OK)]
    [SwaggerResponseExample (StatusCodes.Status404NotFound, typeof (ProblemDetailsLocationNotFoundExample))]
    [ProducesResponseType (StatusCodes.Status404NotFound, Type = typeof (ProblemDetails))]
    public async Task<ActionResult<WeatherConditionsAtLocation>> GetCurrentWeatherConditions (string location) {
        var conditions = await WeatherModel.GetCurrentWeatherConditions (location);
        return Ok (conditions);
    }


    [HttpGet ("Providers")]
    [SwaggerOperation ("Returns all available weather providers.")]
    [ProducesResponseType (StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WeatherProviderInfo>>> GetWeatherProviders () {
        var providers = await WeatherModel.GetProviders ();
        return Ok (providers);
    }
}


