using System.ComponentModel;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.TeamsIntegration;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class AdvancedPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new AdvancedPage { DataContext = this };

    public AdvancedPageViewModel()
    {
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        PropertyChanged += OnPropertyChanged;
        
        // Inicializar configuração do Teams
        IsTeamsIntegrationEnabled = Settings.Data.TeamsIntegrationEnabled;
    }

    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateDecoder e)
    {
        using var suppressor = SuppressChangeNotifications();
        IsSidetoneEnabled = e.SideToneEnabled;
        IsPassthroughEnabled = e.RelieveAmbient;
        IsSeamlessConnectionEnabled = e.SeamlessConnectionEnabled;
        IsCallpathControlEnabled = e.CallPathControl;
        IsExtraClearCallEnabled = e.ExtraClearCallSound;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsSeamlessConnectionEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_SEAMLESS_CONNECTION, !IsSeamlessConnectionEnabled);
                break;
            case nameof(IsPassthroughEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.PASS_THROUGH, IsPassthroughEnabled);
                break;
            case nameof(IsSidetoneEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_SIDETONE, IsSidetoneEnabled);
                break; 
            case nameof(IsCallpathControlEnabled):
                // Boolean value has to be inverted
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_CALL_PATH_CONTROL, !IsCallpathControlEnabled);
                break;
            case nameof(IsExtraClearCallEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.EXTRA_CLEAR_SOUND_CALL, IsExtraClearCallEnabled);
                break;
            case nameof(IsTeamsIntegrationEnabled):
                Settings.Data.TeamsIntegrationEnabled = IsTeamsIntegrationEnabled;
                // Gerenciar o monitor do Teams baseado na configuração
                if (IsTeamsIntegrationEnabled)
                {
                    TeamsCallMonitor.Instance?.Start();
                    Log.Information("Teams integration enabled - monitor started");
                }
                else
                {
                    TeamsCallMonitor.Instance?.Stop();
                    Log.Information("Teams integration disabled - monitor stopped");
                }
                break;
        }
    }

    [Reactive] public bool IsSeamlessConnectionEnabled { set; get; }
    [Reactive] public bool IsPassthroughEnabled { set; get; }
    [Reactive] public bool IsSidetoneEnabled { set; get; }
    [Reactive] public bool IsCallpathControlEnabled { set; get; }
    [Reactive] public bool IsExtraClearCallEnabled { set; get; }
    [Reactive] public bool IsTeamsIntegrationEnabled { set; get; }

    public override string TitleKey => Keys.MainpageAdvanced;
    public override Symbol IconKey => Symbol.WrenchScrewdriver;
    public override bool ShowsInFooter => false;
}


