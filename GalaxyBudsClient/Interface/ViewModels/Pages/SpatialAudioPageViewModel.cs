using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Platform.SpatialAudio;
using System.Threading.Tasks;
using ReactiveUI.SourceGenerators;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public partial class SpatialAudioPageViewModel : MainPageViewModelBase, IDisposable
{
    private readonly OscSpatialBroadcaster _oscBroadcaster = new();
    private readonly OpenTrackBroadcaster _openTrackBroadcaster = new();

    [Reactive] private bool _isTrackingEnabled;
    [Reactive] private double _yaw;
    [Reactive] private double _pitch;
    [Reactive] private double _roll;
    [Reactive] private bool _isOscBroadcasting;
    [Reactive] private bool _isOpenTrackBroadcasting;
    [Reactive] private bool _isDemoPlaying;
    [Reactive] private string _statusText = Strings.SpatialTrackingInactive;
    [Reactive] private int _speakerAngle = 30;
    [Reactive] private int _ambiencePercent = 12;
    [Reactive] private bool _isBlackHoleInstalled;
    [Reactive] private bool _isBlackHoleLoaded;
    [Reactive] private bool _isSystemAudioActive;
    [Reactive] private bool _isInstallingBlackHole;

    public bool IsMacOs => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);
    public bool IsWindows => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

    public SpatialAudioPageViewModel()
    {
        SpatialAudioService.Instance.OrientationUpdated += OnOrientationUpdated;
        SpatialAudioService.Instance.PropertyChanged += OnServicePropertyChanged;
        SpatialMediaPlayer.Instance.PropertyChanged += OnMediaPlayerPropertyChanged;
        SpatialSystemAudioStreamer.Instance.PropertyChanged += OnSystemAudioPropertyChanged;
        PropertyChanged += OnSelfPropertyChanged;

        IsTrackingEnabled = SpatialAudioService.Instance.IsActive;
        IsDemoPlaying = SpatialMediaPlayer.Instance.IsPlaying;
        IsSystemAudioActive = SpatialSystemAudioStreamer.Instance.IsActive;
        SpeakerAngle = (int)Math.Round(SpatialMediaPlayer.Instance.VirtualSpeakerAngle);
        AmbiencePercent = (int)Math.Round(SpatialMediaPlayer.Instance.AmbienceAmount * 100.0);
        UpdateStatusText();
        RefreshBlackHoleStatus();
    }

    private void OnSystemAudioPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SpatialSystemAudioStreamer.IsActive))
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsSystemAudioActive = SpatialSystemAudioStreamer.Instance.IsActive;
            });
        }
    }

    private void OnMediaPlayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SpatialMediaPlayer.IsPlaying))
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsDemoPlaying = SpatialMediaPlayer.Instance.IsPlaying;
            });
        }
    }

    private void OnServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SpatialAudioService.IsActive))
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsTrackingEnabled = SpatialAudioService.Instance.IsActive;
                UpdateStatusText();
            });
        }
    }

    private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsTrackingEnabled):
                if (IsTrackingEnabled && !SpatialAudioService.Instance.IsActive)
                {
                    SpatialAudioService.Instance.Start();
                }
                else if (!IsTrackingEnabled && SpatialAudioService.Instance.IsActive)
                {
                    SpatialAudioService.Instance.Stop();
                }
                UpdateStatusText();
                break;

            case nameof(IsOscBroadcasting):
                _oscBroadcaster.IsEnabled = IsOscBroadcasting;
                break;

            case nameof(IsOpenTrackBroadcasting):
                _openTrackBroadcaster.IsEnabled = IsOpenTrackBroadcasting;
                break;

            case nameof(SpeakerAngle):
                SpatialMediaPlayer.Instance.VirtualSpeakerAngle = (float)SpeakerAngle;
                SpatialSystemAudioStreamer.Instance.VirtualSpeakerAngle = (float)SpeakerAngle;
                Log.Debug("SpatialAudioPageViewModel: SpeakerAngle updated to {Angle}°", SpeakerAngle);
                break;

            case nameof(AmbiencePercent):
                SpatialMediaPlayer.Instance.AmbienceAmount = (float)(AmbiencePercent / 100.0);
                SpatialSystemAudioStreamer.Instance.AmbienceAmount = (float)(AmbiencePercent / 100.0);
                Log.Debug("SpatialAudioPageViewModel: AmbiencePercent updated to {Ambience}%", AmbiencePercent);
                break;
        }
    }

    private void OnOrientationUpdated(object? sender, SpatialOrientationEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Yaw = Math.Round(e.Yaw, 1);
            Pitch = Math.Round(e.Pitch, 1);
            Roll = Math.Round(e.Roll, 1);
        }, DispatcherPriority.Render);
    }

    public void Recenter()
    {
        SpatialAudioService.Instance.Recenter();
    }

    public void ToggleDemo()
    {
        if (SpatialMediaPlayer.Instance.IsPlaying)
        {
            SpatialMediaPlayer.Instance.Stop();
        }
        else
        {
            if (!SpatialAudioService.Instance.IsActive)
            {
                IsTrackingEnabled = true;
            }
            SpatialMediaPlayer.Instance.Play();
        }
    }

    private void UpdateStatusText()
    {
        StatusText = IsTrackingEnabled ? Strings.SpatialTrackingActive : Strings.SpatialTrackingInactive;
    }

    public void RefreshBlackHoleStatus()
    {
        if (!IsMacOs) return;
        IsBlackHoleInstalled = BlackHoleHelper.IsDriverInstalled;
        IsBlackHoleLoaded = BlackHoleHelper.IsLoadedInCoreAudio();
    }

    public async Task InstallBlackHoleAsync()
    {
        if (IsInstallingBlackHole) return;
        IsInstallingBlackHole = true;
        try
        {
            await BlackHoleHelper.InstallViaHomebrewAsync();
            RefreshBlackHoleStatus();
        }
        finally
        {
            IsInstallingBlackHole = false;
        }
    }

    public void RestartCoreAudio()
    {
        BlackHoleHelper.RestartCoreAudio();
        Task.Delay(1500).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(RefreshBlackHoleStatus);
        });
    }

    public void OpenAudioMidiSetup()
    {
        BlackHoleHelper.OpenAudioMidiSetup();
    }

    public void OpenSoundSettings()
    {
        BlackHoleHelper.OpenSoundSettings();
    }

    public void ToggleSystemAudio()
    {
        if (SpatialSystemAudioStreamer.Instance.IsActive)
        {
            SpatialSystemAudioStreamer.Instance.Stop();
        }
        else
        {
            if (!SpatialAudioService.Instance.IsActive)
            {
                IsTrackingEnabled = true;
            }
            SpatialSystemAudioStreamer.Instance.Start();
        }
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();
        RefreshBlackHoleStatus();
        if (SpatialAudioService.Instance.IsSupported && !SpatialAudioService.Instance.IsActive)
        {
            IsTrackingEnabled = true;
        }
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();
        if (SpatialMediaPlayer.Instance.IsPlaying)
        {
            SpatialMediaPlayer.Instance.Stop();
        }
    }

    public override Control CreateView() => new SpatialAudioPage { DataContext = this };

    public override string TitleKey => Keys.PageSpatialAudio;
    public override Symbol IconKey => Symbol.SoundWaveCircle;
    public override bool ShowsInFooter => false;

    public void Dispose()
    {
        SpatialAudioService.Instance.OrientationUpdated -= OnOrientationUpdated;
        SpatialAudioService.Instance.PropertyChanged -= OnServicePropertyChanged;
        SpatialMediaPlayer.Instance.PropertyChanged -= OnMediaPlayerPropertyChanged;
        SpatialSystemAudioStreamer.Instance.PropertyChanged -= OnSystemAudioPropertyChanged;
        SpatialMediaPlayer.Instance.Stop();
        SpatialSystemAudioStreamer.Instance.Stop();
        _oscBroadcaster.Dispose();
        _openTrackBroadcaster.Dispose();
        GC.SuppressFinalize(this);
    }
}
