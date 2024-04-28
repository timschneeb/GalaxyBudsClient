using System;
using System.IO;

namespace GalaxyBudsClient.Utils.Extensions;

public static class DataExtensions
{
    /// <summary>
    /// Seeks the stream to the specified offset, and seeks back to the original position once the disposable has been disposed.
    /// </summary>
    public static IDisposable ScopedSeek(this Stream stream, long offset, SeekOrigin origin)
    {
        var prevPosition = stream.Position;
        stream.Seek(offset, origin);
        return Finally.Create(() => stream.Seek(prevPosition, SeekOrigin.Begin));
    }
}

public readonly struct Finally : IDisposable
{
    private readonly Action? _onDispose;

    public Finally(Action onDispose)
    {
        _ = onDispose ?? throw new ArgumentNullException(nameof(onDispose));

        this._onDispose = onDispose;
    }

    public static Finally Create(Action onDispose)
    {
        return new Finally(onDispose);
    }

    public void Dispose()
    {
        // Keep in mind that a struct can always be created using new() or default and
        // in that cases _onDispose is null!
        this._onDispose?.Invoke();
    }
}