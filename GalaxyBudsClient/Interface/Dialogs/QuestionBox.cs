using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Dialogs;

public class QuestionBox
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string ButtonText { get; set; } = Loc.Resolve("okay");
    public string CloseButtonText { get; set; } = Loc.Resolve("cancel");

    public async Task<bool> ShowAsync(Window? host = null)
    {
        var cd = new ContentDialog
        {
            Title = Title,
            Content = Description,
            PrimaryButtonText = ButtonText,
            CloseButtonText = CloseButtonText,
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await cd.ShowAsync(host ?? MainWindow2.Instance);
        return result == ContentDialogResult.Primary;
    }
}