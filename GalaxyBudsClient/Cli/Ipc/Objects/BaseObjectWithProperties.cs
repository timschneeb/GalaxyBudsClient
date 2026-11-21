using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace GalaxyBudsClient.Cli.Ipc.Objects;

public abstract class BaseObjectWithProperties<T> where T : BaseProperties, new()
{
    private T Properties { get; } = new();

    public event Action<PropertyChanges>? OnPropertiesChanged;

    public Task<T> GetAllAsync()
    {
        return Task.FromResult(Properties);
    }
    
    public Task<object> GetAsync(string prop)
    {
        return Task.FromResult(Get(prop));
    }

    public Task SetAsync(string prop, object val)
    {
        Set(prop, val);
        return Task.CompletedTask;
    }
    
    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
        return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
    }
    
    protected object Get(string prop)
    {
        return Properties.Get(prop);
    }
    
    protected void Set(string prop, object val)
    {
        // D-Bus strings may not contain NUL; the bus disconnects any peer that sends one.
        // Fixed-width fields from the earbuds (e.g. the SKU of a bud that is asleep) arrive zero-filled.
        if (val is string str && str.Contains('\0'))
            val = str.Replace("\0", string.Empty);

        var changed = Properties.Set(prop, val);
        if (changed)
        {
            OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(prop, val));
        }
    }
}