namespace TaskbarGroups.Models;

/// <summary>
/// Represents a single app entry within a group.
/// </summary>
public class AppEntry
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Arguments { get; set; }
    public string? WorkingDirectory { get; set; }
}
