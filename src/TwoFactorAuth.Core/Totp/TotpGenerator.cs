using System.Buffers.Binary;
using System.Security.Cryptography;

namespace TwoFactorAuth.Core.Totp;

/// <summary>RFC 6238 TOTP (SHA-1, 30s period) for authenticator-style apps.</summary>
public static class TotpGenerator
{
    /// <summary>当前时刻的 TOTP。密钥无法解码时返回与位数相同的 <c>'?'</c>，避免展示崩溃。</summary>
    public static string Generate(string base32Secret, int digits = 6, int periodSeconds = 30)
    {
        if (!SecretBytes.TryDecodeSecret(base32Secret, out byte[]? key, out _) || key is null || key.Length == 0)
            return new string('?', digits);
        long counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / periodSeconds;
        return GenerateAtCounter(key, counter, digits);
    }

    public static int SecondsRemaining(int periodSeconds = 30)
    {
        int sec = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % periodSeconds);
        return periodSeconds - sec;
    }

    public static string GenerateAtCounter(byte[] key, long counter, int digits)
    {
        Span<byte> counterBytes = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(counterBytes, counter);

        Span<byte> hash = stackalloc byte[20];
        HMACSHA1.HashData(key, counterBytes, hash);

        int offset = hash[19] & 0x0F;
        int binary =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);
        int mod = (int)Math.Pow(10, digits);
        int otp = binary % mod;
        return otp.ToString(new string('0', digits));
    }
}

/// <summary>Base32 (RFC 4648) without padding.</summary>
public static class Base32
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static byte[] Decode(string input)
    {
        string s = input.Trim().Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
        int buffer = 0;
        int bitsLeft = 0;
        using var ms = new MemoryStream();
        foreach (char c in s)
        {
            if (c == '=') break;
            int val = Alphabet.IndexOf(c);
            if (val < 0) continue;
            buffer = (buffer << 5) | val;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                bitsLeft -= 8;
                ms.WriteByte((byte)(buffer >> bitsLeft));
            }
        }
        return ms.ToArray();
    }
}
