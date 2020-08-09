# Galaxy Buds Client
Windows 비공식 Galaxy Buds Manager (Buds/Buds+)

(다운로드는 [release tab](https://github.com/thepbone/galaxybudsclient/releases)에서 가능합니다.)

<p align="center">
  <img src="screenshots/screencap.gif">
</p>

이 프로그램은 Buds가 정보(설정)를 송/수신할 때 사용하는 커스텀 RFComm 시리얼 프로토콜 연구의 결과물 중 하나입니다. 만약 프로토콜의 구조나 시리얼 통신의 내용에 대해 궁금하시다면, 리버스 엔지니어링을 했던 모든 내용을 기록한 제 노트를 확인하세요:

* [My Buds (2019) Notes](GalaxyBudsRFCommProtocol.md)
* [My Buds Plus Notes](Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md)

## 기능

**새로운 기능** (기존 기능 제외):

* 터치패드: 사용자 지정 터치/길게 누르기 동작 (외부 응용 프로그램 실행, 이퀄라이저 켜기/끄기, 주변 소리 듣기 음량 조절, ...)<sup>[1]</sup>
* 이어버드를 착용했을 시 미디어 재생 재개
* 배터리 상태를 표시하는 시스템 트레이
* 대시보드에 자세한 센서 정보를 표시합니다. 다음을 포함합니다:
  * 양쪽 이어버드 내부 ADC의 전압 및 전류 (아날로그-디지털 컨버터)
  * 양쪽 이어버드의 온도
  * 더 정확한 배터리 용량 (5% 단위 기준 표시를 대체)
* 모든 온보드 컴포넌트에 대해 셀프 테스트 수행
* 다양한 정보를 표시(디버그), 다음을 포함합니다:
  * 하드웨어 리비전
  * (터치) 펌웨어 버전
  * 양쪽 이어버드의 Bluetooth 주소
  * 양쪽 이어버드의 시리얼 번호
  * 펌웨어 빌드 정보 (컴파일 날짜, 개발자 이름)
  * 배터리 종류
  * 기타 센서 정보
* 이퀄라이저: 'Dolby 최적화' 기능 해제
* 터치패드: 음량 올리기/내리기를 다른 옵션과 통합 <sup>[1]</sup>

> <sup>[1]</sup> Galaxy Wearable 앱에서 터치패드 설정을 변경할 경우 이 기능은 자동적으로 초기화됩니다.
## 설치

**이 프로그램은 [.Net Framework](https://dotnet.microsoft.com/download/dotnet-framework/net461) 4.6.1 이나 그 이상의 버전을 필요로 합니다.**

[**여기**](https://github.com/ThePBone/GalaxyBudsClient/releases)에서 전체 설치 파일을 다운로드할 수 있습니다.

정품 **Galaxy Buds (2019)** 와 **Galaxy Buds+ (2020)** 에 대해 모든 기능을 지원합니다.

![Downloads](https://img.shields.io/github/downloads/ThePBone/GalaxyBudsClient/total)

또는, [@superbonaci](https://github.com/superbonaci) 님이 제공하신 Chocolatey 패키지를 사용할 수 있습니다:

```
choco install galaxybudsclient
```

## 번역

* [@Florize](https://github.com/Florize) - 한국어, 일본어 번역

## 기여

* [@superbonaci](https://github.com/superbonaci) - Chocolatey package
* [@githubcatw](https://github.com/githubcatw) - Connection dialog base



___

Bitcoin: 3EawSB3NfX6JQxKBBFYh6ZwHDWXtJB84Ly
