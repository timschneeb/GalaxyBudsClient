using System.Collections.ObjectModel;
using Avalonia.Input;
using GalaxyBudsClient.Utils.Extensions;


namespace GalaxyBudsClient.Interface.ViewModels.Dialogs;

public partial class HotkeyRecorderDialogViewModel : ViewModelBase
{
    public HotkeyRecorderDialogViewModel()
    {
        Hotkeys = new ObservableCollection<Key>();
        Hotkeys.CollectionChanged += (_, _) => HotkeyPreview = Hotkeys.AsAvaloniaHotkeyString();
    }
        
    [Reactive] private string _hotkeyPreview = string.Empty;

    public ObservableCollection<Key> Hotkeys { get; }
}
