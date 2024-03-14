using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Constants;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class NoiseControlPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new NoiseControlPage();
    
    public NoiseControlPageViewModel()
    {
        
    }
    
    [Reactive] public bool IsAmbientSoundEnabled { set; get; }
    [Reactive] public int AmbientSoundVolume { set; get; }
    [Reactive] public int MaximumAmbientSoundVolume { set; get; }
    [Reactive] public bool IsAmbientExtraLoudEnabled { set; get; }
    [Reactive] public bool IsAmbientVoiceFocusEnabled { set; get; }
    [Reactive] public bool IsAncEnabled { set; get; }
    [Reactive] public bool IsAncLevelHigh { set; get; }
    [Reactive] public bool IsAncWithOneEarbudAllowed { set; get; }
    [Reactive] public bool IsVoiceDetectEnabled { set; get; }
    [Reactive] public VoiceDetectTimeouts VoiceDetectTimeout { set; get; }

    public override string TitleKey => "mainpage_noise";
    public override Symbol IconKey => Symbol.HeadphonesSoundWave;
    public override bool ShowsInFooter => false;
}


