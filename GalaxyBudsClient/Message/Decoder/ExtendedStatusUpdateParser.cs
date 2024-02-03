using System;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Decoder
{
    public class ExtendedStatusUpdateParser : BaseMessageParser, IBasicStatusUpdate
    {

        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.EXTENDED_STATUS_UPDATED;

        public int Revision { set; get; }
        public int EarType { set; get; }
        public int BatteryL { set; get; }
        public int BatteryR { set; get; }
        public bool IsCoupled { set; get; }
        public DeviceInv MainConnection { set; get; }
        public WearStates WearState { set; get; }
        public int EqualizerMode { set; get; }
        public bool TouchpadLock { set; get; }
        public TouchOptions TouchpadOptionL { set; get; }
        public TouchOptions TouchpadOptionR { set; get; }
        public bool SeamlessConnectionEnabled { set; get; }


        [Device(new[] { Models.Buds, Models.BudsPlus })]
        public bool AmbientSoundEnabled { set; get; }
        [Device(new[] { Models.Buds, Models.BudsPlus, Models.BudsPro, Models.Buds2 })]
        public int AmbientSoundVolume { set; get; }


        [Device(Models.Buds)]
        public AmbientType AmbientSoundMode { set; get; }
        [Device(Models.Buds)]
        public bool EqualizerEnabled { set; get; }


        [Device(new[] { Models.BudsPlus, Models.BudsLive, Models.BudsPro, Models.Buds2 })]
        public PlacementStates PlacementL { set; get; }
        [Device(new[] { Models.BudsPlus, Models.BudsLive, Models.BudsPro, Models.Buds2 })]
        public PlacementStates PlacementR { set; get; }
        [Device(new[] { Models.BudsPlus, Models.BudsLive, Models.BudsPro, Models.Buds2 })]
        public int BatteryCase { set; get; }
        [Device(new[] { Models.BudsPlus, Models.BudsPro, Models.Buds2 })]
        public bool OutsideDoubleTap { set; get; }
        [Device(new[] { Models.BudsPlus, Models.BudsLive, Models.BudsPro, Models.Buds2 })]
        public Color DeviceColor { set; get; }


        [Device(new []{ Models.BudsPlus, Models.BudsLive, Models.BudsPro, Models.Buds2 })]
        public bool AdjustSoundSync { set; get; }
        [Device(new []{ Models.BudsPlus, Models.BudsPro, Models.BudsPro, Models.Buds2 })]
        public bool SideToneEnabled { set; get; }
        [Device(new []{ Models.BudsPlus, Models.BudsPro, Models.Buds2 })]
        public bool ExtraHighAmbientEnabled { set; get; }


        [Device(Models.BudsLive)]
        public bool RelieveAmbient { set; get; }
        [Device(new []{ Models.BudsLive, Models.BudsPro, Models.Buds2 })]
        public bool VoiceWakeUp { set; get; }
        [Device(new []{ Models.BudsLive, Models.BudsPro, Models.Buds2 })]
        public int VoiceWakeUpLang { set; get; }
        [Device(new []{ Models.BudsLive, Models.BudsPro, Models.Buds2 })]
        public int FmmRevision { set; get; }
        [Device(Models.BudsLive)]
        public bool NoiseCancelling { set; get; }


        [Device(new []{ Models.BudsPro, Models.Buds2 })]
        public NoiseControlMode NoiseControlMode { set; get; }
        [Device(new []{ Models.BudsPro, Models.Buds2 })]
        public bool NoiseControlTouchOff { set; get; }
        [Device(new []{ Models.BudsPro, Models.Buds2 })]
        public bool NoiseControlTouchAnc { set; get; }
        [Device(new []{ Models.BudsPro, Models.Buds2 })]
        public bool NoiseControlTouchAmbient { set; get; }
        
        
        [Device(new []{ Models.Buds2 })]
        public bool NoiseControlTouchLeftOff { set; get; }
        [Device(new []{ Models.Buds2 })]
        public bool NoiseControlTouchLeftAnc { set; get; }
        [Device(new []{ Models.Buds2 })]
        public bool NoiseControlTouchLeftAmbient { set; get; }
        
        
        [Device(new []{ Models.BudsPro, Models.Buds2 })]
        public bool SpeakSeamlessly { set; get; }

        [Device(new []{ Models.BudsPro, Models.Buds2 })]
        public byte NoiseReductionLevel { set; get; }
        [Device(Models.BudsPro)]
        public bool AutoSwitchAudioOutput { set; get; }

        [Device(Models.BudsPro)]
        public bool DetectConversations { set; get; } = true;
        [Device(Models.BudsPro)]
        public byte DetectConversationsDuration { set; get; }
        [Device(Models.BudsPro)]
        public bool SpatialAudio { set; get; }
        
        [Device(new []{ Models.BudsLive, Models.BudsPro, Models.Buds2 })]
        public byte HearingEnhancements { set; get; }
        
        [Device(Models.Buds2)]
        public bool SingleTapOn { set; get; }
        [Device(Models.Buds2)]
        public bool DoubleTapOn { set; get; }
        [Device(Models.Buds2)]
        public bool TripleTapOn { set; get; }
        [Device(Models.Buds2)]
        public bool TouchHoldOn { set; get; }

        [Device(Models.Buds2)]
        public byte TouchType { set; get; }

        [Device(new []{ Models.Buds2 })]
        public bool AncWithOneEarbud { set; get; }
        [Device(new []{ Models.BudsPro, Models.Buds2 })]
        public bool NoiseControlsWithOneEarbud { set; get; }
        
        [Device(new []{ Models.BudsPro, Models.Buds2 })]
        public bool AmbientCustomVolumeOn { set; get; }
        [Device(new []{ Models.BudsPro, Models.Buds2 })]
        public byte AmbientCustomVolumeLeft { set; get; }
        [Device(new []{ Models.BudsPro, Models.Buds2 })]
        public byte AmbientCustomVolumeRight { set; get; }
        [Device(new []{ Models.BudsPro, Models.Buds2 })]
        public byte AmbientCustomSoundTone { set; get; }


        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            if (ActiveModel == Models.Buds)
            {
                Revision = msg.Payload[0];
                EarType = msg.Payload[1];
                BatteryL = msg.Payload[2];
                BatteryR = msg.Payload[3];
                IsCoupled = Convert.ToBoolean(msg.Payload[4]);
                MainConnection = (DeviceInv)msg.Payload[5];
                WearState = (WearStates)msg.Payload[6];
                AmbientSoundEnabled = Convert.ToBoolean(msg.Payload[7]);
                AmbientSoundMode = (AmbientType)msg.Payload[8];
                AmbientSoundVolume = msg.Payload[9];
                EqualizerEnabled = Convert.ToBoolean(msg.Payload[10]);
                EqualizerMode = msg.Payload[11];

                if (msg.Size > 13)
                {
                    TouchpadLock = Convert.ToBoolean(msg.Payload[12]);
                    TouchpadOptionL = DeviceSpec.TouchMap.FromByte((byte)((msg.Payload[13] & 0xF0) >> 4));
                    TouchpadOptionR = DeviceSpec.TouchMap.FromByte((byte)(msg.Payload[13] & 0x0F));
                    if (Revision >= 3)
                    {
                        SeamlessConnectionEnabled = msg.Payload[14] == 0;
                    }
                }
                else
                {
                    TouchpadLock = Convert.ToBoolean((msg.Payload[12] & 0xF0) >> 4);
                    TouchpadOptionL = DeviceSpec.TouchMap.FromByte((byte)(msg.Payload[12] & 0x0F));
                    TouchpadOptionR = DeviceSpec.TouchMap.FromByte((byte)(msg.Payload[12] & 0x0F));
                    if (Revision >= 3)
                    {
                        SeamlessConnectionEnabled = msg.Payload[13] == 0;
                    }
                }
            }
            else
            {
                Revision = msg.Payload[0];
                EarType = msg.Payload[1];
                BatteryL = msg.Payload[2];
                BatteryR = msg.Payload[3];
                IsCoupled = Convert.ToBoolean(msg.Payload[4]);
                MainConnection = (DeviceInv)msg.Payload[5];

                PlacementL = (PlacementStates)((msg.Payload[6] & 240) >> 4);
                PlacementR = (PlacementStates)(msg.Payload[6] & 15);
                if (PlacementL == PlacementStates.Wearing && PlacementR == PlacementStates.Wearing)
                    WearState = WearStates.Both;
                else if (PlacementL == PlacementStates.Wearing)
                    WearState = WearStates.L;
                else if (PlacementR == PlacementStates.Wearing)
                    WearState = WearStates.R;
                else
                    WearState = WearStates.None;

                BatteryCase = msg.Payload[7];

                if (ActiveModel == Models.BudsPlus)
                {
                    AmbientSoundEnabled = Convert.ToBoolean(msg.Payload[8]);
                    AmbientSoundVolume = msg.Payload[9];

                    AdjustSoundSync = msg.Payload[10] == 1;
                    EqualizerMode = msg.Payload[11];
                    TouchpadLock = Convert.ToBoolean(msg.Payload[12]);

                    TouchpadOptionL = DeviceSpec.TouchMap.FromByte((byte)((msg.Payload[13] & 240) >> 4));
                    TouchpadOptionR = DeviceSpec.TouchMap.FromByte((byte)(msg.Payload[13] & 15));

                    OutsideDoubleTap = msg.Payload[14] == 1;

                    short leftColor = BitConverter.ToInt16(msg.Payload, 15);
                    short rightColor = BitConverter.ToInt16(msg.Payload, 17);
                    DeviceColor = (Color)(leftColor != rightColor ? 0 : leftColor);

                    if (Revision >= 8)
                    {
                        SideToneEnabled = msg.Payload[19] == 1;
                    }

                    if (Revision >= 9)
                    {
                        ExtraHighAmbientEnabled = msg.Payload[20] == 1;
                    }

                    if (Revision >= 11)
                    {
                        SeamlessConnectionEnabled = msg.Payload[21] == 0;
                    }
                }
                else if (ActiveModel == Models.BudsLive)
                {
                    AdjustSoundSync = msg.Payload[8] == 1;
                    EqualizerMode = msg.Payload[9];
                    TouchpadLock = Convert.ToBoolean(msg.Payload[10]);

                    TouchpadOptionL = DeviceSpec.TouchMap.FromByte((byte)((msg.Payload[11] & 240) >> 4));
                    TouchpadOptionR = DeviceSpec.TouchMap.FromByte((byte)(msg.Payload[11] & 15));

                    NoiseCancelling = msg.Payload[12] == 1;
                    VoiceWakeUp = msg.Payload[13] == 1;

                    short leftColor = BitConverter.ToInt16(msg.Payload, 14);
                    short rightColor = BitConverter.ToInt16(msg.Payload, 16);
                    DeviceColor = (Color)(leftColor != rightColor ? 0 : leftColor);

                    VoiceWakeUpLang = msg.Payload[18];

                    if (Revision >= 3)
                    {
                        SeamlessConnectionEnabled = msg.Payload[19] == 0;
                    }

                    if (Revision >= 4)
                    {
                        FmmRevision = msg.Payload[20];
                    }

                    if (Revision >= 5)
                    {
                        RelieveAmbient = msg.Payload[21] == 1;
                    }

                    if (Revision >= 7)
                    {
                        HearingEnhancements = msg.Payload[22];
                    }
                }
                else if (ActiveModel == Models.BudsPro)
                {
                    AdjustSoundSync = msg.Payload[8] == 1;
                    EqualizerMode = msg.Payload[9];
                    TouchpadLock = Convert.ToBoolean(msg.Payload[10]);

                    TouchpadOptionL = DeviceSpec.TouchMap.FromByte((byte)((msg.Payload[11] & 240) >> 4));
                    TouchpadOptionR = DeviceSpec.TouchMap.FromByte((byte)(msg.Payload[11] & 15));

                    NoiseControlMode = (NoiseControlMode)msg.Payload[12];
                    VoiceWakeUp = msg.Payload[13] == 1;

                    short leftColor = BitConverter.ToInt16(msg.Payload, 14);
                    short rightColor = BitConverter.ToInt16(msg.Payload, 16);
                    DeviceColor = (Color)(leftColor != rightColor ? 0 : leftColor);

                    VoiceWakeUpLang = msg.Payload[18];
                    SeamlessConnectionEnabled = msg.Payload[19] == 0;
                    FmmRevision = msg.Payload[20];

                    NoiseControlTouchOff = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 0) == 1;
                    NoiseControlTouchAmbient = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 1) == 2;
                    NoiseControlTouchAnc = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 2) == 4;

                    if (Revision >= 8)
                    {
                        NoiseControlTouchLeftOff = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 4) == 16;
                        NoiseControlTouchLeftAmbient = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 5) == 32;
                        NoiseControlTouchLeftAnc = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 6) == 64;
                    }

                    if (Revision < 3)
                    {
                        ExtraHighAmbientEnabled = msg.Payload[22] == 1;
                    }
                    else
                    {
                        SpeakSeamlessly = msg.Payload[22] == 1;
                    }

                    AmbientSoundVolume = msg.Payload[23];
                    NoiseReductionLevel = msg.Payload[24];
                    AutoSwitchAudioOutput = msg.Payload[25] == 1;
                    DetectConversations = msg.Payload[26] == 1;
                    DetectConversationsDuration = msg.Payload[27];
                    if (DetectConversationsDuration > 2)
                    {
                        DetectConversationsDuration = 1;
                    }

                    if (Revision > 1)
                    {
                        SpatialAudio = msg.Payload[28] == 1;
                    }

                    if (Revision >= 5)
                    {
                        HearingEnhancements = msg.Payload[29];
                    }

                    if (Revision >= 6)
                    {
                        ExtraHighAmbientEnabled = msg.Payload[30] == 1;
                    }

                    if (Revision >= 7)
                    {
                        OutsideDoubleTap = msg.Payload[31] == 1;
                    }

                    if (Revision >= 8)
                    {
                        NoiseControlsWithOneEarbud = msg.Payload[32] == 1;
                        AmbientCustomVolumeOn = msg.Payload[33] == 1;
                        AmbientCustomVolumeLeft = ByteArrayUtils.ValueOfLeft(msg.Payload[34]);
                        AmbientCustomVolumeRight = ByteArrayUtils.ValueOfRight(msg.Payload[34]);
                        AmbientCustomSoundTone = msg.Payload[35];
                    }

                    if (Revision >= 9)
                    {
                        SideToneEnabled = msg.Payload[36] == 1;
                    }
                }
                else if (ActiveModel == Models.Buds2)
                {
                    AdjustSoundSync = msg.Payload[8] == 1;
                    EqualizerMode = msg.Payload[9];

                    if (Revision < 4)
                    {
                        TouchpadLock = msg.Payload[10] == 1;
                    }
                    else
                    {
                        TouchHoldOn = (msg.Payload[10] & (1 << 0)) == 1;
                        TripleTapOn = (msg.Payload[10] & (1 << 1)) == 2;
                        DoubleTapOn = (msg.Payload[10] & (1 << 2)) == 4;
                        SingleTapOn = (msg.Payload[10] & (1 << 3)) == 8;
                        TouchpadLock = (msg.Payload[10] & (1 << 7)) != 128;
                    }

                    TouchpadOptionL = DeviceSpec.TouchMap.FromByte((byte)((msg.Payload[11] & 240) >> 4));
                    TouchpadOptionR = DeviceSpec.TouchMap.FromByte((byte)(msg.Payload[11] & 15));

                    NoiseControlMode = (NoiseControlMode)msg.Payload[12];
                    VoiceWakeUp = msg.Payload[13] == 1;

                    short leftColor = BitConverter.ToInt16(msg.Payload, 14);
                    short rightColor = BitConverter.ToInt16(msg.Payload, 16);
                    DeviceColor = (Color)(leftColor != rightColor ? 0 : leftColor);

                    VoiceWakeUpLang = msg.Payload[18];
                    SeamlessConnectionEnabled = msg.Payload[19] == 0;
                    FmmRevision = msg.Payload[20];

                    NoiseControlTouchOff = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 0) == 1;
                    NoiseControlTouchAmbient = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 1) == 2;
                    NoiseControlTouchAnc = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 2) == 4;

                    if (Revision >= 5)
                    {
                        NoiseControlTouchLeftOff = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 4) == 16;
                        NoiseControlTouchLeftAmbient = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 5) == 32;
                        NoiseControlTouchLeftAnc = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 6) == 64;
                    }

                    SpeakSeamlessly = msg.Payload[22] == 1;
                    AmbientSoundVolume = msg.Payload[23];
                    NoiseReductionLevel = msg.Payload[24];
                    HearingEnhancements = msg.Payload[25];
                    ExtraHighAmbientEnabled = msg.Payload[26] == 1;

                    if (Revision >= 2)
                    {
                        TouchType = msg.Payload[27];
                    }

                    if (Revision >= 3)
                    {
                        AncWithOneEarbud = msg.Payload[28] == 1;
                        NoiseControlsWithOneEarbud = Revision >= 5 && AncWithOneEarbud;
                    }

                    if (Revision >= 5)
                    {
                        AmbientCustomVolumeOn = msg.Payload[29] == 1;
                        AmbientCustomVolumeLeft = ByteArrayUtils.ValueOfLeft(msg.Payload[30]);
                        AmbientCustomVolumeRight = ByteArrayUtils.ValueOfRight(msg.Payload[30]);
                        AmbientCustomSoundTone = msg.Payload[31];
                        OutsideDoubleTap = msg.Payload[32] == 1;
                    }

                    if (Revision >= 6)
                    {
                        SideToneEnabled = msg.Payload[33] == 1;
                    }
                }
                else if (ActiveModel == Models.Buds2Pro)
                {
                    AdjustSoundSync = msg.Payload[8] == 1;
                    EqualizerMode = msg.Payload[9];

                    TouchHoldOn = (msg.Payload[10] & (1 << 0)) == 1;
                    TripleTapOn = (msg.Payload[10] & (1 << 1)) == 2;
                    DoubleTapOn = (msg.Payload[10] & (1 << 2)) == 4;
                    SingleTapOn = (msg.Payload[10] & (1 << 3)) == 8;
                    TouchpadLock = (msg.Payload[10] & (1 << 7)) != 128;

                    TouchpadOptionL = DeviceSpec.TouchMap.FromByte((byte)((msg.Payload[11] & 240) >> 4));
                    TouchpadOptionR = DeviceSpec.TouchMap.FromByte((byte)(msg.Payload[11] & 15));

                    NoiseControlMode = (NoiseControlMode)msg.Payload[12];
                    VoiceWakeUp = msg.Payload[13] == 1;

                    short leftColor = BitConverter.ToInt16(msg.Payload, 14);
                    short rightColor = BitConverter.ToInt16(msg.Payload, 16);
                    DeviceColor = (Color)(leftColor != rightColor ? 0 : leftColor);

                    VoiceWakeUpLang = msg.Payload[18];
                    SeamlessConnectionEnabled = msg.Payload[19] == 0;
                    FmmRevision = msg.Payload[20];

                    NoiseControlTouchOff = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 0) == 1;
                    NoiseControlTouchAmbient = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 1) == 2;
                    NoiseControlTouchAnc = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 2) == 4;

                    NoiseControlTouchLeftOff = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 4) == 16;
                    NoiseControlTouchLeftAmbient = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 5) == 32;
                    NoiseControlTouchLeftAnc = ByteArrayUtils.ValueOfBinaryDigit(msg.Payload[21], 6) == 64;
  
                    SpeakSeamlessly = msg.Payload[22] == 1;
                    AmbientSoundVolume = msg.Payload[23];
                    NoiseReductionLevel = msg.Payload[24];
                    HearingEnhancements = msg.Payload[25];
                    
                    DetectConversations = msg.Payload[26] == 1;
                    DetectConversationsDuration = msg.Payload[27];
                    if (DetectConversationsDuration > 2)
                    {
                        DetectConversationsDuration = 1;
                    }
                    
                    AncWithOneEarbud = msg.Payload[28] == 1;
                    NoiseControlsWithOneEarbud = msg.Payload[28] == 1;
                    
                    AmbientCustomVolumeOn = msg.Payload[29] == 1;
                    AmbientCustomVolumeLeft = ByteArrayUtils.ValueOfLeft(msg.Payload[30]);
                    AmbientCustomVolumeRight = ByteArrayUtils.ValueOfRight(msg.Payload[30]);
                    AmbientCustomSoundTone = msg.Payload[31];
                    OutsideDoubleTap = msg.Payload[32] == 1;
  
                    SideToneEnabled = msg.Payload[33] == 1;
                    
                    SpatialAudio = msg.Payload[34] == 1;
                }
            }
        }
    }
}
