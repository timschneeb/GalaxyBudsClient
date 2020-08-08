# Galaxy Buds Client
An unofficial Galaxy Buds Manager for Windows (Buds/Buds+)


**This README is also available in [Korean](/README_kor.md)!**

(You can find downloads in the [release tab](https://github.com/thepbone/galaxybudsclient/releases))

<p align="center">
  <img src="screenshots/screencap.gif">
</p>

This Client is a product of my research on the custom RFComm Serial Protocol the Buds use to receive and send binary (configuration) data. If you are interested in the structure of the protocol and its serial messages, I recommend you to check my notes out which I took while reverse-engineering the whole thing:

* [My Buds (2019) Notes](GalaxyBudsRFCommProtocol.md)
* [My Buds Plus Notes](Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md)

## Features

**New features** (in addition to the existing ones):

* Touchpad: Customizable tap-and-hold actions (launch application, toggle equalizer, change ambient volume, ...)<sup>[1]</sup>
* Resume media playback if Buds are worn
* Systray context menu with battery statistics
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
* Equalizer: unlock 'Optimize for Dolby' feature
* Touchpad: Combine Volume Up/Down with other options<sup>[1]</sup>

> <sup>[1]</sup> Note that the Wearable app will automatically reset this feature when attempting to switch Touchpad options in their app

## Installation

**This app requires [.Net Framework](https://dotnet.microsoft.com/download/dotnet-framework/net461) 4.6.1 or higher**

You can [**download**](https://github.com/ThePBone/GalaxyBudsClient/releases) a fully automated Setup in the [**release section**](https://github.com/ThePBone/GalaxyBudsClient/releases) of this repo!

The original **Galaxy Buds (2019)** and **Galaxy Buds Plus** are fully supported.

![Downloads](https://img.shields.io/github/downloads/ThePBone/GalaxyBudsClient/total)

Alternatively, you can also use the [chocolatey](https://chocolatey.org/courses/getting-started/what-is-chocolatey) package provided by [@superbonaci](https://github.com/superbonaci):

```
choco install galaxybudsclient
```

## Translators

* [@Florize](https://github.com/Florize) - Korean and Japanese translation
* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Russian and Ukrainian translation

## Contributors

* [@superbonaci](https://github.com/superbonaci) - Chocolatey package
* [@githubcatw](https://github.com/githubcatw) - Connection dialog base



___

Bitcoin: 3EawSB3NfX6JQxKBBFYh6ZwHDWXtJB84Ly

