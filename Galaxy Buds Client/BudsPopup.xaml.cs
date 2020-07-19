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
        public BudsPopup(Model model) {
            InitializeComponent();
            string mod = "";
            if (model == Model.BudsPlus) mod = "+";
            string name = Environment.UserName.Split(' ')[0];
            Greeting.Content = $"{name}'s Galaxy Buds{mod}";
            Storyboard popup = new PageTransition().Resources["FadeIn"] as Storyboard;
            if (popup != null) popup.Begin(this);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Rect workArea = System.Windows.SystemParameters.WorkArea;
            this.Left = (workArea.Width / 2) - (this.Width / 2) + workArea.Left;
            this.Top = workArea.Height - this.Height + workArea.Top + 5;
        }
    }
}
