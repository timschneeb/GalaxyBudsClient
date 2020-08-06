using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.transition;
using Galaxy_Buds_Client.util.DynamicLocalization;

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

        public PopupPlacement PopupPlacement { get; set; }

        public BudsPopup(Model model, int left, int right, int box) {
            InitializeComponent();

            string mod = "";
            if (model == Model.BudsPlus) mod = "+";
            string name = Environment.UserName.Split(' ')[0];

            Greeting.Content = string.Format(Loc.GetString("connpopup_title"), name, mod);
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

                CaseLabel.Visibility = BluetoothService.Instance.ActiveModel == Model.BudsPlus
                    ? Visibility.Visible
                    : Visibility.Hidden;
                BatteryC.Visibility = BluetoothService.Instance.ActiveModel == Model.BudsPlus
                    ? Visibility.Visible
                    : Visibility.Hidden;

                BatteryL.Visibility = bl <= 0 ? Visibility.Hidden : Visibility.Visible;
                BatteryR.Visibility = br <= 0 ? Visibility.Hidden : Visibility.Visible;

                if (bl > 0)
                {
                    ImageLeft.Source =
                        (ImageSource) Application.Current.Resources.MergedDictionaries[0]["LeftBudConnected"];
                }
                else
                {
                    ImageLeft.Source =
                        (ImageSource) Application.Current.Resources.MergedDictionaries[0]["LeftBudDisconnected"];
                }

                if (br > 0)
                {
                    ImageRight.Source =
                        (ImageSource) Application.Current.Resources.MergedDictionaries[0]["RightBudConnected"];
                }
                else
                {
                    ImageRight.Source =
                        (ImageSource) Application.Current.Resources.MergedDictionaries[0]["RightBudDisconnected"];
                }
            });
        }

        private void Instance_OnStatusUpdate(object sender, parser.StatusUpdateParser e) {
             UpdateContent(e.BatteryL, e.BatteryR, e.BatteryCase);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Quit();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Rect workArea = SystemParameters.WorkArea;

            int padding = 20;
            switch (this.PopupPlacement)
            {
                case PopupPlacement.TopLeft:
                    this.Left = workArea.Left + padding;
                    this.Top = workArea.Top + padding;
                    break;
                case PopupPlacement.TopCenter:
                    this.Left = (workArea.Width / 2) - (this.Width / 2) + workArea.Left;
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
                    this.Left = (workArea.Width / 2) - (this.Width / 2) + workArea.Left;
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
