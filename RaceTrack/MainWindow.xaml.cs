using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AForge.Video;
using AForge.Video.DirectShow;

namespace RaceTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
 public partial class MainWindow : Window
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;

        public ObservableCollection<LapTime> LapTimes { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the lap times collection
            LapTimes = new ObservableCollection<LapTime>();
            LapTimesList.ItemsSource = LapTimes;

            // Set up the webcam
            SetupWebcam();
        }

        private void SetupWebcam()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("No video devices found.");
                return;
            }

            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            videoSource.Start();
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            using (Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone())
            {
                Dispatcher.Invoke(new Action(() => 
                {
                    WebcamFeed.Source = ConvertBitmap(bitmap);
                }));
            }
        }

        // private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        // {
        //     // save webcam image for testing that its working
        //     if (WebcamFeed.Source != null)
        //     {
        //         var encoder = new JpegBitmapEncoder();
        //         encoder.Frames.Add(BitmapFrame.Create((BitmapSource)WebcamFeed.Source));
        //         using (var filestream = new System.IO.FileStream("test.jpg", System.IO.FileMode.Create))
        //
        //             encoder.Save(filestream);
        //     }
        // }

        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebcamFeed.Source != null)
            {
                var encoder = new JpegBitmapEncoder(); // Use JPEG to save the image
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)WebcamFeed.Source));

                using (var fileStream = new System.IO.FileStream("webcam_snapshot.jpg", System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
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