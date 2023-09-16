using System;
using System.ComponentModel;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Structure;
using RaceTrack.Core.Models;
using RaceTrack.Core.Helpers;

namespace RaceTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private readonly VideoCapture _capture;
        private Mat _frame;
        private int imagecounter = 0;

        private DateTime? StartDate = null;
        private bool RaceOngoing = false;
        public PlayerDataContainer Player1Data { get; set; } = new PlayerDataContainer("Mario");
        public PlayerDataContainer Player2Data { get; set; } = new PlayerDataContainer("Luigi");
        private bool settingPointForPlayer1 = false;
        private bool settingPointForPlayer2 = false;
        public bool RaceIsStarting { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the lap times collection
            LapTimesListPlayer1.ItemsSource = Player1Data.LapTimes;
            LapTimesListPlayer2.ItemsSource = Player2Data.LapTimes;

            try
            {
                _capture = new VideoCapture(0); // 0 for the default camera
                _capture.ImageGrabbed += Capture_ImageGrabbed;

                _frame = new Mat();
                _capture.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Mat _previousFrame;

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            _capture.Retrieve(_frame);

            if (!_frame.IsEmpty && _previousFrame != null)
            {
                Mat diff = new Mat();
                CvInvoke.AbsDiff(_frame, _previousFrame, diff);
                Mat grayDiff = new Mat();
                CvInvoke.CvtColor(diff, grayDiff, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(grayDiff, grayDiff, 25, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                var grayImage = grayDiff.ToImage<Gray, byte>();
                CheckLapPoint(Player1Data, grayImage);
                CheckLapPoint(Player2Data, grayImage);

                _previousFrame = _frame.Clone();

                Dispatcher.Invoke(() =>
                    {
                        var bitmapSource = BitmapSourceConvert.ToBitmapSource(_frame);
                        WebcamFeed.Source = bitmapSource;
                    },
                    System.Windows.Threading.DispatcherPriority.Render);
            }
            else if (_previousFrame == null)
            {
                _previousFrame = _frame.Clone();
            }
        }

        private void CheckLapPoint(PlayerDataContainer playerData, Image<Gray, byte> grayImage)
        {
            if (!RaceOngoing && !RaceIsStarting)
            {
                return;
            }

            if (playerData.LapPoint != null)
            {
                // caltulate the multiplier for the x and y axis
                var yMultiplier = grayImage.Height / WebcamFeed.ActualHeight;
                var xMultiplier = grayImage.Width / WebcamFeed.ActualWidth;

                var fixedY = (int)(playerData.LapPoint.Value.Y * yMultiplier);
                var fixedX = (int)(playerData.LapPoint.Value.X * xMultiplier);

                double colorAtLapPoint = grayImage[fixedY, fixedX].Intensity;
                if (colorAtLapPoint > 0)
                {
                    if (RaceIsStarting)
                    {
                        WarnTooEarly(playerData);
                        return;
                    }

                    // Motion detected at lap point, register lap.
                    // check that the last lap was at least 2 seconds ago
                    if (playerData.LapTimes.Count == 0 || DateTime.Now -
                        DateTime.Parse(playerData.LapTimes[playerData.LapTimes.Count - 1].Time) >
                        TimeSpan.FromSeconds(1))
                    {
                        AddLapTime(playerData);
                    }
                }
            }
        }

        private async void WarnTooEarly(PlayerDataContainer playerData)
        {
            Dispatcher.Invoke(() =>
            {
                BigWarning.Visibility = Visibility.Visible;
                BigWarning.Text = "Too early " + playerData.Name + "!";
            });

            await Task.Delay(3000);
            StopRace();
            Dispatcher.Invoke(() => { BigWarning.Text = ""; });
        }


        private void btnSetPointPlayer1_Click(object sender, RoutedEventArgs e)
        {
            settingPointForPlayer1 = true;
        }

        private void btnSetPointPlayer2_Click(object sender, RoutedEventArgs e)
        {
            settingPointForPlayer2 = true;
        }

        // Mock method to simulate adding lap times (you'd have your actual logic here)
        private void AddLapTime(PlayerDataContainer playerData)
        {
            Dispatcher.Invoke(() =>
            {
                DateTime currentLapTime = DateTime.Now;

                if (playerData.LapStartTime == null)
                {
                    playerData.LapStartTime = currentLapTime;
                }

                TimeSpan duration = currentLapTime - playerData.LapStartTime.Value;

                if (playerData.LapTimes.Count == 0) // The first lap
                {
                    // startdate vs currentlaptime, tells reaction speed
                    duration = currentLapTime - StartDate.Value;
                }

                playerData.LapTimes.Add(new LapTime
                {
                    LapNumber = playerData.LapTimes.Count, Time = currentLapTime.ToString("HH:mm:ss.fff"),
                    Duration = duration, TotalRaceDuration = currentLapTime - StartDate.Value
                });

                playerData.LapStartTime = currentLapTime;
            });

            UpdateRaceStatus();
        }

        // This will be your model for each lap time

        // Ensure to stop the webcam when the window is closed
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _capture?.Dispose();
            _frame?.Dispose();
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.Stop();
            }
        }

        private void WebcamFeed_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SetLapPoint(e.GetPosition(WebcamFeed));
            // You can optionally draw a visual indicator on the video feed for this point.
        }


        private void SetLapPoint(Point position)
        {
            if (settingPointForPlayer1)
            {
                Player1Data.LapPoint = position.ToNullableDrawingPoint(); 
                Canvas.SetLeft(LapPointCirclePlayer1, position.X - (LapPointCirclePlayer1.Width / 2));
                Canvas.SetTop(LapPointCirclePlayer1, position.Y - (LapPointCirclePlayer1.Height / 2));
                LapPointCirclePlayer1.Visibility = Visibility.Visible;
            }
            else if (settingPointForPlayer2)
            {
                Player2Data.LapPoint = position.ToNullableDrawingPoint();
                Canvas.SetLeft(LapPointCirclePlayer2, position.X - (LapPointCirclePlayer2.Width / 2));
                Canvas.SetTop(LapPointCirclePlayer2, position.Y - (LapPointCirclePlayer2.Height / 2));
                LapPointCirclePlayer2.Visibility = Visibility.Visible;
            }

            settingPointForPlayer1 = false;
            settingPointForPlayer2 = false;
        }

        private async void StartRaceButton_Click(object sender, RoutedEventArgs e)
        {
            StartRaceButton.IsEnabled = false; // Disable button to prevent re-clicks during start
            RaceIsStarting = true;
            Light1.Visibility = Visibility.Visible;
            Light2.Visibility = Visibility.Visible;
            Light3.Visibility = Visibility.Visible;
            Light4.Visibility = Visibility.Visible;
            Light5.Visibility = Visibility.Visible;

            // Light up each red light every second
            Light1.Fill = Brushes.Red;
            await Task.Delay(1000);
            Light2.Fill = Brushes.Red;
            await Task.Delay(1000);
            Light3.Fill = Brushes.Red;
            await Task.Delay(1000);
            Light4.Fill = Brushes.Red;
            await Task.Delay(1000);
            Light5.Fill = Brushes.Red;

            // Wait for a random time between 0.2 to 3 seconds
            Random random = new Random();
            int randomDelay = random.Next(200, 3001); // Between 0.2 to 3 seconds
            await Task.Delay(randomDelay);

            // Extinguish the lights and start the race
            Light1.Fill = Brushes.Gray;
            Light2.Fill = Brushes.Gray;
            Light3.Fill = Brushes.Gray;
            Light4.Fill = Brushes.Gray;
            Light5.Fill = Brushes.Gray;
            RaceIsStarting = false;
            StartRace();
            await Task.Delay(2000);
            // hide lights
            Light1.Visibility = Visibility.Hidden;
            Light2.Visibility = Visibility.Hidden;
            Light3.Visibility = Visibility.Hidden;
            Light4.Visibility = Visibility.Hidden;
            Light5.Visibility = Visibility.Hidden;
        }

        private void UpdateRaceStatus()
        {
            bool isPlayer1Leading =
                PlayerDataHelper.IsPlayer1Leading(Player1Data, Player2Data, out var timeDifferenceText);

            Dispatcher.Invoke(() =>
            {
                if (isPlayer1Leading)
                {
                    FirstPlaceText.Text = Player1Data.Name;
                    SecondPlaceText.Text = Player2Data.Name;
                }
                else
                {
                    FirstPlaceText.Text = Player2Data.Name;
                    SecondPlaceText.Text = Player1Data.Name;
                }

                TimeDifferenceText.Text = timeDifferenceText; // Format to two decimal places
            });
        }


        private void StartRace()
        {
            // Initialize race variables and start the race
            StartDate = DateTime.Now;
            RaceOngoing = true;
            // Add any other logic to start the race
        }

        private void StopRace()
        {
            StartDate = null;
            RaceOngoing = false;
            Dispatcher.Invoke(() => { StartRaceButton.IsEnabled = true; });
        }
    }
}