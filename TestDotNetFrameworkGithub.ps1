Write-Host "-------------------------------------------------------------"
Write-Host "-          Starting test for .Net Framework DLLs            -"
Write-Host "-------------------------------------------------------------"

cd "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\TestPlatform\" 

$exitcode=0

mkdir D:\a\Ginger\Ginger\TestResults\DotNetFramework
mkdir D:\a\1\a

./vstest.console.exe "D:\a\Ginger\Ginger\Ginger\GingerCoreTest\bin\Release\net6.0-windows\GingerCoreTest.dll" /Logger:trx /ResultsDirectory:D:\a\Ginger\Ginger\TestResults\DotNetFramework
Write-Host ">>>>>>>>>>>>>>>>>>>>>>>>>>>>> LastExitCode: " $LastExitCode
if ($LastExitCode -ne 0)
{
	$exitcode = 1
}


./vstest.console.exe "D:\a\Ginger\Ginger\Ginger\GingerTest\bin\Release\net6.0-windows\GingerTest.dll" /Logger:trx /ResultsDirectory:D:\a\Ginger\Ginger\TestResults\DotNetFramework
Write-Host ">>>>>>>>>>>>>>>>>>>>>>>>>>>>> LastExitCode: " $LastExitCode
if ($LastExitCode -ne 0)
{
	$exitcode = 1
}

Write-Host "-------------------------------------------------------------"
Write-Host "-                        Tests Completed                    -"
Write-Host "-------------------------------------------------------------"
cd D:\a\Ginger\Ginger\TestResults
dir

Write-Host "-------------------------------------------------------------"
Write-Host "-                    Copy Test Artifacts                    -"
Write-Host "-------------------------------------------------------------"
New-Item -Path "D:\a\1\a" -Name "TestCompleted.txt" -ItemType "file" -Value "Test Completed Artifacts"

Compress-Archive -Path 'D:\a\Ginger\Ginger\Ginger\GingerUtilsTest\bin\Release\net6.0\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerUtilsTestArtifacts'
Compress-Archive -Path 'D:\a\Ginger\Ginger\Ginger\GingerCoreCommonTest\bin\Release\net6.0\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerCoreCommonTestArtifacts'
Compress-Archive -Path 'D:\a\Ginger\Ginger\Ginger\GingerCoreNETUnitTest\bin\Release\net6.0\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerCoreNETUnitTestArtifacts'
Compress-Archive -Path 'D:\a\Ginger\Ginger\Ginger\GingerConsoleTest\bin\Release\net6.0\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerConsoleTestArtifacts'
Compress-Archive -Path 'D:\a\Ginger\Ginger\Ginger\GingerAutoPilotTest\bin\Release\net6.0\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerAutoPilotTestArtifacts'
# Compress-Archive -Path 'd:\a\1\s\Ginger\GingerPluginCoreTest\bin\Release\netcoreapp3.1\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerPluginCoreTestArtifacts'
      
# Compress-Archive -Path 'd:\a\1\s\Ginger\GingerCoreTest\bin\Release\netcoreapp3.1\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerCoreTestTestArtifacts'
# Compress-Archive -Path 'd:\a\1\s\Ginger\GingerTest\bin\Release\netcoreapp3.1\TestArtifacts' -DestinationPath 'D:\a\1\a\GingerArtifacts'

Write-Host "-------------------------------------------------------------"
Write-Host "-                         Artifacts List                    -"
Write-Host "-------------------------------------------------------------"
cd D:\a\1\a\
dir


exit $exitcode
