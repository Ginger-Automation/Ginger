Write-Host "-------------------------------------------------------------"
Write-Host "-          Starting test for .Net Framework DLLs            -"
Write-Host "-------------------------------------------------------------"
cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\Extensions\TestPlatform\" 

$exitcode=0

./vstest.console.exe d:\a\1\s\Ginger\GingerCoreTest\bin\Release\GingerCoreTest.dll /Logger:trx /ResultsDirectory:d:\a\1\s\TestResults
Write-Host ">>>>>>>>>>>>>>>>>>>>>>>>>>>>> LastExitCode: " $LastExitCode
if ($LastExitCode -ne 0)
{
	$exitcode = 1
}

./vstest.console.exe "d:\a\1\s\Ginger\Unit Tests\bin\Release\UnitTests.dll" /Logger:trx /ResultsDirectory:d:\a\1\s\TestResults
Write-Host ">>>>>>>>>>>>>>>>>>>>>>>>>>>>> LastExitCode: " $LastExitCode
if ($LastExitCode -ne 0)
{
	$exitcode = 1
}

./vstest.console.exe "d:\a\1\s\Ginger\GingerTest\bin\Release\GingerTest.dll" /Logger:trx /ResultsDirectory:d:\a\1\s\TestResults
Write-Host ">>>>>>>>>>>>>>>>>>>>>>>>>>>>> LastExitCode: " $LastExitCode
if ($LastExitCode -ne 0)
{
	$exitcode = 1
}

cd d:\a\1\s\TestResults
dir
Write-Host "-------------------------------------------------------------"
Write-Host "-                        Tests Completed                    -"
Write-Host "-------------------------------------------------------------"

exit $exitcode