#!/bin/sh
SCRIPT='./build/build.cs'

# Install tool
dotnet tool update --global dotnet-execute
export PATH="$PATH:$HOME/.dotnet/tools"

echo "dotnet-exec $SCRIPT --args=$@"

dotnet-exec $SCRIPT --args="$@"
