using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
    
    public static string BytesToMacString(this IReadOnlyList<byte> payload, int startIndex = 0)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 6; i++)
        {
            if (i != 0)
            {
                sb.Append(':');
            }
            sb.Append(((payload[i + startIndex] & 240) >> 4).ToString("X"));
            sb.Append((payload[i + startIndex] & 15).ToString("X"));
        }
        return sb.ToString();
    }
}

public readonly struct Finally : IDisposable
{
    private readonly Action? _onDispose;

    public Finally(Action onDispose)
    {
        ArgumentNullException.ThrowIfNull(onDispose);
        _onDispose = onDispose;
    }

    public static Finally Create(Action onDispose)
    {
        return new Finally(onDispose);
    }

    public void Dispose()
    {
        // Keep in mind that a struct can always be created using new() or default and
        // in that cases _onDispose is null!
        _onDispose?.Invoke();
    }
}