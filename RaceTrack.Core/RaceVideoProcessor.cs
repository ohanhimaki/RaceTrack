using RaceTrack.Core.Services;

namespace RaceTrack.Core;
public class RaceVideoProcessor
{
    private readonly IVideoCaptureService _videoCaptureService;
    private readonly ILapDetectionService _lapDetectionService;

    public RaceVideoProcessor(IVideoCaptureService videoCaptureService, ILapDetectionService lapDetectionService)
    {
        _videoCaptureService = videoCaptureService;
        _lapDetectionService = lapDetectionService;

        _videoCaptureService.FrameCaptured += OnFrameCaptured;
        _lapDetectionService.LapDetected += OnLapDetected;
    }

    private void OnFrameCaptured(object sender, FrameCapturedEventArgs e)
    {
        _lapDetectionService.ProcessFrame(e.Frame);
    }

    private void OnLapDetected(object sender, LapDetectedEventArgs e)
    {
        // Handle lap detection event
    }

    public void Start()
    {
        _videoCaptureService.StartCapture();
    }

    public void Stop()
    {
        _videoCaptureService.StopCapture();
    }
}
