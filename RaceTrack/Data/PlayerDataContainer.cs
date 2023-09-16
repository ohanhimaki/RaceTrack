﻿using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace RaceTrack.Data;

public class PlayerDataContainer
{
    private int _shortListCount = 3;

    public Point? LapPoint = null; 
    public DateTime? LapStartTime = null; 
    public ObservableCollection<LapTime> LapTimes { get; set; }

    public string Name { get; set; }

    public PlayerDataContainer(string name)
    {
        Name = name;
        LapTimes = new ObservableCollection<LapTime>();
    }


}