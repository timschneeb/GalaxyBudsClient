using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface;
using GalaxyBudsClient.Interface.Converters;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
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
        // Icons are rendered for the current system appearance
        if (Application.Current?.PlatformSettings is { } platformSettings)
            platformSettings.ColorValuesChanged += (_, _) => _ = RebuildAsync();
        
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
        if (type is NoiseControlModes mode)
        {
            EventDispatcher.Instance.Dispatch(Event.SetNoiseControlState, mode);
            await RebuildAsync();
            return;
        }
        
        if (type is not TrayItemTypes e)
        {
            Log.Error("TrayManager.OnTrayMenuCommand: Unknown item type: {Type}", type);
            return;
        }
            
        switch (e)
        {
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

    private static IEnumerable<NativeMenuItemBase?> RebuildBatteryInfo()
    {
        var bsu = DeviceMessageCache.Instance.BasicStatusUpdate!;
        var batteryCase = bsu.BatteryCase;
        if (batteryCase > 100)
        {
            batteryCase = DeviceMessageCache.Instance.BasicStatusUpdateWithValidCase?.BatteryCase ?? bsu.BatteryCase;
        }
            
        var deviceName = BluetoothImpl.Instance.Device.Current?.Name;
        return
        [
            PlatformUtils.IsOSX && !string.IsNullOrWhiteSpace(deviceName)
                ? new NativeMenuItem(deviceName) { IsEnabled = false, Icon = Icon(Symbol.Headphones) }
                : null,
            bsu.BatteryL > 0
                ? new NativeMenuItem($"{Strings.Left}: {bsu.BatteryL}%") { IsEnabled = false, Icon = BatteryIcon(bsu.BatteryL) }
                : null,
            bsu.BatteryR > 0
                ? new NativeMenuItem($"{Strings.Right}: {bsu.BatteryR}%") { IsEnabled = false, Icon = BatteryIcon(bsu.BatteryR) }
                : null,
            batteryCase is > 0 and <= 100 && BluetoothImpl.Instance.DeviceSpec.Supports(Features.CaseBattery)
                ? new NativeMenuItem($"{Strings.Case}: {batteryCase}%") { IsEnabled = false, Icon = BatteryIcon(batteryCase) }
                : null,

            new NativeMenuItemSeparator()

        ];
    }

    // Menu icons are only styled for the macOS menu bar
    private static Bitmap? Icon(Symbol symbol) => PlatformUtils.IsOSX ? TrayMenuIcons.Glyph(symbol) : null;

    private static readonly BatterySymbolConverter BatterySymbols = new();

    private static Bitmap? BatteryIcon(int level) =>
        Icon((Symbol)BatterySymbols.Convert(level, typeof(Symbol), null, CultureInfo.CurrentCulture));

    private IEnumerable<NativeMenuItemBase> BuildNoiseControlItems()
    {
        var current = MainView.Instance!.ResolveViewModelByType<NoiseControlPageViewModel>()?.NoiseControlMode;
        var items = new List<NativeMenuItemBase>
        {
            new NativeMenuItem(Strings.MainpageNoise) { IsEnabled = false }
        };
        foreach (var (mode, label, symbol) in new[]
                 {
                     (NoiseControlModes.Off, Strings.Off, Symbol.CircleOff),
                     (NoiseControlModes.AmbientSound, Strings.MainpageAmbientSound, Symbol.SoundWaveCircle),
                     (NoiseControlModes.NoiseReduction, Strings.Anc, Symbol.Headphones)
                 })
        {
            var selected = current == mode;
            items.Add(new NativeMenuItem(label)
            {
                // On macOS the filled circle icon marks the active mode instead of a radio bullet
                ToggleType = PlatformUtils.IsOSX ? NativeMenuItemToggleType.None : NativeMenuItemToggleType.Radio,
                IsChecked = selected,
                Icon = PlatformUtils.IsOSX ? TrayMenuIcons.Option(symbol, selected) : null,
                Command = new MiniCommand(OnTrayMenuCommand),
                CommandParameter = mode
            });
        }
        items.Add(new NativeMenuItemSeparator());
        return items;
    }

    private IEnumerable<NativeMenuItemBase> RebuildDynamicActions()
    {
        return BluetoothImpl.Instance.DeviceSpec.TrayShortcuts.SelectMany(type =>
            type == TrayItemTypes.ToggleNoiseControl ? BuildNoiseControlItems() : [BuildToggleItem(type)]);
    }

    private NativeMenuItem BuildToggleItem(TrayItemTypes type)
    {
        var isTouchpadLocked = MainView.Instance!.ResolveViewModelByType<TouchpadPageViewModel>()?.IsTouchpadLocked ?? false;
        var str = type switch
        {
            TrayItemTypes.ToggleEqualizer => MainView.Instance!.ResolveViewModelByType<EqualizerPageViewModel>()?.IsEqEnabled ?? false
                ? Strings.TrayDisableEq
                : Strings.TrayEnableEq, 
            TrayItemTypes.ToggleAmbient => MainView.Instance!.ResolveViewModelByType<NoiseControlPageViewModel>()?.IsAmbientSoundEnabled ?? false
                ? Strings.TrayDisableAmbientSound
                : Strings.TrayEnableAmbientSound,
            TrayItemTypes.ToggleAnc => MainView.Instance!.ResolveViewModelByType<NoiseControlPageViewModel>()?.IsAncEnabled ?? false
                ? Strings.TrayDisableAnc
                : Strings.TrayEnableAnc,
            TrayItemTypes.LockTouchpad => isTouchpadLocked
                ? Strings.TrayUnlockTouchpad
                : Strings.TrayLockTouchpad,
            _ => Strings.Unknown
        };
        var symbol = type switch
        {
            TrayItemTypes.ToggleEqualizer => Symbol.DeviceEq,
            TrayItemTypes.ToggleAmbient => Symbol.SoundWaveCircle,
            TrayItemTypes.ToggleAnc => Symbol.Headphones,
            TrayItemTypes.LockTouchpad => isTouchpadLocked ? Symbol.LockOpen : Symbol.LockClosed,
            _ => Symbol.QuestionCircle
        };
        return new NativeMenuItem(str)
        {
            Icon = Icon(symbol),
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
                    Icon = Icon(Symbol.Window),
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
                    Icon = Icon(Symbol.Bluetooth),
                    Command = new MiniCommand(OnTrayMenuCommand),
                    CommandParameter = TrayItemTypes.Connect
                });
                items.Add(new NativeMenuItemSeparator());
            }
                
            items.Add(new NativeMenuItem(Strings.TrayQuit)
            {
                Icon = Icon(Symbol.Power),
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