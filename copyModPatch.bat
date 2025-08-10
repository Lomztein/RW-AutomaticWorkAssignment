@set name=%1
@set dest=%2%
@set basemod=%3
@set dest=%2%

@echo Copying patch %name% for %basemod% to RimWorld mods (%dest%)

@cd "./%name%"
@for /f %%d in ('dir /b /ad ^| sort /r ^|  findstr /E /R "[0-9]\.[0-9]"') do (
    @(robocopy ".\%%d" "%dest:"=%\%%d\Mods\%name:"=%" /s /r:15 /w:1)
    @if %ERRORLEVEL% GEQ 8 (
        @echo Copy failed 1>&2
        @exit 1
    )
)
:: Prune dirs
@cd "%dest%"
@for /f "delims=" %%d in ('dir /s /b /ad ^| sort /r') do @rd "%%d" 2>NUL

@exit /B
