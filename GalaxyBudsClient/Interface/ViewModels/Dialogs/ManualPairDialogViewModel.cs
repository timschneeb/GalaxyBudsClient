using System.Collections.Generic;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Model.Constants;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Dialogs;

public class ManualPairDialogViewModel : ViewModelBase
{ 
    public required IEnumerable<BluetoothDevice> Devices { init; get; }
    [Reactive] public BluetoothDevice? SelectedDevice { set; get; }
    [Reactive] public Models SelectedModel { set; get; }
}