using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TaskbarGroups.Models;
using TaskbarGroups.Services;
using TaskbarGroups.Windows;

namespace TaskbarGroups;

public sealed partial class App : Application
{
    private GroupStorageService _storage = null!;
    private SettingsWindow? _settingsWindow;
    private TaskbarIcon? _mainTrayIcon;
    private readonly Dictionary<string, (TaskbarIcon Icon, FlyoutWindow Flyout)> _groupTrayIcons = new();

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _storage = new GroupStorageService();
        _storage.Load();

        EnsureTrayIconsForGroups();

        // Create a main tray icon for app-level actions (Settings, Exit)
        _mainTrayIcon = new TaskbarIcon
        {
            ToolTipText = "Taskbar Groups",
            IconSource = new BitmapImage(new Uri("ms-appx:///Assets/default-group-icon.png")),
            ContextMenu = CreateMainContextMenu(),
            MenuActivation = H.NotifyIcon.Core.PopupActivation.LeftOrRightClick
        };

        _mainTrayIcon.ForceCreate();
    }

    private void EnsureTrayIconsForGroups()
    {
        var currentIds = _storage.Groups.Select(g => g.Id).ToHashSet();
        var toRemove = _groupTrayIcons.Keys.Where(id => !currentIds.Contains(id)).ToList();
        foreach (var id in toRemove)
        {
            _groupTrayIcons[id].Icon.Dispose();
            _groupTrayIcons.Remove(id);
        }

        foreach (var group in _storage.Groups)
        {
            if (_groupTrayIcons.ContainsKey(group.Id)) continue;

            var flyout = new FlyoutWindow(group);
            var icon = new TaskbarIcon
            {
                ToolTipText = group.Name,
                IconSource = GetGroupIconSource(group),
                ContextMenu = CreateGroupContextMenu(group),
                MenuActivation = H.NotifyIcon.Core.PopupActivation.LeftOrRightClick,
                TrayPopup = CreateGroupFlyoutContent(group, flyout),
                PopupActivation = H.NotifyIcon.Core.PopupActivation.MouseEnter
            };

            icon.TrayMouseEnter += (s, e) =>
            {
                if (group.Apps.Count > 0)
                {
                    flyout.ShowAboveTaskbar();
                }
            };

            icon.ForceCreate();
            _groupTrayIcons[group.Id] = (icon, flyout);
        }
    }

    private UIElement CreateGroupFlyoutContent(AppGroup group, FlyoutWindow flyout)
    {
        var border = new Border
        {
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(245, 43, 43, 43)),
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            Child = new TextBlock { Text = "Loading...", Padding = new Thickness(8) }
        };

        return border;
    }

    private static BitmapImage GetGroupIconSource(AppGroup group)
    {
        if (!string.IsNullOrEmpty(group.IconPath) && File.Exists(group.IconPath))
        {
            try
            {
                return new BitmapImage(new Uri("file:///" + group.IconPath.Replace("\\", "/")));
            }
            catch { }
        }
        return new BitmapImage(new Uri("ms-appx:///Assets/default-group-icon.png"));
    }

    private MenuFlyout CreateMainContextMenu()
    {
        var menu = new MenuFlyout();
        var settingsItem = new MenuFlyoutItem { Text = "Settings" };
        settingsItem.Click += (s, e) => ShowSettings();
        menu.Items.Add(settingsItem);

        menu.Items.Add(new MenuFlyoutSeparator());

        var exitItem = new MenuFlyoutItem { Text = "Exit" };
        exitItem.Click += (s, e) => Exit();
        menu.Items.Add(exitItem);

        return menu;
    }

    private MenuFlyout CreateGroupContextMenu(AppGroup group)
    {
        var menu = new MenuFlyout();
        var settingsItem = new MenuFlyoutItem { Text = "Settings" };
        settingsItem.Click += (s, e) => ShowSettings();
        menu.Items.Add(settingsItem);
        return menu;
    }

    private void ShowSettings()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow(_storage);
            _settingsWindow.Closed += (s, e) =>
            {
                _settingsWindow = null;
                _storage.Load();
                EnsureTrayIconsForGroups();
            };
        }
        _settingsWindow.Activate();
    }

    private void Exit()
    {
        _mainTrayIcon?.Dispose();
        foreach (var (icon, _) in _groupTrayIcons.Values)
        {
            icon.Dispose();
        }
        _groupTrayIcons.Clear();
        _settingsWindow?.Close();
        Environment.Exit(0);
    }
}
