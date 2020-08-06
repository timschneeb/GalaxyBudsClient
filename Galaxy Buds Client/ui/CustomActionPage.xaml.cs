using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.ui.dialog;
using Galaxy_Buds_Client.util.DynamicLocalization;
using Microsoft.Win32;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for CustomActionPage.xaml
    /// </summary>
    public partial class CustomActionPage : BasePage
    {
        private MainWindow _mainWindow;

        public event EventHandler<CustomAction> Accept;

        private CustomAction _action;

        public CustomActionPage(MainWindow mainWindow)
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
            foreach (int value in Enum.GetValues(typeof(CustomAction.Actions)))
            {
                if (!first)
                    Menu_AddSeparator(ctxMenu);
                else
                    first = false;

                Menu_AddItem(ctxMenu, (CustomAction.Actions)value);
            }

            return ctxMenu;
        }

        private void Menu_AddItem(ContextMenu c, CustomAction.Actions action)
        {
            MenuItem m = new MenuItem();
            m.Header = action.GetDescription();
            m.Click += delegate
            {
                Action.TextDetail = action.GetDescription();
                if (action == CustomAction.Actions.RunExternalProgram)
                {
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.Title = Loc.GetString("cact_external_app_dialog_title");
                    dlg.Multiselect = false;
                    dlg.Filter = "Executable Files|*.exe;*.bat;*.cmd;*.pif|" +
                                 "All Files (*.*)|*.*";

                    bool? result = dlg.ShowDialog();
                    if (result == true)
                    {
                        _action = new CustomAction(action, dlg.FileName);
                    }
                }
                else if (action == CustomAction.Actions.Hotkey)
                {
                    HotkeyRecorder dlg = new HotkeyRecorder();
                    bool? result = dlg.ShowDialog();
                    if (result == true)
                    {
                        _action = new CustomAction(action, dlg.HotkeyString + ";" + String.Join(",", dlg.HotKeysVirtualCodeRaw));
                    }
                }
                else
                {
                    _action = new CustomAction(action);
                }
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
        
        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden; 
            ActionBorder.ContextMenu = GenerateMenu(ActionBorder);
            Action.TextDetail = Loc.GetString("touchoption_custom_null");
            _action = null;
        }

        public override void OnPageHidden()
        {

        }

        
        private void Action_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border el = (Border)sender;
            el.ContextMenu.IsOpen = true;
        }

        private void Cancel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Accept?.Invoke(this, null);
            _mainWindow.GoToPage(MainWindow.Pages.Touch, true);
        }

        private void Apply_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Accept?.Invoke(this, _action);
            _mainWindow.GoToPage(MainWindow.Pages.Touch, true);
        }
    }
}
