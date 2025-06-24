#!/bin/bash

build_param=${1:-"all"}
GAME_PATH="/Users/shreyas/Library/Application Support/CrossOver/Bottles/Schedule I/drive_c/Program Files (x86)/Steam/steamapps/common/Schedule I"
MOD_PATH="$GAME_PATH/Mods/"

clean_build() {
    local config=$1
    local description=$2
    echo "Cleaning build $config with $description"
    rm -rf ./bin/
    rm -rf ./obj/
    dotnet clean
    dotnet build -c $config
    copy_to_game_folder $config
}

clean_build_all() {
    local config1=$1
    local config2=$2
    clean_build $config1 "Mono build with minimal logging"
    clean_build $config2 "IL2CPP build with minimal logging (optimized)"
}

copy_to_game_folder() {
    local config=$1
    echo "Copying to game folder $config"
    # remove any of our dlls from the mod path
    shopt -s nullglob
    rm "$MOD_PATH"/BackSpeakerMod*.dll
    shopt -u nullglob
    # check if config contains mono or il2cpp
    if [[ "$config" == *"MONO"* ]]; then
        cp ./bin/$config/net472/BackSpeakerMod_$config.dll "$MOD_PATH"
    else
        cp ./bin/$config/net6.0/BackSpeakerMod_$config.dll "$MOD_PATH"
    fi
}

case "$build_param" in
    "debug_mono")
        clean_build "DEBUG_MONO" "Full debug logging, unoptimized code, debug symbols"
        ;;
    "mono")
        clean_build "MONO" "Mono build with minimal logging"
        ;;
    "debug_il2cpp")
        clean_build "DEBUG_IL2CPP" "Full debug logging, unoptimized code, debug symbols"
        ;;
    "il2cpp")
        clean_build "IL2CPP" "IL2CPP build with minimal logging (optimized)"
        ;;
    "all")
        clean_build_all "MONO" "IL2CPP"
        ;;
    *)
        echo "Invalid build parameter"
        ;;
esac
