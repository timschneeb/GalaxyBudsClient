using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.Properties;
using Galaxy_Buds_Client.transition;
using Galaxy_Buds_Client.util.DynamicLocalization;
using Application = System.Windows.Application;

namespace Galaxy_Buds_Client.ui.dialog {
    /// <summary>
    /// Interaction logic for BudsPopup.xaml
    /// </summary>
    public partial class BudsPopup : Window
    {

        private bool _isHeaderHidden;
        public bool HideHeader
        {
            get => _isHeaderHidden;
            set
            {
                _isHeaderHidden = value;
                if (value)
                {
                    Height = 205 - 35;
                    HeaderRow.Height = new GridLength(0);
                }
                else
                {
                    Height = 205;
                    HeaderRow.Height = new GridLength(35);
                }
            }
        }

        public EventHandler ClickedEventHandler { get; set; }

        public PopupPlacement PopupPlacement { get; set; }

        public BudsPopup(Model model, int left, int right, int box) {
            InitializeComponent();

            string mod = "";
            if (model == Model.BudsPlus) mod = "+";
            else if (model == Model.BudsLive) mod = " Live";

            string name = Environment.UserName.Split(' ')[0];

            string title = Settings.Default.ConnectionPopupCustomTitle == ""
                ? Loc.GetString("connpopup_title")
                : Settings.Default.ConnectionPopupCustomTitle;

            Greeting.Content = string.Format(title, name, mod);
            UpdateContent(left, right, box);
            
            SPPMessageHandler.Instance.StatusUpdate += Instance_OnStatusUpdate;
        }

        public void ShowWindowWithoutFocus()
        {
            ShowActivated = false;
            ShowInTaskbar = false;
            Topmost = true;

            Storyboard fadeIn = new PageTransition().Resources["FadeIn"] as Storyboard;
            fadeIn?.Begin(this);
            _ = this.ExitPopupAfterDelay();

            Show();
        }

        private void UpdateContent(int bl, int br, int bc)
        {
            Dispatcher.Invoke(() =>
            {
                BatteryL.Content = $"{bl}%";
                BatteryR.Content = $"{br}%";
                BatteryC.Content = $"{bc}%";

                CaseLabel.Visibility = BluetoothService.Instance.ActiveModel != Model.Buds
                    ? Visibility.Visible
                    : Visibility.Hidden;
                BatteryC.Visibility = BluetoothService.Instance.ActiveModel != Model.Buds
                    ? Visibility.Visible
                    : Visibility.Hidden;

                BatteryL.Visibility = bl <= 0 ? Visibility.Hidden : Visibility.Visible;
                BatteryR.Visibility = br <= 0 ? Visibility.Hidden : Visibility.Visible;
                BatteryC.Visibility = bc <= 0 ? Visibility.Hidden : Visibility.Visible;
                CaseLabel.Visibility = bc <= 0 ? Visibility.Hidden : Visibility.Visible;

                string type = BluetoothService.Instance.ActiveModel == Model.BudsLive ? "Bean" : "Bud";

                if (bl > 0)
                {
                    ImageLeft.Source =
                        (ImageSource) Application.Current.Resources.MergedDictionaries[0][$"Left{type}Connected"];
                }
                else
                {
                    ImageLeft.Source =
                        (ImageSource) Application.Current.Resources.MergedDictionaries[0][$"Left{type}Disconnected"];
                }

                if (br > 0)
                {
                    ImageRight.Source =
                        (ImageSource) Application.Current.Resources.MergedDictionaries[0][$"Right{type}Connected"];
                }
                else
                {
                    ImageRight.Source =
                        (ImageSource) Application.Current.Resources.MergedDictionaries[0][$"Right{type}Disconnected"];
                }
            });
        }

        private void Instance_OnStatusUpdate(object sender, parser.StatusUpdateParser e) {
             UpdateContent(e.BatteryL, e.BatteryR, e.BatteryCase);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            ClickedEventHandler?.Invoke(this, null);
            Quit();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var workArea = SystemParameters.WorkArea;

            int padding = 20;
            switch (this.PopupPlacement)
            {
                case PopupPlacement.TopLeft:
                    this.Left = workArea.Left + padding;
                    this.Top = workArea.Top + padding;
                    break;
                case PopupPlacement.TopCenter:
                    this.Left = (workArea.Width / 2f) - (this.Width / 2) + workArea.Left;
                    this.Top = workArea.Top + padding;
                    break;
                case PopupPlacement.TopRight:
                    this.Left = (workArea.Width - this.Width) + workArea.Left - padding;
                    this.Top = workArea.Top + padding;
                    break;
                case PopupPlacement.BottomLeft:
                    this.Left = workArea.Left + padding;
                    this.Top = (workArea.Height - this.Height) + workArea.Top - padding;
                    break;
                case PopupPlacement.BottomCenter:
                    this.Left = (workArea.Width / 2f) - (this.Width / 2) + workArea.Left;
                    this.Top = (workArea.Height - this.Height) + workArea.Top - padding;
                    break;
                case PopupPlacement.BottomRight:
                    this.Left = (workArea.Width - this.Width) + workArea.Left - padding;
                    this.Top = (workArea.Height - this.Height) + workArea.Top - padding;
                    break;
            }
        }

        public void Quit() {
            if (new PageTransition().Resources["FadeOut"] is Storyboard fadeOut)
            {
                fadeOut.Completed += FadeOut_Completed;
                fadeOut.Begin(this);
            }
        }

        private void FadeOut_Completed(object sender, EventArgs e) {
            Close();
        }

        private async Task ExitPopupAfterDelay() {
            await Task.Delay(4000);
            Quit();
        }
    }
}
