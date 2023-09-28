using AForge.Video.DirectShow;
using Emgu.CV;
using RaceTrack.Core.Services;

namespace RaceTrack.Video.Services
{
    public class VideoCaptureService : IVideoCaptureService
    {
        private VideoCapture _capture;
        private Mat? _currentFrame;

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

        public List<string> GetCameraList()
        {
            var cameraList = new List<string>();

            // Get available cameras
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            foreach (FilterInfo device in videoDevices)
            {
                cameraList.Add(device.Name);
            }

            return cameraList;
        }

        public void SetCamera(int cameraIndex)
        {
            _capture.Stop();
            _capture = new VideoCapture(cameraIndex);
            _capture.ImageGrabbed += Capture_ImageGrabbed;
            _capture.Start();
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

        public Task<string> GetCurrentFrame()
        {
            while (_currentFrame.IsEmpty)
            {
                // wait for frame to be captured
            }

            var basePath = @".\Images\";
            var fileName = basePath + "latest" + ".jpg";
            _currentFrame.Save(fileName);
            return Task.FromResult(fileName);
        }
    }
}