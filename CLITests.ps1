Write-Host "-------------------------------------------------------------"
Write-Host "-                         Run CLI Tests                     -"
Write-Host "-------------------------------------------------------------"

cd /home/vsts/work/1/s/Ginger/GingerRuntime/bin/Release/net6.0/publish/


Write-Host "******************************************************************************************************************************"
Write-Host "help run"
Write-Host "******************************************************************************************************************************"

dotnet GingerConsole.dll help run


Write-Host "******************************************************************************************************************************"
Write-Host "run simple solution"
Write-Host "******************************************************************************************************************************"

dotnet GingerConsole.dll run -s "/home/vsts/work/1/s/Ginger/GingerCoreNETUnitTest/TestResources/Solutions/CLI" -e "Default" -r "Default Run Set"


Write-Host "******************************************************************************************************************************"
Write-Host "analyze solution"
Write-Host "******************************************************************************************************************************"

dotnet GingerConsole.dll analyze -s "/home/vsts/work/1/s/Ginger/GingerCoreNETUnitTest/TestResources/Solutions/CLI" 


exit $exitcode