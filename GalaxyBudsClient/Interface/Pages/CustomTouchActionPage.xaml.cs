using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
    public class CustomTouchActionPage : AbstractPage
    {
        public override Pages PageType => Pages.TouchCustomAction;
		
        private readonly PageHeader _pageHeader;
        private readonly MenuDetailListItem _menuDetail;

        private CustomAction? _currentAction;
		
        public event EventHandler<CustomAction>? Accepted;
		
        public Devices CurrentSide { get; set; }
		
        public CustomTouchActionPage()
        {   
            AvaloniaXamlLoader.Load(this);
            _pageHeader = this.FindControl<PageHeader>("PageHeader");
            _menuDetail = this.FindControl<MenuDetailListItem>("Menu");

            Loc.LanguageUpdated += UpdateStrings;
        }
        
        public override void OnPageShown()
        {
            _currentAction = null;
            _menuDetail.Description = Loc.Resolve("touchoption_custom_null");
            UpdateStrings();
        }

        public void UpdateStrings()
        {
            _pageHeader.Title = $"{Loc.Resolve("cact_header")} ({Loc.Resolve(CurrentSide == Devices.L ? "left" : "right")})";
            UpdateTouchActionMenus();
        }
		
        private void UpdateTouchActionMenus()
        {
            foreach (var obj in Enum.GetValues(typeof(Devices)))
            {
                var menuActions = new Dictionary<string,EventHandler<RoutedEventArgs>?>();
                foreach (var value in Enum.GetValues(typeof(CustomAction.Actions)))
                {
                    if (value is CustomAction.Actions action)
                    {
                        if (action == CustomAction.Actions.Event)
                        {
                            continue;
                        }
                        
                        if (!PlatformUtils.IsWindows && action == CustomAction.Actions.TriggerHotkey)
                        {
                            continue;
                        }
                        
                        menuActions.Add(action.GetDescription(), (sender, args) => ItemClicked(new CustomAction(action)));
                    }
                }
                
                Enum.GetValues(typeof(EventDispatcher.Event))
                    .Cast<EventDispatcher.Event>()
                    .Where(EventDispatcher.CheckDeviceSupport)
                    .Where(EventDispatcher.CheckTouchOptionEligibility)
                    .ToList()
                    .ForEach(x =>
                    {
                        var action = new CustomAction(x);
                        menuActions.Add(x.GetDescription(), (sender, args) => ItemClicked(action));   
                    });
                
                _menuDetail.Items = menuActions;
            }
        }

        private async void ItemClicked(CustomAction action)
        {
            switch (action.Action)
            {
                case CustomAction.Actions.RunExternalProgram:
                {
                    OpenFileDialog dlg = new OpenFileDialog
                    {
                        Title = Loc.Resolve("cact_external_app_dialog_title"), 
                        AllowMultiple = false
                    };
                    
                    string[]? result = await dlg.ShowAsync(MainWindow.Instance);
                    if (result != null && result.Length > 0)
                    {
                        _currentAction = new CustomAction(action.Action, result[0]);
                    }

                    break;
                }
                case CustomAction.Actions.TriggerHotkey:
                    var recorder = new HotkeyRecorder();
                    await recorder.ShowDialog(MainWindow.Instance);
                    if (recorder.Result)
                    {
                        _currentAction = new CustomAction(action.Action, string.Join(",", recorder.Hotkeys ?? new List<Key>()));
                    }
                    break;
                default:
                    _currentAction = action;
                    break;
            }

            if (_currentAction != null)
            {
                _menuDetail.Description = _currentAction.ToLongString();
            }
        }
		
        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.Touch);
        }

        private void Apply_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_currentAction != null)
            {
                Accepted?.Invoke(this, _currentAction);
                
                if (CurrentSide == Devices.L)
                {
                    SettingsProvider.Instance.CustomActionLeft.Action = _currentAction.Action;
                    SettingsProvider.Instance.CustomActionLeft.Parameter = _currentAction.Parameter;
                }
                else
                {
                    SettingsProvider.Instance.CustomActionRight.Action = _currentAction.Action;
                    SettingsProvider.Instance.CustomActionRight.Parameter = _currentAction.Parameter;
                }
            }

            MainWindow.Instance.Pager.SwitchPage(Pages.Touch);
        }
    }
}