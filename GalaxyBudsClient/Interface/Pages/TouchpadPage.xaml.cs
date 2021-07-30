using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Interop.TrayIcon;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
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
 	public class TouchpadPage : AbstractPage
	{
		public override Pages PageType => Pages.Touch;

        public bool TouchpadLocked => _lock.IsChecked;
        
		private readonly SwitchListItem _lock;
		private readonly SwitchDetailListItem _edgeTouch;
		private readonly MenuDetailListItem _noiseControlMode;
		private readonly MenuDetailListItem _leftOption;
		private readonly MenuDetailListItem _rightOption;
		
		private TouchOptions _lastLeftOption;
		private TouchOptions _lastRightOption;
		private (bool, bool, bool) _lastNoiseControlMode;
		
		/* ANC, Ambient, Off */
		private static readonly (bool, bool, bool, string)[] NoiseControlModeMap = new[]
		{
			(true, true, false, "touchpad_noise_control_mode_anc_amb"),
			(true, false, true, "touchpad_noise_control_mode_anc_off"),
			(false, true, true, "touchpad_noise_control_mode_amb_off"),
			(true, true, true, "touchpad_noise_control_mode_threeway"),
		};
		
		public TouchpadPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_lock = this.FindControl<SwitchListItem>("LockToggle");
			_noiseControlMode = this.FindControl<MenuDetailListItem>("NoiseSwitchMode");
			_edgeTouch = this.FindControl<SwitchDetailListItem>("DoubleTapVolume");
			_leftOption = this.FindControl<MenuDetailListItem>("LeftOption");
			_rightOption = this.FindControl<MenuDetailListItem>("RightOption");

            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
            EventDispatcher.Instance.EventReceived += OnEventReceived;
            
			Loc.LanguageUpdated += UpdateMenus;
			Loc.LanguageUpdated += UpdateMenuDescriptions;
			UpdateMenus();
		}

		private async void OnEventReceived(EventDispatcher.Event e, object? arg)
		{
			switch (e)
			{
				case EventDispatcher.Event.LockTouchpadToggle:
					_lock.Toggle();
					await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.LOCK_TOUCHPAD, _lock.IsChecked);
					break;
				case EventDispatcher.Event.ToggleDoubleEdgeTouch:
					_edgeTouch.Toggle();
					await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.OUTSIDE_DOUBLE_TAP, _edgeTouch.IsChecked);
					break;
			}
		}
		
        private void InstanceOnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
		{
			_lock.IsChecked = e.TouchpadLock;
			_edgeTouch.IsChecked = e.OutsideDoubleTap;

			_lastLeftOption = e.TouchpadOptionL;
			_lastRightOption = e.TouchpadOptionR;

			_leftOption.Description = e.TouchpadOptionL.GetDescription();
			_rightOption.Description = e.TouchpadOptionR.GetDescription();

			_lastNoiseControlMode = (e.NoiseControlTouchAnc, e.NoiseControlTouchAmbient, e.NoiseControlTouchOff);
			
			UpdateMenuDescriptions();
		}

		private void UpdateMenuDescriptions()
		{
			bool known = false;
			foreach (var mode in NoiseControlModeMap)
			{
				if (mode.Item1 == _lastNoiseControlMode.Item1 &&
				    mode.Item2 == _lastNoiseControlMode.Item2 &&
				    mode.Item3 == _lastNoiseControlMode.Item3)
				{
					_noiseControlMode.Description = Loc.Resolve(mode.Item4);
					known = true;
					break;
				}
			}	
			if(!known)
            {
             	Log.Warning("NoiseControlSwitchMode: Unknown mode");
             	_noiseControlMode.Description = Loc.Resolve("touchpad_noise_control_mode_anc_amb");
            }
			
			if (_lastLeftOption == TouchOptions.OtherL)
			{
				_leftOption.Description =
					$"{Loc.Resolve("touchoption_custom_prefix")} {new CustomAction(SettingsProvider.Instance.CustomActionLeft.Action, SettingsProvider.Instance.CustomActionLeft.Parameter)}";
			}
			else
			{
				_leftOption.Description = _lastLeftOption.GetDescription();
			}

			if (_lastRightOption == TouchOptions.OtherR)
			{
				_rightOption.Description =
					$"{Loc.Resolve("touchoption_custom_prefix")} {new CustomAction(SettingsProvider.Instance.CustomActionRight.Action, SettingsProvider.Instance.CustomActionRight.Parameter)}";
			}
			else
			{
				_rightOption.Description = _lastRightOption.GetDescription();
			}
		}
		
		private void UpdateMenus()
		{
			foreach (var obj in Enum.GetValues(typeof(Devices)))
			{
				if (obj is Devices device)
				{
					var menuActions = new Dictionary<string,EventHandler<RoutedEventArgs>?>();
                    foreach (var value in BluetoothImpl.Instance.DeviceSpec.TouchMap.LookupTable)
                    {
                        if (value.Key.IsHidden())
                            continue;
                        
                        menuActions.Add(value.Key.GetDescription(), (sender, args) => ItemClicked(device, value.Key));
                    }
                    
                    /* Inject custom actions if appropriate */
                    if (BluetoothImpl.Instance.DeviceSpec.TouchMap.LookupTable.ContainsKey(TouchOptions.OtherL) || 
                        BluetoothImpl.Instance.DeviceSpec.TouchMap.LookupTable.ContainsKey(TouchOptions.OtherR))
                    {
	                    menuActions.Add(Loc.Resolve("touchoption_custom"), (sender, args) =>
		                    ItemClicked(device, device == Devices.L ? TouchOptions.OtherL : TouchOptions.OtherR));
                    }

                    switch (device)
                    {
	                    case Devices.L:
		                    _leftOption.Items = menuActions;
		                    break;
	                    case Devices.R:
		                    _rightOption.Items = menuActions;
		                    break;
                    }
				}
			}
			
			var noiseActions = new Dictionary<string,EventHandler<RoutedEventArgs>?>();
			foreach (var value in NoiseControlModeMap)
			{
				noiseActions[Loc.Resolve(value.Item4)] = async (sender, args) =>
				{
					_lastNoiseControlMode = (value.Item1, value.Item2, value.Item3);

					await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS, 
						BitConverter.GetBytes(value.Item1)[0], 
						BitConverter.GetBytes(value.Item2)[0], 
						BitConverter.GetBytes(value.Item3)[0]);
					UpdateMenuDescriptions();
				};
			}
			_noiseControlMode.Items = noiseActions;
		}

		private async void ItemClicked(Devices device, TouchOptions option)
		{
			if (option == TouchOptions.OtherL || option == TouchOptions.OtherR)
			{
				/* Open custom action selector first and await user input,
				 do not send the updated options ids just yet  */
                MainWindow.Instance.ShowCustomActionSelection(device);
			}
			else
			{
				if (device == Devices.L)
				{
					_lastLeftOption = option;
				}
				else
				{
					_lastRightOption = option;
				}
				
				UpdateMenuDescriptions();
				UpdateNoiseSwitchModeVisible();
				await MessageComposer.Touch.SetOptions(_lastLeftOption, _lastRightOption);
				await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS, 
					BitConverter.GetBytes(_lastNoiseControlMode.Item1)[0], 
					BitConverter.GetBytes(_lastNoiseControlMode.Item2)[0], 
					BitConverter.GetBytes(_lastNoiseControlMode.Item3)[0]);
            }
			
		}
		
		private async void CustomTouchActionPageOnAccepted(object? sender, CustomAction e)
		{
			if (MainWindow.Instance.CustomTouchActionPage.CurrentSide == Devices.L)
			{
				_lastLeftOption = TouchOptions.OtherL;
				_leftOption.Description = $"{Loc.Resolve("touchoption_custom_prefix")} {e}";
			}
			else
			{
				_lastRightOption = TouchOptions.OtherR;
				_rightOption.Description = $"{Loc.Resolve("touchoption_custom_prefix")} {e}"; 
			}

			UpdateNoiseSwitchModeVisible();
			await MessageComposer.Touch.SetOptions(_lastLeftOption, _lastRightOption);
		}

		public override void OnPageShown()
		{
			_edgeTouch.Parent.IsVisible =
				BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.DoubleTapVolume);
			UpdateNoiseSwitchModeVisible();
			
			MainWindow.Instance.CustomTouchActionPage.Accepted += CustomTouchActionPageOnAccepted;

			UpdateMenus();
			UpdateMenuDescriptions();
		}

		public void UpdateNoiseSwitchModeVisible()
		{
			_noiseControlMode.Parent.IsVisible =
				BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.NoiseControl) 
				&& (_lastLeftOption == TouchOptions.NoiseControl || _lastRightOption == TouchOptions.NoiseControl);
		}
		
		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Home);
		}

		private async void LockToggle_OnToggled(object? sender, bool e)
		{
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.LOCK_TOUCHPAD, e);
		}

		private async void DoubleTapVolume_OnToggled(object? sender, bool e)
		{
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.OUTSIDE_DOUBLE_TAP, e);
		}
	}
}
