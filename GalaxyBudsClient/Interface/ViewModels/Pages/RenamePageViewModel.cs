using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class RenamePageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new RenamePage();
    public override string TitleKey => Keys.Rename;

    [Reactive] public string? WarningText { set; get; }
    [Reactive] public bool IsActive { set; get; }
    private byte? _charLimit;
    private string _name = "";

    public RenamePageViewModel()
    {
        Loc.LanguageUpdated += OnLanguageUpdated;
        BluetoothImpl.Instance.ConnectedAlternative += OnConnected;
        BluetoothImpl.Instance.DisconnectedAlternative += (s, _) => OnConnected(s, EventArgs.Empty);
        BluetoothImpl.Instance.MessageReceivedAlternative += MsgReceivedAlt;
        BluetoothImpl.Instance.InvalidDataReceivedAlternative += (_, ex) => Log.Error(ex.ToString());
        BluetoothImpl.Instance.BluetoothErrorAlternative += (_, ex) => Log.Error(ex.ToString());
    }

    public override void OnNavigatedTo()
    {
        BluetoothImpl.Instance.Disconnected += OnDisconnected;
        _ = BluetoothImpl.Instance.DisconnectAsync();
        IsActive = BluetoothImpl.Instance.IsConnectedAlternative;
    }

    private void OnLanguageUpdated()
    {
        OnConnected(null, EventArgs.Empty);
    }

    private void OnConnected(object? sender, EventArgs e) // or disconnected from alt
    {
        IsActive = BluetoothImpl.Instance.IsConnectedAlternative;
        WarningText = !IsActive ? Strings.ConnlostConnecting : null;

        if (IsActive)
        {
            _ = SppAlternativeMessage.ReadPropertyAsync(SppAlternativeMessage.AltProperty.SUPPORTED_FEATURES);
        }
        else
        {
            _charLimit = null;
        }
    }

    private void OnDisconnected(object? sender, string e) // disconnected from main
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!BluetoothImpl.Instance.SetAltMode(true))
            {
                WarningText = Strings.Connlost;
                return;
            }

            Task.Run(() => { _ = BluetoothImpl.Instance.ConnectAsync(null, true); });
        });
    }

    public void NameTextChanged(string text)
    {
        _name = text;
    }

    public void DoRenameCommand()
    {
        var str = _name;
        var strBytes = Encoding.UTF8.GetBytes(str);
        if (_charLimit == null || str.Length < 1 || strBytes.Length >= _charLimit)
        {
            _ = new MessageBox
            {
                Title = Strings.Rename,
                Description = Strings.RenameTooShort
            }.ShowAsync(MainWindow.Instance);
            return;
        }
        var prop = new SppAlternativeMessage.Property(SppAlternativeMessage.AltProperty.CMD_PERSONALIZED_NAME_TIMESTAMP,
            ((byte[]) [0, 0]).Concat(BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds())).ToArray());
        var prop2 = new SppAlternativeMessage.Property(SppAlternativeMessage.AltProperty.CMD_PERSONALIZED_NAME_VALUE,
            ((byte[]) [0, 0]).Concat(strBytes).ToArray());
        _ = SppAlternativeMessage.WritePropertyAsync(prop.Encode().Concat(prop2.Encode()).ToArray());
    }

    private void MsgReceivedAlt(object? sender, SppAlternativeMessage e)
    {
        if (e.Id == MsgIds.READ_PROPERTY)
        {
            var data = SppAlternativeMessage.ReadProperty.Decode(e);
            if (data.Type == SppAlternativeMessage.AltProperty.SUPPORTED_FEATURES)
            {
                _charLimit = null;
                for (var i = 0; i < data.Response.Count; i++)
                {
                    var msg = data.Response[i];
                    if (msg.Type == SppAlternativeMessage.AltProperty.FEATURE_CHANGE_DEVICE_NAME)
                    {
                        _charLimit = msg.Response[0];
                    }
                }
            }
        }
        else if (e.Id == MsgIds.UNIVERSAL_MSG_ID_ACKNOWLEDGEMENT)
        {
            Dispatcher.UIThread.Post(() => _ = new MessageBox {
                Title = Strings.Rename,
                Description = Strings.RenameOk
            }.ShowAsync(MainWindow.Instance));
        }
    }

    public override void OnNavigatedFrom()
    {
        BluetoothImpl.Instance.Disconnected -= OnDisconnected;
        Task.Run(() =>
        {
            _ = BluetoothImpl.Instance.DisconnectAsync(true).ContinueWith(
                lastTask =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        BluetoothImpl.Instance.SetAltMode(false);
                        Task.Run(() => { _ = BluetoothImpl.Instance.ConnectAsync(); });
                    });
                });
        });
        IsActive = false;
    }
}