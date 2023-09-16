using System;

namespace RaceTrack.Data;

public class LapTime
{
    public int LapNumber { get; set; }
    public string Time { get; set; }
    public TimeSpan Duration { get; set; }
}