using System.Threading;
using Avalonia.Interactivity;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.Pages;

public partial class WelcomePage : BasePage<WelcomePageViewModel>
{
    private bool _isOfficialAppInstalled;
    
    public WelcomePage()
    {
        InitializeComponent();
        Register.Focus();
        
        CheckIfOfficialAppIsInstalled();
    }
    
    public async void OnRegisterClicked(object? sender, RoutedEventArgs e)
    {
        if (_isOfficialAppInstalled)
        {
            await new MessageBox
            {
                Title = Strings.BudsappHeader, 
                Description = Strings.BudsappTextP1.Trim() + "\n\n" + 
                              Strings.BudsappTextP2.Trim() + "\n\n" + 
                              Strings.BudsappTextP3.Trim(),
                ButtonText = Strings.ContinueButton
            }.ShowAsync();
        }
        
        await DeviceSelectionDialog.OpenDialogAsync();
    }

    private void CheckIfOfficialAppIsInstalled()
    {
        ThreadPool.QueueUserWorkItem(delegate
        {
            _isOfficialAppInstalled = PlatformImpl.OfficialAppDetector.IsInstalledAsync().Result;
        });
    }
}
