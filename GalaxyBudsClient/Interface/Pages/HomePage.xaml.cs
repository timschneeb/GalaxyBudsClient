using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Interop.TrayIcon;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using SettingsProvider = GalaxyBudsClient.Utils.SettingsProvider;
using ToggleSwitch = GalaxyBudsClient.Interface.Elements.ToggleSwitch;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class HomePage : AbstractPage
	{
		public override Pages PageType => Pages.Home;

        public bool AncEnabled => _ancSwitch.IsChecked;
        
        private readonly ToggleSwitch _ancSwitch;
        private readonly LoadingSpinner _loadingSpinner;
        
        private readonly Label _caseLabel;
        private readonly Label _batteryCase;
        
        private readonly Image _batteryIconLeft;
        private readonly Image _batteryIconRight;
        private readonly Image _iconLeft;
        private readonly Image _iconRight;
        
        private readonly Label _batteryVoltLeft;
        private readonly Label _batteryVoltRight;
        private readonly Label _batteryPercentageLeft;
        private readonly Label _batteryPercentageRight;
        private readonly Label _batteryCurrentLeft;
        private readonly Label _batteryCurrentRight;
        private readonly Label _batteryTemperatureLeft;
        private readonly Label _batteryTemperatureRight;
        
        private readonly Label _warningText;
        private readonly Grid _warningContainer;
   
        private readonly Border _ancBorder;
        private readonly Border _ambientBorder;
        private readonly Border _noiseBorder;
        
        private readonly IconListItem _findMyGear;
        private readonly IconListItem _touch;
        
		private readonly DispatcherTimer _refreshTimer = new DispatcherTimer();
        private DebugGetAllDataParser? _lastGetAllDataParser;
        private bool _lastLeftOnline;
        private bool _lastRightOnline;

		private PlacementStates _lastPlacementL = PlacementStates.Disconnected;
		private PlacementStates _lastPlacementR = PlacementStates.Disconnected;
		
		public HomePage()
		{   
            AvaloniaXamlLoader.Load(this);
            _ancSwitch = this.FindControl<ToggleSwitch>("AncToggle");
            _loadingSpinner = this.FindControl<LoadingSpinner>("LoadingSpinner");
            
            _caseLabel = this.FindControl<Label>("CaseLabel");
            _batteryCase = this.FindControl<Label>("BatteryCase");
            _batteryVoltLeft = this.FindControl<Label>("BatteryVoltLeft");
            _batteryVoltRight = this.FindControl<Label>("BatteryVoltRight");
            _batteryPercentageLeft = this.FindControl<Label>("BatteryPercentageLeft");
            _batteryPercentageRight = this.FindControl<Label>("BatteryPercentageRight");
            _batteryCurrentLeft = this.FindControl<Label>("BatteryCurrentLeft");
            _batteryCurrentRight = this.FindControl<Label>("BatteryCurrentRight");
            _batteryTemperatureLeft = this.FindControl<Label>("BatteryTemperatureLeft");
            _batteryTemperatureRight = this.FindControl<Label>("BatteryTemperatureRight");

            _batteryIconLeft = this.FindControl<Image>("BatteryIconLeft");
            _batteryIconRight = this.FindControl<Image>("BatteryIconRight");
            _iconLeft = this.FindControl<Image>("LeftIcon");
            _iconRight = this.FindControl<Image>("RightIcon");

            _warningText = this.FindControl<Label>("WarningText");
            _warningContainer = this.FindControl<Grid>("WarningContainer");
            
            _ancBorder = this.FindControl<Border>("AncBorder");
            _ambientBorder = this.FindControl<Border>("AmbientBorder");
            _noiseBorder = this.FindControl<Border>("NoiseBorder");
            
            _findMyGear = this.FindControl<IconListItem>("FindMyGear");
            _touch = this.FindControl<IconListItem>("Touch");
            
            Loc.LanguageUpdated += OnLanguageUpdated;
            
            _refreshTimer.Interval = new TimeSpan(0, 0, 7);
			_refreshTimer.Tick += async (sender, args) =>
				await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.DEBUG_GET_ALL_DATA);
            
            SPPMessageHandler.Instance.BaseUpdate += (sender, update) => ProcessBasicUpdate(update);
            SPPMessageHandler.Instance.GetAllDataResponse += (sender, parser) => UpdateDetails(parser);
            SPPMessageHandler.Instance.AncEnabledUpdateResponse += (sender, b) => _ancSwitch.IsChecked = b;
            SPPMessageHandler.Instance.AnyMessageReceived += (sender, parser) =>
            {
                SetWarning(false);
                _loadingSpinner.IsVisible = false;
            };
            BluetoothImpl.Instance.BluetoothError += (sender, exception) =>
            {
                _lastLeftOnline = false;
                _lastRightOnline = false;
                _lastGetAllDataParser = null;
            }; 
            BluetoothImpl.Instance.Disconnected += (sender, reason) =>
            { 
                _lastLeftOnline = false;
                _lastRightOnline = false;
                _lastGetAllDataParser = null;
            }; 
            BluetoothImpl.Instance.Connected += (sender, args) =>
            {
                Dispatcher.UIThread.Post((() => _loadingSpinner.IsVisible = false), DispatcherPriority.Render);
            };
            BluetoothImpl.Instance.Connecting += (sender, args) =>
            {  
                Dispatcher.UIThread.Post((() => _loadingSpinner.IsVisible = true), DispatcherPriority.Render);
            };
            BluetoothImpl.Instance.InvalidDataReceived += InstanceOnInvalidDataReceived;

            EventDispatcher.Instance.EventReceived += OnEventReceived;
            
            /* Restore data if restarted */
            var cache = DeviceMessageCache.Instance.BasicStatusUpdate;
            if (cache != null)
            {
                ProcessBasicUpdate(cache);
            }
        }

        private async void OnEventReceived(EventDispatcher.Event e, object? arg)
        {
            if (!BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.Anc))
            {
                return;
            }
            
            switch (e)
            {
                case EventDispatcher.Event.AncToggle:
                    await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_NOISE_REDUCTION, !_ancSwitch.IsChecked);
                    Dispatcher.UIThread.Post(_ancSwitch.Toggle);
                    break;
            }
        }

        private void InstanceOnInvalidDataReceived(object? sender, InvalidPacketException e)
        {
            Dispatcher.UIThread.Post((async() =>
            {
                await BluetoothImpl.Instance.DisconnectAsync();
                SetWarning(true, $"{Loc.Resolve("mainpage_corrupt_data")} ({e.ErrorCode})");
                await Task.Delay(500).ContinueWith(async(_)=>
                {
                    await Task.Factory.StartNew(() => BluetoothImpl.Instance.ConnectAsync());
                    SetWarning(false);
                });
            }), DispatcherPriority.Render);
        }

        private void OnLanguageUpdated()
        {
            if (BluetoothImpl.Instance.IsConnected && BluetoothImpl.Instance.ActiveModel != Models.Buds)
            {
                UpdatePlusPlacement(_lastPlacementL, _lastPlacementR);
            }
        }
        
        public override async void OnPageShown()
		{
            SetWarning(false);
            
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.DEBUG_GET_ALL_DATA);
            
			_refreshTimer.Start();
            _caseLabel.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.CaseBattery);

            /* Initial properties */
            if (_lastGetAllDataParser == null)
            {
                var stub = new DebugGetAllDataParser
                {
                    LeftAdcCurrent = 0,
                    LeftAdcSOC = 0,
                    LeftAdcVCell = 0,
                    LeftThermistor = 0,
                    RightAdcCurrent = 0,
                    RightAdcSOC = 0,
                    RightAdcVCell = 0,
                    RightThermistor = 0
                };
                UpdateDetails(stub);
            }

            UpdateList();
            
            _findMyGear.Source = (IImage?)Application.Current.FindResource($"FindMyGear{(BluetoothImpl.Instance.ActiveModel == Models.BudsLive ? "Bean" : "Bud")}");
            _touch.Source = (IImage?)Application.Current.FindResource($"Touch{(BluetoothImpl.Instance.ActiveModel == Models.BudsLive ? "Bean" : "Bud")}");

            _loadingSpinner.IsVisible = !BluetoothImpl.Instance.IsConnected;
        }

		public override void OnPageHidden()
		{
            _refreshTimer.Stop();
        }
        
		public async void ProcessBasicUpdate(IBasicStatusUpdate parser)
        {
            _batteryCase.Content = !BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.CaseBattery) ? string.Empty : $"{parser.BatteryCase}%";
            _caseLabel.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.CaseBattery);

            if (BluetoothImpl.Instance.ActiveModel != Models.Buds)
            {
                UpdatePlusPlacement(parser.PlacementL, parser.PlacementR);
                _lastPlacementL = parser.PlacementL;
                _lastPlacementR = parser.PlacementR;
            }

            if (parser is ExtendedStatusUpdateParser p &&
                BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.Anc))
            {
                _ancSwitch.SetChecked(p.NoiseCancelling);
            }
            
            /* Update if disconnected */
            if ((_lastLeftOnline && parser.BatteryL <= 0) || (_lastRightOnline && parser.BatteryR <= 0))
            {
                UpdateConnectionState(parser.BatteryL > 0, parser.BatteryR > 0);
            }
            
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.DEBUG_GET_ALL_DATA);
        }

        private void UpdatePlusPlacement(PlacementStates l ,PlacementStates r)
        {
            _batteryTemperatureLeft.Content = l.GetDescription();
            _batteryTemperatureRight.Content = r.GetDescription();
        }
        
        private void UpdateTemperature(double left, double right)
        {
            if (BluetoothImpl.Instance.ActiveModel == Models.Buds)
            {
                if (SettingsProvider.Instance.TemperatureUnit == TemperatureUnits.Fahrenheit)
                {
                    _batteryTemperatureLeft.Content = $"{((9.0 / 5.0) * left) + 32:N1} °F";
                    _batteryTemperatureRight.Content = $"{((9.0 / 5.0) * right) + 32:N1} °F";
                }
                else
                {
                    _batteryTemperatureLeft.Content = $"{left:N1} °C";
                    _batteryTemperatureRight.Content = $"{right:N1} °C";
                }
            }
            else
            {
                //Switch positions for a better layout
                if (SettingsProvider.Instance.TemperatureUnit == TemperatureUnits.Fahrenheit)
                {
                    _batteryCurrentLeft.Content = $"{((9.0 / 5.0) * left) + 32:N1} °F";
                    _batteryCurrentRight.Content = $"{((9.0 / 5.0) * right) + 32:N1} °F";
                }
                else
                {
                    _batteryCurrentLeft.Content = $"{left:N1} °C";
                    _batteryCurrentRight.Content = $"{right:N1} °C";
                }
            }
        }

        private void UpdateDetails(DebugGetAllDataParser p)
        {
            _lastGetAllDataParser = p;

            UpdateBatteryPercentage((int) Math.Round(p.LeftAdcSOC), Devices.L);
            UpdateBatteryPercentage((int) Math.Round(p.RightAdcSOC), Devices.R);
            
            _batteryVoltLeft.Content = $"{p.LeftAdcVCell:N2}V";
            _batteryVoltRight.Content = $"{p.RightAdcVCell:N2}V";

            if (BluetoothImpl.Instance.ActiveModel == Models.Buds)
            {
                _batteryCurrentLeft.Content = $"{p.LeftAdcCurrent:N2}mA";
                _batteryCurrentRight.Content = $"{p.RightAdcCurrent:N2}mA";
            }

            UpdateTemperature(p.LeftThermistor, p.RightThermistor);

            var isLeftOnline = p.LeftAdcSOC > 0;
            var isRightOnline = p.RightAdcSOC > 0;
            
            _batteryVoltLeft.IsVisible = isLeftOnline;
            _batteryVoltRight.IsVisible = isRightOnline;
            _batteryCurrentLeft.IsVisible = isLeftOnline;
            _batteryCurrentRight.IsVisible = isRightOnline;
            _batteryTemperatureLeft.IsVisible = isLeftOnline;
            _batteryTemperatureRight.IsVisible = isRightOnline;

            _batteryIconLeft.IsVisible = isLeftOnline;
            _batteryIconRight.IsVisible = isRightOnline;
            _batteryPercentageLeft.IsVisible = isLeftOnline;
            _batteryPercentageRight.IsVisible = isRightOnline;

            UpdateConnectionState(isLeftOnline, isRightOnline);
        }

        private void UpdateConnectionState(bool isLeftOnline, bool isRightOnline)
        {
            _lastLeftOnline = isLeftOnline;
            _lastRightOnline = isRightOnline;
            string type = BluetoothImpl.Instance.ActiveModel == Models.BudsLive ? "Bean" : "Bud";

            if (isLeftOnline)
            {
                _iconLeft.Source = (IImage?)Application.Current.FindResource($"Left{type}Connected");
            }
            else
            {
                _iconLeft.Source = (IImage?)Application.Current.FindResource($"Left{type}Disconnected");
            }

            if (isRightOnline)
            {
                _iconRight.Source = (IImage?)Application.Current.FindResource($"Right{type}Connected");
            }
            else
            {
                _iconRight.Source = (IImage?)Application.Current.FindResource($"Right{type}Disconnected");
            }
        }

        private void UpdateBatteryPercentage(int p, Devices side)
        {
            IImage? batteryIcon;
            if (p <= 0)
            {
                batteryIcon = (IImage?)Application.Current.FindResource("BatteryDisconnected");
            }
            else if (p <= 25)
            {
                batteryIcon = (IImage?)Application.Current.FindResource("BatteryLow");
            }
            else if (p <= 50)
            {
                batteryIcon = (IImage?)Application.Current.FindResource("BatteryMedium");
            }
            else if (p <= 90)
            {
                batteryIcon = (IImage?)Application.Current.FindResource("BatteryHigh");
            }
            else
            {
                batteryIcon = (IImage?)Application.Current.FindResource("BatteryFull");
            }
            
            switch (side)
            {
                case Devices.L:
                    _batteryIconLeft.Source = batteryIcon;
                    _batteryPercentageLeft.Content = $"{p}%";
                    break;
                case Devices.R:
                    _batteryIconRight.Source = batteryIcon;
                    _batteryPercentageRight.Content = $"{p}%";
                    break;
            }
        }

        public void UpdateList()
        {
            _ancBorder.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.Anc);
            _ambientBorder.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientSound);
            _noiseBorder.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.NoiseControl);
        }

        public void SetWarning(bool visible, string message = "")
        {
            _warningContainer.IsVisible = message != string.Empty;
            _warningText.Content = message;
        }

		private async void AncBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			_ancSwitch.Toggle();
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_NOISE_REDUCTION, true);
		}

		private void FindMyBuds_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.FindMyGear);
		}

		private void Ambient_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.AmbientSound);
		}

		private void Touch_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Touch);
		}

		private void Equalizer_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Equalizer);
		}

		private void Advanced_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Advanced);
		}

		private void System_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.System);
		}
        
        private void NoiseBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.NoiseControlPro);
        }

        private void RequestTempUnitChange_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_lastGetAllDataParser == null)
            {
                return;
            }
            
            SettingsProvider.Instance.TemperatureUnit =
                SettingsProvider.Instance.TemperatureUnit == TemperatureUnits.Celsius
                    ? TemperatureUnits.Fahrenheit
                    : TemperatureUnits.Celsius;
            
            UpdateTemperature(_lastGetAllDataParser.LeftThermistor, _lastGetAllDataParser.RightThermistor);
        }
    }
}
