using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;
using Serilog;
using Tmds.DBus.Protocol;

namespace GalaxyBudsClient.Platform.Linux;

public class NotificationListener : INotificationListener
{
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            if (value)
            {
                _cancelSource = new CancellationTokenSource();
                _loop = Task.Run(WatchBusAsync, _cancelSource.Token);
            }
            else
                _cancelSource.Cancel();
        }
    }

    public event EventHandler<Notification>? NotificationReceived;

    private CancellationTokenSource _cancelSource = new();
    private Task _loop;
    private bool _isEnabled;

    private record NotifyMessageBody
    {
        public required string AppName { get; init; }
        public required uint ReplacesId { get; init; }
        public required string AppIcon { get; init; }
        public required string Summary { get; init; }
        public required string Body { get; init; }
        public required string[] Actions { get; init; }
        public required Dictionary<string, object> Hints { get; init; }
        public required int ExpireTimeout { get; init; }
    }
    
    private NotifyMessageBody? ReadNotifyMessage(Reader reader)
    {
        try
        {
            return new NotifyMessageBody
            {
                AppName = reader.ReadString(),
                ReplacesId = reader.ReadUInt32(),
                AppIcon = reader.ReadString(),
                Summary = reader.ReadString(),
                Body = reader.ReadString(),
                Actions = reader.ReadArray<string>(),
                Hints = reader.ReadDictionary<string, object>(),
                ExpireTimeout = reader.ReadInt32()
            };
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to read Notify message body");
            return null;
        }
    }
    
    private async Task WatchBusAsync()
    {
        Log.Information("Notification listener started");
        
        string address = Address.Session ?? throw new ArgumentNullException(nameof(address));

        var rules = new List<MatchRule>
        {
            new()
            {
                Type = MessageType.MethodCall,
                Path = "/org/freedesktop/Notifications",
                Interface = "org.freedesktop.Notifications",
                Member = "Notify"
            }
        };

        var token = _cancelSource.Token;
        
        while (token.IsCancellationRequested == false)
        {
            await foreach (var dmsg in Connection.MonitorBusAsync(address, rules, token))
            {
                using var _ = dmsg;
                var msg = dmsg.Message;

                if (msg.MemberAsString != "Notify")
                    continue;

                var body = ReadNotifyMessage(msg.GetBodyReader());
                if (body is null)
                    continue;

                NotificationReceived?.Invoke(this,
                    new Notification(body.Summary, body.Body, body.AppName, body.AppName));
            }
        }

        Log.Information("Notification listener stopped");
    }
}