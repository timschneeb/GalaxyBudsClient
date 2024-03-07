using System.Reflection;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.ViewModels;

public class SettingsPageViewModel : ViewModelBase, IMainPageViewModel
{
    public override Control CreateView() => new Pages.SettingsPage();
    
    public bool IsAutoStartEnabled
    {
        get => AutoStartImpl.Instance.Enabled;
        set
        {
            AutoStartImpl.Instance.Enabled = value;
            RaisePropertyChanged(nameof(IsAutoStartEnabled));
        }
    }
    
    public string CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
    
    public string NavHeader => "Settings";
    public Symbol IconKey => Symbol.Settings;
    public bool ShowsInFooter => true;
}
