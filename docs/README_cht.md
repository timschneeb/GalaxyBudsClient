<p align="center">
    <a href="../README.md">English</a> | <a href="/docs/README_chs.md">中文(简体)</a> | 中文(繁體) | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_tr.md">Türkçe</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | <a href="/docs/README_pt.md">Português</a> | <a href="/docs/README_vnm.md">Tiếng Việt</a> <br>
    <sub>注意：此自述文件由翻譯人員維護，可能會與當前的新版本有一定的誤差。 最新信息以英文版為準。</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">這是一個非官方的 Galaxy Buds 管理程式</h4>
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
  <a href="#下載">下載</a> •
  <a href="#運行原理">運行原理</a> •
  <a href="#如何貢獻">如何貢獻</a> •
  <a href="#協助者們">協助者們</a> •
  <a href="#授權協議">授權協議</a>
</p>

<p align="center">
  <span><a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
  <a href="#"><img alt="Screenshot" src="https://github.com/timschneeb/GalaxyBudsClient/raw/master/screenshots/app_dark.png"></a></span>
</p>

## 主要功能

設定和控制任何的 三星 Galaxy Buds 設備並將它們集成到您的電腦中。

除了官方 Android APP 已知的基本功能外，這項專案還可以幫助您釋放耳機全部的潛力並實現實驗室內的功能。
如：

- 更詳細的電池資訊
- 原廠自我檢測與診斷
- 載入大量被隱藏的測試內容
- 可自訂的長按與觸碰操控
- 軟體的寫入、降級 (Buds+, Buds Pro)
- 以及更多功能...

## 下載

目前提供多種 Linux 軟體套件：
* [Flatpak（所有 Linux 發行版）](#flatpak)
* [AUR 套件（Arch Linux）](#aur-套件)

在 [release](https://github.com/ThePBone/GalaxyBudsClient/releases) 中獲取適用於 Windows 的二進制文件。 請在安裝前閱讀上方的發行說明。

在此下載桌面版：
<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

在此下載 Android 版（付費）：
<p align="center">
  <a href='https://play.google.com/store/apps/details?id=me.timschneeberger.galaxybudsclient&utm_source=github&pcampaignid=pcampaignidMKT-Other-global-all-co-prtnr-py-PartBadge-Mar2515-1'>
    <img width="300" alt='Get it on Google Play' src='https://play.google.com/intl/en_us/badges/static/images/badges/en_badge_web_generic.png'/>
  </a>
</p>

### Flatpak

適用於所有 Linux 發行版的通用套件。Flatpak 版本預設不支援開機自動啟動，除非手動進行設定。你可以使用 `galaxybudsclient /StartMinimized`，讓應用程式在系統啟動時以靜默模式啟動。

可從 FlatHub 下載: https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient

```
flatpak install me.timschneeberger.GalaxyBudsClient
```

<a href='https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient'><img width='240' alt='Download on Flathub' src='https://dl.flathub.org/assets/badges/flathub-badge-en.png'/></a>

> **注意**: Flatpak 應用程式運作於沙盒環境中。預設情況下，本應用程式只能存取 `~/.var/app/me.timschneeberger.GalaxyBudsClient/` 


### AUR 套件

由 @joscdk 所維護的 Arch Linux [AUR package](https://aur.archlinux.org/packages/galaxybudsclient-bin/) 包也是可以使用的:

```
yay -S galaxybudsclient-bin
```

### winget

Windows 用戶也可以透過 Windows Package Manager (winget) 的方式來進行安裝

```
winget install ThePBone.GalaxyBudsClient
```

## 運行原理

為了使用藍牙無線技術，設備必須能夠解釋特定的藍牙封包文件，使藍牙設備能夠有效地相互通信。

Galaxy Buds 定義了兩個藍牙封包文件：用於音頻流/控制的 A2DP（高級音頻分發封包文件） 和 用於傳輸二進制流的 SPP（串行端口封包文件）。製造商經常使用此封包文件（依賴於 RFCOMM 協議）來交換封包數據、執行軟體更新或向藍牙設備發送其他命令。

儘管 A2DP 配置文件已經有一定的規範，但此 RFCOMM 協議交換的二進制數據格式通常是專有的。

為了對這種數據格式進行逆向工程，我首先分析了耳機所發送的二進制流的結構。到後來，我還拆解了 Android 的官方 Galaxy Buds APP，以更深入地了解這些設備的內部工作原理。你可以在這邊找到我記下的一些 "不完整的" 筆記。檢查原始文檔以獲取有關協議結構的更多詳細信息。

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notes</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notes</a>
</p>

在仔細觀察 Galaxy Buds Plus 時，我還注意到一些不尋常的功能，例如固件調試模式、未使用的配對模式和藍牙密鑰轉儲器。我在這邊記錄了這些發現：

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Unusual features</a>
</p>

目前，我正在研究修改和逆向工程 Buds+ 的軟體。在撰寫本文時，我創建了兩個工具來獲取和分析官方固件二進製文件。在這裡查看它們：

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">Firmware Downloader</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">Firmware Extractor</a>
</p>

使用此腳本從 Buds Pro 實時流式傳輸頭部跟踪數據： [ThePBone/BudsPro-Headtracking](https://github.com/ThePBone/BudsPro-Headtracking)

## 如何貢獻

隨時歡迎各位，提出任何類型的功能需求、錯誤報告和 git push 請求

如果您打算幫助我們翻譯此應用程序，請參閱我們 [wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations) 上的說明。這並不需要編程知識，您可以在提交拉取請求之前測試您的自定義翻譯，而無需安裝任何開發工具。您可以在 [此處](https://github.com/ThePBone/GalaxyBudsClient/blob/master/meta/translations.md) 找到現有翻譯的自動生成進度報告。

如果你想貢獻你自己的代碼，你可以簡單地提交一個簡單的 git push 請求來解釋你的變化。對於更大和更複雜的貢獻，如果您可以在開始處理之前打開一個問題或通過 (Telegram [@thepbone](https://t.me/thepbone)) 給我發消息，那就更棒了。

## 協助者們

### 貢獻者

* [@nift4](https://github.com/nift4) - macOS 支援與錯誤修復
* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Issue 模板、wiki 和翻譯
* [@AndriesK](https://github.com/AndriesK) - Buds Live 錯誤修正
* [@TheLastFrame](https://github.com/TheLastFrame) - Buds Pro 的圖示設計
* [@githubcatw](https://github.com/githubcatw) - 基礎連線工作階段
* [@GaryGadget9](https://github.com/GaryGadget9) - WinGet 套件維護
* [@joscdk](https://github.com/joscdk) - AUR 套件維護

#### 翻譯人員

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - 俄語和烏克蘭語翻譯
* [@PlasticBrain](https://github.com/fhalfkg) - 韓語和日語翻譯
* [@cozyplanes](https://github.com/cozyplanes) - 韓語翻譯
* [@corydalis10](https://github.com/corydalis10) - 韓語翻譯
* [@erenbektas](https://github.com/erenbektas) 和 [@Eta06](https://github.com/Eta06) - 土耳其語翻譯
* [@kakkk](https://github.com/kakkk)、[@KevinZonda](https://github.com/KevinZonda)、[@ssenkrad](https://github.com/ssenkrad)、[@pseudor](https://github.com/pseudor) 和 [@YexuanXiao](https://github.com/YexuanXiao) - 簡體中文翻譯
* [@YiJhu](https://github.com/YiJhu) - 繁體中文翻譯
* [@efrenbg1](https://github.com/efrenbg1) 和 Andrew Gonza - 西班牙語翻譯
* [@giovankabisano](https://github.com/giovankabisano) - 印度尼西亞語翻譯
* [@lucasskluser](https://github.com/lucasskluser) 和 [@JuanFariasDev](https://github.com/juanfariasdev) - 葡萄牙語翻譯
* [@alb-p](https://github.com/alb-p)、[@mario-donnarumma](https://github.com/mario-donnarumma) - 義大利語翻譯
* [@Buashei](https://github.com/Buashei) - 波蘭語翻譯
* [@KatJillianne](https://github.com/KatJillianne) 和 [@thelegendaryjohn](https://github.com/thelegendaryjohn) - 越南語翻譯
* [@joskaja](https://github.com/joskaja) 和 [@Joedmin](https://github.com/Joedmin) - 捷克語翻譯
* [@Benni0109](https://github.com/Benni0109)、[@TheLastFrame](https://github.com/TheLastFrame)、[@timschneeb](https://github.com/timschneeb) - 德語翻譯
* [@nikossyr](https://github.com/nikossyr) - 希臘語翻譯
* [@grigorem](https://github.com/grigorem) - 羅馬尼亞語翻譯
* [@tretre91](https://github.com/tretre91) - 法語翻譯
* [@Sigarya](https://github.com/Sigarya) - 希伯來語翻譯
* [@domroaft](https://github.com/domroaft) - 匈牙利語翻譯
* [@lampi8426](https://github.com/lampi8426) - 荷蘭語翻譯

## 授權協議

本專案使用 [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE) 授權許可。它不隸屬於三星(Samsung)，也不以任何方式受他們(Samsung)監督。

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
