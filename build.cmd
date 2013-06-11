@echo Off
set target=%1
if "%target%" == "" (
set target=All
)
set config=%2
if "%config%" == "" (
set config=Release
)
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild build\Build.proj /t:"%target%" /p:Configuration="%config%" /m /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
