using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using WeatherStation.DTO;
using WeatherStation.Models;

namespace WeatherStation.Controllers;

[ApiController]
[Route ("[controller]")]
[Consumes ("application/json"), Produces ("application/json")]
[AssertUserAuthorized]
public class UsersController : ControllerBase {

    private readonly ILogger<UsersController> _logger;
    private UsersModel UsersModel { get; }

    public UsersController (ILogger<UsersController> logger, UsersModel usersModel) {
        _logger = logger;
        UsersModel = usersModel;
    }

    private string GetUserId () => HttpContext.Items["User"] as string ?? String.Empty;

    //todo all these 403 decorations are tedious, how to weave them in? custom conventions will be available in efcore 7.0? https://github.com/dotnet/efcore/issues/214

    [HttpGet ("FavoriteLocations")]
    [SwaggerOperation ("Returns list of favorite locations for authenticated user..")]
    [ProducesResponseType (StatusCodes.Status200OK)]
    [SwaggerResponseExample (StatusCodes.Status403Forbidden, typeof (ProblemDetailsForbiddenExample))]
    [ProducesResponseType (StatusCodes.Status403Forbidden, Type = typeof (ProblemDetails))]
    public async Task<ActionResult<List<FavoriteLocation>>> GetFavoriteLocations () {
        var locations = await UsersModel.GetFavoriteLocations (GetUserId ());
        return Ok (locations);
    }


    [HttpPost ("FavoriteLocations")]
    [SwaggerOperation ("Add new favorite location for authenticated user.")]
    [ProducesResponseType (StatusCodes.Status201Created)]
    [SwaggerResponseExample (StatusCodes.Status403Forbidden, typeof (ProblemDetailsForbiddenExample))]
    [ProducesResponseType (StatusCodes.Status403Forbidden, Type = typeof (ProblemDetails))]
    public async Task<ActionResult<List<FavoriteLocation>>> AddFavoriteLocation (CreateFavoriteLocation location) {
        await UsersModel.AddFavoriteLocation (GetUserId (), location);
        var locations = await UsersModel.GetFavoriteLocations (GetUserId ());
        return Created (HttpContext.Request.Path, locations);
    }


    //todo I will need DELETE method to to remove specific favorite location (i.e. unfavorite)


    [HttpPost ("Login"), AllowAnonymous]
    [SwaggerOperation ("Login existing user by email and password. Returns access and refresh JWT tokens on success, HTTP 403 otherwise.")]
    [ProducesResponseType (StatusCodes.Status200OK)]
    [SwaggerResponseExample (StatusCodes.Status403Forbidden, typeof (ProblemDetailsForbiddenExample))]
    [ProducesResponseType (StatusCodes.Status403Forbidden, Type = typeof (ProblemDetails))]
    public async Task<ActionResult<UserAccessInfo>> Login (UserLoginByEmailAndPassword login) {
        var result = await UsersModel.AuthorizeUser (login.Email, login.Password);
        return Ok (result);
    }


    [HttpPost ("Register"), AllowAnonymous]
    [SwaggerOperation ("Create new user with provider information. If provided email is already registered, HTTP 409 will be returned.")]
    [ProducesResponseType (StatusCodes.Status201Created)]
    [ProducesResponseType (StatusCodes.Status409Conflict, Type = typeof (ProblemDetails))]
    [ProducesResponseType (StatusCodes.Status422UnprocessableEntity, Type = typeof (ProblemDetails))]
    public async Task<ActionResult<User>> Register (CreateUser userInfo) {
        var createdUser = await UsersModel.CreateNewUser (userInfo);
        return Created (String.Empty, createdUser);
    }

}

