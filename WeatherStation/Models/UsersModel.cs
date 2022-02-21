using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WeatherStation.DB;
using WeatherStation.DTO;

namespace WeatherStation.Models;

public class UsersModel {

    public UsersModel (JWTHandler accessControl) {
#if MOCK
        MockData ();
#endif
        AccessControl = accessControl;
    }
    JWTHandler AccessControl { get; }


    private void MockData () {
        var password = "abc"; //same password for all mock users
        var names = new List<string> { "John", "Pete", "Angel" };
        using (var db = new UsersDbContext ()) {
            foreach (var name in names) {
                db.Users.Add (new UserDB { id = GenerateID (), Name = name, Email = name.ToLowerInvariant () + @"@gmail.com", SaltedHashedPass = HashPassword (password) });
            }
            db.SaveChanges ();
            db.FavoriteLocations.Add (new FavoriteLocationDB { id = GenerateID (), UserId = db.Users.First ().id, Location = "Miami" });
            db.FavoriteLocations.Add (new FavoriteLocationDB { id = GenerateID (), UserId = db.Users.First ().id, Location = "Boulder, Colorado" });
            db.FavoriteLocations.Add (new FavoriteLocationDB { id = GenerateID (), UserId = db.Users.First ().id, Location = "المدينة" });
            db.SaveChanges ();
        }
    }


    private UserAccessInfo CreateNewAccessInfo (string userId, User userInfo) {
        var refreshId = Nanoid.Nanoid.Generate ();
        var ac = AccessControl;
        return new UserAccessInfo (
           AccessToken: ac.GenerateJWToken (userId, false, TimeSpan.FromMinutes (5)),
           RefreshToken: ac.GenerateJWToken (userId, true, TimeSpan.FromDays (1), new Claim ("refreshId", refreshId)),
           User: userInfo
        );
    }

    public async Task<User> CreateNewUser (CreateUser user) {
        using (var db = new UsersDbContext ()) {
            var sameEmail = db.Users.Any (d => d.Email == user.Email);
            //todo password and other provided info validation
            if (string.IsNullOrWhiteSpace (user.Password) || user.Password.Length < 4) throw new InvalidCredentialsException ();//todo better exception type
            if (sameEmail) throw new DuplicateUserException ();
            var userDb = new UserDB { id = Nanoid.Nanoid.Generate (), Name = user.Name, Email = user.Email, SaltedHashedPass = HashPassword (user.Password) };
            await db.Users.AddAsync (userDb);
            await db.SaveChangesAsync ();
            return new User (userDb.Name, userDb.Email);
        }
    }

    private string GenerateID () => Nanoid.Nanoid.Generate (size: 21);
    BCrypt.Net.HashType PasswordHashType => BCrypt.Net.HashType.SHA512;

    private string HashPassword (string password) {
        return BCrypt.Net.BCrypt.EnhancedHashPassword (password, 12, PasswordHashType);
    }

    public async Task<UserAccessInfo> AuthorizeUser (string email, string pass) {
        using (var db = new UsersDbContext ()) {
            var user = await db.Users.FirstOrDefaultAsync (d => d.Email == email);
            if (user == null) throw new InvalidCredentialsException ();
            if (string.IsNullOrEmpty (pass) || BCrypt.Net.BCrypt.EnhancedVerify (pass, user.SaltedHashedPass, PasswordHashType) == false) throw new InvalidCredentialsException ();
            return CreateNewAccessInfo (user.id, new User (user.Name, user.Email));
        }
    }

    public async Task<List<FavoriteLocation>> GetFavoriteLocations (string userId) {
        using (var db = new UsersDbContext ()) {
            var favoriteLocations = db.FavoriteLocations.Where (d => d.UserId == userId);
            return favoriteLocations.Select (d => new FavoriteLocation (d.Location)).ToList ();
        }
    }


    public async Task AddFavoriteLocation (string userId, CreateFavoriteLocation location) {
        var locations = await GetFavoriteLocations (userId);
        if (locations.Any (d => d.Location == location.Location) == false) {
            using (var db = new UsersDbContext ()) {
                db.FavoriteLocations.Add (new FavoriteLocationDB { id = Nanoid.Nanoid.Generate (), Location = location.Location, UserId = userId });
                await db.SaveChangesAsync ();
            }
        }
    }



}

