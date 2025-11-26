<p align="center">
  <a href="../README.md">English</a> | <a href="./README_chn.md">中文</a> | <a href="./README_rus.md">Русский</a> | 日本語 | <a href="./README_ukr.md">Українська</a> | <a href="./README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | <a href="/docs/README_pt.md">Português</a> | <a href="/docs/README_vnm.md">Tiếng Việt</a> <br>
    <sub>注意: readmeは翻訳者により翻訳されており、時間により最新ではない場合があります。最新の情報は英語版をご覧ください。</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">Buds、Buds+、Buds LiveとBuds Proのための非公式マネージャー</h4>
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
  <a href="#主な機能">主な機能</a> •
  <a href="#ダウンロード">ダウンロード</a> •
  <a href="#仕組み">仕組み</a> •
  <a href="#貢献">貢献</a> •
  <a href="#クレジット">クレジット</a> •
  <a href="#ライセンス">ライセンス</a>
</p>

<p align="center">
    <a href="https://ko-fi.com/H2H83E5J3"><img alt="スクリーンショット" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
</p>

<p align="center">
    <a href="#"><img alt="スクリーンショット" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/screencap.gif"></a>
</p>

## 主な機能

デスクトップでサムスンの Galaxy Buds デバイスを設定、制御できます。

このプロジェクトは公式 Android アプリで知られている基本的な機能以外にも、イヤホンの潜在力を最大に発揮し、次のような新しい機能を使用できるようにします:

- 詳細なバッテリー情報
- 診断およびファクトリーセルフテスト
- 非表示のデバッグ情報のロード
- カスタマイズできる長押し機能
- ファームウェアのインストール, ダウングレード (Buds+, Buds Pro)
- その他…

## ダウンロード

Windows および Linux のバイナリーは[リリーズ](https://github.com/ThePBone/GalaxyBudsClient/releases)でダウンロードすることができます。インストールする前にリリーズノートをお読みください。

<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="ダウンロード" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

Windows パッケージは Windows Package Manager(winget)でもインストールすることができます。

`winget install ThePBone.GalaxyBudsClient`

## 仕組み

Bluetooth 無線技術を使用するには、デバイスが動作可能なアプリやデバイスが他の Bluetooth デバイスと通信するために使用する一般的な動作を定義した Bluetooth プロファイルを解釈できる必要があります。

Galaxy Buds はオーディオストリーミング/制御のための A2DP (Advanced Audio Distribution Profile)やバイナリーストリーム通信のための SPP (Serial Port Profile)の２つの Bluetooth プロファイルを使用します。メーカーは設定データをやり取りし、ファームウェアアップデートやその他のコマンドを他の Bluetooth デバイスに送信するためにこのプロファイルを使用することが多いです。

A2DP プロファイルが標準化・文書化されても、RFCOMM プロトコールで交換される実際のデータの形式は一般的に文書化されていない独自の形式です。

このデータ形式をリバースエンジニアリングするために、私はイヤホンから転送されるバイナリーストリームを分析し始めました。その後は、デバイスの内部動作をより詳しく知るために Android の公式 Galaxy Buds アプリを分析しました。この作業をする間、私は私が考えたことを記録しました。あまり綺麗な記録ではありませんが、下にリンクを記載しています。私が詳細な内容ひとつひとつを全部記録したのではないことをお含みおきください。プロトコールについてより詳しい情報を知りたい方はソースコードをご確認ください。

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) 記録</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds+ 記録</a>
</p>

Galaxy Buds+を注意深く分析しながら、私はファームウェアでバックモードや使われていないペアリングモード、Bluetooth キーダンパーなどのユニークな機能を見つけました。その機能の詳細も下記のリンクに記録しました:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds+: ユニークな機能</a>
</p>

現在、私は Galaxy Buds+のファームウェアを修正し、リバースエンジニアリングしようとしています。作業するときにファームウェアバイナリーを取得し分析できるツールがあります。下記のリンクをご参照ください:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">ファームウェアダウンローダー</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">ファームウェア解凍ツール</a>
</p>

## 貢献

機能の要請、バグの報告、Pull Request などのいかなる形の貢献も歓迎いたします。

バグを報告したり、アイデアを共有したい方はテンプレートと共に提供される [新しい Issue 作成](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose)をご利用ください。 [ウィキ](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues)で詳細をご参照ください。

このプログラムの翻訳のお手伝いをご希望の方は、[ウィキの説明](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations)をご覧ください。プログラミングの知識を要求せず、Pull Request の前にいかなる開発ツールのインストールなしに翻訳をテストできます。
ソースコードに貢献したい方は、変更内容の Pull Request を作成してください。プログラムに対する大きいもしくは敏感な貢献事項は、作業を始める前に Issue を作成してください。(または Telegram [@thepbone](https://t.me/thepbone)に連絡)

## クレジット

#### 貢献

- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Issue テンプレート、ウィキ作成と翻訳
- [@AndriesK](https://github.com/AndriesK) - Buds Live のバグ修正
- [@TheLastFrame](https://github.com/TheLastFrame) - Buds Pro のアイコン
- [@githubcatw](https://github.com/githubcatw) - 接続プッシュ通知のベース作成
- [@GaryGadget9](https://github.com/GaryGadget9) - WinGet パッケージ

#### 翻訳

- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - ロシア語、ウクライナ語
- [@BrainInAVet](https://github.com/fhalfkg) - 韓国語、日本語
- [@cozyplanes](https://github.com/cozyplanes) - 韓国語
- [@erenbektas](https://github.com/erenbektas) - トルコ語
- [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad) and [@pseudor](https://github.com/pseudor) - 中国語
- [@efrenbg1](https://github.com/efrenbg1), Andrew Gonza - スペイン語
- [@giovankabisano](https://github.com/giovankabisano) - インドネシア語
- [@lucasskluser](https://github.com/lucasskluser) and [@JuanFariasDev](https://github.com/juanfariasdev) - ポルトガル語
- [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - イタリア語
- [@Buashei](https://github.com/Buashei) - ポーランド語
- [@KatJillianne](https://github.com/KatJillianne), [@thelegendaryjohn](https://github.com/thelegendaryjohn) - ベトナム語
- [@joskaja](https://github.com/joskaja), [@Joedmin](https://github.com/Joedmin) - チェコ語
- [@TheLastFrame](https://github.com/TheLastFrame), [@ThePBone](https://github.com/ThePBone) - ドイツ語
- [@nikossyr](https://github.com/nikossyr) - ギリシャ語
- [@grigorem](https://github.com/grigorem) - ルーマニア語

## ライセンス

このプロジェクトは[GPLv3](../LICENSE)ライセンスに準拠しています。サムスンと関わりはなく、サムスンからの勧告や制限は一切ありません。

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
