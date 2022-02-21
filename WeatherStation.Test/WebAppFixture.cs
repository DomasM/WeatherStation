using Alba;
using System.Threading.Tasks;
using Xunit;

namespace WeatherStation.Test {
    public class WebAppFixture : IAsyncLifetime {
        public IAlbaHost AlbaHost = null!;

        public async Task InitializeAsync () {
            AlbaHost = await Alba.AlbaHost.For<global::Program> (x => {
                x.ConfigureServices ((context, services) => {
                });
            });
        }

        public async Task DisposeAsync () {
            await AlbaHost.DisposeAsync ();
        }
    }
}