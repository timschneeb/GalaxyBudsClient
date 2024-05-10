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
        for (var i13 = 0; i13 < 6; i13++)
        {
            if (i13 != 0)
            {
                sb.Append(':');
            }
            sb.Append(((payload[i13 + startIndex] & 240) >> 4).ToString("X"));
            sb.Append((payload[i13 + startIndex] & 15).ToString("X"));
        }
        return sb.ToString();
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