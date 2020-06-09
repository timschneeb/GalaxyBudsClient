using System;
using Galaxy_Buds_Client.parser;

namespace Galaxy_Buds_Client.message
{
    public class SPPMessageParserFactory
    {

        private static readonly Type[] RegisteredParsers =
        {
            typeof(ExtendedStatusUpdateParser), typeof(StatusUpdateParser), typeof(SoftwareVersionOTAParser), typeof(UsageReportParser),
             typeof(SelfTestParser), typeof(GenericResponseParser), typeof(BatteryTypeParser), typeof(AmbientModeUpdateParser), typeof(DebugBuildInfoParser),
             typeof(DebugSerialNumberParser), typeof(DebugModeVersionParser), typeof(SppRoleStateParser), typeof(ResetResponseParser),
             typeof(AmbientVoiceFocusParser), typeof(AmbientVolumeParser), typeof(DebugGetAllDataParser), typeof(TouchUpdateParser),
             typeof(AmbientWearingUpdateParser), typeof(MuteUpdateParser), typeof(SetOtherOptionParser)
        };

        public static BaseMessageParser BuildParser(SPPMessage msg)
        {
            BaseMessageParser b = null;
            for (int i = 0; i < RegisteredParsers.Length; i++)
            {
                var act = (object)Activator.CreateInstance(RegisteredParsers[i]);
                if (act.GetType() == RegisteredParsers[i])
                {
                    BaseMessageParser parser = (BaseMessageParser)act;
                    if (parser.HandledType == msg.Id)
                    {
                        parser.ParseMessage(msg);
                        b = parser;
                        break;
                    }
                }
            }
            return b;
        }
    }
}
