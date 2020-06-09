using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;

namespace Galaxy_Buds_Client.parser
{
    class DebugGetAllDataParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MSG_ID_DEBUG_GET_ALL_DATA;
        
        readonly String[] _swMonth = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
        readonly String[] _swRelVer = { "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        readonly String[] _swVer = { "E", "U" };
        readonly String[] _swYear = { "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public String HardwareVersion { set; get; }
        public String SoftwareVersion { set; get; }
        public String LeftBluetoothAddress { set; get; }
        public String RightBluetoothAddress { set; get; }
        public String TouchSoftwareVersion { set; get; }
        public short LeftAcceleratorX { set; get; }
        public short LeftAcceleratorY { set; get; }
        public short LeftAcceleratorZ { set; get; }
        public short RightAcceleratorX { set; get; }
        public short RightAcceleratorY { set; get; }
        public short RightAcceleratorZ { set; get; }
        public ushort LeftProximity { set; get; }
        public ushort RightProximity { set; get; }
        [Postfix(Text = "°C")]
        public double LeftThermistor { set; get; }
        [Postfix(Text = "°C")]
        public double RightThermistor { set; get; }

        [Postfix(Text = "%")]
        public double LeftAdcSOC { set; get; }
        [Postfix(Text = "V")]
        public double LeftAdcVCell { set; get; }
        [Postfix(Text = "mA")]
        public double LeftAdcCurrent { set; get; }

        [Postfix(Text = "%")]
        public double RightAdcSOC { set; get; }
        [Postfix(Text = "V")]
        public double RightAdcVCell { set; get; }
        [Postfix(Text = "mA")]
        public double RightAdcCurrent { set; get; }
        
        public String LeftHall { set; get; }
        public String RightHall { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            int hw1 = (msg.Payload[0] & 240) >> 4;
            int hw2 = (msg.Payload[0] & 15);

            HardwareVersion = "rev" + hw1.ToString("X") + "." + hw2.ToString("X");
            SoftwareVersion = VersionDataToString(msg.Payload, 1, "R");
            TouchSoftwareVersion = msg.Payload[4].ToString("X");
            LeftBluetoothAddress = BytesToMacString(msg.Payload, 5);
            RightBluetoothAddress = BytesToMacString(msg.Payload, 11);

            LeftAcceleratorX = BitConverter.ToInt16(msg.Payload, 17);
            LeftAcceleratorY = BitConverter.ToInt16(msg.Payload, 19);
            LeftAcceleratorZ = BitConverter.ToInt16(msg.Payload, 21);
            RightAcceleratorX = BitConverter.ToInt16(msg.Payload, 23);
            RightAcceleratorY = BitConverter.ToInt16(msg.Payload, 25);
            RightAcceleratorZ = BitConverter.ToInt16(msg.Payload, 27);

            LeftProximity = BitConverter.ToUInt16(msg.Payload, 29);
            RightProximity = BitConverter.ToUInt16(msg.Payload, 31);

            LeftThermistor = BitConverter.ToDouble(msg.Payload, 33);
            RightThermistor = BitConverter.ToDouble(msg.Payload, 41);

            LeftAdcSOC = BitConverter.ToDouble(msg.Payload, 49);
            LeftAdcVCell = BitConverter.ToDouble(msg.Payload, 57);
            LeftAdcCurrent = BitConverter.ToDouble(msg.Payload, 65);
            RightAdcSOC = BitConverter.ToDouble(msg.Payload, 73);
            RightAdcVCell = BitConverter.ToDouble(msg.Payload, 81);
            RightAdcCurrent = BitConverter.ToDouble(msg.Payload, 89);

            LeftHall = msg.Payload[97].ToString("x") + " " + msg.Payload[98].ToString("x");
            RightHall = msg.Payload[99].ToString("x") + " " + msg.Payload[100].ToString("x");
        }
        private String BytesToMacString(byte[] payload, int startIndex)
        {
            StringBuilder sb = new StringBuilder();
            for (int i13 = 0; i13 < 6; i13++)
            {
                if (i13 != 0)
                {
                    sb.Append(":");
                }
                sb.Append(((payload[i13 + startIndex] & 240) >> 4).ToString("X"));
                sb.Append((payload[i13 + startIndex] & 15).ToString("X"));
            }
            return sb.ToString();
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

                var customAttributes = (PostfixAttribute[])property.GetCustomAttributes(typeof(PostfixAttribute), true);
                if (customAttributes.Length > 0)
                {
                    map.Add(property.Name, property.GetValue(this).ToString() + customAttributes[0].Text);
                }
                else
                {
                    map.Add(property.Name, property.GetValue(this).ToString());
                }
            }

            return map;
        }
    }
}