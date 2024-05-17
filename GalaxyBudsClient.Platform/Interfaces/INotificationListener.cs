using System;
using GalaxyBudsClient.Platform.Model;

namespace GalaxyBudsClient.Platform.Interfaces;

public interface INotificationListener
{
    public bool IsEnabled { set; get; }
    public event EventHandler<Notification>? NotificationReceived;
}