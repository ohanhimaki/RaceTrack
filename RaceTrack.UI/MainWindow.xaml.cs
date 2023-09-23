using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RaceTrack.Core;
using RaceTrack.Core.Models;
using RaceTrack.Core.Messaging;
using RaceTrack.Core.Messaging.Messages;
using RaceTrack.Core.Services;
using RaceTrack.Db.Entities;
using RaceTrack.Video.Services;

namespace RaceTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RaceManagerDbService _raceManagerDbService;
        private bool _settingPointForPlayer1;
        private bool _settingPointForPlayer2;

        // observable collection for the lap times
        private readonly ObservableCollection<LapTime> _lapTimesPlayer1 = new ObservableCollection<LapTime>();
        private readonly ObservableCollection<LapTime> _lapTimesPlayer2 = new ObservableCollection<LapTime>();

        public RaceManager RaceManager { get; set; }
        private VideoCaptureService _videoCaptureService { get; set; }

        public MainWindow(EventAggregator eventAggregator, RaceManagerDbService raceManagerDbService)
        {
            _raceManagerDbService = raceManagerDbService;
            _videoCaptureService = new VideoCaptureService();
            RaceManager = new RaceManager(eventAggregator, _videoCaptureService, raceManagerDbService);
            _videoCaptureService.FrameCaptured += VideoCaptureServiceOnFrameCaptured;
            InitializeComponent();

            // Initialize the lap times collection
            LapTimesListPlayer1.ItemsSource = _lapTimesPlayer1;
            LapTimesListPlayer2.ItemsSource = _lapTimesPlayer2;
            LoadPlayers();

            try
            {
                _videoCaptureService.StartCapture();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            eventAggregator.Subscribe<RaceStatusMessage>(message =>
            {
                Dispatcher.Invoke(() =>
                {
                    FirstPlaceText.Text = message.FirstPlaceText;
                    SecondPlaceText.Text = message.SecondPlaceText;
                    LapText.Text = message.LapCountText;
                });
            });
            eventAggregator.Subscribe<StartButtonStateMessage>(message =>
            {
                Dispatcher.Invoke(() => { StartRaceButton.Content = message.IsEnabled ? "Start" : "Reset"; });
            });
            eventAggregator.Subscribe<BigWarningMessage>(message =>
            {
                Dispatcher.Invoke(() => { BigWarning.Text = message.Message; });
            });
            eventAggregator.Subscribe<ResetRaceManagerMessage>(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    StartRaceButton.IsEnabled = true;
                    BigWarning.Text = "";
                    _lapTimesPlayer1.Clear();
                    _lapTimesPlayer2.Clear();
                    StartRaceButton.Content = "Start";
                });
            });
            eventAggregator.Subscribe<RaceStartLightsMessage>(message =>
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
                    StartRaceButton.Content = message.StartButtonEnabled ? "Start" : "Reset";
                });
            });
            eventAggregator.Subscribe<NewLapTimeMessage>(message =>
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
            eventAggregator.Subscribe<LapPointEditedMessage>(message =>
            {
                Dispatcher.Invoke(() =>
                {
                    var multiplierx = WebcamFeed.ActualWidth / WebcamFeed.Source.Width;
                    var multipliery = WebcamFeed.ActualHeight / WebcamFeed.Source.Height;
                    var fixedX = message.Position.X * multiplierx;
                    var fixedY = message.Position.Y * multipliery;
                    if (message.PlayerNbr == 1)
                    {
                        Canvas.SetLeft(LapPointCirclePlayer1, fixedX - (LapPointCirclePlayer1.Width / 2));
                        Canvas.SetTop(LapPointCirclePlayer1, fixedY - (LapPointCirclePlayer1.Height / 2));
                        LapPointCirclePlayer1.Visibility =
                            message.ShowLapPoint ? Visibility.Visible : Visibility.Hidden;
                    }
                    else
                    {
                        Canvas.SetLeft(LapPointCirclePlayer2, fixedX - (LapPointCirclePlayer2.Width / 2));
                        Canvas.SetTop(LapPointCirclePlayer2, fixedY - (LapPointCirclePlayer2.Height / 2));
                        LapPointCirclePlayer2.Visibility =
                            message.ShowLapPoint ? Visibility.Visible : Visibility.Hidden;
                    }
                });
            });
        }

        private void VideoCaptureServiceOnFrameCaptured(object? sender, FrameCapturedEventArgs e)
        {
            Dispatcher.Invoke(() => { WebcamFeed.Source = BitmapSourceConvert.ToBitmapSource(e.Frame); });
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
            // calculate point of video feed
            var multiplierx = WebcamFeed.ActualWidth / WebcamFeed.Source.Width;
            var multipliery = WebcamFeed.ActualHeight / WebcamFeed.Source.Height;

            var fixedX = (int)(position.X / multiplierx);
            var fixedY = (int)(position.Y / multipliery);
            if (_settingPointForPlayer1)
            {
                RaceManager.Player1Data.SetLapPoint(new System.Drawing.Point(fixedX, fixedY));
            }
            else if (_settingPointForPlayer2)
            {
                RaceManager.Player2Data.SetLapPoint(new System.Drawing.Point(fixedX, fixedY));
            }

            _settingPointForPlayer1 = false;
            _settingPointForPlayer2 = false;
        }

        private async void StartRaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (RaceManager.RaceOngoing)
            {
                RaceManager.ResetRaceManager();
            }
            else
            {
                RaceManager.StartRace(int.Parse((RaceLapSetting.SelectedItem as ComboBoxItem).Content.ToString()),
                    (Player)Player1ComboBox.SelectedItem, (Player)Player2ComboBox.SelectedItem);
            }
        }
        
        private void LoadPlayers()
        {
            // Fetch players from the database
            var players = _raceManagerDbService.GetPlayers();

            // Assign to ComboBox's ItemsSource
            Player1ComboBox.ItemsSource = players;
            Player2ComboBox.ItemsSource = players;
        }
        private void AddNewPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            var playerName = newPlayerNameTextBox.Text;
        
            if (!string.IsNullOrEmpty(playerName))
            {
                _raceManagerDbService.AddPlayer(playerName);
                LoadPlayers();
                newPlayerNameTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Please enter a valid player name.");
            }
        }
    }
}