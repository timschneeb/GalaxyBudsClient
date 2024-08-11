using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Constants;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class HiddenModePageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new HiddenModePage { DataContext = this };
    public override string TitleKey => Keys.SystemHiddenAtMode;
    
    [Reactive] public Devices TargetDevice { set; get; }
    [Reactive] public bool IsUartEnabled { set; get; }
}


