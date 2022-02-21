using WeatherStation;

//todo cleanup
static class Temp {

    public static JWTHandler.JWTConfig CreateJWTConfig () {
        var jwtConfig = new JWTHandler.JWTConfig () { Audience = "weatherstation.com", Issuer = "weatherstation.com" };

#if MOCK
        jwtConfig.SetRandomKeyValues ();

#else
        //todo implement: load keys from key vault via app settings
        throw new NotImplementedException ();
            //jwtConfig.PrivateKey = LoadRSAKey ("AccessTokenPrivateKey", true);
            //jwtConfig.PublicKey = LoadRSAKey ("AccessTokenPublicKey", false);
            //jwtConfig.RefreshPrivateKey = LoadRSAKey ("RefreshTokenPrivateKey", true);
            //jwtConfig.RefreshPublicKey = LoadRSAKey ("RefreshTokenPublicKey", false);

#endif
        return jwtConfig;

    }


}
