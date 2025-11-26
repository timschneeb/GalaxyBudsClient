<p align="center">
  <a href="../README.md">English</a> | 简体中文 | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | <a href="/docs/README_pt.md">Português</a> | <a href="/docs/README_vnm.md">Tiếng Việt</a> <br>
    <sub>注意：该 README 文档由翻译人员维护，可能会过时，以英文版最新版本为准。</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">一个非官方的 Galaxy Buds 管理工具(支持 Buds、Buds+、Buds Live 和 Buds Pro)</h4>
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
  <a href="#key-features">主要功能</a> •
  <a href="#download">下载</a> •
  <a href="#how-it-works">工作原理</a> •
  <a href="#contributing">贡献项目</a> •
  <a href="#credits">关于</a> •
  <a href="#license">License</a>
</p>

<p align="center">
    <a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
</p>

<p align="center">
    <a href="#"><img alt="Screenshot" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/screencap.gif"></a>
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

在 [release](https://github.com/ThePBone/GalaxyBudsClient/releases) 页面中获取 Windows 和 Linux 的二进制文件。安装前请阅读发行说明。

<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

### winget

Windows 版本也在 Windows Package Manager (winget) 提供安装

```
winget install ThePBone.GalaxyBudsClient
```

### AUR 包

@joscdk 维护的 Arch Linux 平台的 [AUR 包](https://aur.archlinux.org/packages/galaxybudsclient-bin/) 也可获得：

```
yay -S galaxybudsclient-bin
```

## 工作原理

要使用蓝牙无线技术，设备必须能够解释某些蓝牙配置文件，这些配置文件是对可能的应用的定义，并规定了支持蓝牙的设备用来与其他蓝牙设备通信的一般行为。

Galaxy Buds 定义了两种蓝牙模式：用于音频流/控制的 A2DP (高级音频分发配置)和用于传输二进制流的 SPP (串行端口配置)。制造商通常使用这些配置(依赖于 RFCOMM 协议) 交换信息、执行固件更新或向蓝牙设备发送其他指令。

尽管 A2DP 是标准化的，并且有文档，但是 RFCOMM 协议所交换的具体二进制数据通常没有文档，并且是专有的。

为了对这种数据格式进行逆向工程，我首先分析了耳机发送的二进制流的结构。此外，我还解析了 Android 的官方 Galaxy Buds 应用程序，以便更深入地了解这些设备的内部工作原理。在做这些工作的时候，我把我的想法写进了笔记中。尽管它们不是那么美观，我还是把它们放在下面。请注意，我并没有把每一个细节都写下来。请查看源代码以获得关于协议结构的更多详细信息。

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notes</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notes</a>
</p>

在进一步研究 Galaxy Buds Plus 的时候，我还注意到了一些不寻常的特性，比如固件调试模式、未使用的配对模式和蓝牙密钥转储程序。我在这里记录了这些发现：

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Unusual features</a>
</p>

目前，我正在研究修改和逆向 Buds+ 的固件。在撰写本文时，我有两个工具来获取和分析官方固件二进制文件。在这里查看：

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

- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Issue 模板、wiki 和翻译
- [@AndriesK](https://github.com/AndriesK) - Buds Live bug 修正
- [@TheLastFrame](https://github.com/TheLastFrame) - Buds Pro 的图标设计
- [@githubcatw](https://github.com/githubcatw) - 基础连接会话
- [@GaryGadget9](https://github.com/GaryGadget9) - WinGet 包维护
- [@joscdk](https://github.com/joscdk) - AUR 包维护

#### 翻译人员

- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - 俄语和乌克兰语翻译
- [@PlasticBrain](https://github.com/fhalfkg) - 韩语和日语翻译
- [@cozyplanes](https://github.com/cozyplanes) - 韩语翻译
- [@erenbektas](https://github.com/erenbektas) - 土耳其语翻译
- [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad), [@pseudor](https://github.com/pseudor) and [@YexuanXiao](https://github.com/YexuanXiao) - 中文翻译
- [@efrenbg1](https://github.com/efrenbg1) and Andrew Gonza - 西班牙语翻译
- [@giovankabisano](https://github.com/giovankabisano) - 印度尼西亚语翻译
- [@lucasskluser](https://github.com/lucasskluser) and [@JuanFariasDev](https://github.com/juanfariasdev) - 葡萄牙语翻译
- [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - 意大利语翻译
- [@Buashei](https://github.com/Buashei) - 波兰语翻译
- [@KatJillianne](https://github.com/KatJillianne), [@thelegendaryjohn](https://github.com/thelegendaryjohn) - 越南语翻译
- [@joskaja](https://github.com/joskaja) and [@Joedmin](https://github.com/Joedmin) - 捷克语翻译
- [@TheLastFrame](https://github.com/TheLastFrame) and [@ThePBone](https://github.com/ThePBone) - 德语翻译
- [@nikossyr](https://github.com/nikossyr) - 希腊语翻译

## License

本项目使用 [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE) 许可。它既不属于三星，也不受三星的任何监管。

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
