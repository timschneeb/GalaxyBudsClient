using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GalaxyBudsClient.Model;
using Tmds.DBus;
using MainWindow = GalaxyBudsClient.Interface.MainWindow;

#pragma warning disable CS0414 // Field is assigned but its value is never used

// ReSharper disable InconsistentNaming

[assembly: InternalsVisibleTo(Connection.DynamicAssemblyName)]
namespace GalaxyBudsClient.Cli.Ipc.Objects;

[DBusInterface("me.timschneeberger.GalaxyBudsClient.Application")]
public interface IApplicationObject : IDBusObject
{
    Task ActivateAsync();
    Task ShowBatteryPopupAsync();
        
    Task<IDictionary<string, string>> ListActionsAsync();
    Task ExecuteActionAsync(string action);
        
    Task<object> GetAsync(string prop);
    Task<ApplicationProperties> GetAllAsync();
    Task SetAsync(string prop, object val);
    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
}

[Dictionary]
public class ApplicationProperties : BaseProperties
{
    [Property(Access = PropertyAccess.Read)]
    internal string _AppVersion = string.Empty;
}
    
public sealed class ApplicationObject : BaseObjectWithProperties<ApplicationProperties>, IApplicationObject
{
    public static readonly ObjectPath Path = new("/me/timschneeberger/galaxybudsclient");
    public ObjectPath ObjectPath => Path;
            
    public ApplicationObject()
    {
        Set(nameof(ApplicationProperties._AppVersion), Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "null");
    }

    public Task ShowBatteryPopupAsync()
    {
        EventDispatcher.Instance.Dispatch(Event.ShowBatteryPopup);
        return Task.CompletedTask;
    }

    public Task<IDictionary<string, string>> ListActionsAsync()
    {
        var values = EventExtensions.GetValues();
        var actions = new Dictionary<string, string>();
        foreach (var value in values)
        {
            var description = value.GetLocalizedDescription();
            if(description.Length > 0)
                actions.Add(value.ToString(), description);
        }

        return Task.FromResult(actions as IDictionary<string, string>);
    }

    public Task ExecuteActionAsync(string action)
    {
        if (EventExtensions.TryParse(action, out var result, true))
        {
            EventDispatcher.Instance.Dispatch(result);
            return Task.CompletedTask;
        }

        return Task.FromException(new ArgumentException("Invalid action"));
    }

    public Task ActivateAsync()
    {
        MainWindow.Instance.BringToFront();
        return Task.CompletedTask;
    }
}