
<p align="center">
  <a href="../README.md">English</a> | <a href="/docs/README_chs.md">中文(简体)</a> | <a href="/docs/README_cht.md">中文(繁體)</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_tr.md">Türkçe</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | <a href="/docs/README_pt.md">Português</a> | Tiếng Việt <br>
    <sub>Lưu ý: các tệp readme được biên dịch viên duy trì và có thể trở nên lỗi thời theo thời gian. Để biết thông tin mới nhất, hãy dựa vào phiên bản tiếng Anh.</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">Một trình quản lý không chính thức dành cho thiết bị Galaxy Buds</h4>
<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
    <img alt="GitHub downloads count" src="https://img.shields.io/github/downloads/thepbone/galaxybudsclient/total">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
   <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/LICENSE">
      <img alt="License" src="https://img.shields.io/github/license/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/timschneeb/GalaxyBudsClient/releases">
    <img alt="Platform" src="https://img.shields.io/badge/platform-Windows/macOS/Linux/Android-yellowgreen">
  </a>
</p>
<p align="center">
  <a href="#các-tính-năng-chính">Các tính năng chính</a> •
  <a href="#tải-về">Tải về</a> •
  <a href="#cách-hoạt-động">Cách hoạt động</a> •
  <a href="#đóng-góp">Đóng góp</a> •
  <a href="#credit">Credit</a> •
  <a href="#giấy-phép">Giấy phép</a>
</p>

<p align="center">
  <span><a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
  <a href="#"><img alt="Screenshot" src="https://github.com/timschneeb/GalaxyBudsClient/blob/master/screenshots/app_dark.png"></a></span>
</p>

## Các tính năng chính

Tùy chỉnh và điều khiển bất kỳ thiết bị Samsung Galaxy Buds nào và tích hợp nó vào PC của bạn.

Ngoài các tính năng tiêu chuẩn được biết đến từ ứng dụng Android chính thức, dự án này giúp bạn phát huy hết tiềm năng của chiếc tai nghe của bạn và triển khai các chức năng mới như:

* Thống kê pin chi tiết
* Các trình chẩn đoán và tự kiểm
* Cực nhiều các thông tin gỡ lỗi ẩn
* Tùy chỉnh các hành động chạm giữ lâu
* Flash firmware, hạ cấp (Buds+, Buds Pro)
* và nhiều hơn nữa...

Nếu bạn đang tìm kiếm các tệp firmware cũ hơn, hãy xem tại đây: [https://github.com/timschneeb/galaxy-buds-firmware-archive](https://github.com/timschneeb/galaxy-buds-firmware-archive#galaxy-buds-firmware-archive)

## Tải về

Một số package cho Linux:
* [Flatpak (Tất cả các bản Linux distro)](#flatpak)
* [Package AUR (Arch Linux)](#package-aur)

Tệp cho Windows nằm trong phần [release](https://github.com/timschneeb/GalaxyBudsClient/releases). Vui lòng đọc ghi chú phát hành trước khi cài đặt.

Tải xuống phiên bản dành cho PC tại đây:
<p align="center">
    <a href="https://github.com/timschneeb/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/timschneeb/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

Tải xuống phiên bản Android tại đây (có trả phí):
<p align="center">
  <a href='https://play.google.com/store/apps/details?id=me.timschneeberger.galaxybudsclient&utm_source=github&pcampaignid=pcampaignidMKT-Other-global-all-co-prtnr-py-PartBadge-Mar2515-1'> 
    <img width="300" alt='Get it on Google Play' src='https://play.google.com/intl/en_us/badges/static/images/badges/en_badge_web_generic.png'/>
  </a>
</p>

### Flatpak

Các package phổ biến cho tất cả các bản Linux distro. Phiên bản Flatpak không hỗ trợ tự động khởi động trừ khi được thiết lập thủ công. Bạn có thể dùng `galaxybudsclient /StartMinimized` để khởi chạy ứng dụng một cách âm thầm trong khi khởi động.

Tải xuống ngay trên FlatHub: https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient
```
flatpak install me.timschneeberger.GalaxyBudsClient
```

<a href='https://flathub.org/apps/me.timschneeberger.GalaxyBudsClient'><img width='240' alt='Download on Flathub' src='https://dl.flathub.org/assets/badges/flathub-badge-en.png'/></a>

> **Lưu ý**: Các Flatpak đều được sandbox. Theo mặc định, ứng dụng này chỉ có thể truy cập `~/.var/app/me.timschneeberger.GalaxyBudsClient/`.

### Package AUR 

[Package AUR](https://aur.archlinux.org/packages/galaxybudsclient-bin/) dành cho Arch Linux do @joscdk bảo trì cũng khả dụng:
```
yay -S galaxybudsclient-bin
```

### winget

Package cho Windows cũng có thể cài đặt bằng Windows Package Manager (winget)

```
winget install timschneeb.GalaxyBudsClient
```

## Cách hoạt động

Để sử dụng công nghệ không dây Bluetooth, thiết bị phải hiểu được các cấu hình Bluetooth cụ thể cho phép các thiết bị Bluetooth giao tiếp hiệu quả với nhau.

Galaxy Buds định nghĩa hai cấu hình Bluetooth: A2DP (Advanced Audio Distribution) để phát trực tuyến/điều khiển âm thanh và SPP (Serial Port Profile) để truyền các luồng nhị phân. Các nhà sản xuất thường sử dụng cấu hình này (dựa trên giao thức RFCOMM) để trao đổi dữ liệu cấu hình, thực hiện cập nhật chương trình cơ sở hoặc gửi các lệnh khác đến thiết bị Bluetooth.

Mặc dù cấu hình A2DP được chuẩn hóa và ghi chép lại, nhưng định dạng dữ liệu nhị phân được trao đổi bởi giao thức RFCOMM này thường là độc quyền.

Để nghiên cứu đảo ngược định dạng dữ liệu này, tôi bắt đầu bằng cách phân tích cấu trúc của luồng nhị phân do tai nghe gửi đi. Sau đó, tôi cũng đã mổ xẻ các ứng dụng Galaxy Buds chính thức dành cho Android để hiểu rõ hơn về hoạt động bên trong của các thiết bị này. Bạn có thể tìm thấy một số ghi chú (chưa đầy đủ) mà tôi đã ghi lại bên dưới. Kiểm tra mã nguồn để biết thông tin chi tiết hơn về cấu trúc của giao thức này.

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Ghi chú cho Galaxy Buds (2019)</a> •
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Ghi chú cho Galaxy Buds Plus</a>
</p>

Khi xem xét kỹ hơn Galaxy Buds Plus, tôi cũng nhận thấy một số tính năng lạ, chẳng hạn như chế độ gỡ lỗi phần mềm, chế độ ghép nối chưa sử dụng và công cụ dump key Bluetooth. Tôi đã ghi lại những phát hiện này ở đây:

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Những tính năng lạ</a>
</p>

Hiện tại, tôi đang tìm cách sửa đổi và nghiên cứu đảo ngược phần mềm cho Buds+. Tại thời điểm viết bài này, tôi đã tạo ra hai công cụ để lấy và phân tích các tệp firmware chính thức. Xem chúng tại đây:

<p align="center">
  <a href="https://github.com/timschneeb/GalaxyBudsFirmwareDownloader">Trình tải xuống firmware</a> •
  <a href="https://github.com/timschneeb/GalaxyBudsFirmwareExtractor">Trình trích xuất firmware</a>
</p>

Truyền dữ liệu theo dõi chuyển động đầu theo thời gian thực từ Buds Pro của bạn bằng cách sử dụng script này: [timschneeb/BudsPro-Headtracking](https://github.com/timschneeb/BudsPro-Headtracking)

## Đóng góp

Yêu cầu tính năng, báo cáo lỗi và pull request dưới mọi hình thức luôn được hoan nghênh.

Nếu bạn muốn báo cáo lỗi hoặc đề xuất ý tưởng của mình cho dự án này, bạn có thể [mở một issue mới](https://github.com/timschneeb/GalaxyBudsClient/issues/new/choose) với một mẫu issue phù hợp. [Truy cập wiki của chúng tôi](https://github.com/timschneeb/GalaxyBudsClient/wiki/2.-How-to-submit-issues) để xem giải thích chi tiết.

Nếu bạn đang có ý định muốn giúp chúng tôi dịch ứng dụng này, [hãy tham khảo hướng dẫn trên wiki của chúng tôi](https://github.com/timschneeb/GalaxyBudsClient/wiki/3.-How-to-help-with-translations). Không cần bất kỳ kiến ​​thức lập trình nào cả, bạn có thể kiểm tra bản dịch của mình mà không cần cài đặt bất kỳ công cụ phát triển nào trước khi gửi pull request.
Bạn có thể tìm thấy báo cáo tiến trình được tự động tạo cho các bản dịch hiện có [tại đây](https://github.com/timschneeb/GalaxyBudsClient/blob/master/meta/translations.md).

Nếu bạn muốn đóng góp code của riêng mình, bạn chỉ cần gửi pull request và đơn giản là giải thích các thay đổi của bạn. Đối với các đóng góp lớn và phức tạp hơn, sẽ rất là tuyệt vời nếu bạn có thể mở một issue (hoặc nhắn tin cho tôi qua Telegram [@thepbone](https://t.me/thepbone)) trước khi bắt đầu làm việc.

## Credit

### Người đóng góp

* [@nift4](https://github.com/nift4) - Hỗ trợ cho macOS và sửa bug
* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Mẫu cho issue, wiki và phiên dịch
* [@AndriesK](https://github.com/AndriesK) - Sửa bug cho Buds Live
* [@TheLastFrame](https://github.com/TheLastFrame) - Icon cho Buds Pro
* [@githubcatw](https://github.com/githubcatw) - Nền tảng cho hộp thoại kết nối
* [@GaryGadget9](https://github.com/GaryGadget9) - Package WinGet
* [@joscdk](https://github.com/joscdk) - Package AUR

### Người phiên dịch

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Bản dịch tiếng Nga và tiếng Ukraina
* [@PlasticBrain](https://github.com/fhalfkg) - Bản dịch tiếng Hàn và tiếng Nhật
* [@cozyplanes](https://github.com/cozyplanes) - Bản dịch tiếng Hàn
* [@erenbektas](https://github.com/erenbektas) và [@Eta06](https://github.com/Eta06) - Bản dịch tiếng Thổ Nhĩ Kỳ
* [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad), [@pseudor](https://github.com/pseudor) và [@YexuanXiao](https://github.com/YexuanXiao) - Bản dịch tiếng Trung
* [@YiJhu](https://github.com/YiJhu) - Bản dịch tiếng Trung Phồn thể
* [@efrenbg1](https://github.com/efrenbg1) và Andrew Gonza - Bản dịch tiếng Tây Ban Nha
* [@giovankabisano](https://github.com/giovankabisano) - Bản dịch tiếng Indonesia
* [@lucasskluser](https://github.com/lucasskluser) and [@JuanFariasDev](https://github.com/juanfariasdev) - Bản dịch tiếng Bồ Đào Nha
* [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - Bản dịch tiếng Ý
* [@Buashei](https://github.com/Buashei) - Bản dịch tiếng Ba Lan
* [@KatJillianne](https://github.com/KatJillianne) và [@thelegendaryjohn](https://github.com/thelegendaryjohn) - Bản dịch tiếng Việt
* [@joskaja](https://github.com/joskaja) và [@Joedmin](https://github.com/Joedmin) - Bản dịch tiếng Séc
* [@Benni0109](https://github.com/Benni0109), [@TheLastFrame](https://github.com/TheLastFrame), [@timschneeb](https://github.com/timschneeb) - Bản dịch tiếng Đức
* [@nikossyr](https://github.com/nikossyr) - Bản dịch tiếng Hy Lạp
* [@grigorem](https://github.com/grigorem) - Bản dịch tiếng Rumani
* [@tretre91](https://github.com/tretre91) - Bản dịch tiếng Pháp
* [@Sigarya](https://github.com/Sigarya) - Bản dịch tiếng Hebrew
* [@domroaft](https://github.com/domroaft) - Bản dịch tiếng Hungary
* [@lampi8426](https://github.com/lampi8426) - Bản dịch tiếng Hà Lan

### Dịch vụ

* [Cloudflare](https://www.cloudflare.com/) - Bảo mật các API backend của GalaxyBudsClient và cung cấp giấy phép bản Pro

### Asset
* Asset earbud được sử dụng trong icon của bản Android được tạo bởi [Archival](https://www.flaticon.com/authors/archival) từ [Flaticon](https://www.flaticon.com/)

## Giấy phép

Dự án này được cấp phép theo giấy phép [GPLv3](https://github.com/timschneeb/GalaxyBudsClient/blob/master/LICENSE). Dự án này không liên kết với Samsung hay được Samsung giám sát dưới bất kỳ hình thức nào.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR 
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
