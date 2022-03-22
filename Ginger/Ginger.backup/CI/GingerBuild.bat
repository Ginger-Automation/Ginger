rem ### Get latest Ginger code
@echo off
setlocal
cd C:\TFS\GingerDev\GingerNextVer_Dev\
c:
tf get /noprompt /recursive

rem ### Build Ginger Solution
rem %windir%\microsoft.net\framework\v4.0.30319\msbuild /m C:\TFS\GingerDev\GingerNextVer_Dev\Ginger.sln /p:Configuration=Release "/p:Platform=Any CPU"
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"  "C:\TFS\GingerDev\GingerNextVer_Dev\Ginger.sln" "/p:Configuration=Release" "/p:Platform=Any CPU"
@IF %ERRORLEVEL% NEQ 0 GOTO err

rem ### Run Confuser for sensetive DLL's to protect the binaries
del /Q C:\TFS\GingerBuilder\GingerConfused\*.*
C:\TFS\GingerBuilder\ConfuserEx_bin\Confuser.CLI.exe C:\TFS\GingerBuilder\GingerConfuserDev.crproj
C:\TFS\GingerBuilder\ConfuserEx_bin\Confuser.CLI.exe C:\TFS\GingerBuilder\GingerConfuserDev2.crproj

rem ### Copy the latest DLL's to the Package.Dev folder
del /Q C:\TFS\GingerBuilder\Package.Dev\*
rd /Q C:\TFS\GingerBuilder\Package.Dev\*
xcopy /E/Y C:\TFS\GingerDev\GingerNextVer_Dev\Ginger\bin\Release\* C:\TFS\GingerBuilder\Package.Dev\

rem ### Overwrite with the latest confused DLL's in the Package.Dev folder
copy /Y C:\TFS\GingerBuilder\GingerConfused\*.* C:\TFS\GingerBuilder\Package.Dev\


@exit /B 0
:err
@PAUSE
@exit /B 1