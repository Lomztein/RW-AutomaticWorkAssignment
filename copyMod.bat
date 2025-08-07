@set name=%1
@set dest=%2%
@set dest=%dest:"=%\%name:"=%

@echo Copying mod %name% to RimWorld mods (%dest%)

@robocopy "./%name%" "%dest%" /xd "obj" "bin" /xf "*.csproj" "*.cs" /s
:: Prune dirs
cd "%dest%"
for /f "delims=" %%d in ('dir /s /b /ad ^| sort /r') do @rd "%%d" 2>NUL

@exit /B
