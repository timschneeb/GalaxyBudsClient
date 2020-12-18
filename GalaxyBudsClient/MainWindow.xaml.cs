using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Bluetooth.Linux;
using GalaxyBudsClient.Decoder;
using GalaxyBudsClient.Interface;
using GalaxyBudsClient.Interface.Developer;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Interface.Transition;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using JetBrains.Annotations;
using NetSparkleUpdater.Enums;
using Serilog;

namespace GalaxyBudsClient
{
    public sealed class MainWindow : Window
    {
        private readonly HomePage _homePage = new HomePage();
        private readonly UnsupportedFeaturePage _unsupportedFeaturePage = new UnsupportedFeaturePage();
        private readonly CustomTouchActionPage _customTouchActionPage = new CustomTouchActionPage();
        private readonly ConnectionLostPage _connectionLostPage = new ConnectionLostPage();
        private readonly UpdatePage _updatePage = new UpdatePage();
        private readonly UpdateProgressPage _updateProgressPage = new UpdateProgressPage();

        private readonly CustomTitleBar _titleBar;
        private BudsPopup _popup;
        private DateTime _lastPopupTime = DateTime.UtcNow;
        private WearStates _lastWearState = WearStates.Both;

        public PageContainer Pager { get; }
        public CustomTouchActionPage CustomTouchActionPage => _customTouchActionPage;
        public UpdatePage UpdatePage => _updatePage;
        public UpdateProgressPage UpdateProgressPage => _updateProgressPage;

        private static MainWindow? _instance;
        public static MainWindow Instance => _instance ??= new MainWindow();

        public static void Kill()
        {
            _instance = null;
        }
        
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();
            
            Pager = this.FindControl<PageContainer>("Container");
            
            Pager.RegisterPages(_homePage, new AmbientSoundPage(), new FindMyGearPage(), new FactoryResetPage(), new CreditsPage(),
                new TouchpadPage(), new EqualizerPage(), new AdvancedPage(), new SystemPage(), new SelfTestPage(), new SettingsPage(),
                new PopupSettingsPage(), _connectionLostPage, _customTouchActionPage, new DeviceSelectionPage(),
                new WelcomePage(), _unsupportedFeaturePage, _updatePage, _updateProgressPage);

            _titleBar = this.FindControl<CustomTitleBar>("TitleBar");
            _titleBar.PointerPressed += (i, e) => PlatformImpl?.BeginMoveDrag(e);
            _titleBar.OptionsPressed += (i, e) => _titleBar.OptionsButton.ContextMenu.Open(_titleBar.OptionsButton);

            _popup = new BudsPopup();
            
            BluetoothImpl.Instance.BluetoothError += OnBluetoothError;
            BluetoothImpl.Instance.Disconnected += OnDisconnected;
            BluetoothImpl.Instance.Connected += OnConnected;
            
            SPPMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
            SPPMessageHandler.Instance.StatusUpdate += OnStatusUpdate;
            SPPMessageHandler.Instance.OtherOption += HandleOtherTouchOption;

            Pager.PageSwitched += (sender, pages) => BuildOptionsMenu();
            Loc.LanguageUpdated += BuildOptionsMenu;
            BuildOptionsMenu();

            if (BluetoothImpl.Instance.RegisteredDeviceValid)
            {
                Task.Factory.StartNew(() => BluetoothImpl.Instance.ConnectAsync());
                Pager.SwitchPage(AbstractPage.Pages.Home);
            }
            else
            {
                Pager.SwitchPage(AbstractPage.Pages.Welcome);
            }
        }

        private void OnStatusUpdate(object? sender, StatusUpdateParser e)
        {
            if (_lastWearState == WearStates.None &&
                e.WearState != WearStates.None && SettingsProvider.Instance.ResumePlaybackOnSensor)
            {
                MediaKeyRemoteImpl.Instance.Play();
            }

            _lastWearState = e.WearState;
        }

        private async void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
        {
            ShowPopup();
            await MessageComposer.SetManagerInfo();
        }

        private void OnConnected(object? sender, EventArgs e)
        {
            Pager.SwitchPage(AbstractPage.Pages.Home);
        }

        private void OnBluetoothError(object? sender, BluetoothException e)
        {
            switch (e.ErrorCode)
            {
                case BluetoothException.ErrorCodes.NoAdaptersAvailable:
                    new MessageBox()
                    {
                        Title = Loc.Resolve("error"),
                        Description = Loc.Resolve("nobluetoothdev")
                    }.ShowDialog(this);
                    break;
                default:
                    Pager.SwitchPage(BluetoothImpl.Instance.RegisteredDeviceValid
                        ? AbstractPage.Pages.NoConnection
                        : AbstractPage.Pages.Welcome);
                    break;
            }
        }

        private void OnDisconnected(object? sender, string e)
        {
            Pager.SwitchPage(BluetoothImpl.Instance.RegisteredDeviceValid
                ? AbstractPage.Pages.NoConnection
                : AbstractPage.Pages.Welcome);
        }

        private void HandleOtherTouchOption(object? sender, TouchOptions e)
        {
            ICustomAction action = e == TouchOptions.OtherL ? 
                SettingsProvider.Instance.CustomActionLeft : SettingsProvider.Instance.CustomActionRight;

            if (action.Action == CustomAction.Actions.RunExternalProgram)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = action.Parameter,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                catch (FileNotFoundException ex)
                {
                    new MessageBox()
                    {
                        Title = "Custom long-press action failed",
                        Description = $"Unable to launch external application.\n" +
                                      $"File not found: '{ex.FileName}'"
                    }.Show(this);
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode == 13 && PlatformUtils.IsLinux)
                    {
                        new MessageBox()
                        {
                            Title = "Custom long-press action failed",
                            Description = $"Unable to launch external application.\n\n" +
                                          $"Insufficient permissions. Please add execute permissions for your user/group to this file.\n\n" +
                                          $"Run this command in a terminal: chmod +x \"{action.Parameter}\""
                        }.Show(this);
                    }
                    else
                    {
                        new MessageBox()
                        {
                            Title = "Custom long-press action failed",
                            Description = $"Unable to launch external application.\n\n" +
                                          $"Detailed information:\n\n" +
                                          $"{ex.Message}"
                        }.Show(this);
                    }
                }
            }
        }
        
        private void BuildOptionsMenu()
        {
            bool restricted = Pager.CurrentPage == AbstractPage.Pages.Welcome ||
                              Pager.CurrentPage == AbstractPage.Pages.DeviceSelect ||
                              !BluetoothImpl.Instance.RegisteredDeviceValid;
            
            var options = new Dictionary<string, EventHandler<RoutedEventArgs>?>()
            {
                [Loc.Resolve("optionsmenu_settings")] =
                    (sender, args) => Pager.SwitchPage(AbstractPage.Pages.Settings),
                [Loc.Resolve("optionsmenu_refresh")] = async (sender, args) =>
                await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_DEBUG_GET_ALL_DATA),
                [Loc.Resolve("optionsmenu_deregister")] = (sender, args) => BluetoothImpl.Instance.UnregisterDevice()
                    .ContinueWith((_) => Pager.SwitchPage(AbstractPage.Pages.Welcome))
            };
                
            if (restricted)
            {
                options.Clear();
            }

            options[Loc.Resolve("optionsmenu_update")] = async (sender, args) =>
            {
                var result  = await UpdateManager.Instance.DoManualCheck();
                if (result != UpdateStatus.UpdateAvailable)
                {
                    await new MessageBox()
                    {
                        Title = Loc.Resolve("updater_noupdate_title"),
                        Description = Loc.Resolve("updater_noupdate"),
                    }.ShowDialog(this);
                }
            };
            options[Loc.Resolve("optionsmenu_credits")] = (sender, args) => Pager.SwitchPage(AbstractPage.Pages.Credits);

            
            _titleBar.OptionsButton.ContextMenu = MenuFactory.BuildContextMenu(options);
        }


        public void ShowPopup(bool ignoreRestrictions = false)
        {
            DateTime now = DateTime.UtcNow;
            if ((now.Subtract(_lastPopupTime).TotalSeconds >= 5 && !IsActive) || ignoreRestrictions)
            {
                if (_popup.IsVisible)
                {
                    _popup.UpdateSettings();
                    _popup.RearmTimer();
                }

                try
                {
                    _popup.Show();
                }
                catch (InvalidOperationException)
                {
                    /* Window already closed down */
                    _popup = new BudsPopup();
                    _popup.Show();
                }
                _lastPopupTime = now; 
            }
       
        }

        public void ShowDevTools()
        {
            new DevTools().Show(this);
        }

        public void ShowUnsupportedFeaturePage(string assertion)
        {
            _unsupportedFeaturePage.RequiredVersion = assertion;
            Pager.SwitchPage(AbstractPage.Pages.UnsupportedFeature);
        }

        public void ShowCustomActionSelection(Devices device)
        {
            _customTouchActionPage.CurrentSide = device;
            Pager.SwitchPage(AbstractPage.Pages.TouchCustomAction);
        }

        private async void OnOpened(object? sender, EventArgs e)
        {
            if (BluetoothImpl.Instance.RegisteredDeviceValid)
            {
                await Task.Delay(3000).ContinueWith((_) => UpdateManager.Instance.SilentCheck());
            }
        }
    }
}