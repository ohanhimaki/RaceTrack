using System;

namespace RaceTrack.Core.Models;

public class LapTime
{
    public int LapNumber { get; set; }
    public string Time { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan TotalRaceDuration { get; set; }
}