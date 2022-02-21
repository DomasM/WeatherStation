using WeatherStation.DTO;

namespace WeatherStation.Models.WeatherProviders;

internal class AstrologyWeatherProvider : IWeatherProvider {
    public WeatherProviderInfo Info => new WeatherProviderInfo ("Optimistic Astrology", "https://www.astrology-online.com/");



    public async Task<WeatherConditions> GetCurrentWeatherConditions (GeoCoordinate coordinate) {

        if (coordinate.Longitude < -20) throw new TimeoutException ("");

        return new WeatherConditions (Temperature: coordinate.Latitude - coordinate.Longitude, Humidity: DateTime.Now.Hour + 10.5d, WindSpeed: Math.Abs (coordinate.Latitude / 10d), FeltTemperature: coordinate.Latitude - coordinate.Longitude + DateTime.Now.Hour / 12d - 3d);
    }

}
