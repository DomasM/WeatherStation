using Alba;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WeatherStation.DTO;
using Xunit;

namespace WeatherStation.Test {
    public class UsersServiceTest : IClassFixture<WebAppFixture> {

        private IAlbaHost Host { get; }
        public UsersServiceTest (WebAppFixture app) {
            Host = app.AlbaHost;
        }

        private string Uri => "/Users";



        [Theory]
        [InlineData ("john@gmail.com", "abc")]
        [InlineData ("pete@gmail.com", "abc")]
        public async Task UsersCanLogin (string email, string password) {

            var result = await Host.Scenario (_ => {
                _.Post.Json (new UserLoginByEmailAndPassword (email, password)).ToUrl (Uri + "/Login");
                _.StatusCodeShouldBeOk ();
            });
            var output = result.ReadAsJson<UserAccessInfo> ();
            output.Should ().NotBeNull ();
            output.AccessToken.Should ().NotBeNull ();
            output.RefreshToken.Should ().NotBeNull ();
            output.User.Should ().NotBeNull ();
            output.User.Email.Should ().BeEquivalentTo (email);
            output.AccessToken.Length.Should ().BeGreaterThan (100);
            output.RefreshToken.Length.Should ().BeGreaterThan (100);
        }


        [Theory]
        [InlineData ("john@gmail.com", "abc")]
        [InlineData ("pete@gmail.com", "abc")]
        public async Task UsersCanLoginAndModifyFavoriteLocations (string email, string password) {

            var result = await Host.Scenario (_ => {
                _.Post.Json (new UserLoginByEmailAndPassword (email, password)).ToUrl (Uri + "/Login");
                _.StatusCodeShouldBeOk ();
            });
            var output = result.ReadAsJson<UserAccessInfo> ();

            var result2 = await Host.Scenario (_ => {
                _.WithBearerToken (output.AccessToken);
                _.Post.Json (new CreateFavoriteLocation (email))
                .ToUrl (Uri + "/FavoriteLocations");//my favorite location is my email
                _.StatusCodeShouldBe (HttpStatusCode.Created);
            });
            var output2 = result2.ReadAsJson<List<FavoriteLocation>> ();
            output2.Should ().NotBeNull ();
            output2.Count (d => d.Location == email).Should ().Be (1);
        }


        [Theory]
        [InlineData ("alba@gmail.com", "felix1")]
        [InlineData ("snape@gmail.com", "draco2")]
        public async Task UsersCanRegisterLoginAndModifyFavoriteLocations (string email, string password) {

            var name = email.Split ('@')[0];
            var result0 = await Host.Scenario (_ => {
                _.Post.Json (new CreateUser (name, email, password)).ToUrl (Uri + "/Register");
                _.StatusCodeShouldBe (HttpStatusCode.Created);
            });
            var output0 = result0.ReadAsJson<User> ();
            output0.Should ().NotBeNull ();
            output0.Name.Should ().BeEquivalentTo (name);
            output0.Email.Should ().BeEquivalentTo (email);

            var result1 = await Host.Scenario (_ => {
                _.Post.Json (new UserLoginByEmailAndPassword (email, password)).ToUrl (Uri + "/Login");
                _.StatusCodeShouldBeOk ();
            });
            var output1 = result1.ReadAsJson<UserAccessInfo> ();

            var newLocation = Guid.NewGuid ().ToString ();


            var favoriteLocationBeforeResult = await Host.Scenario (_ => {
                _.WithBearerToken (output1.AccessToken);
                _.Get.Url (Uri + "/FavoriteLocations");
            });

            var favLocationsBefore = favoriteLocationBeforeResult.ReadAsJson<List<FavoriteLocation>> ();

            favLocationsBefore.Count (d => d.Location == newLocation).Should ().Be (0);

            await Host.Scenario (_ => {
                _.Get.Url (Uri + "/FavoriteLocations");
                _.StatusCodeShouldBe ((HttpStatusCode.Forbidden));
            });

            var result2 = await Host.Scenario (_ => {
                _.WithBearerToken (output1.AccessToken);
                _.Post.Json (new CreateFavoriteLocation (newLocation))
                .ToUrl (Uri + "/FavoriteLocations");
                _.StatusCodeShouldBe (HttpStatusCode.Created);
            });
            var output2 = result2.ReadAsJson<List<FavoriteLocation>> ();
            output2.Should ().NotBeNull ();
            output2.Count (d => d.Location == newLocation).Should ().Be (1);
        }


        [Theory]
        [InlineData ("john@gmail.com", "felix1")]
        [InlineData ("pete@gmail.com", "draco2")]
        public async Task CanNotRegisterWithExistingEmail (string email, string password) {
            var name = email.Split ('@')[0];
            var result = await Host.Scenario (_ => {
                _.Post.Json (new CreateUser (name, email, password)).ToUrl (Uri + "/Register");
                _.StatusCodeShouldBe (HttpStatusCode.Conflict);
            });
        }

        [Fact]
        public async Task CanNotAccessFavoriteLocationsWithBadAuth () {

            var location = Guid.NewGuid ().ToString ();
            await Host.Scenario (_ => {
                _.WithBearerToken ("dfjdifjdlfkdjlfkd");
                _.Post.Json (new CreateFavoriteLocation (location))
                .ToUrl (Uri + "/FavoriteLocations");
                _.StatusCodeShouldBe (HttpStatusCode.Forbidden);
            });
        }

        [Fact]
        public async Task CanNotAccessFavoriteLocationsWithNoAuth () {
            var location = Guid.NewGuid ().ToString ();
            await Host.Scenario (_ => {
                _.Post.Json (new CreateFavoriteLocation (location))
                .ToUrl (Uri + "/FavoriteLocations");
                _.StatusCodeShouldBe (HttpStatusCode.Forbidden);
            });
        }

        [Fact]
        public async Task CanNotReadFavoriteLocationsWithNoAuth () {
            await Host.Scenario (_ => {
                _.Get.Url (Uri + "/FavoriteLocations");
                _.StatusCodeShouldBe ((HttpStatusCode.Forbidden));
            });
        }


        [Theory]
        [InlineData ("john@gmail.com", "wrong")]
        [InlineData ("pete@gmail.com", "")]
        [InlineData ("djlkfjdlkfjdlkfdj@gmail.com", "abc")]
        public async Task UsersCanNotLogin (string email, string password) {

            var result = await Host.Scenario (_ => {
                _.Post.Json (new UserLoginByEmailAndPassword (email, password)).ToUrl (Uri + "/Login");
                _.StatusCodeShouldBe (HttpStatusCode.Forbidden);
            });
            var output = result.ReadAsJson<ProblemDetails> ();
            output.Should ().NotBeNull ();
        }




    }
}