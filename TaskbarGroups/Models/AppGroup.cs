namespace TaskbarGroups.Models;

/// <summary>
/// Represents a group of apps with a custom icon.
/// </summary>
public class AppGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "New Group";
    public string? IconPath { get; set; }
    public List<AppEntry> Apps { get; set; } = new();
}
