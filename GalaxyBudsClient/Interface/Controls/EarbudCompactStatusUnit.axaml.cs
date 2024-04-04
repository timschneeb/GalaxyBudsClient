using Avalonia.Controls;
using GalaxyBudsClient.Interface.ViewModels.Controls;

namespace GalaxyBudsClient.Interface.Controls;

public partial class EarbudCompactStatusUnit : Panel
{
    public EarbudCompactStatusUnit()
    {
        InitializeComponent();
        DataContext = new EarbudCompactStatusUnitViewModel();
    }
}