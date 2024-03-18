using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.InterfaceOld.Dialogs;
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
    public ObservableCollection<Hotkey> Hotkeys { get; } = new(Settings.Instance.Hotkeys);
    [Reactive] public bool NoHotkeys { private set; get; }

    public HotkeyPageViewModel()
    {
        Hotkeys.CollectionChanged += (_, _) => NoHotkeys = !Hotkeys.Any();
    }
    
    public async void DoNewCommand()
    {
        var builder = new HotkeyActionBuilder();
        await builder.ShowDialog(MainWindow2.Instance);

        if (builder is { Result: true, Hotkey: not null })
        {
            Hotkeys.Add(builder.Hotkey);
            SaveChanges();
            
            // Implicitly enable tray icon if a global hotkey is added
            if (PlatformUtils.SupportsTrayIcon)
            {
                Settings.Instance.MinimizeToTray = true;
            }
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
                          
        var builder = new HotkeyActionBuilder(hotkey);
        await builder.ShowDialog(MainWindow2.Instance);

        if (builder is { Result: true, Hotkey: not null })
        {
            Hotkeys[index] = builder.Hotkey;
            SaveChanges();
        }
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
        HotkeyReceiverImpl.Instance.Update();
    }

}