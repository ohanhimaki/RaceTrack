namespace RaceTrack.Db.Entities;

public class RacePlayer
{
    public int RaceId { get; set; }
    public Race Race { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; }
}
