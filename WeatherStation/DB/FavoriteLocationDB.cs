using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WeatherStation.DB;

[Index (nameof (UserId))]
public class FavoriteLocationDB {
    [Key]
    public string id { get; set; }
    public string Location { get; set; }
    public string UserId { get; set; }
}

