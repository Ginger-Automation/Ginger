
@rem ------------ JAVA HOME -------------------------------------------------

@rem You must set JAVA_HOME if JAVA_HOME_DEFAULT below does not match
@rem your machine installation of the JDK.

@rem set JAVA_HOME_DEFAULT=C:\Amdocs9.0.0.pb00\Eclipse372\plugins\com.amdocs.uif.tools.designer_9.0.0.pb00_20130228\Java
rem set JAVA_HOME_DEFAULT=c:\Program Files (x86)\Java\jdk1.7.0_51\
rem if not "%JAVA_HOME%" == "" goto CheckJavaHome

rem echo JAVA_HOME is not set in the environment - using %JAVA_HOME_DEFAULT%
rem set JAVA_HOME=%JAVA_HOME_DEFAULT%

@rem ------------------------------------------------------------------------
@rem Make sure this is a valid directory
@rem ------------------------------------------------------------------------

:CheckJavaHome
rem if exist "%JAVA_HOME%/bin/java.exe" goto CheckJavaVersion

rem echo JAVA_HOME=%JAVA_HOME% is not a valid Java home directory
rem exit

:CheckJavaVersion
set TEMPFILE=%TEMP%\java.version
"%JAVA_HOME%\bin\java" -version 2> %TEMPFILE%
for /F "tokens=3,3 eol=-" %%i in ('%SystemRoot%\system32\find "java version" %TEMPFILE%') do (set FULL_VERSION=%%i)
del %TEMPFILE%

for /F "tokens=1,1 delims=_" %%i in (%FULL_VERSION%) do set JAVA_VERSION=%%i

rem if %FULL_VERSION% == "1.7.0" goto RunScript

rem echo WARNING:  please verify that your java update version matches the current software platform matrix.  Your version = %FULL_VERSION%
goto RunScript
rem if %JAVA_VERSION% == 1.7.0 goto RunScript

rem echo Java version required = 1.7.0, currently = %JAVA_VERSION%
exit

:RunScript

set CRM="CRM_PBG_8_1"

@rem fix hard coded Yaron folder

rem OK for Yaron "%JAVA_HOME%\bin\javaw.exe" -Djava.library.path=C:\Yaron\TFS\NextGenWPF\Devs\GingerNextVer_Dev\\GingerASCFToolBox\v8 -Dfile.encoding=Cp1252 -classpath c:\tfs\Tools\CRMEverestJar\lib\jviewsall__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\jaxb-impl__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-contract-EM__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-qualityCM__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-CIM-MC__V8_1.jar;C:\Yaron\TFS\NextGenWPF\Devs\GingerNextVer_Dev\\GingerASCFToolBox\v8\classes;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\slf4j-api.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jdatepicker.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\commons-codec.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jexplorer.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jaxen-1.1-beta-9.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\activation.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\commons-httpclient.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jniwrap.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\winpack.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\je-runtime-license.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jdom.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\UIFFormLayout.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jsr173_1.0_api.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jnlp.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\UIFClient.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\AmdocsCore.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\commons-logging.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jniwrap-native.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jaxb-api.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\UIFCommon.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\AmdocsProcMgrLiteClient.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\slf4j-nop.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\UIFClientSource.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\comfyj.jar;c:\tfs\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jaxb-impl.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-iadmin__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\jsr173_1.0_api__V8_1.jar;c:\tfs\Tools\CRMEverestJar\bin;c:\tfs\Tools\CRMEverestJar\lib\OMS__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-shared__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-devicecare__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\jaxb-api__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\jnlp__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-logistics__V8_1.jar;c:\tfs\Tools\CRMEverestJar\AmdocsCRMSource.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-isales__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCore__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-morder__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-isupport__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsProcMgrLiteClient__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\ClfyMediaClient__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-common__V8_1.jar;c:\tfs\Tools\CRMEverestJar\lib\jacob\jacob.jar;c:\tfs\Tools\CRMEverestJar\lib\AmdocsCRM-BM-Collection__V8_1.jar com.amdocs.uif.workspace.Main $app com/amdocs/desktop/crm/AmdocsCim.uifad $user sa $pwd sa $locale en_US $workspace workspace.xml $ginger_port 7777

rem "%JAVA_HOME%"\bin\java.exe -XX:MaxPermSize=256M -Xmx512M -DmdiWorkspace.showLogbrowserKeyStroke="alt shift B" -classpath %CRM%\classes;C:\Yaron\TFS\NextGenWPF\Devs\GingerNextVer_Dev\GingerASCFToolBox\v8\classes;%CRM%\lib\*;%CRM%\lib\ilog\*;%CRM%\lib\jacob\* com.amdocs.uif.workspace.Main $app com/amdocs/desktop/crm/AmdocsSales.uifad $user Asmsa1 $pwd Asmsa1 $locale en_US $workspace workspace.xml >crm.log 2>&1
"C:\Program Files (x86)\Java\jre\bin\javaw.exe" -Djava.library.path=C:\TFS\GingerDev\GingerASCFToolBox -Dfile.encoding=Cp1252 -classpath C:\TFS\Tools\CRMEverestJar\lib\jviewsall__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\jaxb-impl__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-contract-EM__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-qualityCM__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-CIM-MC__V8_1.jar;C:\TFS\GingerDev\GingerASCFToolBox\classes;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\slf4j-api.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jdatepicker.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\commons-codec.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jexplorer.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jaxen-1.1-beta-9.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\activation.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\commons-httpclient.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jniwrap.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\winpack.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\je-runtime-license.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jdom.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\UIFFormLayout.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jsr173_1.0_api.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jnlp.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\UIFClient.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\AmdocsCore.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\commons-logging.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jniwrap-native.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jaxb-api.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\UIFCommon.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\AmdocsProcMgrLiteClient.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\slf4j-nop.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\UIFClientSource.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\comfyj.jar;C:\TFS\Tools\Amdocs8.2.0\\plugins\com.amdocs.uif.tools.designer_8.2.0\custom\jaxb-impl.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-iadmin__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\jsr173_1.0_api__V8_1.jar;C:\TFS\Tools\CRMEverestJar\bin;C:\TFS\Tools\CRMEverestJar\lib\OMS__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-shared__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-devicecare__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\jaxb-api__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\jnlp__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-logistics__V8_1.jar;C:\TFS\Tools\CRMEverestJar\AmdocsCRMSource.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-isales__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCore__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-morder__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-isupport__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsProcMgrLiteClient__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\ClfyMediaClient__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-common__V8_1.jar;C:\TFS\Tools\CRMEverestJar\lib\jacob\jacob.jar;C:\TFS\Tools\CRMEverestJar\lib\AmdocsCRM-BM-Collection__V8_1.jar com.amdocs.uif.workspace.Main $app com/amdocs/desktop/crm/AmdocsCim.uifad $user sa $pwd sa $locale en_US $workspace workspace.xml $ginger_port 7777

