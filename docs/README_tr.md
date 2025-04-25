<p align="center">
   <a href="../README.md">English</a> | <a href="/docs/README_chs.md">中文(简体)</a> | <a href="/docs/README_cht.md">中文(繁體)</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | Türkçe | <a href="/docs/README_gr.md">Ελληνικά</a> | <a href="/docs/README_pt.md">Português</a> | <a href="/docs/README_vnm.md">Tiếng Việt</a> <br>
    <sub>Uyarı: Benioku dosyaları çevirmenler tarafından yönetilmektedir ve zaman zaman güncelliğini yitirebilir. En güncel bilgiler için İngilizce sürüme bakın.</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">Buds, Buds+, Buds Live ve Buds Pro için resmi olmayan bir yönetici</h4>
<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
    <img alt="GitHub indirme sayısı" src="https://img.shields.io/github/downloads/thepbone/galaxybudsclient/total">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
   <img alt="GitHub sürümü (tarihe göre en son)" src="https://img.shields.io/github/v/release/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/LICENSE">
      <img alt="Lisans" src="https://img.shields.io/github/license/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
    <img alt="Platform" src="https://img.shields.io/badge/platform-Windows/Linux-yellowgreen">
  </a>
</p>
<p align="center">
  <a href="#anahtar-özellikler">Anahtar Özellikler</a> •
  <a href="#indir">İndir</a> •
  <a href="#nasıl-çalışır">Nasıl Çalışır</a> •
  <a href="#katkıda-bulunma">Katkıda Bulunma</a> •
  <a href="#emegi-gecenler">Emeği Geçenler</a> •
  <a href="#lisans">Lisans</a>
</p>

<p align="center">
    <a href="https://ko-fi.com/H2H83E5J3"><img alt="Ekran Görüntüsü" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
</p>

<p align="center">
    <a href="#"><img alt="Ekran Görüntüsü" src="https://github.com/timschneeb/GalaxyBudsClient/blob/master/screenshots/app_dark.png"></a>
</p>

## Anahtar Özellikler

Herhangi bir Samsung Galaxy Buds cihazını yapılandırın, kontrol edin ve masaüstünüze entegre edin.

Resmi Android uygulamasından bilinen standart özelliklerin yanı sıra, bu proje kulaklıklarınızın tüm potansiyelini ortaya çıkarmanıza yardımcı olur ve aşağıdaki gibi yeni işlevler uygular:

- Ayrıntılı pil istatistikleri
- Teşhis ve fabrika kendi kendini testleri
- Bir sürü gizli hata ayıklama bilgisi
- Özelleştirilebilir uzun basma dokunma eylemleri
- Ürün yazılımı flaş etme, düşürme (Buds+, Buds Pro)
- ve çok daha fazlası...

Daha eski ürün yazılımı ikili dosyalarını arıyorsanız, buraya bir göz atın: [https://github.com/timschneeb/galaxy-buds-firmware-archive](https://github.com/timschneeb/galaxy-buds-firmware-archive#galaxy-buds-firmware-archive)

## Indir

Birkaç Linux paketi mevcuttur:

- [Flatpak (Tüm Linux dağıtımları)](#flatpak)
- [AUR paketi (Arch Linux)](#aur-paketi)

[Sürüm](https://github.com/timschneeb/GalaxyBudsClient/releases) bölümünde Windows için ikili dosyaları edinin. Lütfen kurulumdan önce sürüm notlarını okuyun:

<p align="center">
    <a href="https://github.com/timschneeb/GalaxyBudsClient/releases"><img alt="İndir" src="https://github.com/timschneeb/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

### Flatpak

Tüm Linux dağıtımları için evrensel ikili paketler. Bu, GalaxyBudsClient'ı Linux'a yüklemenin önerilen yoludur.

FlatHub'dan indirilebilir: https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient

```
flatpak install me.timschneeberger.GalaxyBudsClient
```

<a href='https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient'><img width='240' alt='Download on Flathub' src='https://dl.flathub.org/assets/badges/flathub-badge-en.png'/></a>

> **Not**: Flatpak'ler kum kutuludur. Bu uygulama varsayılan olarak yalnızca `~/.var/app/me.timschneeberger.GalaxyBudsClient/` dizinine erişebilir.

### AUR paketi

@joscdk tarafından sağlanan Arch Linux için bir [AUR paketi](https://aur.archlinux.org/packages/galaxybudsclient-bin/) de mevcuttur:

```
yay -S galaxybudsclient-bin
```

### winget

Windows paketi, Windows Paket Yöneticisi (winget) ile de yüklenebilir

```
winget install timschneeb.GalaxyBudsClient
```

## Nasıl Çalışır

Bluetooth kablosuz teknolojisini kullanmak için, bir cihazın Bluetooth cihazlarının birbirleriyle verimli bir şekilde iletişim kurmasını sağlayan belirli Bluetooth profillerini yorumlayabilmesi gerekir.

Galaxy Buds iki Bluetooth profili tanımlar: Ses akışı/kontrolü için A2DP (Gelişmiş Ses Dağıtım Profili) ve ikili akışları iletmek için SPP (Seri Bağlantı Noktası Profili). Üreticiler genellikle bu profili (RFCOMM protokolüne dayanan) yapılandırma verilerini değiştirmek, ürün yazılımı güncellemeleri gerçekleştirmek veya Bluetooth cihazına diğer komutları göndermek için kullanır.

A2DP profili standartlaştırılmış ve belgelenmiş olsa da, bu RFCOMM protokolü tarafından değiştirilen ikili verilerin biçimi genellikle tescillidir.

Bu veri biçimini tersine mühendislik yapmak için, kulaklıklar tarafından gönderilen ikili akışın yapısını analiz ederek başladım. Daha sonra, bu cihazların iç işleyişine daha fazla bilgi edinmek için Android için resmi Galaxy Buds uygulamalarını da söktüm. Aşağıda aldığım bazı (eksik) notları bulabilirsiniz. Protokolün yapısı hakkında daha ayrıntılı bilgi edinmek için kaynak koduna bakın.

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notları</a> •
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notları</a>
</p>

Galaxy Buds Plus'a daha yakından baktığımda, bir ürün yazılımı hata ayıklama modu, kullanılmayan bir eşleştirme modu ve bir Bluetooth anahtar dökümü gibi bazı alışılmadık özellikler de fark ettim. Bu bulguları burada belgeledim:

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Olağandışı özellikler</a>
</p>

Şu anda, Buds+ için ürün yazılımını değiştirme ve tersine mühendislik yapma konusunu araştırıyorum. Bu yazıyı yazarken, resmi ürün yazılımı ikili dosyalarını almak ve analiz etmek için iki araç oluşturdum. Bunları buradan kontrol edin:

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsFirmwareDownloader">Ürün Yazılımı İndirme Aracı</a> •
  <a href="https://github.com/timschneeb/GalaxyBudsFirmwareExtractor">Ürün Yazılımı Ayıklama Aracı</a>
</p>

Bu betiği kullanarak Buds Pro'nuzdan gerçek zamanlı olarak baş takip verilerini akışlayın: [timschneeb/BudsPro-Headtracking](https://github.com/timschneeb/BudsPro-Headtracking)

## Katkıda Bulunma

Her türlü özellik isteği, hata raporu ve çekme isteği her zaman memnuniyetle karşılanır.

Bu projede hataları bildirmek veya fikirlerinizi önermek istiyorsanız, uygun bir şablonla [yeni bir konu açabilirsiniz](https://github.com/timschneeb/GalaxyBudsClient/issues/new/choose). Ayrıntılı bir açıklama için [wiki sayfamızı ziyaret edin](https://github.com/timschneeb/GalaxyBudsClient/wiki/2.-How-to-submit-issues).

Bu uygulamayı çevirmemize yardımcı olmayı planlıyorsanız, [wiki'mizdeki talimatlara bakın](https://github.com/timschneeb/GalaxyBudsClient/wiki/3.-How-to-help-with-translations). Herhangi bir programlama bilgisi gerekmez, bir çekme isteği göndermeden önce özel çevirilerinizi herhangi bir geliştirme aracı yüklemeden test edebilirsiniz.
Mevcut çeviriler için otomatik oluşturulan ilerleme raporlarını [burada](https://github.com/timschneeb/GalaxyBudsClient/blob/master/meta/translations.md) bulabilirsiniz.

Kendi kodunuzu eklemek istiyorsanız, değişikliklerinizi açıklayan düz bir çekme isteği gönderebilirsiniz. Daha büyük ve karmaşık katkılar için çalışmaya başlamadan önce bir konu açmanız (veya Telegram üzerinden bana mesaj göndermeniz [@thepbone](https://t.me/thepbone)) güzel olurdu.

## Emegi Gecenler

### Katkıda Bulunanlar

- [@nift4](https://github.com/nift4) - macOS desteği ve hata düzeltmeleri
- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Konu şablonları, wiki ve çeviriler
- [@AndriesK](https://github.com/AndriesK) - Buds Live hata düzeltmesi
- [@TheLastFrame](https://github.com/TheLastFrame) - Buds Pro simgeleri
- [@githubcatw](https://github.com/githubcatw) - Bağlantı iletişim kutusu tabanı
- [@GaryGadget9](https://github.com/GaryGadget9) - WinGet paketi
- [@joscdk](https://github.com/joscdk) - AUR paketi

### Çevirmenler

- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Rusça ve Ukraynaca çeviri
- [@PlasticBrain](https://github.com/fhalfkg) - Korece ve Japonca çeviri
- [@cozyplanes](https://github.com/cozyplanes) - Korece çeviri
- [@erenbektas](https://github.com/erenbektas) ve [@Eta06](https://github.com/Eta06) - Türkçe çeviri
- [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad), [@pseudor](https://github.com/pseudor) ve [@YexuanXiao](https://github.com/YexuanXiao) - Çince çeviri
- [@YiJhu](https://github.com/YiJhu) - Geleneksel Çince çeviri
- [@efrenbg1](https://github.com/efrenbg1) ve Andrew Gonza - İspanyolca çeviri
- [@giovankabisano](https://github.com/giovankabisano) - Endonezce çeviri
- [@lucasskluser](https://github.com/lucasskluser) ve [@JuanFariasDev](https://github.com/juanfariasdev) - Portekizce çeviri
- [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - İtalyanca çeviri
- [@Buashei](https://github.com/Buashei) - Lehçe çeviri
- [@KatJillianne](https://github.com/KatJillianne) ve [@thelegendaryjohn](https://github.com/thelegendaryjohn) - Vietnamca çeviri
- [@joskaja](https://github.com/joskaja) ve [@Joedmin](https://github.com/Joedmin) - Çekçe çeviri
- [@Benni0109](https://github.com/Benni0109), [@TheLastFrame](https://github.com/TheLastFrame), [@timschneeb](https://github.com/timschneeb) - Almanca çeviri
- [@nikossyr](https://github.com/nikossyr) - Yunanca çeviri
- [@grigorem](https://github.com/grigorem) - Rumence çeviri
- [@tretre91](https://github.com/tretre91) - Fransızca çeviri
- [@Sigarya](https://github.com/Sigarya) - İbranice çeviri
- [@domroaft](https://github.com/domroaft) - Macarca çeviri
- [@lampi8426](https://github.com/lampi8426) - Hollandaca çeviri

## Lisans

Bu proje [GPLv3](https://github.com/timschneeb/GalaxyBudsClient/blob/master/LICENSE) lisansı altında lisanslanmıştır. Samsung'a bağlı değildir ve hiçbir şekilde onlar tarafından denetlenmez.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
