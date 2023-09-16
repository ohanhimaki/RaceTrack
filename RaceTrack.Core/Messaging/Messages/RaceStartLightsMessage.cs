using System.Drawing;

namespace RaceTrack.Core.Messaging.Messages;

public class RaceStartLightsMessage
{
    // lights from 1 to 5, visibility and color
    public Color Light1Fill { get; set; }
    public Color Light2Fill { get; set; }
    public Color Light3Fill { get; set; }
    public Color Light4Fill { get; set; }
    public Color Light5Fill { get; set; }
    public bool Light1Visible { get; set; }
    public bool Light2Visible { get; set; }
    public bool Light3Visible { get; set; }
    public bool Light4Visible { get; set; }
    public bool Light5Visible { get; set; }
    public bool StartButtonEnabled { get; set; }
}