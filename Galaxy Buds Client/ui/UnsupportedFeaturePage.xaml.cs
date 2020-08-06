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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.util.DynamicLocalization;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for UnsupportedFeaturePage.xaml
    /// </summary>
    public partial class UnsupportedFeaturePage : BasePage
    {
        private MainWindow _mainWindow;

        public UnsupportedFeaturePage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();

            SPPMessageHandler.Instance.SwVersionResponse += InstanceOnSwVersionResponse;
        }

        private void InstanceOnSwVersionResponse(object sender, string e)
        {
            Dispatcher.Invoke(() => { CurrentFw.Text = $"{Loc.GetString("unsupported_feature_current_fwver")} {e.Remove(0, 1)}"; });
        }


        public override void OnPageShown()
        {
        }

        public override void OnPageHidden()
        {
        }

        public void SetRequiredVersion(String e)
        {
            Dispatcher.Invoke(() =>
            {
                RequiredFw.Text = $"{Loc.GetString("unsupported_feature_required_fwver")} {e}";
            });
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.ReturnToHome(true);
        }
    }
}
