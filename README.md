# Galaxy Buds Client
An unofficial Galaxy Buds Manager for Windows

<p align="center">
  <img src="screenshots/screencap.gif">
</p>

This Client is a product of my research on the custom RFComm Serial Protocol the Buds use to receive and send binary (configuration) data. If you are interested in the structure of the protocol and its serial messages, I recommend you to [check my notes out](GalaxyBudsRFCommProtocol.md) which I took while reverse-engineering the whole thing.

## Features

**New features** (in addition to the existing ones):

* Display detailed sensor statistics on the dashboard, this includes:
  * Voltage and Current of the in-built ADC (Analog-to-Digital converter) of both Earbuds
  * Temperature of both Earbuds
  * More precise battery percentage (instead of steps of 5)
* Perform a self-test with all on-board components
* Display various (debug) information, including:
  * Hardware Revision
  * (Touch) Firmware Version
  * Bluetooth Addresses of both Earbuds
  * Serial Numbers of both Earbuds
  * Firmware Build Info (Compile Date, Developer Name)
  * Battery Type
  * Other sensor data
* Equalizer: unlock 'Optimize for Dolby' feature<sup>[1]</sup>
* Touchpad: Combine Volume Up/Down with other options<sup>[2]</sup>

> <sup>[1]</sup> There are actually 10 EQ-Presets, not 5. If a Samsung Android device has Dolby enabled, then another set of presets will be used. It is unknown if those extra presets actually are different from the normal ones, but according to the Wearable app, these presets seem to be optimized for use with Dolby. (Note that the Wearable app will automatically reset this feature once connected to a non-Samsung mobile device (with Dolby disabled) when attempting to switch EQ presets in the app)
>
> <sup>[2]</sup> Note that the Wearable app will automatically reset this feature when attempting to switch Touchpad options in their app

## Installation

**This app requires .Net Framework 4.7.2 or higher** ([Download](https://dotnet.microsoft.com/download/dotnet-framework/net472))

You can find a fully automated Setup in the [release section](https://github.com/ThePBone/GalaxyBudsClient/releases) of this repo!

Designed for the original Galaxy Buds. **Galaxy Buds Plus are unsupported** since I do not own a pair.

___

In case somone wants to donate a buck or two ;-) 

Bitcoin: 3EawSB3NfX6JQxKBBFYh6ZwHDWXtJB84Ly

