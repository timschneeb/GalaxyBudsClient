using System.ComponentModel;
using System.Linq;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Platform;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Dialogs;

public class HiddenModeTerminalDialogViewModel : ViewModelBase
{
    public HiddenModeTerminalDialogViewModel()
    {
        PropertyChanged += OnPropertyChanged;
        SppMessageReceiver.Instance.HiddenCmdData += OnHiddenCmdDataReceived;
    }

    [Reactive] public string CommandId { set; get; } = string.Empty;
    [Reactive] public string CommandParameter { set; get; } = string.Empty;
    [Reactive] public string TerminalOutput { set; get; } = string.Empty;
    
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
        TerminalOutput = e.Content ?? "<null>";
    }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(CommandId):
                break;
        }
    }
}