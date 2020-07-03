using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.model.Constants;

namespace Galaxy_Buds_Client.message
{
    public static class SPPMessageBuilder
    {

        public static SPPMessage UpdateTime(long timestamp = -1, int offset = -1)
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

            return new SPPMessage(SPPMessage.MessageIds.MSG_ID_UPDATE_TIME,
                SPPMessage.MsgType.Request, payload);
        }
        public static SPPMessage SetManagerInfo(ClientDeviceType type = ClientDeviceType.Samsung, int androidSdkVersion = 29)
        {
            byte[] payload = new byte[3];
            payload[0] = 1;
            payload[1] = (byte)type;
            payload[2] = (byte)androidSdkVersion;
            return new SPPMessage(SPPMessage.MessageIds.MSG_ID_MANAGER_INFO,
                SPPMessage.MsgType.Request, payload);
        }
        /**
         * Incomplete
         */
        public static SPPMessage SetGameMode(int state = 2)
        {
            byte[] payload = new byte[1];
            payload[0] = (byte)state;
            return new SPPMessage(SPPMessage.MessageIds.MSG_ID_GAME_MODE,
                SPPMessage.MsgType.Request, payload);
        }
        /**
         * Buds only
         */
        public static SPPMessage SetSeamlessConnection(bool enable)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(!enable);
            return new SPPMessage(SPPMessage.MessageIds.MSG_ID_SET_SEAMLESS_CONNECTION,
                SPPMessage.MsgType.Request, payload);
        }
        /**
         * Buds+ only
         */
        public static SPPMessage SetAdjustSoundSyncEnabled(bool enable)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(enable);
            return new SPPMessage(SPPMessage.MessageIds.MSG_ID_ADJUST_SOUND_SYNC,
                SPPMessage.MsgType.Request, payload);
        }

        public static SPPMessage SetMainConnection(DeviceInv side)
        {
            byte[] payload = new byte[1];
            payload[0] = (byte)side;
            return new SPPMessage(SPPMessage.MessageIds.MSG_ID_MAIN_CHANGE,
                SPPMessage.MsgType.Request, payload);
        }
        public static SPPMessage SetEqualizer(bool enable, EqPreset preset, bool dolbyMode)
        {
            //Dolby mode has no effect on the Buds+
            if (BluetoothService.Instance.ActiveModel == Model.Buds)
            {
                int rawPreset = (int)preset;
                if (!dolbyMode)
                    rawPreset += 5;

                byte[] payload = new byte[2];
                payload[0] = Convert.ToByte(enable);
                payload[1] = (byte)rawPreset;
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_EQUALIZER,
                    SPPMessage.MsgType.Request, payload);
            }
            else
            {
                byte[] payload = new byte[1];
                payload[0] = !enable ? (byte) 0 : Convert.ToByte(preset + 1);
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_EQUALIZER,
                    SPPMessage.MsgType.Request, payload);
            }
        }

        public static SPPMessage FactoryReset()
        {
            return new SPPMessage(SPPMessage.MessageIds.MSG_ID_RESET,
                SPPMessage.MsgType.Request, new byte[0]);
        }

        public static class Touch
        {
            public static SPPMessage Lock(bool enable)
            {
                byte[] payload = new byte[1];
                payload[0] = Convert.ToByte(enable);
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_LOCK_TOUCHPAD,
                    SPPMessage.MsgType.Request, payload);
            }
            public static SPPMessage SetOptions(TouchOption.Universal left, TouchOption.Universal right)
            {
                byte[] payload = new byte[2];
                payload[0] = TouchOption.ToRawByte(left);
                payload[1] = TouchOption.ToRawByte(right);
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_SET_TOUCHPAD_OPTION,
                    SPPMessage.MsgType.Request, payload);
            }
            /**
             * Buds+ only
             */
            public static SPPMessage SetOutsideDoubleTapEnabled(bool enable)
            {
                byte[] payload = new byte[1];
                payload[0] = Convert.ToByte(enable);
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_OUTSIDE_DOUBLE_TAP,
                    SPPMessage.MsgType.Request, payload);
            }
        }

        public static class Ambient
        {
            public static SPPMessage SetEnabled(bool enable)
            {
                byte[] payload = new byte[1];
                payload[0] = Convert.ToByte(enable);
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_SET_AMBIENT_MODE,
                    SPPMessage.MsgType.Request, payload);
            }
            public static SPPMessage SetVolume(int volume)
            {
                byte[] payload = new byte[1];
                payload[0] = (byte)volume;
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_AMBIENT_VOLUME,
                    SPPMessage.MsgType.Request, payload);
            }
            /**
             * Buds only
             */
            public static SPPMessage SetType(AmbientType type)
            {
                byte[] payload = new byte[1];
                payload[0] = (byte)type;
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_AMBIENT_VOICE_FOCUS,
                    SPPMessage.MsgType.Request, payload);
            }
            /**
             * Buds+ only
             */
            public static SPPMessage SetExtraHighVolumeEnabled(bool enable)
            {
                byte[] payload = new byte[1];
                payload[0] = Convert.ToByte(enable);
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_EXTRA_HIGH_AMBIENT,
                    SPPMessage.MsgType.Request, payload);
            }
            /**
             * Buds+ only
             */
            public static SPPMessage SetSidetoneEnabled(bool enable)
            {
                byte[] payload = new byte[1];
                payload[0] = Convert.ToByte(enable);
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_SET_SIDETONE,
                    SPPMessage.MsgType.Request, payload);
            }
        }

        public static class FindMyGear
        {
            public static SPPMessage Start()
            {
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_FIND_MY_EARBUDS_START,
                    SPPMessage.MsgType.Request, new byte[0]);
            }
            public static SPPMessage Stop()
            {
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_FIND_MY_EARBUDS_STOP,
                    SPPMessage.MsgType.Request, new byte[0]);
            }
            public static SPPMessage MuteEarbud(bool leftMuted, bool rightMuted)
            {
                byte[] payload = new byte[2];
                payload[0] = Convert.ToByte(leftMuted);
                payload[1] = Convert.ToByte(rightMuted);
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_MUTE_EARBUD,
                    SPPMessage.MsgType.Request, payload);
            }
        }

        public static class Info
        {
            public static SPPMessage GetSerialNumber()
            {
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_DEBUG_SERIAL_NUMBER,
                    SPPMessage.MsgType.Request, new byte[0]);
            }
            public static SPPMessage GetAllData()
            {
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_DEBUG_GET_ALL_DATA,
                    SPPMessage.MsgType.Request, new byte[0]);
            }
            public static SPPMessage GetBuildString()
            {
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_DEBUG_BUILD_INFO,
                    SPPMessage.MsgType.Request, new byte[0]);
            }
            public static SPPMessage RunSelfTest()
            {
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_SELF_TEST,
                    SPPMessage.MsgType.Request, new byte[0]);
            }
            /**
             * Buds only
             */
            public static SPPMessage GetBatteryType()
            {
                return new SPPMessage(SPPMessage.MessageIds.MSG_ID_BATTERY_TYPE,
                    SPPMessage.MsgType.Request, new byte[0]);
            }
        }

    }
}
