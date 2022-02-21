using Newtonsoft.Json;
using System.Runtime.Caching;
using WeatherStation.DTO;
using WeatherStation.Models.LocationProviders;
using WeatherStation.Models.WeatherProviders;

namespace WeatherStation.Models;

public class WeatherModel {

    internal List<IWeatherProvider> WeatherProviders { get; } = new List<IWeatherProvider> ();
    internal ILocationProvider LocationProvider { get; }

    public WeatherModel (ILocationProvider locationProvider, IEnumerable<IWeatherProvider> weatherProviders) {
        LocationProvider = locationProvider;
        WeatherProviders.AddRange (weatherProviders);

    }

    ObjectCache WeatherConditionsCache { get; } = new MemoryCache ("WeatherConditions");
    ObjectCache LocationsCache { get; } = new MemoryCache ("Locations");

    public async Task<WeatherConditionsAtLocation> GetCurrentWeatherConditions (string location) {
        var useCache = true;
        var resolvedLocation = await CacheHelper.GetItemMaybeFromCache (location, LocationsCache, useCache, TimeSpan.FromHours (1), async d => await LocationProvider.Locate (d)) ?? throw new LocationNotFoundException (location);
        var weather = await GetWeatherConditionsAtLocation (resolvedLocation, useCache);
        var weathers = weather.Zip (await GetProviders ()).Select (d => new WeatherConditionsWithProviderInfo (d.First, d.Second)).ToList ();

        return new WeatherConditionsAtLocation (Location: resolvedLocation, Weather: weathers);
    }

    private async Task<List<WeatherConditions?>> GetWeatherConditionsAtLocation (Location resolvedLocation, bool useCahce) {
        var weather = await CacheHelper.GetItemMaybeFromCache (JsonConvert.SerializeObject (resolvedLocation.Coordinate), WeatherConditionsCache, useCahce, TimeSpan.FromMinutes (1), async d => {
            var getWeatherTasks = WeatherProviders.Select (d => d.GetCurrentWeatherConditions (resolvedLocation.Coordinate)).ToList ();
            try {
                await Task.WhenAll (getWeatherTasks);
            } catch (Exception ex) {
                //todo log exception
            }
            return getWeatherTasks.Select (d => d.IsFaulted ? null : d.Result).ToList ();
        }
        );
        return weather;
    }

    internal async Task<List<WeatherProviderInfo>> GetProviders () => WeatherProviders.Select (d => d.Info).ToList ();



}
