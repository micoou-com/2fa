using System.Globalization;

namespace TwoFactorAuth.Win;

internal static class CultureSetup
{
    internal static void Apply()
    {
        string name = CultureInfo.CurrentUICulture.Name;
        string culture = name.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? "zh-CN" : "en";
        var ci = new CultureInfo(culture);
        CultureInfo.CurrentUICulture = ci;
        CultureInfo.CurrentCulture = ci;
    }
}
