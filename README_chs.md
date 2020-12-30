# Galaxy Buds Client

Windows 非官方的 Galaxy Buds Manager (Buds, Buds+, Buds Live)

**此 README 同样在以下语言可用 [英语](/README.md), [韩语](/README_kor.md), [日语](/README_jpn.md), [俄语](/README_rus.md) 和 [乌克兰语](/README_ukr.md)!**

(在 [release tab](https://github.com/thepbone/galaxybudsclient/releases) 你可以找到你需要的下载内容。)

<p align="center">
  <img src="screenshots/screencap.gif">
</p>

这个客户端是我研究自定义 RFComm 串行协议的产物，Buds 用来接收和发送二进制（配置）数据。如果你对协议的结构和它的串行消息感兴趣，我建议你看看我在逆向工程时做的笔记：

* [Buds (2019) Notes](GalaxyBudsRFCommProtocol.md)
* [Buds Plus Notes](Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md)
* [Buds Plus: Undocumented calls](https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md)

## 特点

**新功能**（除现有功能外）：

* 触摸板：可自定义点击和按住操作（启动应用程序、切换均衡器、改变环境音量等）<sup>[1]</sup>
* 如果佩戴了耳塞，恢复媒体播放。
* 带电池统计的托盘栏菜单
* 在仪表盘上显示详细的传感器统计信息，包括：
  * 内置 ADC（模数转换器）的电压、电流和温度。
  * 精确的电池百分比（而不是5%为单位的）
* 对所有内部部件进行自检
* 显示多种（调试）信息，包括：
  * 硬件/软件/触摸固件版本
  * 蓝牙地址、序列号
  * 固件构建信息（编译日期、开发者名称）
  * 电池类型
  * 其他传感器数据
* 触摸板：合并音量加/减与其他选项<sup>[1]</sup>
* 均衡器：解锁“为优化杜比”功能<sup>[2]</sup>

> <sup>[1]</sup> 三星官方 Wearable 应用无法使用这些功能
>
> <sup>[2]</sup> 仅适用于原版 Buds (2019)

## 安装 ![下载](https://img.shields.io/github/downloads/ThePBone/GalaxyBudsClient/total)

你可以在本项目的 [**release section**](https://github.com/ThePBone/GalaxyBudsClient/releases) 下载一个全自动安装程序！

*本程序需要 [.Net Framework](https://dotnet.microsoft.com/download/dotnet-framework/net461) 4.6.1 或更高*

## 翻译者

* [@PlasticBrain](https://github.com/fhalfkg) - 韩语和日语翻译
* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - 俄语和乌克兰语翻译
* [@erenbektas](https://github.com/erenbektas) - 土耳其语翻译
* [@kakkk](https://github.com/kakkk) ， [@KevinZonda](https://github.com/KevinZonda) 与 [@ssenkrad](https://github.com/ssenkrad) - 汉语翻译
* [@efrenbg1](https://github.com/efrenbg1) 与 Andrew Gonza - 西班牙语翻译
* [@giovankabisano](https://github.com/giovankabisano) - 印度尼西亚语翻译
* [@lucasskluser](https://github.com/lucasskluser) - 葡萄牙语翻译
* [@alb-p](https://github.com/alb-p) - 意大利语翻译

## 构建者

* [@AndriesK](https://github.com/AndriesK) - Buds Live bug 修正
* [@githubcatw](https://github.com/githubcatw) - 基础连接会话
* [@superbonaci](https://github.com/superbonaci) - Chocolatey package
___

欢迎查看我的网站： <https://timschneeberger.me>
