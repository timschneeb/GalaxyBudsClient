using System.Windows.Controls;

namespace Galaxy_Buds_Client.ui
{
    public abstract class BasePage : UserControl
    {
        public abstract void OnPageShown();
        public abstract void OnPageHidden();
    }
}