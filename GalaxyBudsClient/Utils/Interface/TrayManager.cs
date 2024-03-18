using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.ViewModels;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.InterfaceOld.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Utils.Interface
{
    class TrayManager
    {
        private bool _allowUpdate = true;
        private bool _missedUpdate = false;

        private TrayManager()
        {
            // Make sure nobody is updating the menu while it's open
            // because it crashes mac https://github.com/AvaloniaUI/Avalonia/issues/14578
            (Application.Current as App)!.TrayMenu.Opening += (_, _) =>
            {
                _allowUpdate = false;
            };
            (Application.Current as App)!.TrayMenu.Closed += (_, _) =>
            {
                _allowUpdate = true;
                if (_missedUpdate)
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        await RebuildAsync();
                    });
                }
            };
            // It's important to trigger a rebuild every time some event happens
            // otherwise tray will show outdated infos
            (Application.Current as App)!.TrayMenu.NeedsUpdate += (_, _) =>
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await RebuildAsync();
                });
            };
            BluetoothImpl.Instance.Connected += (sender, args) => _ = RebuildAsync();
            BluetoothImpl.Instance.Disconnected += (sender, args) => _ = RebuildAsync();
            EventDispatcher.Instance.EventReceived += (ev, _) =>
            {
                if (ev == Event.UpdateTrayIcon)
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        await RebuildAsync();
                    });
                }
            };
            // triggering rebuild when battery % changes
            SppMessageHandler.Instance.StatusUpdate += (_, _) => _ = RebuildAsync();
            SppMessageHandler.Instance.ExtendedStatusUpdate += (_, _) => _ = RebuildAsync();
            // triggering rebuild when noise control / ambient / anc state changes is handled in MessageComposer.cs
            // triggering rebuild when lock touchpad changes is handled in TouchpadPage.xaml.cs
            // triggering rebuild when eq state changes is handled in MessageComposer.cs
        }

        private async void OnTrayMenuCommand(object? type)
        {
            if (type is not ItemType e)
            {
                Log.Error("TrayManager.OnTrayMenuCommand: Unknown item type: {Type}", type);
                return;
            }
            
            switch (e)
            {
                case ItemType.ToggleNoiseControl:
                    var ncVm = MainWindow2.Instance.MainView.ResolveViewModelByType<NoiseControlPageViewModel>();
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
                case ItemType.LockTouchpad:
                    EventDispatcher.Instance.Dispatch(Event.LockTouchpadToggle);
                    break;
                case ItemType.ToggleAnc:
                    EventDispatcher.Instance.Dispatch(Event.AncToggle);
                    break;
                case ItemType.ToggleEqualizer:
                    EventDispatcher.Instance.Dispatch(Event.EqualizerToggle);
                    break;
                case ItemType.ToggleAmbient:
                    EventDispatcher.Instance.Dispatch(Event.AmbientToggle);
                    break;
                case ItemType.Connect:
                    if (!BluetoothImpl.Instance.IsConnectedLegacy && BluetoothImpl.Instance.RegisteredDeviceValid)
                    {
                        await BluetoothImpl.Instance.ConnectAsync();
                    }
                    break;
                case ItemType.Open:
                    Dispatcher.UIThread.Post(MainWindow2.Instance.BringToFront);
                    break;
                case ItemType.Quit:
                    Log.Information("TrayManager: Exit requested by user");
                    MainWindow2.Instance.OverrideMinimizeTray = true;
                    Dispatcher.UIThread.Post(MainWindow2.Instance.Close);
                    break;
            }
            
            await RebuildAsync();
        }

        private static List<NativeMenuItemBase?> RebuildBatteryInfo()
        {
            var bsu = DeviceMessageCache.Instance.BasicStatusUpdate!;
            if (bsu.BatteryCase > 100)
            {
                bsu.BatteryCase = DeviceMessageCache.Instance.BasicStatusUpdateWithValidCase?.BatteryCase ?? bsu.BatteryCase;
            }
            
            return
            [
                bsu.BatteryL > 0
                    ? new NativeMenuItem($"{Loc.Resolve("left")}: {bsu.BatteryL}%") { IsEnabled = false }
                    : null,
                bsu.BatteryR > 0
                    ? new NativeMenuItem($"{Loc.Resolve("right")}: {bsu.BatteryR}%") { IsEnabled = false }
                    : null,
                (bsu.BatteryCase is > 0 and <= 100 && BluetoothImpl.Instance.DeviceSpec.Supports(Features.CaseBattery))
                    ? new NativeMenuItem($"{Loc.Resolve("case")}: {bsu.BatteryCase}%") { IsEnabled = false }
                    : null,

                new NativeMenuItemSeparator()

            ];
        }

        private IEnumerable<NativeMenuItemBase> RebuildDynamicActions()
        {
            return from type in BluetoothImpl.Instance.DeviceSpec.TrayShortcuts
                let resourceKey = type switch
                {
                    ItemType.ToggleNoiseControl => "tray_switch_noise",
                    ItemType.ToggleEqualizer => MainWindow2.Instance.MainView.ResolveViewModelByType<EqualizerPageViewModel>()?.IsEqEnabled ?? false
                        ? "tray_disable_eq"
                        : "tray_enable_eq",
                    ItemType.ToggleAmbient => MainWindow2.Instance.MainView.ResolveViewModelByType<NoiseControlPageViewModel>()?.IsAmbientSoundEnabled ?? false
                        ? "tray_disable_ambient_sound"
                        : "tray_enable_ambient_sound",
                    ItemType.ToggleAnc => MainWindow2.Instance.MainView.ResolveViewModelByType<NoiseControlPageViewModel>()?.IsAncEnabled ?? false
                        ? "tray_disable_anc"
                        : "tray_enable_anc",
                    ItemType.LockTouchpad => /* TODO MainWindow2.Instance.MainView.ResolveViewModelByType<TouchpadPageViewModel>()?.IsTouchLocked ?? */ false
                        ? "tray_unlock_touchpad"
                        : "tray_lock_touchpad",
                    _ => "unknown"
                }
                select new NativeMenuItem(resourceKey)
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
                    items.Add(new NativeMenuItem(Loc.Resolve("window_open"))
                    {
                        Command = new MiniCommand(OnTrayMenuCommand),
                        CommandParameter = ItemType.Open
                    });
                    items.Add(new NativeMenuItemSeparator());
                }
                if (BluetoothImpl.Instance.IsConnectedLegacy && DeviceMessageCache.Instance.BasicStatusUpdate != null)
                {
                    items.AddRange(RebuildBatteryInfo().OfType<NativeMenuItemBase>());
                    items.AddRange(RebuildDynamicActions());
                    items.Add(new NativeMenuItemSeparator());
                }
                else if (BluetoothImpl.Instance.RegisteredDeviceValid)
                {
                    items.Add(new NativeMenuItem(Loc.Resolve("connlost_connect"))
                    {
                        Command = new MiniCommand(OnTrayMenuCommand),
                        CommandParameter = ItemType.Connect
                    });
                    items.Add(new NativeMenuItemSeparator());
                }
                
                items.Add(new NativeMenuItem(Loc.Resolve("tray_quit"))
                {
                    Command = new MiniCommand(OnTrayMenuCommand),
                    CommandParameter = ItemType.Quit
                });
                
                (Application.Current as App)?.TrayMenu.Items.Clear();
                foreach (var item in items)
                {
                    (Application.Current as App)?.TrayMenu.Items.Add(item);
                }
            }, DispatcherPriority.MaxValue);
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
}