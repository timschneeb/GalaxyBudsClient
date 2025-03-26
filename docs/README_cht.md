<p align="center">
    <a href="../README.md">English</a> | <a href="/docs/README_chs.md">中文(简体)</a> | 中文(繁體) | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | <a href="/docs/README_pt.md">Português</a> | <a href="/docs/README_vnm.md">Tiếng Việt</a> <br>
    <sub>注意：此自述文件由翻譯人員維護，可能會與當前的新版本有一定的誤差。 最新信息以英文版為準。</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">這是一個非官方的管理程式 支援 Galaxy Buds 、 Buds+ 、 Buds Live 和 Buds Pro</h4>
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
  <a href="#主要功能">主要功能</a> •
  <a href="#下載">下載</a> •
  <a href="#運行原理">運行原理</a> •
  <a href="#如何貢獻">如何貢獻</a> •
  <a href="#協助者們">協助者們</a> •
  <a href="#授權協議">授權協議</a>
</p>

<p align="center">
    <a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
</p>

<p align="center">
    <a href="#"><img alt="Screenshot" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/screencap.gif"></a>
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

在 [release](https://github.com/ThePBone/GalaxyBudsClient/releases) 中獲取適用於 Windows 和 Linux 的二進制文件。 請在安裝前閱讀上方的發行說明。

<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

### winget

Windows 用戶也可以透過 封裝管理員 (winget) 的方式來進行安裝

```
winget install ThePBone.GalaxyBudsClient
```

### AUR package

由 @joscdk 所維護的 Arch Linux [AUR package](https://aur.archlinux.org/packages/galaxybudsclient-bin/) 包也是可以使用的:

```
yay -S galaxybudsclient-bin
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

- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Issue 模板、 wiki 和 翻譯
- [@AndriesK](https://github.com/AndriesK) - Buds Live 的錯誤修復
- [@TheLastFrame](https://github.com/TheLastFrame) - Buds Pro 的圖示
- [@githubcatw](https://github.com/githubcatw) - Connection dialog base
- [@GaryGadget9](https://github.com/GaryGadget9) - WinGet package
- [@joscdk](https://github.com/joscdk) - AUR package

### 翻譯人員

- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - 俄語 和 烏克蘭語 翻譯
- [@PlasticBrain](https://github.com/fhalfkg) - 韓語 和 日語 翻譯
- [@cozyplanes](https://github.com/cozyplanes) - 韓語 翻譯
- [@erenbektas](https://github.com/erenbektas) - 土耳其語 翻譯
- [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad) 和 [@pseudor](https://github.com/pseudor) - 簡體中文 翻譯
- [@YiJhu](https://github.com/YiJhu) - 繁體中文 翻譯
- [@efrenbg1](https://github.com/efrenbg1) 和 Andrew Gonza - 西班牙語 翻譯
- [@giovankabisano](https://github.com/giovankabisano) - 印尼語 翻譯
- [@lucasskluser](https://github.com/lucasskluser) 和 [@JuanFariasDev](https://github.com/juanfariasdev) - 葡萄牙語 翻譯
- [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - 義大利語 翻譯
- [@Buashei](https://github.com/Buashei) - 波蘭語 翻譯
- [@KatJillianne](https://github.com/KatJillianne), [@thelegendaryjohn](https://github.com/thelegendaryjohn) - 越南語 翻譯
- [@joskaja](https://github.com/joskaja) and [@Joedmin](https://github.com/Joedmin) - 捷克語 翻譯
- [@Benni0109](https://github.com/Benni0109), [@TheLastFrame](https://github.com/TheLastFrame), [@ThePBone](https://github.com/ThePBone) - 德語 翻譯
- [@nikossyr](https://github.com/nikossyr) - 希臘語 翻譯
- [@grigorem](https://github.com/grigorem) - 羅馬尼亞語 翻譯
- [@tretre91](https://github.com/tretre91) - 法語 翻譯
- [@Sigarya](https://github.com/Sigarya) - 希伯來語 翻譯

## 協議

本專案使用 [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE) 授權許可。它不隸屬於三星(Samsung)，也不以任何方式受他們(Samsung)監督。

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
