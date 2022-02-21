using Microsoft.EntityFrameworkCore;

namespace WeatherStation.DB;
public class UsersDbContext : DbContext {
    public DbSet<UserDB> Users { get; set; }
    public DbSet<FavoriteLocationDB> FavoriteLocations { get; set; }

    protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder) {
        //todo this should be moved out to general setup
        optionsBuilder.UseInMemoryDatabase ("Users");
    }
}
