@echo off
::-------------------------------------------------
:: readIni <iniFilePath>
:: readIni <iniFilePath> <section>
:: readIni <iniFilePath> <section> <item>
::-------------------------------------------------
set item=
set filepath=
set section=
setlocal EnableDelayedExpansion
if not "%~1"=="" (
        set filepath=%1
) else goto :file_err
if not exist %filepath% goto :file_err
if not "%~2"=="" (
        set section=%2
        if "!section:~0,1!"==""^" set section=!section:~1!
        if "!section:~-1!"==""^" set section=!section:~0,-1!
) else goto :section
if not "%~3"=="" (
        set item=%3
        if "!item:~0,1!"==""^" set item=!item:~1!
        if "!item:~-1!"==""^" set item=!item:~0,-1!
)
setlocal disableDelayedExpansion
set filter=
for /f "usebackq delims=[]" %%i in (`find /i "%section%" /n %filepath%`) do set filter=%%i
for /f "usebackq tokens=1* delims== skip=%filter%" %%i in (`type %filepath%`) do (
        set a=%%i
        setlocal EnableDelayedExpansion
        if "!a:~0,1!"=="[" goto :eof
        if not "!a:~0,1!"==";" (
                setlocal disableDelayedExpansion
                for /f "delims=;" %%x in ("%%i=%%j") do (
                        if not DEFINED item (echo %%x) else (if /i "%%i"=="%item%" echo %%j)
                )
        )
)
goto :eof
 
:section
setlocal disableDelayedExpansion
for /f "usebackq delims== skip=2" %%i in (`find /i "[" %filepath%`) do echo %%i
goto :eof
 
:file_err
setlocal disableDelayedExpansion
echo.
echo Can not find %1 file!
echo.
goto :eof