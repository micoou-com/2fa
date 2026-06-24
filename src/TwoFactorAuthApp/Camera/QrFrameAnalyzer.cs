using Android.OS;
using AndroidX.Camera.Core;
using TwoFactorAuth.Core.Totp;
using ZXing;
using ZXing.Common;

namespace TwoFactorAuthApp.Camera;

/// <summary>Decodes QR from camera frames; invokes callback once for valid otpauth TOTP URI.</summary>
internal sealed class QrFrameAnalyzer(Action<string> onOtpAuthUri) : Java.Lang.Object, ImageAnalysis.IAnalyzer
{
    private static readonly Dictionary<DecodeHintType, object?> Hints = new()
    {
        { DecodeHintType.POSSIBLE_FORMATS, new List<BarcodeFormat> { BarcodeFormat.QR_CODE } },
        { DecodeHintType.TRY_HARDER, true }
    };

    private readonly Handler _main = new(Looper.MainLooper!);
    private long _lastDecodeMs;
    private volatile bool _done;

    public void Analyze(IImageProxy? imageProxy)
    {
        if (imageProxy is null)
            return;

        if (_done)
        {
            imageProxy.Close();
            return;
        }

        try
        {
            long now = SystemClock.UptimeMillis();
            if (now - _lastDecodeMs < 250)
                return;

            _lastDecodeMs = now;

            byte[]? lum = LuminanceExtractor.TryExtract(imageProxy);
            if (lum is null)
                return;

            int w = imageProxy.Width;
            int h = imageProxy.Height;
            var source = new RGBLuminanceSource(lum, w, h);
            var bin = new BinaryBitmap(new HybridBinarizer(source));
            var reader = new MultiFormatReader { Hints = Hints };
            ZXing.Result? decoded = reader.decode(bin);
            string? text = decoded?.Text;
            if (text is null || !text.StartsWith("otpauth://totp/", StringComparison.OrdinalIgnoreCase))
                return;
            if (!OtpAuthParser.TryParse(text, out OtpAuthEntry? entry, out _) || entry is null)
                return;

            _done = true;
            _main.Post(() => onOtpAuthUri(text));
        }
        catch (ReaderException)
        {
            // no QR in frame
        }
        catch (Exception)
        {
            // ignore frame
        }
        finally
        {
            imageProxy.Close();
        }
    }
}
