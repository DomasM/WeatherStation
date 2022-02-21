using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace WeatherStation;

//todo do I really need to write it all myself?
//todo refresh token rotation is not implemented
public class JWTHandler {

    ITokenReplayCache UsedRefreshTokens { get; }

    JWTConfig SecretsConfig { get; }


    public string GenerateJWToken (string userId, bool isRefreshToken, TimeSpan validFor, params Claim[] claims) {
        var securityKey = new RsaSecurityKey (isRefreshToken ? SecretsConfig.RefreshPrivateKey : SecretsConfig.PrivateKey);

        var _claims = new List<Claim>
                {
                        new Claim ("sub", userId),
                        new Claim ("type", isRefreshToken ? "refresh" : "access")
                    };
        _claims.AddRange (claims);
        var tokenHandler = new JsonWebTokenHandler ();
        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity (_claims),
            Expires = DateTime.UtcNow.Add (validFor),
            Issuer = SecretsConfig.Issuer,
            Audience = SecretsConfig.Audience,
            SigningCredentials = new SigningCredentials (securityKey, SecurityAlgorithms.RsaSha256)
        };

        var token = tokenHandler.CreateToken (tokenDescriptor);
        return token;
    }



    public bool ValidateTokenReturnIsValid (string token, bool isRefreshToken) {
        try {
            ValidateTokenAndThrow (token, isRefreshToken);
            return true;
        } catch (Exception) {
            return false;
        }
    }

    public JWTHandler (ITokenReplayCache refreshTokenReplayCahce, JWTConfig jWTConfig) {

        UsedRefreshTokens = refreshTokenReplayCahce;
        SecretsConfig = jWTConfig;
    }

    public static string GetClaim (string token, string claimType) {
        var securityToken = new JwtSecurityTokenHandler ().ReadToken (token) as JwtSecurityToken;
        var stringClaimValue = securityToken?.Claims?.FirstOrDefault (claim => claim.Type == claimType)?.Value ?? String.Empty;
        return stringClaimValue;
    }


    public SecurityToken ValidateTokenAndThrow (string token, bool isRefreshToken) {
        if (string.IsNullOrWhiteSpace (token)) throw new SecurityTokenException ("empty token");
        var securityKey = new RsaSecurityKey (isRefreshToken ? SecretsConfig.RefreshPublicKey : SecretsConfig.PublicKey);
        var tokenHandler = new JwtSecurityTokenHandler ();
        tokenHandler.ValidateToken (token, new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = SecretsConfig.Issuer,
            ValidAudience = SecretsConfig.Audience,

            TryAllIssuerSigningKeys = true,
            IssuerSigningKey = securityKey,
            IssuerSigningKeys = new List<SecurityKey> () { securityKey },
            ValidateTokenReplay = isRefreshToken,
            TokenReplayCache = UsedRefreshTokens
        }, out SecurityToken validatedToken);
        if (validatedToken as JwtSecurityToken == null) throw new SecurityTokenException ("invalid token type");
        var tokenType = (validatedToken as JwtSecurityToken)?.Claims?.FirstOrDefault (claim => claim.Type == "type")?.Value ?? String.Empty;
        var requiredType = isRefreshToken ? "refresh" : "access";
        if (tokenType != requiredType) {
            throw new SecurityTokenException ("invalid token claim for type");
        }
        return validatedToken;
    }

    public class JWTConfig { //todo I guess jwt config is already somewhere in asp.net
        public string Issuer { get; set; }
        public string Audience { get; set; }

        public RSAParameters PublicKey { get; set; }
        public RSAParameters PrivateKey { get; set; }
        public RSAParameters RefreshPublicKey { get; set; }
        public RSAParameters RefreshPrivateKey { get; set; }

        public JWTConfig () { }

        public void SetRandomKeyValues () {
            var t1 = new RSACryptoServiceProvider (2048);
            var t2 = new RSACryptoServiceProvider (2048);
            PublicKey = t1.ExportParameters (false);
            PrivateKey = t1.ExportParameters (true);
            RefreshPublicKey = t2.ExportParameters (false);
            RefreshPrivateKey = t2.ExportParameters (true);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="req">incoming request</param>
    /// <returns>whether token is valid and token itself of empty string</returns>
    public Tuple<bool, string> AuthAccessToken (HttpRequest req) {
        var authHeader = "Authorization";
        if (req.Headers.ContainsKey (authHeader) == false) return Tuple.Create (false, string.Empty);
        var auth = req.Headers.First (d => d.Key == authHeader);
        var bToken = auth.Value.First ();
        if (bToken == null) return Tuple.Create (false, string.Empty);
        if (bToken.StartsWith ("Bearer ") == false) return Tuple.Create (false, string.Empty);
        var splits = bToken.Split (" ", StringSplitOptions.RemoveEmptyEntries).Select (d => d.Trim ()).ToList ();
        if (splits.Count != 2) return Tuple.Create (false, string.Empty);
        var token = splits[1];
        var isValid = ValidateTokenReturnIsValid (token, false);
        return Tuple.Create (isValid, token);
    }




}



