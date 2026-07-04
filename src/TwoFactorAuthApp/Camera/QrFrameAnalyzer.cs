using Android.OS;
using AndroidX.Camera.Core;

namespace TwoFactorAuthApp.Camera;

/// <summary>Decodes QR from live camera frames; invokes callback once for a valid otpauth TOTP URI.</summary>
internal sealed class QrFrameAnalyzer(Action<string> onOtpAuthUri, LatestFrameBuffer latestFrame)
    : Java.Lang.Object, ImageAnalysis.IAnalyzer
{
    private readonly Handler _main = new(Looper.MainLooper!);
    private long _lastDecodeMs;
    private volatile bool _done;

    public void TryDecodeLatest(Action<string> onSuccess, Action onFailure)
    {
        string? uri = latestFrame.TryDecode();
        if (uri is not null)
        {
            _done = true;
            onSuccess(uri);
        }
        else
        {
            onFailure();
        }
    }

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
            byte[]? lum = LuminanceExtractor.TryExtract(imageProxy);
            if (lum is null)
                return;

            int rotation = imageProxy.ImageInfo?.RotationDegrees ?? 0;
            latestFrame.Update(lum, imageProxy.Width, imageProxy.Height, rotation);

            long now = SystemClock.UptimeMillis();
            if (now - _lastDecodeMs < 150)
                return;

            _lastDecodeMs = now;

            string? uri = QrDecoder.TryDecodeOtpAuthUri(
                lum, imageProxy.Width, imageProxy.Height, rotation, tryAllRotations: true);
            if (uri is null)
                return;

            _done = true;
            _main.Post(() => onOtpAuthUri(uri));
        }
        finally
        {
            imageProxy.Close();
        }
    }
}
