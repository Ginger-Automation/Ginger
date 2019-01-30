@echo off
for /f %%i in ('dir *.cs /s /b 2^> nul ^| find "" /v /c') do set VAR=%%i
echo %VAR%