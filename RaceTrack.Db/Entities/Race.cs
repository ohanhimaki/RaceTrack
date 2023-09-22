using System.ComponentModel.DataAnnotations;

namespace RaceTrack.Db.Entities;
public class Race
{
    [Key]
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }  // Nullable if you want to set it only after the race ends
    public int TotalLaps { get; set; }
    public Player? WinnerPlayer { get; set; }  // Can be null if the race hasn't finished yet
    public RaceStatus Status { get; set; }  // Enum for race status (e.g., NotStarted, Ongoing, Completed)
    
    // Navigation properties
    public ICollection<Lap> Laps { get; set; }        // All laps recorded during the race
}
public enum RaceStatus
{
    NotStarted,
    Ongoing,
    Completed,
    Cancelled
}