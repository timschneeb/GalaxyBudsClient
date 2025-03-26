
<p align="center">
  English | <a href="/docs/README_chs.md">中文(简体)</a> | <a href="/docs/README_cht.md">中文(繁體)</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_tr.md">Türkçe</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | <a href="/docs/README_pt.md">Português</a> | <a href="/docs/README_vnm.md">Tiếng Việt</a> <br>
    <sub>Attention: readme files are maintained by translators and may become outdated from time to time. For the newest info rely on the English version.</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">An unofficial manager for Galaxy Buds devices</h4>
<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
    <img alt="GitHub downloads count" src="https://img.shields.io/github/downloads/thepbone/galaxybudsclient/total">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
   <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/LICENSE">
      <img alt="License" src="https://img.shields.io/github/license/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
    <img alt="Platform" src="https://img.shields.io/badge/platform-Windows/macOS/Linux/Android-yellowgreen">
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
  <span><a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
  <a href="#"><img alt="Screenshot" src="https://github.com/timschneeb/GalaxyBudsClient/raw/master/screenshots/app_dark.png"></a></span>
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

If you're looking for older firmware binaries, have a look here: [https://github.com/timschneeb/galaxy-buds-firmware-archive](https://github.com/timschneeb/galaxy-buds-firmware-archive#galaxy-buds-firmware-archive)

## Download

There are several Linux packages available:
* [Flatpak (All Linux distros)](#flatpak)
* [AUR package (Arch Linux)](#aur-package)

Get binaries for Windows in the [release](https://github.com/timschneeb/GalaxyBudsClient/releases) section. Please read the release notes before installation.

Download the desktop version here:
<p align="center">
    <a href="https://github.com/timschneeb/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/timschneeb/GalaxyBudsClient/raw/master/screenshots/download.png"></a>
</p>

Download the Android mobile version here (paid):
<p align="center">
  <a href='https://play.google.com/store/apps/details?id=me.timschneeberger.galaxybudsclient&utm_source=github&pcampaignid=pcampaignidMKT-Other-global-all-co-prtnr-py-PartBadge-Mar2515-1'>
    <img width="300" alt='Get it on Google Play' src='https://play.google.com/intl/en_us/badges/static/images/badges/en_badge_web_generic.png'/>
  </a>
</p>

### Flatpak

Universal binary packages for all Linux distributions. The Flatpak version does not support autostart unless it is set up manually. You can use `galaxybudsclient /StartMinimized` to launch the app silently during startup.

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
winget install timschneeb.GalaxyBudsClient
```

## How it works

In order to use Bluetooth wireless technology, a device must be able to interpret specific Bluetooth profiles that enable Bluetooth devices to communicate efficiently with each other.

The Galaxy Buds define two Bluetooth profiles: A2DP (Advanced Audio Distribution Profile) for audio streaming/controlling and SPP (Serial Port Profile) for transmitting binary streams. Manufacturers often use this profile (which relies on the RFCOMM protocol) to exchange configuration data, perform firmware updates, or send other commands to the Bluetooth device.

Even though the A2DP profile is standardized and documented, the format of the binary data exchanged by this RFCOMM protocol is usually proprietary.

To reverse-engineer this data format, I started by analyzing the structure of the binary stream sent by the earbuds. Later on, I also disassembled the official Galaxy Buds apps for Android to gain more insight into these devices' inner workings. You can find some (incomplete) notes I took down below. Check the source code to get more detailed information on the structure of the protocol.

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notes</a> •
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notes</a>
</p>

While taking a closer look at the Galaxy Buds Plus, I also noticed some unusual features, such as a firmware debug mode, an unused pairing mode, and a Bluetooth key dumper. I documented these findings here:

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Unusual features</a>
</p>

Currently, I'm looking into modifying and reverse-engineering the firmware for the Buds+. At time of writing I have created two tools to fetch and analyze official firmware binaries. Check them out here:

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsFirmwareDownloader">Firmware Downloader</a> •
  <a href="https://github.com/timschneeb/GalaxyBudsFirmwareExtractor">Firmware Extractor</a>
</p>

Stream head-tracking data in realtime from your Buds Pro using this script: [timschneeb/BudsPro-Headtracking](https://github.com/timschneeb/BudsPro-Headtracking)

## Contributing

Feature requests, bug reports, and pull requests of any kind are always welcome.

If you want to report bugs or propose your ideas for this project, you are welcome to [open a new issue](https://github.com/timschneeb/GalaxyBudsClient/issues/new/choose) with a suitable template. [Visit our wiki](https://github.com/timschneeb/GalaxyBudsClient/wiki/2.-How-to-submit-issues) for a detailed explanation.

If you are planning to help us translate this app, [refer to the instructions on our wiki](https://github.com/timschneeb/GalaxyBudsClient/wiki/3.-How-to-help-with-translations). No programming knowledge is required, you can test your custom translations without installing any development tools before submitting a pull request.
You can find auto-generated progress reports for existing translations [here](https://github.com/timschneeb/GalaxyBudsClient/blob/master/meta/translations.md).

If you want to contribute your own code, you can simply submit a plain pull request explaining you changes. For larger and complex contributions it would be nice if you could open an issue (or message me via Telegram [@thepbone](https://t.me/thepbone)) before starting to work on it.

## Credits

### Contributors

* [@nift4](https://github.com/nift4) - macOS support and bug fixes
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
* [@corydalis10](https://github.com/corydalis10) - Korean translation
* [@erenbektas](https://github.com/erenbektas) and [@Eta06](https://github.com/Eta06)  - Turkish translation
* [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), * [@JuanFariasDev](https://github.com/juanfariasdev), [@ssenkrad](https://github.com/ssenkrad), [@pseudor](https://github.com/pseudor) and [@YexuanXiao](https://github.com/YexuanXiao) - Chinese translation
* [@YiJhu](https://github.com/YiJhu) - Chinese-Traditional translation
* [@efrenbg1](https://github.com/efrenbg1) and Andrew Gonza - Spanish translation
* [@giovankabisano](https://github.com/giovankabisano) - Indonesian translation
* [@lucasskluser](https://github.com/lucasskluser) and [@JuanFariasDev](https://github.com/juanfariasdev) - Portuguese translation
* [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - Italian translation
* [@Buashei](https://github.com/Buashei) - Polish translation
* [@KatJillianne](https://github.com/KatJillianne) and [@thelegendaryjohn](https://github.com/thelegendaryjohn) - Vietnamese translation
* [@joskaja](https://github.com/joskaja) and [@Joedmin](https://github.com/Joedmin) - Czech translation
* [@Benni0109](https://github.com/Benni0109), [@TheLastFrame](https://github.com/TheLastFrame), [@timschneeb](https://github.com/timschneeb) - German translation
* [@nikossyr](https://github.com/nikossyr) - Greek translation
* [@grigorem](https://github.com/grigorem) - Romanian translation
* [@tretre91](https://github.com/tretre91) - French translation
* [@Sigarya](https://github.com/Sigarya) - Hebrew translation
* [@domroaft](https://github.com/domroaft) - Hungarian translation
* [@lampi8426](https://github.com/lampi8426) - Dutch translation

### Services

* [Cloudflare](https://www.cloudflare.com/) - Secures the backend APIs of GalaxyBudsClient and provided a Pro license

### Assets
* Earbud asset used in the Android icon created by [Archival](https://www.flaticon.com/authors/archival) from [Flaticon](https://www.flaticon.com/)

## License

This project is licensed under [GPLv3](https://github.com/timschneeb/GalaxyBudsClient/blob/master/LICENSE). It is not affiliated with Samsung nor supervised by them in any way.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
