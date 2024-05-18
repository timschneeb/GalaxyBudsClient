using System;
using System.ComponentModel;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Platform.Model;
using PortAudioSharp;
using Serilog;

namespace GalaxyBudsClient.Platform;

public class NotificationReaderManager
{
    public static bool PortAudioMissing { private set; get; }
    
    public NotificationReaderManager()
    {
        Settings.MainSettingsPropertyChanged += OnMainSettingsPropertyChanged;
        PlatformImpl.NotificationListener.NotificationReceived += OnNotificationReceived;
        try
        {
            PortAudio.Initialize();
        }
        catch (DllNotFoundException ex)
        {
            Log.Error(ex, "PortAudio library not found");
            PortAudioMissing = true;
        }
    }

    private void OnNotificationReceived(object? sender, Notification e)
    {
    }

    private static void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(SettingsData.ReadNotificationsAloud))
            PlatformImpl.NotificationListener.IsEnabled = Settings.Data.ReadNotificationsAloud;
    }
    
    #region Singleton
    private static readonly object Padlock = new();
    public static void Init()
    {
        _instance ??= new NotificationReaderManager();
    }
    
    private static NotificationReaderManager? _instance;
    public static NotificationReaderManager Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new NotificationReaderManager();
            }
        }
    }
    #endregion
}