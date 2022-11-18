@echo off


set currentpath=%~dp0
set parentpath=

:begin
for /f "tokens=1,* delims=\" %%i in ("%currentpath%") do (set content=%%i&&set currentpath=%%j)

if "%parentpath%%content%\" == "%~dp0" goto end

set parentpath=%parentpath%%content%\

goto begin

:end
echo parent path=%parentpath%
set hostcmd='call readini.bat "%parentpath%redis.ini" [Common] host'
set portcmd='call readini.bat "%parentpath%redis.ini" [Sentinel] port'
set redisdircmd='call readini.bat "%parentpath%redis.ini" [Common] redisdir'
echo hostcmd = %hostcmd%
echo portcmd = %portcmd%

for /f "delims=" %%a in (%hostcmd%) do (
	set host=%%a
)

for /f "delims=" %%a in (%portcmd%) do (
	set port=%%a
)

for /f "delims=" %%a in (%redisdircmd%) do (
	set redisdir=%%a
)
echo redis path=%redisdir%
echo host = %host%
echo port = %port%

"%redisdir%\redis-cli.exe" -h %host% -p %port% info Sentinel

pause