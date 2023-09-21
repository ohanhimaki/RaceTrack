using System.Drawing;

namespace RaceTrack.Core.Messaging.Messages;

public class LapPointEditedMessage
{
    public int PlayerNbr { get; set; }
    public Point Position { get; set; }
    public bool ShowLapPoint { get; set; }
}