using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy_Buds_Client.model
{
    namespace Constants
    {
        public enum Model
        {
            NULL = 0,
            Buds = 1,
            BudsPlus = 2
        }

        public enum Color
        {
            Blue = 258,
            Pink = 259,
            Black = 260,
            White = 261,
            Thom_Brown = 262,
            Red = 263,
            Deep_Blue = 264,
            Olympic = 265,
            Purple = 266
        }

        public enum PlacementStates
        {
            Disconnected = 0,
            Wearing = 1,
            Idle = 2,
            Case = 3,
            ClosedCase = 4
        }

        public enum WearStates
        {
            None = 0,
            R = 1,
            L = 16,
            Both = 17
        }
        public enum DeviceInv
        {
            L = 1,
            R = 0
        }
        public enum Devices
        {
            L = 0,
            R = 1
        }
        public enum SppRoleStates
        {
            Done = 0,
            Changing = 1,
            Unknown = 2
        }
        public class TouchOption
        {
            public enum Universal
            {
                [Description("Voice Assistant (Android only)")]
                VoiceAssistant,
                [Description("Quick Ambient Sound")]
                QuickAmbientSound,
                [Description("Volume")]
                Volume,
                [Description("Ambient Sound")]
                AmbientSound,
                [Description("Spotify (Android only)")]
                SpotifySpotOn,
                OtherL,
                OtherR
            }
            public enum OptionsBuds
            {
                VoiceAssistant = 0,
                QuickAmbientSound = 1,
                Volume = 2,
                AmbientSound = 3,
                SpotifySpotOn = 4,
                OtherL = 5,
                OtherR = 6
            }
            public enum OptionsBudsPlus
            {
                VoiceAssistant = 1,
                AmbientSound = 2,
                Volume = 3,
                SpotifySpotOn = 4,
                OtherL = 5,
                OtherR = 6
            }
            public static byte ToRawByte(Universal uOption)
            {
                if (BluetoothService.Instance.ActiveModel == Model.Buds)
                {
                    foreach (int i in Enum.GetValues(typeof(OptionsBuds)))
                    {
                        String name = Enum.GetName(typeof(OptionsBuds), i);
                        if (name == uOption.ToString())
                            return (byte)i;
                    }
                }
                else
                {
                    foreach (int i in Enum.GetValues(typeof(OptionsBudsPlus)))
                    {
                        String name = Enum.GetName(typeof(OptionsBudsPlus), i);
                        if (name == uOption.ToString())
                            return (byte)i;
                    }
                }

                Console.WriteLine("Warning: TouchOption not translatable");
                return 0;
            }
            public static Universal ToUniversal(int iOption)
            {
                if (BluetoothService.Instance.ActiveModel == Model.Buds)
                {
                    OptionsBuds opt = (OptionsBuds)iOption;
                    foreach (int i in Enum.GetValues(typeof(Universal)))
                    {
                        String name = Enum.GetName(typeof(Universal), i);
                        if (name == opt.ToString())
                            return (Universal)i;
                    }
                }
                else
                {
                    OptionsBudsPlus opt = (OptionsBudsPlus)iOption;
                    foreach (int i in Enum.GetValues(typeof(Universal)))
                    {
                        String name = Enum.GetName(typeof(Universal), i);
                        if (name == opt.ToString())
                            return (Universal)i;
                    }
                }

                Console.WriteLine("Warning: TouchOption not translatable");
                return 0;
            }
        }

        public enum ClientDeviceType
        {
            Samsung = 1,
            Other = 2
        }
        public enum EqPreset
        {
            BassBoost = 0,
            Soft = 1,
            Dynamic = 2,
            Clear = 3,
            TrebleBoost = 4
        }
        public enum AmbientType
        {
            Default = 0,
            VoiceFocus = 1
        }

    }
}
