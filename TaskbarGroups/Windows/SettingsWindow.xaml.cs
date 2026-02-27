using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TaskbarGroups.Models;
using TaskbarGroups.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace TaskbarGroups.Windows;

public sealed partial class SettingsWindow : Window
{
    private readonly GroupStorageService _storage;
    private AppGroup? _selectedGroup;

    public SettingsWindow(GroupStorageService storage)
    {
        InitializeComponent();
        _storage = storage;
        LoadGroups();
    }

    private void LoadGroups()
    {
        GroupsList.ItemsSource = null;
        GroupsList.ItemsSource = _storage.Groups.ToList();
    }

    private void AddGroupButton_Click(object sender, RoutedEventArgs e)
    {
        var group = new AppGroup { Name = "New Group" };
        _storage.AddGroup(group);
        LoadGroups();
        GroupsList.SelectedItem = group;
    }

    private void GroupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GroupsList.SelectedItem is not AppGroup group)
        {
            GroupDetailsPanel.Visibility = Visibility.Collapsed;
            return;
        }

        _selectedGroup = group;

        GroupDetailsPanel.Visibility = Visibility.Visible;
        GroupTitle.Text = _selectedGroup.Name;
        AppsList.ItemsSource = null;
        AppsList.ItemsSource = _selectedGroup.Apps;
    }

    private async void RenameGroupButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGroup == null) return;

        var dialog = new ContentDialog
        {
            Title = "Rename Group",
            Content = new TextBox
            {
                Text = _selectedGroup.Name,
                Width = 300
            },
            PrimaryButtonText = "OK",
            CloseButtonText = "Cancel",
            XamlRoot = Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Content is TextBox tb && !string.IsNullOrWhiteSpace(tb.Text))
        {
            _selectedGroup.Name = tb.Text.Trim();
            _storage.UpdateGroup(_selectedGroup);
            LoadGroups();
            GroupTitle.Text = _selectedGroup.Name;
        }
    }

    private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGroup == null) return;

        _storage.RemoveGroup(_selectedGroup);
        _selectedGroup = null;
        LoadGroups();
        GroupDetailsPanel.Visibility = Visibility.Collapsed;
    }

    private async void AddAppButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGroup == null) return;

        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add(".exe");
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            _selectedGroup.Apps.Add(new AppEntry
            {
                Name = Path.GetFileNameWithoutExtension(file.Path),
                Path = file.Path
            });
            _storage.UpdateGroup(_selectedGroup);
            AppsList.ItemsSource = null;
            AppsList.ItemsSource = _selectedGroup.Apps;
        }
    }

    private void RemoveAppButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGroup == null || sender is not Button btn) return;
        if (btn.DataContext is AppEntry app)
        {
            _selectedGroup.Apps.Remove(app);
            _storage.UpdateGroup(_selectedGroup);
            AppsList.ItemsSource = null;
            AppsList.ItemsSource = _selectedGroup.Apps;
        }
    }

    private void AppsList_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void AppsList_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
        if (_selectedGroup == null) return;

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            foreach (var item in items)
            {
                if (item is StorageFile file && file.Path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    _selectedGroup.Apps.Add(new AppEntry
                    {
                        Name = Path.GetFileNameWithoutExtension(file.Path),
                        Path = file.Path
                    });
                }
            }
            _storage.UpdateGroup(_selectedGroup);
            AppsList.ItemsSource = null;
            AppsList.ItemsSource = _selectedGroup.Apps;
        }
    }
}
