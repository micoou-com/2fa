using TwoFactorAuth.Core.Data;
using TwoFactorAuth.Core.Totp;
using Xunit;

namespace TwoFactorAuth.Logic.Tests;

public class TotpAndOtpAuthTests
{
    [Fact]
    public void Totp_GenerateAtCounter_DigitsAndDeterministic()
    {
        byte[] key = Base32.Decode("JBSWY3DPEHPK3PXP");
        string a = TotpGenerator.GenerateAtCounter(key, 123456789L, digits: 6);
        string b = TotpGenerator.GenerateAtCounter(key, 123456789L, digits: 6);
        Assert.Equal(6, a.Length);
        Assert.True(a.All(char.IsDigit));
        Assert.Equal(a, b);
    }

    [Fact]
    public void OtpAuth_ParseGoogleStyle_ReturnsSecret()
    {
        const string uri =
            "otpauth://totp/Example:alice@google.com?secret=JBSWY3DPEHPK3PXP&issuer=Example";
        bool ok = OtpAuthParser.TryParse(uri, out OtpAuthEntry? e, out string? err);
        Assert.True(ok, err ?? "");
        Assert.NotNull(e);
        Assert.Equal("Example", e!.Issuer);
        Assert.Equal("alice@google.com", e.AccountName);
        Assert.Contains("JBSWY3DPEHPK3PXP", e.SecretBase32, StringComparison.Ordinal);
    }

    [Fact]
    public void OtpAuth_Hotp_Rejected()
    {
        const string uri = "otpauth://hotp/Label?secret=ABCDEF";
        bool ok = OtpAuthParser.TryParse(uri, out _, out string? err);
        Assert.False(ok);
        Assert.Equal("hotp_not_supported", err);
    }

    [Fact]
    public void AccountEntry_Defaults()
    {
        var a = new AccountEntry { SecretBase32 = "ABCD", Issuer = "T" };
        Assert.Equal(6, a.Digits);
        Assert.Equal(30, a.PeriodSeconds);
    }

    [Fact]
    public void SecretBytes_Normalize_RemovesSpacesAndHyphens()
    {
        string n = SecretBytes.NormalizeSeparators(" JBSWY-3DPE-HPK3-PXP ");
        Assert.Equal("JBSWY3DPEHPK3PXP", n);
    }

    [Fact]
    public void SecretBytes_Decode_DashedBase32_EqualsPlain()
    {
        byte[] a = SecretBytes.DecodeSecret("JBSWY-3DPE-HPK3-PXP");
        byte[] b = Base32.Decode("JBSWY3DPEHPK3PXP");
        Assert.Equal(b, a);
    }

    [Fact]
    public void SecretBytes_Decode_Base32Unchanged()
    {
        byte[] a = SecretBytes.DecodeSecret("JBSWY3DPEHPK3PXP");
        byte[] b = Base32.Decode("JBSWY3DPEHPK3PXP");
        Assert.Equal(b, a);
    }

    [Fact]
    public void SecretBytes_TryDecode_HexOnly_Rejected()
    {
        bool ok = SecretBytes.TryDecodeSecret("02F0CC9D37", out _, out string? err);
        Assert.False(ok);
        Assert.Equal("base32_invalid", err);
    }

    [Fact]
    public void SecretBytes_TryDecode_Base32InvalidChar_Fails()
    {
        bool ok = SecretBytes.TryDecodeSecret("JBSW!Y3DPEHPK3PXP", out _, out string? err);
        Assert.False(ok);
        Assert.Equal("base32_invalid", err);
    }

    [Fact]
    public void SecretBytes_TryDecode_EmptyAfterNormalize_Fails()
    {
        bool ok = SecretBytes.TryDecodeSecret("  \n--  ", out _, out string? err);
        Assert.False(ok);
        Assert.Equal("empty", err);
    }

    [Fact]
    public void Totp_Generate_InvalidSecret_ReturnsPlaceholders()
    {
        string s = TotpGenerator.Generate("not-valid-!!!", digits: 6);
        Assert.Equal("??????", s);
    }

    [Fact]
    public void SecretBytes_Base32_DecodeConsistent()
    {
        const string t = "42424242";
        Assert.True(SecretBytes.TryDecodeSecret(t, out byte[]? key, out _), "decode");
        Assert.Equal(Base32.Decode(t), key);
    }
}
