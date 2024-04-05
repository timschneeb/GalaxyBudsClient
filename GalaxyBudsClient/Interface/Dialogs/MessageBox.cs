using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.Dialogs;

public class MessageBox
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string ButtonText { get; set; } = Strings.Okay;

    public async Task ShowAsync(Window? host = null)
    {
        var cd = new ContentDialog
        {
            Title = Title,
            Content = Description,
            CloseButtonText = ButtonText,
            DefaultButton = ContentDialogButton.Close
        };

        await cd.ShowAsync(host ?? MainWindow.Instance);
    }
}