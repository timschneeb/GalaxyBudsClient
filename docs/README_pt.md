<p align="center">
   <a href="../README.md">English</a> | <a href="/docs/README_chs.md">中文(简体)</a> | <a href="/docs/README_cht.md">中文(繁體)</a> | <a href="/docs/README_rus.md">Русский</a> | <a href="/docs/README_jpn.md">日本語</a> | <a href="/docs/README_ukr.md">Українська</a> | <a href="/docs/README_kor.md">한국어</a> | <a href="/docs/README_cze.md">Česky</a> | <a href="/docs/README_gr.md">Ελληνικά</a> | Português <br>
    <sub>Atenção: os arquivos "readme" são mantidos por tradutores e podem ficar desatualizados com o tempo. Para ter as informações mais atuais, visite o arquivo em inglês.</sub>
</p>
<h1 align="center">
  Galaxy Buds Client
  <br>
</h1>
<h4 align="center">Um gerenciador não oficial para os Buds, Buds+, Buds Live e Buds Pro</h4>
<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="GitHub downloads count" src="https://img.shields.io/github/downloads/thepbone/galaxybudsclient/total">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
   <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE">
      <img alt="License" src="https://img.shields.io/github/license/thepbone/galaxybudsclient">
  </a>
  <a href="https://github.com/ThePBone/GalaxyBudsClient/releases">
    <img alt="Platform" src="https://img.shields.io/badge/platform-Windows/Linux-yellowgreen">
  </a>
</p>
<p align="center">
  <a href="#funções-principais">Funções Principais</a> •
  <a href="#download">Download</a> •
  <a href="#como-funciona">Como funciona</a> •
  <a href="#contribuindo">Contribuindo</a> •
  <a href="#creditos">Creditos</a> •
  <a href="#licença">Licença</a>
</p>

<p align="center">
    <a href="https://ko-fi.com/H2H83E5J3"><img alt="Screenshot" src="https://ko-fi.com/img/githubbutton_sm.svg"></a>
</p>

<p align="center">
    <a href="#"><img alt="Screenshot" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/screencap.gif"></a>
</p>

## Funções Principais

Configurar e controlar qualquer aparelho da linha Samsung Galaxy Buds e integra-los ao seu desktop.

Além das funções conhecidas do aplicativo oficial de Android esse projeto também ajuda você a desbloquear o potencial máximo de seus earbuds e implementa novas funcionalidades como:

* Estátisticas detalhadas das baterias
* Diagnósticos e auto-testes de fabrica
* Diversas informações ocultas de debugging
* Customização nas ações de toque longo
* Firmware flashing, downgrading (Buds+, Buds Pro)
* e muito mais...

## Download

Para obter os binaries para Windows e Linux na página de [versões](https://github.com/ThePBone/GalaxyBudsClient/releases). Por favor, leia as notas antes de realizar a instação. 

<p align="center">
    <a href="https://github.com/ThePBone/GalaxyBudsClient/releases"><img alt="Download" src="https://github.com/ThePBone/GalaxyBudsClient/blob/master/screenshots/download.png"></a>
</p>

### winget

O pacote para Windows também está disponivel para instalação com o Windows Package Manager (winget)

```
winget install ThePBone.GalaxyBudsClient
```

### AUR package 

Um [AUR package](https://aur.archlinux.org/packages/galaxybudsclient-bin/) para Arch Linux mantido por @joscdk também está disponivel:
```
yay -S galaxybudsclient-bin
```


## Como funciona

Para utilizar a técnologia sem fio, Bluetooth, o aparelho deve ser capaz de intepretar perfis especificos de Bluetooth que permitem a comunicação eficiente entre um aparelho e o outro.

O Galaxy Buds define dois perfis Bluetooth: AD2P (Perfil de Distribuição de Audio Avançado) para controle/transmissão de áudio e SPP (Perfil de Serial Port) para transmissão de streams binários. Os fabricantes frequentemente usam esse perfil (que depende do protocolo RFCOMM) para configurar a troca de informação, realizar updates de firmware, ou enviar outros comandos para o aparelho Bluetooth.

Mesmo que o perfil A2DP é padronizado e documentado, o formato que a troca de dados binarios realizados por esse protocolo RFCOMM é normlamente próprio da empresa.

Para realizar engenharia-reversa desse formato de dados eu comecei analisando a estrutura da transmissão binaria enviada pelos earbuds. Mais tarde eu também dissequei o aplicativo oficial dos Galaxy Buds para conseguir mais entendimento sobre o funcionamento interno dos fones. Você pode encontrar algumas notas (incompletas) que eu tomei abaixo. Confira o código fonte para obter informações mais detalhadas na estrutura do protocolo.

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsRFCommProtocol.md">Galaxy Buds (2019) Notas</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/Galaxy%20Buds%20Plus%20RFComm%20Protocol%20Notes.md">Galaxy Buds Plus Notas</a>
</p>

Enquanto me aprofundava nos Galaxy Buds Plus eu também notei algumas particularidades, como um modo de debug de firmware, um modo de pareamento não utilizado e a enumeração de chaves Bluetooth. Eu documentei esses achados aqui:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md">Galaxy Buds Plus: Funções incomuns</a>
</p>

No momento eu estou tentando modificar e realizar engenharia-reversa no firmware dos Buds+. No momento que eu escrevo isso, já fiz duas ferramentas para acessar e analzar os firmware em binario. Confira eles aqui:

<p align="center">
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareDownloader">Firmware Downloader</a> •
  <a href="https://github.com/ThePBone/GalaxyBudsFirmwareExtractor">Firmware Extractor</a>
</p>

Transmita o monitoramento de informação em tempo real dos seus Buds Pro usando esse script: [ThePBone/BudsPro-Headtracking](https://github.com/ThePBone/BudsPro-Headtracking)

## Contribuindo

Feature requests, bug reports, and pull requests of any kind are always welcome.

If you want to report bugs or propose your ideas for this project, you are welcome to [open a new issue](https://github.com/ThePBone/GalaxyBudsClient/issues/new/choose) with a suitable template. [Visit our wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/2.-How-to-submit-issues) for a detailed explanation.

If you are planning to help us translating this app, [refer to the instructions on our wiki](https://github.com/ThePBone/GalaxyBudsClient/wiki/3.-How-to-help-with-translations). No programming knowledge is required, you can test your custom translations without installing any development tools before submitting a pull request.
You can find auto-generated progress reports for existing translations [here](https://github.com/ThePBone/GalaxyBudsClient/blob/master/meta/translations.md).

If you want to contribute your own code, you can simply submit a plain pull request explaining you changes. For larger and complex contributions it would be nice if you could open an issue (or message me via Telegram [@thepbone](https://t.me/thepbone)) before starting to work on it.

## Creditos

### Contributors

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Issue templates, wiki and translations
* [@AndriesK](https://github.com/AndriesK) - Buds Live bug fix
* [@TheLastFrame](https://github.com/TheLastFrame) - Buds Pro icons
* [@githubcatw](https://github.com/githubcatw) - Connection dialog base
* [@GaryGadget9](https://github.com/GaryGadget9) - WinGet package
* [@joscdk](https://github.com/joscdk) - AUR package

### Translators

* [@ArthurWolfhound](https://github.com/ArthurWolfhound) - Russian and Ukrainian translation
* [@PlasticBrain](https://github.com/fhalfkg) - Korean and Japanese translation
* [@cozyplanes](https://github.com/cozyplanes) - Korean translation
* [@erenbektas](https://github.com/erenbektas) - Turkish translation
* [@kakkk](https://github.com/kakkk), [@KevinZonda](https://github.com/KevinZonda), [@ssenkrad](https://github.com/ssenkrad) and [@pseudor](https://github.com/pseudor) - Chinese translation
* [@YiJhu](https://github.com/YiJhu) - Chinese-Traditional translation
* [@efrenbg1](https://github.com/efrenbg1) and Andrew Gonza - Spanish translation
* [@giovankabisano](https://github.com/giovankabisano) - Indonesian translation
* [@lucasskluser](https://github.com/lucasskluser) - Portuguese translation
* [@alb-p](https://github.com/alb-p), [@mario-donnarumma](https://github.com/mario-donnarumma) - Italian translation
* [@Buashei](https://github.com/Buashei) - Polish translation
* [@KatJillianne](https://github.com/KatJillianne) - Vietnamese translation
* [@joskaja](https://github.com/joskaja) and [@Joedmin](https://github.com/Joedmin) - Czech translation
* [@Benni0109](https://github.com/Benni0109), [@TheLastFrame](https://github.com/TheLastFrame), [@ThePBone](https://github.com/ThePBone) - German translation
* [@nikossyr](https://github.com/nikossyr) - Greek translation
* [@grigorem](https://github.com/grigorem) - Romanian translation
* [@tretre91](https://github.com/tretre91) - French translation
* [@Sigarya](https://github.com/Sigarya) - Hebrew translation
* [@domroaft](https://github.com/domroaft) - Hungarian translation

## Licença

This project is licensed under [GPLv3](https://github.com/ThePBone/GalaxyBudsClient/blob/master/LICENSE). It is not affiliated with Samsung nor supervised by them in any way.

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR 
THE USE OR O
