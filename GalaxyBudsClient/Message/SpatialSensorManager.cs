using System;
using System.Numerics;
using System.Timers;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Message;

/*
 * Message receiver for the spatial head-tracking sensor of the earbuds
 *
 * This class is a reverse-engineered implementation of Samsung's client-side head-tracking receiver.
 * It can collect the correct head orientation of the listener as a 4D quaternion in real-time.
 */
public class SpatialSensorManager : IDisposable
{
    // Event handler, called when new tracking data is available
    public event EventHandler<Quaternion>? NewQuaternionReceived; 
        
    // The earbuds expect a keep-alive package to be sent periodically.
    private readonly Timer _keepAliveTimer = new(2000);

    public SpatialSensorManager()
    {
        // Setup keep-alive message trigger
        _keepAliveTimer.Elapsed += KeepAliveOnElapsed;
        _keepAliveTimer.AutoReset = true;
            
        // Register callback for incoming Bluetooth SPP messages
        SppMessageReceiver.Instance.AnyMessageDecoded += OnMessageDecoded;
    }

    private void OnMessageDecoded(object? sender, BaseMessageDecoder? e)
    {
        // Filter incoming SPP messages for Spatial Audio packets
        switch (e)
        {
            case SpatialAudioControlDecoder control:
                // Control packet received
                switch (control.ResultCode)
                {
                    case SpatialAudioControl.AttachSuccess:
                        Log.Debug("SpatialSensorManager: Attach successful");
                        break;  
                    case SpatialAudioControl.DetachSuccess:
                        Log.Debug("SpatialSensorManager: Detach successful");
                        break;
                }

                break;
            case SpatialAudioDataDecoder data:
                // Data packet received
                switch (data.EventId)
                {
                    case SpatialAudioData.BudGrv:
                        if (data.GrvFloatArray == null)
                        {
                            break;
                        }
                        NewQuaternionReceived?.Invoke(this,
                            new Quaternion(data.GrvFloatArray[0], data.GrvFloatArray[1],
                                data.GrvFloatArray[2], data.GrvFloatArray[3]));
                        break;
                    case SpatialAudioData.BudGyrocal:
                        break;
                    case SpatialAudioData.BudSensorStuck:
                        break;
                    case SpatialAudioData.WearOff:
                        break;
                    case SpatialAudioData.WearOn:
                        break;
                }

                break;
        }
    }
        
    public void Dispose()
    {
        SppMessageReceiver.Instance.AnyMessageDecoded -= OnMessageDecoded;
        Detach();
    }

    // Request earbuds to enter head-tracking mode
    public async void Attach()
    {
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_SPATIAL_AUDIO, 1);
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SPATIAL_AUDIO_CONTROL, (byte)SpatialAudioControl.Attach);
        _keepAliveTimer.Start();
    }

    // Request earbuds to exit head-tracking mode
    public async void Detach()
    {
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SPATIAL_AUDIO_CONTROL, (byte)SpatialAudioControl.Detach);
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_SPATIAL_AUDIO, 0);
        _keepAliveTimer.Stop();
    }
        
    private static async void KeepAliveOnElapsed(object? sender, ElapsedEventArgs e)
    {
        // Send keep-alive packet
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SPATIAL_AUDIO_CONTROL, (byte)SpatialAudioControl.KeepAlive);
    }
}
