using System.Drawing;
using RaceTrack.Core.Helpers;
using RaceTrack.Core.Messaging;
using RaceTrack.Core.Messaging.Messages;
using RaceTrack.Core.Models;
using RaceTrack.Core.Services;
using RaceTrack.Db.Entities;

namespace RaceTrack.Core;

public class RaceManager
{
    private readonly EventAggregator _eventAggregator;
    private readonly IVideoCaptureService _videoCaptureService;
    private readonly RaceManagerDbService _raceManagerDbService;
    public DateTime? StartTime = null;
    public DateTime? EndTime = null;
    public bool RaceOngoing = false;
    private readonly RaceVideoProcessor _raceVideoProcessor;
    public PlayerDataContainer Player1Data { get; set; }
    public PlayerDataContainer Player2Data { get; set; }
    public bool RaceIsStarting { get; set; }
    public int RaceLaps { get; set; } = 5;
    public Race? Race { get; set; }

    public RaceManager(EventAggregator eventAggregator, IVideoCaptureService videoCaptureService, RaceManagerDbService raceManagerDbService)
    {
        _eventAggregator = eventAggregator;
        _videoCaptureService = videoCaptureService;
        _raceManagerDbService = raceManagerDbService;
        _raceVideoProcessor = new RaceVideoProcessor(_videoCaptureService, new LapDetectionService(this));

        Player1Data = new PlayerDataContainer("Mario");
        Player1Data.LapTimeAdded += Player1DataOnLapTimeAdded;
        Player1Data.LapPointEdited += Player1LapPointEdited;
        Player2Data = new PlayerDataContainer("Luigi");
        Player2Data.LapTimeAdded += Player2DataOnLapTimeAdded;
        Player2Data.LapPointEdited += Player2LapPointEdited;
        UpdateRaceStatus();
    }

    private void Player2LapPointEdited(object? sender, Point e)
    {
        _eventAggregator.Publish(new LapPointEditedMessage()
        {
            PlayerNbr = 2,
            Position = e,
            ShowLapPoint = true
            
        });
    }

    public string SetPlayer1(string name)
    {
        if (RaceIsStarting || RaceOngoing)
        {
            return "Ei voi vaihtaa nimeä";
        }
        Player1Data.Name = name;
        return "";
    }
    public string SetPlayer2(string name)
    {
        if (RaceIsStarting || RaceOngoing)
        {
            return "Ei voi vaihtaa nimeä";
        }
        Player2Data.Name = name;
        return "";
    }

    private void Player1LapPointEdited(object? sender, Point e)
    {
        _eventAggregator.Publish(new LapPointEditedMessage()
        {
            PlayerNbr = 1,
            Position = e,
            ShowLapPoint = true
            
        });
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
        if(!RaceIsStarting && !RaceOngoing)
        {
            var message = new RaceStatusMessage
            {
                FirstPlaceText = Player1Data.Name,
                SecondPlaceText = Player2Data.Name ,
                LapCountText = ""
            };
            _eventAggregator.Publish(message);
            return;
        }
        bool isPlayer1Leading =
            PlayerDataHelper.IsPlayer1Leading(Player1Data, Player2Data, out var timeDifferenceText);

        var additionalMessage = "";
        if (isPlayer1Leading)
        {
            if (Player1Data.Finished)
            {
                additionalMessage = " " + Player1Data.TotalRaceDuration.ToString("mm\\:ss\\.fff");
            }
            var message = new RaceStatusMessage
            {
                FirstPlaceText = Player1Data.Name + additionalMessage,
                SecondPlaceText = Player2Data.Name + " " + timeDifferenceText,
                LapCountText = Player1Data.LapTimesCount + "/" + RaceLaps
            };
            _eventAggregator.Publish(message);
        }
        else
        {
            if (Player2Data.Finished)
            {
                additionalMessage = " " + Player1Data.TotalRaceDuration.ToString("mm\\:ss\\.fff");
            }
            var message = new RaceStatusMessage
            {
                FirstPlaceText = Player2Data.Name + additionalMessage,
                SecondPlaceText = Player1Data.Name + " +" + timeDifferenceText,
                LapCountText = Player2Data.LapTimesCount-1 + "/" + RaceLaps
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
        if (playerData.Finished)
        {
            return;
        }
        DateTime currentLapTime = DateTime.Now;

        if (playerData.LapStartTime == null)
        {
            playerData.LapStartTime = currentLapTime;
        }

        TimeSpan duration = currentLapTime - playerData.LapStartTime.Value;

        if (playerData.LapTimesCount == 0) // The first lap
        {
            // startdate vs currentlaptime, tells reaction speed
            duration = currentLapTime - StartTime.Value;
        }

        playerData.AddLapTime(new LapTime
        {
            LapNumber = playerData.LapTimesCount, Time = currentLapTime.ToString("HH:mm:ss.fff"),
            Duration = duration, TotalRaceDuration = currentLapTime - StartTime.Value
        });
        if (playerData.LapTimesCount-1 == RaceLaps)
        {
            playerData.FinishRace();
            if (Player1Data.Finished && Player2Data.Finished)
            {
                StopRace();
            }
            else
            {
                _eventAggregator.Publish(new BigWarningMessage
                {
                    Message = Player1Data.Name + " IS THE WINNER!!!"
                });
            }
        }

        playerData.LapStartTime = currentLapTime;

        UpdateRaceStatus();
    }

    public async void StartRace(int lapcount, string player1Name, string player2Name)
    {
        SetPlayer1(player1Name);
        SetPlayer2(player2Name);
        RaceLaps = lapcount;
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
        StartTime = DateTime.Now;
        RaceOngoing = true;
        await Task.Delay(2000);
        message.Light1Visible = false;
        message.Light2Visible = false;
        message.Light3Visible = false;
        message.Light4Visible = false;
        message.Light5Visible = false;
        _eventAggregator.Publish(message);
        
    }

    public async Task StopRace()
    {
        EndTime = DateTime.Now;
        RaceOngoing = false;
        await _raceManagerDbService.SaveRace(this);
        StartTime = null;
        _eventAggregator.Publish(new StartButtonStateMessage { IsEnabled = true });
    }

    public void ResetRaceManager()
    {
        Player1Data.Reset();
        Player2Data.Reset();
        StartTime = null;
        RaceOngoing = false;
        RaceIsStarting = false;
        _eventAggregator.Publish(new ResetRaceManagerMessage());
    }

    public int? GetWinnerOfLastRace()
    {
        if (Player1Data.Finished && Player2Data.Finished)
        {
            if (Player1Data.TotalRaceDuration < Player2Data.TotalRaceDuration)
            {
                // todo clean this
                return 1;
            }
            else
            {
                return 2;
            }
        }
        else
        {
            return null;
        }
    }
}