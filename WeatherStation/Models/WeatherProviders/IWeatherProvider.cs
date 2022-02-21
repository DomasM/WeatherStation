using WeatherStation.DTO;

namespace WeatherStation.Models.WeatherProviders;
public interface IWeatherProvider {
    Task<WeatherConditions> GetCurrentWeatherConditions (GeoCoordinate coordinate);
    WeatherProviderInfo Info { get; }
}
