using System.Reflection;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.ViewModels;

public class SettingsPageViewModel : MainPageViewModelBase
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
    
    public override string TitleKey => "Settings";
    public override Symbol IconKey => Symbol.Settings;
    public override bool ShowsInFooter => true;
}
