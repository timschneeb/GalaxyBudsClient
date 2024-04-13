using System.Threading.Tasks;
using Avalonia.Interactivity;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Utils;
using Sentry;

namespace GalaxyBudsClient.Interface.Pages;

public partial class TouchpadPage : BasePage<TouchpadPageViewModel>
{
    public TouchpadPage()
    {
        InitializeComponent();
    }
    
    private void OnEditLeftCustomActionClicked(object? sender, RoutedEventArgs e)
    {
        _ = RunEditDialogAsync(LegacySettings.Instance.CustomActionLeft).ContinueWith(task =>
        {
            if (task is { Result: not null, IsCompletedSuccessfully: true })
            {
                LegacySettings.Instance.CustomActionLeft.Action = task.Result.Action;
                LegacySettings.Instance.CustomActionLeft.Parameter = task.Result.Parameter;
            }
            else if (task.Exception != null) 
                SentrySdk.CaptureException(task.Exception);
        });
    }
    
    private void OnEditRightCustomActionClicked(object? sender, RoutedEventArgs e)
    {
        _ = RunEditDialogAsync(LegacySettings.Instance.CustomActionRight).ContinueWith(task =>
        {
            if (task is { Result: not null, IsCompletedSuccessfully: true })
            {
                LegacySettings.Instance.CustomActionRight.Action = task.Result.Action;
                LegacySettings.Instance.CustomActionRight.Parameter = task.Result.Parameter;
            }
            else if (task.Exception != null) 
                SentrySdk.CaptureException(task.Exception);
        });
    }
    
    private static async Task<CustomAction?> RunEditDialogAsync(ICustomAction? storedAction)
    {
        var action = storedAction != null ? new CustomAction(storedAction.Action, storedAction.Parameter) : null;
        return await TouchActionEditorDialog.OpenEditDialogAsync(action);
    }
}
