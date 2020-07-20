using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.transition;

namespace Galaxy_Buds_Client.ui.dialog {
    /// <summary>
    /// Interaction logic for BudsPopup.xaml
    /// </summary>
    public partial class BudsPopup : Window {
        public BudsPopup(Model model, int left, int right, int box) {
            InitializeComponent();

            string mod = "";
            if (model == Model.BudsPlus) mod = "+";
            string name = Environment.UserName.Split(' ')[0];

            Greeting.Content = $"{name}'s Galaxy Buds{mod}";

            BatteryL.Content = $"{left}%";
            BatteryR.Content = $"{right}%";
            if (model == Model.BudsPlus) {
                BatteryC.Content = $"{box}%";
                Case.Content = "Case";
            }

            SPPMessageHandler.Instance.StatusUpdate += Instance_StatusUpdate;

            Storyboard fadeIn = new PageTransition().Resources["FadeIn"] as Storyboard;
            if (fadeIn != null) fadeIn.Begin(this);

            _ = this.exitPopupAfterDelay();
        }

  
        public void ShowWindowWithoutFocus()
        {
            ShowActivated = false;
            Topmost = true;
            Show();
        }

        private void Instance_StatusUpdate(object sender, parser.StatusUpdateParser e) {
            Dispatcher.Invoke(() => {
                BatteryL.Content = $"{e.BatteryL}%";
                BatteryR.Content = $"{e.BatteryR}%";
                if (e.ActiveModel == Model.BudsPlus) {
                    BatteryC.Content = $"{e.BatteryCase}%";
                    Case.Content = "Case";
                }
            });
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Quit();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Rect workArea = SystemParameters.WorkArea;
            this.Left = (workArea.Width / 2) - (this.Width / 2) + workArea.Left;
            this.Top = workArea.Height - this.Height + workArea.Top + 5;
        }

        private void Quit() {
            Storyboard fadeOut = new PageTransition().Resources["FadeOut"] as Storyboard;
            fadeOut.Completed += FadeOut_Completed;
            if (fadeOut != null) fadeOut.Begin(this);
        }

        private void FadeOut_Completed(object sender, EventArgs e) {
            Close();
        }

        private async Task exitPopupAfterDelay() {
            await Task.Delay(5000);
            Quit();
        }
    }
}
