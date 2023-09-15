using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using AForge.Video.DirectShow;
using Emgu.CV;

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

        public ObservableCollection<LapTime> LapTimes { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the lap times collection
            LapTimes = new ObservableCollection<LapTime>();
            LapTimesList.ItemsSource = LapTimes;
            
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
        
        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            _capture.Retrieve(_frame);
            
            if (!_frame.IsEmpty)
            {


                Dispatcher.Invoke(() =>
                    {
                        var bitmapSource = BitmapSourceConvert.ToBitmapSource(_frame);
                        WebcamFeed.Source = (bitmapSource);
                    },
                    System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        // Mock method to simulate adding lap times (you'd have your actual logic here)
        private void AddLapTime()
        {
            LapTimes.Add(new LapTime { LapNumber = LapTimes.Count + 1, Time = DateTime.Now.ToString("HH:mm:ss.fff") });
        }

        // This will be your model for each lap time
        public class LapTime
        {
            public int LapNumber { get; set; }
            public string Time { get; set; }
        }

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