using System.Drawing;
using RaceTrack.Core.Helpers;
using RaceTrack.Core.Messaging;
using RaceTrack.Core.Messaging.Messages;
using RaceTrack.Core.Models;
using RaceTrack.Core.Services;

namespace RaceTrack.Core;

public class RaceManager
{
    private readonly EventAggregator _eventAggregator;
    private readonly IVideoCaptureService _videoCaptureService;
    public DateTime? StartDate = null;
    public bool RaceOngoing = false;
    private readonly RaceVideoProcessor _raceVideoProcessor;
    public PlayerDataContainer Player1Data { get; set; }
    public PlayerDataContainer Player2Data { get; set; }
    public bool RaceIsStarting { get; set; }

    public RaceManager(EventAggregator eventAggregator, IVideoCaptureService videoCaptureService)
    {
        _eventAggregator = eventAggregator;
        _videoCaptureService = videoCaptureService;
        _raceVideoProcessor = new RaceVideoProcessor(_videoCaptureService, new LapDetectionService(this));

        Player1Data = new PlayerDataContainer("Mario");
        Player1Data.LapTimeAdded += Player1DataOnLapTimeAdded;
        Player2Data = new PlayerDataContainer("Luigi");
        Player2Data.LapTimeAdded += Player2DataOnLapTimeAdded;
    }

    private void Player2DataOnLapTimeAdded(object? sender, LapTime e)
    {
        _eventAggregator.Publish(new NewLapTimeMessage()
        {
            LapTime = e,
            PlayerNbr = 2
        });
    }

    private void Player1DataOnLapTimeAdded(object? sender, LapTime e)
    {
        _eventAggregator.Publish(new NewLapTimeMessage()
        {
            LapTime = e,
            PlayerNbr = 1
        });
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

    public async void WarnTooEarly(PlayerDataContainer playerData)
    {
        _eventAggregator.Publish(new BigWarningMessage() { Message = "Too early " + playerData.Name + "!" });


        await Task.Delay(3000);
        StopRace();
        _eventAggregator.Publish(new BigWarningMessage());
    }

    public void AddLapTime(PlayerDataContainer playerData)
    {
        DateTime currentLapTime = DateTime.Now;

        if (playerData.LapStartTime == null)
        {
            playerData.LapStartTime = currentLapTime;
        }

        TimeSpan duration = currentLapTime - playerData.LapStartTime.Value;

        if (playerData.LapTimesCount == 0) // The first lap
        {
            // startdate vs currentlaptime, tells reaction speed
            duration = currentLapTime - StartDate.Value;
        }

        playerData.AddLapTime(new LapTime
        {
            LapNumber = playerData.LapTimesCount, Time = currentLapTime.ToString("HH:mm:ss.fff"),
            Duration = duration, TotalRaceDuration = currentLapTime - StartDate.Value
        });

        playerData.LapStartTime = currentLapTime;

        UpdateRaceStatus();
    }

    public async void StartRace()
    {
        var message = new RaceStartLightsMessage
        {
            Light1Fill = Color.Gray,
            Light2Fill = Color.Gray,
            Light3Fill = Color.Gray,
            Light4Fill = Color.Gray,
            Light5Fill = Color.Gray,
            Light1Visible = true,
            Light2Visible = true,
            Light3Visible = true,
            Light4Visible = true,
            Light5Visible = true,
            StartButtonEnabled = false
        };
        RaceIsStarting = true;
        _eventAggregator.Publish(message);

        // Light up each red light every second
        message.Light1Fill = Color.Red;
        _eventAggregator.Publish(message);
        await Task.Delay(1000);
        message.Light2Fill = Color.Red;
        _eventAggregator.Publish(message);
        await Task.Delay(1000);
        message.Light3Fill = Color.Red;
        _eventAggregator.Publish(message);
        await Task.Delay(1000);
        message.Light4Fill = Color.Red;
        _eventAggregator.Publish(message);
        await Task.Delay(1000);
        message.Light5Fill = Color.Red;
        _eventAggregator.Publish(message);

        // Wait for a random time between 0.2 to 3 seconds
        Random random = new Random();
        int randomDelay = random.Next(200, 3001); // Between 0.2 to 3 seconds
        await Task.Delay(randomDelay);

        // Extinguish the lights and start the race
        message.Light1Fill = Color.Gray;
        message.Light2Fill = Color.Gray;
        message.Light3Fill = Color.Gray;
        message.Light4Fill = Color.Gray;
        message.Light5Fill = Color.Gray;
        _eventAggregator.Publish(message);

        RaceIsStarting = false;
        StartDate = DateTime.Now;
        RaceOngoing = true;
        await Task.Delay(2000);
        message.Light1Visible = false;
        message.Light2Visible = false;
        message.Light3Visible = false;
        message.Light4Visible = false;
        message.Light5Visible = false;
        _eventAggregator.Publish(message);
        
    }

    public void StopRace()
    {
        StartDate = null;
        RaceOngoing = false;
        _eventAggregator.Publish(new StartButtonStateMessage { IsEnabled = true });
    }
}