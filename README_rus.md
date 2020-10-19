# Galaxy Buds Client
Неофициальный менеджер для наушников Galaxy Buds (Buds, Buds+, Buds Live) для Windows 

(Вы можете найти загрузки в разделе [релизов](https://github.com/thepbone/galaxybudsclient/releases))

<p align="center">
  <img src="screenshots/screencap.gif">
</p>
Этот клиент является результатом моего исследования специальной версии последовательного протокола RFComm, который Buds используют для получения и отправки двоичных (конфигурационных) данных. Если вас интересует структура протокола и его последовательные сообщения, я рекомендую вам изучить мои заметки, которые я сделал при реверс-инжиниринге:

* [Заметки о Buds (2019), английский](GalaxyBudsRFCommProtocol.md)
* [Заметки о Buds Plus, английский](Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md)

## Функции

**Новые функции** (в дополнение к уже существующим):

* Сенсорная панель: Настраиваемые действия (нажать и задержать) для запуска приложений, управления эквалайзером, управления громкостью звукового фона, ...<sup>[1]</sup>
* Возобновить воспроизведение если наушники одеты
* Контекстное меню в системном трее со сведениями о заряде батареи
* Отображение расширенных данных сенсоров на главном экране, что включает:
  * Вольты и ток на встроенном АЦП (Аналого-цифровой преобразователь) обоих наушников
  * Температура обоих наушников
  * Более точный статус заряда (вместо шагов в 5 процентов)
* Выполнение самодиагностики всех компонентов на борту
* Отображение различной (отладочной) информации, включая:
  * Аппаратная версия
  * (Сенсорная панель) Версия прошивки
  * Адреса Bluetooth обоих наушников
  * Серйиные номера обоих наушников
  * Информация о сборке (Дата компиляции, Имя сборщика)
  * Тип батареи
  * Данные других сенсоров
* Сенсорная панель: Комбинируйте управление громкостью с другими действиями<sup>[1]</sup>
* Эквалайзер: разблокируйте функцию 'Оптимизировать для Dolby'<sup>[2]</sup> 

> <sup>[1]</sup> Учтите что приложение Wearable автоматически сбросит настройки этих функций при открытии настроек Сенсорной панели на телефоне
>
> <sup>[2]</sup> Только для Buds (2019)

## Установка

Вы можете [**загрузить**](https://github.com/ThePBone/GalaxyBudsClient/releases) полностью автоматический установочный пакет в [**разделе релизов**](https://github.com/ThePBone/GalaxyBudsClient/releases) этого репозитория!

*Эта программа требует [.Net Framework](https://dotnet.microsoft.com/download/dotnet-framework/net461) 4.6.1 или новее*

![Downloads](https://img.shields.io/github/downloads/ThePBone/GalaxyBudsClient/total)

Как вариант, вы можете использовать пакет [chocolatey](https://chocolatey.org/courses/getting-started/what-is-chocolatey) созданный [@superbonaci](https://github.com/superbonaci):

```
choco install galaxybudsclient
```

## Переводчики

* [@Florize](https://github.com/Florize) - Корейский и Японский переводы
* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Русский и Украинский переводы
* [@erenbektas](https://github.com/erenbektas) - Турецкий перевод
* [@kakkk](https://github.com/kakkk) - Китайский перевод
* [@efrenbg1](https://github.com/efrenbg1) вместе с Andrew Gonza - Испанский перевод

## Соучастники

* [@AndriesK](https://github.com/AndriesK) - Исправление ошибок при работе с Buds Live
* [@githubcatw](https://github.com/githubcatw) - Програмная основа для диалога подключения
* [@superbonaci](https://github.com/superbonaci) - Пакет Chocolatey

___

Посетите мой сайт: <https://timschneeberger.me>

