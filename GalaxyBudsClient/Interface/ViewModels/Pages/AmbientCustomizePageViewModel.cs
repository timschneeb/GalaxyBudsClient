using System.ComponentModel;
using Avalonia.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class AmbientCustomizePageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new AmbientCustomizePage();
    
    public AmbientCustomizePageViewModel()
    {
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
       
        Settings.Instance.RegisteredDevice.PropertyChanged += OnDevicePropertyChanged;
        PropertyChanged += OnPropertyChanged;
        UpdateVolumeSliders();
    }

    private void OnDevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateVolumeSliders();
    }

    private void UpdateVolumeSliders()
    {
        var maxLevel = BluetoothImpl.Instance.DeviceSpec.MaximumAmbientVolume;
        
        if(BluetoothImpl.Instance.DeviceSpec.Supports(Features.AmbientExtraLoud) && IsAmbientExtraLoudEnabled)
            maxLevel += 1;
        
        MaximumLeftRightAmbientSoundVolume =
            BluetoothImpl.Instance.DeviceSpec.Supports(Features.AmbientCustomizeLegacy) ||
                (BluetoothImpl.Instance.DeviceSpec.Supports(Features.AmbientExtraLoud) && IsAmbientExtraLoudEnabled) ? 4 : 2;
        
        MaximumAmbientSoundVolume = maxLevel;
        
        // Workaround: Use the setter to retrigger the converter 
        AmbientSoundVolume = AmbientSoundVolume;
    }
    
    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(AmbientSoundVolume):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.AMBIENT_VOLUME, (byte)AmbientSoundVolume);
                break;
            case nameof(IsAmbientVoiceFocusEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.AMBIENT_VOICE_FOCUS, IsAmbientVoiceFocusEnabled);
                break;
            case nameof(IsAmbientExtraLoudEnabled):
                UpdateVolumeSliders();
                
                if (IsAmbientExtraLoudEnabled || AmbientSoundVolume >= 3)
                    AmbientSoundVolume = MaximumAmbientSoundVolume;
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.EXTRA_HIGH_AMBIENT, IsAmbientExtraLoudEnabled);
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.AMBIENT_VOLUME, (byte)AmbientSoundVolume);
                break;
            case nameof(IsAmbientCustomizationEnabled) or nameof(AmbientSoundTone) or 
                nameof(AmbientSoundVolumeLeft) or nameof(AmbientSoundVolumeRight):

                await BluetoothImpl.Instance.SendAsync(new CustomizeAmbientEncoder
                {
                    IsEnabled = IsAmbientCustomizationEnabled,
                    AmbientVolumeLeft = (byte)AmbientSoundVolumeLeft,
                    AmbientVolumeRight = (byte)AmbientSoundVolumeRight,
                    AmbientTone = (byte)AmbientSoundTone
                });
                break;
        }
    }
    
    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateDecoder e)
    {    
        using var suppressor = SuppressChangeNotifications();
        
        AmbientSoundVolume = e.AmbientSoundVolume;
        IsAmbientVoiceFocusEnabled = e.AmbientSoundMode == AmbientTypes.VoiceFocus;
        IsAmbientExtraLoudEnabled = e.ExtraHighAmbientEnabled;
        AmbientSoundTone = e.AmbientCustomSoundTone;
        AmbientSoundVolumeLeft = e.AmbientCustomVolumeLeft;
        AmbientSoundVolumeRight = e.AmbientCustomVolumeRight;
        IsAmbientCustomizationEnabled = e.AmbientCustomVolumeOn;
    }

    protected override void OnEventReceived(Event type, object? parameter)
    {
        switch (type)
        {
            case Event.AmbientVolumeUp:
                EventDispatcher.Instance.Dispatch(Event.SetNoiseControlState, NoiseControlModes.AmbientSound);
     
                if (IsAmbientCustomizationEnabled)
                {
                    if (AmbientSoundVolumeLeft < MaximumLeftRightAmbientSoundVolume)
                        AmbientSoundVolumeLeft += 1;
                    if (AmbientSoundVolumeRight < MaximumLeftRightAmbientSoundVolume)
                        AmbientSoundVolumeRight += 1;
                }
                else
                {
                    if (AmbientSoundVolume < MaximumAmbientSoundVolume)
                        AmbientSoundVolume += 1;
                }
                EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
                break;
            case Event.AmbientVolumeDown:
                if (IsAmbientCustomizationEnabled)
                {
                    if (AmbientSoundVolumeLeft <= 0 && AmbientSoundVolumeRight <= 0)
                    {
                        EventDispatcher.Instance.Dispatch(Event.SetNoiseControlState, NoiseControlModes.Off);
                    }
                    else
                    {
                        EventDispatcher.Instance.Dispatch(Event.SetNoiseControlState, NoiseControlModes.AmbientSound);
                        if (AmbientSoundVolumeLeft > 0)
                            AmbientSoundVolumeLeft -= 1;
                        if (AmbientSoundVolumeRight > 0)
                            AmbientSoundVolumeRight -= 1;
                    }
                }
                else
                {
                    if (AmbientSoundVolume <= 0)
                    {
                        EventDispatcher.Instance.Dispatch(Event.SetNoiseControlState, NoiseControlModes.Off);
                    }
                    else
                    {
                        EventDispatcher.Instance.Dispatch(Event.SetNoiseControlState, NoiseControlModes.AmbientSound);
                        AmbientSoundVolume -= 1;
                    }
                }
                EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
                break;
        }
    }

    [Reactive] public int AmbientSoundVolume { set; get; }
    [Reactive] public bool IsAmbientExtraLoudEnabled { set; get; }
    [Reactive] public bool IsAmbientVoiceFocusEnabled { set; get; }
    [Reactive] public bool IsAmbientCustomizationEnabled { set; get; }
    [Reactive] public int AmbientSoundVolumeLeft { set; get; }
    [Reactive] public int AmbientSoundVolumeRight { set; get; }
    [Reactive] public int AmbientSoundTone { set; get; }
    [Reactive] public int MaximumAmbientSoundVolume { set; get; }
    [Reactive] public int MaximumLeftRightAmbientSoundVolume { set; get; }
    
    public override string TitleKey => Keys.NcAsHeader;
}