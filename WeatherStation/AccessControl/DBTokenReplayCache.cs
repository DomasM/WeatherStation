using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace WeatherStation;

[Obsolete ("not tested", true)]
class DBTokenReplayCache : ITokenReplayCache {

    public DBTokenReplayCache (DbContext dbContainer) {
        DBContext = dbContainer;
    }


    public bool TryAdd (string securityToken, DateTime expiresOn) {
        var id = ComputeSHA256Hash (securityToken);
        var item = new DBRefreshTokenEntry { id = id, ttl = (int)Math.Ceiling ((expiresOn - DateTime.UtcNow).TotalSeconds) };
        DBContext.Add (item);
        DBContext.SaveChanges ();
        return true;
    }

    public bool TryFind (string securityToken) {
        var id = ComputeSHA256Hash (securityToken);
        var result = DBContext.Find<DBRefreshTokenEntry> (new object[] { id });
        return result != null;
    }

    DbContext DBContext { get; }

    private static string ComputeSHA256Hash (string text) {
        using (var sha256 = SHA256.Create ()) {
            return BitConverter.ToString (sha256.ComputeHash (Encoding.UTF8.GetBytes (text))).Replace ("-", "");
        }
    }

    class DBRefreshTokenEntry {
        public string id { get; set; }
        public int ttl { get; set; }
    }
}

