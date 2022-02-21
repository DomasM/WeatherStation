
using Flurl;
using Flurl.Http;
using WeatherStation.DTO;

namespace WeatherStation.Models.LocationProviders;

internal class OpenWeatherLocationProvider : ILocationProvider {


    private string APIKey { get; }
    private string APIUrl = @"https://api.openweathermap.org/geo/1.0/";

    public OpenWeatherLocationProvider (string apiKey) {
        APIKey = apiKey;
    }



    public async Task<Location> Locate (string query) {
        List<LocationData>? locations = null;
        try {
            locations = await APIUrl
        .AppendPathSegment ("direct")
        .SetQueryParams (new { appid = APIKey, q = query, limit = 1 })
        .WithTimeout (TimeSpan.FromSeconds (4))
        .GetAsync ()
        .ReceiveJson<List<LocationData>> ();
        } catch (Exception ex) {
            //todo log original exception, I am eating up everything here which is optimistic regards to 404
            throw new LocationNotFoundException ("Location " + query + " not found.");
        }


        var match = locations?.FirstOrDefault ();
        if (match == null) throw new LocationNotFoundException ("Location " + query + " not found.");
        return new Location (new GeoCoordinate (match.lat, match.lon), match.country, match.state, match.name);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private class LocationData {
        public string name { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string country { get; set; }
        public string state { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
