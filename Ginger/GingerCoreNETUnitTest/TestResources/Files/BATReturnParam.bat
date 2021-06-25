@echo off
REM GINGER_Description Hello Bat
REM GINGER_$ Param1
set Param1=%1
echo ~~~GINGER_RC_START~~~
echo Hello = hello world
echo Arg = %Param1%
echo ~~~GINGER_RC_END~~~
exit(1)