using System;
using System.Linq;
using RaceTrack.Core.Models;

namespace RaceTrack.Core.Helpers;

public static class PlayerDataHelper
{
    public static bool IsPlayer1Leading(PlayerDataContainer player1Data, PlayerDataContainer player2Data,
        out string differenceMsg)
    {
        differenceMsg = string.Empty;
        if (player1Data.LapTimesCount == 0 || player2Data.LapTimesCount == 0)
        {
            return true;
        }
        var test = player1Data.LapTimesCount;
        var test2 = player2Data.LapTimesCount;
        var difference = player1Data.LapTimesCount - player2Data.LapTimesCount;
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
            return CalculateOneRoundDifference(player1Data, player2Data, out differenceMsg);
        }

        if (difference == -1)
        {
            return !CalculateOneRoundDifference(player2Data, player1Data, out differenceMsg);
        }

        var player2TotalDuration = player2Data.LapTimes.Last().TotalRaceDuration;
        var player1TotalDuration = player1Data.LapTimes.Last().TotalRaceDuration;
        var duration = player2TotalDuration > player1TotalDuration ? player2TotalDuration-player1TotalDuration : player1TotalDuration-player2TotalDuration;
        differenceMsg = (duration).ToString(@"ss\.fff");
        return player1TotalDuration < player2TotalDuration;

    }

    private static bool CalculateOneRoundDifference(PlayerDataContainer player1Data, PlayerDataContainer player2Data, out string differenceMsg)
    {
        var player1TotalDurationAll = player1Data.LapTimes.Last().TotalRaceDuration;
        var player2TotalDurationAll = player2Data.LapTimes.Last().TotalRaceDuration;
        var player1LeadsByLap = player1TotalDurationAll < player2TotalDurationAll;
        if (player1LeadsByLap)
        {
            differenceMsg = "1 lap";
            return true;
        }
        
        
        var player1TotalDuration = player1Data.GetLapTime(player2Data.LapTimesCount).TotalRaceDuration;
        var player2TotalDuration = player2Data.LapTimes.Last().TotalRaceDuration;
        differenceMsg = (player1TotalDuration - player2TotalDuration).ToString(@"ss\.fff");
        return true;
    }
}