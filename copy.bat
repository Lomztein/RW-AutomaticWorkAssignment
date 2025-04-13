@cd "C:\Users\lomzt\source\repos\RW-AutomaticWorkAssignment"
@set name=%1

@echo Copying mod %name% to RimWorld mods

@set dest=%2%
@set dest=%dest:"=%

@echo %name%
@echo %dest%

@del "%dest%/%name%" /q
@robocopy "./%1" "%dest%/%name%" /xd "obj" "bin" /xf "*.csproj" /mir

@exit /B