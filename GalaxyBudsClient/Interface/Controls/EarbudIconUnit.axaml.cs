using Avalonia.Controls;
using GalaxyBudsClient.Interface.ViewModels.Controls;

namespace GalaxyBudsClient.Interface.Controls
{
    public partial class EarbudIconUnit : UserControl
    {
        public EarbudIconUnit()
        {
            InitializeComponent();
            DataContext = new EarbudIconUnitViewModel();
        }
    }
}
