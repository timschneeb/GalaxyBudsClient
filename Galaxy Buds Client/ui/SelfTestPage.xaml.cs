using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.util.DynamicLocalization;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class SelfTestPage : BasePage
    {
        private MainWindow _mainWindow;

        private String Waiting => Loc.GetString("system_waiting_for_device");
        private String Left => Loc.GetString("left");
        private String Right => Loc.GetString("right");

        public SelfTestPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            SPPMessageHandler.Instance.SelfTestResponse += InstanceOnSelfTestResponse;
        }

        private void InstanceOnSelfTestResponse(object sender, SelfTestParser e)
        {
            Dispatcher.Invoke(() =>
            {
                LoadingSpinner.Visibility = Visibility.Hidden;
                LoadingSpinner.Stop();

                HwVer.TextDetail = strfy(e.HardwareVersion);
                SwVer.TextDetail = strfy(e.SoftwareVersion);
                TouchFwVer.TextDetail = strfy(e.TouchFirmwareVersion);
                BtAddr.TextDetail = $"{Left}: {strfy(e.LeftBluetoothAddress)}, {Right}: {strfy(e.RightBluetoothAddress)}";
                Proximity.TextDetail = $"{Left}: {strfy(e.LeftProximity)}, {Right}: {strfy(e.RightProximity)}";
                Thermo.TextDetail = $"{Left}: {strfy(e.LeftThermistor)}, {Right}: {strfy(e.RightThermistor)}";
                AdcSoc.TextDetail = $"{Left}: {strfy(e.LeftAdcSOC)}, {Right}: {strfy(e.RightAdcSOC)}";
                AdcVoltage.TextDetail = $"{Left}: {strfy(e.LeftAdcVCell)}, {Right}: {strfy(e.RightAdcVCell)}";
                AdcCurrent.TextDetail = $"{Left}: {strfy(e.LeftAdcCurrent)}, {Right}: {strfy(e.RightAdcCurrent)}";
                Hall.TextDetail = $"{Left}: {strfy(e.LeftHall)}, {Right}: {strfy(e.RightHall)}";
                Accelerator.TextDetail = $"{Left}: {strfy(e.AllLeftAccelerator)}, {Right}: {strfy(e.AllRightAccelerator)}";

                SelfTestResult.Text = e.AllChecks ? Loc.GetString("selftest_pass_long") : Loc.GetString("selftest_fail_long");
            });
        }

        private String strfy(bool b)
        {
            return b ? Loc.GetString("selftest_pass") : Loc.GetString("selftest_fail");
        }
        
        public override void OnPageShown()
        {
            if (BluetoothService.Instance.ActiveModel == Model.BudsPlus)
                Title.Content = Loc.GetString("selftest_header_alt");
            else
                Title.Content = Loc.GetString("selftest_header");

            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Info.RunSelfTest());
            LoadingSpinner.Visibility = Visibility.Visible;
            LoadingSpinner.Start();

            HwVer.TextDetail = Waiting;
            SwVer.TextDetail = Waiting;
            TouchFwVer.TextDetail = Waiting;
            BtAddr.TextDetail = Waiting;
            Proximity.TextDetail = Waiting;
            Thermo.TextDetail = Waiting;
            AdcSoc.TextDetail = Waiting;
            AdcVoltage.TextDetail = Waiting;
            AdcCurrent.TextDetail = Waiting;
            Hall.TextDetail = Waiting;
            Accelerator.TextDetail = Waiting;
            SelfTestResult.Text = Waiting;
        }

        public override void OnPageHidden()
        {
            
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.GoToPage(MainWindow.Pages.System, true);
        }

    }
}
