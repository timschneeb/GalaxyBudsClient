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
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.ui.element;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class FindMyGearPage : BasePage
    {
        public enum Side
        {
            L,
            R
        }

        private MainWindow _mainWindow;

        public FindMyGearPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
            SPPMessageHandler.Instance.StatusUpdate += InstanceOnStatusUpdate;
            SPPMessageHandler.Instance.FindMyGearStopped += InstanceOnFindMyGearStopped;
            ScannerBtn.ScanningStatusChanged += ScannerBtnOnScanningStatusChanged;
        }

        private void InstanceOnFindMyGearStopped(object sender, EventArgs e)
        {
            ScannerBtn.Stop();
        }

        private void ScannerBtnOnScanningStatusChanged(object sender, bool e)
        {
            if (e)
            {
                BluetoothService.Instance.SendAsync(SPPMessageBuilder.FindMyGear.Start());
            }
            else
            {
                BluetoothService.Instance.SendAsync(SPPMessageBuilder.FindMyGear.Stop());

            }
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
                var p = (ExtendedStatusUpdateParser)parser;
                UpdateBatteryPercentage(p.BatteryL, Side.L);
                UpdateBatteryPercentage(p.BatteryR, Side.R);
                DisableEarbudIcon(p.BatteryL <= 0, p.BatteryR <= 0);
                EarbudWarning(p.WearState == Constants.WearStates.L
                              || p.WearState == Constants.WearStates.Both,
                    p.WearState == Constants.WearStates.R
                    || p.WearState == Constants.WearStates.Both);
            }
            else if (parser.GetType() == typeof(StatusUpdateParser))
            {
                var p = (StatusUpdateParser)parser;
                UpdateBatteryPercentage(p.BatteryL, Side.L);
                UpdateBatteryPercentage(p.BatteryR, Side.R);
                DisableEarbudIcon(p.BatteryL <= 0, p.BatteryR <= 0);
                EarbudWarning(p.WearState == Constants.WearStates.L
                              || p.WearState == Constants.WearStates.Both,
                    p.WearState == Constants.WearStates.R
                    || p.WearState == Constants.WearStates.Both);
            }
        }

        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
        }

        public override void OnPageHidden()
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.FindMyGear.Stop());
            ScannerBtn.Stop();
        }

        private void EarbudWarning(bool l, bool r)
        {
            String notice = "";
            if (l && r)
            {
                notice = "Please remove both earbuds from your ears.";
            }
            else if (l)
            {
                notice = "Please remove the left earbud from your ear.";
            }
            else if (r)
            {
                notice = "Please remove the right earbud from your ear.";
            }

            if (notice == "")
            {
                Dispatcher.Invoke(() => { EarbudWarningContainer.Visibility = Visibility.Hidden; });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    EarbudWarningContainer.Visibility = Visibility.Visible;
                    EarbudWarningText.Content = notice;
                });
            }
        }

        private void DisableEarbudIcon(bool l, bool r)
        {
            Dispatcher.Invoke(() =>
                    {
                        if (!l)
                        {
                            LeftIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/device/left_gray.png"));
                            BatteryIconLeft.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            LeftIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/device/left_gray_dim.png"));
                            BatteryIconLeft.Visibility = Visibility.Hidden;
                        }

                        if (!r)
                        {
                            RightIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/device/right_gray.png"));
                            BatteryIconRight.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            RightIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/device/right_gray_dim.png"));
                            BatteryIconRight.Visibility = Visibility.Hidden;
                        }
                    });
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
            else
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                if (side == Side.L)
                {
                    BatteryIconLeft.Source = new BitmapImage(new Uri(imageUri));
                }
                else if (side == Side.R)
                {
                    BatteryIconRight.Source = new BitmapImage(new Uri(imageUri));
                }
            });

        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.ReturnToHome();
        }

    }
}
