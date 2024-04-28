using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;
using Serilog;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.USAGE_REPORT_V2)]
public class Usage2ReportDecoder : BaseMessageDecoder
{
    public enum LoggingType
    {
        Event = 0,
        Status = 1
    }

    public record LoggingItem
    {
        public required string Id { init; get; }
        public required LoggingType Type { init; get; } 
        public required ValueItem[] ValueItems { init; get; }

        public string FriendlyName =>
            Events.Concat(Statuses).FirstOrDefault(x => x.Value == Id).Key ?? Id;
    }

    public record ValueItem
    {
        public required char Detail { get; init; }
        public required long Value { get; init; }
    }
    
    public string? ErrorReason { set; get; }
    public LoggingItem[] Items { set; get; } = Array.Empty<LoggingItem>();

    public override void Decode(SppMessage msg)
    {
        ErrorReason = null;
        
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);

        var list = new List<LoggingItem>();
        try
        {
            var loggingItemCount = reader.ReadByte();

            for (var i = 0; i < loggingItemCount; i++)
            {
                var idBytes = new byte[8];
                reader.ReadBytes(7).CopyTo(idBytes, 0);
                
                var headerByte = reader.ReadByte();
                var valueSize = (byte)(headerByte & 15);
                var valueItemSize = valueSize + 1;
                var valueItemTotalLength = reader.ReadByte();
                var valueItemCount = valueItemTotalLength / valueItemSize;

                if (valueItemSize * valueItemCount != valueItemTotalLength) {
                    throw new InvalidOperationException("valueItemSize * valueItemCount != valueItemTotalLength");
                }

                var loggingItem = new LoggingItem
                {
                    Id = Encoding.UTF8.GetString(idBytes).Trim().Trim('\0'),
                    Type = (LoggingType)((headerByte & 240) >> 4),
                    ValueItems = new ValueItem[valueItemCount]
                };
                for (var j = 0; j < valueItemCount; j++)
                {
                    
                    loggingItem.ValueItems[j] = new ValueItem
                    {
                        Detail = reader.ReadChar(),
                        Value = valueSize switch
                        {
                            1 => reader.ReadByte(),
                            2 => reader.ReadUInt16(),
                            4 => reader.ReadUInt32(),
                            _ => throw new Exception("Wrong valueSize")
                        }
                    };
                }

                list.Add(loggingItem);
            }
            
            Items = list.ToArray();
            
            if (reader.BaseStream.Position < reader.BaseStream.Length - 1)
            {
                throw new Exception("Not all data read");
            }
        }
        catch (Exception th)
        {
            Log.Warning(th, "Failed to parse USAGE_REPORT_V2");
            ErrorReason = th.Message;
        }
    }

    public override Dictionary<string, string> ToStringMap()
    {
        Dictionary<string, string> map = new();
        if (ErrorReason != null)
            map[nameof(ErrorReason)] = ErrorReason;
        
        foreach (var item in Items)
        {
            map[$"{item.FriendlyName} ({item.Type})"] = string.Join(", ", item.ValueItems.Select(x => x.Detail == 0 ? x.Value.ToString() : $"{x.Detail}: {x.Value}"));
        }
        return map;
    }
    
    // Names & values copied from the official app
    private static readonly Dictionary<string, string> Events = new()
    {
        { "ABOUT_EARBUDS", "2310" },
        { "ABOUT_GALAXY_WEARABLE", "2014" },
        { "ACTIVE_NOISE_CANCELLING_HIGH", "1023" },
        { "ACTIVE_NOISE_CANCELLING_LEVEL", "2377" },
        { "ACTIVE_NOISE_CANCELLING_LOW", "1024" },
        { "ADD_NEW_DEVICE", "1026" },
        { "ADD_NEW_DEVICE_2", "DRAW0003" },
        { "ADVANCED", "2309" },
        { "ALLOW_NOTIFICATION", "6655" },
        { "AMBIENT_SOUND_LEVEL", "2379" },
        { "AMBIENT_SOUND_ON_OFF", "2322" },
        { "AMBIENT_SOUND_TONE", "SET2723" },
        { "AMBIENT_SOUND_VOLUME", "2323" },
        { "AMBIENT_SOUND_VOLUME_LEFT", "SET2721" },
        { "AMBIENT_SOUND_VOLUME_RIGHT", "SET2722" },
        { "AMPLIFY_AMBIENT", "2712" },
        { "ANC_SWITCH", "2335" },
        { "APP_INFORMATION", "2203" },
        { "APP_NOTIFICATION", "1110" },
        { "APP_NOTIFICATION_LIST", "6602" },
        { "AUTO_ADJUST_SOUND", "SET128" },
        { "AUTO_DOWNLOAD_OVER_WIFI", "7001" },
        { "AUTO_UPDATE", "SET135" },
        { "AUTO_UPDATE_CHECKBOX", "5311" },
        { "AUTO_UPDATE_GW_CHECKBOX", "5312" },
        { "BASE_BOOST", "2301" },
        { "BATTERY_INFORMATION", "2262" },
        { "BATTERY_WIDGET", "6672" },
        { "BIXBY_VOICE_WEAK_UP", "2700" },
        { "BOOST_VOICES", "SET129" },
        { "CALIBRATION_DONE", "SET0091" },
        { "CALIBRATION_GET_HELP", "SET0081" },
        { "CALIBRATION_START", "SET0061" },
        { "CALIBRATION_TRY_AGAIN", "SET0082" },
        { "CALIBRATION_UP", "SET0060" },
        { "CALIBRATION_UP_DONE", "SET0090" },
        { "CALIBRATION_UP_FAILED", "SET0080" },
        { "CALIBRATION_UP_OR_BACK", "SET0070" },
        { "CAL_DONE", "SET149" },
        { "CHANGE_DEVICE", "1033" },
        { "CLEAR", "2304" },
        { "CONNECT_DISCONNECT", "DRAW0002" },
        { "CONTACT_US", "1030" },
        { "CONVERSATION_MODE_TIME_SETTING", "2383" },
        { "COVER_WIDGET_360", "WIDG0008" },
        { "COVER_WIDGET_BLOCK_TOUCHES", "WIDG0006" },
        { "COVER_WIDGET_NOISE_CONTROL", "WIDG0005" },
        { "COVER_WIDGET_OPEN_APP", "WIDG0004" },
        { "CURRENT_DEVICE", "1032" },
        { "DEVICE_NAME_EDIT", "2352" },
        { "DISCONNECT", "1025" },
        { "DOUBLE_TAB_DETAIL", "2347" },
        { "DOUBLE_TAB_SWITCH", "2348" },
        { "DOUBLE_TAP_EARBUD_EDGE", "9026" },
        { "DOUBLE_TAP_SWITCH", "SET0009" },
        { "DOUBLE_TAP_SWITCH_FOR_CALLS", "SET0020" },
        { "DOWNLOAD_AND_INSTALL", "7000" },
        { "DOWNLOAD_LATER", "7005" },
        { "DOWNLOAD_NOW", "7004" },
        { "DRAWER_ABOUT_GALAXY_WEARABLE", "DRAW0007" },
        { "DRAWER_AUTO_UPDATE_GALAXY_WEARABLE", "DRAW0108" },
        { "DRAWER_CONTACT_US", "DRAW0006" },
        { "DRAWER_NOTIFICATIONS", "DRAW0018" },
        { "DRAWER_PERMISSIONS", "DRAW0103" },
        { "DYNAMIC", "2303" },
        { "EARBUDS_ACTIVE_NOISE_CANCELING_DURATION", "9019" },
        { "EARBUDS_AMBIENT_SOUND_DURATION", "9007" },
        { "EARBUDS_AMBIENT_SOUND_TIMES", "9008" },
        { "EARBUDS_ASSERT_CPU_EXCEPTION", "9016" },
        { "EARBUDS_BATTERY_CONSUMPTION_FOR_EACH_USE", "9021" },
        { "EARBUDS_BIXBY_VOICE_WAKE_UP", "9027" },
        { "EARBUDS_DISCONNECTION_DURING_CALL", "9024" },
        { "EARBUDS_DISCONNECTION_WHILE_INCOMING_CALL", "9023" },
        { "EARBUDS_DOUBLE_TAP", "9002" },
        { "EARBUDS_DURATION_THAT_EARBUDS_CONNECTED_BUT", "9025" },
        { "EARBUDS_ERROR_CHARGING", "9015" },
        { "EARBUDS_ERROR_DISCHARGING", "9014" },
        { "EARBUDS_FIT_TEST", "SET0001" },
        { "EARBUDS_FIT_TEST_HOW_TO_WAER_TOUR_EARBUDS", "2391" },
        { "EARBUDS_FIT_TEST_NEXT", "2389" },
        { "EARBUDS_FIT_TEST_RETRY", "2390" },
        { "EARBUDS_FIT_TEST_START", "2393" },
        { "EARBUDS_FIT_TEST_VIDEO", "2392" },
        { "EARBUDS_LOW_BATTERY", "9013" },
        { "EARBUDS_MASTER_SWITCHING_FOR_EACH_USE", "9022" },
        { "EARBUDS_MUSIC_FROM_PHONE_DURATION", "9009" },
        { "EARBUDS_MUSIC_FROM_PHONE_TIMES", "9010" },
        { "EARBUDS_SETTINGS", "1020" },
        { "EARBUDS_SINGLE_TAP", "9001" },
        { "EARBUDS_TAP_AND_HOLD_BOTH", "9006" },
        { "EARBUDS_TAP_AND_HOLD_LEFT", "9004" },
        { "EARBUDS_TAP_AND_HOLD_RIGHT", "9005" },
        { "EARBUDS_TRIPLE_TAP", "9003" },
        { "EARBUDS_WEARING_DURATION", "9011" },
        { "EARBUDS_WEARING_TIMES", "9012" },
        { "ELSE_RESET_EARBUDS", "2340" },
        { "ELSE_USER_MANUAL", "2341" },
        { "EQUALIZER", "2329" },
        { "EQUALIZER_SLIDER", "2335" },
        { "EXTRA_HIGH_VOLUME_AMBIENT", "2350" },
        { "FIND_MY_EARBUDS", "2307" },
        { "FIND_MY_EARBUDS_PLAY_SOUND", "3011" },
        { "FIND_MY_EARBUDS_START", "3010" },
        { "FIND_MY_EARBUDS_STOP", "3050" },
        { "GALAXY_WEARABLE_SETTINGS", "DRAW0017" },
        { "GAME_MODE", "2338" },
        { "GAME_MODE_DETAIL", "2353" },
        { "GAME_MODE_SWITCH", "2354" },
        { "GENERAL", "2332" },
        { "GOT_IT", "6674" },
        { "GOT_IT_AMBIENT", "6674" },
        { "GOT_IT_SPOTIFY", "6673" },
        { "HEADPHONE_ASSISTANCE", "2263" },
        { "HEAD_TRACKING_SWITCH", "SET127" },
        { "HEARING_ENHANCEMENTS", "2376" },
        { "HOW_TO_STRETCH_NECK", "SET137" },
        { "INLINE_QUE_AMBIENT_SOUND_DURING_CALLS_LATER", "CUE0008" },
        { "INLINE_QUE_AMBIENT_SOUND_DURING_CALLS_TRY", "CUE0009" },
        { "INLINE_QUE_CONNECT", "CUE0001" },
        { "INLINE_QUE_FIT_TEST_LATER", "CUE0006" },
        { "INLINE_QUE_FIT_TEST_START", "CUE0007" },
        { "INLINE_QUE_STF_CLOSE", "CUE0002" },
        { "INLINE_QUE_STF_IN_CASE_CLOSE", "CUE0015" },
        { "INLINE_QUE_STF_IN_CASE_START", "CUE0016" },
        { "INLINE_QUE_STF_START", "CUE0003" },
        { "INLINE_QUE_WIDGET_ADD", "CUE0005" },
        { "INLINE_QUE_WIDGET_LATER", "CUE0004" },
        { "INSTALL_LATER", "7005" },
        { "INSTALL_NOW", "7006" },
        { "IN_CASE_OF_ONLY_ONE_EARBUD_A_WEEK", "9030" },
        { "IN_EAR_DETECTION", "SET125" },
        { "IN_EAR_DETECTION_SWITCH", "SET0051" },
        { "LABS", "2344" },
        { "LAST_UPDATE", "7003" },
        { "LEFT_MUTE", "3051" },
        { "LEGAL_INFORMATION", "2210" },
        { "LOCKSCREEN_WIDGET_360", "LckrWidg0012" },
        { "LOCKSCREEN_WIDGET_NOISE_CONTROL", "LckrWidg0010" },
        { "LOCKSCREEN_WIDGET_TOUCH_CONTROL", "LckrWidg0011" },
        { "LOCK_TOUCHPAD", "2316" },
        { "MANAGE_DEVICE", "DRAW0004" },
        { "MANAGE_DEVICES", "1111" },
        { "MANAGE_NOTIFICATIONS", "6600" },
        { "MANAGE_NOTIFICATIONS_SWITCH", "6601" },
        { "NECK_STRETCH_NOTI_CLEARED", "SET151" },
        { "NECK_STRETCH_NOTI_CREATED", "SET150" },
        { "NECK_STRETCH_RECALIBRATE", "SET138" },
        { "NECK_STRETCH_REMINDER", "SET126" },
        { "NECK_STRETCH_REMINDER_SWITCH", "SET136" },
        { "NEXT", "OOBE0002" },
        { "NOISE_CONTROLS", "2375" },
        { "NOISE_CONTROLS_WITH_ONE_EARBUDS", "SET0005" },
        { "NOISE_REDUCTION", "SET130" },
        { "NORMAL", "2333" },
        { "NOTIFICATIONS", "2306" },
        { "NOTIFICATION_APPS", "6671" },
        { "NOTIFICATION_SEE_ALL", "6670" },
        { "NUM_OF_TIMES_DEFFERENCE_BATTERY_EXCEEDED_15_PERCENT_A_WEEK", "9029" },
        { "NUM_OF_TIMES_SINGLE_EARBUDS_CONNECTION_A_WEEK", "9028" },
        { "OPEN_SOURCE_LICENSES", "2343" },
        { "PRACTICE_TAPPING", "2369" },
        { "PREV", "OOBE0003" },
        { "READ_NOTIFICATION_ALOUD", "6655" },
        { "READ_OUT_NOTIFICATION", "6678" },
        { "READ_OUT_NOTIFICATIONS", "6656" },
        { "READ_OUT_WHILE_USING_PHONE", "6650" },
        { "RELIEVE_PRESSURE_WITH_AMBIENT_SOUND", "2366" },
        { "REPEAT", "6657" },
        { "RESET_EARBUDS", "2260" },
        { "RIGHT_MUTE", "3052" },
        { "SEAMLESS_EARBUDS_CONNECTION_SWITCH", "2371" },
        { "SEAMLESS_EARBUD_CONNECTION", "2364" },
        { "SEARCH_ALL_TAGS", "SRCH0106" },
        { "SEARCH_APP", "6679" },
        { "SEARCH_DELETE_INDIVIDUAL_SEARCH", "SRCH0103" },
        { "SEARCH_HISTORY_CLEAR_ALL", "SRCH0303" },
        { "SEARCH_RECENT_SEARCH", "SRCH0102" },
        { "SEARCH_RESULT_CLEAR_SEARCH_FIELD", "SRCH0201" },
        { "SEARCH_RESULT_EXPAND_RESULT", "SRCH0207" },
        { "SEARCH_RESULT_RESULT_FORM_TIPS", "SRCH0208" },
        { "SEARCH_RESULT_TAP_RESULT", "SRCH0202" },
        { "SEARCH_RESULT_TOGGLE_RESULT", "SRCH0203" },
        { "SEARCH_RESULT_TRUN_ONOFF_RESULT", "SRCH0204" },
        { "SEARCH_SELECT_TAG", "SRCH0402" },
        { "SEARCH_TAG", "SRCH0105" },
        { "SEARCH_TAG_UP_BUTTON", "SRCH00" },
        { "SEARCH_UP_BUTTON", "SRCH0100" },
        { "SEARCH_VOICE_SEARCH", "SRCH0101" },
        { "SET_NOISE_CONTROLS", "2387" },
        { "SKIP", "OOBE0001" },
        { "SOFT", "2302" },
        { "SOUND_BALANCE", "2710" },
        { "SPEAK_SEAMLESSLY", "2702" },
        { "START_CAL", "SET146" },
        { "START_TAPPING_PRACTICE", "2370" },
        { "SWITCH_NOISE_CONTROLS_LEFT_SETTING", "SET0002" },
        { "SWITCH_NOISE_CONTROLS_POPUP", "SET0004" },
        { "SWITCH_NOISE_CONTROLS_RIGHT_SETTING", "SET0003" },
        { "TAP_AND_HOLD_OTHERS_APPS", "9018" },
        { "TAP_AND_HOLD_TOUCHPAD_LEFT", "2317" },
        { "TAP_AND_HOLD_TOUCHPAD_RIGHT", "2318" },
        { "TAP_SWITCH", "SET0008" },
        { "TIPS_ADD_WIDGET_ADD_NOW", "TPS002" },
        { "TIPS_AND_USER_MANUAL", "2261" },
        { "TIPS_CLOSE", "2312" },
        { "TIPS_FOR_TOUCH_CONTROLS", "SET0012" },
        { "TIPS_HOW_TO_WEAR_TEST_FIT", "TPS003" },
        { "TIPS_LETS_GO", "2314" },
        { "TIPS_MORE_OPTION", "TPS004" },
        { "TIPS_OK", "2315" },
        { "TIPS_TOUCH_CONTROLS_SETTINGS", "TPS001" },
        { "TIPS_VIEW_UPDATE", "2313" },
        { "TOUCHPAD", "2308" },
        { "TOUCH_AND_HOLD", "2385" },
        { "TOUCH_AND_HOLD_SWITCH", "SET0011" },
        { "TOUCH_AND_HOLD_SWITCH_FOR_CALLS", "SET0021" },
        { "TOUCH_CONTROL", "6680" },
        { "TOUCH_CONTROLS", "SET0006" },
        { "TOUCH_CONTROLS_SWITCH", "SET0013" },
        { "TOUCH_CONTROLS_SWITCH_CARD", "SET0007" },
        { "TREBLE_BOOST", "2305" },
        { "TRIPLE_TAP_SWITCH", "SET0010" },
        { "TROUBLE_TAPPING", "2368" },
        { "TRYAGAIN_STEP1", "SET147" },
        { "TRYAGAIN_STEP2", "SET148" },
        { "TURN_ON_AND_OFF_CUSTOMIZE_AMBIENT_SOUND", "SET2720" },
        { "T_AND_C_AGREE", "5310" },
        { "UPDATE", "2204" },
        { "UPDATE_EARBUDS_SOFTWARE", "2208" },
        { "UP_BUTTON", "1000" },
        { "USER_MANUAL", "2342" },
        { "USE_AMBIENT_SOUND_DURING_CALLS", "SET0018" },
        { "USE_AMBIENT_SOUND_DURING_CALLS_SWITCH", "2345" },
        { "VOICE_DETECT", "1022" },
        { "VOICE_DETECT_DEPTH_IN", "1021" },
        { "VOICE_DETECT_SWITCH", "2381" },
        { "VOICE_WAKE_UP_SWITCH", "2360" },
        { "WEARABLE_DEVICES", "DRAW0001" },
        { "WEAR_BOTH_AMBIENT_SOUND", "EB0007" },
        { "WEAR_BOTH_ANC", "EB0004" },
        { "WEAR_BOTH_NOISE_CONTROL_OFF", "EB0001" },
        { "WEAR_LEFT_NOISE_CONTROL_OFF", "EB0002" },
        { "WEAR_RIGHT_NOISE_CONTROL_OFF", "EB0003" },
        { "WIDGET_360", "WIDG0009" },
        { "WIDGET_AMBIENT_SOUND", "6676" },
        { "WIDGET_ANC", "6673" },
        { "WIDGET_BLOCK_TOUCHES", "WIDG0003" },
        { "WIDGET_LOCK_TOUCHPAD", "6677" },
        { "WIDGET_NOISE_CONTROL", "WIDG0002" },
        { "WIDGET_TAP", "WIDG0001" },
        { "_360_AUDIO", "2720" },
        { "_360_AUDIO_MENU", "2373" },
        { "_360_AUDIO_SWITCH", "SET144" },
        { "_3D_AUDIO_FOR_VIDEOS_SWITCH", "2373" },
    };

    private static readonly Dictionary<string, string> Statuses = new()
    {
        { "ACTIVE_NOISE_CANCELLING_LEVEL_STATUS", "2378" },
        { "ALL_APPS", "6604" },
        { "AMBIENT_SOUND", "2325" },
        { "AMBIENT_SOUND_TONE", "SET2723" },
        { "AMBIENT_SOUND_VOLUME", "2326" },
        { "AMBIENT_SOUND_VOLUME_LEFT", "SET2721" },
        { "AMBIENT_SOUND_VOLUME_RIGHT", "SET2722" },
        { "AMBIENT_SOUND_VOLUME_STATUS", "2380" },
        { "AMPLIFY_AMBIENT", "2712" },
        { "ANC", "2356" },
        { "APPS_ALLOWED", "6672" },
        { "APP_TO_SELECT", "6654" },
        { "AUTO_ADJUST_SOUND", "SET51" },
        { "AUTO_DOWNLOAD_OVER_WIFI", "7002" },
        { "AUTO_UPDATE_SETTING", "SET52" },
        { "BOOST_VOICE", "SET49" },
        { "CHARGE_POWERSHARE_COUNT_LEFT", "EB0010" },
        { "CHARGE_POWERSHARE_COUNT_RIGHT", "EB0011" },
        { "CHARGE_POWERSHARE_TIME_LEFT", "EB0016" },
        { "CHARGE_POWERSHARE_TIME_RIGHT", "EB0017" },
        { "CHARGE_WIRED_COUNT_LEFT", "EB0012" },
        { "CHARGE_WIRED_COUNT_RIGHT", "EB0013" },
        { "CHARGE_WIRED_TIME_LEFT", "EB0018" },
        { "CHARGE_WIRED_TIME_RIGHT", "EB0019" },
        { "CHARGE_WIRELESS_COUNT_LEFT", "EB0008" },
        { "CHARGE_WIRELESS_COUNT_RIGHT", "EB0009" },
        { "CHARGE_WIRELESS_TIME_LEFT", "EB0014" },
        { "CHARGE_WIRELESS_TIME_RIGHT", "EB0015" },
        { "CONVERSATION_MODE_TIME_SETTING", "2384" },
        { "CURRENT_DEVICE_STATUS", "DRAW0001" },
        { "DEVICE_NAME", "2214" },
        { "DOUBLE_TAP_SIDE", "2349" },
        { "DOUBLE_TAP_STATUS", "SET0009" },
        { "DOUBLE_TAP_SWITCH_FOR_CALLS", "SET0016" },
        { "DRAWER_PANEL_STATUS", "DRAW0002" },
        { "EARBUDS_SW_VERSION", "9017" },
        { "EARBUD_FIT_TEST_RESULT_LEFT", "SET0014" },
        { "EARBUD_FIT_TEST_RESULT_RIGHT", "SET0015" },
        { "EQUALIZER_STATUS", "2329" },
        { "EXTRA_HIGH_VOLUME_AMBIENT", "2351" },
        { "GAME_MODE", "2339" },
        { "GAMING_MODE_STATUS", "2355" },
        { "HEAD_TRACKING_STATUS", "SET48" },
        { "IN_EAR_DETECTION_STATUS", "SET0061" },
        { "LEFT_EARBUD_CHARGING_CYCLE", "9033" },
        { "LOCK_TOUCHPAD", "2319" },
        { "NECK_STRETCH_REMINDER", "SET53" },
        { "NOISE_CONTROLS_MODE", "2324" },
        { "NOISE_CONTROLS_MODE_RIGHT", "2327" },
        { "NOISE_CONTROLS_STATUS", "2376" },
        { "NOISE_CONTROLS_STATUS_SINGLE", "2377" },
        { "NOISE_CONTROLS_WITH_ONE_EARBUDS", "SET0005" },
        { "NOISE_REDUCTION", "SET50" },
        { "NOTIFICATION_ON", "6603" },
        { "READ_OUT_NOTIFICATION", "6661" },
        { "READ_OUT_WHILE_USING_PHONE", "6678" },
        { "RELIEVE_PRESSURE_WITH_AMBIENT_SOUND_STATUS", "2367" },
        { "REPEAT", "6660" },
        { "RIGHT_EARBUD_CHARGING_CYCLE", "9032" },
        { "SEAMLESS_EARBUD_CONNECTION_STATUS", "2365" },
        { "SOUND_BALANCE", "2711" },
        { "SPEAK_SEAMLESSLY", "2703" },
        { "TAH_LEFT_STATUS", "2320" },
        { "TAH_RIGHT_STATUS", "2321" },
        { "TAP_STATUS", "SET0008" },
        { "TOUCH_AND_HOLD_LEFT", "2317" },
        { "TOUCH_AND_HOLD_RIGHT", "2318" },
        { "TOUCH_AND_HOLD_STATUS", "SET0011" },
        { "TOUCH_AND_HOLD_SWITCH_FOR_CALLS", "SET0017" },
        { "TOUCH_CONTROLS_STATUS", "SET0013" },
        { "TRIPLE_TAP_STATUS", "SET0010" },
        { "USE_AMBIENT_SOUND_DURING_CALLS", "2346" },
        { "VOICE_DETECT_STATUS", "2382" },
        { "VOICE_WAKE_UP_STATUS", "2701" },
        { "WITCH_WIDGET_INSTALLED", "6681" },
        { "_3D_AUDIO_FOR_VIDEOS_STATUS", "2721" },
    };
}