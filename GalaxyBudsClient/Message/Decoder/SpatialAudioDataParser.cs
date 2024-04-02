using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Model.Constants;
using Serilog;

namespace GalaxyBudsClient.Message.Decoder;

internal class SpatialAudioDataParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.SPATIAL_AUDIO_DATA;
    public SpatialAudioData EventId { set; get; }
        
    /* Wear on/off */
    public bool WearOnOff { set; get; }
    public bool WearOnOffParam2 { set; get; }
        
    /* Grv */
    public float[]? GrvFloatArray { set; get; }
    public bool GrvBoolean { set; get; }

    /* Gyrocal */
    public int[]? GyrocalBias { set; get; }
        
    /* SensorStuck */
    public int StuckParameter { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        EventId = (SpatialAudioData) msg.Payload[0];

        var length = msg.Payload.Length - 1;
        var data = new byte[length < 0 ? 0 : length];
        Array.Copy(msg.Payload, 1, data, 0, length);

        switch (EventId)
        {
            case SpatialAudioData.BudGrv:
                if (data.Length < 9) {
                    Log.Error("SpatialAudioDataParser.GRV: wrong data length: {Length}", data.Length);
                    return;
                }
                    
                var fArr = new float[4];
                for (var i = 0; i < 4; i++) {
                    fArr[i] = (float) (short) ByteBufferToInt(data, i * 2, 2, false) / 10000.0f;
                }

                GrvFloatArray = fArr;
                GrvBoolean = data[8] == 0;
                break;
            case SpatialAudioData.BudGyrocal:
                if (data.Length < 6) 
                {
                    Log.Error("SpatialAudioDataParser.Gyrocal: Invalid gyro bias info received");
                    return;
                }
                    
                int[] iArr = [(short) ByteBufferToInt(data, 0, 2, false), (short) ByteBufferToInt(data, 2, 2, false), (short) ByteBufferToInt(data, 4, 2, false)
                ];
                var list = new List<int>();
                list.AddRange(iArr);
                if (data.Length >= 9) 
                {
                    list.Add(data[6]);
                    list.Add(ByteBufferToInt(data, 7, 2, false));
                    Log.Debug( "SpatialAudioDataParser.Gyrocal: Bias: {Bias}", string.Join(",", list));
                }
                else
                {
                    Log.Debug("SpatialAudioDataParser.Gyrocal: Bias: {Bias}", iArr);
                }

                GyrocalBias = list.ToArray();
                break;
            case SpatialAudioData.BudSensorStuck:
                Log.Debug("SpatialAudioDataParser.SensorStuck: {Param}", data[0]);
                StuckParameter = data[0];
                break;
            case SpatialAudioData.WearOff:
                WearOnOff = false;
                    
                if (msg.Payload.Length >= 2) {
                    WearOnOffParam2 = msg.Payload[1] == 1;
                    return;
                }

                Log.Warning("SpatialAudioDataParser: Wear off updated, but wrong message format");

                break;
            case SpatialAudioData.WearOn:
                WearOnOff = true;
                    
                if (msg.Payload.Length >= 2) {
                    WearOnOffParam2 = msg.Payload[1] == 1;
                    return;
                }

                Log.Warning("SpatialAudioDataParser: Wear on updated, but wrong message format");
                break;
            case SpatialAudioData.SensorSupported:
            case SpatialAudioData.GyroBiasExistence:
            case SpatialAudioData.ManualGyrocalReady:
            case SpatialAudioData.ManualGyrocalNotReady:
            case SpatialAudioData.BudGyrocalFail:
                break;
            default:
                Log.Debug("SpatialAudioDataParser: Unknown id ({Id})", msg.Payload[0]);
                break;
        }
    }

    private static int ByteBufferToInt(IReadOnlyList<byte> bArr, int i, int i2, bool z)
    {
        if (bArr.Count >= i + i2)
        {
            var i4 = 0;
            int i3;
            if (z) {
                i3 = 0;
                while (i4 < i2) 
                {
                    i3 += (bArr[i + i4] & 255) << ((i2 - 1 - i4) * 8);
                    i4++;
                }
            } 
            else
            {
                i3 = 0;
                while (i4 < i2)
                {
                    i3 += (bArr[i + i4] & 255) << (i4 * 8);
                    i4++;
                }
            }
            return i3;
        } 
        else 
        {
            return -2;
        }
    }

        
    public override Dictionary<string, string> ToStringMap()
    {
        Dictionary<string, string> map = new();
        var properties = GetType().GetProperties();
        foreach (var property in properties)
        {
            if (IsHiddenProperty(property))
                continue;

            switch (EventId)
            {
                case SpatialAudioData.BudGrv:
                    if (property.Name != nameof(GrvFloatArray) &&
                        property.Name != nameof(GrvBoolean))
                        continue;
                    break;
                case SpatialAudioData.BudGyrocal:
                    if (property.Name != nameof(GyrocalBias))
                        continue;
                    break;
                case SpatialAudioData.BudSensorStuck:
                    if (property.Name != nameof(StuckParameter))
                        continue;
                    break;
                case SpatialAudioData.WearOff:
                case SpatialAudioData.WearOn:
                    if (property.Name != nameof(WearOnOff) &&
                        property.Name != nameof(WearOnOffParam2))
                        continue;
                    break;
            }

            if (property.PropertyType.IsArray)
            {
                var array = (Array?) property.GetValue(this);
                if (array == null)
                {
                    map.Add(property.Name, "null");
                    continue;
                }
                    
                var str = "";
                foreach (var o in array)
                {
                    str += o?.ToString();
                    str += ',';
                }
                str = str.Remove(str.Length - 1, 1);
                    
                map.Add(property.Name, str);
            }
            else
            {
                map.Add(property.Name, property.GetValue(this)?.ToString() ?? "null");
            }
        }

        return map;
    }
}