using System;
using System.Collections.Generic;
using System.Text;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder
{
    public class DebugGetAllDataParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.DEBUG_GET_ALL_DATA;

        readonly string[] _swMonth = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L"];
        readonly string[] _swRelVer = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
        ];
        readonly string[] _swVer = ["E", "U"];
        readonly string[] _swYear = ["O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"];
        public string? HardwareVersion { set; get; }
        public string? SoftwareVersion { set; get; }
        public string? LeftBluetoothAddress { set; get; }
        public string? RightBluetoothAddress { set; get; }
        public string? TouchSoftwareVersion { set; get; }
        public short LeftAcceleratorX { set; get; }
        public short LeftAcceleratorY { set; get; }
        public short LeftAcceleratorZ { set; get; }
        public short RightAcceleratorX { set; get; }
        public short RightAcceleratorY { set; get; }
        public short RightAcceleratorZ { set; get; }
        public short LeftProximity { set; get; }
        public short RightProximity { set; get; }
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

        public string? LeftHall { set; get; }
        public string? RightHall { set; get; }

        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short LeftProximityOffset { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short RightProximityOffset { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public byte MsgVersion { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short LeftTspAbs { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short RightTspAbs { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short LeftTspDiff0 { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short LeftTspDiff1 { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short LeftTspDiff2 { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short RightTspDiff0 { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short RightTspDiff1 { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short RightTspDiff2 { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short LeftPR { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short RightPR { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short LeftWD { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public short RightWD { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public byte LeftCradleFlag { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public byte RightCradleFlag { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public byte LeftCradleBatt { set; get; }
        [Device([Models.BudsPlus, Models.BudsLive, Models.BudsPro])]
        public byte RightCradleBatt { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            if (ActiveModel == Models.Buds)
            {

                var hw1 = (msg.Payload[0] & 240) >> 4;
                var hw2 = (msg.Payload[0] & 15);

                HardwareVersion = "rev" + hw1.ToString("X") + "." + hw2.ToString("X");
                SoftwareVersion = VersionDataToString(msg.Payload, 1);
                TouchSoftwareVersion = $"0x{msg.Payload[4]:X}";
                LeftBluetoothAddress = BytesToMacString(msg.Payload, 5);
                RightBluetoothAddress = BytesToMacString(msg.Payload, 11);

                LeftAcceleratorX = BitConverter.ToInt16(msg.Payload, 17);
                LeftAcceleratorY = BitConverter.ToInt16(msg.Payload, 19);
                LeftAcceleratorZ = BitConverter.ToInt16(msg.Payload, 21);
                RightAcceleratorX = BitConverter.ToInt16(msg.Payload, 23);
                RightAcceleratorY = BitConverter.ToInt16(msg.Payload, 25);
                RightAcceleratorZ = BitConverter.ToInt16(msg.Payload, 27);

                LeftProximity = BitConverter.ToInt16(msg.Payload, 29);
                RightProximity = BitConverter.ToInt16(msg.Payload, 31);

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
            else
            {
                var hw1 = (msg.Payload[1] & 240) >> 4;
                var hw2 = (msg.Payload[1] & 15);

                MsgVersion = msg.Payload[0];

                HardwareVersion = "rev" + hw1.ToString("X") + "." + hw2.ToString("X");
                SoftwareVersion = VersionDataToString(msg.Payload, 2);
                TouchSoftwareVersion = $"0x{msg.Payload[5]:X}";
                LeftBluetoothAddress = BytesToMacString(msg.Payload, 6);
                RightBluetoothAddress = BytesToMacString(msg.Payload, 12);

                LeftAcceleratorX = BitConverter.ToInt16(msg.Payload, 18);
                LeftAcceleratorY = BitConverter.ToInt16(msg.Payload, 20);
                LeftAcceleratorZ = BitConverter.ToInt16(msg.Payload, 22);
                RightAcceleratorX = BitConverter.ToInt16(msg.Payload, 24);
                RightAcceleratorY = BitConverter.ToInt16(msg.Payload, 26);
                RightAcceleratorZ = BitConverter.ToInt16(msg.Payload, 28);

                LeftProximity = BitConverter.ToInt16(msg.Payload, 30);
                LeftProximityOffset = BitConverter.ToInt16(msg.Payload, 32);
                RightProximity = BitConverter.ToInt16(msg.Payload, 34);
                RightProximityOffset = BitConverter.ToInt16(msg.Payload, 36);

                LeftThermistor = BitConverter.ToInt16(msg.Payload, 38) * 0.1d;
                RightThermistor = BitConverter.ToInt16(msg.Payload, 40) * 0.1d;

                LeftAdcSOC = BitConverter.ToInt16(msg.Payload, 42);
                LeftAdcVCell = BitConverter.ToInt16(msg.Payload, 44) * 0.01d;
                LeftAdcCurrent = BitConverter.ToInt16(msg.Payload, 46) * -0.1d; //1.0E-4d;
                RightAdcSOC = BitConverter.ToInt16(msg.Payload, 48);
                RightAdcVCell = BitConverter.ToInt16(msg.Payload, 50) * 0.01d;
                RightAdcCurrent = BitConverter.ToInt16(msg.Payload, 52) * -0.1d; //1.0E-4d;

                LeftTspAbs = BitConverter.ToInt16(msg.Payload, 54);
                RightTspAbs = BitConverter.ToInt16(msg.Payload, 56);

                LeftTspDiff0 = BitConverter.ToInt16(msg.Payload, 58);
                LeftTspDiff1 = BitConverter.ToInt16(msg.Payload, 60);
                LeftTspDiff2 = BitConverter.ToInt16(msg.Payload, 62);
                RightTspDiff0 = BitConverter.ToInt16(msg.Payload, 64);
                RightTspDiff1 = BitConverter.ToInt16(msg.Payload, 66);
                RightTspDiff2 = BitConverter.ToInt16(msg.Payload, 68);

                LeftHall = msg.Payload[70].ToString("x");
                RightHall = msg.Payload[71].ToString("x");

                LeftPR = BitConverter.ToInt16(msg.Payload, 72);
                RightPR = BitConverter.ToInt16(msg.Payload, 74);
                LeftWD = BitConverter.ToInt16(msg.Payload, 76);
                RightWD = BitConverter.ToInt16(msg.Payload, 78);

                LeftCradleFlag = msg.Payload[79];
                RightCradleFlag = msg.Payload[80];

                LeftCradleBatt = msg.Payload[81];
                RightCradleBatt = msg.Payload[82];
            }
        }
        
        private string BytesToMacString(IReadOnlyList<byte> payload, int startIndex)
        {
            var sb = new StringBuilder();
            for (var i13 = 0; i13 < 6; i13++)
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

        private string VersionDataToString(IReadOnlyList<byte> payload, int startIndex)
        {
            var buildPrefix = ActiveModel.GetModelMetadata()?.BuildPrefix ?? "R???";
            
            if (ActiveModel == Models.Buds)
            {
                int swVarIndex = payload[startIndex];
                var swYearIndex = (payload[startIndex + 1] & 240) >> 4;
                var swMonthIndex = payload[startIndex + 1] & 15;
                var swRelVerIndex = payload[startIndex + 2];

                string swRelVarString;
                if (swRelVerIndex <= 15)
                {
                    swRelVarString = (swRelVerIndex & 255).ToString("X");
                }
                else
                {
                    swRelVarString = _swRelVer[swRelVerIndex - 16];
                }

                return buildPrefix + "XX" + _swVer[swVarIndex] + "0A" + _swYear[swYearIndex] + _swMonth[swMonthIndex] +
                       swRelVarString;
            }
            else
            {
                var swVar = (payload[startIndex] & 1) == 0 ? "E" : "U";
                var isFotaDm = (payload[startIndex] & 240) >> 4;
                
                var swYearIndex = (payload[startIndex + 1] & 240) >> 4;
                var swMonthIndex = payload[startIndex + 1] & 15;
                var swRelVerIndex = payload[startIndex + 2];
                
                return buildPrefix + "XX" + swVar + "0A" 
                       + _swYear[swYearIndex] + _swMonth[swMonthIndex] + _swRelVer[swRelVerIndex];
            }
        }
    }
}