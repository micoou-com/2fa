namespace TwoFactorAuthApp.Camera;

/// <summary>Holds the most recent camera frame for manual capture decoding.</summary>
internal sealed class LatestFrameBuffer
{
    private readonly object _lock = new();
    private byte[]? _luminance;
    private int _width;
    private int _height;
    private int _rotation;

    public void Update(byte[] luminance, int width, int height, int rotation)
    {
        lock (_lock)
        {
            if (_luminance is null || _luminance.Length != luminance.Length)
                _luminance = new byte[luminance.Length];
            Buffer.BlockCopy(luminance, 0, _luminance, 0, luminance.Length);
            _width = width;
            _height = height;
            _rotation = rotation;
        }
    }

    public string? TryDecode()
    {
        lock (_lock)
        {
            if (_luminance is null || _width <= 0 || _height <= 0)
                return null;

            return QrDecoder.TryDecodeOtpAuthUri(_luminance, _width, _height, _rotation, tryAllRotations: true);
        }
    }
}
