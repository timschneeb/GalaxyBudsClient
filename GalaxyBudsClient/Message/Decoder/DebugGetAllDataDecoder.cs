using System;
using System.Collections.Generic;
using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.DEBUG_GET_ALL_DATA)]
public class DebugGetAllDataDecoder : BaseMessageDecoder
{
    private readonly string[] _swMonth = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L"];

    private readonly string[] _swRelVer = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"];

    private readonly string[] _swVer = ["E", "U"];
    private readonly string[] _swYear = ["O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"];
    public string? HardwareVersion { get; }
    public string? SoftwareVersion { get; }
    public string? LocalBluetoothAddress { get; }
    public string? PeerBluetoothAddress { get; }
    public string? TouchSoftwareVersion { get; }
    public short LeftAcceleratorX { get; }
    public short LeftAcceleratorY { get; }
    public short LeftAcceleratorZ { get; }
    public short RightAcceleratorX { get; }
    public short RightAcceleratorY { get; }
    public short RightAcceleratorZ { get; }
    public short LeftProximity { get; }
    public short RightProximity { get; }
    [Postfix(Text = "°C")]
    public double LeftThermistor { get; }
    [Postfix(Text = "°C")]
    public double RightThermistor { get; }

    [Postfix(Text = "%")]
    public double LeftAdcSoc { get; }
    [Postfix(Text = "V")]
    public double LeftAdcVCell { get; }
    [Postfix(Text = "mA")]
    public double LeftAdcCurrent { get; }

    [Postfix(Text = "%")]
    public double RightAdcSoc { get; }
    [Postfix(Text = "V")]
    public double RightAdcVCell { get; }
    [Postfix(Text = "mA")]
    public double RightAdcCurrent { get; }

    public string? LeftHall { get; }
    public string? RightHall { get; }

    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short LeftProximityOffset { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short RightProximityOffset { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public byte MsgVersion { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short LeftTspAbs { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short RightTspAbs { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short LeftTspDiff0 { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short LeftTspDiff1 { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short LeftTspDiff2 { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short RightTspDiff0 { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short RightTspDiff1 { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short RightTspDiff2 { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short LeftPr { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short RightPr { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short LeftWd { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public short RightWd { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public byte LeftCradleFlag { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public byte RightCradleFlag { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public byte LeftCradleBatt { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public byte RightCradleBatt { get; }

    public DebugGetAllDataDecoder(SppMessage msg) : base(msg)
    {
        if (TargetModel == Models.Buds)
        {
            var hw1 = (msg.Payload[0] & 240) >> 4;
            var hw2 = msg.Payload[0] & 15;

            HardwareVersion = "rev" + hw1.ToString("X") + "." + hw2.ToString("X");
            SoftwareVersion = VersionDataToString(msg.Payload, 1);
            TouchSoftwareVersion = $"0x{msg.Payload[4]:X}";
            LocalBluetoothAddress = msg.Payload.BytesToMacString(5);
            PeerBluetoothAddress = msg.Payload.BytesToMacString(11);

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

            LeftAdcSoc = BitConverter.ToDouble(msg.Payload, 49);
            LeftAdcVCell = BitConverter.ToDouble(msg.Payload, 57);
            LeftAdcCurrent = BitConverter.ToDouble(msg.Payload, 65);
            RightAdcSoc = BitConverter.ToDouble(msg.Payload, 73);
            RightAdcVCell = BitConverter.ToDouble(msg.Payload, 81);
            RightAdcCurrent = BitConverter.ToDouble(msg.Payload, 89);

            LeftHall = msg.Payload[97].ToString("x") + " " + msg.Payload[98].ToString("x");
            RightHall = msg.Payload[99].ToString("x") + " " + msg.Payload[100].ToString("x");
        }
        else
        {
            var hw1 = (msg.Payload[1] & 240) >> 4;
            var hw2 = msg.Payload[1] & 15;

            MsgVersion = msg.Payload[0];

            HardwareVersion = "rev" + hw1.ToString("X") + "." + hw2.ToString("X");
            SoftwareVersion = VersionDataToString(msg.Payload, 2);
            TouchSoftwareVersion = $"0x{msg.Payload[5]:X}";
            LocalBluetoothAddress = msg.Payload.BytesToMacString(6);
            PeerBluetoothAddress = msg.Payload.BytesToMacString(12);

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

            LeftAdcSoc = BitConverter.ToInt16(msg.Payload, 42);
            LeftAdcVCell = BitConverter.ToInt16(msg.Payload, 44) * 0.01d;
            LeftAdcCurrent = BitConverter.ToInt16(msg.Payload, 46) * -0.1d; //1.0E-4d;
            RightAdcSoc = BitConverter.ToInt16(msg.Payload, 48);
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

            LeftPr = BitConverter.ToInt16(msg.Payload, 72);
            RightPr = BitConverter.ToInt16(msg.Payload, 74);
            LeftWd = BitConverter.ToInt16(msg.Payload, 76);
            RightWd = BitConverter.ToInt16(msg.Payload, 78);

            LeftCradleFlag = msg.Payload[79];
            RightCradleFlag = msg.Payload[80];

            LeftCradleBatt = msg.Payload[81];
            RightCradleBatt = msg.Payload[82];
        }
    }

    private string VersionDataToString(IReadOnlyList<byte> payload, int startIndex)
    {
        var buildPrefix = TargetModel.GetModelMetadataAttribute()?.BuildPrefix ?? "R???";
            
        if (TargetModel == Models.Buds)
        {
            int swVarIndex = payload[startIndex];
            var swYearIndex = (payload[startIndex + 1] & 240) >> 4;
            var swMonthIndex = payload[startIndex + 1] & 15;
            var swRelVerIndex = payload[startIndex + 2];

            var swRelVarString = swRelVerIndex <= 15 ? (swRelVerIndex & 255).ToString("X") : _swRelVer[swRelVerIndex - 16];
            return buildPrefix + "XX" + _swVer[swVarIndex] + "0A" + _swYear[swYearIndex] + _swMonth[swMonthIndex] +
                   swRelVarString;
        }
        else
        {
            var swVar = (payload[startIndex] & 1) == 0 ? "E" : "U";
            // var isFotaDm = (payload[startIndex] & 240) >> 4;
                
            var swYearIndex = (payload[startIndex + 1] & 240) >> 4;
            var swMonthIndex = payload[startIndex + 1] & 15;
            var swRelVerIndex = payload[startIndex + 2];
                
            return buildPrefix + "XX" + swVar + "0A" 
                   + _swYear[swYearIndex] + _swMonth[swMonthIndex] + _swRelVer[swRelVerIndex];
        }
    }
}