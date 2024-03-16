using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using GalaxyBudsClient.InterfaceOld.Elements;
using GalaxyBudsClient.InterfaceOld.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ToggleSwitch = GalaxyBudsClient.InterfaceOld.Elements.ToggleSwitch;

namespace GalaxyBudsClient.InterfaceOld.Pages
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
        
		private readonly DispatcherTimer _refreshTimer = new();
        private DebugGetAllDataParser? _lastGetAllDataParser;
        private bool _lastLeftOnline;
        private bool _lastRightOnline;

		private PlacementStates _lastPlacementL = PlacementStates.Disconnected;
		private PlacementStates _lastPlacementR = PlacementStates.Disconnected;
		
		public HomePage()
		{   
            AvaloniaXamlLoader.Load(this);
            _ancSwitch = this.GetControl<ToggleSwitch>("AncToggle");
            _loadingSpinner = this.GetControl<LoadingSpinner>("LoadingSpinner");
            
            _caseLabel = this.GetControl<Label>("CaseLabel");
            _batteryCase = this.GetControl<Label>("BatteryCase");
            _batteryVoltLeft = this.GetControl<Label>("BatteryVoltLeft");
            _batteryVoltRight = this.GetControl<Label>("BatteryVoltRight");
            _batteryPercentageLeft = this.GetControl<Label>("BatteryPercentageLeft");
            _batteryPercentageRight = this.GetControl<Label>("BatteryPercentageRight");
            _batteryCurrentLeft = this.GetControl<Label>("BatteryCurrentLeft");
            _batteryCurrentRight = this.GetControl<Label>("BatteryCurrentRight");
            _batteryTemperatureLeft = this.GetControl<Label>("BatteryTemperatureLeft");
            _batteryTemperatureRight = this.GetControl<Label>("BatteryTemperatureRight");

            _batteryIconLeft = this.GetControl<Image>("BatteryIconLeft");
            _batteryIconRight = this.GetControl<Image>("BatteryIconRight");
            _iconLeft = this.GetControl<Image>("LeftIcon");
            _iconRight = this.GetControl<Image>("RightIcon");

            _warningText = this.GetControl<Label>("WarningText");
            _warningContainer = this.GetControl<Grid>("WarningContainer");
            
            _ancBorder = this.GetControl<Border>("AncBorder");
            _ambientBorder = this.GetControl<Border>("AmbientBorder");
            _noiseBorder = this.GetControl<Border>("NoiseBorder");
            
            _findMyGear = this.GetControl<IconListItem>("FindMyGear");
            _touch = this.GetControl<IconListItem>("Touch");
            
            Loc.LanguageUpdated += OnLanguageUpdated;
            
            _refreshTimer.Interval = new TimeSpan(0, 0, 7);
			_refreshTimer.Tick += async (sender, args) =>
				await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA);
            
            SppMessageHandler.Instance.BaseUpdate += (sender, update) => ProcessBasicUpdate(update);
            SppMessageHandler.Instance.GetAllDataResponse += (sender, parser) => UpdateDetails(parser);
            SppMessageHandler.Instance.AncEnabledUpdateResponse += (sender, b) => _ancSwitch.IsChecked = b;
            SppMessageHandler.Instance.AnyMessageReceived += (sender, parser) =>
            {
                /* A warning label is shown when a corrupted/invalid message has been received.
                   As soon as we receive the next valid message, we can hide the warning. */
                SetWarning(false);
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
                Dispatcher.UIThread.Post((async() =>
                {
                    _loadingSpinner.IsVisible = false;
                    await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA);
                    _refreshTimer.Start();
                }), DispatcherPriority.Render);
            };
            BluetoothImpl.Instance.Connecting += (sender, args) =>
            {  
                Dispatcher.UIThread.Post((() =>
                {
                    _loadingSpinner.IsVisible = true;
                    _refreshTimer.Stop();
                }), DispatcherPriority.Render);
            };
            BluetoothImpl.Instance.InvalidDataReceived += InstanceOnInvalidDataReceived;

            EventDispatcher.Instance.EventReceived += OnEventReceived;
        }

        public void ResetCache()
        {
            _lastGetAllDataParser = null;
            _lastPlacementL = PlacementStates.Disconnected;
            _lastPlacementR = PlacementStates.Disconnected;
        }

        private async void OnEventReceived(EventDispatcher.Event e, object? arg)
        {
            // NoiseControl case is handled in NoiseProPage.xaml.cs
            if (!BluetoothImpl.Instance.DeviceSpec.Supports(Features.Anc)
                || BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControl))
            {
                return;
            }
            
            switch (e)
            {
                case EventDispatcher.Event.AncToggle:
                    await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_NOISE_REDUCTION, !_ancSwitch.IsChecked);
                    Dispatcher.UIThread.Post(_ancSwitch.Toggle);
                    EventDispatcher.Instance.Dispatch(EventDispatcher.Event.UpdateTrayIcon);
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
            if (BluetoothImpl.Instance.IsConnectedLegacy && BluetoothImpl.ActiveModel != Models.Buds)
            {
                UpdatePlusPlacement(_lastPlacementL, _lastPlacementR);
            }
        }
        
        public override async void OnPageShown()
		{
            SetWarning(false);

            if (BluetoothImpl.Instance.IsConnectedLegacy)
            {
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA);

                _refreshTimer.Start();
            }

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
            
            _findMyGear.Source = (IImage?)Application.Current?.FindResource($"FindMyGear{BluetoothImpl.Instance.DeviceSpec.IconResourceKey}");
            _touch.Source = (IImage?)Application.Current?.FindResource($"Touch{BluetoothImpl.Instance.DeviceSpec.IconResourceKey}");

            _loadingSpinner.IsVisible = !BluetoothImpl.Instance.IsConnectedLegacy;

            /* Restore data if restarted */
            var cache = DeviceMessageCache.Instance.BasicStatusUpdate;
            if (cache != null)
            {
                ProcessBasicUpdate(cache);
            }
        }

		public override void OnPageHidden()
		{
            _refreshTimer.Stop();
            _batteryCase.IsVisible = _caseLabel.IsVisible = false;
        }

        private async void ProcessBasicUpdate(IBasicStatusUpdate parser)
        {
            if (parser.BatteryCase > 100)
            {
                parser.BatteryCase = DeviceMessageCache.Instance.BasicStatusUpdateWithValidCase?.BatteryCase ?? parser.BatteryCase;
            }

            var hasCaseBattery = BluetoothImpl.Instance.DeviceSpec.Supports(Features.CaseBattery) &&
                               parser.BatteryCase <= 100;
            _batteryCase.Content = hasCaseBattery ? $"{parser.BatteryCase}%" : string.Empty;
            _batteryCase.IsVisible = _caseLabel.IsVisible = hasCaseBattery;

            if (BluetoothImpl.ActiveModel != Models.Buds)
            {
                UpdatePlusPlacement(parser.PlacementL, parser.PlacementR);
                _lastPlacementL = parser.PlacementL;
                _lastPlacementR = parser.PlacementR;
            }

            if (parser is ExtendedStatusUpdateParser p &&
                BluetoothImpl.Instance.DeviceSpec.Supports(Features.Anc))
            {
                _ancSwitch.SetChecked(p.NoiseCancelling);
            }
            
            /* Update if disconnected */
            if ((_lastLeftOnline && parser.BatteryL <= 0) || (_lastRightOnline && parser.BatteryR <= 0))
            {
                UpdateConnectionState(parser.BatteryL > 0, parser.BatteryR > 0);
            }
            
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA);
        }

        private void UpdatePlusPlacement(PlacementStates l, PlacementStates r)
        {
            if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.Voltage))
            {
                _batteryTemperatureLeft.Content = l.GetDescription();
                _batteryTemperatureRight.Content = r.GetDescription();
            }
            else
            {
                _batteryTemperatureRight.Content = "";
                _batteryTemperatureLeft.Content = "";
                _batteryCurrentLeft.Content = l.GetDescription();
                _batteryCurrentRight.Content = r.GetDescription();
            }
        }
        
        private void UpdateTemperature(double left, double right)
        {
            string tempLeft, tempRight;
            if (Settings.Instance.TemperatureUnit == TemperatureUnits.Fahrenheit)
            {
                 tempLeft = $"{((9.0 / 5.0) * left) + 32:N1} °F";
                 tempRight = $"{((9.0 / 5.0) * right) + 32:N1} °F";
            }
            else
            {
                tempLeft = $"{left:N1} °C";
                tempRight = $"{right:N1} °C";
            }
            if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.Current))
            {
                _batteryTemperatureLeft.Content = tempLeft;
                _batteryTemperatureRight.Content = tempRight;
            }
            //Switch positions for a better layout
            else if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.Voltage))
            {
                _batteryCurrentLeft.Content = tempLeft;
                _batteryCurrentRight.Content = tempRight;
            }
            else
            {
                _batteryVoltLeft.Content = tempLeft;
                _batteryVoltRight.Content = tempRight;
            }
        }

        private void UpdateDetails(DebugGetAllDataParser p)
        {
            _lastGetAllDataParser = p;

            UpdateBatteryPercentage((int) Math.Round(p.LeftAdcSOC), Devices.L);
            UpdateBatteryPercentage((int) Math.Round(p.RightAdcSOC), Devices.R);

            if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.Voltage))
            {
                _batteryVoltLeft.Content = $"{p.LeftAdcVCell:N2}V";
                _batteryVoltRight.Content = $"{p.RightAdcVCell:N2}V";
            }

            if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.Current))
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

            var type = BluetoothImpl.Instance.DeviceSpec.IconResourceKey;
            
            var leftSourceName = $"Left{type}{(isLeftOnline ? "Connected" : "Disconnected")}";
            _iconLeft.Source = (IImage?)Application.Current?.FindResource(leftSourceName);
            var rightSourceName = $"Right{type}{(isRightOnline ? "Connected" : "Disconnected")}";
            _iconRight.Source = (IImage?)Application.Current?.FindResource(rightSourceName);
        }

        private void UpdateBatteryPercentage(int p, Devices side)
        {
            var batteryIcon = p switch
            {
                <= 0 => (IImage?)Application.Current?.FindResource("BatteryDisconnected"),
                <= 25 => (IImage?)Application.Current?.FindResource("BatteryLow"),
                <= 50 => (IImage?)Application.Current?.FindResource("BatteryMedium"),
                <= 90 => (IImage?)Application.Current?.FindResource("BatteryHigh"),
                _ => (IImage?)Application.Current?.FindResource("BatteryFull")
            };

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

        private void UpdateList()
        {
            _ancBorder.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(Features.Anc);
            _ambientBorder.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(Features.AmbientSound);
            _noiseBorder.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControl);
        }

        private void SetWarning(bool visible, string message = "")
        {
            _warningContainer.IsVisible = message != string.Empty;
            _warningText.Content = message;
        }

		private async void AncBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			_ancSwitch.Toggle();
			await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_NOISE_REDUCTION, _ancSwitch.IsChecked);
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
            
            Settings.Instance.TemperatureUnit =
                Settings.Instance.TemperatureUnit == TemperatureUnits.Celsius
                    ? TemperatureUnits.Fahrenheit
                    : TemperatureUnits.Celsius;
            
            UpdateTemperature(_lastGetAllDataParser.LeftThermistor, _lastGetAllDataParser.RightThermistor);
        }
    }
}
