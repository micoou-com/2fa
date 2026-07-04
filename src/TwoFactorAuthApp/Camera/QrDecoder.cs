using Android.Graphics;
using TwoFactorAuth.Core.Totp;
using ZXing;
using ZXing.Common;

namespace TwoFactorAuthApp.Camera;

internal static class QrDecoder
{
    private static readonly Dictionary<DecodeHintType, object?> Hints = new()
    {
        { DecodeHintType.POSSIBLE_FORMATS, new List<BarcodeFormat> { BarcodeFormat.QR_CODE } },
        { DecodeHintType.TRY_HARDER, true }
    };

    public static string? TryDecodeOtpAuthUri(byte[] luminance, int width, int height, int rotationDegrees, bool tryAllRotations = false)
    {
        int[] rotations = tryAllRotations
            ? [rotationDegrees, (rotationDegrees + 90) % 360, (rotationDegrees + 180) % 360, (rotationDegrees + 270) % 360]
            : [rotationDegrees];

        foreach (int rot in rotations)
        {
            (byte[] data, int w, int h) = RotateClockwise(luminance, width, height, rot);
            string? text = TryDecodeRaw(data, w, h);
            if (text is not null)
                return text;
        }

        return null;
    }

    public static string? TryDecodeFromJpeg(byte[] jpegBytes)
    {
        Bitmap? bitmap = BitmapFactory.DecodeByteArray(jpegBytes, 0, jpegBytes.Length);
        if (bitmap is null)
            return null;

        try
        {
            byte[] lum = BitmapToLuminance(bitmap);
            return TryDecodeRaw(lum, bitmap.Width, bitmap.Height);
        }
        finally
        {
            bitmap.Recycle();
        }
    }

    private static string? TryDecodeRaw(byte[] luminance, int width, int height)
    {
        string? text = DecodeOnce(luminance, width, height, false);
        if (text is not null)
            return text;

        return DecodeOnce(luminance, width, height, true);
    }

    private static string? DecodeOnce(byte[] luminance, int width, int height, bool invert)
    {
        try
        {
            byte[] data = luminance;
            if (invert)
            {
                data = new byte[luminance.Length];
                for (int i = 0; i < luminance.Length; i++)
                    data[i] = (byte)(255 - luminance[i]);
            }

            var source = new RGBLuminanceSource(data, width, height);
            var bin = new BinaryBitmap(new HybridBinarizer(source));
            var reader = new MultiFormatReader { Hints = Hints };
            string? text = reader.decode(bin)?.Text;
            if (IsValidOtpAuthUri(text))
                return text;

            bin = new BinaryBitmap(new GlobalHistogramBinarizer(source));
            text = reader.decode(bin)?.Text;
            return IsValidOtpAuthUri(text) ? text : null;
        }
        catch (ReaderException)
        {
            return null;
        }
    }

    private static bool IsValidOtpAuthUri(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return false;
        if (!text.StartsWith("otpauth://totp/", StringComparison.OrdinalIgnoreCase))
            return false;

        return OtpAuthParser.TryParse(text, out OtpAuthEntry? entry, out _) && entry is not null;
    }

    private static byte[] BitmapToLuminance(Bitmap bitmap)
    {
        int w = bitmap.Width;
        int h = bitmap.Height;
        int[] pixels = new int[w * h];
        bitmap.GetPixels(pixels, 0, w, 0, 0, w, h);

        var lum = new byte[w * h];
        for (int i = 0; i < pixels.Length; i++)
        {
            int px = pixels[i];
            int r = (px >> 16) & 0xff;
            int g = (px >> 8) & 0xff;
            int b = px & 0xff;
            lum[i] = (byte)((r * 19595 + g * 38469 + b * 7472) >> 16);
        }

        return lum;
    }

    private static (byte[] Data, int Width, int Height) RotateClockwise(byte[] src, int w, int h, int degrees)
    {
        degrees = ((degrees % 360) + 360) % 360;
        if (degrees == 0)
            return (src, w, h);

        if (degrees == 90)
        {
            int nw = h;
            int nh = w;
            var dst = new byte[nw * nh];
            for (int row = 0; row < h; row++)
            {
                for (int col = 0; col < w; col++)
                    dst[row + (w - 1 - col) * nw] = src[col + row * w];
            }

            return (dst, nw, nh);
        }

        if (degrees == 180)
        {
            var dst = new byte[w * h];
            for (int i = 0; i < w * h; i++)
                dst[i] = src[w * h - 1 - i];
            return (dst, w, h);
        }

        if (degrees == 270)
        {
            int nw = h;
            int nh = w;
            var dst = new byte[nw * nh];
            for (int row = 0; row < h; row++)
            {
                for (int col = 0; col < w; col++)
                    dst[h - 1 - row + col * nw] = src[col + row * w];
            }

            return (dst, nw, nh);
        }

        return (src, w, h);
    }
}
