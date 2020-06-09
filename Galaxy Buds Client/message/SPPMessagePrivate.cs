namespace Galaxy_Buds_Client.message
{
    public partial class SPPMessage
    {
        public enum MsgType : byte
        {
            INVALID = 255,
            Request = 0, 
            Response = 1
        }

        public enum Constants : byte
        {
            SOM = 0xFE,
            EOM = 0xEE
        }

        public enum MessageIds : byte
        {
            MSG_ID_BATTERY_TYPE = 148,
            MSG_ID_A2DP_VOLUME_UPDATED = 131,
            MSG_ID_AMBIENT_MODE_UPDATED = 129,
            MSG_ID_AMBIENT_VOICE_FOCUS = 133,
            MSG_ID_AMBIENT_VOLUME = 132,
            MSG_ID_AMBIENT_WEARING_STATUS_UPDATED = 137,
            MSG_ID_CONNECTION_UPDATED = 98,
            MSG_ID_DEBUG_BUILD_INFO = 40,
            MSG_ID_DEBUG_ERROR = 34,
            MSG_ID_DEBUG_GET_ALL_DATA = 38,
            MSG_ID_DEBUG_GET_PE_RSSI = 35,
            MSG_ID_DEBUG_GET_VERSION = 36,
            MSG_ID_DEBUG_MODE_LOG = 33,
            MSG_ID_DEBUG_SERIAL_NUMBER = 41,
            MSG_ID_EQUALIZER = 134,
            MSG_ID_EXTENDED_STATUS_UPDATED = 97,
            MSG_ID_FIND_MY_EARBUDS_START = 160,
            MSG_ID_FIND_MY_EARBUDS_STOP = 161,
            MSG_ID_FOTA_ABORT = 181,
            MSG_ID_FOTA_CONTROL = 177,
            MSG_ID_FOTA_DEVICE_INFO_SW_VERSION = 180,
            MSG_ID_FOTA_DOWNLOAD_DATA = 178,
            MSG_ID_FOTA_SESSION = 176,
            MSG_ID_FOTA_UPDATED = 179,
            MSG_ID_GAME_MODE = 135,
            MSG_ID_GET_MODE = 17,
            MSG_ID_HIDDEN_CMD_DATA = 19,
            MSG_ID_HIDDEN_CMD_MODE = 18,
            MSG_ID_LOCK_TOUCHPAD = 144,
            MSG_ID_LOG_COREDUMP_COMMIT_SUICIDE = 48,
            MSG_ID_LOG_COREDUMP_COMPLETE = 51,
            MSG_ID_LOG_COREDUMP_DATA = 50,
            MSG_ID_LOG_COREDUMP_DATA_DONE = 56,
            MSG_ID_LOG_COREDUMP_DATA_SIZE = 49,
            MSG_ID_LOG_SESSION_CLOSE = 59,
            MSG_ID_LOG_SESSION_OPEN = 58,
            MSG_ID_LOG_TRACE_COMPLETE = 54,
            MSG_ID_LOG_TRACE_DATA = 53,
            MSG_ID_LOG_TRACE_DATA_DONE = 57,
            MSG_ID_LOG_TRACE_ROLE_SWITCH = 55,
            MSG_ID_LOG_TRACE_START = 52,
            MSG_ID_MAIN_CHANGE = 112,
            MSG_ID_MANAGER_INFO = 136,
            MSG_ID_MUTE_EARBUD = 162,
            MSG_ID_MUTE_EARBUD_STATUS_UPDATED = 163,
            MSG_ID_NOTIFICATION_INFO = 166,
            MSG_ID_PROFILE_CONTROL = 113,
            MSG_ID_RESET = 80,
            MSG_ID_RESP = 81,
            MSG_ID_SELF_TEST = 171,
            MSG_ID_SET_A2DP_VOL = 130,
            MSG_ID_SET_AMBIENT_MODE = 128,
            MSG_ID_SET_DEBUG_MODE = 32,
            MSG_ID_SET_MODE_CHANGE = 16,
            MSG_ID_SET_TOUCHPAD_OPTION = 146,
            MSG_ID_SET_TOUCHPAD_OTHER_OPTION = 147,
            MSG_ID_SET_VOICE_CMD = 193,
            MSG_ID_SPP_ROLE_STATE = 115,
            MSG_ID_START_VOICE_RECORD = 168,
            MSG_ID_STATUS_UPDATED = 96,
            MSG_ID_TOUCH_UPDATED = 145,
            MSG_ID_UPDATE_TIME = 167,
            MSG_ID_UPDATE_VOICE_CMD = 192,
            MSG_ID_USAGE_REPORT = 64,
            MSG_ID_VOICE_NOTI_STATUS = 164,
            MSG_ID_VOICE_NOTI_STOP = 165,

            MSG_ID_DEBUGMODE_VERSION = 99
        }
    }
}
