using System;
using System.Threading.Tasks;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Message
{
    public static class MessageComposer
    {

        public static async Task UpdateTime(long timestamp = -1, int offset = -1)
        {
            if (timestamp < 0 | offset < 0)
            {
                TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                timestamp = (long)span.TotalMilliseconds;
                offset = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalMilliseconds;
            }

            byte[] payload = new byte[12];
            byte[] timestampRaw = BitConverter.GetBytes(timestamp);
            byte[] offsetRaw = BitConverter.GetBytes(offset);
            Array.Copy(timestampRaw, 0, payload, 0, 8);
            Array.Copy(payload, 8, offsetRaw, 0, 4);

            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.UPDATE_TIME, payload);
        }
        
        public static async Task SetManagerInfo(ClientDeviceType type = ClientDeviceType.Samsung, int androidSdkVersion = 29)
        {
            byte[] payload = new byte[3];
            payload[0] = 1;
            payload[1] = (byte)type;
            payload[2] = (byte)androidSdkVersion;
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MANAGER_INFO, payload);
        }
        
        public static async Task SetMainConnection(DeviceInv side)
        {
            byte[] payload = new byte[1];
            payload[0] = (byte)side;
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MAIN_CHANGE, payload);
        }
        
        public static async Task SetEqualizer(bool enable, EqPreset preset, bool dolbyMode)
        {
            // Dolby mode has no effect on the Buds+/Live/Pro
            if (BluetoothImpl.Instance.ActiveModel == Models.Buds)
            {
                int rawPreset = (int)preset;
                if (!dolbyMode)
                    rawPreset += 5;

                byte[] payload = new byte[2];
                payload[0] = Convert.ToByte(enable);
                payload[1] = (byte)rawPreset;
                await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.EQUALIZER, payload);
            }
            else
            {
                byte[] payload = new byte[1];
                payload[0] = !enable ? (byte) 0 : Convert.ToByte(preset + 1);
                await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.EQUALIZER, payload);
            }
        }
        

        public static class Touch
        {
            public static async Task SetOptions(TouchOptions left, TouchOptions right)
            {
                byte[] payload = new byte[2];
                if (left == TouchOptions.NoiseControl)
                {
                    await NoiseControl.SetTouchNoiseControls(true, true, false);
                }
                if (right == TouchOptions.NoiseControl)
                {
                    await NoiseControl.SetTouchNoiseControls(true, true, false);
                }

                payload[0] = BluetoothImpl.Instance.DeviceSpec.TouchMap.ToByte(left);
                payload[1] = BluetoothImpl.Instance.DeviceSpec.TouchMap.ToByte(right);
                await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_TOUCHPAD_OPTION, payload);
            }
        }
        
        public static class FindMyGear
        {
            public static async Task MuteEarbud(bool leftMuted, bool rightMuted)
            {
                byte[] payload = new byte[2];
                payload[0] = Convert.ToByte(leftMuted);
                payload[1] = Convert.ToByte(rightMuted);
                await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MUTE_EARBUD, payload);
            }
        }
        
        /* Buds Pro only */
        public static class NoiseControl
        {
            public static async Task SetMode(NoiseControlMode mode)
            {
                await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.NOISE_CONTROLS, (byte)mode);
            }

            public static async Task SetTouchNoiseControls(bool anc, bool ambient, bool off)
            {
                await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS,
                    new byte[]{Convert.ToByte(anc), Convert.ToByte(ambient), Convert.ToByte(off)});
            }
        }
    }
}
