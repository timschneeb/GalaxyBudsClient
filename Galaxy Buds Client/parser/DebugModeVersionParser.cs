using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;

namespace Galaxy_Buds_Client.parser
{
    class DebugModeVersionParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MSG_ID_DEBUGMODE_VERSION;

        readonly String[] _swMonth = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
        readonly String[] _swRelVer = { "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        readonly String[] _swVer = { "E", "U" };
        readonly String[] _swYear = { "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        public String LeftHardwareVersion { set; get; }
        public String RightHardwareVersion { set; get; }
        public String LeftSoftwareVersion { set; get; }
        public String RightSoftwareVersion { set; get; }
        public String LeftTouchSoftwareVersion { set; get; }
        public String RightTouchSoftwareVersion { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            int l1 = (msg.Payload[0] & 240) >> 4;
            int l2 = (msg.Payload[0] & 15);
            int r1 = (msg.Payload[1] & 240) >> 4;
            int r2 = (msg.Payload[1] & 15);

            LeftHardwareVersion = "rev" + l1.ToString("X") + "." + l2.ToString("X");
            RightHardwareVersion = "rev" + r1.ToString("X") + "." + r2.ToString("X");

            LeftSoftwareVersion = VersionDataToString(msg.Payload, 2, "L");
            RightSoftwareVersion = VersionDataToString(msg.Payload, 5, "R");

            LeftTouchSoftwareVersion = msg.Payload[8].ToString("x");
            RightTouchSoftwareVersion = msg.Payload[9].ToString("x");
        }

        private String VersionDataToString(byte[] payload, int startIndex, String side)
        {
            int swVarIndex = payload[startIndex];
            int swYearIndex = (payload[startIndex + 1] & 240) >> 4;
            int swMonthIndex = payload[startIndex + 1] & 15;
            byte swRelVerIndex = payload[startIndex + 2];

            String swRelVarString;
            if (swRelVerIndex <= 15)
            {
                swRelVarString = (swRelVerIndex & 255).ToString("X");
            }
            else
            {
                swRelVarString = _swRelVer[swRelVerIndex - 16];
            }

            return side + "170XX" + _swVer[swVarIndex] + "0A" + _swYear[swYearIndex] + _swMonth[swMonthIndex] +
                   swRelVarString;
        }

        public override Dictionary<String, String> ToStringMap()
        {
            Dictionary<String, String> map = new Dictionary<string, string>();
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "HandledType")
                    continue;

                map.Add(property.Name, property.GetValue(this).ToString());
            }

            return map;
        }
    }
}