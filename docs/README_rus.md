<p align="center">
  <a href="../README.md">English</a> | <a href="./docs/README_chs.md">中文</a> | Русский | <a href="./docs/README_jpn.md">日本語</a> | <a href="./docs/README_ukr.md">Українська</a> | <a href="./docs/README_kor.md">한국어</a><br>
    <sub>Внимание: файлы "О проекте" поддерживаются переводчиками и могут время от времени не соответствовать текущей версии. Для новейшей информации полагайтесь на англоязычный вариант.</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">Неофициальный менеджер для Buds, Buds+ и Buds Live</h4>
<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="Кол-во скачиваний с GitHub" src="https://img.shields.io/github/downloads/thepbone/galaxybudsclient/total">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
  	<img alt="Последняя версия на GitHub" src="https://img.shields.io/github/v/release/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE">
      <img alt="Лицензия" src="https://img.shields.io/github/license/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="Платформа" src="https://img.shields.io/badge/platform-Windows-yellowgreen">
  </a>
</p>
<p align="center">
  <a href="#основные-возможности">Основные возможности</a> •
  <a href="#скачать">Скачать</a> •
  <a href="#как-это-работает">Как это работает</a> •
  <a href="#внести-вклад">Внести вклад</a> •
  <a href="#дорожная-карта">Дорожная карта</a> •
  <a href="#имена">Имена</a> •
  <a href="#лицензия">Лицензия</a> 
</p>


<p align="center">
    <a href="#"><img alt="Screenshot" src="screenshots/screencap.gif"></a>
</p>

## Основные возможности

Настраивайте и управляйте любым устройством Samsung Galaxy Buds и интегрируйте их в свой компьютер.

Помимо стандартных функций, известных из официального приложения для Android, этот проект поможет вам раскрыть весь потенциал ваших наушников и реализует новые функции, такие как:

* Подробная статистика батареи
* Диагностика и заводское самотестирование
* Множество скрытой отладочной информации
* Настраиваемые действия удержания сенсорной панели
* и многое другое...

## Скачать

Загрузите файлы для Windows в разделе [релизы](https://github.com/ThePBone/GalaxyBudsClient/releases). Пожалуйста, прочтите примечания к релизу перед установкой.

<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="screenshots/download.png"></a>
</p>

## Как это работает

Чтобы использовать беспроводную технологию Bluetooth, устройство должно иметь возможность интерпретировать определенные профили Bluetooth, которые являются определениями возможных приложений и определять общее поведение, которое устройства с поддержкой Bluetooth используют для связи с другими устройствами. 

Galaxy Buds определяют два профиля Bluetooth: A2DP (Advanced Audio Distribution Profile) для потоковой передачи / управления аудио и SPP (Serial Port Profile) для передачи двоичного потока. Производители часто используют этот профиль (который основан на протоколе RFCOMM) для обмена данными конфигурации, выполнения обновлений прошивки или отправки других команд на устройство Bluetooth.

Несмотря на то, что профиль A2DP стандартизирован и задокументирован, формат фактических двоичных данных, которыми обменивается этот протокол RFCOMM, обычно не документируется и является собственностью компании производителя.

Чтобы реконструировать этот формат данных, я начал с анализа структуры двоичного потока, отправляемого наушниками. Позже я также разобрал официальные приложения Galaxy Buds для Android, чтобы лучше понять внутреннюю работу этих устройств. Работая над этим, я записывал свои мысли в небольшой блокнот. Хотя они не такие и красивые, я приложил их ниже. Имейте в виду, что я не стал записывать каждую деталь. Проверьте исходный код, чтобы получить более подробную информацию о структуре протокола.

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notes</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notes</a>
</p>


Присмотревшись к Galaxy Buds Plus, я также заметил некоторые необычные функции, такие как режим отладки прошивки, неиспользуемый режим сопряжения и дампер адресов Bluetooth. Я задокументировал эти результаты здесь:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Unusual features</a>
</p>

В настоящее время я занимаюсь модификацией и реверс-инжинирингом прошивки для Buds +. На момент написания у меня есть два инструмента для извлечения и анализа с помощью официальных двоичных файлов прошивки. Посмотрите их здесь:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">Firmware Downloader</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">Firmware Extractor</a>
</p>

## Внести вклад

Предложения функций, отчеты об ошибках и запросы на перенос (пулл реквесты) любого рода всегда приветствуются.

Если вы хотите сообщить об ошибках или предложить свои идеи для этого проекта, вы можете [подать запрос](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose) с подходящим шаблоном. [Посетите нашу вики](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues) для получения подробного объяснения.

Если вы планируете помочь нам в переводе этого приложения, [просмотрите инструкции в нашей вики](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations). Знания в области программирования не требуются, вы можете протестировать свои переводы без установки каких-либо инструментов разработки перед отправкой запроса на перенос.

Если вы хотите внести свой собственный код, вы можете просто отправить простой запрос на перенос с объяснением ваших изменений. Для более крупных и сложных вкладов было бы неплохо, если бы вы могли открыть запрос (или написать мне в Telegram [@thepbone](https://t.me/thepbone)), прежде чем начинать работу над ним.

## Дорожная карта

Выход версии 4.0 запланирован на конец 2020 и принесет кросс-платформенную поддержку. Вот что удалось реализовать уже сейчас:


- [x] Move to .NET Core 3.1
- [x] Linux: Implement native Bluetooth interface
- [ ] Windows: Implement native Bluetooth interface
- [ ] Windows/Linux: Native tray icon support (libappindicator)
- [ ] Cross-platform Bluetooth device selection dialog
- [ ] Port user interface to AvaloniaUI.NET
- [ ] Linux: Solve NVIDIA incompatibility with libSkia

Вклады в отношении этой дорожной карты очень ценятся. Новая реализайия доступна на ветке [`avalonia-rewrite`](https://github.com/ThePBone/GalaxyBudsClient/tree/avalonia-rewrite).


## Имена

#### Соучастники

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Шаблоны уведомлений о проблемах, вики и переводы
* [@AndriesK](https://github.com/AndriesK) - Исправление ошибок при работе с Buds Live
* [@githubcatw](https://github.com/githubcatw) - Програмная основа диалога подключения

#### Переводчики

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Русский и Украинский переводы
* [@PlasticBrain](https://github.com/fhalfkg) - Корейский и Японский переводы
* [@erenbektas](https://github.com/erenbektas) - Турецкий перевод
* [@kakkk](https://github.com/kakkk) , [@KevinZonda](https://github.com/KevinZonda) и [@ssenkrad](https://github.com/ssenkrad) - Китайский перевод
* [@efrenbg1](https://github.com/efrenbg1) и Andrew Gonza - Испанский перевод
* [@giovankabisano](https://github.com/giovankabisano) - Индонезийский перевод
* [@lucasskluser](https://github.com/lucasskluser) - Португальский перевод
* [@alb-p](https://github.com/alb-p) - Итальянский перевод
* [@Buashei](https://github.com/Buashei) - Польский перевод

## Лицензия

Этот проект распространяется по лицензии [GPLv3](LICENSE). Он никоим образом не связан с Samsung и не контролируется ею.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```

