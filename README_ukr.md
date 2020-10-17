# Galaxy Buds Client
Неофіційний менеджер для навушників Galaxy Buds (Buds, Buds+, Buds Live) для Windows 

(Ви можете знайти завантаження у розділі [релізів](https://github.com/thepbone/galaxybudsclient/releases))

<p align="center">
  <img src="screenshots/screencap.gif">
</p>

Цей клієнт є результатом мого дослідження спеціальної версії послідовного протоколу RFComm, який Buds використовують для отримання та відправки бінарних (конфігураційних) даних. Якщо вас цікавить структура протоколу і його послідовні повідомлення, я рекомендую вам вивчити мої замітки, які я зробив під час реверс-інжинірингу:

* [Нотатки про Buds (2019), англійська](GalaxyBudsRFCommProtocol.md)
* [Нотатки про Buds Plus, англійська](Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md)

## Функції

**Нові функції** (в доповнення уже існуючим):

* Сенсорна панель: Налаштованиі дії (натиснути і затримати) для запуску програм, керування еквалайзером, керування гучністю звукового фону, ...<sup>[1]</sup>
* Відновити програвання коли навушники одягнуті
* Контекстне меню в системному треї з даними про заряд батареї
* Відображення розширенних даних сенсорів на головному екрані, що включає:
  * Вольти і напруга на вбудованому АЦП (Аналого-цифровий перетворювач) обох навушників
  * Температура обох навушників
  * Більш точний статус заряду (замість кроків у 5 відсотків)
* Виконання самодіагностики усіх компонентів на борту
* Відображення різноманітної інформації (для налагодження), включаючи:
  * Апаратна версія
  * (Сенсорна панель) Версія прошивки
  * Адреса Bluetooth обох навушників
  * Серійні номери обох навушників
  * Інформація про збирання (Дата компіляції, Ім'я збиральника)
  * Тип батареї
  * Дані інших сенсорів
* Еквалайзер: розблокуйте функцію 'Оптимізувати для Dolby'
* Сенсорна панель: Комбінуйте керування гучністю разом з іншими діями<sup>[1]</sup>

> <sup>[1]</sup> Майте на увазі що додаток Wearable автоматично скине налаштування цих функцій при відкритті налаштувань Сенсорної панелі на телефоні
>
> <sup>[2]</sup> Тільки для Buds (2019)

## Встановлення

Ви можете [**завантажити**](https://github.com/ThePBone/GalaxyBudsClient/releases) повністю автоматичний пакет інсталятора в [**розділі релізів**](https://github.com/ThePBone/GalaxyBudsClient/releases) цього репозиторія!

*Ця программа вимагає [.Net Framework](https://dotnet.microsoft.com/download/dotnet-framework/net461) 4.6.1 або новіше*

![Downloads](https://img.shields.io/github/downloads/ThePBone/GalaxyBudsClient/total)

Як варіант, ви можете використати пакет [chocolatey](https://chocolatey.org/courses/getting-started/what-is-chocolatey) створений [@superbonaci](https://github.com/superbonaci):

```
choco install galaxybudsclient
```

## Перекладачі

* [@Florize](https://github.com/Florize) - Корейський та Японський переклади
* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Російський та Український переклади
* [@erenbektas](https://github.com/erenbektas) - Турецький переклад
* [@kakkk](https://github.com/kakkk) - Китайський переклад
* [@efrenbg1](https://github.com/efrenbg1) разом з Andrew Gonza - Іспанський переклад

## Співучасники

* [@AndriesK](https://github.com/AndriesK) - Виправлення помилок при роботі з Buds Live
* [@githubcatw](https://github.com/githubcatw) - Програмна основа для діалогу підключення
* [@superbonaci](https://github.com/superbonaci) - Пакет Chocolatey

___

Відвідайте мій сайт: <https://timschneeberger.me>

