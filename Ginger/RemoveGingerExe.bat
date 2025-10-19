@echo off
setlocal

REM Remove Registry entries for the ginger protocol
echo Removing Registry entries...
reg delete "HKEY_CLASSES_ROOT\ginger\shell\open\command" /f
reg delete "HKEY_CLASSES_ROOT\ginger\shell\open" /f
reg delete "HKEY_CLASSES_ROOT\ginger\shell" /f
reg delete "HKEY_CLASSES_ROOT\ginger" /f
echo Registry entries removed successfully.

REM Optional: Remove the application path from system PATH
REM WARNING: This part is tricky because batch scripts can't easily remove a specific path from PATH.
REM You may want to do this manually or with PowerShell if needed.
echo Note: The application path remains in the system PATH. Manual cleanup may be required.

endlocal
pause
