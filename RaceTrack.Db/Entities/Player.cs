namespace RaceTrack.Db.Entities;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Lap> Laps { get; set; } // Navigation property
    public ICollection<RacePlayer> RacePlayers { get; set; }
}
