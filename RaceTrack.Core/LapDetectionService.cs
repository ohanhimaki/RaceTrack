using Emgu.CV;
using Emgu.CV.Structure;
using RaceTrack.Core.Models;

namespace RaceTrack.Core;

public interface ILapDetectionService
{
    event EventHandler<LapDetectedEventArgs> LapDetected;
    void ProcessFrame(Mat frame);
}

public class LapDetectionService : ILapDetectionService
{
    public event EventHandler<LapDetectedEventArgs> LapDetected;
    private Mat _previousFrame;
    private List<PlayerDataContainer> _playerDataContainers;
    private RaceManager _raceManager;

    public LapDetectionService(RaceManager raceManager)
    {
        _raceManager = raceManager;
    }


    public void ProcessFrame(Mat frame)
    {
        // Add your computer vision logic here to detect laps.
        // If a lap is detected, raise the LapDetected event.
        // LapDetected?.Invoke(this, new LapDetectedEventArgs(/* data */));
        if (!frame.IsEmpty && _previousFrame != null)
        {
            Mat diff = new Mat();
            CvInvoke.AbsDiff(frame, _previousFrame, diff);
            Mat grayDiff = new Mat();
            CvInvoke.CvtColor(diff, grayDiff, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
            CvInvoke.Threshold(grayDiff, grayDiff, 25, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
            var grayImage = grayDiff.ToImage<Gray, byte>();

            CheckLapPoint(_raceManager.Player1Data, grayImage);
            CheckLapPoint(_raceManager.Player2Data, grayImage);

            _previousFrame = frame.Clone();
        }
        else if (_previousFrame == null)
        {
            _previousFrame = frame.Clone();
        }
    }

    private void CheckLapPoint(PlayerDataContainer playerData, Image<Gray, byte> grayImage)
    {
        if (!_raceManager.RaceOngoing && !_raceManager.RaceIsStarting)
        {
            return;
        }

        if (playerData.LapPoint != null)
        {
            // caltulate the multiplier for the x and y axis

            var fixedY = (int)(playerData.LapPoint.Value.Y );
            var fixedX = (int)(playerData.LapPoint.Value.X );

            double colorAtLapPoint = grayImage[fixedY, fixedX].Intensity;
            if (colorAtLapPoint > 0)
            {
                if (_raceManager.RaceIsStarting)
                {
                    _raceManager.WarnTooEarly(playerData);
                    return;
                }

                // Motion detected at lap point, register lap.
                // check that the last lap was at least 2 seconds ago
                if (playerData.LapTimesCount == 0 || DateTime.Now -
                    DateTime.Parse(playerData.GetLapTime(playerData.LapTimesCount - 1).Time) >
                    TimeSpan.FromSeconds(1))
                {
                    _raceManager.AddLapTime(playerData);
                }
            }
        }
    }
}

public class LapDetectedEventArgs
{
    PlayerDataContainer PlayerData { get; set; }
}