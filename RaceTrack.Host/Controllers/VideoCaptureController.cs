using Microsoft.AspNetCore.Mvc;
using RaceTrack.Core.Services;

namespace RaceTrack.Host.Controllers;

[ApiController]
[Route("[controller]")]
public class VideoCaptureController : ControllerBase
{
    
    private readonly IVideoCaptureService _videoCaptureService;

    public VideoCaptureController(
        IVideoCaptureService videoCaptureService)
    {
        _videoCaptureService = videoCaptureService;
    }
    [HttpGet("[action]")]
    public async Task<IActionResult> GetLatestImage()
    {
        //TODO fix this 
        var currentFrame = await _videoCaptureService.GetCurrentFrame();
        var file = System.IO.File.OpenRead(currentFrame);
        
        return File(file, "image/jpeg");
    }
}