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
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.ui.element;
using Galaxy_Buds_Client.util.DynamicLocalization;

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

        private bool _lastWarningRight = false;
        private bool _lastWarningLeft = false;

        private MainWindow _mainWindow;

        public FindMyGearPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
            SPPMessageHandler.Instance.StatusUpdate += InstanceOnStatusUpdate;
            SPPMessageHandler.Instance.FindMyGearStopped += InstanceOnFindMyGearStopped;
            SPPMessageHandler.Instance.FindMyGearMuteUpdate += InstanceOnFindMyGearMuteUpdate;
            ScannerBtn.ScanningStatusChanged += ScannerBtnOnScanningStatusChanged;
            LeftMuteBtn.StatusChanged += LeftMuteBtnOnStatusChanged;
            RightMuteBtn.StatusChanged += RightMuteBtnOnStatusChanged;
        }

        private void InstanceOnFindMyGearMuteUpdate(object sender, MuteUpdateParser e)
        {
            LeftMuteBtn.SetMuted(e.LeftMuted);
            RightMuteBtn.SetMuted(e.RightMuted);
        }

        private void RightMuteBtnOnStatusChanged(object sender, bool e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.FindMyGear.MuteEarbud(LeftMuteBtn.IsMuted, RightMuteBtn.IsMuted));
        }

        private void LeftMuteBtnOnStatusChanged(object sender, bool e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.FindMyGear.MuteEarbud(LeftMuteBtn.IsMuted, RightMuteBtn.IsMuted));
        }

        private void InstanceOnFindMyGearStopped(object sender, EventArgs e)
        {
            ScannerBtn.Stop();
            Dispatcher.Invoke(() =>
            {
                LeftMuteBtn.Visibility = Visibility.Hidden;
                RightMuteBtn.Visibility = Visibility.Hidden;
                LeftMuteBtn.SetMuted(false);
                RightMuteBtn.SetMuted(false);
            });
        }

        private void ScannerBtnOnScanningStatusChanged(object sender, bool e)
        {
            if (e)
            {
                BluetoothService.Instance.SendAsync(SPPMessageBuilder.FindMyGear.Start());
                LeftMuteBtn.Visibility = Visibility.Visible;
                RightMuteBtn.Visibility = Visibility.Visible;
            }
            else
            {
                BluetoothService.Instance.SendAsync(SPPMessageBuilder.FindMyGear.Stop());
                LeftMuteBtn.Visibility = Visibility.Hidden;
                RightMuteBtn.Visibility = Visibility.Hidden;
                LeftMuteBtn.SetMuted(false);
                RightMuteBtn.SetMuted(false);
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
                EarbudWarning(p.WearState == WearStates.L
                              || p.WearState == WearStates.Both,
                    p.WearState == WearStates.R
                    || p.WearState == WearStates.Both);
            }
            else if (parser.GetType() == typeof(StatusUpdateParser))
            {
                var p = (StatusUpdateParser)parser;
                UpdateBatteryPercentage(p.BatteryL, Side.L);
                UpdateBatteryPercentage(p.BatteryR, Side.R);
                DisableEarbudIcon(p.BatteryL <= 0, p.BatteryR <= 0);
                EarbudWarning(p.WearState == WearStates.L
                              || p.WearState == WearStates.Both,
                    p.WearState == WearStates.R
                    || p.WearState == WearStates.Both);
            }
        }

        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            LeftMuteBtn.Visibility = Visibility.Hidden;
            RightMuteBtn.Visibility = Visibility.Hidden;
            RefreshEarbudIcon();
            LeftMuteBtn.SetMuted(false);
            RightMuteBtn.SetMuted(false);
            EarbudWarning(_lastWarningLeft, _lastWarningRight);
        }

        public override void OnPageHidden()
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.FindMyGear.Stop());
            ScannerBtn.Stop();
        }

        private void EarbudWarning(bool l, bool r)
        {
            _lastWarningLeft = l;
            _lastWarningRight = r;

            String notice = "";
            if (l && r)
            {
                notice = Loc.GetString("fmg_warning_both");
            }
            else if (l)
            {
                notice = Loc.GetString("fmg_warning_left");
            }
            else if (r)
            {
                notice = Loc.GetString("fmg_warning_right");
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

                        string type = BluetoothService.Instance.ActiveModel == Model.BudsLive ? "Bean" : "Bud";

                        if (!l)
                        {
                            LeftIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0][$"Left{type}Connected"];
                            BatteryIconLeft.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            LeftIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0][$"Left{type}Disconnected"];
                            BatteryIconLeft.Visibility = Visibility.Hidden;
                        }

                        if (!r)
                        {
                            RightIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0][$"Right{type}Connected"];
                            BatteryIconRight.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            RightIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0][$"Right{type}Disconnected"];
                            BatteryIconRight.Visibility = Visibility.Hidden;
                        }
                    });
        }

        private void RefreshEarbudIcon()
        {
            Dispatcher.Invoke(() =>
            {
                string type = BluetoothService.Instance.ActiveModel == Model.BudsLive ? "Bean" : "Bud";

                if (LeftIcon.Visibility == Visibility.Visible)
                {
                    LeftIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0][$"Left{type}Connected"];
                }
                else
                {
                    LeftIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0][$"Left{type}Disconnected"];
                }

                if (RightIcon.Visibility == Visibility.Visible)
                {
                    RightIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0][$"Right{type}Connected"];
                }
                else
                {
                    RightIcon.Source = (ImageSource)Application.Current.Resources.MergedDictionaries[0][$"Right{type}Disconnected"];
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
