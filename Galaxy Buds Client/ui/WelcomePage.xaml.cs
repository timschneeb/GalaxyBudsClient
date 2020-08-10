using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.util;
using Galaxy_Buds_Client.util.DynamicLocalization;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class WelcomePage : BasePage
    {
        private MainWindow _mainWindow;

        public WelcomePage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
        }

        public override void OnPageShown()
        {
            _mainWindow.SetOptionsEnabled(false);
            LocaleBorder.ContextMenu = GenerateMenu(LocaleBorder);

            if (Properties.Settings.Default.DarkMode2 == model.Constants.DarkMode.System)
                DarkMode.Visibility = Visibility.Collapsed;
            else
                DarkMode.Visibility = Visibility.Visible;

            DarkMode.Switch.SetChecked(Properties.Settings.Default.DarkMode2 == model.Constants.DarkMode.Dark);
        }

        public override void OnPageHidden()
        {

        }
        private void Continue_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.GoToPage(MainWindow.Pages.DeviceSelect);
        }

        private void DarkMode_OnSwitchToggled(object sender, bool e)
        {
            Properties.Settings.Default.DarkMode2 = (DarkMode)Convert.ToInt32(e);
            Properties.Settings.Default.Save();
            DarkModeHelper.Update();
        }

        private void Language_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border el = (Border)sender;
            el.ContextMenu.IsOpen = true;
        }

        private ContextMenu GenerateMenu(UIElement target)
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

                Menu_AddItem(ctxMenu, (Locale)value);
            }

            return ctxMenu;
        }

        private void Menu_AddItem(ContextMenu c, Locale loc)
        {
            MenuItem m = new MenuItem();
            m.Header = loc.GetDescription();
            m.Click += delegate
            {
                Properties.Settings.Default.CurrentLocale = loc;
                Properties.Settings.Default.Save();
                Loc.Load();
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
    }
}
