using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Emgu.CV;
using Emgu.CV.Structure;
using RaceTrack.Core;
using RaceTrack.Core.Models;
using RaceTrack.Core.Messaging;
using RaceTrack.Core.Messaging.Messages;
using RaceTrack.Core.Services;
using RaceTrack.Video.Services;

namespace RaceTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly EventAggregator _eventAggregator;

        private bool _settingPointForPlayer1;
        private bool _settingPointForPlayer2;
        
        // observable collection for the lap times
        private readonly ObservableCollection<LapTime> _lapTimesPlayer1 = new ObservableCollection<LapTime>();
        private readonly ObservableCollection<LapTime> _lapTimesPlayer2 = new ObservableCollection<LapTime>();
        
        public RaceManager RaceManager { get; set; }   
        private VideoCaptureService _videoCaptureService { get; set; }   
        public MainWindow(EventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _videoCaptureService = new VideoCaptureService();
            RaceManager = new RaceManager(_eventAggregator, _videoCaptureService);
            _videoCaptureService.FrameCaptured += VideoCaptureServiceOnFrameCaptured;
            InitializeComponent();

            // Initialize the lap times collection
            LapTimesListPlayer1.ItemsSource = _lapTimesPlayer1;
            LapTimesListPlayer2.ItemsSource = _lapTimesPlayer2;

            try
            {
                _videoCaptureService.StartCapture();
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
                    BigWarning.Text = message.Message;
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
            _eventAggregator.Subscribe<NewLapTimeMessage>(message =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (message.PlayerNbr == 1)
                    {
                        _lapTimesPlayer1.Add(message.LapTime);
                    }
                    else
                    {
                        _lapTimesPlayer2.Add(message.LapTime);
                    }
                });
            });
        }

        private void VideoCaptureServiceOnFrameCaptured(object? sender, FrameCapturedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                WebcamFeed.Source = BitmapSourceConvert.ToBitmapSource(e.Frame);
            });
        }

        private void btnSetPointPlayer1_Click(object sender, RoutedEventArgs e)
        {
            _settingPointForPlayer1 = true;
        }

        private void btnSetPointPlayer2_Click(object sender, RoutedEventArgs e)
        {
            _settingPointForPlayer2 = true;
        }

        // Mock method to simulate adding lap times (you'd have your actual logic here)

        // This will be your model for each lap time

        // Ensure to stop the webcam when the window is closed
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _videoCaptureService.StopCapture();
        }

        private void WebcamFeed_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SetLapPoint(e.GetPosition(WebcamFeed));
            // You can optionally draw a visual indicator on the video feed for this point.
        }


        private void SetLapPoint(Point position)
        {
            if (_settingPointForPlayer1)
            {
                // calculate point of video feed
                var multiplierx = WebcamFeed.ActualWidth / WebcamFeed.Source.Width;
                var multipliery = WebcamFeed.ActualHeight / WebcamFeed.Source.Height;
                
                var fixedY = (int)(position.Y * multipliery);
                var fixedX = (int)(position.X * multiplierx);

                RaceManager.Player1Data.LapPoint = new System.Drawing.Point(fixedX, fixedY);
                Canvas.SetLeft(LapPointCirclePlayer1, position.X - (LapPointCirclePlayer1.Width / 2));
                Canvas.SetTop(LapPointCirclePlayer1, position.Y - (LapPointCirclePlayer1.Height / 2));
                LapPointCirclePlayer1.Visibility = Visibility.Visible;
            }
            else if (_settingPointForPlayer2)
            {
                // calculate point of video feed
                var multiplierx = WebcamFeed.ActualWidth / WebcamFeed.Source.Width;
                var multipliery = WebcamFeed.ActualHeight / WebcamFeed.Source.Height;
                
                var fixedY = (int)(position.Y * multipliery);
                var fixedX = (int)(position.X * multiplierx);

                RaceManager.Player2Data.LapPoint = new System.Drawing.Point(fixedX, fixedY);
                Canvas.SetLeft(LapPointCirclePlayer2, position.X - (LapPointCirclePlayer2.Width / 2));
                Canvas.SetTop(LapPointCirclePlayer2, position.Y - (LapPointCirclePlayer2.Height / 2));
                LapPointCirclePlayer2.Visibility = Visibility.Visible;
            }

            _settingPointForPlayer1 = false;
            _settingPointForPlayer2 = false;
        }

        private async void StartRaceButton_Click(object sender, RoutedEventArgs e)
        {
            RaceManager.StartRace();
        }

    }
}