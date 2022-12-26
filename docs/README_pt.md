<p align="center">
   <a href="../README.md">English</a> | <a href="/docs/README_chs.md">中文(简体)</a> | <a href="/docs/README_cht.md">中文(繁體)</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | Português <br>
    <sub>Atenção: os arquivos "readme" são mantidos por tradutores e podem ficar desatualizados com o tempo. Para ter as informações mais atuais, visite o arquivo em inglês.</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">Um gerenciador não oficial para os Buds, Buds+, Buds Live e Buds Pro</h4>
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
  <a href="#como-funciona">Como funciona</a> •
  <a href="#contribuindo">Contribuindo</a> •
  <a href="#creditos">Creditos</a> •
  <a href="#licença">Licença</a>
</p>

<p align="center">
    <a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
</p>

<p align="center">
    <a href="#"><img alt="Screenshot" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/screencap.gif"></a>
</p>

## Key Features

Configurar e controlar qualquer aparelho da linha Samsung Galaxy Buds e integra-los ao seu desktop.

Além das funções conhecidas do aplicativo oficial de Android esse projeto também ajuda você a desbloquear o potencial máximo de seus earbuds e implementa novas funcionalidades como:

* Estátisticas detalhadas das baterias
* Diagnósticos e auto-testes de fabrica
* Diversas informações ocultas de debugging
* Customização nas ações de toque longo
* Firmware flashing, downgrading (Buds+, Buds Pro)
* e muito mais...

## Download

Para obter os binaries para Windows e Linux na página de [versões](https://github.com/ThePBone/GalaxyBudsClient/releases). Por favor, leia as notas antes de realizar a instação. 

<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

### winget

O pacote para Windows também está disponivel para instalação com o Windows Package Manager (winget)

```
winget install ThePBone.GalaxyBudsClient
```

### AUR package 

Um [AUR package](https://aur.archlinux.org/packages/galaxybudsclient-bin/) para Arch Linux mantido por @joscdk também está disponivel:
```
yay -S galaxybudsclient-bin
```


## Como funciona

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

## Contribuindo

Feature requests, bug reports, and pull requests of any kind are always welcome.

If you want to report bugs or propose your ideas for this project, you are welcome to [open a new issue](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose) with a suitable template. [Visit our wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues) for a detailed explanation.

If you are planning to help us translating this app, [refer to the instructions on our wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations). No programming knowledge is required, you can test your custom translations without installing any development tools before submitting a pull request.
You can find auto-generated progress reports for existing translations [here](https://github.com/ThePBone/GalaxyBudsClient/blob/master/meta/translations.md).

If you want to contribute your own code, you can simply submit a plain pull request explaining you changes. For larger and complex contributions it would be nice if you could open an issue (or message me via Telegram [@thepbone](https://t.me/thepbone)) before starting to work on it.

## Creditos

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
* [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad) and [@pseudor](https://github.com/pseudor) - Chinese translation
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

## Licença

This project is licensed under [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE). It is not affiliated with Samsung nor supervised by them in any way.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR 
THE USE OR O
