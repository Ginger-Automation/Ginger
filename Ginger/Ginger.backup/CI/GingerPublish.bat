rem ### Get latest Ginger code and do Build
rem @echo off
setlocal
cd C:\TFS\GingerDev\GingerNextVer_Dev\
c:
tf get /noprompt /recursive

rem ### Build Ginger Solution
 "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"  "C:\TFS\GingerDev\GingerNextVer_Dev\Ginger.sln" "/p:Configuration=Release" "/p:Platform=Any CPU"
@IF %ERRORLEVEL% NEQ 0 GOTO err

rem ### Run Confuser for sensitive DLL's to protect the binaries
del /Q C:\TFS\GingerBuilder\GingerConfused\*.*
C:\TFS\GingerBuilder\ConfuserEx_bin\Confuser.CLI.exe C:\TFS\GingerBuilder\GingerConfuserDev.crproj
C:\TFS\GingerBuilder\ConfuserEx_bin\Confuser.CLI.exe C:\TFS\GingerBuilder\GingerConfuserDev2.crproj

rem ### Copy the latest DLL's to the Package.Dev folder
del /Q C:\TFS\GingerBuilder\Package.Dev\*
rd /Q C:\TFS\GingerBuilder\Package.Dev\*
xcopy /E/Y C:\TFS\GingerDev\GingerNextVer_Dev\Ginger\bin\Release\* C:\TFS\GingerBuilder\Package.Dev\

rem ### Overwrite with the latest confused DLL's in the Package.Dev folder
copy C:\TFS\GingerBuilder\GingerConfused\*.* C:\TFS\GingerBuilder\Package.Dev\

rem ### Zip and save the Ginger solution files in Share folder
For /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set mydate=%%b-%%a-%%c)
For /f "tokens=1-2 delims=: " %%a in ('time /t') do (set mytime=%%a-%%b)
"C:\Program Files\7-Zip\7z" a -r C:\share\GingerV2.4Beta\GingerBetaV2.4_%mydate%_%mytime%.zip C:\TFS\GingerBuilder\Package.Dev\

rem ### Build the GingerSetup project for creating new installation MSI
C:\"Program Files (x86)"\"Microsoft Visual Studio 12.0"\Common7\IDE\devenv.exe C:\TFS\GingerDev\GingerSetupProc\GingerSetup\GingerSetup\GingerSetup.vdproj /build Release

rem ### Copy the new Ginger installation MSI to the Support Site Download folder
rem TODO: Need to check the file exist and from today to make sure the MSI build succeed 
copy /Y C:\TFS\GingerDev\GingerSetupProc\GingerSetup\GingerSetup\Release\GingerSetup.msi E:\Ginger\GingerSupportSite\External\Downloads\GingerSetup2.4beta.msi

rem ### Update the new Beta version in Ginger project & Support site
attrib -R C:\TFS\GingerDev\GingerNextVer_Dev\Ginger\Properties\AssemblyInfo.cs
attrib -R E:\Ginger\GingerSupportSite\Views\Home\Downloads.cshtml
cscript C:\TFS\GingerDev\GingerNextVer_Dev\Ginger\CI\increasebuildNum.vbs C:\TFS\GingerDev\GingerNextVer_Dev\Ginger\Properties\AssemblyInfo.cs E:\Ginger\GingerSupportSite\Views\Home\Downloads.cshtml

rem ### Check in the new Beta release number changes
c:
cd C:\TFS\GingerDev\GingerNextVer_Dev\
tf checkin /noprompt /comment:"Update Ginger Beta number after publish"


@exit /B 0
:err
@PAUSE
@exit /B 1