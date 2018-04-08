#!/bin/sh

if [ -z "$1" ]; then
    TARGET="Release"
else
    TARGET=$1    
fi

dotnet restore src/SoundFingerprinting.sln
dotnet pack src/SoundFingerprinting/SoundFingerprinting.csproj -c $TARGET -o ../../build
