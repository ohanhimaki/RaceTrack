using System.ComponentModel.DataAnnotations;

namespace RaceTrack.Db.Entities;

public class Lap
{
    [Key]
    public int Id { get; set; }
    public int LapNumber { get; set; }        // Represents the sequence of the lap in the race.
    public TimeSpan Duration { get; set; }    // Represents the duration of this particular lap.
    public TimeSpan TotalRaceDuration { get; set; } // Represents the total race time up to the end of this lap.
    public int PlayerId { get; set; }         // Foreign key for Player.
    public Player Player { get; set; }        // Navigation property for Player.
    public int RaceId { get; set; }           // Foreign key for Race.
    public Race Race { get; set; }            // Navigation property for Race.
}