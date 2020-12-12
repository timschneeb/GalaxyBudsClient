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
using GalaxyBudsClient.Bluetooth.Linux;
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
using Serilog;

namespace GalaxyBudsClient
{
    public sealed class MainWindow : Window
    {
        private readonly HomePage _homePage = new HomePage();
        private readonly UnsupportedFeaturePage _unsupportedFeaturePage = new UnsupportedFeaturePage();
        private readonly CustomTouchActionPage _customTouchActionPage = new CustomTouchActionPage();
        
        private readonly CustomTitleBar _titleBar;
        private BudsPopup _popup;
        
        public PageContainer Pager { get; }
        public CustomTouchActionPage CustomTouchActionPage => _customTouchActionPage;

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
                new PopupSettingsPage(), new ConnectionLostPage(), _customTouchActionPage, new DeviceSelectionPage(),
                new WelcomePage(), _unsupportedFeaturePage, new UpdatePage());
            Pager.SwitchPage(AbstractPage.Pages.Home);

            _titleBar = this.FindControl<CustomTitleBar>("TitleBar");
            _titleBar.PointerPressed += (i, e) => PlatformImpl?.BeginMoveDrag(e);
            _titleBar.OptionsPressed += (i, e) => _titleBar.OptionsButton.ContextMenu.Open(_titleBar.OptionsButton);

            _popup = new BudsPopup();
            
            SPPMessageHandler.Instance.OtherOption += HandleOtherTouchOption;
            
            Loc.LanguageUpdated += OnLanguageUpdated;

            BuildOptionsMenu();

            /* TODO */
            var connectTask = BluetoothImpl.Instance.ConnectAsync("80:7B:3E:21:79:EC", Models.BudsPlus);
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

        private void OnLanguageUpdated()
        {
            BuildOptionsMenu();
        }

        private void BuildOptionsMenu()
        {
            _titleBar.OptionsButton.ContextMenu = 
                MenuFactory.BuildContextMenu(new Dictionary<string, EventHandler<RoutedEventArgs>?>()
                {
                    [Loc.Resolve("optionsmenu_settings")] = (sender, args) => Pager.SwitchPage(AbstractPage.Pages.Settings),
                    [Loc.Resolve("optionsmenu_refresh")] = async (sender, args) => await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_DEBUG_GET_ALL_DATA),
                    [Loc.Resolve("optionsmenu_deregister")] = (sender, args) => Log.Debug("Deregister selected"),
                    [Loc.Resolve("optionsmenu_update")] = (sender, args) => Log.Debug("Check for updates selected"),
                    [Loc.Resolve("optionsmenu_credits")] = (sender, args) => Pager.SwitchPage(AbstractPage.Pages.Credits),
                });
        }
        
        public void ShowPopup()
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
            catch (InvalidOperationException ex)
            {
                /* Window already closed down */
                _popup = new BudsPopup();
                _popup.Show();
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
    }
}