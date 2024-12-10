using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;

namespace GalaxyBudsClient.Interface.Dialogs;

public class MessageBox
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string ButtonText { get; init; } = Strings.Okay;

    public async Task ShowAsync(Visual? host = null)
    {
        var cd = new ContentDialog
        {
            Title = Title,
            Content = Description,
            CloseButtonText = ButtonText,
            DefaultButton = ContentDialogButton.Close
        };

        await cd.ShowAsync(TopLevel.GetTopLevel(MainView.Instance));
    }
}