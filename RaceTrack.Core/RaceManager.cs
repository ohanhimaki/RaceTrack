using RaceTrack.Core.Models;

namespace RaceTrack.Core;

public class RaceManager
{
    public DateTime? StartDate = null;
    public bool RaceOngoing = false;
    public PlayerDataContainer Player1Data { get; set; }
    public PlayerDataContainer Player2Data { get; set; }
    public bool RaceIsStarting { get; set; }

    public RaceManager()
    {
        Player1Data = new PlayerDataContainer("Mario");
        Player2Data = new PlayerDataContainer("Luigi");
        
    }
    public event Action<string> ShowBigWarning;
}