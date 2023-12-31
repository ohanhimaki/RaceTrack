﻿using Emgu.CV;

namespace RaceTrack.Core.Services;

public interface IVideoCaptureService
{
    event EventHandler<FrameCapturedEventArgs> FrameCaptured;
    void StartCapture();
    void StopCapture();
    void Dispose();
}

public class FrameCapturedEventArgs : EventArgs
{
    public Mat Frame { get; set; }
}
