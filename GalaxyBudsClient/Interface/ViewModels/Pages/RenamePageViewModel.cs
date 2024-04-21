using System;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class RenamePageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new RenamePage();
    public override string TitleKey => Keys.Rename;
    
    [Reactive] public string? WarningText { set; get; }
    [Reactive] public bool IsWarningHidden { set; get; }
    [Reactive] public bool IsActive { set; get; }
    
    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            
            if (!IsNameValid(value))
                throw new DataValidationException(Strings.RenameTooShort);
        }
    }
    
    private byte? _charLimit;
    private string _name = Strings.RenameHint;

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
        
        IsActive = IsWarningHidden = BluetoothImpl.Instance.IsConnectedAlternative;
        WarningText = Strings.ConnlostConnecting;
    }

    private void OnLanguageUpdated()
    {
        OnConnected(null, EventArgs.Empty);
    }

    private void OnConnected(object? sender, EventArgs e) // or disconnected from alt
    {
        IsActive = BluetoothImpl.Instance.IsConnectedAlternative;
        WarningText = !IsActive ? Strings.ConnlostConnecting : Strings.RenameReadingName;

        if (IsActive)
        {
            _ = SppAlternativeMessage.ReadPropertyAsync(SppAlternativeMessage.AltProperty.SUPPORTED_FEATURES);
            _ = SppAlternativeMessage.ReadPropertyAsync(SppAlternativeMessage.AltProperty.ALL_CURRENT_STATES);
        }
        else
        {
            IsWarningHidden = true;
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

    private bool IsNameValid(string name) =>
        !(_charLimit == null || name.Length < 1 || Encoding.UTF8.GetBytes(name).Length >= _charLimit);
    
    public void DoRenameCommand()
    {
        if(!IsNameValid(Name))
            return;
        
        var nameBytes = Encoding.UTF8.GetBytes(Name);
        var prop = new SppAlternativeMessage.Property(SppAlternativeMessage.AltProperty.CMD_PERSONALIZED_NAME_TIMESTAMP,
            ((byte[]) [0, 0]).Concat(BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds())).ToArray());
        var prop2 = new SppAlternativeMessage.Property(SppAlternativeMessage.AltProperty.CMD_PERSONALIZED_NAME_VALUE,
            ((byte[]) [0, 0]).Concat(nameBytes).ToArray());
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
                foreach (var msg in data.Response)
                {
                    if (msg.Type == SppAlternativeMessage.AltProperty.FEATURE_CHANGE_DEVICE_NAME)
                    {
                        _charLimit = msg.Response[0];
                    }
                }
            }
            else if (data.Type == SppAlternativeMessage.AltProperty.ALL_CURRENT_STATES)
            {
                foreach (var msg in data.Response)
                {
                    if (msg.Type == SppAlternativeMessage.AltProperty.STATE_CURRENT_NAME)
                    {
                        IsWarningHidden = true;
                        
                        var currentName = Encoding.UTF8.GetString(msg.Response);
                        if (IsNameValid(currentName))
                        {
                            Name = currentName;
                        }
                    }
                }
            }
        }
        else if (e.Id == MsgIds.UNIVERSAL_MSG_ID_ACKNOWLEDGEMENT)
        {
            Dispatcher.UIThread.Post(() => _ = new MessageBox {
                Title = Strings.RenameOkTitle,
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