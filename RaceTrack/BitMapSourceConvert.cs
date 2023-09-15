using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;

public static class BitmapSourceConvert
{
    public static BitmapSource ToBitmapSource(Mat image)
    {
        Image<Bgr, Byte> img = image.ToImage<Bgr, Byte>();

        PixelFormat format = PixelFormats.Bgr24;

        BitmapSource bitmapSource = BitmapSource.Create(
            img.Width,
            img.Height,
            96, 96,
            format,
            null,
            img.MIplImage.ImageData,
            img.Width * img.NumberOfChannels * img.Height,
            img.Width * img.NumberOfChannels);

        return bitmapSource;
    }
}
