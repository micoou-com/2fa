using System.Globalization;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace TwoFactorAuthApp;

[Application]
public class TwoFactorApp : Application
{
    public TwoFactorApp(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();
        LocaleHelper.Apply(Java.Util.Locale.Default);
    }
}

internal static class LocaleHelper
{
    internal static void Apply(Java.Util.Locale? locale)
    {
        string culture = locale?.Language == "zh" ? "zh-CN" : "en";
        var ci = new CultureInfo(culture);
        CultureInfo.CurrentUICulture = ci;
        CultureInfo.CurrentCulture = ci;
    }
}
