using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;

namespace GalaxyBudsClient.Interface.Dialogs;

public class QuestionBox
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string ButtonText { get; init; } = Strings.Okay;
    public string CloseButtonText { get; init; } = Strings.Cancel;

    public async Task<bool> ShowAsync(TopLevel? host = null)
    {
        var cd = new ContentDialog
        {
            Title = Title,
            Content = Description,
            PrimaryButtonText = ButtonText,
            CloseButtonText = CloseButtonText,
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await cd.ShowAsync(host ?? TopLevel.GetTopLevel(MainView.Instance));
        return result == ContentDialogResult.Primary;
    }
}