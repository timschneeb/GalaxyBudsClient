using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Interface.Pages;

public partial class HiddenModePage : BasePage<HiddenModePageViewModel>
{
    public HiddenModePage()
    {
        InitializeComponent();
    }

    private void OnChangeSerialClicked(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void OnHiddenTerminalClicked(object? sender, RoutedEventArgs e)
    {
        // TODO
    }
}