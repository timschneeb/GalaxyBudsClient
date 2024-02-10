using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Utils
{
    class TrayManager
    {
        private readonly Timer _timer = new();
        public TrayManager()
        {
            (Application.Current as App)!.TrayMenu.NeedsUpdate += async (sender, args) =>
            {
                await RebuildAsync();
            }; 
            (Application.Current as App)!.TrayMenu.Opening += async (sender, args) =>
            {
                await RebuildAsync();
            };
            BluetoothImpl.Instance.Connected += (sender, args) => _ = RebuildAsync();
            
            // TODO Avalonia bug workaround https://github.com/AvaloniaUI/Avalonia/issues/8076
            _timer.Interval = 1000;
            _timer.AutoReset = true;
            _timer.Elapsed += async (sender, args) => await RebuildAsync();
            _timer.Start();
        }

        private async void OnTrayMenuCommand(object? type)
        {
            if (type is not ItemType e)
            {
                Log.Error($"TrayManager.OnTrayMenuCommand: Unknown item type: {type}");
                return;
            }
            
            switch (e)
            {
                case ItemType.ToggleNoiseControl:
                    var noisePage = MainWindow.Instance.Pager.FindPage(AbstractPage.Pages.NoiseControlPro);
                    if (noisePage is NoiseProPage page)
                    {
                        if (page.AmbientEnabled)
                        {
                            /* Ambient is on, use ANC toggle */
                            EventDispatcher.Instance.Dispatch(EventDispatcher.Event.AncToggle);
                        }
                        else if (page.AncEnabled)
                        {
                            /* ANC is on, use ANC toggle to disable itself */
                            EventDispatcher.Instance.Dispatch(EventDispatcher.Event.AncToggle);
                        }
                        else
                        {
                            /* Nothing is on, use ambient toggle */
                            EventDispatcher.Instance.Dispatch(EventDispatcher.Event.AmbientToggle);
                        }
                    }
                    break;
                case ItemType.LockTouchpad:
                    EventDispatcher.Instance.Dispatch(EventDispatcher.Event.LockTouchpadToggle);
                    break;
                case ItemType.ToggleAnc:
                    EventDispatcher.Instance.Dispatch(EventDispatcher.Event.AncToggle);
                    break;
                case ItemType.ToggleEqualizer:
                    EventDispatcher.Instance.Dispatch(EventDispatcher.Event.EqualizerToggle);
                    break;
                case ItemType.ToggleAmbient:
                    EventDispatcher.Instance.Dispatch(EventDispatcher.Event.AmbientToggle);
                    break;
                case ItemType.Connect:
                    if (!BluetoothImpl.Instance.IsConnected && BluetoothImpl.Instance.RegisteredDeviceValid)
                    {
                        await BluetoothImpl.Instance.ConnectAsync();
                    }
                    break;
                case ItemType.Open:
                    Dispatcher.UIThread.Post(MainWindow.Instance.BringToFront);
                    break;
                case ItemType.Quit:
                    Log.Information("TrayManager: Exit requested by user");
                    MainWindow.Instance.OverrideMinimizeTray = true;
                    Dispatcher.UIThread.Post(MainWindow.Instance.Close);
                    break;
            }
            
            await RebuildAsync();
        }

        private List<NativeMenuItemBase?> RebuildBatteryInfo()
        {
            var bsu = DeviceMessageCache.Instance.BasicStatusUpdate!;
            if (bsu.BatteryCase > 100)
            {
                bsu.BatteryCase = DeviceMessageCache.Instance.BasicStatusUpdateWithValidCase?.BatteryCase ?? bsu.BatteryCase;
            }
            
            return new List<NativeMenuItemBase?>
            {
                bsu.BatteryL > 0 ? new NativeMenuItem($"{Loc.Resolve("left")}: {bsu.BatteryL}%"){IsEnabled = false} : null,
                bsu.BatteryR > 0 ? new NativeMenuItem($"{Loc.Resolve("right")}: {bsu.BatteryR}%"){IsEnabled = false} : null,
                (bsu.BatteryCase is > 0 and <= 100 && BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.CaseBattery)) ?
                    new NativeMenuItem($"{Loc.Resolve("case")}: {bsu.BatteryCase}%"){IsEnabled = false} : null,
                new NativeMenuItemSeparator(),
            };
        }

        private List<NativeMenuItemBase> RebuildDynamicActions()
        {
            var items = new List<NativeMenuItemBase>();

            foreach (var type in BluetoothImpl.Instance.DeviceSpec.TrayShortcuts)
            {
                switch (type)
                {
                    case ItemType.ToggleNoiseControl:
                        items.Add(new NativeMenuItem(Loc.Resolve("tray_switch_noise"))
                        {  
                            Command = new MiniCommand(OnTrayMenuCommand),
                            CommandParameter = type
                        });
                        break;
                    case ItemType.ToggleEqualizer:
                        var eqEnabled =
                            (MainWindow.Instance.Pager.FindPage(AbstractPage.Pages.Equalizer) as EqualizerPage)
                            ?.EqualizerEnabled ?? DeviceMessageCache.Instance.ExtendedStatusUpdate?.EqualizerEnabled ?? false;
                        items.Add(new NativeMenuItem(eqEnabled ? Loc.Resolve("tray_disable_eq") : Loc.Resolve("tray_enable_eq"))
                        {  
                            Command = new MiniCommand(OnTrayMenuCommand),
                            CommandParameter = type
                        });
                        break;
                    case ItemType.ToggleAmbient:
                        bool ambEnabled;
                        if (BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.NoiseControl))
                        {
                            ambEnabled = (MainWindow.Instance.Pager.FindPage(AbstractPage.Pages.NoiseControlPro) as NoiseProPage)
                                ?.AmbientEnabled ?? DeviceMessageCache.Instance.ExtendedStatusUpdate?.AmbientSoundEnabled ?? false;
                        }
                        else
                        {
                            ambEnabled = 
                                (MainWindow.Instance.Pager.FindPage(AbstractPage.Pages.AmbientSound) as AmbientSoundPage)
                                ?.AmbientEnabled ?? DeviceMessageCache.Instance.ExtendedStatusUpdate?.AmbientSoundEnabled ?? false;
                        }
                        items.Add(new NativeMenuItem(ambEnabled ? Loc.Resolve("tray_disable_ambient_sound") : Loc.Resolve("tray_enable_ambient_sound"))
                        {  
                            Command = new MiniCommand(OnTrayMenuCommand),
                            CommandParameter = type
                        });
                        break;
                    case ItemType.ToggleAnc:
                        bool ancEnabled;
                        if (BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.NoiseControl))
                        {
                            ancEnabled = (MainWindow.Instance.Pager.FindPage(AbstractPage.Pages.NoiseControlPro) as NoiseProPage)
                                ?.AncEnabled ?? DeviceMessageCache.Instance.ExtendedStatusUpdate?.NoiseCancelling ?? false;
                        }
                        else
                        {
                            ancEnabled =
                                (MainWindow.Instance.Pager.FindPage(AbstractPage.Pages.Home) as HomePage)
                                ?.AncEnabled ?? DeviceMessageCache.Instance.ExtendedStatusUpdate?.NoiseCancelling ?? false;
                        }
                        items.Add(new NativeMenuItem(ancEnabled ? Loc.Resolve("tray_disable_anc") : Loc.Resolve("tray_enable_anc"))
                        {  
                            Command = new MiniCommand(OnTrayMenuCommand),
                            CommandParameter = type
                        });
                        break;
                    case ItemType.LockTouchpad:
                        var lockEnabled =
                            (MainWindow.Instance.Pager.FindPage(AbstractPage.Pages.Touch) as TouchpadPage)
                            ?.TouchpadLocked ?? DeviceMessageCache.Instance.ExtendedStatusUpdate?.TouchpadLock ?? false;
                        items.Add(new NativeMenuItem(lockEnabled ? Loc.Resolve("tray_unlock_touchpad") : Loc.Resolve("tray_lock_touchpad"))
                        {  
                            Command = new MiniCommand(OnTrayMenuCommand),
                            CommandParameter = type
                        });
                        break;
                }
            }
            
            return items;
        }

        public async Task RebuildAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
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
                if (BluetoothImpl.Instance.IsConnected && DeviceMessageCache.Instance.BasicStatusUpdate != null)
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
        private static readonly object Padlock = new object();
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