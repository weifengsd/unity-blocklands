#!/bin/zsh
set -eu
cd "${0:A:h}/.."
mkdir -p Assets/Plugins
sdk=$(xcrun --sdk macosx --show-sdk-path)
/usr/bin/clang -fobjc-arc -O2 -dynamiclib -arch arm64 -arch x86_64 -isysroot "$sdk" -mmacosx-version-min=12.0 -framework Foundation -framework Speech -framework AVFoundation Native/VoiceBridge.m -o Assets/Plugins/libVoiceBridge.dylib
