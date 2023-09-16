using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Structure;
using RaceTrack.Data;

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
        public PlayerDataContainer Player1Data { get; set; } = new PlayerDataContainer("Mario");
        public PlayerDataContainer Player2Data { get; set; } = new PlayerDataContainer("Luigi");
        private bool settingPointForPlayer1 = false;
        private bool settingPointForPlayer2 = false;

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
            catch(Exception ex)
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
                CheckLapPoint(Player1Data,grayImage);
                CheckLapPoint(Player2Data,grayImage);

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
                if (playerData.LapPoint != null)
                {
                    double colorAtLapPoint = grayImage[(int)playerData.LapPoint.Value.Y, (int)playerData.LapPoint.Value.X].Intensity;
                    if (colorAtLapPoint > 0)
                    {
                        // Motion detected at lap point, register lap.
                        // check that the last lap was at least 2 seconds ago
                        if (playerData.LapTimes.Count == 0 || DateTime.Now - DateTime.Parse(playerData.LapTimes[playerData.LapTimes.Count - 1].Time) >
                            TimeSpan.FromSeconds(1))
                        {
                            AddLapTime(playerData);
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
        private void AddLapTime(PlayerDataContainer playerData)
        {
            Dispatcher.Invoke(() =>
            {
                DateTime currentLapTime = DateTime.Now;

                if (StartDate == null)
                {
                    StartDate = currentLapTime;
                }

                TimeSpan duration = currentLapTime - StartDate.Value;

                if (playerData.LapTimes.Count == 0) // The first lap
                {
                    duration = TimeSpan.Zero;
                }

                playerData.LapTimes.Add(new LapTime
                {
                    LapNumber = playerData.LapTimes.Count, Time = currentLapTime.ToString("HH:mm:ss.fff"), Duration = duration
                });

                // Update the StartDate for the next lap:
                StartDate = currentLapTime;
            });
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
                Player1Data.LapPoint = position;
                Canvas.SetLeft(LapPointCirclePlayer1, position.X - (LapPointCirclePlayer1.Width / 2));
                Canvas.SetTop(LapPointCirclePlayer1, position.Y - (LapPointCirclePlayer1.Height / 2));
                LapPointCirclePlayer1.Visibility = Visibility.Visible;
            }
            else if (settingPointForPlayer2)
            {
                Player2Data.LapPoint = position;
                Canvas.SetLeft(LapPointCirclePlayer2, position.X - (LapPointCirclePlayer2.Width / 2));
                Canvas.SetTop(LapPointCirclePlayer2, position.Y - (LapPointCirclePlayer2.Height / 2));
                LapPointCirclePlayer2.Visibility = Visibility.Visible;
            }
            settingPointForPlayer1 = false;
            settingPointForPlayer2 = false;
        }
        
private BitmapSource ConvertBitmap(System.Drawing.Bitmap bitmap)
{
    IntPtr bmpPtr = bitmap.GetHbitmap();
    BitmapSource bitmapSource;

    try
    {
        bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            bmpPtr, 
            IntPtr.Zero, 
            System.Windows.Int32Rect.Empty, 
            BitmapSizeOptions.FromEmptyOptions());
    }
    finally
    {
        // Release the HBitmap
        DeleteObject(bmpPtr);
    }

    return bitmapSource;
}

[System.Runtime.InteropServices.DllImport("gdi32.dll")]
[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
private static extern bool DeleteObject(IntPtr hObject);
    }
}