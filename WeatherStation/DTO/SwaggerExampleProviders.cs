using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace WeatherStation.DTO;


public class ProblemDetailsLocationNotFoundExample : IExamplesProvider<ProblemDetails> {
    public ProblemDetails GetExamples () {
        return new ProblemDetails () { Status = 404, Title = "Not Found", Detail = "Location chrumumrumry not found." };
    }
}
public class ProblemDetailsForbiddenExample : IExamplesProvider<ProblemDetails> {
    public ProblemDetails GetExamples () {
        return new ProblemDetails () { Status = 403, Title = "Forbidden" };
    }
}
