echo "-------------------------------------------------------------"
echo "-                         Run CLI Tests                     -"
echo "-------------------------------------------------------------"

cd /home/runner/work/Ginger/Ginger/Ginger/GingerRuntime/bin/Release/net6.0/publish/
pwd
ls -alt

echo "******************************************************************************************************************************"
echo "help run"
echo "******************************************************************************************************************************"

dotnet GingerRuntime.dll help run


echo "******************************************************************************************************************************"
echo "run simple solution"
echo "******************************************************************************************************************************"

dotnet GingerRuntime.dll run -s "/home/runner/work/Ginger/Ginger/Ginger/GingerCoreNETUnitTest/TestResources/Solutions/CLI" -e "Default" -r "Default Run Set"


# echo "******************************************************************************************************************************"
# echo "analyze solution"
# echo "******************************************************************************************************************************"

# dotnet GingerRuntime.dll analyze -s "/home/runner/work/Ginger/Ginger/Ginger/GingerCoreNETUnitTest/TestResources/Solutions/CLI" 

# exit $exitcode