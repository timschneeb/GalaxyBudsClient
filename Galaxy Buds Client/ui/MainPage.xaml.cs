using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Galaxy_Buds_Client.message;
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

        public MainPage(MainWindow mainWindow)
        {
            InitializeComponent();

            SetWarning(false);

            _mainWindow = mainWindow;
            _refreshTimer.Tick += RefreshTimerCallback;
            _refreshTimer.Interval = new TimeSpan(0, 0, 12);

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
            }
            else if (parser.GetType() == typeof(StatusUpdateParser))
            {
                if (_allowGetAllResponse)
                    BluetoothService.Instance.SendAsync(SPPMessageBuilder.Info.GetAllData());
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
        }

        public override void OnPageHidden()
        {
            SetLoaderVisible(false);
            _allowGetAllResponse = false;
            _refreshTimer.Stop();
        }

        private void UpdateDetails(DebugGetAllDataParser p)
        {
            BatteryVoltLeft.Content = $"{p.LeftAdcVCell:N}V";
            BatteryVoltRight.Content = $"{p.RightAdcVCell:N}V";
            BatteryCurrentLeft.Content = $"{p.LeftAdcCurrent:N}mA";
            BatteryCurrentRight.Content = $"{p.RightAdcCurrent:N}mA";
            if (Properties.Settings.Default.UseFahrenheit)
            {
                BatteryTemperatureLeft.Content = $"{((9.0 / 5.0) * p.LeftThermistor) + 32:N} °F";
                BatteryTemperatureRight.Content = $"{((9.0 / 5.0) * p.RightThermistor) + 32:N} °F";
            }
            else
            {
                BatteryTemperatureLeft.Content = $"{p.LeftThermistor:N} °C";
                BatteryTemperatureRight.Content = $"{p.RightThermistor:N} °C";
            }

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
                LeftIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/device/left_gray.png"));
            }
            else
            {
                LeftIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/device/left_gray_dim.png"));
            }

            if (rightSide == Visibility.Visible)
            {
                RightIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/device/right_gray.png"));
            }
            else
            {
                RightIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/device/right_gray_dim.png"));
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
    }
}
