namespace RaceTrack;

public static class PointExtensions
{
    public static System.Drawing.Point ToDrawingPoint(this System.Windows.Point wpfPoint)
    {
        return new System.Drawing.Point((int)wpfPoint.X, (int)wpfPoint.Y);
    }

    public static System.Drawing.Point? ToNullableDrawingPoint(this System.Windows.Point wpfPoint)
    {
        return new System.Drawing.Point((int)wpfPoint.X, (int)wpfPoint.Y);
    }
}
