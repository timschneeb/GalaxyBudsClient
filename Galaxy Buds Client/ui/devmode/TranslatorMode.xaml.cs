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
            ForceEnglish.IsChecked = Properties.Settings.Default.ForceEnglish;
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

        private void ForceEnglish_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ForceEnglish.IsChecked != null)
                Properties.Settings.Default.ForceEnglish = (bool)ForceEnglish.IsChecked;
            Properties.Settings.Default.Save();
            Loc.Load();
        }

        private void ReloadXaml_Click(object sender, RoutedEventArgs e)
        {
            ForceEnglish.IsChecked = false;
            Properties.Settings.Default.ForceEnglish = false;
            Properties.Settings.Default.Save();
            Loc.Load();
        }
    }
}
