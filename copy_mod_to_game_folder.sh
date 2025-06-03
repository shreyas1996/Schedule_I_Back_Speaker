GAME_PATH="/Users/shreyas/Library/Application Support/CrossOver/Bottles/Schedule I/drive_c/Program Files (x86)/Steam/steamapps/common/Schedule I/"
MOD_PATH="$GAME_PATH/Mods/"
DLL_PATH="bin/$(echo $1 | tr '[:lower:]' '[:upper:]')/net6.0/BackSpeakerMod.dll"

# check if the DLL exists
if [ ! -f "$DLL_PATH" ]; then
    echo "DLL does not exist at $DLL_PATH"
    exit 1
fi

# copy the DLL to the game folder
cp "$DLL_PATH" "$MOD_PATH"

echo "Copied $DLL_PATH to $MOD_PATH"
