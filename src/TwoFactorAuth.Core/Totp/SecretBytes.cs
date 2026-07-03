using System.Diagnostics.CodeAnalysis;
using System.Text;
using TwoFactorAuth.Core.Localization;

namespace TwoFactorAuth.Core.Totp;

/// <summary>
/// TOTP 共享密钥：仅接受 **RFC 4648 Base32**（与 <c>otpauth</c> 的 <c>secret</c> 一致），规范化后解码为字节参与 RFC 6238。
/// </summary>
public static class SecretBytes
{
    /// <summary>规范化后 Base32 主体最少字符数（约 5 字节密钥）。</summary>
    public const int MinNormalizedBase32Length = 8;

    /// <summary>删除空白、ASCII/Unicode 连字符（便于抄写分段）；不改变其余字符大小写。</summary>
    public static string NormalizeSeparators(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        Span<char> buf = stackalloc char[input.Length];
        int n = 0;
        foreach (char c in input)
        {
            if (char.IsWhiteSpace(c))
                continue;
            if (c is '-' or '\u2013' or '\u2014' or '\u2212')
                continue;
            buf[n++] = c;
        }

        return new string(buf[..n]);
    }

    /// <summary>将用户输入规范化为 Base32 串后解码；**不支持**十六进制、恢复码或其它编码。</summary>
    /// <param name="error">失败时为稳定错误码，见 <see cref="DescribeDecodeError"/>。</param>
    public static bool TryDecodeSecret(string? input, [NotNullWhen(true)] out byte[]? key, out string? error)
    {
        key = null;
        error = null;
        string raw = input ?? "";
        string t = NormalizeSeparators(raw);
        if (t.Length == 0)
        {
            error = "empty";
            return false;
        }

        string? kind = ClassifyRejectedInput(raw, t);
        if (kind is not null)
        {
            error = kind;
            return false;
        }

        if (!IsStrictBase32Format(t))
        {
            error = "base32_invalid";
            return false;
        }

        if (t.Length < MinNormalizedBase32Length)
        {
            error = "base32_too_short";
            return false;
        }

        byte[] decoded = Base32.Decode(t);
        if (decoded.Length == 0)
        {
            error = "base32_empty";
            return false;
        }

        key = decoded;
        return true;
    }

    /// <summary>兼容旧调用：解码失败时返回长度为 0 的数组。</summary>
    public static byte[] DecodeSecret(string input) =>
        TryDecodeSecret(input, out byte[]? k, out _) && k is not null ? k : [];

    /// <summary>将 <see cref="TryDecodeSecret"/> 的错误码转为本地化说明（供 UI 提示）。</summary>
    public static string DescribeDecodeError(string? error) =>
        error switch
        {
            "empty" => Loc.T("secret.empty"),
            "recovery_code_likely" => Loc.T("secret.recovery_code_likely"),
            "recovery_code_wrong_count" => Loc.T("secret.recovery_code_wrong_count"),
            "hex_likely" => Loc.T("secret.hex_likely"),
            "base32_invalid" => Loc.T("secret.base32_invalid"),
            "base32_too_short" => Loc.T("secret.base32_too_short", MinNormalizedBase32Length),
            "base32_empty" => Loc.T("secret.base32_empty"),
            _ => Loc.T("secret.invalid"),
        };

    /// <summary>在严格 Base32 校验前识别恢复码、十六进制等误粘贴。</summary>
    private static string? ClassifyRejectedInput(string raw, string normalized)
    {
        List<string> segments = SplitSegments(raw);
        string? recovery = ClassifyRecoveryLayout(segments, normalized);
        if (recovery is not null)
            return recovery;

        if (LooksLikeHex(normalized))
            return "hex_likely";

        if (ContainsBase32ExcludedDigit(normalized))
            return "recovery_code_likely";

        return null;
    }

  private static string? ClassifyRecoveryLayout(List<string> segments, string normalized)
    {
        if (segments.Count < 2)
            return null;

        int len = segments[0].Length;
        if (len is not (4 or 5) || !segments.All(s => s.Length == len))
            return null;

        bool recoveryAlphabet = segments.Any(s => SegmentLooksLikeRecoveryAlphabet(s));
        if (!recoveryAlphabet)
            return null;

        if (len == 4 && segments.Count != 8)
            return "recovery_code_wrong_count";
        if (len == 5 && segments.Count != 5)
            return "recovery_code_wrong_count";

        return "recovery_code_likely";
    }

    private static bool SegmentLooksLikeRecoveryAlphabet(string segment)
    {
        if (segment.All(char.IsDigit))
            return true;

        foreach (char c in segment)
        {
            if (c is '0' or '1' or '8' or '9')
                return true;
        }

        return false;
    }

    private static bool ContainsBase32ExcludedDigit(string normalized)
    {
        foreach (char c in normalized.ToUpperInvariant())
        {
            if (c is '0' or '1' or '8' or '9')
                return true;
        }

        return false;
    }

    private static bool LooksLikeHex(string normalized)
    {
        if (normalized.Length < 4)
            return false;

        bool hasHexLetter = false;
        foreach (char c in normalized)
        {
            if (c is >= '0' and <= '9')
                continue;
            char u = char.ToUpperInvariant(c);
            if (u is >= 'A' and <= 'F')
            {
                hasHexLetter = true;
                continue;
            }

            return false;
        }

        return hasHexLetter || ContainsBase32ExcludedDigit(normalized);
    }

    private static List<string> SplitSegments(string input)
    {
        var list = new List<string>();
        var sb = new StringBuilder();
        foreach (char c in input)
        {
            if (char.IsWhiteSpace(c) || c is '-' or '\u2013' or '\u2014' or '\u2212')
            {
                if (sb.Length > 0)
                {
                    list.Add(sb.ToString());
                    sb.Clear();
                }

                continue;
            }

            sb.Append(c);
        }

        if (sb.Length > 0)
            list.Add(sb.ToString());

        return list;
    }

    private static bool IsStrictBase32Format(string t)
    {
        string u = t.ToUpperInvariant();
        if (u.Length == 0)
            return false;

        int eq = u.IndexOf('=');
        ReadOnlySpan<char> body = eq < 0 ? u : u.AsSpan(0, eq);
        ReadOnlySpan<char> pad = eq < 0 ? ReadOnlySpan<char>.Empty : u.AsSpan(eq);
        if (!pad.IsEmpty)
        {
            for (int i = 0; i < pad.Length; i++)
            {
                if (pad[i] != '=')
                    return false;
            }
        }

        if (body.IsEmpty)
            return false;

        for (int i = 0; i < body.Length; i++)
        {
            char c = body[i];
            if (c is >= '2' and <= '7')
                continue;
            if (c is >= 'A' and <= 'Z')
                continue;
            return false;
        }

        return true;
    }
}
