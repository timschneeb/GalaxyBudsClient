using System.Collections.ObjectModel;
using Avalonia.Input;
using GalaxyBudsClient.Utils.Extensions;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Dialogs;

public class HotkeyRecorderDialogViewModel : ViewModelBase
{
    public HotkeyRecorderDialogViewModel()
    {
        Hotkeys = new ObservableCollection<Key>();
        Hotkeys.CollectionChanged += (_, _) => HotkeyPreview = Hotkeys.AsAvaloniaHotkeyString();
    }
        
    [Reactive] public string HotkeyPreview { set; get; } = string.Empty;

    public ObservableCollection<Key> Hotkeys { get; private set; }
}