using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.parser;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : BasePage
    {
        public enum ClickEvents
        {
            FindMyGear,
            Touch,
            Ambient,
            System,
            Equalizer,
            Advanced
        }
        public enum Side
        {
            L,
            R
        }

        public event EventHandler<ClickEvents> MainMenuClicked;

        private readonly DispatcherTimer _refreshTimer = new DispatcherTimer();
        private bool _allowGetAllResponse = false;
        private readonly MainWindow _mainWindow;
        private DebugGetAllDataParser _lastGetAllDataParser = null;

        public MainPage(MainWindow mainWindow)
        {
            InitializeComponent();

            SetWarning(false);

            _mainWindow = mainWindow;
            _refreshTimer.Tick += RefreshTimerCallback;
            _refreshTimer.Interval = new TimeSpan(0, 0, 12);

            BatteryCase.Content = "";
            CaseLabel.Visibility = Visibility.Hidden;

            SetLoaderVisible(false);
            UpdateDetails(new DebugGetAllDataParser());

            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
            SPPMessageHandler.Instance.StatusUpdate += InstanceOnStatusUpdate;
            SPPMessageHandler.Instance.GetAllDataResponse += InstanceOnGetAllDataResponse;
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Info.GetAllData());
        }

        public void SetLoaderVisible(bool b)
        {
            Dispatcher.Invoke(() =>
            {
                LoadingSpinner.Visibility = b ? Visibility.Visible : Visibility.Hidden;
                if (b)
                    LoadingSpinner.Start();
                else
                    LoadingSpinner.Stop();
            });

        }

        private void RefreshTimerCallback(object sender, EventArgs e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Info.GetAllData());
        }

        private void InstanceOnGetAllDataResponse(object sender, DebugGetAllDataParser e)
        {
            UpdateDashboard(e);
        }

        private void InstanceOnStatusUpdate(object sender, StatusUpdateParser e)
        {
            UpdateDashboard(e);
        }

        private void InstanceOnExtendedStatusUpdate(object sender, ExtendedStatusUpdateParser e)
        {
            UpdateDashboard(e);
        }

        public void UpdateDashboard(BaseMessageParser parser)
        {
            if (parser.GetType() == typeof(ExtendedStatusUpdateParser))
            {
                if (_allowGetAllResponse)
                    BluetoothService.Instance.SendAsync(SPPMessageBuilder.Info.GetAllData());
                Dispatcher.Invoke(() =>
                {
                    if (BluetoothService.Instance.ActiveModel == Model.BudsPlus)
                    {
                        ExtendedStatusUpdateParser p = (ExtendedStatusUpdateParser)parser;
                        BatteryTemperatureLeft.Content = p.PlacementL.ToString();
                        BatteryTemperatureRight.Content = p.PlacementR.ToString();
                        BatteryCase.Content = p.BatteryCase <= 0 ? "" : $"{p.BatteryCase}%";
                        CaseLabel.Visibility = p.BatteryCase <= 0 ? Visibility.Hidden : Visibility.Visible;
                    }
                });
            }
            else if (parser.GetType() == typeof(StatusUpdateParser))
            {
                if (_allowGetAllResponse)
                    BluetoothService.Instance.SendAsync(SPPMessageBuilder.Info.GetAllData());
                Dispatcher.Invoke(() =>
                {
                    if (BluetoothService.Instance.ActiveModel == Model.BudsPlus)
                    {
                        StatusUpdateParser p = (StatusUpdateParser)parser;
                        BatteryTemperatureLeft.Content = p.PlacementL.ToString();
                        BatteryTemperatureRight.Content = p.PlacementR.ToString();
                        BatteryCase.Content = p.BatteryCase <= 0 ? "" : $"{p.BatteryCase}%";
                        CaseLabel.Visibility = p.BatteryCase <= 0 ? Visibility.Hidden : Visibility.Visible;

                    }
                });
            }
            else if (parser.GetType() == typeof(DebugGetAllDataParser))
            {
                var p = (DebugGetAllDataParser)parser;
                UpdateBatteryPercentage((int)Math.Round(p.LeftAdcSOC), Side.L);
                UpdateBatteryPercentage((int)Math.Round(p.RightAdcSOC), Side.R);

                Dispatcher.Invoke(() =>
                {
                    UpdateDetails(p);
                });
            }
        }

        public override void OnPageShown()
        {
            _mainWindow.SetOptionsEnabled(true);
            _allowGetAllResponse = true;
            _refreshTimer.Start();

            if (BluetoothService.Instance.ActiveModel == Model.Buds)
            {
                BatteryCase.Content = "";
                CaseLabel.Visibility = Visibility.Hidden;
            }
        }

        public override void OnPageHidden()
        {
            SetLoaderVisible(false);
            _allowGetAllResponse = false;
            _refreshTimer.Stop();
        }

        private void UpdateTemperature(double left, double right)
        {
            if (BluetoothService.Instance.ActiveModel == Model.Buds)
            {
                if (Properties.Settings.Default.UseFahrenheit)
                {
                    BatteryTemperatureLeft.Content = $"{((9.0 / 5.0) * left) + 32:N} °F";
                    BatteryTemperatureRight.Content = $"{((9.0 / 5.0) * right) + 32:N} °F";
                }
                else
                {
                    BatteryTemperatureLeft.Content = $"{left:N} °C";
                    BatteryTemperatureRight.Content = $"{right:N} °C";
                }
            }
            else
            {
                //Switch positions for a better layout
                if (Properties.Settings.Default.UseFahrenheit)
                {
                    BatteryCurrentLeft.Content = $"{((9.0 / 5.0) * left) + 32:N} °F";
                    BatteryCurrentRight.Content = $"{((9.0 / 5.0) * right) + 32:N} °F";
                }
                else
                {
                    BatteryCurrentLeft.Content = $"{left:N} °C";
                    BatteryCurrentRight.Content = $"{right:N} °C";
                }
            }
        }

        private void UpdateDetails(DebugGetAllDataParser p)
        {
            _lastGetAllDataParser = p;

            BatteryVoltLeft.Content = $"{p.LeftAdcVCell:N}V";
            BatteryVoltRight.Content = $"{p.RightAdcVCell:N}V";

            if (BluetoothService.Instance.ActiveModel == Model.Buds)
            {
                BatteryCurrentLeft.Content = $"{p.LeftAdcCurrent:N}mA";
                BatteryCurrentRight.Content = $"{p.RightAdcCurrent:N}mA";
            }

            UpdateTemperature(p.LeftThermistor, p.RightThermistor);

            Visibility leftSide = p.LeftAdcSOC <= 0 ? Visibility.Hidden : Visibility.Visible;
            Visibility rightSide = p.RightAdcSOC <= 0 ? Visibility.Hidden : Visibility.Visible;

            BatteryVoltLeft.Visibility = leftSide;
            BatteryVoltRight.Visibility = rightSide;
            BatteryCurrentLeft.Visibility = leftSide;
            BatteryCurrentRight.Visibility = rightSide;
            BatteryTemperatureLeft.Visibility = leftSide;
            BatteryTemperatureRight.Visibility = rightSide;

            BatteryIconLeft.Visibility = leftSide;
            BatteryIconRight.Visibility = rightSide;
            BatteryPercentageLeft.Visibility = leftSide;
            BatteryPercentageRight.Visibility = rightSide;

            if (leftSide == Visibility.Visible)
            {
                LeftIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0]["LeftBudConnected"];
            }
            else
            {
                LeftIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0]["LeftBudDisconnected"];
            }

            if (rightSide == Visibility.Visible)
            {
                RightIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0]["RightBudConnected"];
            }
            else
            {
                RightIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0]["RightBudDisconnected"];
            }
        }

        private void UpdateBatteryPercentage(int p, Side side)
        {
            String imageUri = "pack://application:,,,/Resources/battery/";
            if (p <= 0)
            {
                imageUri += "disconnected.png";
            }
            else if (p <= 25)
            {
                imageUri += "low.png";
            }
            else if (p <= 50)
            {
                imageUri += "medium.png";
            }
            else if (p <= 90)
            {
                imageUri += "high.png";
            }
            else if (p <= 100)
            {
                imageUri += "full.png";
            }

            Dispatcher.Invoke(() =>
            {
                if (side == Side.L)
                {
                    BatteryIconLeft.Source = new BitmapImage(new Uri(imageUri));
                    BatteryPercentageLeft.Content = $"{p}%";
                }
                else if (side == Side.R)
                {
                    BatteryIconRight.Source = new BitmapImage(new Uri(imageUri));
                    BatteryPercentageRight.Content = $"{p}%";
                }
            });

        }

        public void SetWarning(bool visible, String message = "")
        {
            if (message == "")
            {
                Dispatcher.Invoke(() => { WarningContainer.Visibility = Visibility.Hidden; });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    WarningContainer.Visibility = Visibility.Visible;
                    WarningText.Content = message;
                });
            }
        }
        private void FindMyGear_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainMenuClicked?.Invoke(this, ClickEvents.FindMyGear);
        }

        private void Touchpad_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainMenuClicked?.Invoke(this, ClickEvents.Touch);
        }

        private void Ambient_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainMenuClicked?.Invoke(this, ClickEvents.Ambient);
        }

        private void System_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainMenuClicked?.Invoke(this, ClickEvents.System);
        }

        private void Equalizer_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainMenuClicked?.Invoke(this, ClickEvents.Equalizer);
        }

        private void Advanced_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainMenuClicked?.Invoke(this, ClickEvents.Advanced);
        }

        private void SwitchTempUnits_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Properties.Settings.Default.UseFahrenheit = !Properties.Settings.Default.UseFahrenheit;
            Properties.Settings.Default.Save();
            UpdateTemperature(_lastGetAllDataParser.LeftThermistor, _lastGetAllDataParser.RightThermistor);
        }
    }
}
