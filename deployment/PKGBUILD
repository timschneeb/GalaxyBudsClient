# Maintainer: Tim Schneeberger (ThePBone) <tim.schneeberger(at)gmail.com>
pkgname=galaxybudsclient
pkgver=%VERSION%
pkgrel=1
pkgdesc="Galaxy Buds Client for Linux"
arch=('x86_64'
      'armv7h'
      'aarch64')
url="https://github.com/ThePBone/GalaxyBudsClient"
license=('GPL3')
depends=()
makedepends=('git' 'wget')
provides=("${pkgname}")
conflicts=("${pkgname}")
options=("!strip")
source=("${pkgname}.desktop" "icon_white.png")
sha256sums=('d38eefc5491e2624de67bee39c553cee5e54ddb3d1e5cac6aad46446ce5b4947' 'SKIP')

package() {
	cd "$srcdir/${pkgname}"

	targetarch=""
	if [ $CARCH = "x86_64" ]; then
   		targetarch="64bit"
	elif [ $CARCH = "armv7h" ]; then
   		targetarch="arm"
	elif [ $CARCH = "aarch64" ]; then
   		targetarch="arm64"
	else 
		echo "ERROR: CPU architecture not supported"
	fi

	curl -s https://api.github.com/repos/thepbone/GalaxyBudsClient/releases/tags/%VERSION% | grep -E 'browser_download_url' | grep -i linux_${targetarch}_portable | cut -d '"' -f 4 | wget --progress=bar:force -i - -O "GalaxyBudsClient.bin"

	install -Dm755 GalaxyBudsClient.bin "$pkgdir/usr/bin/galaxybudsclient"
	install -Dm644 "$srcdir/${pkgname}.desktop" -t "$pkgdir/usr/share/applications"
	install -Dm644 "$srcdir/icon_white.png" "$pkgdir/usr/share/pixmaps/galaxybudsclient.png"
}
