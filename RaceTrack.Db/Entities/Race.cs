using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceTrack.Db.Entities;
public class Race
{

    [Key]
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }  // Nullable if you want to set it only after the race ends
    public int TotalLaps { get; set; }
    public Player? WinnerPlayer { get; set; }  // Can be null if the race hasn't finished yet
    private string _status;
    public string Status 
    {
        get => _status;
        set
        {
            _status = value;
            EnumStatus = (RaceStatus) Enum.Parse(typeof(RaceStatus), value);
        }
    }

    [NotMapped]  
    public RaceStatus EnumStatus 
    { 
        get => (RaceStatus) Enum.Parse(typeof(RaceStatus), _status);
        set 
        {
            _status = value.ToString();
        }
    }
    
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