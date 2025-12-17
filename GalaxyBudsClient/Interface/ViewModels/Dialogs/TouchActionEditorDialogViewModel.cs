using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Interface.ViewModels.Dialogs;

public partial class TouchActionEditorDialogViewModel : ViewModelBase
{
    public TouchActionEditorDialogViewModel() : this(null) {}

    public TouchActionEditorDialogViewModel(CustomAction? action)
    {
        PropertyChanged += OnPropertyChanged;
            
        action ??= new CustomAction(Event.None);
            
        ActionMode = action.Action;
        switch (ActionMode)
        {
            case CustomActions.Event:
                EventParameter = action.Event;
                break;
            case CustomActions.RunExternalProgram:
                PathParameter = action.Parameter;
                break;
            case CustomActions.TriggerHotkey:
                HotkeyParameter = action.Parameter;
                break;
        }
    }

    [Reactive] private CustomActions _actionMode;
    [Reactive] private Event _eventParameter = Event.None;
    [Reactive] private string _pathParameter = string.Empty;
    [Reactive] private string _hotkeyParameter = string.Empty;

    [Reactive] private bool _isEventParameterEditable;
    [Reactive] private bool _isPathParameterEditable;
    [Reactive] private bool _isHotkeyParameterEditable;
        
    public IEnumerable<CustomActions> ActionModeSource =>
        CustomActionsExtensions.GetValues()
            .Where(x => x != CustomActions.TriggerHotkey || PlatformUtils.SupportsHotkeysBroadcast);

    public IEnumerable<Event> EventSource =>
        EventExtensions.GetValues()
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
                
        var file = await TopLevel.GetTopLevel(MainView.Instance)!.OpenFilePickerAsync(filters, Strings.CactExternalAppDialogTitle);
        var path = file?.TryGetLocalPath();
        if (path == null)
            return;
            
        PathParameter = path;
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
                IsEventParameterEditable = ActionMode == CustomActions.Event;
                IsPathParameterEditable = ActionMode == CustomActions.RunExternalProgram;
                IsHotkeyParameterEditable = ActionMode == CustomActions.TriggerHotkey;
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
            CustomActions.Event when EventParameter == Event.None => Strings.HotkeyEditInvalidAction,
            CustomActions.RunExternalProgram when string.IsNullOrWhiteSpace(PathParameter) ||
                                                         !Path.Exists(PathParameter) => Strings.FileNotFound,
            CustomActions.TriggerHotkey when string.IsNullOrWhiteSpace(HotkeyParameter) => Strings.HotkeyEditInvalid,
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
            CustomActions.Event => EventParameter.ToString(),
            CustomActions.RunExternalProgram => PathParameter,
            CustomActions.TriggerHotkey => HotkeyParameter,
            _ => throw new IndexOutOfRangeException(nameof(ActionMode))
        });
    }
}
