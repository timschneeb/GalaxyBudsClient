#!/bin/zsh
# Script para compilar libNativeInterop.dylib e copiar para a pasta local correta

# Diretório do próprio script
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]:-${(%):-%N}}")" && pwd)"

# Caminho do projeto Xcode
PROJECT_PATH="$SCRIPT_DIR/NativeInterop.xcodeproj"
# Caminho de saída desejado
OUTPUT_PATH="$SCRIPT_DIR/Build/Release/libNativeInterop.dylib"

# Compila usando xcodebuild para Release
xcodebuild -project "$PROJECT_PATH" -scheme NativeInterop -configuration Release build CONFIGURATION_BUILD_DIR="$SCRIPT_DIR/Build/Release"

echo "File libNativeInterop.dylib generated in $SCRIPT_DIR/Build/Release/"
