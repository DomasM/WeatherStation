using WeatherStation.DTO;

namespace WeatherStation.Models.LocationProviders;
public interface ILocationProvider {
    Task<Location> Locate (string query);
}
