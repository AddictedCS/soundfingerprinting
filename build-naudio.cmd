@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

dotnet test .\src\SoundFingerprinting.Audio.NAudio.Test\SoundFingerprinting.Audio.NAudio.Test.csproj -c %config%
dotnet pack .\src\SoundFingerprinting.Audio.NAudio\SoundFingerprinting.Audio.NAudio.csproj -c %config% -o build