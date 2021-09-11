
<p align="center">
  English | <a href="/docs/README_chs.md">中文</a> | <a href="/docs/README_cht.md">中文(繁體)</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_gr.md">Ελληνικά</a> <br>
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
  <a href="#key-features">主要功能</a> •
  <a href="#download">下載</a> •
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

## How it works

要使用藍牙無線技術，設備必須能夠解釋某些藍牙配置文件，這些配置文件是可能的應用程序的定義，並指定支持藍牙的設備用於與其他藍牙設備通信的一般行為。

Galaxy Buds 定義了兩個藍牙配置文件：用於音頻流/控制的 A2DP（高級音頻分發配置文件）和用於傳輸二進制流的 SPP（串行端口配置文件）。製造商通常使用此配置文件（依賴於 RFCOMM 協議）來交換配置數據、執行固件更新或向藍牙設備發送其他命令。

儘管 A2DP 配置文件已標準化並已開放文檔，但由該 RFCOMM 協議交換的實際二進制數據的格式通常沒有記錄和開放文檔。

為了對這種數據格式進行逆向工程，我首先分析了耳機發送的二進制流的結構。後來，我還拆解了 Android 的官方 Galaxy Buds 應用程序，以更深入地了解這些設備的內部工作原理。在做這個的時候，我把我的想法寫在一個小便箋簿上。儘管它們不是那麼漂亮，但我已將它們連結放在下面。但請記住，我沒有費心寫下每一個細節。檢查原始文檔以獲取有關協議結構的更多詳細信息。

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notes</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notes</a>
</p>

While taking a closer look at the Galaxy Buds Plus, I also noticed some unusual features, such as a firmware debug mode, an unused pairing mode and a Bluetooth key dumper. I documented these findings here:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Unusual features</a>
</p>

Currently, I'm looking into modifying and reverse-engineering the firmware for the Buds+. At time of writing I have two tools to fetch and analyse with official firmware binaries. Check them out here:

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

### Translators

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Russian and Ukrainian translation
* [@PlasticBrain](https://github.com/fhalfkg) - Korean and Japanese translation
* [@cozyplanes](https://github.com/cozyplanes) - Korean translation
* [@erenbektas](https://github.com/erenbektas) - Turkish translation
* [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad) and [@pseudor](https://github.com/pseudor) - Chinese translation
* [@efrenbg1](https://github.com/efrenbg1) and Andrew Gonza - Spanish translation
* [@giovankabisano](https://github.com/giovankabisano) - Indonesian translation
* [@lucasskluser](https://github.com/lucasskluser) - Portuguese translation
* [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - Italian translation
* [@Buashei](https://github.com/Buashei) - Polish translation
* [@KatJillianne](https://github.com/KatJillianne) - Vietnamese translation
* [@joskaja](https://github.com/joskaja) and [@Joedmin](https://github.com/Joedmin) - Czech translation
* [@TheLastFrame](https://github.com/TheLastFrame), [@ThePBone](https://github.com/ThePBone) - German translation
* [@nikossyr](https://github.com/nikossyr) - Greek translation
* [@grigorem](https://github.com/grigorem) - Romanian translation
* [@tretre91](https://github.com/tretre91) - French translation
* [@Sigarya](https://github.com/Sigarya) - Hebrew translation

## License

This project is licensed under [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE). It is not affiliated with Samsung nor supervised by them in any way.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR 
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
