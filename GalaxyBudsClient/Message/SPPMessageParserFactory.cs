using System;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Utils;
using Sentry;

namespace GalaxyBudsClient.Message
{
    public class SPPMessageParserFactory
    {

        private static readonly Type[] RegisteredParsers =
        {
            typeof(ExtendedStatusUpdateParser), typeof(StatusUpdateParser), typeof(SoftwareVersionOTAParser), typeof(UsageReportParser),
            typeof(SelfTestParser), typeof(GenericResponseParser), typeof(BatteryTypeParser), typeof(AmbientModeUpdateParser), typeof(DebugBuildInfoParser),
            typeof(DebugSerialNumberParser), typeof(DebugModeVersionParser), typeof(SppRoleStateParser), typeof(ResetResponseParser),
            typeof(AmbientVoiceFocusParser), typeof(AmbientVolumeParser), typeof(DebugGetAllDataParser), typeof(TouchUpdateParser),
            typeof(AmbientWearingUpdateParser), typeof(MuteUpdateParser), typeof(SetOtherOptionParser), typeof(BondedDevicesParser),
            typeof(DebugSkuParser), typeof(SetInBandRingtoneParser), typeof(NoiseReductionModeUpdateParser),
            typeof(LogTraceStartParser), typeof(LogTraceDataParser), typeof(LogCoredumpDataParser), typeof(LogCoredumpDataSizeParser),
            typeof(NoiseControlUpdateParser)
        };

        public static BaseMessageParser? BuildParser(SPPMessage msg)
        {
            BaseMessageParser? b = null;
            for (int i = 0; i < RegisteredParsers.Length; i++)
            {
                var act = Activator.CreateInstance(RegisteredParsers[i]);
                if (act?.GetType() == RegisteredParsers[i])
                {
                    BaseMessageParser parser = (BaseMessageParser)act;
                    if (parser.HandledType == msg.Id)
                    {
                        SentrySdk.ConfigureScope(scope =>
                        {
                            scope.SetTag("msg-data-available", "true");
                            scope.SetExtra("msg-type", msg.Type.ToString());
                            scope.SetExtra("msg-id", msg.Id);
                            scope.SetExtra("msg-size", msg.Size);
                            scope.SetExtra("msg-total-size", msg.TotalPacketSize);
                            scope.SetExtra("msg-crc16", msg.Crc16);
                            scope.SetExtra("msg-payload", HexUtils.Dump(msg.Payload, 512, false, false, false));
                        });

                        parser.ParseMessage(msg);
                        b = parser;
                        break;
                    }
                }
            }

            if (b != null)
            {
                foreach (var hook in ScriptManager.Instance.DecoderHooks)
                {
                    hook?.OnDecoderCreated(msg, ref b);
                }
            }

            return b;
        }
    }
}