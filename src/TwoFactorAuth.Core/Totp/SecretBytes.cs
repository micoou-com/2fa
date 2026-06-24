using System.Diagnostics.CodeAnalysis;

namespace TwoFactorAuth.Core.Totp;

/// <summary>
/// TOTP 共享密钥：仅接受 **RFC 4648 Base32**（与 <c>otpauth</c> 的 <c>secret</c> 一致），规范化后解码为字节参与 RFC 6238。
/// </summary>
public static class SecretBytes
{
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

    /// <summary>将用户输入规范化为 Base32 串后解码；**不支持**十六进制或其它编码。</summary>
    /// <param name="error">失败时为稳定错误码：<c>empty</c>、<c>base32_invalid</c>、<c>base32_empty</c>。</param>
    public static bool TryDecodeSecret(string? input, [NotNullWhen(true)] out byte[]? key, out string? error)
    {
        key = null;
        error = null;
        string t = NormalizeSeparators(input ?? "");
        if (t.Length == 0)
        {
            error = "empty";
            return false;
        }

        if (!IsStrictBase32Format(t))
        {
            error = "base32_invalid";
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

    /// <summary>将 <see cref="TryDecodeSecret"/> 的错误码转为简短中文说明（供 UI 提示）。</summary>
    public static string DescribeDecodeError(string? error) =>
        error switch
        {
            "empty" => "密钥不能为空。",
            "base32_invalid" => "须为 RFC 4648 Base32（字母 A–Z、数字 2–7，可选末尾 =）。",
            "base32_empty" => "密钥过短，无法组成有效字节。",
            _ => "密钥格式无效。",
        };

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
