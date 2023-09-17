using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Structure;
using RaceTrack.Core;
using RaceTrack.Core.Models;
using RaceTrack.Core.Messaging;
using RaceTrack.Core.Messaging.Messages;

namespace RaceTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly EventAggregator _eventAggregator;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private readonly VideoCapture _capture;
        private Mat _frame;

        private bool settingPointForPlayer1 = false;
        private bool settingPointForPlayer2 = false;
        
        public RaceManager RaceManager { get; set; }   
        public MainWindow(EventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            RaceManager = new RaceManager(_eventAggregator);
            InitializeComponent();

            // Initialize the lap times collection
            LapTimesListPlayer1.ItemsSource = RaceManager.Player1Data.LapTimes;
            LapTimesListPlayer2.ItemsSource = RaceManager.Player2Data.LapTimes;

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
            
            _eventAggregator.Subscribe<RaceStatusMessage>(message => 
            {
                Dispatcher.Invoke(() =>
                {
                    FirstPlaceText.Text = message.FirstPlaceText;
                    SecondPlaceText.Text = message.SecondPlaceText;
                    TimeDifferenceText.Text = message.TimeDifferenceText;
                });
            });
            _eventAggregator.Subscribe<StartButtonStateMessage>(message => 
            {
                Dispatcher.Invoke(() =>
                {
                    StartRaceButton.IsEnabled = message.IsEnabled;
                });
            });
            _eventAggregator.Subscribe<BigWarningMessage>(message => 
            {
                Dispatcher.Invoke(() =>
                {
                    BigWarning.Text = "";
                });
            });
            _eventAggregator.Subscribe<RaceStartLightsMessage>(message => 
            {
                Dispatcher.Invoke(() =>
                {
                    Light1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(message.Light1Fill.Name));
                    Light2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(message.Light2Fill.Name));
                    Light3.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(message.Light3Fill.Name));
                    Light4.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(message.Light4Fill.Name));
                    Light5.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(message.Light5Fill.Name));
                    Light1.Visibility = message.Light1Visible ? Visibility.Visible : Visibility.Hidden;
                    Light2.Visibility = message.Light2Visible ? Visibility.Visible : Visibility.Hidden;
                    Light3.Visibility = message.Light3Visible ? Visibility.Visible : Visibility.Hidden;
                    Light4.Visibility = message.Light4Visible ? Visibility.Visible : Visibility.Hidden;
                    Light5.Visibility = message.Light5Visible ? Visibility.Visible : Visibility.Hidden;
                    StartRaceButton.IsEnabled = message.StartButtonEnabled;
                });
            });
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
                CheckLapPoint(RaceManager.Player1Data, grayImage);
                CheckLapPoint(RaceManager.Player2Data, grayImage);

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
            if (!RaceManager.RaceOngoing && !RaceManager.RaceIsStarting)
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
                    if (RaceManager.RaceIsStarting)
                    {
                        RaceManager.WarnTooEarly(playerData);
                        return;
                    }

                    // Motion detected at lap point, register lap.
                    // check that the last lap was at least 2 seconds ago
                    if (playerData.LapTimes.Count == 0 || DateTime.Now -
                        DateTime.Parse(playerData.LapTimes[playerData.LapTimes.Count - 1].Time) >
                        TimeSpan.FromSeconds(1))
                    {
                        RaceManager.AddLapTime(playerData);
                    }
                }
            }
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
                RaceManager.Player1Data.LapPoint = position.ToNullableDrawingPoint(); 
                Canvas.SetLeft(LapPointCirclePlayer1, position.X - (LapPointCirclePlayer1.Width / 2));
                Canvas.SetTop(LapPointCirclePlayer1, position.Y - (LapPointCirclePlayer1.Height / 2));
                LapPointCirclePlayer1.Visibility = Visibility.Visible;
            }
            else if (settingPointForPlayer2)
            {
                RaceManager.Player2Data.LapPoint = position.ToNullableDrawingPoint();
                Canvas.SetLeft(LapPointCirclePlayer2, position.X - (LapPointCirclePlayer2.Width / 2));
                Canvas.SetTop(LapPointCirclePlayer2, position.Y - (LapPointCirclePlayer2.Height / 2));
                LapPointCirclePlayer2.Visibility = Visibility.Visible;
            }

            settingPointForPlayer1 = false;
            settingPointForPlayer2 = false;
        }

        private async void StartRaceButton_Click(object sender, RoutedEventArgs e)
        {
            RaceManager.StartRace();
        }

    }
}