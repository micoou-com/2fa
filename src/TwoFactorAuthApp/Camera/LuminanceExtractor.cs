using Android.Media;
using AndroidX.Camera.Core;

namespace TwoFactorAuthApp.Camera;

internal static class LuminanceExtractor
{
    /// <summary>Build grayscale buffer for ZXing: RGBA8888 frame, or Y plane from YUV_420_888.</summary>
    public static byte[]? TryExtract(IImageProxy proxy)
    {
        Image? image = proxy.Image;
        if (image is null)
            return null;

        int w = proxy.Width;
        int h = proxy.Height;
        Image.Plane[]? planes = image.GetPlanes();
        if (planes is null || planes.Length == 0)
            return null;

        int p0 = planes[0].Buffer!.Remaining();
        if (p0 == w * h * 4)
            return FromRgba8888(planes[0], w, h);

        return FromYPlane420(planes[0], w, h);
    }

    private static byte[] FromRgba8888(Image.Plane plane, int w, int h)
    {
        var buf = plane.Buffer!;
        buf.Rewind();
        int n = w * h;
        var lum = new byte[n];
        for (int i = 0; i < n; i++)
        {
            int r = buf.Get() & 0xff;
            int g = buf.Get() & 0xff;
            int b = buf.Get() & 0xff;
            _ = buf.Get(); // alpha
            lum[i] = (byte)((r * 19595 + g * 38469 + b * 7472) >> 16);
        }

        return lum;
    }

    private static byte[] FromYPlane420(Image.Plane yPlane, int w, int h)
    {
        var buf = yPlane.Buffer!;
        buf.Rewind();
        int rowStride = yPlane.RowStride;
        int pixelStride = yPlane.PixelStride;
        var lum = new byte[w * h];

        if (pixelStride == 1 && rowStride == w)
        {
            buf.Get(lum, 0, w * h);
            return lum;
        }

        for (int row = 0; row < h; row++)
        {
            int rowStart = row * rowStride;
            for (int col = 0; col < w; col++)
                lum[row * w + col] = (byte)(buf.Get(rowStart + col * pixelStride) & 0xff);
        }

        return lum;
    }
}
