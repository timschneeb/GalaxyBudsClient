using System.Reflection;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Platform;
using ReactiveUI;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class SettingsPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new SettingsPage();
    
    public bool IsAutoStartEnabled
    {
        get => AutoStartHelper.Instance.Enabled;
        set
        {
            AutoStartHelper.Instance.Enabled = value;
            this.RaisePropertyChanged();
        }
    }
    
    // TODO add device unregister
    
    public string CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
    
    public override string TitleKey => "settings_header";
    public override Symbol IconKey => Symbol.Settings;
    public override bool ShowsInFooter => true;
}
