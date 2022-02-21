using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WeatherStation.DB;

[Index (nameof (Email))]
public class UserDB {
    [Key]
    public string id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string SaltedHashedPass { get; set; }

}
