using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.transition;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Galaxy_Buds_Client {
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

            Battery.Content = $"left: {left} right: {right}";
            if (model == Model.BudsPlus) Battery.Content += $" case: {box}";

            Storyboard fadeIn = new PageTransition().Resources["FadeIn"] as Storyboard;
            if (fadeIn != null) fadeIn.Begin(this);

            _ = this.exitPopupAfterDelay();
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
