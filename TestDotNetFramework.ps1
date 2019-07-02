Write-Host "Starting test for .Net Framework DLLs"
cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\Extensions\TestPlatform\" 
dir
./vstest.console.exe d:\a\1\s\Ginger\GingerCoreTest\bin\Release\GingerCoreTest.dll /Logger:trx    

Write-Host "Tests Completed"
exit 0