using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Dialogs;

public class MessageBox
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string ButtonText { get; set; } = Loc.Resolve("okay");

    public async Task ShowAsync(Window? host = null)
    {
        var cd = new ContentDialog
        {
            Title = Title,
            Content = Description,
            CloseButtonText = ButtonText,
            DefaultButton = ContentDialogButton.Close
        };

        await cd.ShowAsync(host ?? MainWindow2.Instance);
    }
}