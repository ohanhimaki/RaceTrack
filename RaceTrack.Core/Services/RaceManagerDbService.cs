using RaceTrack.Db;
using RaceTrack.Db.Entities;

namespace RaceTrack.Core;

public class RaceManagerDbService
{

    public RaceManagerDbService()
    {
    }

    // save race 
    public async Task SaveRace(RaceManager raceManager)
    {
        if (raceManager.StartTime is null)
        {
            throw new Exception("Race not started");
        }

        Race race;
        if (raceManager.Race is null)
        {
            race = new Race();
        }
        else
        {
            race = raceManager.Race;
        }

        var Player1 = GetPlayerByName(raceManager.Player1Data.Name);
        var Player2 = GetPlayerByName(raceManager.Player2Data.Name);

        race.StartTime = (DateTime)raceManager.StartTime;
        race.EndTime = raceManager.EndTime;
        race.TotalLaps = raceManager.RaceLaps;
        race.WinnerPlayer = raceManager.GetWinnerOfLastRace() switch
        {
            1 => Player1,
            2 => Player2,
            _ => null
        };
        race.EnumStatus = RaceStatus.Completed;

        race.Laps = GetLaps(raceManager, Player1, Player2);

        using var raceTrackDbContext = new RaceTrackDbContext();
        raceTrackDbContext.Races.Attach(race);

        await raceTrackDbContext.SaveChangesAsync();

        return;
    }

    private List<Lap> GetLaps(RaceManager raceManager, Player player1, Player player2)
    {
        var laps = new List<Lap>();
        foreach (var lap in raceManager.Player1Data.LapTimes)
        {
            laps.Add(new Lap
            {
                LapNumber = lap.LapNumber,
                Duration = lap.Duration,
                TotalRaceDuration = lap.TotalRaceDuration,
                PlayerId = player1.Id,
                Player = player1,
            });
        }

        foreach (var lap in raceManager.Player2Data.LapTimes)
        {
            laps.Add(new Lap
            {
                LapNumber = lap.LapNumber,
                Duration = lap.Duration,
                TotalRaceDuration = lap.TotalRaceDuration,
                PlayerId = player2.Id,
                Player = player2,
            });
        }

        return laps;
    }

    private Player GetPlayerByName(string player1DataName)
    {
        using var raceTrackDbContext = new RaceTrackDbContext();
        var player = raceTrackDbContext.Players.FirstOrDefault(p => p.Name == player1DataName);

        if (player is null)
        {
            player = new Player
            {
                Name = player1DataName,
            };
        }

        raceTrackDbContext.Players.Add(player);
        return player;
    }
    public void AddPlayer(string playerName)
    {
        using var raceTrackDbContext = new RaceTrackDbContext();
        var newPlayer = new Player { Name = playerName };
        raceTrackDbContext.Players.Add(newPlayer);
        raceTrackDbContext.SaveChanges();
    }

    public List<Player> GetPlayers()
    {
        using var raceTrackDbContext = new RaceTrackDbContext();
        return raceTrackDbContext.Players.ToList();
    }
}