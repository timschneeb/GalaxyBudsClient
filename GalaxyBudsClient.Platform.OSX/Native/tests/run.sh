#!/bin/bash
set -euo pipefail

native_dir="$(cd "$(dirname "$0")/.." && pwd)"
build_dir="$(mktemp -d "${TMPDIR:-/tmp}/galaxy-hotkey-tests.XXXXXX")"
trap 'rm -rf "$build_dir"' EXIT

xcodebuild -quiet \
    -project "$native_dir/NativeInterop.xcodeproj" \
    -scheme NativeInterop -configuration Release \
    -destination "platform=macOS,arch=$(uname -m)" \
    -onlyUsePackageVersionsFromResolvedFile \
    MACOSX_DEPLOYMENT_TARGET=12.0 ONLY_ACTIVE_ARCH=YES \
    CONFIGURATION_BUILD_DIR="$build_dir/products" \
    CONFIGURATION_TEMP_DIR="$build_dir/intermediates" build

xcrun clang++ -std=c++17 -fobjc-arc \
    -framework AppKit -framework Carbon -framework IOBluetooth -framework CoreGraphics \
    -I "$native_dir/include" -I "$native_dir/src" \
    -I "$build_dir/intermediates/NativeInterop.build/DerivedSources" \
    "$native_dir/tests/HotkeySimulationTests.mm" \
    -L "$build_dir/products" -lNativeInterop -o "$build_dir/tests"

DYLD_LIBRARY_PATH="$build_dir/products" "$build_dir/tests"
