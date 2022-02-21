using Alba;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WeatherStation.DTO;
using Xunit;

namespace WeatherStation.Test {
    public class WeatherServiceTest : IClassFixture<WebAppFixture> {

        private IAlbaHost Host { get; }
        public WeatherServiceTest (WebAppFixture app) {
            Host = app.AlbaHost;
        }

        private string Uri => "/WeatherConditions";


        [Fact]
        public async Task ConditionsInMiami () {
            var output = await Host.GetAsJson<WeatherConditionsAtLocation> (Uri + "/Conditions?Location=Miami");
            output.Should ().NotBeNull ();
            output.Location.Should ().NotBeNull ();
            output.Weather.Count (d => d.Weather != null).Should ().BePositive ();
            var w = output.Weather.Where (d => d.Weather != null);
            w.Should ().AllSatisfy (d => d.Weather.Temperature.Should ().BeInRange (-20, 70));
            w.Should ().AllSatisfy (d => d.Weather.Humidity.Should ().BeInRange (10, 90));
            w.Should ().AllSatisfy (d => d.Weather.FeltTemperature.Should ().BeInRange (-20, 70));
            w.Should ().AllSatisfy (d => d.Weather.WindSpeed.Should ().BeInRange (0, 30));
        }


        [Theory]
        [InlineData ("Miami")]
        [InlineData ("Boulder, Colorado")]
        [InlineData ("Kamajai")]
        [InlineData ("Владивосто́к")]
        [InlineData ("المدينة")]

        public async Task LocationsShouldBeFound (string location) {

            var result = await Host.Scenario (_ => {
                _.Get.Url (Uri + "/Conditions?Location=" + location);
                _.StatusCodeShouldBeOk ();
            });
            var output = result.ReadAsJson<WeatherConditionsAtLocation> ();
            output.Location.Should ().NotBeNull ();
            output.Weather.Should ().OnlyHaveUniqueItems (d => d.ProviderInfo.Name);
            output.Weather.Should ().OnlyHaveUniqueItems (d => d.ProviderInfo.Url);
            output.Weather.Count (d => d.Weather != null).Should ().BePositive ();

        }

        [Theory]
        [InlineData ("London", "London, United Kingdom")]
        [InlineData ("المدينة", "Medina")]

        public async Task SameLocationSameResult (string location1, string location2) {

            var result1 = await Host.Scenario (_ => {
                _.Get.Url (Uri + "/Conditions?Location=" + location1);
                _.StatusCodeShouldBeOk ();
            });
            var output1 = result1.ReadAsJson<WeatherConditionsAtLocation> ();

            var result2 = await Host.Scenario (_ => {
                _.Get.Url (Uri + "/Conditions?Location=" + location2);
                _.StatusCodeShouldBeOk ();
            });
            var output2 = result1.ReadAsJson<WeatherConditionsAtLocation> ();

            output1.Should ().BeEquivalentTo (output2);
        }




        [Theory]
        [InlineData ("sefdfsdfsdjoif")]
        [InlineData ("dfijdofiduj, Colorado")]
        [InlineData (", , , ,")]
        public async Task LocationsShouldBeNotFound (string location) {

            var result = await Host.Scenario (_ => {
                _.Get.Url (Uri + "/Conditions?Location=" + location);
                _.StatusCodeShouldBe (HttpStatusCode.NotFound);
            });
            var output = result.ReadAsJson<ProblemDetails> ();
            output.Should ().NotBeNull ();
            output.Title.Should ().Be ("Not Found");
            output.Status.Should ().Be ((int)HttpStatusCode.NotFound);
            output.Detail.Should ().Contain (location);

        }

        [Fact]
        public async Task AtLeastOneProvider () {
            var output = await Host.GetAsJson<List<WeatherProviderInfo>> (Uri + "/Providers");
            output.Should ().NotBeNull ();
            output.Count.Should ().BePositive ();
        }


    }
}