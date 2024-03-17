using Avalonia.Controls;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class FirmwarePageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new FirmwarePage();
    public override string TitleKey => "system_header";
    
    public FirmwarePageViewModel()
    {
        BluetoothImpl.Instance.Connected += (_, _) => RequestData();
    }
    
    public static async void RequestData()
    { 
        if(!BluetoothImpl.Instance.IsConnected)
            return;
        
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.DebugSku))
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_SKU);
    }

    public override async void OnNavigated()
    {
        if (Settings.Instance.FirmwareWarningAccepted) 
            return;
        
        // Show disclaimer
        var result = await new QuestionBox
        {
            Title = Loc.Resolve("fw_disclaimer"),
            Description = Loc.Resolve("fw_disclaimer_desc"),
            ButtonText = Loc.Resolve("continue_button")
        }.ShowAsync();

        Settings.Instance.FirmwareWarningAccepted = result;
        if (!result)
        {
            MainWindow2.Instance.MainView.FrameView.GoBack();
        }
    }
}


