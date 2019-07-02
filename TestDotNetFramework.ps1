Write-Host "-------------------------------------------------------------"
Write-Host "-          Starting test for .Net Framework DLLs            -"
Write-Host "-------------------------------------------------------------"
cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\Extensions\TestPlatform\" 
# cd "d:\a\1\s\Ginger\GingerCoreTest\bin\Release"
# dir
./vstest.console.exe d:\a\1\s\Ginger\GingerCoreTest\bin\Release\GingerCoreTest.dll /Logger:trx /ResultsDirectory: d:\a\1\s\TestResults
cd d:\a\1\s\Ginger\ResultsDirectory
dir
# "vstest.console.exe GingerCoreTest.dll /Logger:trx"
# ./vstest.console.exe d:\a\1\s\Ginger\GingerCoreTest\bin\Release\GingerCoreTest.dll /Logger:trx   
Write-Host "-------------------------------------------------------------"
Write-Host "-                        Tests Completed                    -"
Write-Host "-------------------------------------------------------------"
exit 0