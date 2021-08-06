Rem this is comment

@ECHO OFF
SET InputA=%1
SET InputB=%2

ECHO Execution Started
SET /A c = %InputA% + %InputB% 
ECHO Execution Ended
ECHO ~~~GINGER_RC_START~~~
ECHO Result=%c%
ECHO ~~~GINGER_RC_END~~~

