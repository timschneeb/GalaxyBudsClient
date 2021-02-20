using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Connection.DynamicAssemblyName)]
namespace ThePBone.MprisClient
{
    [DBusInterface("org.mpris.MediaPlayer2")]
    interface IMediaPlayer2 : IDBusObject
    {
        Task RaiseAsync();
        Task QuitAsync();
        Task<T> GetAsync<T>(string prop);
        Task<MediaPlayer2Properties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class MediaPlayer2Properties
    {
        private bool _CanQuit = default(bool);
        public bool CanQuit
        {
            get
            {
                return _CanQuit;
            }

            set
            {
                _CanQuit = (value);
            }
        }

        private bool _Fullscreen = default(bool);
        public bool Fullscreen
        {
            get
            {
                return _Fullscreen;
            }

            set
            {
                _Fullscreen = (value);
            }
        }

        private bool _CanSetFullscreen = default(bool);
        public bool CanSetFullscreen
        {
            get
            {
                return _CanSetFullscreen;
            }

            set
            {
                _CanSetFullscreen = (value);
            }
        }

        private bool _CanRaise = default(bool);
        public bool CanRaise
        {
            get
            {
                return _CanRaise;
            }

            set
            {
                _CanRaise = (value);
            }
        }

        private bool _HasTrackList = default(bool);
        public bool HasTrackList
        {
            get
            {
                return _HasTrackList;
            }

            set
            {
                _HasTrackList = (value);
            }
        }

        private string? _Identity = default(string);
        public string? Identity
        {
            get
            {
                return _Identity;
            }

            set
            {
                _Identity = (value);
            }
        }

        private string? _DesktopEntry = default(string);
        public string? DesktopEntry
        {
            get
            {
                return _DesktopEntry;
            }

            set
            {
                _DesktopEntry = (value);
            }
        }

        private string[]? _SupportedUriSchemes = default(string[]);
        public string[]? SupportedUriSchemes
        {
            get
            {
                return _SupportedUriSchemes;
            }

            set
            {
                _SupportedUriSchemes = (value);
            }
        }

        private string[]? _SupportedMimeTypes = default(string[]);
        public string[]? SupportedMimeTypes
        {
            get
            {
                return _SupportedMimeTypes;
            }

            set
            {
                _SupportedMimeTypes = (value);
            }
        }
    }

    static class MediaPlayer2Extensions
    {
        public static Task<bool> GetCanQuitAsync(this IMediaPlayer2 o) => o.GetAsync<bool>("CanQuit");
        public static Task<bool> GetFullscreenAsync(this IMediaPlayer2 o) => o.GetAsync<bool>("Fullscreen");
        public static Task<bool> GetCanSetFullscreenAsync(this IMediaPlayer2 o) => o.GetAsync<bool>("CanSetFullscreen");
        public static Task<bool> GetCanRaiseAsync(this IMediaPlayer2 o) => o.GetAsync<bool>("CanRaise");
        public static Task<bool> GetHasTrackListAsync(this IMediaPlayer2 o) => o.GetAsync<bool>("HasTrackList");
        public static Task<string> GetIdentityAsync(this IMediaPlayer2 o) => o.GetAsync<string>("Identity");
        public static Task<string> GetDesktopEntryAsync(this IMediaPlayer2 o) => o.GetAsync<string>("DesktopEntry");
        public static Task<string[]> GetSupportedUriSchemesAsync(this IMediaPlayer2 o) => o.GetAsync<string[]>("SupportedUriSchemes");
        public static Task<string[]> GetSupportedMimeTypesAsync(this IMediaPlayer2 o) => o.GetAsync<string[]>("SupportedMimeTypes");
        public static Task SetFullscreenAsync(this IMediaPlayer2 o, bool val) => o.SetAsync("Fullscreen", val);
    }

    [DBusInterface("org.mpris.MediaPlayer2.Player")]
    public interface IPlayer : IDBusObject
    {
        Task NextAsync();
        Task PreviousAsync();
        Task PauseAsync();
        Task PlayPauseAsync();
        Task StopAsync();
        Task PlayAsync();
        Task SeekAsync(long Offset);
        Task SetPositionAsync(ObjectPath TrackId, long Position);
        Task OpenUriAsync(string Uri);
        Task<IDisposable> WatchSeekedAsync(Action<long> handler, Action<Exception>? onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<PlayerProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    public class PlayerProperties
    {
        private string? _PlaybackStatus = default(string);
        public string? PlaybackStatus
        {
            get
            {
                return _PlaybackStatus;
            }

            set
            {
                _PlaybackStatus = (value);
            }
        }

        private string? _LoopStatus = default(string);
        public string? LoopStatus
        {
            get
            {
                return _LoopStatus;
            }

            set
            {
                _LoopStatus = (value);
            }
        }

        private double _Rate = default(double);
        public double Rate
        {
            get
            {
                return _Rate;
            }

            set
            {
                _Rate = (value);
            }
        }

        private IDictionary<string, object>? _Metadata = default(IDictionary<string, object>);
        public IDictionary<string, object>? Metadata
        {
            get
            {
                return _Metadata;
            }

            set
            {
                _Metadata = (value);
            }
        }

        private double _Volume = default(double);
        public double Volume
        {
            get
            {
                return _Volume;
            }

            set
            {
                _Volume = (value);
            }
        }

        private long _Position = default(long);
        public long Position
        {
            get
            {
                return _Position;
            }

            set
            {
                _Position = (value);
            }
        }

        private double _MinimumRate = default(double);
        public double MinimumRate
        {
            get
            {
                return _MinimumRate;
            }

            set
            {
                _MinimumRate = (value);
            }
        }

        private double _MaximumRate = default(double);
        public double MaximumRate
        {
            get
            {
                return _MaximumRate;
            }

            set
            {
                _MaximumRate = (value);
            }
        }

        private bool _CanGoNext = default(bool);
        public bool CanGoNext
        {
            get
            {
                return _CanGoNext;
            }

            set
            {
                _CanGoNext = (value);
            }
        }

        private bool _CanGoPrevious = default(bool);
        public bool CanGoPrevious
        {
            get
            {
                return _CanGoPrevious;
            }

            set
            {
                _CanGoPrevious = (value);
            }
        }

        private bool _CanPlay = default(bool);
        public bool CanPlay
        {
            get
            {
                return _CanPlay;
            }

            set
            {
                _CanPlay = (value);
            }
        }

        private bool _CanPause = default(bool);
        public bool CanPause
        {
            get
            {
                return _CanPause;
            }

            set
            {
                _CanPause = (value);
            }
        }

        private bool _CanSeek = default(bool);
        public bool CanSeek
        {
            get
            {
                return _CanSeek;
            }

            set
            {
                _CanSeek = (value);
            }
        }

        private bool _CanControl = default(bool);
        public bool CanControl
        {
            get
            {
                return _CanControl;
            }

            set
            {
                _CanControl = (value);
            }
        }
    }

    static class PlayerExtensions
    {
        public static Task<string> GetPlaybackStatusAsync(this IPlayer o) => o.GetAsync<string>("PlaybackStatus");
        public static Task<string> GetLoopStatusAsync(this IPlayer o) => o.GetAsync<string>("LoopStatus");
        public static Task<double> GetRateAsync(this IPlayer o) => o.GetAsync<double>("Rate");
        public static Task<IDictionary<string, object>> GetMetadataAsync(this IPlayer o) => o.GetAsync<IDictionary<string, object>>("Metadata");
        public static Task<double> GetVolumeAsync(this IPlayer o) => o.GetAsync<double>("Volume");
        public static Task<long> GetPositionAsync(this IPlayer o) => o.GetAsync<long>("Position");
        public static Task<double> GetMinimumRateAsync(this IPlayer o) => o.GetAsync<double>("MinimumRate");
        public static Task<double> GetMaximumRateAsync(this IPlayer o) => o.GetAsync<double>("MaximumRate");
        public static Task<bool> GetCanGoNextAsync(this IPlayer o) => o.GetAsync<bool>("CanGoNext");
        public static Task<bool> GetCanGoPreviousAsync(this IPlayer o) => o.GetAsync<bool>("CanGoPrevious");
        public static Task<bool> GetCanPlayAsync(this IPlayer o) => o.GetAsync<bool>("CanPlay");
        public static Task<bool> GetCanPauseAsync(this IPlayer o) => o.GetAsync<bool>("CanPause");
        public static Task<bool> GetCanSeekAsync(this IPlayer o) => o.GetAsync<bool>("CanSeek");
        public static Task<bool> GetCanControlAsync(this IPlayer o) => o.GetAsync<bool>("CanControl");
        public static Task SetLoopStatusAsync(this IPlayer o, string val) => o.SetAsync("LoopStatus", val);
        public static Task SetRateAsync(this IPlayer o, double val) => o.SetAsync("Rate", val);
        public static Task SetVolumeAsync(this IPlayer o, double val) => o.SetAsync("Volume", val);
    }
}