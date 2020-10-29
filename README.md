# Galaxy Buds Client
An unofficial Galaxy Buds Manager for Windows (Buds, Buds+, Buds Live)


**This README is also available in [Korean](/README_kor.md), [Japanese](/README_jpn.md), [Russian](/README_rus.md) and [Ukrainian](/README_ukr.md)!**

(You can find downloads in the [release tab](https://github.com/thepbone/galaxybudsclient/releases))

<p align="center">
  <img src="screenshots/screencap.gif">
</p>

This Client is a product of my research on the custom RFComm Serial Protocol the Buds use to receive and send binary (configuration) data. If you are interested in the structure of the protocol and its serial messages, I recommend you to check my notes out which I took while reverse-engineering the whole thing:

* [Buds (2019) Notes](GalaxyBudsRFCommProtocol.md)
* [Buds Plus Notes](Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md)
* [Buds Plus: Undocumented calls](https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md)

## Features

**New features** (in addition to the existing ones):

* Touchpad: Customizable tap-and-hold actions (launch application, toggle equalizer, change ambient volume, ...)<sup>[1]</sup>
* Resume media playback if Buds are worn
* Tray-bar context menu with battery statistics
* Display detailed sensor statistics on the dashboard, this includes:
  * Voltage, current and temperature of the built-in ADC (Analog-to-Digital converter)
  * Precise battery percentage (instead of steps of 5 percent)
* Perform a self-test with all on-board components
* Display various (debug) information, including:
  * Hardware/Software/Touch revision
  * Bluetooth addresses, serial numbers
  * Firmware build info (compile date, developer name)
  * Battery type
  * Other sensor data
* Touchpad: Combine Volume Up/Down with other options<sup>[1]</sup>
* Equalizer: unlock 'Optimize for Dolby' feature<sup>[2]</sup>

> <sup>[1]</sup> The official Wearable app by Samsung cannot handle these features
>
> <sup>[2]</sup> Buds (2019) only

## Installation ![Downloads](https://img.shields.io/github/downloads/ThePBone/GalaxyBudsClient/total)

You can [**download**](https://github.com/ThePBone/GalaxyBudsClient/releases) a fully automated Setup executable in the [**release section**](https://github.com/ThePBone/GalaxyBudsClient/releases) of this repository!

*This app requires [.Net Framework](https://dotnet.microsoft.com/download/dotnet-framework/net461) 4.6.1 or later*

## Translators

* [@Florize](https://github.com/Florize) - Korean and Japanese translation
* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Russian and Ukrainian translation
* [@erenbektas](https://github.com/erenbektas) - Turkish translation
* [@kakkk](https://github.com/kakkk) - Chinese translation
* [@efrenbg1](https://github.com/efrenbg1) and Andrew Gonza - Spanish translation
* [@giovankabisano](https://github.com/giovankabisano) - Indonesian translation
* [@lucasskluser](https://github.com/lucasskluser) - Portuguese translation
 
## Contributors
* [@AndriesK](https://github.com/AndriesK) - Buds Live bug fix
* [@githubcatw](https://github.com/githubcatw) - Connection dialog base
* [@superbonaci](https://github.com/superbonaci) - Chocolatey package
___

Check my website out: <https://timschneeberger.me>
