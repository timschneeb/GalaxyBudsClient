## Galaxy Buds+ RFComm Protocol Notes

These notes do not describe every aspect of Samsung's RFCOMM protocol. Check the sources of the Unofficial Galaxy Buds Client for more precise details.

[TOC]

## Packets

### Non-fragmented

| Name                                               | Value (hex) | Size                  |
| -------------------------------------------------- | ----------- | --------------------- |
| Preamble                                           | FD          | 1 byte                |
| Header<sup>[1]</sup>                               | xx xx       | 2 bytes               |
| Message ID                                         | xx          | 1 byte                |
| Message payload                                    | ...         | dynamic<sup>[2]</sup> |
| Checksum<sup>[3]</sup> (CRC16-CCITT)<sup>[4]</sup> | xx xx       | 2 byte                |
| Postamble                                          | DD          | 1 byte                |

> <sup>[1] </sup>contains encoded message/fragment type and size information
>
> <sup>[2]</sup> check header for size
>
> <sup>[3]</sup> checksum of the Message ID and content
>
> <sup>[4]</sup> more details: <https://gist.github.com/ThePBone/435b625418945592d7a0a3f04adc67b0>

#### Header

| Name         | Distribution (bin) | Size    |
| ------------ | ------------------ | ------- |
| Payload size | xxxxx000 00000000  | 11 bits |
| Is Fragment? | xxxx0xxx xxxxxxxx  | 1 bit   |
| Is Response? | xxx0xxxx xxxxxxxx  | 1 bit   |

### Fragmented

Skipped for now; only used for large data transmissions (e.g. Firmware OTA, Core Dumps, Trace Dumps)

## Messages

### Receive

#### MSG_ID_EXTENDED_STATUS_UPDATED

| Index | Name                   | Description                                     | Size   |
| ----- | ---------------------- | ----------------------------------------------- | ------ |
| 0     | Interface revision     | Interface version code of the current FW        | 1 byte |
| 1     | EarType                | 0 = Open or 1 = Kernel; Unused                  | 1 byte |
| 2     | DeviceBatGageL         | 0-100%; Battery (L)                             | 1 byte |
| 3     | DeviceBatGageR         | 0-100%; Battery (R)                             | 1 byte |
| 4     | Is Coupled             | 0 or 1; Coupled device status                   | 1 byte |
| 5     | MainConnection         | 0 (R) or 1 (L); Current main connection         | 1 byte |
| 6     | PlacementStatus        | MSB: Left, LSB: Right<sup>[1]</sup>             | 1 byte |
| 7     | BatteryCase            | 0-100%; Battery (Case)                          | 1 byte |
| 8     | AmbientSoundEnable     | 0 or 1; Is ambient sound enabled                | 1 byte |
| 9     | AmbientSoundType       | 0 or 1; Is voice focus mode enabled             | 1 byte |
| 10    | AdjustSoundSync        | Low latency (may require Samsung's prop. codec) | 1 byte |
| 11    | EqualizerMode          | 0 = Disable; 1-5 = Presets                      | 1 byte |
| 12    | TouchLock              | 0 or 1                                          | 1 byte |
| 13    | TouchOptions           | MSB: Left, LSB: Right                           | 1 byte |
| 14    | DeviceColor            | MSB: Left color, LSB: Right color               | 1 byte |
| 15    | SideToneEnable         | Enable ambient sound in calls                   | 1 byte |
| 16    | ExtraHighAmbientEnable | Enable extra volume step for ambient mode       | 1 byte |

> <sup>[1]</sup> values: unknown = 0, wearing = 1, not_wearing = 2, in_case = 3, in_closed_case = 4

The device probably expects a response. The same SPPMessage should be sent back (response type) and after that, you should transmit basic (android) manager app information. (Refer to [`MSG_ID_MANAGER_INFO`](#MSG_ID_MANAGER_INFO))

### Send

#### MSG_ID_UPDATE_TIME

| Index | Name      | Description                                              | Size   |
| ----- | --------- | -------------------------------------------------------- | ------ |
| 0     | Timestamp | Milliseconds since 1970 as long byte buffer              | 8 byte |
| 8     | Timezone  | Timezone offset (amount of ms to add) as int byte buffer | 4 byte |

#### MSG_ID_MANAGER_INFO

| Index | Name            | Description                                | Size   |
| ----- | --------------- | ------------------------------------------ | ------ |
| 0     | ClientType      | Wearable App = 1, Other codes are unknown  | 1 byte |
| 1     | IsSamsungDevice | Samsung device = 1, Other manufacturer = 2 | 1 byte |
| 2     | AndroidSdk      | Android Version as SDK Integer             | 1 byte |

Might have been used to enable phone-exclusive functions on older FWs

#### MSG_ID_VOICE_NOTI_STATUS

| Index | Name   | Description | Size   |
| ----- | ------ | ----------- | ------ |
| 0     | Status | 0 or 1      | 1 byte |

Prepare for client-based TTS voice notifications. The single-tap option will be temporarely overwritten to send `VOICE_NOTI_STOP` messages back to the client while enabled.

#### MSG_ID_NOTIFICATION_INFO

| Index | Name | Description                                          | Size   |
| ----- | ---- | ---------------------------------------------------- | ------ |
| 0     | ?    | 0 or 1; Probably used to play an internal beep sound | 1 byte |

Used for notifications. Does not seem to have any effect

#### MSG_ID_GAME_MODE

| Index | Name           | Description                                                  | Size   |
| ----- | -------------- | ------------------------------------------------------------ | ------ |
| 0     | Status (Multi) | If game is in foreground, flip fifth bit. If screen is on, flip first bit | 1 byte |
##### Left-over Enumeration

| ID       | Description        |
| -------- | ------------------ |
| 0        | Game Mode unknown  |
| 1        | Game Mode active   |
| 2        | Game Mode inactive |
| 255 (-1) | Game Mode max      |

This enum above seems to be defunct, but it might be still related to the unused Game Mode on the original Galaxy Buds (2019) -> more information required

Unknown effect. Only internally used on Samsung client devices with Game Mode available.

#### MSG_ID_EQUALIZER

| Index | Name   | Description      | Size   |
| ----- | ------ | ---------------- | ------ |
| 0     | Preset | 0-5; Preset/Mode | 1 byte |

##### Presets

| Preset ID | Description                        |
| --------- | ---------------------------------- |
| 0         | Disabled/Normal                    |
| 1         | Bass boost                         |
| 2         | Soft                               |
| 3         | Dynamic                            |
| 4         | Clear                              |
| 5         | Treble boost                       |

#### MSG_ID_LOCK_TOUCHPAD

| Index | Name    | Description           | Size   |
| ----- | ------- | --------------------- | ------ |
| 0     | Enabled | 0 or 1; Touchpad lock | 1 byte |

#### MSG_ID_SET_AMBIENT_MODE

| Index | Name    | Description                  | Size   |
| ----- | ------- | ---------------------------- | ------ |
| 0     | Enabled | 0 or 1; Enable ambient sound | 1 byte |

#### MSG_ID_AMBIENT_VOLUME

| Index | Name   | Description                                  | Size   |
| ----- | ------ | -------------------------------------------- | ------ |
| 0     | Volume | 0-3; Limited to 2 if extra level is disabled | 1 byte |

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
| 1     | RightOption | ID of Action to be set on the left device | 1 byte |

##### Available Actions

| ID      | Description                                          |
| ------- | ---------------------------------------------------- |
| 0       | Unused                                               |
| 1       | Voice Assistant                                      |
| 2       | Volume (if set on left: volume down, else volume up) |
| 3       | Ambient sound                                        |
| 4       | Spotify SpotOn (requires client compatibility)       |
| 5...255 | Other...                                             |

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
| MSG_ID_BATTERY_TYPE          | Should return battery type strings but on the Buds+ it return four zero-bytes |
| MSG_ID_DEBUG_SKU             | Returns zero-data (most of the time?). At least four bytes may contain information |
| MSG_ID_DEBUG_PE_RSSI         | Originally found in the Buds (2019) plugin app. Unlike the original Buds, the Buds+ actually respond to it but return 183 bytes of zero-data. |
| MSG_ID_SELF_TEST             | Runs self-test. Disconnects both Buds+ while performing the test for some reason. No reference to it in the Buds+ app, the information from the Buds (2019) app somewhat works but seems incomplete |

## Other notes

* Client should respond with one zero-byte in case of `VERSION_INFO`, `STATUS_UPDATED`, `EXTENDED_STATUS_UPDATED`
* Client should respond with `MANAGER_INFO` when `EXTENDED_STATUS_UPDATED` is received