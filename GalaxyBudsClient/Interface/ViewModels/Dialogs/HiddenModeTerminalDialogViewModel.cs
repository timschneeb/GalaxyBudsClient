using System.ComponentModel;
using System.Linq;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.ViewModels.Dialogs;

public partial class HiddenModeTerminalDialogViewModel : ViewModelBase
{
    public HiddenModeTerminalDialogViewModel()
    {
        SppMessageReceiver.Instance.HiddenCmdData += OnHiddenCmdDataReceived;
    }

    [Reactive] private string _commandId = string.Empty;
    [Reactive] private string _commandParameter = string.Empty;
    [Reactive] private string _terminalOutput = string.Empty;
    
    public async void DoSendCommand()
    {
        if (string.IsNullOrEmpty(CommandId))
        {
            return;
        }

        if (!CommandId.All(char.IsAsciiHexDigit))
        {
            _ = new MessageBox()
            {
                Title = Strings.Error,
                Description = Strings.AtTerminalCmdIdInvalid
            }.ShowAsync();
            return;
        }
        
        TerminalOutput = string.Empty;
        await BluetoothImpl.Instance.SendAsync(new HiddenCmdDataEncoder
        {
            CommandId = CommandId,
            Parameter = CommandParameter
        });
    }
    
    private void OnHiddenCmdDataReceived(object? sender, HiddenCmdDataDecoder e)
    {
        TerminalOutput = e.Content;
    }
}
