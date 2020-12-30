

<p align="center">
  <a href="../README.md">English</a> | <a href="./README_chn.md">中文</a> | <a href="./README_rus.md">Русский</a> | 日本語 | <a href="./README_ukr.md">Українська</a> | <a href="./README_kor.md">한국어</a>
</p>

_____

# Galaxy Buds Client

Windows 非公式 Galaxy Buds Manager (Buds/Buds+)

([release tab](https://github.com/thepbone/galaxybudsclient/releases)からダウンロードができます。)

<p align="center">
  <img src="../screenshots/screencap.gif">
</p>

このプログラムはBudsが情報を送受信するときに使用するカスタムRFCommシリアルプロトコルの研究の結果物の一つです。もし、プロトコルの構造やシリアル通信の内容が知りたい方は、リバースエンジニアリングをしたすべての内容を記録した私のノートをご覧ください。

* [My Buds (2019) Notes](../GalaxyBudsRFCommProtocol.md)
* [My Buds Plus Notes](../Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md)

## 機能

**新しい機能** (既存機能を除く):

* タッチパッド: カスタム長押し動作 (プログラムを開く、イコライザーON/OFF、周辺の音の音量調整、...)<sup>[1]</sup>
* イヤホンを装着したときにメディアの再生を再開
* バッテリーの状態を表示するシステムトレイ
* ダッシュボードに詳しいセンサ情報を表示します。次を含みます:
  * 両方のイヤホンの内部ADCの電圧と電流 (アナログ-デジタル変換回路)
  * 両方のイヤホンの温度
  * より正確なバッテリー容量 (5%ごとの表記を削除)
* すべてのオンボードコンポーネントに対してセルフテストを遂行
* 様々な情報を表示(デバッグ)、次を含みます:
  * ハードウェアリビジョン
  * (タップ)ファームウェアバージョン
  * 両方のイヤホンのBluetoothアドレス
  * 両方のイヤホンのシリアル番号
  * ファームウェアのビルド情報 (コンパイルした日付、開発者の名前)
  * バッテリーのタイプ
  * その他のセンサー情報
* イコライザー: 'Dolby最適化' 機能の解禁
* タッチパッド: 音量を上げる/下げるを他のオプションと結合<sup>[1]</sup>

> <sup>[1]</sup> Galaxy Wearableアプリでタッチパッドの設定を変更する場合、この機能は自動的に無効化されます。
## インストール

**このプログラムは[.Net Framework](https://dotnet.microsoft.com/download/dotnet-framework/net461) 4.6.1かそれ以上のバージョンを要求します。**

[**ここ**](https://github.com/ThePBone/GalaxyBudsClient/releases)からフルインストールファイルのダウンロードができます。

正品**Galaxy Buds (2019)** や **Galaxy Buds+ (2020)** に対してすべての機能をサポートします。

![Downloads](https://img.shields.io/github/downloads/ThePBone/GalaxyBudsClient/total)

または、[@superbonaci](https://github.com/superbonaci)さんが提供したChocolateyパッケージの使用ができます:

```
choco install galaxybudsclient
```

## 翻訳

* [@Florize](https://github.com/Florize) - 韓国語、日本語翻訳

## 寄与

* [@superbonaci](https://github.com/superbonaci) - Chocolatey package
* [@githubcatw](https://github.com/githubcatw) - Connection dialog base



___

Bitcoin: 3EawSB3NfX6JQxKBBFYh6ZwHDWXtJB84Ly
