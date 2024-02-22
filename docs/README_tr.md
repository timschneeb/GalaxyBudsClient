<p align="center">
   english | <a href="/docs/README_chs.md">中文(简体)</a> | <a href="/docs/README_cht.md">中文(繁體)</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | <a href="/docs/README_pt.md">Português</a> <br>
   <sub>Dikkat: readme dosyaları çevirmenler tarafından güncellenir ve zaman zaman güncelliğini yitirebilir. En güvenilir bilgi için İngilizce versiyonuna başvurun.</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">Buds, Buds+, Buds Live ve Buds Pro için resmi olmayan bir yönetici</h4>
<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="GitHub indirme sayısı" src="https://img.shields.io/github/downloads/thepbone/galaxybudsclient/total">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
   <img alt="GitHub release (tarihe göre en yenisi)" src="https://img.shields.io/github/v/release/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE">
      <img alt="Lisans" src="https://img.shields.io/github/license/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="Platform" src="https://img.shields.io/badge/platform-Windows/Linux-yellowgreen">
  </a>
</p>
<p align="center">
  <a href="#key-features">Temel Özellikler</a> •
  <a href="#download">İndir</a> •
  <a href="#how-it-works">Nasıl Çalışır</a> •
  <a href="#contributing">Katkıda Bulunma</a> •
  <a href="#credits">Emeği Geçenler</a> •
  <a href="#license">Lisans</a>
</p>

<p align="center">
  <a href="https://ko-fi.com/H2H83E5J3"><img alt="Görüntü" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
</p>

<p align="center">
  <a href="#"><img alt="Görüntü" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/screencap.gif"></a>
</p>

## Temel Özellikler

Samsung Galaxy Buds cihazlarını yapılandırın, kontrol edin ve masaüstünüze entegre edin.

Resmi Android uygulamasında bilinen standart özelliklerin yanı sıra, bu proje kulaklıklarınızın tüm potansiyelini ortaya çıkarmanıza yardımcı olur ve aşağıdaki gibi yeni işlevler sunar:

* Detaylı pil istatistikleri
* Teşhis ve fabrika öz-testleri
* Birçok gizli hata ayıklama bilgisi
* Özelleştirilebilir uzun basılı dokunma eylemleri
* Ürün yazılımını yükleme, eski sürüme indirgeme (Buds+, Buds Pro)
* ve çok daha fazlası...

Eski donanım yazılımı ikili dosyalarını arıyorsanız, şu adrese bir göz atın: [https://github.com/ThePBone/galaxy-buds-firmware-archive](https://github.com/ThePBone/galaxy-buds-firmware-archive#galaxy-buds-firmware-archive)

## İndir

Birkaç Linux paketi mevcuttur:
* [Flatpak (Tüm Linux dağıtımları)](#flatpak)
* [AUR paketi (Arch Linux)](#aur-package)

Windows için ikili dosyaları [sürüm](https://github.com/ThePBone/GalaxyBudsClient/releases) bölümünden edinin. Kurulumdan önce lütfen sürüm notlarını okuyun:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="İndir" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

### Flatpak

Tüm Linux dağıtımları için evrensel ikili paketler. GalaxyBudsClient'ı Linux'a kurmanın önerilen yolu budur.

FlatHub'dan indirilebilir: https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient
```
flatpak install me.timschneeberger.GalaxyBudsClient
```

<a href='https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient'><img width='240' alt='FlatHub üzerinden indir' src='https://dl.flathub.org/assets/badges/flathub-badge-en.png'/></a>

> **Not:** Flatpak'lar sanal ortamlarda çalışır (sandboxed). Bu uygulama varsayılan olarak yalnızca `~/.var/app/me.timschneeberger.GalaxyBudsClient/` dizinine erişebilir.

### AUR paketi

@joscdk tarafından sağlanan Arch Linux için bir [AUR paketi](https://aur.archlinux.org/packages/galaxybudsclient-bin/) de mevcuttur:
```
yay -S galaxybudsclient-bin
```

### winget

Windows paketi, Windows Paket Yöneticisi (winget) ile de yüklenebilir.

```
winget install ThePBone.GalaxyBudsClient
```

## Nasıl çalışır?

Bluetooth kablosuz teknolojisini kullanmak için, bir cihazın, Bluetooth cihazlarının birbirleriyle verimli bir şekilde iletişim kurmasını sağlayan belirli Bluetooth profillerini yorumlayabilmesi gerekir.

Galaxy Buds iki Bluetooth profili tanımlar: A2DP (Gelişmiş Ses Dağıtım Profili) ses akışı/kontrolü için ve SPP (Seri Bağlantı Noktası Profili) ikili akışları iletmek amacıyla. Üreticiler genellikle bu profili kullanırlar (RFCOMM protokolüne dayanır); yapılandırma verilerini değiştirmek, ürün yazılımı güncellemeleri yapmak veya Bluetooth cihazına diğer komutları göndermek için.

A2DP profili standartlaştırılmış ve belgelenmiş olsa da, bu RFCOMM protokolü tarafından değiştirilen ikili verilerin formatı genellikle tescillidir.

Bu veri formatının tersine mühendisliğini yapmak için, kulaklıklar tarafından gönderilen ikili akışın yapısını analiz ederek başladım. Daha sonra, bu cihazların iç işleyişlerine dair daha fazla bilgi edinmek için Android için resmi Galaxy Buds uygulamalarını da parçaladım. Aşağıda tuttuğum bazı (eksik) notları bulabilirsiniz. Protokolün yapısı hakkında daha ayrıntılı bilgi almak için kaynak kodunu inceleyin.

<p align="center">
 <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notları</a> •
 <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notları</a>
</p>

Galaxy Buds Plus'ı daha yakından incelerken, ürün yazılımı hata ayıklama modu, kullanılmayan bir eşleştirme modu ve bir Bluetooth anahtar dökümü gibi bazı alışılmadık özellikler de fark ettim. Bu bulguları burada belgeledim:

<p align="center">
 <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Alışılmadık özellikler</a>
</p>

Şu anda, Buds+ için ürün yazılımını değiştirmeyi ve tersine mühendislik yapmayı araştırıyorum. Bu yazıyı kaleme aldığım sırada resmi ürün yazılımı ikili dosyalarını getirmek ve analiz etmek için iki araç oluşturdum. Onları burada bulabilirsiniz:

<p align="center">
 <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">Ürün Yazılımı İndirme Aracı</a> •
 <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">Ürün Yazılımı Çıkarma Aracı</a>
</p>

Buds Pro'nuzdan gerçek zamanlı olarak veri akışı almak için şu betiği kullanın: [ThePBone/BudsPro-Headtracking](https://github.com/ThePBone/BudsPro-Headtracking)

## Katkıda Bulunma

Özellik talepleri, hata raporları ve her türlü çekme isteği (pull requests) her zaman memnuniyetle karşılanır.

Hataları bildirmek ya da bu proje için fikirlerinizi önermek istiyorsanız, uygun bir şablonla [yeni bir konu açabilirsiniz](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose). Detaylı bir açıklama için [wiki sayfamızı](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues) ziyaret edin.

Uygulamayı çevirmemize yardımcı olmayı planlıyorsanız, [wiki sayfamızdaki talimatlara](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations) başvurun. Herhangi bir geliştirme aracını kurmadan önce özel çevirilerinizi test edebilir ve bir çekme isteği göndermeden önceki halini gözden geçirebilirsiniz.  Çevirilerdeki otomatik oluşturulmuş ilerleme raporlarını [buradan](https://github.com/ThePBone/GalaxyBudsClient/blob/master/meta/translations.md) bulabilirsiniz.

Kendi kodunuzu eklemek istiyorsanız, değişikliklerinizi açıklayan düz bir çekme isteği gönderebilirsiniz. Daha büyük ve karmaşık katkılar için, üzerinde çalışmaya başlamadan önce bir konu açmanız (veya bana Telegram [@thepbone](https://t.me/thepbone) üzerinden mesaj atmanız) iyi olacaktır.

## Emeği Geçenler

### Katkıda Bulunanlar

* [@nift4](https://github.com/nift4) - macOS desteği ve hata düzeltmeleri
* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Konu şablonları, wiki ve çeviriler
* [@AndriesK](https://github.com/AndriesK) - Buds Live hata düzeltmesi
* [@TheLastFrame](https://github.com/TheLastFrame) - Buds Pro simgeleri
* [@githubcatw](https://github.com/githubcatw) - Bağlantı diyalog penceresi
* [@GaryGadget9](https://github.com/GaryGadget9) - WinGet paketi
* [@joscdk](https://github.com/joscdk) - AUR paketi

### Çevirmenler

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Rusça ve Ukraynaca çeviri
* [@PlasticBrain](https://github.com/fhalfkg) - Korece ve Japonca çeviri
* [@cozyplanes](https://github.com/cozyplanes) - Korece çeviri
* [@erenbektas](https://github.com/erenbektas) - Türkçe çeviri 
* [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad), [@pseudor](https://github.com/pseudor) and [@YexuanXiao](https://github.com/YexuanXiao) - Çince çeviri
* [@YiJhu](https://github.com/YiJhu) - Geleneksel Çince çeviri
* [@efrenbg1](https://github.com/efrenbg1) and Andrew Gonza - İspanyolca çeviri
* [@giovankabisano](https://github.com/giovankabisano) - Endonezce çeviri
* [@lucasskluser](https://github.com/lucasskluser) - Portekizce çeviri
* [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - İtalyanca çeviri
* [@Buashei](https://github.com/Buashei) - Lehçe çeviri
* [@KatJillianne](https://github.com/KatJillianne) - Vietnamca çeviri
* [@joskaja](https://github.com/joskaja) and [@Joedmin](https://github.com/Joedmin) - Çekçe çeviri
* [@Benni0109](https://github.com/Benni0109), [@TheLastFrame](https://github.com/TheLastFrame), [@ThePBone](https://github.com/ThePBone) - Almanca çeviri
* [@nikossyr](https://github.com/nikossyr) - Yunanca çeviri
* [@grigorem](https://github.com/grigorem) - Rumence çeviri
* [@tretre91](https://github.com/tretre91) - Fransızca çeviri
* [@Sigarya](https://github.com/Sigarya) - İbranice çeviri
* [@domroaft](https://github.com/domroaft) - Macarca çeviri
* [@lampi8426](https://github.com/lampi8426) - Felemenkçe çeviri 

## Lisans

Bu proje [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE) ile lisanslanmıştır. Samsung ile herhangi bir şekilde bağlantılı değildir veya onlar tarafından denetlenmez. 

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR 
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
