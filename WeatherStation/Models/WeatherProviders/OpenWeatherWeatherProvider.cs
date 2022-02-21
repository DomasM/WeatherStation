
using Flurl;
using Flurl.Http;
using WeatherStation.DTO;

namespace WeatherStation.Models.WeatherProviders;

internal class OpenWeatherWeatherProvider : IWeatherProvider {
    private string APIKey { get; }

    public OpenWeatherWeatherProvider (string apiKey) {
        APIKey = apiKey;
    }

    public WeatherProviderInfo Info => new WeatherProviderInfo ("OpenWeather", "https://openweathermap.org/");

    private static string APIUrl = @"https://api.openweathermap.org/data/2.5/";


    public async Task<WeatherConditions> GetCurrentWeatherConditions (GeoCoordinate coordinate) {

        var weather = await APIUrl
    .AppendPathSegment ("weather")
    .SetQueryParams (new { appid = APIKey, lat = coordinate.Latitude, lon = coordinate.Longitude, units = "metric" })
    .WithTimeout (TimeSpan.FromSeconds (5))
    .GetAsync ()
    .ReceiveJson<WeatherData> ();

        return new WeatherConditions (Temperature: weather.main.temp, Humidity: weather.main.humidity, WindSpeed: weather.wind.speed, FeltTemperature: weather.main.feels_like);
    }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private class Coord {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    private class Weather {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    private class Main {
        public double temp { get; set; }
        public double feels_like { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
        public double pressure { get; set; }
        public double humidity { get; set; }
    }

    private class Wind {
        public double speed { get; set; }
        public double deg { get; set; }
    }

    private class Clouds {
        public double all { get; set; }
    }

    private class Sys {
        public int type { get; set; }
        public int id { get; set; }
        public double message { get; set; }
        public string country { get; set; }
        public double sunrise { get; set; }
        public double sunset { get; set; }
    }

    private class WeatherData {
        public Coord coord { get; set; }
        public List<Weather> weather { get; set; }
        public string @base { get; set; }
        public Main main { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public double dt { get; set; }
        public Sys sys { get; set; }
        public double timezone { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
