using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform.Model;


namespace GalaxyBudsClient.Interface.ViewModels.Dialogs;

public partial class ManualPairDialogViewModel : ViewModelBase
{ 
    public required IEnumerable<BluetoothDevice> Devices { init; get; }
    [Reactive] private BluetoothDevice? _selectedDevice;
    [Reactive] private Models? _selectedModel;
}
