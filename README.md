
<p align="center">
  English | <a href="/docs/README_chs.md">中文(简体)</a> | <a href="/docs/README_cht.md">中文(繁體)</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | <a href="/docs/README_pt.md">Português</a> <br>
    <sub>Attention: readme files are maintained by translators and may become outdated from time to time. For newest info rely on English version.</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">An unofficial manager for the Buds, Buds+, Buds Live and Buds Pro</h4>
<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="GitHub downloads count" src="https://img.shields.io/github/downloads/thepbone/galaxybudsclient/total">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
   <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE">
      <img alt="License" src="https://img.shields.io/github/license/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="Platform" src="https://img.shields.io/badge/platform-Windows/Linux-yellowgreen">
  </a>
</p>
<p align="center">
  <a href="#key-features">Key Features</a> •
  <a href="#download">Download</a> •
  <a href="#how-it-works">How it works</a> •
  <a href="#contributing">Contributing</a> •
  <a href="#credits">Credits</a> •
  <a href="#license">License</a>
</p>

<p align="center">
    <a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
</p>

<p align="center">
    <a href="#"><img alt="Screenshot" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/screencap.gif"></a>
</p>

## Key Features

Configure and control any Samsung Galaxy Buds device and integrate them into your desktop.

Aside from standard features known from the official Android app, this project helps you to release the full potential of your earbuds and implements new functionality such as:

* Detailed battery statistics
* Diagnostics and factory self-tests
* Loads of hidden debugging information
* Customizable long-press touch actions
* Firmware flashing, downgrading (Buds+, Buds Pro)
* and much more...

If you're looking for older firmware binaries, have a look here: [https://github.com/ThePBone/galaxy-buds-firmware-archive](https://github.com/ThePBone/galaxy-buds-firmware-archive#galaxy-buds-firmware-archive)

## Download

There are several Linux packages available:
* [Flatpak (All Linux distros)](#flatpak)
* [AUR package (Arch Linux)](#aur-package)

Get binaries for Windows in the [release](https://github.com/ThePBone/GalaxyBudsClient/releases) section. Please read the release notes before installation:
<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

### Flatpak

Universal binary packages for all Linux distributions. This is the recommended way of installing GalaxyBudsClient on Linux.

Available for download on FlatHub: https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient
```
flatpak install me.timschneeberger.GalaxyBudsClient
```

<a href='https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient'><img width='240' alt='Download on Flathub' src='https://dl.flathub.org/assets/badges/flathub-badge-en.png'/></a>

> **Note**: Flatpaks are sandboxed. This application can only access `~/.var/app/me.timschneeberger.GalaxyBudsClient/` by default.

### AUR package 

An [AUR package](https://aur.archlinux.org/packages/galaxybudsclient-bin/) for Arch Linux maintained by @joscdk is also available:
```
yay -S galaxybudsclient-bin
```

### winget

The Windows package is also available to install with Windows Package Manager (winget)

```
winget install ThePBone.GalaxyBudsClient
```

## How it works

In order to use Bluetooth wireless technology, a device must be able to interpret specific Bluetooth profiles that enable Bluetooth devices to communicate efficiently with each other.

The Galaxy Buds define two Bluetooth profiles: A2DP (Advanced Audio Distribution Profile) for audio streaming/controlling and SPP (Serial Port Profile) for transmitting binary streams. Manufacturers often use this profile (which relies on the RFCOMM protocol) to exchange configuration data, perform firmware updates, or send other commands to the Bluetooth device.

Even though the A2DP profile is standardized and documented, the format of the binary data exchanged by this RFCOMM protocol is usually proprietary.

To reverse-engineer this data format, I started by analyzing the structure of the binary stream sent by the earbuds. Later on, I also disassembled the official Galaxy Buds apps for Android to gain more insight into these devices' inner workings. You can find some (incomplete) notes I took down below. Check the source code to get more detailed information on the structure of the protocol.

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notes</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notes</a>
</p>

While taking a closer look at the Galaxy Buds Plus, I also noticed some unusual features, such as a firmware debug mode, an unused pairing mode, and a Bluetooth key dumper. I documented these findings here:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Unusual features</a>
</p>

Currently, I'm looking into modifying and reverse-engineering the firmware for the Buds+. At time of writing I have created two tools to fetch and analyze official firmware binaries. Check them out here:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">Firmware Downloader</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">Firmware Extractor</a>
</p>

Stream head-tracking data in realtime from your Buds Pro using this script: [ThePBone/BudsPro-Headtracking](https://github.com/ThePBone/BudsPro-Headtracking)

## Contributing

Feature requests, bug reports, and pull requests of any kind are always welcome.

If you want to report bugs or propose your ideas for this project, you are welcome to [open a new issue](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose) with a suitable template. [Visit our wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues) for a detailed explanation.

If you are planning to help us translating this app, [refer to the instructions on our wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations). No programming knowledge is required, you can test your custom translations without installing any development tools before submitting a pull request.
You can find auto-generated progress reports for existing translations [here](https://github.com/ThePBone/GalaxyBudsClient/blob/master/meta/translations.md).

If you want to contribute your own code, you can simply submit a plain pull request explaining you changes. For larger and complex contributions it would be nice if you could open an issue (or message me via Telegram [@thepbone](https://t.me/thepbone)) before starting to work on it.

## Credits

### Contributors

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Issue templates, wiki and translations
* [@AndriesK](https://github.com/AndriesK) - Buds Live bug fix
* [@TheLastFrame](https://github.com/TheLastFrame) - Buds Pro icons
* [@githubcatw](https://github.com/githubcatw) - Connection dialog base
* [@GaryGadget9](https://github.com/GaryGadget9) - WinGet package
* [@joscdk](https://github.com/joscdk) - AUR package

### Translators

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Russian and Ukrainian translation
* [@PlasticBrain](https://github.com/fhalfkg) - Korean and Japanese translation
* [@cozyplanes](https://github.com/cozyplanes) - Korean translation
* [@erenbektas](https://github.com/erenbektas) - Turkish translation
* [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad), [@pseudor](https://github.com/pseudor) and [@YexuanXiao](https://github.com/YexuanXiao) - Chinese translation
* [@YiJhu](https://github.com/YiJhu) - Chinese-Traditional translation
* [@efrenbg1](https://github.com/efrenbg1) and Andrew Gonza - Spanish translation
* [@giovankabisano](https://github.com/giovankabisano) - Indonesian translation
* [@lucasskluser](https://github.com/lucasskluser) - Portuguese translation
* [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - Italian translation
* [@Buashei](https://github.com/Buashei) - Polish translation
* [@KatJillianne](https://github.com/KatJillianne) - Vietnamese translation
* [@joskaja](https://github.com/joskaja) and [@Joedmin](https://github.com/Joedmin) - Czech translation
* [@Benni0109](https://github.com/Benni0109), [@TheLastFrame](https://github.com/TheLastFrame), [@ThePBone](https://github.com/ThePBone) - German translation
* [@nikossyr](https://github.com/nikossyr) - Greek translation
* [@grigorem](https://github.com/grigorem) - Romanian translation
* [@tretre91](https://github.com/tretre91) - French translation
* [@Sigarya](https://github.com/Sigarya) - Hebrew translation
* [@domroaft](https://github.com/domroaft) - Hungarian translation
* [@lampi8426](https://github.com/lampi8426) - Dutch translation

## License

This project is licensed under [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE). It is not affiliated with Samsung nor supervised by them in any way.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR 
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
