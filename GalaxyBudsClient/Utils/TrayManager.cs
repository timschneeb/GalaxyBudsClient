using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Interop.TrayIcon;
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
        public TrayManager()
        {
            NotifyIconImpl.Instance.TrayMenuItemSelected += InstanceOnTrayMenuItemSelected;
            NotifyIconImpl.Instance.RightClicked += (sender, args) => Rebuild();
            BluetoothImpl.Instance.Connected += (sender, args) => Rebuild();
        }

        private async void InstanceOnTrayMenuItemSelected(object? sender, TrayMenuItem e)
        {
            switch (e.Id)
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
                case ItemType.Quit:
                    Log.Information("TrayManager: Exit requested by user");
                    MainWindow.Instance.OverrideMinimizeTray = true;
                    Dispatcher.UIThread.Post(MainWindow.Instance.Close);
                    break;
            }
            
            Rebuild();
        }

        private List<TrayMenuItem?> RebuildBatteryInfo()
        {
            var bsu = DeviceMessageCache.Instance.BasicStatusUpdate!;

            return new List<TrayMenuItem?>
            {
                bsu.BatteryL > 0 ? new TrayMenuItem($"{Loc.Resolve("left")}: {bsu.BatteryL}%", false) : null,
                bsu.BatteryR > 0 ? new TrayMenuItem($"{Loc.Resolve("right")}: {bsu.BatteryR}%", false) : null,
                (bsu.BatteryCase > 0 && BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.CaseBattery)) ?
                    new TrayMenuItem($"{Loc.Resolve("case")}: {bsu.BatteryCase}%", false) : null,
                new TrayMenuSeparator(),
            };
        }

        private List<TrayMenuItem> RebuildDynamicActions()
        {
            var items = new List<TrayMenuItem>();

            foreach (var type in BluetoothImpl.Instance.DeviceSpec.TrayShortcuts)
            {
                switch (type)
                {
                    case ItemType.ToggleNoiseControl:
                        items.Add(new TrayMenuItem(Loc.Resolve("tray_switch_noise"), type));
                        break;
                    case ItemType.ToggleEqualizer:
                        bool eqEnabled =
                            (MainWindow.Instance.Pager.FindPage(AbstractPage.Pages.Equalizer) as EqualizerPage)
                            ?.EqualizerEnabled ?? DeviceMessageCache.Instance.ExtendedStatusUpdate?.EqualizerEnabled ?? false;
                        items.Add(new TrayMenuItem(eqEnabled ? Loc.Resolve("tray_disable_eq") : Loc.Resolve("tray_enable_eq"), type));
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
                        items.Add(new TrayMenuItem(ambEnabled ? Loc.Resolve("tray_disable_ambient_sound") : Loc.Resolve("tray_enable_ambient_sound"), type));
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
                        items.Add(new TrayMenuItem(ancEnabled ? Loc.Resolve("tray_disable_anc") : Loc.Resolve("tray_enable_anc"), type));
                        break;
                    case ItemType.LockTouchpad:
                        bool lockEnabled =
                            (MainWindow.Instance.Pager.FindPage(AbstractPage.Pages.Touch) as TouchpadPage)
                            ?.TouchpadLocked ?? DeviceMessageCache.Instance.ExtendedStatusUpdate?.TouchpadLock ?? false;
                        items.Add(new TrayMenuItem(lockEnabled ? Loc.Resolve("tray_unlock_touchpad") : Loc.Resolve("tray_lock_touchpad"), type));
                        break;
                }
            }
            
            return items;
        }

        public void Rebuild()
        {
            Dispatcher.UIThread.Post(() =>
            {
                List<TrayMenuItem> items = new List<TrayMenuItem>();
                if (BluetoothImpl.Instance.IsConnected && DeviceMessageCache.Instance.BasicStatusUpdate != null)
                {
                    items.AddRange(RebuildBatteryInfo().OfType<TrayMenuItem>());
                    items.AddRange(RebuildDynamicActions());
                }
                else if (BluetoothImpl.Instance.RegisteredDeviceValid)
                {
                    items.Add(new TrayMenuItem(Loc.Resolve("connlost_connect"), ItemType.Connect));
                    items.Add(new TrayMenuSeparator());
                }

                items.Add(new TrayMenuSeparator());
                items.Add(new TrayMenuItem(Loc.Resolve("tray_quit"), ItemType.Quit));

                NotifyIconImpl.Instance.MenuItems = items;

            });
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