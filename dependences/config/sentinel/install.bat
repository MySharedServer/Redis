@echo off

set currentpath=%~dp0
set parentpath=

:begin
for /f "tokens=1,* delims=\" %%i in ("%currentpath%") do (set content=%%i&&set currentpath=%%j)

if "%parentpath%%content%\" == "%~dp0" goto end

set parentpath=%parentpath%%content%\

goto begin

:end
REM echo parent path=%parentpath%
set redisdircmd='call readini.bat "%parentpath%redis.ini" [Common] redisdir'
set servicenamecmd='call readini.bat "%parentpath%redis.ini" [Sentinel] sentinel_name'
REM echo servicename cmd = %servicenamecmd%

for /f "delims=" %%a in (%redisdircmd%) do (
	set redisdir=%%a
)
for /f "delims=" %%a in (%servicenamecmd%) do (
	set servicename=%%a
)
REM echo redis path=%redisdir%
REM echo %~dp0sentinel.conf
REM echo service-name=%servicename%
"%redisdir%\redis-server" --service-install "%~dp0sentinel.conf" --sentinel --service-name %servicename%
"%redisdir%\redis-server" --service-start --service-name %servicename%

pause