using System;
using System.Numerics;
using System.Timers;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Message
{
    public class SpatialSensorManager : IDisposable
    {
        public event EventHandler<Quaternion>? NewQuaternionReceived; 
        
        private readonly Timer _keepAliveTimer = new Timer(2000);
        public SpatialSensorManager()
        {
            _keepAliveTimer.Elapsed += KeepAliveOnElapsed;
            _keepAliveTimer.AutoReset = true;
            
            SPPMessageHandler.Instance.AnyMessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object? sender, BaseMessageParser? e)
        {
            if (e is SpatialAudioControlParser control)
            {
                switch (control.ResultCode)
                {
                    case SpatialAudioControl.AttachSuccess:
                        Log.Debug("SpatialSensorManager: Attach successful");
                        break;  
                    case SpatialAudioControl.DetachSuccess:
                        Log.Debug("SpatialSensorManager: Detach successful");
                        break;
                }
            }
            else if (e is SpatialAudioDataParser data)
            {
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
            }
        }
        
        public void Dispose()
        {
            SPPMessageHandler.Instance.AnyMessageReceived -= OnMessageReceived;
            Detach();
        }

        public async void Attach()
        {
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_SPATIAL_AUDIO, 1);
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SPATIAL_AUDIO_CONTROL, (byte)SpatialAudioControl.Attach);
            _keepAliveTimer.Start();
        }

        public async void Detach()
        {
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SPATIAL_AUDIO_CONTROL, (byte)SpatialAudioControl.Detach);
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_SPATIAL_AUDIO, 0);
            _keepAliveTimer.Stop();
        }
        
        private static async void KeepAliveOnElapsed(object? sender, ElapsedEventArgs e)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SPATIAL_AUDIO_CONTROL, (byte)SpatialAudioControl.KeepAlive);
        }
    }
}