using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Platform;
using Serilog;
// ReSharper disable InconsistentNaming

namespace GalaxyBudsClient.Message;

public class SppAlternativeMessage
{
    public readonly SppMessage Msg;
    public readonly MsgTypes Type;
    public readonly MsgIds Id;
    public readonly byte[] Payload;
    
    public SppAlternativeMessage(SppMessage msg)
    {
        if (msg.Id != MsgIds.UNK_SPP_ALT)
        {
            throw new InvalidPacketException(InvalidPacketException.ErrorCodes.OutOfRange);
        }
        if (msg.Payload.Length == 0)
        {
            throw new InvalidPacketException(InvalidPacketException.ErrorCodes.TooSmall);
        }
        Msg = msg;
        Type = Msg.Type;
        Id = (MsgIds)Msg.Payload[0];
        Payload = Msg.Payload.Length > 1 ? Msg.Payload.TakeLast(Msg.Payload.Length - 1).ToArray() : [];
    }

    public SppAlternativeMessage(MsgIds id, byte[] payload, MsgTypes type)
        : this(new SppMessage(MsgIds.UNK_SPP_ALT, type, ((byte[]) [(byte)id]).Concat(payload).ToArray()))
    {
    }

    public override string ToString()
    {
        return $"SppAlternativeMessage: {Msg}";
    }

    public static async Task ReadPropertyAsync(AltProperty id)
    {
        await SendAltRequestAsync(MsgIds.READ_PROPERTY, (byte)id, (byte)((int)id >> 8));
    }

    public static async Task WritePropertyAsync(byte[] payload)
    {
        await SendAltRequestAsync(MsgIds.WRITE_PROPERTY, payload);
    }

    private static async Task SendAltRequestAsync(MsgIds id, params byte[]? payload)
    {
        await BluetoothImpl.Instance.SendAltAsync(new SppAlternativeMessage(id, payload ?? [], MsgTypes.Request));
    }

    public enum AltProperty : short
    { 
        SUPPORTED_FEATURES = 256,
        FEATURE_BATTERY = 257,
        FEATURE_EQ = 258,
        FEATURE_ANC_LEVEL = 259,
        FEATURE_AMBIENT_LEVEL = 260,
        FEATURE_TOUCH_LOCK = 261,
        FEATURE_SWITCH_AUDIO_PATH_BY_WEARING_STATE = 262,
        FEATURE_ALWAYS_ON_MIC = 263,
        FEATURE_SEAMLESS_EARBUD_CONNECTION = 264,
        FEATURE_SBM_JITTER_LEVEL_MIN = 265,
        FEATURE_SBM_JITTER_LEVEL_MIDDLE = 266,
        FEATURE_SBM_JITTER_LEVEL_MAX = 267,
        FEATURE_FIND_MY_BUDS = 268,
        FEATURE_BINARY_UPDATE = 269,
        FEATURE_AUTO_SWITCH = 270,
        FEATURE_SPATIAL_AUDIO = 271,
        FEATURE_REAL_SOUND_RECORDING = 272,
        FEATURE_TOUCH_LONG_PRESS_HOTKEY_VOICE_ASSISTANT = 273,
        FEATURE_TOUCH_LONG_PRESS_HOTKEY_ANC = 274,
        FEATURE_TOUCH_LONG_PRESS_HOTKEY_AMBIENT = 275,
        FEATURE_TOUCH_LONG_PRESS_HOTKEY_VOLUME_CTRL = 276,
        FEATURE_USE_AMBIENT_DURING_CALL = 277,
        FEATURE_TOUCH_LONG_PRESS_HOTKEY_MUSIC_PLAYER = 278,
        FEATURE_RELAY_CALL = 279,
        FEATURE_TOUCH_CONTROL = 280,
        FEATURE_CHANGE_DEVICE_NAME = 281,
        FEATURE_RECHARGE = 282,
        FEATURE_TWP = 283,
        FEATURE_LE_AUDIO = 284,
        FEATURE_AURACAST = 285,
        FEATURE_RESPONSIVE_HEARING = 287,
        FEATURE_LOW_LATENCY_MODE = 290,
        ALL_CURRENT_STATES = 512,
        STATE_MODEL = 513,
        STATE_COUPLED = 514,
        STATE_PRIMARY = 515,
        STATE_DEVICE_ID_L = 516,
        STATE_DEVICE_ID_R = 517,
        STATE_WEARING_L = 518,
        STATE_WEARING_R = 519,
        STATE_BATTERY_L = 520,
        STATE_BATTERY_R = 521,
        STATE_BATTERY_CRADLE = 522,
        STATE_EQ = 523,
        STATE_ANC = 524,
        STATE_AMBIENT = 525,
        STATE_TOUCH_LOCK = 526,
        STATE_TOUCH_LONG_PRESS_HOTKEY_MAPPING_L = 527,
        STATE_TOUCH_LONG_PRESS_HOTKEY_MAPPING_R = 528,
        STATE_ALWAYS_ON_MIC = 529,
        STATE_ALWAYS_ON_MIC_WAKEUP_LANGUAGE = 530,
        STATE_SEAMLESS_EARBUD_CONNECTION = 531,
        STATE_GAME_MODE = 532,
        STATE_BINARY_VERSION_L = 533,
        STATE_BINARY_VERSION_R = 534,
        STATE_BINARY_VERSION_STR = 535,
        STATE_MODEL_STR = 536,
        STATE_DEVICE_GROUP_ID = 537,
        STATE_SBM_SINK_PREFERENCE = 538,
        STATE_SINK_CONDITION = 539,
        STATE_SPATIAL_AUDIO = 540,
        STATE_ADDRESS_L = 541,
        STATE_ADDRESS_R = 542,
        STATE_CONN_COUNT = 543,
        STATE_SERIAL_NUMBER_L = 544,
        STATE_SERIAL_NUMBER_R = 545,
        STATE_SIMPLE_NAME = 546,
        STATE_FULL_NAME = 547,
        STATE_NUM_OF_BONDED_DEVICES = 548,
        STATE_NAME_CHANGED_TIME = 549,
        STATE_CURRENT_NAME = 550,
        STATE_SPATIAL_AUDIO_HT = 551,
        STATE_RECHARGE_L = 552,
        STATE_RECHARGE_R = 553,
        STATE_RECHARGE_CRADLE = 554,
        STATE_TWP_SE = 555,
        STATE_TWP_PE = 556,
        STATE_TWP_CACHED = 557,  
        STATE_RESPONSIVE_HEARING = 559,
        CMD_SUBSCRIBE = 769,
        CMD_TERMINATE_PROFILE = 770,
        CMD_SUPPORT_SBM = 771,
        CMD_RELAY_CALL_STATE = 772,
        CMD_RELAY_CALL_NUMBER = 773,
        CMD_RELAY_CALL_NAME = 774,
        CMD_PERSONALIZED_NAME_TIMESTAMP = 775,
        CMD_PERSONALIZED_NAME_VALUE = 776,
        CMD_CTRL_TWP_RUN = 777,
        CMD_RE_PAIR_FOR_LEAUDIO = 778,
        CMD_CTRL_LOW_LATENCY_MODE = 780,
        ALL_DESCRIPTIONS = 1024,
        DESCRIPTION_EIR_MANU_DATA = 1025,
        REPLY_RELAY_CALL_ANSWER = 1282
        // these are just app boilerplate and earbuds don't know them:
        // CUSTOM_NUMBER = 49408,
        // CUSTOM_SERIAL = 49409,
        // CUSTOM_DISC_FOR_AUTO_SWITCH = 49410,
        // CUSTOM_CONNECTION_STATE = 49411,
        // CUSTOM_SUBSCRIBE = 49412,
        // CUSTOM_SUPPORT_RENAME = 49413,
        // CUSTOM_CONNECTED_WITH_PROFILE = 49414,
        // INVALID_KEY = 65535
    }

    public class Property(AltProperty type, byte[] response)
    {
        public readonly AltProperty Type = type;
        public readonly byte[] Response = response;

        /*public static List<Property> DecodeNotifyProperty(SppAlternativeMessage msg)
        {
            return msg.Id != MsgIds.NOTIFY_PROPERTY ? [] : Decode(msg.Payload);
        }*/
        
        public static List<Property> Decode(byte[] payload) { 
            var i = 0;
            List<Property> list = [];
            while (i < payload.Length)
            {
                if (i + 3 >= payload.Length)
                {
                    Log.Error("NotifyDecode: packet part too small");
                    return list;
                }

                // DESCRIPTION_EIR_MANU_DATA falsely claims that it contains a 256 byte payload, even though it is much smaller
                // Limit the responseSize to the remaining payload size to prevent out of bounds access
                var responseSize = Math.Min(payload[i + 2], payload.Length - i - 3);
                
                var item = new Property(
                    (AltProperty)(payload[i] | (payload[i + 1] << 8)),
                    new byte[responseSize]
                    );
                if (item.Response.Length > 0)
                {
                    Array.Copy(payload, i + 3, item.Response, 0, item.Response.Length);
                }
                list.Add(item);
                i += 3 + item.Response.Length;
            }

            return list;
        }

        public byte[] Encode()
        {
            return ((byte[]) [(byte)Type, (byte)((int)Type >> 8), (byte)Response.Length]).Concat(Response).ToArray();
        }

        public override string ToString()
        {
            return $"NotifyProperty(Type={Type}, Payload=[{BitConverter.ToString(Response).Replace("-", " ")}])";
        }
    }

    public class ReadProperty(AltProperty type, List<Property> response)
    {
        public readonly AltProperty Type = type;
        public readonly List<Property> Response = response;

        public static ReadProperty Decode(SppAlternativeMessage msg)
        {
            return new ReadProperty(
                (AltProperty)(msg.Payload[0] | (msg.Payload[1] << 8)),
                Property.Decode(msg.Payload.TakeLast(msg.Payload.Length - 2).ToArray())
            );
        }
    }
}