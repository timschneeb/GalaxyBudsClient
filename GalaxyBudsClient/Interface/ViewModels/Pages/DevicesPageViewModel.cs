using System.Linq;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class DevicesPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new DevicesPage();
    public override string TitleKey => Keys.DevicesHeader;
    public override Symbol IconKey => Symbol.BluetoothConnected;
    public override bool ShowsInFooter => true;
    
    public async void DoNewCommand()
    {
        await DeviceSelectionDialog.OpenDialogAsync();
    }
    
    public async void DoConnectCommand(object? param)
    {
        if (param is not Device device)
            return;
   
        // TODO
        // device.MacAddress
    }
    
    public async void DoDeleteCommand(object? param)
    {
        if (param is not Device device)
            return;
        
        var result = await new QuestionBox
        {
            Title = Strings.DevicesDeleteLong,
            Description = Strings.DevicesDeleteConfirmation,
            ButtonText = Strings.ContinueButton,
            CloseButtonText = Strings.Cancel
        }.ShowAsync();

        if(result)
            BluetoothImpl.Instance.UnregisterDevice(device);
    }
}