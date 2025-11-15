using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using Serilog;
using MainWindow = GalaxyBudsClient.Interface.MainWindow;

namespace GalaxyBudsClient.Utils.Interface;

internal class TrayManager
{
    private bool _allowUpdate = true;
    private bool _missedUpdate;

    private TrayManager()
    {
        // Make sure nobody is updating the menu while it's open
        // because it crashes mac https://github.com/AvaloniaUI/Avalonia/issues/14578
        (Application.Current as App)!.TrayMenu.Opening += (_, _) => _allowUpdate = false;
        
        (Application.Current as App)!.TrayMenu.Closed += (_, _) =>
        {
            _allowUpdate = true;
            if (_missedUpdate)
            {
                _ = RebuildAsync();
            }
        };
        // It's important to trigger a rebuild every time some event happens
        // otherwise tray will show outdated infos
        (Application.Current as App)!.TrayMenu.NeedsUpdate += (sender, args) => _ = RebuildAsync();
        
        BluetoothImpl.Instance.Connected += (sender, args) => _ = RebuildAsync();
        BluetoothImpl.Instance.Disconnected += (sender, args) => _ = RebuildAsync();
        EventDispatcher.Instance.EventReceived += (ev, args) =>
        {
            if (ev == Event.UpdateTrayIcon) 
                _ = RebuildAsync();
        };
        // triggering rebuild when battery % changes
        SppMessageReceiver.Instance.StatusUpdate += (_, _) => _ = RebuildAsync();
        SppMessageReceiver.Instance.ExtendedStatusUpdate += (_, _) => _ = RebuildAsync();
        // triggering rebuild when noise control / ambient / anc state changes is handled in MessageComposer.cs
        // triggering rebuild when lock touchpad changes is handled in TouchpadPage.xaml.cs
        // triggering rebuild when eq state changes is handled in MessageComposer.cs
    }

    private async void OnTrayMenuCommand(object? type)
    {
        if (type is not TrayItemTypes e)
        {
            Log.Error("TrayManager.OnTrayMenuCommand: Unknown item type: {Type}", type);
            return;
        }
            
        switch (e)
        {
            case TrayItemTypes.ShowBatteryPopup:
                EventDispatcher.Instance.Dispatch(Event.ShowBatteryPopup);
                break;
            case TrayItemTypes.ToggleNoiseControl:
                var ncVm = MainView.Instance!.ResolveViewModelByType<NoiseControlPageViewModel>();
                if (ncVm != null)
                {
                    if (ncVm.IsAmbientSoundEnabled)
                    {
                        /* Ambient is on, use ANC toggle */
                        EventDispatcher.Instance.Dispatch(Event.AncToggle);
                    }
                    else if (ncVm.IsAncEnabled)
                    {
                        /* ANC is on, use ANC toggle to disable itself */
                        EventDispatcher.Instance.Dispatch(Event.AncToggle);
                    }
                    else
                    {
                        /* Nothing is on, use ambient toggle */
                        EventDispatcher.Instance.Dispatch(Event.AmbientToggle);
                    }
                }
                break;
            case TrayItemTypes.LockTouchpad:
                EventDispatcher.Instance.Dispatch(Event.LockTouchpadToggle);
                break;
            case TrayItemTypes.ToggleAnc:
                EventDispatcher.Instance.Dispatch(Event.AncToggle);
                break;
            case TrayItemTypes.ToggleEqualizer:
                EventDispatcher.Instance.Dispatch(Event.EqualizerToggle);
                break;
            case TrayItemTypes.ToggleAmbient:
                EventDispatcher.Instance.Dispatch(Event.AmbientToggle);
                break;
            case TrayItemTypes.Connect:
                if (!BluetoothImpl.Instance.IsConnected && BluetoothImpl.HasValidDevice)
                {
                    await BluetoothImpl.Instance.ConnectAsync();
                }
                break;
            case TrayItemTypes.Open:
                Dispatcher.UIThread.Post(MainWindow.Instance.BringToFront);
                break;
            case TrayItemTypes.Quit:
                Log.Information("TrayManager: Exit requested by user");
                if(Application.Current?.ApplicationLifetime is IControlledApplicationLifetime lifetime)
                    lifetime.Shutdown();
                else
                    Environment.Exit(0);
                break;
        }
            
        await RebuildAsync();
    }

    private IEnumerable<NativeMenuItemBase?> RebuildBatteryInfo()
    {
        var bsu = DeviceMessageCache.Instance.BasicStatusUpdate!;
        var batteryCase = bsu.BatteryCase;
        if (batteryCase > 100)
        {
            batteryCase = DeviceMessageCache.Instance.BasicStatusUpdateWithValidCase?.BatteryCase ?? bsu.BatteryCase;
        }
            
        return
        [
            bsu.BatteryL > 0
                ? new NativeMenuItem($"{Strings.Left}: {bsu.BatteryL}%") { IsEnabled = false }
                : null,
            bsu.BatteryR > 0
                ? new NativeMenuItem($"{Strings.Right}: {bsu.BatteryR}%") { IsEnabled = false }
                : null,
            batteryCase is > 0 and <= 100 && BluetoothImpl.Instance.DeviceSpec.Supports(Features.CaseBattery)
                ? new NativeMenuItem($"{Strings.Case}: {batteryCase}%") { IsEnabled = false }
                : null,

            new NativeMenuItem("Show Battery Details")
            {
                Command = new MiniCommand(OnTrayMenuCommand),
                CommandParameter = TrayItemTypes.ShowBatteryPopup
            },
            
            new NativeMenuItemSeparator()

        ];
    }

    private IEnumerable<NativeMenuItemBase> RebuildDynamicActions()
    {
        return from type in BluetoothImpl.Instance.DeviceSpec.TrayShortcuts
            let str = type switch
            {
                TrayItemTypes.ToggleNoiseControl => Strings.TraySwitchNoise,
                TrayItemTypes.ToggleEqualizer => MainView.Instance!.ResolveViewModelByType<EqualizerPageViewModel>()?.IsEqEnabled ?? false
                    ? Strings.TrayDisableEq
                    : Strings.TrayEnableEq, 
                TrayItemTypes.ToggleAmbient => MainView.Instance!.ResolveViewModelByType<NoiseControlPageViewModel>()?.IsAmbientSoundEnabled ?? false
                    ? Strings.TrayDisableAmbientSound
                    : Strings.TrayEnableAmbientSound,
                TrayItemTypes.ToggleAnc => MainView.Instance!.ResolveViewModelByType<NoiseControlPageViewModel>()?.IsAncEnabled ?? false
                    ? Strings.TrayDisableAnc
                    : Strings.TrayEnableAnc,
                TrayItemTypes.LockTouchpad => MainView.Instance!.ResolveViewModelByType<TouchpadPageViewModel>()?.IsTouchpadLocked ?? false
                    ? Strings.TrayUnlockTouchpad
                    : Strings.TrayLockTouchpad,
                _ => Strings.Unknown
            }
            select new NativeMenuItem(str)
            {
                Command = new MiniCommand(OnTrayMenuCommand),
                CommandParameter = type
            };
    }

    public async Task RebuildAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (!_allowUpdate)
            {
                _missedUpdate = true;
                return;
            }
            _missedUpdate = false;
            var items = new List<NativeMenuItemBase>();
            if (PlatformUtils.IsOSX || PlatformUtils.IsLinux)
            {
                items.Add(new NativeMenuItem(Strings.WindowOpen)
                {
                    Command = new MiniCommand(OnTrayMenuCommand),
                    CommandParameter = TrayItemTypes.Open
                });
                items.Add(new NativeMenuItemSeparator());
            }
            if (BluetoothImpl.Instance.IsConnected && DeviceMessageCache.Instance.BasicStatusUpdate != null)
            {
                items.AddRange(RebuildBatteryInfo().OfType<NativeMenuItemBase>());
                items.AddRange(RebuildDynamicActions());
                items.Add(new NativeMenuItemSeparator());
            }
            else if (BluetoothImpl.HasValidDevice)
            {
                items.Add(new NativeMenuItem(Strings.ConnlostConnect)
                {
                    Command = new MiniCommand(OnTrayMenuCommand),
                    CommandParameter = TrayItemTypes.Connect
                });
                items.Add(new NativeMenuItemSeparator());
            }
                
            items.Add(new NativeMenuItem(Strings.TrayQuit)
            {
                Command = new MiniCommand(OnTrayMenuCommand),
                CommandParameter = TrayItemTypes.Quit
            });
                
            (Application.Current as App)?.TrayMenu.Items.Clear();
            foreach (var item in items)
            {
                (Application.Current as App)?.TrayMenu.Items.Add(item);
            }
        }, DispatcherPriority.Normal);
    }

    #region Singleton
    private static readonly object Padlock = new();
    private static TrayManager? _instance;
    public static TrayManager Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new TrayManager();
            }
        }
    }

    public static void Init()
    {
        lock (Padlock)
        {
            _instance ??= new TrayManager();
        }
    }
    #endregion
}