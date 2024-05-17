using System;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;

namespace GalaxyBudsClient.Platform.Stubs;

public class DummyNotificationListener : INotificationListener
{
    public bool IsEnabled { get; set; }
    
#pragma warning disable CS0067
    public event EventHandler<Notification>? NotificationReceived;
#pragma warning restore CS0067
}