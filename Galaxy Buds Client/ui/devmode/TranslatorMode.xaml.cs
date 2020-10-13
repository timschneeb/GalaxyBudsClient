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
using System.Windows.Shapes;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.util.DynamicLocalization;

namespace Galaxy_Buds_Client.ui.devmode
{
    /// <summary>
    /// Interaction logic for TranslatorMode.xaml
    /// </summary>
    public partial class TranslatorMode : Window
    {
        private MainWindow _mainWindow;
        public TranslatorMode(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            InitializeComponent();

            Language.SelectedValue = Properties.Settings.Default.CurrentLocale;
            ModelSel.SelectedValue = _mainWindow.GetRegisteredDeviceModel();
            XamlPath.Text = Loc.GetTranslatorModeFile();
        }

        private void Jump_Click(object sender, RoutedEventArgs e)
        {
            if (Pages.SelectedValue == null)
            {
                MessageBox.Show("Please select a page", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                _mainWindow.GoToPage((MainWindow.Pages)Pages.SelectedValue);
            }
        }
        
        private void ReloadXaml_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CurrentLocale = (Locale)Language.SelectedValue;
            Properties.Settings.Default.Save();
            Loc.Load();
        }

        private void IgnoreConnLost_Checked(object sender, RoutedEventArgs e)
        {
            BluetoothService.Instance.SuppressSocketException =
                IgnoreConnLost.IsChecked == null? false : (bool) IgnoreConnLost.IsChecked;
        }

        private void Model_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Model? model = (Model?)ModelSel.SelectedValue;
            if (model == null)
                model = Model.NULL;
            BluetoothService.Instance.ActiveModel = (Model)model;
            Properties.Settings.Default.RegisteredDeviceModel = (Model)model;
            Properties.Settings.Default.Save();
        }
    }
}
