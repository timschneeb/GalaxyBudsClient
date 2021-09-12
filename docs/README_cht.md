
<p align="center">
 <a href="../README.md">English</a> | <a href="/docs/README_chs.md">中文(简体)</a> | 中文(繁體) | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_gr.md">Ελληνικά</a> <br>
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
  <a href="#運行方式">運行方式</a> •
  <a href="#貢獻">貢獻</a> •
  <a href="#credits">Credits</a> •
  <a href="#license">License</a>
</p>

<p align="center">
    <a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
</p>

<p align="center">
    <a href="#"><img alt="Screenshot" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/screencap.gif"></a>
</p>

## 主要功能

配置和控制任何三星 Galaxy Buds 設備並將它們集成到您的電腦中。

除了官方 Android 應用程序已知的標準功能外，該項目還可以幫助您釋放耳機的全部潛力並實現新功能，例如：

* 更詳細的電池資訊
* 診斷和原廠自檢
* 顯示大量隱藏的調試信息
* 可定制的長按觸摸動作
* 固件刷機、降級（Buds+、Buds Pro）
* 以及更多...

## 下載

在 [release](https://github.com/ThePBone/GalaxyBudsClient/releases) 的部分。 獲取適用於 Windows 和 Linux 的二進製文件。 請在安裝前閱讀發行說明。

<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

Windows 程序包也可與 Windows 程序包管理器 (winget) 一起安裝

```
winget install ThePBone.GalaxyBudsClient
```

## 運行方式

要使用藍牙無線技術，設備必須能夠解釋某些藍牙配置文件，這些配置文件是可能的應用程序的定義，並指定支持藍牙的設備用於與其他藍牙設備通信的一般行為。

Galaxy Buds 定義了兩個藍牙配置文件：用於音頻流/控制的 A2DP（高級音頻分發配置文件）和用於傳輸二進制流的 SPP（串行端口配置文件）。製造商通常使用此配置文件（依賴於 RFCOMM 協議）來交換配置數據、執行固件更新或向藍牙設備發送其他命令。

儘管 A2DP 配置文件已標準化並已開放文檔，但由該 RFCOMM 協議交換的實際二進制數據的格式通常沒有記錄和開放文檔。

為了對這種數據格式進行逆向工程，我首先分析了耳機發送的二進制流的結構。後來，我還拆解了 Android 的官方 Galaxy Buds 應用程序，以更深入地了解這些設備的內部工作原理。在做這個的時候，我把我的想法寫在一個小便箋簿上。儘管它們不是那麼漂亮，但我已將它們連結放在下面。但請記住，我沒有費心寫下每一個細節。檢查原始文檔以獲取有關協議結構的更多詳細信息。

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notes</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notes</a>
</p>

在仔細觀察 Galaxy Buds Plus 的同時，我還注意到了一些不尋常的功能，例如固件調試模式、未使用的配對模式和藍牙密鑰轉儲器。我在這裡記錄了這些發現：

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Unusual features</a>
</p>

目前，我正在研究、修改和反向工程 Buds+ 的固件。在撰寫本文時，我有兩個工具可以使用官方固件二進製文件獲取和分析。在這裡查看它們：

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">Firmware Downloader</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">Firmware Extractor</a>
</p>

使用此腳本從 Buds Pro 實施流動式傳輸頭部跟踪數據: [ThePBone/BudsPro-Headtracking](https://github.com/ThePBone/BudsPro-Headtracking)

## 貢獻

隨時歡迎各位，提出任何類型的功能需求、錯誤報告和拉取請求

如果你想為這個項目報告 Bug 或提出你的想法，歡迎你用合適的模板 [創建新的 issue](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose) 亦或者 [觀看本專案的 wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues) 以獲得詳細說明。

如果您打算幫助我們翻譯此應用程序，請參閱我們 [wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations) 上的說明。這並不需要編程知識，您可以在提交拉取請求之前測試您的自定義翻譯，而無需安裝任何開發工具。您可以在 [此處](https://github.com/ThePBone/GalaxyBudsClient/blob/master/meta/translations.md) 找到現有翻譯的自動生成進度報告。

如果你想貢獻你自己的代碼，你可以簡單地提交一個簡單的拉取請求來解釋你的變化。對於更大和更複雜的貢獻，如果您可以在開始處理之前打開一個問題或通過 (Telegram [@thepbone](https://t.me/thepbone)) 給我發消息，那就更棒了。

## Credits

### 貢獻人員

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Issue模板、 wiki 和 翻譯
* [@AndriesK](https://github.com/AndriesK) - Buds Live 錯誤修復
* [@TheLastFrame](https://github.com/TheLastFrame) - Buds Pro 圖標
* [@githubcatw](https://github.com/githubcatw) - Connection dialog base
* [@GaryGadget9](https://github.com/GaryGadget9) - WinGet package

### 翻譯人員

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - 俄語和烏克蘭語翻譯
* [@PlasticBrain](https://github.com/fhalfkg) - 韓語和日語翻譯
* [@cozyplanes](https://github.com/cozyplanes) -韓語翻譯
* [@erenbektas](https://github.com/erenbektas) - 土耳其語翻譯
* [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad), [@pseudor](https://github.com/pseudor) - 中文翻譯
* [@YiJhu](https://github.com/YiJhu) - 繁體中文翻譯
* [@efrenbg1](https://github.com/efrenbg1) 和 Andrew Gonza - 西班牙語翻譯
* [@giovankabisano](https://github.com/giovankabisano) - 印尼語翻譯
* [@lucasskluser](https://github.com/lucasskluser) - 葡萄牙語翻譯
* [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - 義大利語翻譯
* [@Buashei](https://github.com/Buashei) - 波蘭語翻譯
* [@KatJillianne](https://github.com/KatJillianne) - 越南語翻譯
* [@joskaja](https://github.com/joskaja), [@Joedmin](https://github.com/Joedmin) - 捷克語翻譯
* [@TheLastFrame](https://github.com/TheLastFrame), [@ThePBone](https://github.com/ThePBone) - 德語翻譯
* [@nikossyr](https://github.com/nikossyr) - 希臘語翻譯
* [@grigorem](https://github.com/grigorem) - 羅馬尼亞語翻譯
* [@tretre91](https://github.com/tretre91) - 法語翻譯
* [@Sigarya](https://github.com/Sigarya) - 希伯來語翻譯

## License

本專案使用 [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE) 授權許可。它不隸屬於三星(Samsung)，也不以任何方式受他們(Samsung)監督。

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR 
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
