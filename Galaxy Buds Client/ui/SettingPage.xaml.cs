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
using Galaxy_Buds_Client.model.Constants;
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

            LocaleBorder.ContextMenu = Locale_GenerateMenu(LocaleBorder);
            LocaleSelect.TextDetail = Properties.Settings.Default.CurrentLocale.GetDescription();

            DarkModeBorder.ContextMenu = DarkMode_GenerateMenu(DarkModeBorder);
            DarkModeSelect.TextDetail = Properties.Settings.Default.DarkMode2.GetDescription();

            TransitionToggle.SetChecked(!Properties.Settings.Default.DisableSlideTransition);
            MinimizeTrayToggle.SetChecked(Properties.Settings.Default.MinimizeTray);
            AutostartToggle.SetChecked(AutoStartHelper.Enabled);
        }

        public override void OnPageHidden()
        {

        }

        private ContextMenu Locale_GenerateMenu(UIElement target)
        {
            ContextMenu ctxMenu = new ContextMenu();
            ctxMenu.Style = (Style)FindResource("ContextMenuStyle");
            ctxMenu.PlacementTarget = target;
            ctxMenu.Placement = PlacementMode.Bottom;

            bool first = true;
            foreach (int value in Enum.GetValues(typeof(Locale)))
            {
                if (value == (int)Locale.custom && !Loc.IsTranslatorModeEnabled())
                    continue;

                if (!first)
                    Menu_AddSeparator(ctxMenu);
                else
                    first = false;

                Locale_Menu_AddItem(ctxMenu, (Locale)value);
            }

            return ctxMenu;
        }

        private void Locale_Menu_AddItem(ContextMenu c, Locale loc)
        {
            MenuItem m = new MenuItem();
            m.Header = loc.GetDescription();
            m.Click += delegate
            {
                LocaleSelect.TextDetail = loc.GetDescription();
                Properties.Settings.Default.CurrentLocale = loc;
                Properties.Settings.Default.Save();
                Loc.Load();

                /* Update cached strings */
                DarkModeBorder.ContextMenu = DarkMode_GenerateMenu(DarkModeBorder);
                DarkModeSelect.TextDetail = Properties.Settings.Default.DarkMode2.GetDescription();

                if(BluetoothService.Instance.IsConnected)
                    BluetoothService.Instance.SendAsync(SPPMessageBuilder.Info.GetAllData());
            };
            m.Style = (Style)FindResource("MenuItemStyle");
            c.Items.Add(m);
        }
        private void Menu_AddSeparator(ContextMenu c)
        {
            Separator s = new Separator();
            s.Style = (Style)FindResource("SeparatorStyle");
            c.Items.Add(s);
        }
        private ContextMenu DarkMode_GenerateMenu(UIElement target)
        {
            ContextMenu ctxMenu = new ContextMenu();
            ctxMenu.Style = (Style)FindResource("ContextMenuStyle");
            ctxMenu.PlacementTarget = target;
            ctxMenu.Placement = PlacementMode.Bottom;

            bool first = true;
            foreach (int value in Enum.GetValues(typeof(DarkMode)))
            {
                if (value == (int) DarkMode.Unset)
                    continue;

                if (!first)
                    Menu_AddSeparator(ctxMenu);
                else
                    first = false;

                DarkMode_Menu_AddItem(ctxMenu, (DarkMode)value);
            }

            return ctxMenu;
        }

        private void DarkMode_Menu_AddItem(ContextMenu c, DarkMode mode)
        {
            MenuItem m = new MenuItem();
            m.Header = mode.GetDescription();
            m.Click += delegate
            {
                DarkModeSelect.TextDetail = mode.GetDescription();
                Properties.Settings.Default.DarkMode2 = mode;
                Properties.Settings.Default.Save();
                DarkModeHelper.Update();

                if (BluetoothService.Instance.IsConnected)
                    BluetoothService.Instance.SendAsync(SPPMessageBuilder.Info.GetAllData());
            };
            m.Style = (Style)FindResource("MenuItemStyle");
            c.Items.Add(m);
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
            Border el = (Border)sender;
            el.ContextMenu.IsOpen = true;
        }

        private void SaveChanges()
        {
            Properties.Settings.Default.DisableSlideTransition = !TransitionToggle.IsChecked;
            Properties.Settings.Default.MinimizeTray = MinimizeTrayToggle.IsChecked;
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
            Border el = (Border)sender;
            el.ContextMenu.IsOpen = true;
        }

        private void PopupSettings_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.GoToPage(MainWindow.Pages.SettingsPopup);
        }
    }
}
