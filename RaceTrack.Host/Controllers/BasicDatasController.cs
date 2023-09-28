using Microsoft.AspNetCore.Mvc;
using RaceTrack.Core;
using RaceTrack.Db.Entities;

namespace RaceTrack.Host.Controllers;

[ApiController]
[Route("[controller]")]
public class BasicDatasController : ControllerBase
{
    private readonly RaceManagerDbService _raceManagerDbService;

    public BasicDatasController(RaceManagerDbService raceManagerDbService)
    {
        _raceManagerDbService = raceManagerDbService;
    }
    [HttpGet("[action]")]
    public async Task<IEnumerable<Player>> GetPlayers()
    {
        var players = await _raceManagerDbService.GetPlayersAsync();
        return players;
    }
    [HttpPost("[action]")]
    public async Task AddPlayer(string playerName)
    {
        await _raceManagerDbService.AddPlayerAsync(playerName);

    }
    
}