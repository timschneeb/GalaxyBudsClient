using System.Linq;
using Avalonia.Controls;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class HotkeyPageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new HotkeyPage();
    public override string TitleKey => Generated.I18N.Keys.HotkeyHeader;
    [Reactive] public bool NoHotkeys { private set; get; }

    public HotkeyPageViewModel()
    {
        NoHotkeys = !Settings.Data.Hotkeys.Any();
        Settings.HotkeyCollectionChanged += (_, _) => NoHotkeys = !Settings.Data.Hotkeys.Any();
    }
    
    public async void DoNewCommand()
    {
        var result = await HotkeyEditorDialog.OpenEditDialogAsync(null);
        if (result is null) 
            return;
        
        Settings.Data.Hotkeys.Add(result);
        SaveChanges();
            
        // Implicitly enable tray icon if a global hotkey is added
        if (PlatformUtils.SupportsTrayIcon)
        {
            Settings.Data.MinimizeToTray = true;
        }
    }
    
    public async void DoEditCommand(object? param)
    {
        if (param is not Hotkey hotkey)
            return;
        
        var index = Settings.Data.Hotkeys.IndexOf(hotkey);
        if (index < 0)
        {
            Log.Debug("HotkeyPage.Edit: Cannot find hotkey in configuration");
            return;
        }

        var result = await HotkeyEditorDialog.OpenEditDialogAsync(hotkey);
        if (result is null)
            return;
        
        Settings.Data.Hotkeys[index] = result;
        SaveChanges();
    }
    
    public void DoDeleteCommand(object? param)
    {
        if (param is not Hotkey hotkey)
            return;

        Settings.Data.Hotkeys.Remove(hotkey);
        SaveChanges();
    }

    private static void SaveChanges()
    {
        HotkeyReceiver.Instance.Update();
    }
}