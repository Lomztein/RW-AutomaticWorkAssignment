@set slnDir=%1
@cd "%slnDir%"
@set name=%2

@echo Copying mod %name% to RimWorld mods

@set dest=%3%
@set dest=%dest:"=%

@echo %name%
@echo %dest%

@del "%dest%/%name%" /q
@robocopy "./%2" "%dest%/%name%" /xd "obj" "bin" /xf "*.csproj" /mir

@exit /B
