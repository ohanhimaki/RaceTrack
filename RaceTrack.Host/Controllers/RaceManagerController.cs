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
    private readonly RaceManagerDbService _raceManagerDbService;
    private readonly RaceManager _raceManager;
    private readonly IVideoCaptureService _videoCaptureService;

    public RaceManagerController(RaceManagerDbService raceManagerDbService, RaceManager raceManager,
        IVideoCaptureService videoCaptureService)
    {
        _raceManagerDbService = raceManagerDbService;
        _raceManager = raceManager;
        _videoCaptureService = videoCaptureService;
    }
    [HttpGet("[action]")]
    public async Task<IEnumerable<Player>> GetRaceManagerStatus()
    {
        //todo
        var players = await _raceManagerDbService.GetPlayersAsync();
        return players;
    }
    [HttpGet("[action]")]
    public async Task<IActionResult> GetLatestImage()
    {
        //TODO fix this 
        var currentFrame = await _videoCaptureService.GetCurrentFrame();
        var file = System.IO.File.OpenRead(currentFrame);
        
        return File(file, "image/jpeg");
    }
    
}