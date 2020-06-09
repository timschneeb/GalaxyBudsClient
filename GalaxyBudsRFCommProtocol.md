## Galaxy Buds RFComm Protocol Notes

[TOC]

## Packets

### General notes

FrameType: if `payload size > 250` then `1` else `0`

ExtendedFrameType: if `payload size > 250` then `2` else `1` (byte)

### Non-fragmented

| Name                                               | Value (hex) | Size                  |
| -------------------------------------------------- | ----------- | --------------------- |
| Preamble                                           | FE          | 1 byte                |
| Type (Req = 0 or Rsp = 1)<sup>[1]</sup>            | 0x          | 1 byte                |
| Payload size                                       | xx          | 1 byte                |
| Message ID                                         | xx          | 1 byte                |
| Message payload                                    | ...         | dynamic<sup>[2]</sup> |
| Checksum<sup>[3]</sup> (CRC16-CCITT)<sup>[4]</sup> | xx xx       | 2 byte                |
| Postamble                                          | EE          | 1 byte                |
> <sup>[1] </sup>maybe more types -> used by isFragment()
>
> <sup>[2]</sup> calculation: `x = payload size - 1 byte (msg_id) - 2 bytes (crc)` 
>
> <sup>[3]</sup> checksum of the Message ID and content
>
> <sup>[4]</sup> more details: <https://gist.github.com/ThePBone/435b625418945592d7a0a3f04adc67b0>

### Fragmented

Skipped for now; only used for large data transmissions (e.g. Firmware OTA, Core Dumps)

## Messages

### Send

#### MSG_ID_UPDATE_TIME

| Index | Name      | Description                                              | Size   |
| ----- | --------- | -------------------------------------------------------- | ------ |
| 0     | Timestamp | Milliseconds since 1970 as long byte buffer              | 8 byte |
| 8     | Timezone  | Timezone offset (amount of ms to add) as int byte buffer | 4 byte |

#### MSG_ID_MANAGER_INFO

| Index | Name            | Description                    | Size   |
| ----- | --------------- | ------------------------------ | ------ |
| 0     | *Hardcoded*     | Always contains 1              | 1 byte |
| 1     | IsSamsungDevice | Samsung device = 1, Other = 2  | 1 byte |
| 2     | AndroidSdk      | Android Version as SDK Integer | 1 byte |

Might be used to enable phone-exclusive functions

#### MSG_ID_VOICE_NOTI_STATUS

| Index | Name   | Description | Size   |
| ----- | ------ | ----------- | ------ |
| 0     | Status | 0 or 1      | 1 byte |

Used to prepare for client-based TTS voice notifications. Does not seem to have any effect

#### MSG_ID_NOTIFICATION_INFO

| Index | Name | Description                                          | Size   |
| ----- | ---- | ---------------------------------------------------- | ------ |
| 0     | ?    | 0 or 1; Probably used to play an internal beep sound | 1 byte |

Used for notifications. Does not seem to have any effect

#### MSG_ID_GAME_MODE

| Index | Name  | Description                                             | Size   |
| ----- | ----- | ------------------------------------------------------- | ------ |
| 0     | Mode? | Set to 2 if phone supports Game Mode, otherwise unknown | 1 byte |

Unknown effect. Only used on Samsung devices with Game Mode available.

#### MSG_ID_EQUALIZER

| Index | Name    | Description       | Size   |
| ----- | ------- | ----------------- | ------ |
| 0     | Enabled | 0 or 1; Enable EQ | 1 byte |
| 1     | Preset  | 0-10; Preset      | 1 byte |

##### Presets

| Preset ID | Description                        |
| --------- | ---------------------------------- |
| 0         | Bass boost (optimized for Dolby)   |
| 1         | Soft (optimized for Dolby)         |
| 2         | Dynamic (optimized for Dolby)      |
| 3         | Clear (optimized for Dolby)        |
| 4         | Treble boost (optimized for Dolby) |
| 5         | Bass boost                         |
| 6         | Soft                               |
| 7         | Dynamic                            |
| 8         | Clear                              |
| 9         | Treble boost                       |

#### MSG_ID_LOCK_TOUCHPAD

| Index | Name    | Description           | Size   |
| ----- | ------- | --------------------- | ------ |
| 0     | Enabled | 0 or 1; Touchpad lock | 1 byte |

#### MSG_ID_SET_AMBIENT_MODE

| Index | Name    | Description                  | Size   |
| ----- | ------- | ---------------------------- | ------ |
| 0     | Enabled | 0 or 1; Enable ambient sound | 1 byte |

#### MSG_ID_AMBIENT_VOICE_FOCUS

| Index | Name | Description                   | Size   |
| ----- | ---- | ----------------------------- | ------ |
| 0     | Type | 0 or 1; Default or VoiceFocus | 1 byte |

#### MSG_ID_AMBIENT_VOLUME

| Index | Name   | Description         | Size   |
| ----- | ------ | ------------------- | ------ |
| 0     | Volume | 1-5; Ambient volume | 1 byte |

#### MSG_ID_MUTE_EARBUD

| Index | Name            | Description  | Size   |
| ----- | --------------- | ------------ | ------ |
| 0     | LeftMuteStatus  | 0 or 1; Mute | 1 byte |
| 1     | RightMuteStatus | 0 or 1; Mute | 1 byte |

Only available in FindMyGear mode

#### MSG_ID_SET_TOUCHPAD_OPTION

| Index | Name        | Description                               | Size   |
| ----- | ----------- | ----------------------------------------- | ------ |
| 0     | LeftOption  | ID of Action to be set on the left device | 1 byte |
| 1     | RightOption | ID of Action to be set on the left device                                          | 1 byte |

##### Available Actions

| ID   | Description                                          |
| ---- | ---------------------------------------------------- |
| 0    | Voice assistant                                      |
| 1    | Quick ambient sound                                  |
| 2    | Volume (if set on left: volume down, else volume up) |
| 3    | Ambient sound                                        |
| 4    | Spotify SpotOn (requires client compatibility)       |
| 5    | Other... (left only)                                 |
| 6    | Other... (right only)                                |

#### MSG_ID_MAIN_CHANGE

| Index | Name   | Description                                     | Size   |
| ----- | ------ | ----------------------------------------------- | ------ |
| 0     | Device | Hand-over main connection (1 = left, 0 = right) | 1 byte |

#### Parameter-less messages

| Message ID                   | Description                                                  |
| ---------------------------- | ------------------------------------------------------------ |
| MSG_ID_FIND_MY_EARBUDS_START | Starts playing loud beeping sounds                           |
| MSG_ID_FIND_MY_EARBUDS_STOP  | Stops playing and return to the default BT audio             |
| MSG_ID_DEBUG_SERIAL_NUMBER   | Returns serial number for both devices                       |
| MSG_ID_RESET                 | Resets device to factory defaults and responds with a result code |
| MSG_ID_DEBUG_GET_ALL_DATA    | Returns version data and sensor measurements                 |
| MSG_ID_DEBUG_BUILD_INFO      | Returns device string for both devices                       |
| MSG_ID_LOG_SESSION_OPEN      | Open logging session and disconnect the music stream         |
| MSG_ID_LOG_SESSION_CLOSE     | Close logging session and resume the music stream            |
| MSG_ID_BATTERY_TYPE      | Returns battery type strings for both devices                |

### Receive

#### MSG_ID_EXTENDED_STATUS_UPDATED

| Index | Name                   | Description                                               | Size   |
| ----- | ---------------------- | --------------------------------------------------------- | ------ |
| 0     | VersionOfMR            | 1 or 2; Also used to check for the new ambient module     | 1 byte |
| 1     | EarType                | 0 or 1; Unused<sup>[1] </sup>                             | 1 byte |
| 2     | DeviceBatGageL         | 0-100%; Battery (L)                                       | 1 byte |
| 3     | DeviceBatGageR         | 0-100%; Battery (R)                                       | 1 byte |
| 4     | TwsStatus              | 0 or 1; Coupled device status<sup>[1] </sup>              | 1 byte |
| 5     | MainConnection         | 0 (R) or 1 (L); Current main connection<sup>[1] </sup>    | 1 byte |
| 6     | WearingStatus          | 0 (none), 1 (L), 16 (R), 17 (both)<sup>[1] </sup>         | 1 byte |
| 7     | AmbientSoundEnable     | 0 or 1; Is ambient sound enabled<sup>[1] </sup>           | 1 byte |
| 8     | AmbientSoundType       | 0 or 1; Is voice focus mode enabled<sup>[1] </sup>        | 1 byte |
| 9     | AmbientSoundVolume     | Ambient sound volume<sup>[2] </sup>                       | 1 byte |
| 10    | EqualizerEnable        | 0 or 1; Is EQ enabled<sup>[1] </sup>                      | 1 byte |
| 11    | EqualizerType          | 0-10<sup>[3]</sup>; EQ type                               | 1 byte |
| 12    | \<dynamic\>            | Touch lock state (+ one option slot, if byte 13 not used) | 1 byte |
| 13    | \<dynamic\> (optional) | Touch option (L and R)<sup>[4]</sup>                      | 1 byte |

> <sup>[1] </sup>additional code: -1 (test code)
>
> <sup>[2] </sup>post-calculation: `int saved = vol + -1;` -> expected range [1;5]
>
> <sup>[3] </sup>range: [0;10] -> `if (type >= 5) type -= 5;` -> actual range: [0;5]
>
> <sup>[4]</sup>byte 13 is only required for touch actions that can set two different actions for L and R long press at the same time. If only one option slot is sufficent, byte 13 is not transmitted and the single action is stored in byte 12 alongside the touch lock state which can be either 0 or 1.

The device probably expects a response. The same SPPMessage should be sent back (response type) and after that, you should transmit basic (android) manager app information. (Refer to [`MSG_ID_MANAGER_INFO`](#MSG_ID_MANAGER_INFO))

##### Byte 12 and 13

**Single touch option**

* Byte 13 is not transmitted, instead the option is stored inside byte 12
* Used by 'volume button preset'

```java
byte lock_touchpad_status = (sppMessage.getParameters()[12] & 240) >> 4;
byte touchpad_option = sppMessage.getParameters()[12] & 15;
storeLeftTouchpadOption(touchpad_option);
```

**Double touch option**

* Byte 13 is transmitted, both options are stored inside
* The touch lock state remains in byte 12

```java
byte lock_touchpad_status = sppMessage.getParameters()[12] & 255;
byte touchpad_option_l = (sppMessage.getParameters()[13] & 240) >> 4;
byte touchpad_option_r = sppMessage.getParameters()[13] & 15;
```

_____

#### MSG_ID_STATUS_UPDATED

| Index | Name           | Description                                            | Size   |
| ----- | -------------- | ------------------------------------------------------ | ------ |
| 0     | EarType        | 0 or 1; Unused<sup>[1] </sup>                          | 1 byte |
| 1     | DeviceBatGageL | 0-100%; Battery (L)                                    | 1 byte |
| 2     | DeviceBatGageR | 0-100%; Battery (R)                                    | 1 byte |
| 3     | TwsStatus      | 0 or 1; Coupled device status<sup>[1] </sup>           | 1 byte |
| 4     | MainConnection | 0 (R) or 1 (L); Current main connection<sup>[1] </sup> | 1 byte |
| 5     | WearingStatus  | 0 (none), 1 (L), 16 (R), 17 (both)<sup>[1] </sup>      | 1 byte |


> <sup>[1] </sup>additional code: -1 (test code)

The device probably expects a response. The same SPPMessage should be sent back (response type) and after that, you should transmit basic (android) manager app information. (Refer to [`MSG_ID_MANAGER_INFO`](#MSG_ID_MANAGER_INFO))

#### MSG_ID_RESP

| Index | Name             | Description                                               | Size   |
| ----- | ---------------- | --------------------------------------------------------- | ------ |
| 0     | Action           | Original ID of message sent to the earbuds                | 1 byte |
| 1     | ResultCode       | Result/error code (0 = Success)                           | 1 byte |
| 2     | ExtraData (opt.) | Only used if the MainConnection has been manually changed | 2 byte |

Generic response. Android app always sends a time update if received.

##### Exceptions

[UNCHECKED] If byte 0 corresponds to 112, then we're looking at the response message of a manual MainConnection change. In this case, byte 2 describes which earpiece is declared as the main connection 0 (R) or 1 (L).

## Other notes

* (Extended) Status Update requires (?) RESP with result code
