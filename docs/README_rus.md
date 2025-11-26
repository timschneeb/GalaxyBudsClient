<p align="center">
  English | <a href="/docs/README_chs.md">中文(简体)</a> | <a href="/docs/README_cht.md">中文(繁體)</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_tr.md">Türkçe</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | <a href="/docs/README_pt.md">Português</a> | <a href="/docs/README_vnm.md">Tiếng Việt</a> <br>
    <sub>Внимание: файлы readme поддерживаются переводчиками и могут время от времени устаревать. Для самой актуальной информации ориентируйтесь на английскую версию.</sub>
</p>
<h1 align="center">
  Клиент Galaxy Buds
  <br>
</h1>
<h4 align="center">Неофициальный менеджер для устройств Samsung Galaxy Buds</h4>
<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
    <img alt="Количество загрузок GitHub" src="https://img.shields.io/github/downloads/thepbone/galaxybudsclient/total">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
   <img alt="Последний релиз на GitHub" src="https://img.shields.io/github/v/release/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/LICENSE">
      <img alt="Лицензия" src="https://img.shields.io/github/license/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
    <img alt="Платформы" src="https://img.shields.io/badge/platform-Windows/macOS/Linux/Android-yellowgreen">
  </a>
</p>
<p align="center">
  <a href="#основные-функции">Основные функции</a> •
  <a href="#скачать">Скачать</a> •
  <a href="#как-это-работает">Как это работает</a> •
  <a href="#вклад-в-разработку">Вклад в разработку</a> •
  <a href="#участники">Участники</a> •
  <a href="#лицензия">Лицензия</a>
</p>

<p align="center">
  <span><a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
  <a href="#"><img alt="Screenshot" src="https://github.com/timschneeb/GalaxyBudsClient/raw/master/screenshots/app_dark.png"></a></span>
</p>

## Основные функции

Настройка и управление устройствами Samsung Galaxy Buds и интеграция их в ваш рабочий стол.

Помимо стандартных функций, известных из официального приложения для Android, этот проект помогает раскрыть полный потенциал ваших наушников и внедрить новые функции, такие как:

- Подробная статистика заряда батареи
- Диагностика и тесты на заводе
- Множество скрытой отладочной информации
- Настраиваемые действия при длительном нажатии
- Прошивка, откат (Buds+, Buds Pro)
- и многое другое...

Если вы ищете старые версии прошивок, ознакомьтесь с этим архивом: [https://github.com/timschneeb/galaxy-buds-firmware-archive](https://github.com/timschneeb/galaxy-buds-firmware-archive#galaxy-buds-firmware-archive)

## Скачать

Доступны несколько пакетов для Linux:
- [Flatpak (для всех дистрибутивов Linux)](#flatpak)
- [Пакет AUR (для Arch Linux)](#aur-package)

Загрузите исполняемые файлы для Windows в разделе [релизов](https://github.com/timschneeb/GalaxyBudsClient/releases). Пожалуйста, прочитайте примечания к релизу перед установкой.

Скачайте версию для ПК здесь:
<p align="center">
    <a href="https://github.com/timschneeb/GalaxyBudsClient/releases"><img alt="Скачать" src="https://github.com/timschneeb/GalaxyBudsClient/raw/master/screenshots/download.png"></a>
</p>

Скачайте версию для Android (платная):
<p align="center">
  <a href='https://play.google.com/store/apps/details?id=me.timschneeberger.galaxybudsclient&utm_source=github&pcampaignid=pcampaignidMKT-Other-global-all-co-prtnr-py-PartBadge-Mar2515-1'>
    <img width="300" alt='Доступно в Google Play' src='https://play.google.com/intl/en_us/badges/static/images/badges/en_badge_web_generic.png'/>
  </a>
</p>

### Flatpak

Универсальные двоичные пакеты для всех дистрибутивов Linux. Версия Flatpak не поддерживает автозапуск, если он не настроен вручную. Вы можете использовать команду `galaxybudsclient /StartMinimized` для запуска приложения в фоновом режиме при старте системы.

Доступно для скачивания на FlatHub: https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient
```
flatpak install me.timschneeberger.GalaxyBudsClient
```

<a href='https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient'><img width='240' alt='Скачать на Flathub' src='https://dl.flathub.org/assets/badges/flathub-badge-en.png'/></a>

> **Примечание**: Flatpaks работают в изолированной среде. Это приложение может получить доступ только к `~/.var/app/me.timschneeberger.GalaxyBudsClient/` по умолчанию.

### AUR package

[Пакет AUR](https://aur.archlinux.org/packages/galaxybudsclient-bin/) для Arch Linux, поддерживаемый @joscdk, также доступен:
```
yay -S galaxybudsclient-bin
```

### winget

Пакет для Windows также можно установить с помощью менеджера пакетов Windows (winget):
```
winget install timschneeb.GalaxyBudsClient
```

## Как это работает

Для использования беспроводной технологии Bluetooth устройство должно иметь возможность интерпретировать конкретные профили Bluetooth, которые позволяют Bluetooth-устройствам эффективно обмениваться информацией друг с другом.

Наушники Galaxy Buds определяют два профиля Bluetooth: A2DP (Advanced Audio Distribution Profile) для передачи и управления аудио и SPP (Serial Port Profile) для передачи бинарных данных. Производители часто используют этот профиль (который зависит от протокола RFCOMM) для обмена конфигурационными данными, выполнения прошивки или отправки других команд на Bluetooth-устройство.

Несмотря на то, что профиль A2DP стандартизирован и задокументирован, формат бинарных данных, обмениваемых через этот протокол RFCOMM, обычно является собственностью производителя.

Для обратной инженерии формата данных я начал с анализа структуры бинарных данных, отправляемых наушниками. Позже я также разобрал официальные приложения Galaxy Buds для Android, чтобы получить более подробное представление о внутренней работе этих устройств. Ниже приведены некоторые (неполные) заметки, которые я сделал. Для получения более подробной информации о структуре протокола обратитесь к исходному коду.

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Заметки по Galaxy Buds (2019)</a> •
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Заметки по Galaxy Buds Plus</a>
</p>

Исследуя наушники Galaxy Buds Plus, я также заметил несколько необычных функций, таких как режим отладки прошивки, неиспользуемый режим сопряжения и дампер Bluetooth-ключей. Я задокументировал эти находки здесь:

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Необычные функции</a>
</p>

В настоящее время я занимаюсь модификацией и обратной инженерией прошивки для Buds+. На момент написания у меня есть два инструмента для извлечения и анализа официальных прошивок. Вы можете ознакомиться с ними здесь:

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsFirmwareDownloader">Firmware Downloader</a> •
  <a href="https://github.com/timschneeb/GalaxyBudsFirmwareExtractor">Firmware Extractor</a>
</p>

Получайте данные о трекинге головы в реальном времени с наушников Buds Pro с помощью этого скрипта: [timschneeb/BudsPro-Headtracking](https://github.com/timschneeb/BudsPro-Headtracking)

## Вклад в разработку

Запросы на добавление функций, отчеты об ошибках и запросы на внесение изменений всегда приветствуются.

Если вы хотите сообщить об ошибках или предложить свои идеи для этого проекта, вы можете [открыть новое задание](https://github.com/timschneeb/GalaxyBudsClient/issues/new/choose) с подходящим шаблоном. [Посетите нашу вики](https://github.com/timschneeb/GalaxyBudsClient/wiki/2.-How-to-submit-issues) для подробного объяснения.

Если вы планируете помочь нам в переводе этого приложения, [посетите инструкции на нашей вики](https://github.com/timschneeb/GalaxyBudsClient/wiki/3.-How-to-help-with-translations). Не требуется знание программирования, вы можете проверить свои собственные переводы без установки каких-либо средств разработки, прежде чем отправить запрос на внесение изменений. Прогресс существующих переводов можно найти [здесь](https://github.com/timschneeb/GalaxyBudsClient/blob/master/meta/translations.md).

Если вы хотите внести свой код, вы можете просто отправить запрос на внесение изменений с пояснением ваших изменений. Для крупных и сложных вкладов было бы хорошо, если бы вы могли открыть задачу (или связаться со мной через Telegram [@thepbone](https://t.me/thepbone)) перед началом работы.

## Участники

### Участники

- [@nift4](https://github.com/nift4) - Поддержка macOS и исправление ошибок
- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Шаблоны задач, вики и переводы
- [@AndriesK](https://github.com/AndriesK) - Исправление ошибок Buds Live
- [@TheLastFrame](https://github.com/TheLastFrame) - Иконки Buds Pro
- [@githubcatw](https://github.com/githubcatw) - База диалога подключения
- [@GaryGadget9](https://github.com/GaryGadget9) - Пакет WinGet
- [@joscdk](https://github.com/joscdk) - Пакет AUR

### Переводчики

- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Русский и украинский перевод
- [@PlasticBrain](https://github.com/fhalfkg) - Корейский и японский перевод
- [@cozyplanes](https://github.com/cozyplanes) - Корейский перевод
- [@corydalis10](https://github.com/corydalis10) - Корейский перевод
- [@erenbektas](https://github.com/erenbektas) и [@Eta06](https://github.com/Eta06) - Турецкий перевод
- [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad), [@pseudor](https://github.com/pseudor) и [@YexuanXiao](https://github.com/YexuanXiao) - Китайский перевод
- [@YiJhu](https://github.com/YiJhu) - Китайский (традиционный) перевод
- [@efrenbg1](https://github.com/efrenbg1) и Andrew Gonza - Испанский перевод
- [@giovankabisano](https://github.com/giovankabisano) - Индонезийский перевод
- [@lucasskluser](https://github.com/lucasskluser) и [@JuanFariasDev](https://github.com/juanfariasdev) - Португальский перевод
- [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - Итальянский перевод
- [@Buashei](https://github.com/Buashei) - Польский перевод
- [@KatJillianne](https://github.com/KatJillianne) и [@thelegendaryjohn](https://github.com/thelegendaryjohn) - Вьетнамский перевод
- [@joskaja](https://github.com/joskaja) и [@Joedmin](https://github.com/Joedmin) - Чешский перевод
- [@Benni0109](https://github.com/Benni0109), [@TheLastFrame](https://github.com/TheLastFrame), [@timschneeb](https://github.com/timschneeb) - Немецкий перевод
- [@nikossyr](https://github.com/nikossyr) - Греческий перевод
- [@grigorem](https://github.com/grigorem) - Румынский перевод
- [@tretre91](https://github.com/tretre91) - Французский перевод
- [@Sigarya](https://github.com/Sigarya) - Ивритский перевод
- [@domroaft](https://github.com/domroaft) - Венгерский перевод
- [@lampi8426](https://github.com/lampi8426) - Нидерландский перевод

### Сервисы

- [Cloudflare](https://www.cloudflare.com/) - Обеспечивает защиту API GalaxyBudsClient и предоставляет Pro-лицензию

### Ассеты

- Иконка наушников, используемая в Android-приложении, создана [Archival](https://www.flaticon.com/authors/archival) с [Flaticon](https://www.flaticon.com/).

## Лицензия

Этот проект лицензирован на условиях [GPLv3](https://github.com/timschneeb/GalaxyBudsClient/blob/master/LICENSE). Он не связан с компанией Samsung и не находится под ее контролем ни в каком виде.

```
ПРОГРАММНОЕ ОБЕСПЕЧЕНИЕ ПРЕДОСТАВЛЯЕТСЯ «КАК ЕСТЬ», БЕЗ КАКИХ-ЛИБО ГАРАНТИЙ, ЯВНЫХ ИЛИ ПОДРАЗУМЕВАЕМЫХ,
ВКЛЮЧАЯ, ПОМИМО ПРОЧЕГО, ГАРАНТИИ ТОВАРНОЙ ПРИГОДНОСТИ, ПРИГОДНОСТИ ДЛЯ ОПРЕДЕЛЕННОЙ ЦЕЛИ И ОТСУТСТВИЯ НАРУШЕНИЙ.

НИ ПРИ КАКИХ ОБСТОЯТЕЛЬСТВАХ АВТОРЫ ИЛИ ПРАВООБЛАДАТЕЛИ НЕ НЕСУТ ОТВЕТСТВЕННОСТИ ЗА ЛЮБЫЕ ПРЕТЕНЗИИ, УЩЕРБ
ИЛИ ИНЫЕ ОБЯЗАТЕЛЬСТВА, ВОЗНИКАЮЩИЕ В РЕЗУЛЬТАТЕ ДОГОВОРНОГО ОБЯЗАТЕЛЬСТВА, ДЕЛИКТА ИЛИ ИНЫМ ОБРАЗОМ,
СВЯЗАННЫМ С ПРОГРАММНЫМ ОБЕСПЕЧЕНИЕМ, ИСПОЛЬЗОВАНИЕМ ИЛИ ИНЫМ ДРУГИМ ОБРАЩЕНИЕМ.
```
