<p align="center">
  <a href="../README.md">English</a> | <a href="/docs/README_chs.md">中文</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | Ελληνικά | <a href="/docs/README_pt.md">Português</a> | <a href="/docs/README_vnm.md">Tiếng Việt</a> <br>
    <sub>Προσοχή: Τα αρχεία readme συντηρούνται από μεταφραστές και ενδέχεται να καθίστανται απαρχαιωμένα κατά καιρούς. Για τις πιο πρόσφατες πληροφορίες στηριχτείτε στην αγγλική έκδοση.</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">Μία ανεπίσημη εφαρμογή διαχείρισης των ακουστικών Buds, Buds+, Buds Live και Buds Pro</h4>
<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="μετρηρής λήψεων GitHub" src="https://img.shields.io/github/downloads/thepbone/galaxybudsclient/total">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
   <img alt="Έκδοση GitHub  (πιο πρόσφατη)" src="https://img.shields.io/github/v/release/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE">
      <img alt="Άδεια" src="https://img.shields.io/github/license/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="Πλατφόρμα" src="https://img.shields.io/badge/platform-Windows/Linux-yellowgreen">
  </a>
</p>
<p align="center">
  <a href="#κύρια-χαρακτηριστικά">Κύρια χαρακτηριστικά</a> •
  <a href="#λήψη">Λήψη</a> •
  <a href="#πώς-λειτουργεί">Πώς λειτουργεί</a> •
  <a href="#συνεισφορά">Συνεισφορά</a> •
  <a href="#συντελεστές">Συντελεστές</a> •
  <a href="#άδεια">Άδεια</a>
</p>

<p align="center">
    <a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
</p>

<p align="center">
    <a href="#"><img alt="Screenshot" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/screencap.gif"></a>
</p>

## Κύρια χαρακτηριστικά

Διαχειριστείτε και ελέγξτε τη συσκευή Samsung Galaxy Buds σας και ενσωματώστε την στον υπολογιστή σας.

Εκτός από τα κύρια χαρακτηριστικά που προσφέρει η επίσημη εφαρμογή για Android, αυτή η εφαρμογή βοηθά στην απελευθέρωση όλων των δυνατοτήτων των ακουστικών σας και την ενσωμάτωση νέας λειτουργικότητας όπως:

- Λεπτομερή στατιστικά μπαταρίας
- Διαγνωστικές πληροφορίες και εργοστασιακά τεστ αυτοαξιολόγησης
- Πολλές κρυμμένες πληροφορίες αποσφαλμάτωσης
- Προσαρμοσμένες λειτουργίες παρατεταμένου αγγίγματος
- και πολλά ακόμα...

## Λήψη

Λήψη εκδόσεων για Windows και Linux στην ενότητα [λήψεων](https://github.com/ThePBone/GalaxyBudsClient/releases). Παρακαλώ διαβάστε τις σημειώσεις των εκδόσεων πριν την εγκατάσταση.

<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

## Πώς λειτουργεί

Για τη χρήση της τεχνολογίας της ασύρματης σύνδεσης μέσω Bluetooth, μια συσκευή θα πρέπει να μπορεί να αντιλαμβάνεται συγκεκριμένα προφίλ Bluetooth. Τα προφίλ Bluetooth ερμηνεύουν πιθανές εφαρμογές και ορίζουν γενικές συμπεριφορές, τις οποίες χρησιμοποιούν συσκευές με ενεργό Bluetooth για την επικοινωνία με άλλες συσκευές Bluetooth.

Τα ακουστικά Galaxy Buds ορίζουν δύο προφίλ Bluetooth: το A2DP (Advanced Audio Distribution Profile) για τη μετάδοση ήχου και ελέγχου αυτής της μετάδοσης και το SPP (Serial Port Profile) για τη μεταφορά δυαδικών δεδομένων. Οι κατασκευαστές συχνά χρησιμοποιούν το τελευταίο προφίλ (το οποίο βασίζεται στο πρωτόκολλο RFCOMM) για την ανταλλαγή δεδομένων ρυθμίσεων, την ενημέρωση του firmware ή της αποστολή άλλων εντολών στη συσκευή Bluetooth.

Παρόλο που το προφίλ A2DP είναι τυποποιημένο και τεκμηριωμένο, η μορφή των δεδομένων που ανταλλάσσονται με αυτό το πρωτόκολλο RFCOMM δεν είναι τεκμηριωμένη ενώ συνήθως είναι και ιδιόκτητη.

Προκειμένου να ερευνηθεί ανάδρομα (reverse-engineer) αυτή η μορφή δεδομένων, ξεκίνησα αναλύοντας τη δομή της δυαδικής ροής που στέλνεται από τα ακουστικά. Στη συνέχεια, έκανα disassemble την επίσημη εφαρμογή για τα Galaxy Buds για τις συσκευές Android για να αποκτήσω περισσότερες γνώσεις για το πως λειτουργούν εσωτερικά αυτές οι συσκευές. Παράλληλα, κατέγραφα τις σκέψεις μου σε ένα μικρό σημειωματάριο. Παρόλο που οι σημειώσεις μου δεν είναι πολύ όμορφες, μπορείτε να τους ρίξετε μια ματιά με το παρακάτω link. Λάβετε υπόψιν ότι δεν κατέγραψα την κάθε παραμικρή λεπτομέρεια. Δείτε τον πηγαίο κώδικα για περισσότερες λεπτομέρειες σχετικά με τη δομή του πρωτοκόλλου.

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Σημειώσεις για τα Galaxy Buds (2019) </a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Σημειώσεις για τα Galaxy Buds Plus</a>
</p>

Κοιτάζοντας πιο προσεχτικά τα Galaxy Buds Plus, παρατήρησα κάποια ασυνήθιστα χαρακτηριστικά, όπως λειτουργία αποσφαλμάτωσης για το firmware, μια αχρησιμοποίητη λειτουργία σύζευξης και ένα λειτουργία μεταφοράς κλειδιού Bluetooth (key dumper). Τα ευρήματα αυτά είναι καταγεγραμμένα εδώ:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Ασυνήθιστα χαρακτηριστικά</a>
</p>

Επί του παρόντος, προσπαθώ να τροποποιήσω και να ερευνήσω ανάδρομα (reverse-engineer) το firmware των Buds+. Τη στιγμή συγγραφής του παρόντος, έχω δύο εργαλεία ανάκτησης και ανάλυσης των επίσημων firmware. Δείτε τα εδώ:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">Εργαλείο λήψης Firmware</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">Εργαλείο εξαγωγής Firmware</a>
</p>

## Συνεισφορά

Αιτήματα νέων λειτουργιών, αναφορές σφαλμάτων, και αιτήματα pull requests κάθε είδους είναι καλοδεχούμενα.

Αν θέλετε να αναφέρετε σφάλματα ή να προτείνετε τις ιδέες σας για την εφαρμογή, είσαστε ευπρόσδεκτοι να [ανοίξετε ένα νέο issue](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose) με το κατάλληλο υπόδειγμα (template). [Επισκεφτείτε το wiki μας](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues) για μια λεπτομερή επεξήγηση.

Αν σχεδιάζετε να βοηθήσετε στη μετάφραση της εφαρμογής, [ανατρέξτε στις οδηγείες στο wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations). Δεν απαιτείται κάποια γνώση προγραμματισμού ενώ μπορείτε να δοκιμάσετε την μετάφρασή σας χωρίς την πρόσθετη εγκατάσταση κάποιου εργαλείου ανάπτυξης πριν την καταχώρηση ενός pull request.
Μπορείτε να βρείτε αναφορές προόδου που δημιουργούνται αυτόματα για τις υπάρχουσες μεταφράσεις εδώ: [/meta/translations.md](https://github.com/ThePBone/GalaxyBudsClient/blob/master/meta/translations.md)

Αν θέλετε να συνεισφέρετε με το δικό σας κώδικα, μπορείτε να καταχωρίσετε ένα απλό pull request εξηγώντας τις αλλαγές σας. Για μεγαλύτερες και πιο σύνθετες συνεισφορές θα ήταν καλό να ανοίξετε ένα issue (ή να μου στείλετε μήνυμα μέσω του via Telegram [@thepbone](https://t.me/thepbone)) πριν ξεκινήσετε να δουλεύετε πάνω σε αυτό.

## Συντελεστές

#### Συνεργάτες

- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Υποδείγματα Issue, wiki και μεταφράσεις
- [@AndriesK](https://github.com/AndriesK) - Διόρθωση σφάλματος στα Buds Live
- [@TheLastFrame](https://github.com/TheLastFrame) - Εικονίδια Buds Pro
- [@githubcatw](https://github.com/githubcatw) - Κορμός διαλόγου σύνδεσης

#### Μεταφραστές

- [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Ρωσική και Ουκρανική μετάφραση
- [@PlasticBrain](https://github.com/fhalfkg) - Κορεάτικη και Ιαπωνική μετάφραση
- [@cozyplanes](https://github.com/cozyplanes) - Κορεάτικη μετάφραση
- [@erenbektas](https://github.com/erenbektas) - Τουρκική μετάφραση
- [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad) and [@pseudor](https://github.com/pseudor) - Κινεζική μετάφραση
- [@efrenbg1](https://github.com/efrenbg1) and Andrew Gonza - Ισπανική μετάφραση
- [@giovankabisano](https://github.com/giovankabisano) - Ινδονησιακή μετάφραση
- [@lucasskluser](https://github.com/lucasskluser) and [@JuanFariasDev](https://github.com/juanfariasdev) - Πορτογαλική μετάφραση
- [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - Ιταλική μετάφραση
- [@Buashei](https://github.com/Buashei) - Πολωνική μετάφραση
- [@KatJillianne](https://github.com/KatJillianne) and [@thelegendaryjohn](https://github.com/thelegendaryjohn) - Βιετναμέζικη μετάφραση
- [@joskaja](https://github.com/joskaja) and [@Joedmin](https://github.com/Joedmin) - Τσεχική μετάφραση
- [@TheLastFrame](https://github.com/TheLastFrame), [@ThePBone](https://github.com/ThePBone) - Γερμανική μετάφραση
- [@nikossyr](https://github.com/nikossyr) - Ελληνική μετάφραση

## Άδεια

Η εφαρμογή αυτή έχει άδεια βάσει [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE). Δεν συνδέεται με τη Samsung ούτε εποπτεύεται από αυτή με κανένα τρόπο.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```
