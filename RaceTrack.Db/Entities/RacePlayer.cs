using System.ComponentModel.DataAnnotations;

namespace RaceTrack.Db.Entities;

public class RacePlayer
{
    [Key]
    public int Id { get; set; }
    public int RaceId { get; set; }
    public Race Race { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; }
}
