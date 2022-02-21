using Microsoft.IdentityModel.Tokens;

namespace WeatherStation;
class InMemoryTokenReplayCache : ITokenReplayCache {

    HashSet<string> UsedRefreshTokens { get; } = new HashSet<string> ();

    public bool TryAdd (string securityToken, DateTime expiresOn) {
        return UsedRefreshTokens.Add (securityToken);
    }

    public bool TryFind (string securityToken) {
        return UsedRefreshTokens.Contains (securityToken);
    }
}
