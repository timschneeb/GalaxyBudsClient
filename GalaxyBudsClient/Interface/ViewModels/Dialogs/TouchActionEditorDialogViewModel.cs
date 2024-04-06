using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Extensions;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Dialogs;

public class TouchActionEditorDialogViewModel : ViewModelBase
{
    public TouchActionEditorDialogViewModel() : this(null) {}

    public TouchActionEditorDialogViewModel(CustomAction? action)
    {
        PropertyChanged += OnPropertyChanged;
            
        action ??= new CustomAction(Event.None);
            
        ActionMode = action.Action;
        switch (ActionMode)
        {
            case CustomAction.Actions.Event:
                EventParameter = action.Event;
                break;
            case CustomAction.Actions.RunExternalProgram:
                PathParameter = action.Parameter;
                break;
            case CustomAction.Actions.TriggerHotkey:
                HotkeyParameter = action.Parameter;
                break;
        }
    }

    [Reactive] public CustomAction.Actions ActionMode { set; get; }
    [Reactive] public Event EventParameter { set; get; } = Event.None;
    [Reactive] public string PathParameter { set; get; } = string.Empty;
    [Reactive] public string HotkeyParameter { set; get; } = string.Empty;

    [Reactive] public bool IsEventParameterEditable { set; get; }
    [Reactive] public bool IsPathParameterEditable { set; get; }
    [Reactive] public bool IsHotkeyParameterEditable { set; get; }
        
    public IEnumerable<CustomAction.Actions> ActionModeSource =>
        Enum.GetValues<CustomAction.Actions>()
            .Where(x => x != CustomAction.Actions.TriggerHotkey || PlatformUtils.SupportsHotkeysBroadcast);

    public IEnumerable<Event> EventSource =>
        Enum.GetValues<Event>()
            .Where(EventDispatcher.CheckDeviceSupport)
            .Where(EventDispatcher.CheckTouchOptionEligibility);
    public string InfoBoxMessage => Strings.CactNoticeContentP1 + "\n" + 
                                    Strings.CactNoticeContentP2;

    public CustomAction? Action => VerifyAndMakeAction();

    public async void DoFilePickCommand()
    {
        var filters = new List<FilePickerFileType>
        {
            new("All files") { Patterns = new List<string> { "*" } }
        };
                
        var file = await MainWindow.Instance.OpenFilePickerAsync(filters, Strings.CactExternalAppDialogTitle);
        if (file == null)
            return;
            
        PathParameter = file;
    }

    public async void DoHotkeyRecordCommand()
    {
        var result = await HotkeyRecorderDialog.OpenDialogAsync();
        if(result == null)
            return;
            
        HotkeyParameter = string.Join(",", result);
    }
        
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ActionMode):
                IsEventParameterEditable = ActionMode == CustomAction.Actions.Event;
                IsPathParameterEditable = ActionMode == CustomAction.Actions.RunExternalProgram;
                IsHotkeyParameterEditable = ActionMode == CustomAction.Actions.TriggerHotkey;
                break;
            case nameof(EventParameter):
            case nameof(PathParameter):
            case nameof(HotkeyParameter):
                break;
        }
    }

    private CustomAction? VerifyAndMakeAction(bool notifyUser = true)
    {
        var error = ActionMode switch
        {
            CustomAction.Actions.Event when EventParameter == Event.None => Strings.HotkeyEditInvalidAction,
            CustomAction.Actions.RunExternalProgram when string.IsNullOrWhiteSpace(PathParameter) ||
                                                         !Path.Exists(PathParameter) => Strings.FileNotFound,
            CustomAction.Actions.TriggerHotkey when string.IsNullOrWhiteSpace(HotkeyParameter) => Strings.HotkeyEditInvalid,
            _ => null
        };

        if (error != null)
        {
            if (notifyUser)
            {
                _ = new MessageBox
                {
                    Title = Strings.Error,
                    Description = error
                }.ShowAsync();
            }
            return null;
        }
            
        return new CustomAction(ActionMode, ActionMode switch
        {
            CustomAction.Actions.Event => EventParameter.ToString(),
            CustomAction.Actions.RunExternalProgram => PathParameter,
            CustomAction.Actions.TriggerHotkey => HotkeyParameter,
            _ => throw new IndexOutOfRangeException(nameof(ActionMode))
        });
    }
}