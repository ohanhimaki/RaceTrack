using System;
using System.Linq;
using RaceTrack.Data;

namespace RaceTrack;

internal static class PlayerDataHelper
{
    public static bool IsPlayer1Leading(PlayerDataContainer player1Data, PlayerDataContainer player2Data,
        out string differenceMsg)
    {
        differenceMsg = string.Empty;
        if (player1Data.LapTimes.Count == 0 || player2Data.LapTimes.Count == 0)
        {
            return true;
        }
        var test = player1Data.LapTimes.Count;
        var test2 = player2Data.LapTimes.Count;
        var difference = player1Data.LapTimes.Count - player2Data.LapTimes.Count;
        if (difference >= 2)
        {
            differenceMsg = (difference - 1) + " laps";
            return true;
        }

        if (difference <= -2)
        {
            differenceMsg = Math.Abs((difference + 1)) + " laps";
            return false;
        }

        if (difference == 1)
        {
            var player1TotalDuration = player1Data.LapTimes[player2Data.LapTimes.Count].TotalRaceDuration;
            var player2TotalDuration = player2Data.LapTimes.Last().TotalRaceDuration;
            differenceMsg = (player1TotalDuration - player2TotalDuration).ToString(@"ss\.fff");
            return true;
        }

        if (difference == -1)
        {
            var player2TotalDuration = player2Data.LapTimes[player1Data.LapTimes.Count].TotalRaceDuration;
            var player1TotalDuration = player1Data.LapTimes.Last().TotalRaceDuration;
            differenceMsg = (player2TotalDuration - player1TotalDuration).ToString(@"ss\.fff");
            return true;
        }

        if (difference == 0)
        {
            var player2TotalDuration = player2Data.LapTimes.Last().TotalRaceDuration;
            var player1TotalDuration = player1Data.LapTimes.Last().TotalRaceDuration;
            differenceMsg = (player2TotalDuration - player1TotalDuration).ToString(@"ss\.fff");
            return player2TotalDuration < player1TotalDuration;
        }

        return true;
    }
}