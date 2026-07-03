using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace TwoFactorAuth.Core.Localization;

/// <summary>Shared UI strings for Core (secret / otpauth errors). Language follows <see cref="CultureInfo.CurrentUICulture"/>.</summary>
public static class Loc
{
    private static readonly Dictionary<string, Dictionary<string, string>> Catalog = LoadCatalog();

    public static string T(string key, params object[] args)
    {
        string lang = CurrentLang;
        if (!Catalog.TryGetValue(lang, out Dictionary<string, string>? table))
            table = Catalog["en"];
        if (!table.TryGetValue(key, out string? template))
            Catalog["en"].TryGetValue(key, out template);
        template ??= key;
        return args.Length > 0
            ? string.Format(CultureInfo.CurrentUICulture, template, args)
            : template;
    }

    private static string CurrentLang =>
        CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? "zh-CN" : "en";

    private static Dictionary<string, Dictionary<string, string>> LoadCatalog()
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        Assembly asm = typeof(Loc).Assembly;
        foreach (string lang in new[] { "en", "zh-CN" })
        {
            string name = $"TwoFactorAuth.Core.Localization.strings.{lang}.json";
            using Stream? stream = asm.GetManifestResourceStream(name);
            if (stream is null)
                continue;
            using var reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                       ?? new Dictionary<string, string>();
            result[lang] = dict;
        }

        if (!result.ContainsKey("en"))
            result["en"] = new Dictionary<string, string>();
        return result;
    }
}
