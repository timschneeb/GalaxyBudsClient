using Avalonia.Controls;
using Avalonia.Input;
using GalaxyBudsClient.Interface.ViewModels.Controls;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.Controls;

public partial class EarbudCompactStatusUnit : Panel
{
    public EarbudCompactStatusUnit()
    {
        InitializeComponent();
        DataContext = new EarbudCompactStatusUnitViewModel();
    }
}