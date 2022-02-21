namespace WeatherStation.DTO {



    public record Location {
        public Location (GeoCoordinate Coordinate, string Country, string Region, string City) {
            this.Coordinate = Coordinate;
            this.Country = Country;
            this.Region = Region;
            this.City = City;
        }
        /// <summary>
        /// Position of location
        /// </summary>
        public GeoCoordinate Coordinate { get; }
        /// <example>US</example>
        public string Country { get; }
        /// <example>Colorado</example>
        public string Region { get; }
        /// <example>Boulder</example>
        public string City { get; }
    }

    public record GeoCoordinate {

        public GeoCoordinate (double Latitude, double Longitude) {
            this.Latitude = Latitude;
            this.Longitude = Longitude;
        }

        /// <summary>
        /// South-North position in degress (-90 to 90)
        /// </summary>
        /// <example>40.0150</example>
        public double Latitude { get; }
        /// <summary>
        /// West-East position in degress (-180 to 180)
        /// </summary>
        /// <example>-105.2705</example>
        public double Longitude { get; }
    }

    public record WeatherConditions {
        public WeatherConditions (double Temperature, double Humidity, double WindSpeed, double FeltTemperature) {
            this.Temperature = Temperature;
            this.Humidity = Humidity;
            this.WindSpeed = WindSpeed;
            this.FeltTemperature = FeltTemperature;
        }
        /// <summary>
        /// Temperature in degree Celsius.
        /// </summary>
        /// <example>23.3</example>
        public double Temperature { get; }
        /// <summary>
        /// Relative humidity in  percents.
        /// </summary>
        /// <example>42.3</example>
        public double Humidity { get; }
        /// <summary>
        /// Wind speed in meters/second
        /// </summary>
        /// <example>3.1</example>
        public double WindSpeed { get; }
        /// <summary>
        /// Temperature as felt by humans (adjustment by wind and humidity etc).
        /// </summary>
        /// <example>19.5</example>
        public double FeltTemperature { get; }
    }

    public record WeatherConditionsAtLocation (Location Location, List<WeatherConditionsWithProviderInfo> Weather);

    public record WeatherConditionsWithProviderInfo (WeatherConditions? Weather, WeatherProviderInfo ProviderInfo);


    public record WeatherProviderInfo (string Name, string Url);

    public record UserAccessInfo (string AccessToken, string RefreshToken, User User);
    public record UserLoginByEmailAndPassword (string Email, string Password);

    public record CreateUser (string Name, string Email, string Password);
    public record User (string Name, string Email);


    public record FavoriteLocation (string Location);
    public record CreateFavoriteLocation (string Location);






}
