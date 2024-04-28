using System;
using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Message.Parameter;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.UNIVERSAL_MSG_ID_ACKNOWLEDGEMENT)]
public class AcknowledgementDecoder : BaseMessageDecoder
{ 
    public MsgIds Id { set; get; }
    public byte[]? RawParameters { set; get; }
    public IAckParameter? Parameters { set; get; }

    public override void Decode(SppMessage msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream); 
        
        Id = (MsgIds)reader.ReadByte();
        try
        {
            RawParameters = msg.Payload[1..];
            
            // TODO: check which ACKs to handle in the ViewModels
            switch (Id)
            {
                case MsgIds.ADJUST_SOUND_SYNC:
                case MsgIds.SET_SIDETONE:
                case MsgIds.SET_ANC_WITH_ONE_EARBUD:
                case MsgIds.NECK_POSTURE_SET:
                case MsgIds.EXTRA_CLEAR_SOUND_CALL:
                case MsgIds.OUTSIDE_DOUBLE_TAP:
                case MsgIds.EXTRA_HIGH_AMBIENT:
                case MsgIds.SET_VOICE_WAKE_UP:
                case MsgIds.PASS_THROUGH:
                case MsgIds.AMBIENT_VOLUME:
                case MsgIds.NOISE_CONTROLS:
                case MsgIds.NOISE_REDUCTION_LEVEL:
                case MsgIds.SET_NOISE_REDUCTION:
                case MsgIds.EQUALIZER:
                case MsgIds.TAP_TEST_MODE:
                case MsgIds.SET_SPEAK_SEAMLESSLY:
                case MsgIds.SET_DETECT_CONVERSATIONS:
                case MsgIds.SET_DETECT_CONVERSATIONS_DURATION:
                    // Boolean or byte
                    Parameters = new SimpleAckParameter { Value = RawParameters[0] };
                    break;
                case MsgIds.SET_CALL_PATH_CONTROL:
                case MsgIds.SET_SEAMLESS_CONNECTION:
                    // Inverted boolean
                    Parameters = new SimpleAckParameter { Value = (byte)(RawParameters[0] == 0 ? 1 : 0) };
                    break;
                case MsgIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS:
                    Parameters = new TouchAndHoldNoiseControlsAckParameter(reader);
                    break;
                case MsgIds.CUSTOMIZE_AMBIENT_SOUND:
                    Parameters = new CustomizeSoundAckParameter(reader);
                    break;
                case MsgIds.LOCK_TOUCHPAD:
                    Parameters = new LockTouchpadAckParameter(reader);
                    break;
                case MsgIds.SET_TOUCHPAD_OPTION:
                    Parameters = new SetTouchpadOptionAckParameter(reader);
                    break;
                case MsgIds.MUTE_EARBUD:
                    Parameters = new MuteEarbudAckParameter(reader);
                    break;
                default:
                    Parameters = null;
                    break;
            }
        }
        catch(IndexOutOfRangeException) {}
    }
}