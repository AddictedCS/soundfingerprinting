@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

dotnet restore .\src\SoundFingerprinting.sln
dotnet test .\src\SoundFingerprinting.Tests\SoundFingerprinting.Tests.csproj -c %config%
dotnet pack .\src\SoundFingerprinting\SoundFingerprinting.csproj -c %config% -o ..\..\build