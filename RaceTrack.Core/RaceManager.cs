using RaceTrack.Core.Helpers;
using RaceTrack.Core.Messaging;
using RaceTrack.Core.Messaging.Messages;
using RaceTrack.Core.Models;

namespace RaceTrack.Core;

public class RaceManager
{
    private readonly EventAggregator _eventAggregator;
    public DateTime? StartDate = null;
    public bool RaceOngoing = false;
    public PlayerDataContainer Player1Data { get; set; }
    public PlayerDataContainer Player2Data { get; set; }
    public bool RaceIsStarting { get; set; }

    public RaceManager(EventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;

        Player1Data = new PlayerDataContainer("Mario");
        Player2Data = new PlayerDataContainer("Luigi");
    }


    public void UpdateRaceStatus()
    {
        bool isPlayer1Leading =
            PlayerDataHelper.IsPlayer1Leading(Player1Data, Player2Data, out var timeDifferenceText);

        if (isPlayer1Leading)
        {
            var message = new RaceStatusMessage
            {
                FirstPlaceText = Player1Data.Name,
                SecondPlaceText = Player2Data.Name,
                TimeDifferenceText = timeDifferenceText
            };
            _eventAggregator.Publish(message);
        }
        else
        {
            var message = new RaceStatusMessage
            {
                FirstPlaceText = Player2Data.Name,
                SecondPlaceText = Player1Data.Name,
                TimeDifferenceText = timeDifferenceText
            };
            _eventAggregator.Publish(message);
        }
    }


    public void StartRace()
    {
        // Initialize race variables and start the race
        StartDate = DateTime.Now;
        RaceOngoing = true;
        // Add any other logic to start the race
    }

    public void StopRace()
    {
        StartDate = null;
        RaceOngoing = false;
        _eventAggregator.Publish(new StartButtonStateMessage { IsEnabled = true });
    }
}