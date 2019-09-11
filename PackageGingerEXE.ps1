Write-Host "-------------------------------------------------------------"
Write-Host "-                    Package Ginger EXE                      "
Write-Host "-------------------------------------------------------------"

Compress-Archive -Path 'D:\a\1\s\Ginger\Ginger\bin\Release' -DestinationPath 'D:\a\1\a\GingerEXE'

Write-Host "-------------------------------------------------------------"
Write-Host "-                    Package Ginger.log                      "
Write-Host "-------------------------------------------------------------"

cd D:\a\1\s\Ginger\Ginger\bin\Release
Ginger.exe help
# Compress-Archive -Path /home/vsts/.config/amdocs/Ginger/WorkingFolder/Logs/Ginger_Log.txt -DestinationPath '/home/vsts/work/1/a/GingerLog'


Write-Host "-------------------------------------------------------------"
Write-Host "-                         Artifacts List                    -"
Write-Host "-------------------------------------------------------------"
cd D:\a\1\a\
dir

exit $exitcode