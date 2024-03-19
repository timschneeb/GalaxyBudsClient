using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.InterfaceOld.Dialogs;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Extensions;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Dialogs
{
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
            Enum.GetValues(typeof(CustomAction.Actions))
                .Cast<CustomAction.Actions>()
                .Where(x => x != CustomAction.Actions.TriggerHotkey || PlatformUtils.SupportsHotkeysBroadcast);

        public IEnumerable<Event> EventSource =>
            Enum.GetValues(typeof(Event))
                .Cast<Event>()
                .Where(EventDispatcher.CheckDeviceSupport)
                .Where(EventDispatcher.CheckTouchOptionEligibility);
        public string InfoBoxMessage => Loc.Resolve("cact_notice_content_p1") + "\n" + 
                                        Loc.Resolve("cact_notice_content_p2");

        public CustomAction? Action => VerifyAndMakeAction();

        public async void DoFilePickCommand()
        {
            var filters = new List<FilePickerFileType>()
            {
                new("All files") { Patterns = new List<string> { "*" } },
            };
                
            var file = await MainWindow2.Instance.OpenFilePickerAsync(filters);
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
                CustomAction.Actions.Event when EventParameter == Event.None => "hotkey_edit_invalid_action",
                CustomAction.Actions.RunExternalProgram when (string.IsNullOrWhiteSpace(PathParameter) ||
                                                              !Path.Exists(PathParameter)) => "file_not_found",
                CustomAction.Actions.TriggerHotkey when string.IsNullOrWhiteSpace(HotkeyParameter) => "hotkey_edit_invalid",
                _ => null
            };

            if (error != null)
            {
                if (notifyUser)
                {
                    _ = new MessageBox
                    {
                        Title = Loc.Resolve("error"),
                        Description = Loc.Resolve(error)
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
}
