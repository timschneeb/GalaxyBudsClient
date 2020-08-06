using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.ui.devmode;
using Galaxy_Buds_Client.ui.element;
using Galaxy_Buds_Client.util;
using Galaxy_Buds_Client.util.DynamicLocalization;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for AmbientSoundPage.xaml
    /// </summary>
    public partial class SettingPage : BasePage
    {
        private MainWindow _mainWindow;

        public SettingPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();

        }

        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;

            TransitionToggle.SetChecked(Properties.Settings.Default.DisableSlideTransition);
            LocaleToggle.SetChecked(Properties.Settings.Default.ForceEnglish);
            MinimizeTrayToggle.SetChecked(Properties.Settings.Default.MinimizeTray);
            AutostartToggle.SetChecked(AutoStartHelper.Enabled);
            DarkModeToggle.SetChecked(Properties.Settings.Default.DarkMode);
        }

        public override void OnPageHidden()
        {

        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.ReturnToHome();
        }

        private void Transition_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TransitionToggle.Toggle();
            SaveChanges();
        }

        private void Locale_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LocaleToggle.Toggle();
            SaveChanges();
            Loc.Load();
        }

        private void SaveChanges()
        {
            Properties.Settings.Default.ForceEnglish = LocaleToggle.IsChecked;
            Properties.Settings.Default.DisableSlideTransition = TransitionToggle.IsChecked;
            Properties.Settings.Default.MinimizeTray = MinimizeTrayToggle.IsChecked;
            Properties.Settings.Default.DarkMode = DarkModeToggle.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void DevMode_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            new DevWindow().Show();
        }

        private void MinimizedTray_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MinimizeTrayToggle.Toggle();
            if (!MinimizeTrayToggle.IsChecked && AutostartToggle.IsChecked)
            {
                AutostartToggle.Toggle();
                AutoStartHelper.Enabled = AutostartToggle.IsChecked;
            }
            SaveChanges();
        }

        private void Autostart_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AutostartToggle.Toggle();
            if (!MinimizeTrayToggle.IsChecked && AutostartToggle.IsChecked)
            {
                MinimizeTrayToggle.Toggle();
            }

            AutoStartHelper.Enabled = AutostartToggle.IsChecked;
            SaveChanges();
        }

        private void DarkMode_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DarkModeToggle.Toggle();
            DarkModeHelper.SetState(DarkModeToggle.IsChecked);
            SaveChanges();
        }

        private void PopupSettings_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.GoToPage(MainWindow.Pages.SettingsPopup);
        }
    }
}
