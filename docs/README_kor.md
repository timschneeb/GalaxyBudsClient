<p align="center">
    <a href="../README.md">English</a> | <a href="./README_chs.md">中文</a> | <a href="./README_rus.md">Русский</a> | <a href="./README_jpn.md">日本語</a> | <a href="./README_ukr.md">Українська</a> | 한국어<br>
    <sub>주의: readme 파일들은 번역자들에 의해 업데이트되며 시간이 지나면 최신 내용이 아니게 될 수 있습니다. 새로운 기능은 가장 먼저 영어 버전에 기재됩니다.</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">버즈, 버즈 플러스와 버즈 라이브를 위한 비공식 매니저 프로그램</h4>
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
    <img alt="Platform" src="https://img.shields.io/badge/platform-Windows-yellowgreen">
  </a>
</p>
<p align="center">
  <a href="#주요-기능">주요 기능</a> •
  <a href="#다운로드">다운로드</a> •
  <a href="#동작-방식">동작 방식</a> •
  <a href="#기여">기여</a> •
  <a href="#제작자">제작자</a> •
  <a href="#라이선스">라이선스</a> 
</p>



<p align="center">
    <a href="#"><img alt="Screenshot" src="../screenshots/screencap.gif"></a>
</p>

## 주요 기능

데스크탑에서 삼성 갤럭시 버즈 디바이스의 설정 및 제어가 가능합니다.

알려진 공식 안드로이드 앱의 기본 기능과 별도로, 이 프로젝트는 이어버즈의 모든 잠재 기능들과 새로운 기능들을 사용할 수 있도록 도와줍니다. 예를 들면:

* 자세한 배터리 정보
* 기능 점검과 공장 자체 테스트
* 숨겨진 디버깅 정보를 로드
* 사용자 설정이 가능한 길게 누르기 동작
* 그 이외 다른 기능들...

## 다운로드

Windows 바이너리는 [release](https://github.com/ThePBone/GalaxyBudsClient/releases)에서 다운로드가 가능합니다. 설치하기 전에 릴리즈 노트를 읽어 주세요.

<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="../screenshots/download.png"></a>
</p>

## 동작 방식

Bluetooth 무선 기술을 사용하기 위해, 디바이스는 동작 가능한 앱과 디바이스가 다른 Bluetooth 디바이스와 통신하는 데 사용하는 일반적인 동작을 정의한 Bluetooth 프로필을 해석할 수 있어야 합니다.

갤럭시 버즈는 두 개의 Bluetooth 프로필을 정의합니다: A2DP (Advanced Audio Distribution Profile) 오디오 스트리밍/제어와 바이너리 스트림 통신을 위한 SPP (Serial Port Profile). 제조사들은 설정 데이터를 주고 받고, 펌웨어 업데이트나 기타 명령을 다른 Bluetooth 장치로 전송하기 위해 보통 이 프로필들을 사용하는 경우가 많습니다.

A2DP 프로필이 표준화 및 문서화되더라도, RFCOMM 프로토콜로 교환되는 실제 바이너리 데이터의 형식은 일반적으로 문서화되지 않은 독자적 형식입니다.

이 데이터 형식을 역분석하기 위해, 저는 이어버즈로부터 전송되는 바이너리 스트림을 분석하기 시작했습니다. 그 후에는 디바이스들의 내부 동작을 더 자세히 알기 위해 안드로이드 공식 갤럭시 버즈 앱을 분석했습니다. 이 일을 하는 동안, 저는 제가 했던 생각들을 기록했습니다. 별로 아름다운 기록은 아니지만, 밑에 링크를 기재했습니다. 제가 상세한 내용 하나 하나에 대해서 모두 기록하지는 않았음을 알아두세요. 프로토콜에 대해 더 자세한 정보를 알고 싶다면 소스 코드를 확인하세요.

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">갤럭시 버즈 (2019) 기록</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">갤럭시 버즈 플러스 기록</a>
</p>

갤럭시 버즈 플러스를 유심히 분석하면서, 저는 펌웨어 디버그 모드와 쓰이지 않은 페어링 모드, Bluetooth 키 덤퍼와 같은 특이한 기능을 찾아냈습니다. 그 기능들에 대한 내용도 아래의 링크에 기록했습니다: 

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">갤럭시 버즈 플러스: 특이한 기능들</a>
</p>

현재, 저는 버즈 플러스의 펌웨어를 수정하고 역분석하려고 합니다. 작업 시 펌웨어 바이너리를 가져와 분석할 수 있는 툴이 있습니다. 아래의 링크를 참조하세요:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">펌웨어 다운로더</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">펌웨어 추출기</a>
</p>

## 기여

기능 요청, 버그 보고, Pull Requests 등 어떤 형태의 기여도 환영합니다.

버그를 제보하거나 아이디어를 제시하고 싶으시면, 템플릿과 함께 제공되는 [새로운 Issue 생성](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose)을 이용해 주세요. [위키](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues)를 방문해 자세한 설명을 참고하세요.

이 프로그램의 번역에 도움을 제공하고 싶으시다면, [위키의 설명](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations)을 참조하세요. 프로그래밍 지식을 요구하지 않으며, Pull Request 전 어떤 개발 툴의 설치도 없이 번역을 테스트할 수 있습니다.

소스 코드에 기여하고 싶으시다면, 변경한 내용에 대한 Pull Request를 생성하면 됩니다. 프로그램에 대한 크거나 민감한 기여 사항은 작업을 시작하기 전에 Issue를 생성해 주세요. (또는 텔레그램 [@thepbone](https://t.me/thepbone)으로 연락)

## 제작자

#### 기여

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Issue 템플릿, 위키 제작과 번역
* [@AndriesK](https://github.com/AndriesK) - 버즈 라이브 버그 픽스
* [@githubcatw](https://github.com/githubcatw) - 연결 알림 기반 제작

#### 번역

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - 러시아어, 우크라이나어 번역
* [@cozyplanes](https://github.com/cozyplanes) - 한국어 번역
* [@PlasticBrain](https://github.com/fhalfkg) - 한국어, 일본어 번역
* [@erenbektas](https://github.com/erenbektas) - 터키어 번역
* [@kakkk](https://github.com/kakkk) , [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad) - 중국어 번역
* [@efrenbg1](https://github.com/efrenbg1), Andrew Gonza - 스페인어 번역
* [@giovankabisano](https://github.com/giovankabisano) - 마인어 번역
* [@lucasskluser](https://github.com/lucasskluser) - 포르투갈어 번역
* [@alb-p](https://github.com/alb-p) - 이탈리아어 번역
* [@Buashei](https://github.com/Buashei) - 폴란드어 번역

## 라이선스

이 프로젝트는 [GPLv3](../LICENSE) 라이선스를 따릅니다. 삼성과 관련되지 않았으며 그 어떤 권고나 제한도 받지 않습니다.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR 
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```

