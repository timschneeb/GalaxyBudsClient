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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Galaxy_Buds_Client.ui.basewindow;

namespace Galaxy_Buds_Client.ui.window
{
    /// <summary>
    /// Interaction logic for SWMessageWindow.xaml
    /// </summary>
    public partial class SWMessageWindow : SWWindow
    {
        public SWMessageWindow(String text)
        {
            InitializeComponent();
            SetText(text);
        }

        public void SetText(String text)
        {
            MsgText.Text = text;
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
