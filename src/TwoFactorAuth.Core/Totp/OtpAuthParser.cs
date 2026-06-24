namespace TwoFactorAuth.Core.Totp;

public sealed class OtpAuthEntry
{
    public string Issuer { get; init; } = "";
    public string AccountName { get; init; } = "";
    public string SecretBase32 { get; init; } = "";
    public int Digits { get; init; } = 6;
    public int PeriodSeconds { get; init; } = 30;
}

public static class OtpAuthParser
{
    public static bool TryParse(string raw, out OtpAuthEntry? entry, out string? error)
    {
        entry = null;
        error = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "empty";
            return false;
        }

        raw = raw.Trim();
        if (raw.StartsWith("otpauth://hotp/", StringComparison.OrdinalIgnoreCase))
        {
            error = "hotp_not_supported";
            return false;
        }
        if (!raw.StartsWith("otpauth://totp/", StringComparison.OrdinalIgnoreCase))
        {
            error = "not_otpauth";
            return false;
        }

        try
        {
            var uri = new Uri(raw);
            string path = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            string issuer = "";
            string account = path;
            int colon = path.IndexOf(':');
            if (colon >= 0)
            {
                issuer = path[..colon];
                account = path[(colon + 1)..];
            }

            var query = ParseQuery(uri.Query);
            if (!query.TryGetValue("secret", out string? secret) || string.IsNullOrEmpty(secret))
            {
                error = "no_secret";
                return false;
            }

            secret = SecretBytes.NormalizeSeparators(secret);

            if (query.TryGetValue("issuer", out string? qIssuer) && !string.IsNullOrEmpty(qIssuer))
                issuer = Uri.UnescapeDataString(qIssuer);

            int digits = 6;
            if (query.TryGetValue("digits", out string? ds) && int.TryParse(ds, out int d) && d is 6 or 8)
                digits = d;

            int period = 30;
            if (query.TryGetValue("period", out string? ps) && int.TryParse(ps, out int p) && p > 0 && p <= 120)
                period = p;

            entry = new OtpAuthEntry
            {
                Issuer = issuer,
                AccountName = account,
                SecretBase32 = secret,
                Digits = digits,
                PeriodSeconds = period
            };
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(query))
            return d;
        string q = query.StartsWith('?') ? query[1..] : query;
        foreach (string part in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            int eq = part.IndexOf('=');
            if (eq < 0)
                d[Uri.UnescapeDataString(part)] = "";
            else
            {
                string k = Uri.UnescapeDataString(part[..eq]);
                string v = Uri.UnescapeDataString(part[(eq + 1)..]);
                d[k] = v;
            }
        }
        return d;
    }
}
