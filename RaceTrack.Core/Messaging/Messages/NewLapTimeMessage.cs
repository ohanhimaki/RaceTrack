using RaceTrack.Core.Models;

namespace RaceTrack.Core.Messaging.Messages;

public class NewLapTimeMessage
{
    public LapTime LapTime { get; set; }
    public int PlayerNbr { get; set; }
}