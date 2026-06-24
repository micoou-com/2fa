using System.Text.Json;

namespace TwoFactorAuth.Core.Data;

public interface IAccountStore
{
    List<AccountEntry> Load();
    void Save(IReadOnlyList<AccountEntry> accounts);
    void Add(AccountEntry entry);
    void Remove(string id);
}

/// <summary>Persists accounts to a single JSON file (Android files dir or Windows AppData).</summary>
public sealed class JsonAccountStore(string filePath) : IAccountStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string _path = filePath;

    public List<AccountEntry> Load()
    {
        try
        {
            if (!File.Exists(_path))
                return [];
            string json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<List<AccountEntry>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IReadOnlyList<AccountEntry> accounts)
    {
        string? dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        string json = JsonSerializer.Serialize(accounts.ToList(), JsonOptions);
        File.WriteAllText(_path, json);
    }

    public void Add(AccountEntry entry)
    {
        var list = Load();
        list.Add(entry);
        Save(list);
    }

    public void Remove(string id)
    {
        var list = Load();
        list.RemoveAll(a => a.Id == id);
        Save(list);
    }
}
