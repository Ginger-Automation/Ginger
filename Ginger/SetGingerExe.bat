@echo off
setlocal

REM Set the application path here
set "appPath=C:\Program Files (x86)\Amdocs\Ginger by amdocs"

REM Add the application path to the system PATH variable
setx PATH "%PATH%;%appPath%" /M
echo Application path added to system PATH variable.

REM Update the Registry
echo Updating the Registry...
reg add "HKEY_CLASSES_ROOT\ginger" /ve /d "" /f
reg add "HKEY_CLASSES_ROOT\ginger" /v "URL Protocol" /d "" /f
reg add "HKEY_CLASSES_ROOT\ginger\shell" /ve /d "" /f
reg add "HKEY_CLASSES_ROOT\ginger\shell\open" /ve /d "" /f
reg add "HKEY_CLASSES_ROOT\ginger\shell\open\command" /ve /d "\"C:\Program Files (x86)\Amdocs\Ginger by amdocs\Ginger.exe\" \"%%1\"" /f
echo Registry updated successfully.

pause
