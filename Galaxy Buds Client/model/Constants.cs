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
        public enum Locale
        {
            [Description("English")]
            en,
            [Description("German")]
            de,
            [Description("Spanish")]
            es,
            [Description("Korean")]
            ko,
            [Description("Japanese")]
            ja,
            [Description("Russian")]
            ru,
            [Description("Ukrainian")]
            ua,
            [Description("Turkish")]
            tr,

            [Description("custom_language.xaml")]
            custom
        }
        public enum DarkMode
        {
            [LocalizedDescription("darkmode_disabled")]
            Light,
            [LocalizedDescription("darkmode_enabled")]
            Dark,
            [LocalizedDescription("darkmode_system")]
            System,

            Unset
        }

        public enum PopupPlacement
        {
            [LocalizedDescription("connpopup_placement_tl")]
            TopLeft,
            [LocalizedDescription("connpopup_placement_tc")]
            TopCenter,
            [LocalizedDescription("connpopup_placement_tr")]
            TopRight,
            [LocalizedDescription("connpopup_placement_bl")]
            BottomLeft,
            [LocalizedDescription("connpopup_placement_bc")]
            BottomCenter,
            [LocalizedDescription("connpopup_placement_br")]
            BottomRight,
        }

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
            [LocalizedDescription("placement_disconnected")]
            Disconnected = 0,
            [LocalizedDescription("placement_wearing")]
            Wearing = 1,
            [LocalizedDescription("placement_not_wearing")]
            Idle = 2,
            [LocalizedDescription("placement_in_case")]
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
                [LocalizedDescription("touchoption_voice")]
                VoiceAssistant,
                [LocalizedDescription("touchoption_quickambientsound")]
                QuickAmbientSound,
                [LocalizedDescription("touchoption_volume")]
                Volume,
                [LocalizedDescription("touchoption_ambientsound")]
                AmbientSound,
                [LocalizedDescription("touchoption_spotify")]
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
