
<p align="center">
  <a href="../README.md">English</a> | 中文(简体) | <a href="/docs/README_cht.md">中文(繁體)</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_tr.md">Türkçe</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | <a href="/docs/README_pt.md">Português</a> | <a href="/docs/README_vnm.md">Tiếng Việt</a> <br>
    <sub>注意：该 README 文档由翻译人员维护，可能会过时，以英文版最新版本为准。</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">一个非官方的 Galaxy Buds 管理工具</h4>
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
  <a href="#主要功能">主要功能</a> •
  <a href="#下载">下载</a> •
  <a href="#工作原理">工作原理</a> •
  <a href="#贡献项目">贡献项目</a> •
  <a href="#鸣谢">鸣谢</a> •
  <a href="#license">License</a>
</p>

<p align="center">
  <span><a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
  <a href="#"><img alt="Screenshot" src="https://github.com/timschneeb/GalaxyBudsClient/raw/master/screenshots/app_dark.png"></a></span>
</p>

## 主要功能

配置和控制任何 Galaxy Buds 设备，并整合到你的设备中。

除了官方 Android 程序中已知的功能外，该项目还可以帮助您释放耳机的全部潜力，并实现了新的功能，例如：

- 详细电池统计
- 诊断和工厂自检
- 大量隐藏的调试信息
- 可定制的长按触摸动作
- 固件写入，降级 (Buds+, Buds Pro)
- 以及更多...

## 下载

目前提供多种 Linux 软件包：
* [Flatpak（所有 Linux 发行版）](#flatpak)
* [AUR 软件包（Arch Linux）](#aur-软件包)

在 [release](https://github.com/ThePBone/GalaxyBudsClient/releases) 页面中获取 Windows 的二进制文件。安装前请阅读发行说明。

在此下载桌面版：
<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

在此下载 Android 版（收费）：
<p align="center">
  <a href='https://play.google.com/store/apps/details?id=me.timschneeberger.galaxybudsclient&utm_source=github&pcampaignid=pcampaignidMKT-Other-global-all-co-prtnr-py-PartBadge-Mar2515-1'>
    <img width="300" alt='Get it on Google Play' src='https://play.google.com/intl/en_us/badges/static/images/badges/en_badge_web_generic.png'/>
  </a>
</p>

### Flatpak

适用于所有 Linux 发行版的通用软件包。Flatpak 版本默认不支持开机自启动，除非手动进行配置。你可以使用 `galaxybudsclient /StartMinimized`，让应用在系统启动时静默启动。

可从 FlatHub 下载: https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient
```
flatpak install me.timschneeberger.GalaxyBudsClient
```

<a href='https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient'><img width='240' alt='Download on Flathub' src='https://dl.flathub.org/assets/badges/flathub-badge-en.png'/></a>

> **注意**: Flatpak 应用运行在沙盒环境中。默认情况下，本应用只能访问 `~/.var/app/me.timschneeberger.GalaxyBudsClient/` 



### AUR 软件包

@joscdk 维护的 Arch Linux 平台的 [AUR 包](https://aur.archlinux.org/packages/galaxybudsclient-bin/) 也可供下载：
```
yay -S galaxybudsclient-bin
```

### winget

Windows 版本也可以通过 Windows Package Manager (winget) 安装

```
winget install ThePBone.GalaxyBudsClient
```

## 工作原理

为了使用蓝牙无线技术，设备必须能够解析特定的蓝牙配置文件，从而使蓝牙设备之间能够高效通信。

Galaxy Buds 定义了两种蓝牙配置文件：用于音频流传输与控制的 A2DP（高级音频分发配置文件）；以及用于传输二进制数据流的 SPP（串行端口配置文件）。制造商通常使用该配置文件（其基于 RFCOMM 协议）来交换配置数据、执行固件更新或向蓝牙设备发送其他命令。

尽管 A2DP 配置文件已标准化并有公开文档，但通过 RFCOMM 协议交换的二进制数据格式通常为专有格式。

为了逆向分析该数据格式，我首先分析了耳塞发送的二进制数据流结构。随后，我还反编译了官方的 Android 版 Galaxy Buds 应用，以深入了解这些设备的内部工作机制。以下是我整理的部分（不完整的）笔记。有关协议结构的更详细信息，请参阅源代码。

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notes</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notes</a>
</p>

在进一步研究 Galaxy Buds Plus 的时候，我还注意到了一些不寻常的特性，比如固件调试模式、未使用的配对模式和蓝牙密钥转储程序。我在这里记录了这些发现：

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Unusual features</a>
</p>

目前，我正在研究对 Buds+ 的固件进行修改和逆向工程。在撰写本文时，我已经创建了两个工具，用于获取和分析官方固件二进制文件。请在此处查看：

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">Firmware Downloader</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">Firmware Extractor</a>
</p>

使用这个脚本，实时查看流式的 Buds Pro 头部追踪数据：[ThePBone/BudsPro-Headtracking](https://github.com/ThePBone/BudsPro-Headtracking)

## 贡献项目

我们始终欢迎任何形式的功能请求、错误报告和 PR。

如果你想报告错误或提出你对这个项目的想法，欢迎你使用模板[开启一个新的 Issues](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose)。查看 [wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues) 以获得更详细的信息。

如果您打算帮助我们翻译此应用程序，[请参阅 Wiki 上的说明](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations)。您可以在提交 PR 前测试自定义翻译，这无需编程知识，也不需要安装任何开发工具。
你可以找到现有翻译的自动生成的进度报告在[此处](https://github.com/ThePBone/GalaxyBudsClient/blob/master/meta/translations.md)。

如果您想贡献自己的代码，只需提交一个简单的 PR 来解释您的更改。对于更复杂的代码，我希望能够在提交请求之前通过 Telegram [@thepbone](https://t.me/thepbone) 与我交流。

## 鸣谢

#### 贡献人员

* [@nift4](https://github.com/nift4) - macOS 支持与错误修复
* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Issue 模板、wiki 和翻译
* [@AndriesK](https://github.com/AndriesK) - Buds Live bug 修正
* [@TheLastFrame](https://github.com/TheLastFrame) - Buds Pro 的图标设计
* [@githubcatw](https://github.com/githubcatw) - 基础连接会话
* [@GaryGadget9](https://github.com/GaryGadget9) - WinGet 包维护
* [@joscdk](https://github.com/joscdk) - AUR 包维护

#### 翻译人员

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - 俄语和乌克兰语翻译
* [@PlasticBrain](https://github.com/fhalfkg) - 韩语和日语翻译
* [@cozyplanes](https://github.com/cozyplanes) - 韩语翻译
* [@corydalis10](https://github.com/corydalis10) - 韩语翻译
* [@erenbektas](https://github.com/erenbektas) 和 [@Eta06](https://github.com/Eta06) - 土耳其语翻译
* [@kakkk](https://github.com/kakkk)、[@KevinZonda](https://github.com/KevinZonda)、[@ssenkrad](https://github.com/ssenkrad)、[@pseudor](https://github.com/pseudor) 和 [@YexuanXiao](https://github.com/YexuanXiao) - 简体中文翻译
* [@YiJhu](https://github.com/YiJhu) - 繁体中文翻译
* [@efrenbg1](https://github.com/efrenbg1) 和 Andrew Gonza - 西班牙语翻译
* [@giovankabisano](https://github.com/giovankabisano) - 印度尼西亚语翻译
* [@lucasskluser](https://github.com/lucasskluser) 和 [@JuanFariasDev](https://github.com/juanfariasdev) - 葡萄牙语翻译
* [@alb-p](https://github.com/alb-p)、[@mario-donnarumma](https://github.com/mario-donnarumma) - 意大利语翻译
* [@Buashei](https://github.com/Buashei) - 波兰语翻译
* [@KatJillianne](https://github.com/KatJillianne) 和 [@thelegendaryjohn](https://github.com/thelegendaryjohn) - 越南语翻译
* [@joskaja](https://github.com/joskaja) 和 [@Joedmin](https://github.com/Joedmin) - 捷克语翻译
* [@Benni0109](https://github.com/Benni0109)、[@TheLastFrame](https://github.com/TheLastFrame)、[@timschneeb](https://github.com/timschneeb) - 德语翻译
* [@nikossyr](https://github.com/nikossyr) - 希腊语翻译
* [@grigorem](https://github.com/grigorem) - 罗马尼亚语翻译
* [@tretre91](https://github.com/tretre91) - 法语翻译
* [@Sigarya](https://github.com/Sigarya) - 希伯来语翻译
* [@domroaft](https://github.com/domroaft) - 匈牙利语翻译
* [@lampi8426](https://github.com/lampi8426) - 荷兰语翻译

## License

本项目使用 [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE) 许可。它既不属于三星，也不受三星的任何监管。

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
