using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using RaceTrack.Core;
using RaceTrack.Core.Services;
using RaceTrack.Db.Entities;

namespace RaceTrack.Host.Controllers;

[ApiController]
[Route("[controller]")]
public class RaceManagerController : ControllerBase
{
    private readonly RaceManager _raceManager;

    public RaceManagerController(RaceManager raceManager)
    {
        _raceManager = raceManager;
    }
    [HttpGet("[action]")]
    public async Task<RaceManagerStatus> GetRaceManagerStatus()
    {
        var raceManagerStatus = _raceManager.GetStatus();
        return raceManagerStatus;
    }
    
    [HttpPost("[action]")]
    public async Task<RaceManagerStatus> StartRace()
    {
        await _raceManager.StartRace();
        var raceManagerStatus = _raceManager.GetStatus();
        return raceManagerStatus;
    }
    
    
    // set lapCount
    [HttpPost("[action]")]
    public async Task<RaceManagerStatus> SetLapCount(int laps)
    {
        _raceManager.RaceLaps = laps;
        var raceManagerStatus = _raceManager.GetStatus();
        return raceManagerStatus;
    }
    
}