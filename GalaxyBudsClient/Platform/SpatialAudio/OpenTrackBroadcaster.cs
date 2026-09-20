using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

/// <summary>
/// Broadcasts 6DOF head tracking data via standard UDP protocol for OpenTrack / FreeTrack (port 4242).
/// Enables head tracking in flight simulators, racing games, and space combat games.
/// </summary>
public class OpenTrackBroadcaster : IDisposable
{
    private UdpClient? _udpClient;
    private IPEndPoint? _endpoint;
    private bool _isEnabled;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value) return;
            _isEnabled = value;
            if (_isEnabled)
            {
                InitSocket();
                SpatialAudioService.Instance.OrientationUpdated += OnOrientationUpdated;
            }
            else
            {
                SpatialAudioService.Instance.OrientationUpdated -= OnOrientationUpdated;
                CloseSocket();
            }
        }
    }

    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 4242;

    private void InitSocket()
    {
        try
        {
            _endpoint = new IPEndPoint(IPAddress.Parse(Host), Port);
            _udpClient = new UdpClient();
            Log.Information("OpenTrackBroadcaster: Started broadcasting to {Host}:{Port}", Host, Port);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "OpenTrackBroadcaster: Failed to initialize UDP client");
            _isEnabled = false;
        }
    }

    private void CloseSocket()
    {
        try
        {
            _udpClient?.Close();
            _udpClient?.Dispose();
            _udpClient = null;
            _endpoint = null;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "OpenTrackBroadcaster: Error while closing socket");
        }
    }

    private void OnOrientationUpdated(object? sender, SpatialOrientationEventArgs e)
    {
        if (!_isEnabled || _udpClient == null || _endpoint == null)
            return;

        try
        {
            // OpenTrack standard protocol expects 6 double precision floats (48 bytes) in little endian:
            // X (cm), Y (cm), Z (cm), Yaw (deg), Pitch (deg), Roll (deg)
            using var stream = new MemoryStream(48);
            using var writer = new BinaryWriter(stream);

            writer.Write(0.0); // X
            writer.Write(0.0); // Y
            writer.Write(0.0); // Z
            writer.Write((double)e.Yaw);
            writer.Write((double)e.Pitch);
            writer.Write((double)e.Roll);

            var packet = stream.ToArray();
            _udpClient.Send(packet, packet.Length, _endpoint);
        }
        catch (Exception ex)
        {
            Log.Verbose(ex, "OpenTrackBroadcaster: Failed sending packet");
        }
    }

    public void Dispose()
    {
        IsEnabled = false;
    }
}
