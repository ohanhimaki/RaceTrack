using Emgu.CV;
using RaceTrack.Core.Services;

namespace RaceTrack.Video.Services
{
    public class VideoCaptureService : IVideoCaptureService
    {
        private readonly VideoCapture _capture;
        private Mat _currentFrame;

        public event EventHandler<FrameCapturedEventArgs> FrameCaptured;

        public VideoCaptureService()
        {
            _capture = new VideoCapture(0); // Using 0 for default camera. Adjust if needed.
            _currentFrame = new Mat();
            _capture.ImageGrabbed += Capture_ImageGrabbed;
        }
        
        public void StartCapture()
        {
            _capture.Start();
        }

        public void StopCapture()
        {
            _capture.Stop();
        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            _capture.Retrieve(_currentFrame);
            FrameCaptured?.Invoke(this, new FrameCapturedEventArgs { Frame = _currentFrame.Clone() });
        }

        public void Dispose()
        {
            _currentFrame?.Dispose();
            _capture?.Dispose();
        }
    }

}
