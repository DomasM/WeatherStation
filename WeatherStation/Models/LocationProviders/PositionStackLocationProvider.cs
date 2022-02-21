


using Flurl;
using Flurl.Http;
using WeatherStation.DTO;

namespace WeatherStation.Models.LocationProviders;


[Obsolete ("This API is down since start of February", true)]
internal class PositionStackLocationProvider {

    public PositionStackLocationProvider (string apiKey) {
        APIKey = apiKey;
    }
    private string APIKey { get; }

    private string APIUrl => @"http://api.positionstack.com/v1/";

    public async Task<Location> Locate (string query) {

        var locations = await APIUrl
    .AppendPathSegment ("forward")
    .SetQueryParams (new { access_key = APIKey, query, limit = 1 })
    .WithTimeout (TimeSpan.FromSeconds (5))
    .GetAsync ()
    .ReceiveJson<ForwardLocationData> ();

        if (locations?.data?.results?.Any () == false) throw new LocationNotFoundException (query);

        var match = locations.data.results.First ();
        return new Location (new GeoCoordinate (match.latitude, match.longitude), match.country, match.region, match.name);
    }

    private class ForwardLocationData {
        internal LocationsData? data { get; set; }
    }

    private class LocationsData {
        internal List<LocationData>? results { get; set; } = new List<LocationData> ();
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private class LocationData {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string label { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string number { get; set; }
        public string street { get; set; }
        public string postal_code { get; set; }
        public int confidence { get; set; }
        public string region { get; set; }
        public string region_code { get; set; }
        public object administrative_area { get; set; }
        public string neighbourhood { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public string map_url { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
