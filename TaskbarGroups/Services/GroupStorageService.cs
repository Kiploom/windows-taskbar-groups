using System.Text.Json;
using TaskbarGroups.Models;

namespace TaskbarGroups.Services;

/// <summary>
/// Handles persistence of app groups to JSON in %AppData%.
/// </summary>
public class GroupStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _configPath;
    private List<AppGroup> _groups = new();

    public GroupStorageService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "TaskbarGroups");
        Directory.CreateDirectory(appFolder);
        _configPath = Path.Combine(appFolder, "groups.json");
    }

    public IReadOnlyList<AppGroup> Groups => _groups.AsReadOnly();

    public void Load()
    {
        if (!File.Exists(_configPath))
        {
            _groups = new List<AppGroup>();
            return;
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            var loaded = JsonSerializer.Deserialize<List<AppGroup>>(json, JsonOptions);
            _groups = loaded ?? new List<AppGroup>();
        }
        catch
        {
            _groups = new List<AppGroup>();
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_groups, JsonOptions);
        File.WriteAllText(_configPath, json);
    }

    public void AddGroup(AppGroup group)
    {
        _groups.Add(group);
        Save();
    }

    public void RemoveGroup(AppGroup group)
    {
        _groups.Remove(group);
        Save();
    }

    public void UpdateGroup(AppGroup group)
    {
        var index = _groups.FindIndex(g => g.Id == group.Id);
        if (index >= 0)
        {
            _groups[index] = group;
            Save();
        }
    }

    public AppGroup? GetGroupById(string id) => _groups.FirstOrDefault(g => g.Id == id);
}
