Write-Host "-------------------------------------------------------------"
Write-Host "-                    Package Test Artifacts                 -"
Write-Host "-------------------------------------------------------------"
New-Item -Path "/home/vsts/work/1/a" -Name "TestCompleted.txt" -ItemType "file" -Value "Test Completed Artifacts"

Compress-Archive -Path '/home/vsts/work/1/s/Ginger/GingerUtilsTest/bin/Release/netcoreapp3.1/TestArtifacts' -DestinationPath '/home/vsts/work/1/a/GingerUtilsTestArtifacts'
Compress-Archive -Path '/home/vsts/work/1/s/Ginger/GingerCoreCommonTest/bin/Release/netcoreapp3.1/TestArtifacts' -DestinationPath '/home/vsts/work/1/a/GingerCoreCommonTestArtifacts'
Compress-Archive -Path '/home/vsts/work/1/s/Ginger/GingerCoreNETUnitTest/bin/Release/netcoreapp3.1/TestArtifacts' -DestinationPath '/home/vsts/work/1/a/GingerCoreNETUnitTestArtifacts'
Compress-Archive -Path '/home/vsts/work/1/s/Ginger/GingerPluginCoreTest/bin/Release/netcoreapp3.1/TestArtifacts' -DestinationPath '/home/vsts/work/1/a/GingerPluginCoreTestArtifacts'
Compress-Archive -Path '/home/vsts/work/1/s/Ginger/GingerConsoleTest/bin/Release/netcoreapp3.1/TestArtifacts' -DestinationPath '/home/vsts/work/1/a/GingerConsoleTestArtifacts'
Compress-Archive -Path '/home/vsts/work/1/s/Ginger/GingerAutoPilotTest/bin/Release/netcoreapp3.1/TestArtifacts' -DestinationPath '/home/vsts/work/1/a/GingerAutoPilotTestArtifacts'

      
# Compress-Archive -Path '/home/vsts/work/1/s/GingerCoreTest/bin/Release/netcoreapp3.1/TestArtifacts' -DestinationPath '/home/vsts/work/1/a/GingerCoreTestTestArtifacts'
# Compress-Archive -Path '/home/vsts/work/1/s/GingerTest/bin/Release/netcoreapp3.1/TestArtifacts' -DestinationPath '/home/vsts/work/1/a/GingerTestArtifacts'


Write-Host "-------------------------------------------------------------"
Write-Host "-                    Package GingerConsole                  -"
Write-Host "-------------------------------------------------------------"
Compress-Archive -Path '/home/vsts/work/1/s/Ginger/GingerConsole/bin/Release/netcoreapp3.1/publish' -DestinationPath '/home/vsts/work/1/a/GingerConsole'


Write-Host "-------------------------------------------------------------"
Write-Host "-                    Package Ginger.log                      "
Write-Host "-------------------------------------------------------------"
Compress-Archive -Path /home/vsts/.config/amdocs/Ginger/WorkingFolder/Logs/Ginger_Log.txt -DestinationPath '/home/vsts/work/1/a/GingerLog'


Write-Host "-------------------------------------------------------------"
Write-Host "-                         Artifacts List                    -"
Write-Host "-------------------------------------------------------------"
cd /home/vsts/work/1/a/
dir

exit $exitcode