using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

/// <summary>
/// Broadcasts spatial head tracking data via Open Sound Control (OSC) protocol over UDP.
/// Compatible with digital audio workstations (DAWs) like Reaper, Logic Pro, and 3D spatial audio plugins (IEM, Dolby Atmos).
/// </summary>
public class OscSpatialBroadcaster : IDisposable
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
    public int Port { get; set; } = 9000;

    private void InitSocket()
    {
        try
        {
            _endpoint = new IPEndPoint(IPAddress.Parse(Host), Port);
            _udpClient = new UdpClient();
            Log.Information("OscSpatialBroadcaster: Started broadcasting to {Host}:{Port}", Host, Port);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "OscSpatialBroadcaster: Failed to initialize UDP client");
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
            Log.Warning(ex, "OscSpatialBroadcaster: Error while closing socket");
        }
    }

    private void OnOrientationUpdated(object? sender, SpatialOrientationEventArgs e)
    {
        if (!_isEnabled || _udpClient == null || _endpoint == null)
            return;

        try
        {
            // Send /spatial/ypr (yaw, pitch, roll in degrees)
            var packet = CreateOscMessage("/spatial/ypr", ",fff", e.Yaw, e.Pitch, e.Roll);
            _udpClient.Send(packet, packet.Length, _endpoint);

            // Send /spatial/quaternion (x, y, z, w)
            var q = e.RelativeQuaternion;
            var quatPacket = CreateOscMessage("/spatial/quaternion", ",ffff", q.X, q.Y, q.Z, q.W);
            _udpClient.Send(quatPacket, quatPacket.Length, _endpoint);
        }
        catch (Exception ex)
        {
            Log.Verbose(ex, "OscSpatialBroadcaster: Failed sending packet");
        }
    }

    private static byte[] CreateOscMessage(string address, string typeTag, params float[] values)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Address padded to 4 bytes
        WritePaddedString(writer, address);

        // Type tag padded to 4 bytes
        WritePaddedString(writer, typeTag);

        // Floats in Big-Endian (network order)
        foreach (var val in values)
        {
            var bytes = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            writer.Write(bytes);
        }

        return stream.ToArray();
    }

    private static void WritePaddedString(BinaryWriter writer, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        writer.Write(bytes);
        writer.Write((byte)0); // Null terminator

        var pad = 4 - ((bytes.Length + 1) % 4);
        if (pad < 4)
        {
            for (var i = 0; i < pad; i++)
                writer.Write((byte)0);
        }
    }

    public void Dispose()
    {
        IsEnabled = false;
    }
}
