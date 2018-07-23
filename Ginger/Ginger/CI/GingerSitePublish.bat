


@echo off
setlocal
rem stop iis site GingerSupportSite
appcmd stop sites "GingerSupportSite"
rem backup external dir
move /Y E:\Ginger\GingerSupportSite\External E:\Ginger\bk\
move /Y E:\Ginger\GingerSupportSite\Views\Home\Downloads.cshtml E:\Ginger\bk\
%windir%\microsoft.net\framework\v4.0.30319\msbuild /m .\GingerSupportSite.sln /p:DeployOnBuild=true /p:PublishProfile=GingerSupportSite

rem restore external dir
rd /S /Q E:\Ginger\GingerSupportSite\External
move /Y E:\Ginger\bk\External E:\Ginger\GingerSupportSite\External
move /Y E:\Ginger\bk\Downloads.cshtml E:\Ginger\GingerSupportSite\Views\Home\Downloads.cshtml
rem grant RW access to all for \External\NewsImages
icacls "E:\Ginger\GingerSupportSite\External\NewsImages" /grant everyone:(OI)(CI)F

rem grant RW access to all for \App_Data
icacls "E:\Ginger\GingerSupportSite\App_Data" /grant everyone:(OI)(CI)F

rem start iis site GingerSupportSite
C:\Windows\System32\inetsrv\appcmd start sites "GingerSupportSite"

@IF %ERRORLEVEL% NEQ 0 GOTO err



@exit /B 0
:err
@PAUSE
@exit /B 1