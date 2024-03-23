using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class HotkeyPageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new HotkeyPage();
    public override string TitleKey => "hotkey_header";
    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    public ObservableCollection<Hotkey> Hotkeys { get; } = new(Settings.Instance.Hotkeys ?? Array.Empty<Hotkey>());
    [Reactive] public bool NoHotkeys { private set; get; }

    public HotkeyPageViewModel()
    {
        Hotkeys.CollectionChanged += (_, _) => NoHotkeys = !Hotkeys.Any();
    }
    
    public async void DoNewCommand()
    {
        var result = await HotkeyEditorDialog.OpenEditDialogAsync(null);
        if (result is null) 
            return;
        
        Hotkeys.Add(result);
        SaveChanges();
            
        // Implicitly enable tray icon if a global hotkey is added
        if (PlatformUtils.SupportsTrayIcon)
        {
            Settings.Instance.MinimizeToTray = true;
        }
    }
    
    public async void DoEditCommand(object? param)
    {
        if (param is not Hotkey hotkey)
            return;
        
        var index = Hotkeys.IndexOf(hotkey);
        if (index < 0)
        {
            Log.Debug("HotkeyPage.Edit: Cannot find hotkey in configuration");
            return;
        }

        var result = await HotkeyEditorDialog.OpenEditDialogAsync(hotkey);
        if (result is null)
            return;
        
        Hotkeys[index] = result;
        SaveChanges();
    }
    
    public void DoDeleteCommand(object? param)
    {
        if (param is not Hotkey hotkey)
            return;

        Hotkeys.Remove(hotkey);
        SaveChanges();
    }

    private void SaveChanges()
    {
        Settings.Instance.Hotkeys = Hotkeys.ToArray();
        HotkeyReceiver.Instance.Update();
    }
}