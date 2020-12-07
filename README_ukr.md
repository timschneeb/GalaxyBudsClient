<p align="center">
  <a href="./README.md">English</a> | <a href="./README_chs.md">中文</a> | <a href="./README_rus.md">Русский</a> | <a href="./README_jpn.md">日本語</a> | Українська | <a href="./README_kor.md">한국어</a><br>
    <sub>Увага: файли "Про проект" підтримуються перекладачами і можуть час від часу не відповідати поточній версії. Для новішої інформації покладайтеся на англоязичний варіант.</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">Неофіційний менеджер для Buds, Buds+ і Buds Live</h4>
<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="Кількість завантажень з GitHub" src="https://img.shields.io/github/downloads/thepbone/galaxybudsclient/total">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
  	<img alt="Остання версія на GitHub" src="https://img.shields.io/github/v/release/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE">
      <img alt="Ліцензія" src="https://img.shields.io/github/license/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="Платформа" src="https://img.shields.io/badge/platform-Windows-yellowgreen">
  </a>
</p>
<p align="center">
  <a href="#основні-можливості">Основні можливості</a> •
  <a href="#завантажити">Завантажити</a> •
  <a href="#як-це-працює">Як це працює</a> •
  <a href="#зробити-внесок">Зробити внесок</a> •
  <a href="#дорожня-карта">Дорожня карта</a> •
  <a href="#імена">Імена</a> •
  <a href="#ліцензія">Ліцензія</a> 
</p>



<p align="center">
    <a href="#"><img alt="Screenshot" src="screenshots/screencap.gif"></a>
</p>

## Основні можливості

Налаштуйте та керуйте будь-яким пристроєм Samsung Galaxy Buds і інтегруйте їх в свій комп'ютер.

Крім стандартних функцій, відомих з офіційного додатку для Android, цей проект допоможе вам розкрити весь потенціал ваших навушників і реалізує нові функції, такі як:

* Детальна статистика батареї
* Діагностика і заводське самотестування
* Безліч прихованої налагоджувальної інформації
* Настроювані дії утримання сенсорної панелі
* і багато іншого...

## Завантажити

Завантажте файли для Windows у розділі [релізи](https://github.com/ThePBone/GalaxyBudsClient/releases). Будь ласка, прочитайте нотатки до релізу перед встановленням.

<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="screenshots/download.png"></a>
</p>

## Як це працює

Щоб використовувати бездротову технологію Bluetooth, пристрій повинен мати можливість інтерпретувати певні профілі Bluetooth, які є визначеннями можливих додатків і визначати загальну поведінку, яку пристрої з підтримкою Bluetooth використовують для зв'язку з іншими пристроями.

Galaxy Buds визначають два профілі Bluetooth: A2DP (Advanced Audio Distribution Profile) для потокової передачі / управління аудіо і SPP (Serial Port Profile) для передачі двійкового потоку. Виробники часто використовують цей профіль (який заснований на протоколі RFCOMM) для обміну даними конфігурації, виконання оновлень прошивки або відправки інших команд на пристрій Bluetooth.

Незважаючи на те, що профіль A2DP стандартизований і задокументований, формат фактичних двійкових даних, якими обмінюється цей протокол RFCOMM, зазвичай не документується і є власністю компанії виробника.

Для того, щоб перепроектувати цей формат даних, я розпочав з аналізу структури двійкового потоку, що надсилається навушниками. Пізніше я також розібрав офіційні програми Galaxy Buds для Android, щоб отримати глибше розуміння внутрішньої роботи цих пристроїв. Працюючи над цим, я записав свої думки в невеликий блокнот. Незважаючи на те, що вони не такі вже і красиві, я приклав їх нижче. Майте на увазі, що я не записував кожну окрему деталь. Перевірте початковий код, щоб отримати більш детальну інформацію про структуру протоколу.

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notes</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notes</a>
</p>


Придивившись до Galaxy Buds Plus, я також помітив деякі незвичайні функції, такі як режим налагодження прошивки, невикористаний режим сполучення і дампер адрес Bluetooth. Я задокументував ці результати тут:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Unusual features</a>
</p>

В даний час я займаюся модифікацією і реверс-інжинірингом прошивки для Buds +. На момент написання у мене є два інструменти для витягання і аналізу за допомогою офіційних двійкових файлів прошивки. Подивіться їх тут:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">Firmware Downloader</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">Firmware Extractor</a>
</p>

## Зробити внесок

Пропозиції функцій, звіти про помилки та запити на перенесення (пулл реквести) будь-якого роду завжди вітаються.

Якщо ви хочете повідомити про помилки або запропонувати свої ідеї для цього проекту, ви можете [подати запит](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose) з відповідним шаблоном. [Відвідайте нашу вікі](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues) для отримання докладного пояснення.

Якщо ви плануєте допомогти нам у перекладі цього додатка, [перегляньте інструкції в нашій вікі](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations). Знання в області програмування не потрібні, ви можете протестувати свої переклади без встановлення будь-яких інструментів розробки перед відправкою запиту на перенесення.

Якщо ви хочете внести свій власний код, ви можете просто відправити простий запит на перенесення з поясненням ваших змін. Для більших і складних вкладів було б непогано, якби ви могли відкрити запит (або написати мені в Telegram [@thepbone](https://t.me/thepbone)), перш ніж починати роботу над ним.

## Дорожня карта

Випуск версії 4.0 запланований на кінець 2020 і принесе кросс-платформену підтримку. Ось що вже вдалося реалізувати:


- [x] Move to .NET Core 3.1
- [x] Linux: Implement native Bluetooth interface
- [ ] Windows: Implement native Bluetooth interface
- [ ] Windows/Linux: Native tray icon support (libappindicator)
- [ ] Cross-platform Bluetooth device selection dialog
- [ ] Port user interface to AvaloniaUI.NET
- [ ] Linux: Solve NVIDIA incompatibility with libSkia

Внески по відношенню до цією дорожньої карти дуже цінуються. Нова реалізація доступна на гілці [`avalonia-rewrite`](https://github.com/ThePBone/GalaxyBudsClient/tree/avalonia-rewrite).


## Імена

#### Співучасники

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Шаблони повідомлень про проблеми, вікі та переклади
* [@AndriesK](https://github.com/AndriesK) - Виправлення помилок при роботі з Buds Live
* [@githubcatw](https://github.com/githubcatw) - Програмна база діалогу підключення

#### Перекладачі

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Російський та Український переклади
* [@PlasticBrain](https://github.com/fhalfkg) - Корейський та Японський переклади
* [@erenbektas](https://github.com/erenbektas) - Турецький переклад
* [@kakkk](https://github.com/kakkk) , [@KevinZonda](https://github.com/KevinZonda) і [@ssenkrad](https://github.com/ssenkrad) - Китайський переклад
* [@efrenbg1](https://github.com/efrenbg1) і Andrew Gonza - Іспанський переклад
* [@giovankabisano](https://github.com/giovankabisano) - Індонезійський переклад
* [@lucasskluser](https://github.com/lucasskluser) - Португальский переклад
* [@alb-p](https://github.com/alb-p) - Італійський переклад
* [@Buashei](https://github.com/Buashei) - Польський переклад

## Ліцензія

Цей проект розповсюджується за ліцензією [GPLv3](LICENSE). Він ніяким чином не пов'язаний з Samsung і не контролюється нею.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```

