GAME_PATH="/Users/shreyas/Library/Application Support/CrossOver/Bottles/Schedule I/drive_c/Program Files (x86)/Steam/steamapps/common/Schedule I/"
MOD_PATH="$GAME_PATH/Mods/"

# Default to Release if no configuration is specified
CONFIG=${1:-Release}
DLL_PATH="bin/$(echo $CONFIG | tr '[:lower:]' '[:upper:]')/net6.0/BackSpeakerMod.dll"

# build the mod using the build_configurations.sh script
./build_configurations.sh $1

# check if the build was successful
if [ $? -ne 0 ]; then
    echo "Build failed"
    exit 1
fi

# check if the DLL exists
if [ ! -f "$DLL_PATH" ]; then
    echo "DLL does not exist at $DLL_PATH"
    exit 1
fi

# copy the DLL to the game folder
cp "$DLL_PATH" "$MOD_PATH"

echo "Copied $DLL_PATH to $MOD_PATH"
