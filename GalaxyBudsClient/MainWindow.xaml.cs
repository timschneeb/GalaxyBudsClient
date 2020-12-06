using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Bluetooth.Linux;
using GalaxyBudsClient.Interface;
using GalaxyBudsClient.Interface.Developer;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Interface.Transition;
using GalaxyBudsClient.Message;
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
        
        private readonly CustomTitleBar _titleBar;
        
        public PageContainer Pager { get; }

        public static MainWindow Instance { get; }
        static MainWindow()
        {
            Instance = new MainWindow();
            Log.Debug("MainWindow instance allocated");
        }
        
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            Application.Current.Styles[1] = App.FluentLight;
            
            Pager = this.FindControl<PageContainer>("Container");
            Pager.RegisterPages(_homePage, new AmbientSoundPage(), new FindMyGearPage(), new FactoryResetPage(), new CreditsPage(),
                new TouchpadPage(), new EqualizerPage(), new AdvancedPage(), new SystemPage(), new SelfTestPage(), new SettingsPage(),
                new PopupSettingsPage(), new ConnectionLostPage(), new CustomTouchActionPage(), new DeviceSelectionPage(),
                new WelcomePage(), new UnsupportedFeaturePage(), new UpdatePage());
            Pager.SwitchPage(AbstractPage.Pages.Home);

            _titleBar = this.FindControl<CustomTitleBar>("TitleBar");
            _titleBar.PointerPressed += (i, e) => PlatformImpl?.BeginMoveDrag(e);
            _titleBar.OptionsPressed += (i, e) => _titleBar.OptionsButton.ContextMenu.Open();
            
            Loc.LanguageUpdated += OnLanguageUpdated;

            BuildOptionsMenu();

            BluetoothImpl.Instance.ConnectAsync("80:7B:3E:21:79:EC", Models.BudsPlus).ContinueWith(delegate(Task task)
            {
                BluetoothImpl.Instance.Connected += (sender, args) => BluetoothImpl.Instance.SendAsync(SPPMessage.MessageIds.MSG_ID_SET_AMBIENT_MODE, new byte[1]{0x01});
            });
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
                [Loc.Resolve("optionsmenu_refresh")] = (sender, args) => Pager.SwitchPage(AbstractPage.Pages.UpdateAvailable),
                [Loc.Resolve("optionsmenu_deregister")] = (sender, args) => Log.Debug("Deregister selected"),
                [Loc.Resolve("optionsmenu_update")] = (sender, args) => Log.Debug("Check for updates selected"),
                [Loc.Resolve("optionsmenu_credits")] = (sender, args) => Pager.SwitchPage(AbstractPage.Pages.Credits),
            });
        }

        public void ShowDevTools()
        {
            new DevTools().Show(this);
        }
    }
}