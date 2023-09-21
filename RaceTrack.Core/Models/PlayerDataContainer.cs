
using System.Collections.ObjectModel;
using System.Drawing;
using RaceTrack.Core.Messaging;
using RaceTrack.Core.Messaging.Messages;

namespace RaceTrack.Core.Models;

public class PlayerDataContainer
{
    public Point? LapPoint = null; 
    public DateTime? LapStartTime = null; 
    public bool Finished { get; set; }

    public List<LapTime> LapTimes { get; set; } = new List<LapTime>();

    public string Name { get; set; }
    
    // get laptimes count
    public int LapTimesCount => LapTimes.Count;
    public TimeSpan TotalRaceDuration => LapTimes.Last().TotalRaceDuration;


    public PlayerDataContainer(string name)
    {
        Name = name;
    }
    
    //action for parent to subscribe to
    public event EventHandler<LapTime> LapTimeAdded;
    public event EventHandler<Point> LapPointEdited;

    public void AddLapTime(LapTime time)
    {
        LapTimes.Add(time);
        LapTimeAdded?.Invoke(this, time);
    }

    public void RemoveLapTime(LapTime time)
    {
        LapTimes.Remove(time);
    }
    
    public LapTime GetLapTime(int index)
    {
        return LapTimes[index];
    }


    public void SetLapPoint(Point point)
    {
        LapPoint = point;
        LapPointEdited?.Invoke(this, point);
    }

    public void FinishRace()
    {
        Finished = true;
    }

    public void Reset()
    {
        LapTimes.Clear();
        LapStartTime = null;
        Finished = false;
    }
}