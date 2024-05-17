using System.ComponentModel;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class NotificationReaderPageViewModel : SubPageViewModelBase
{
    public NotificationReaderPageViewModel()
    {
    }
    

    [Reactive] public bool IsEqEnabled { set; get; }
    [Reactive] public int EqPreset { set; get; }
    [Reactive] public int StereoBalance { set; get; }

    public override Control CreateView() => new NotificationReaderPage();
    public override string TitleKey => Keys.NotificationHeader;
}
