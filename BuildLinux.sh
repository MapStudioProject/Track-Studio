#!/bin/sh

pwd

dotnet build --runtime linux-x64 --self-contained "$(pwd)/Track Studio"
cp -R "$(pwd)/Plugins" "$(pwd)/Track Studio/bin/Debug/net8.0/linux-x64/"
cp -R "$(pwd)/Track Studio/bin/Debug/net8.0/Plugins" "$(pwd)/Track Studio/bin/Debug/net8.0/linux-x64/"

echo "Built Track Studio correctly! Run './Track\ Studio/bin/Debug/net8.0/linux-x64/Track\ Studio' to launch!"
