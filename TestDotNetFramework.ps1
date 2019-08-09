Write-Host "-------------------------------------------------------------"
Write-Host "-          Starting test for .Net Framework DLLs            -"
Write-Host "-------------------------------------------------------------"

cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\Extensions\TestPlatform\" 

$exitcode=0

mkdir d:\a\1\s\TestResults\DotNetFramework

./vstest.console.exe "d:\a\1\s\Ginger\GingerCoreTest\bin\Release\GingerCoreTest.dll" /Logger:trx /ResultsDirectory:d:\a\1\s\TestResults\DotNetFramework
Write-Host ">>>>>>>>>>>>>>>>>>>>>>>>>>>>> LastExitCode: " $LastExitCode
if ($LastExitCode -ne 0)
{
	$exitcode = 1
}


./vstest.console.exe "d:\a\1\s\Ginger\GingerTest\bin\Release\GingerTest.dll" /Logger:trx /ResultsDirectory:d:\a\1\s\TestResults\DotNetFramework
Write-Host ">>>>>>>>>>>>>>>>>>>>>>>>>>>>> LastExitCode: " $LastExitCode
if ($LastExitCode -ne 0)
{
	$exitcode = 1
}

Write-Host "-------------------------------------------------------------"
Write-Host "-                        Tests Completed                    -"
Write-Host "-------------------------------------------------------------"
cd d:\a\1\s\TestResults
dir

Write-Host "-------------------------------------------------------------"
Write-Host "-                    Copy Test Artifacts                    -"
Write-Host "-------------------------------------------------------------"
New-Item -Path "d:\a\1\a" -Name "TestCompleted.txt" -ItemType "file" -Value "Test Completed Artifacts"

Compress-Archive -Path 'd:\a\1\s\Ginger\GingerUtilsTest\bin\Release\netcoreapp2.2\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerUtilsTestArtifacts'
Compress-Archive -Path 'd:\a\1\s\Ginger\GingerCoreCommonTest\bin\Release\netcoreapp2.2\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerCoreCommonTestArtifacts'
Compress-Archive -Path 'd:\a\1\s\Ginger\GingerCoreNETUnitTest\bin\Release\netcoreapp2.2\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerCoreNETUnitTestArtifacts'
Compress-Archive -Path 'd:\a\1\s\Ginger\GingerConsoleTest\bin\Release\netcoreapp2.2\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerConsoleTestArtifacts'
Compress-Archive -Path 'd:\a\1\s\Ginger\GingerAutoPilotTest\bin\Release\netcoreapp2.2\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerAutoPilotTestArtifacts'
# Compress-Archive -Path 'd:\a\1\s\Ginger\GingerPluginCoreTest\bin\Release\netcoreapp2.2\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerPluginCoreTestArtifacts'
      
# Compress-Archive -Path 'd:\a\1\s\Ginger\GingerCoreTest\bin\Release\netcoreapp2.2\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerCoreTestTestArtifacts'
# Compress-Archive -Path 'd:\a\1\s\Ginger\GingerTest\bin\Release\netcoreapp2.2\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerTestArtifacts'

Write-Host "-------------------------------------------------------------"
Write-Host "-                         Artifacts List                    -"
Write-Host "-------------------------------------------------------------"
cd d:\a\1\a\
dir


exit $exitcode