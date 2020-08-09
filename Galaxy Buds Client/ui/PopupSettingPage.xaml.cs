using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.Properties;
using Galaxy_Buds_Client.ui.dialog;
using Galaxy_Buds_Client.util;
using Galaxy_Buds_Client.util.DynamicLocalization;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for PopupSettingPage.xaml
    /// </summary>
    public partial class PopupSettingPage : BasePage
    {
        private MainWindow _mainWindow;

        public PopupSettingPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
        }

        private ContextMenu GenerateMenu(UIElement target)
        {
            ContextMenu ctxMenu = new ContextMenu();
            ctxMenu.Style = (Style)FindResource("ContextMenuStyle");
            ctxMenu.PlacementTarget = target;
            ctxMenu.Placement = PlacementMode.Bottom;

            bool first = true;
            foreach (var value in EnumHelper.GetValues<PopupPlacement>())
            {
                if (!first)
                {
                    Separator s = new Separator();
                    s.Style = (Style)FindResource("SeparatorStyle");
                    ctxMenu.Items.Add(s);
                }
                first = false;

                MenuItem m = new MenuItem();
                m.Header = value.GetDescription();
                m.Click += delegate
                {
                    PositionPopup.TextDetail = value.GetDescription();
                    Settings.Default.ConnectionPopupPosition = value;
                    Settings.Default.Save();
                };
                m.Style = (Style)FindResource("MenuItemStyle");
                ctxMenu.Items.Add(m);
            }

            return ctxMenu;
        }

        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            PositionPopupBorder.ContextMenu = GenerateMenu(PositionPopupBorder);
            EnablePopup.Switch.SetChecked(Settings.Default.ConnectionPopupEnabled);
            CompactPopup.Switch.SetChecked(Settings.Default.ConnectionPopupCompact);
            PositionPopup.TextDetail = Settings.Default.ConnectionPopupPosition.GetDescription();
            OverrideTitle.TextDetail = Settings.Default.ConnectionPopupCustomTitle == "" ?
                Loc.GetString("notset") : Settings.Default.ConnectionPopupCustomTitle;
        }

        public override void OnPageHidden()
        { }


        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.GoToPage(MainWindow.Pages.Settings, true);
        }

        private void EnablePopup_OnSwitchToggled(object sender, bool e)
        {
            Settings.Default.ConnectionPopupEnabled = e;
            Settings.Default.Save();
        }

        private void CompactPopup_OnSwitchToggled(object sender, bool e)
        {
            Settings.Default.ConnectionPopupCompact = e;
            Settings.Default.Save();
        }

        private void PositionPopup_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border el = (Border)sender;
            el.ContextMenu.IsOpen = true;
        }

        private void TestPopup_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.ShowDemoPopup();
        }

        private void OverrideTitle_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dlg = new InputDialog();
            dlg.Input.Text = Settings.Default.ConnectionPopupCustomTitle;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                OverrideTitle.TextDetail = dlg.Text == "" ? Loc.GetString("notset") : dlg.Text;
                Settings.Default.ConnectionPopupCustomTitle = dlg.Text;
                Settings.Default.Save();
            }
        }
    }
}
