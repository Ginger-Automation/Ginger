Write-Host "-------------------------------------------------------------"
Write-Host "-                    Package Ginger EXE                      "
Write-Host "-------------------------------------------------------------"
cd D:\a\1\s\Ginger\Ginger\bin\Release
dir
# delete app.publish since it is ~30mb which are not needed so save space and time for download
Remove-Item D:\a\1\s\Ginger\Ginger\bin\Release\app.publish\Ginger.exe
#zip release folder 
Compress-Archive -Path 'D:\a\1\s\Ginger\Ginger\bin\Release' -DestinationPath 'D:\a\1\a\GingerEXE'


Write-Host "-------------------------------------------------------------"
Write-Host "-                    Run CLI tests                           "
Write-Host "-------------------------------------------------------------"
cd D:\a\1\s\Ginger\Ginger\bin\Release
$p = Start-Process Ginger.exe help
$p.HasExited
$p.ExitCode

Write-Host "-------------------------------------------------------------"
Write-Host "-                    Package Ginger.log                      "
Write-Host "-------------------------------------------------------------"
# Compress-Archive -Path C:\Users\VssAdministrator\AppData\Roaming\amdocs\Ginger\WorkingFolder\Logs\Ginger_Log.txt -DestinationPath 'D:\a\1\a\GingerLog'


Write-Host "-------------------------------------------------------------"
Write-Host "-                         Artifacts List                    -"
Write-Host "-------------------------------------------------------------"
cd D:\a\1\a\
dir

exit $exitcode