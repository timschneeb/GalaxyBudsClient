<p align="center">
  <a href="../README.md">English</a> | <a href="./README_chs.md">中文</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="./README_jpn.md">日本語</a> | <a href="./README_ukr.md">Українська</a> | <a href="./README_kor.md">한국어</a> | Česky<br>
    <sub>Upozornění: README soubory jsou spravovány překladately a proto mohou být zaostalé. Pro nejnovější informace se spoléhejte na anglickou verzi.</sub>
</p>

<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">Neoficiální manažer pro Buds, Buds+, Buds Live a Buds Pro</h4>
<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="GitHub počet stažení" src="https://img.shields.io/github/downloads/thepbone/galaxybudsclient/total">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
  	<img alt="GitHub vydání (nejnovější)" src="https://img.shields.io/github/v/release/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE">
      <img alt="Licence" src="https://img.shields.io/github/license/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="Platforma" src="https://img.shields.io/badge/platform-Windows/Linux-yellowgreen">
  </a>
</p>
<p align="center">
  <a href="#key-features">Klíčové vlastnosti</a> •
  <a href="#download">Stažení</a> •
  <a href="#how-it-works">Jak to funguje</a> •
  <a href="#contributing">Spolupáce</a> •
  <a href="#credits">Zásluhy</a> •
  <a href="#license">Licence</a> 
</p>


<p align="center">
    <a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
</p>

<p align="center">
    <a href="#"><img alt="Screenshot" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/screencap.gif"></a>
</p>


## Klíčové vlastnosti

Spravujte a ovládejte jakékoliv zařízení Samsung Galaxy Buds a integrujte je do vašeho počítače.

Kromě standartních funkcí známých z oficiální Androidí aplikace, Vám tento projekt pomáhá uvolnit plný potenciál sluchátek a implementovat nové funkce, jako je:

* Detailní statistiky baterie
* Diagnostika a tovární autotesty
* Spousta skrytých informací o ladění
* Přizpůsobitelné akce dlouhého dotyku
* a spousty dalších...

## Stažení

Získejte binární soubory pro Windows a Linux v sekci [vydání](https://github.com/ThePBone/GalaxyBudsClient/releases). Prosím přečtěte si poznámky k vydání před stažením (v angličtině).

<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

## Jak to funguje

Chcete-li používat bezdrátovou technologii Bluetooth, musí být zařízení schopno interpretovat určité profily Bluetooth, což jsou definice možných aplikací, a určit obecné chování, které zařízení podporující technologii Bluetooth používá ke komunikaci s jinými zařízeními Bluetooth.

Galaxy Buds definují dva profily Bluetooth: A2DP (Advanced Audio Distribution Profile) pro audio streaming / ovládání a SPP (Serial Port Profile) pro přenos binárního proudu. Výrobci často používají tento profil (který se spoléhá na protokol RFCOMM) k výměně konfiguračních dat, provádění aktualizací firmwaru nebo k odesílání dalších příkazů do zařízení Bluetooth.

I když je profil A2DP standardizovaný a dokumentovaný, formát skutečných binárních dat vyměňovaných tímto protokolem RFCOMM obvykle není dokumentován a chráněn.

Abych mohl zpětně analyzovat tento datový formát, začal jsem analýzou struktury binárního proudu posílaného sluchátky. Později jsem také rozebral oficiální aplikace Galaxy Buds pro Android, abych získal lepší přehled o vnitřním fungování těchto zařízení. Když jsem na tom pracoval, zapsal jsem své myšlenky do malého zápisníku. I když nejsou tak krásné, propojil jsem je níže. Mějte na paměti, že jsem se neobtěžoval psát každý jednotlivý detail. Zkontrolujte zdrojový kód a získejte podrobnější informace o struktuře protokolu.

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019): Poznámky</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus: Poznámky</a>
</p>

Při bližším pohledu na Galaxy Buds Plus jsem si také všiml některých neobvyklých funkcí, jako je režim ladění firmwaru, nepoužívaný režim párování a klíčenka Bluetooth. Zde jsem zdokumentoval tato zjištění:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Neobvyklé funkce</a>
</p>

V současné době zkoumám úpravy a reverzní inženýrství firmwaru pro Buds+. V době psaní tohoto článku mám dva nástroje pro načítání a analýzu pomocí oficiálních binárních souborů firmwaru. Podívejte se na ně zde:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">Stahovač Firmwaru</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">Extraktor Firmwaru</a>
</p>

## Spolupráce

Žádosti o funkce, nahlašování chyb, a požadavky o sjednocení kódů jakéhokolik typu jsou vždy vítány.

Pokud chcete nahlásit chybu nebo navrhnout Vaše nápady na tento projekt, jste vítáni k [otevření nového problému](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose) s vhodnou šablonou. [Navštivte naši wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues) pro detailní vysvětlení.

Pokud nám plánujete pomoci s překladem aplikace, [viz pokyny na naší wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations). Nejsou vyžadovány žádné znalosti programování, můžete vyzkoušet vaše překlady bez instalace jakýchkoliv vývojářských nástrojů před podání žádosti o sjednocení kódu.

If you want to contribute your own code, you can simply submit a plain pull request explaining you changes. For larger and complex contributions it would be nice if you could open an issue (or message me via Telegram [@thepbone](https://t.me/thepbone)) before starting to work on it.

## Zásluhy

#### Přispěvatelé

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Vydání šablon, wiki a překlady
* [@AndriesK](https://github.com/AndriesK) - Oprava chyby u Buds Live
* [@githubcatw](https://github.com/githubcatw) - Základní dialogové okno připojení

#### Překladatelé

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - ruský a ukrajinský překlad
* [@PlasticBrain](https://github.com/fhalfkg) - korejský a japonský překlad
* [@cozyplanes](https://github.com/cozyplanes) - korejský překlad
* [@erenbektas](https://github.com/erenbektas) - turecký překlad
* [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad) a [@pseudor](https://github.com/pseudor) - čínský překlad
* [@efrenbg1](https://github.com/efrenbg1) a Andrew Gonza - španělský překlad
* [@giovankabisano](https://github.com/giovankabisano) - indonéský překlad
* [@lucasskluser](https://github.com/lucasskluser) - portugalský překlad
* [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - italský překlad
* [@Buashei](https://github.com/Buashei) - polský překlad
* [@KatJillianne](https://github.com/KatJillianne) - vietnamský překlad
* [@joskaja](https://github.com/joskaja) a [@Joedmin580](https://github.com/Joedmin580) - český překlad

## Licence

Tento projekt je licencován pod [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE). Není přidružený se společností Samsung nebo pod jejich dohledem v jakémkoliv ohledu.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR 
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```

