using System.Reflection;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Platform;
using ReactiveUI;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class SettingsPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new Interface.Pages.SettingsPage();
    
    public bool IsAutoStartEnabled
    {
        get => AutoStartImpl.Instance.Enabled;
        set
        {
            AutoStartImpl.Instance.Enabled = value;
            this.RaisePropertyChanged();
        }
    }
    
    public string CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
    
    public override string TitleKey => "settings_header";
    public override Symbol IconKey => Symbol.Settings;
    public override bool ShowsInFooter => true;
}
