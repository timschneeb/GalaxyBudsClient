using System;
using System.Threading.Tasks;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Message;

public static class MessageComposer
{
    public static async Task UpdateTime(long timestamp = -1, int offset = -1)
    {
        if ((timestamp < 0) | (offset < 0))
        {
            var span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            timestamp = (long)span.TotalMilliseconds;
            offset = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalMilliseconds;
        }

        var payload = new byte[12];
        var timestampRaw = BitConverter.GetBytes(timestamp);
        var offsetRaw = BitConverter.GetBytes(offset);
        Array.Copy(timestampRaw, 0, payload, 0, 8);
        Array.Copy(payload, 8, offsetRaw, 0, 4);

        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.UPDATE_TIME, payload);
    }
        
    public static async Task SetManagerInfo(ClientDeviceTypes types = ClientDeviceTypes.Samsung, int androidSdkVersion = 29)
    {
        var payload = new byte[3];
        payload[0] = 1;
        payload[1] = (byte)types;
        payload[2] = (byte)androidSdkVersion;
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.MANAGER_INFO, payload);
    }
        
    public static async Task SetMainConnection(DevicesInverted side)
    {
        var payload = new byte[1];
        payload[0] = (byte)side;
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.MAIN_CHANGE, payload);
    }
        
    public static async Task SetEqualizer(bool enable, EqPresets preset, bool dolbyMode)
    {
        // Dolby mode has no effect on the Buds+/Live/Pro
        if (BluetoothImpl.ActiveModel == Models.Buds)
        {
            var rawPreset = (int)preset;
            if (!dolbyMode)
                rawPreset += 5;

            var payload = new byte[2];
            payload[0] = Convert.ToByte(enable);
            payload[1] = (byte)rawPreset;
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.EQUALIZER, payload);
        }
        else
        {
            var payload = new byte[1];
            payload[0] = !enable ? (byte) 0 : Convert.ToByte(preset + 1);
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.EQUALIZER, payload);
        }
        EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
    }
    
    public static async Task SetNoiseControlMode(NoiseControlModes mode)
    {
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.NOISE_CONTROLS, (byte)mode);
        EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
    }
}