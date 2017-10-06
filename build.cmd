@echo Off
set target=%1
if "%target%" == "" (
   set target=All
)
set config=%2
if "%config%" == "" (
   set config=Release
)

"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild" build\Build.proj /t:"%target%" /p:Configuration="%config%" /m /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
