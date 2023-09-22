using System.ComponentModel.DataAnnotations;

namespace RaceTrack.Db.Entities;

public class Player
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Lap> Laps { get; set; } // Navigation property
}
