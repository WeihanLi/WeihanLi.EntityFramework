#!/bin/sh
SCRIPT='./build/build.cs'

# Install tool
dotnet tool install --global dotnet-execute
export PATH="$PATH:$HOME/.dotnet/tools"

# Start Cake
EXEC_ARGS="$SCRIPT --debug"

echo "dotnet-exec $EXEC_ARGS $@"

dotnet-exec $EXEC_ARGS "$@"
