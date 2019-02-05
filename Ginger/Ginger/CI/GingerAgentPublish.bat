rem @echo off
setlocal
rem Adding Steps to publish GingerAgent and GingerAgent Starter
cscript C:\TFS\GingerDev\GingerNextVer_Dev\Ginger\CI\updateGingerAgentVersion.vbs C:\TFS\GingerDev\GingerNextVer_Dev\GingerCore\Drivers\JavaDriverLib\GingerJavaAgent\agent\com\amdocs\ginger\GingerAgent.java
cd C:\TFS\GingerDev\GingerNextVer_Dev\GingerCore\Drivers\JavaDriverLib\GingerJavaAgent
"c:\Program Files (x86)\java\jdk1.6.0_29\bin\javac.exe" -d bin -sourcepath agent -cp C:\Users\GingerGen\Ginger\javassist.jar;"c:\Program Files (x86)\java\jdk1.6.0_29\lib\tools.jar" agent/com/amdocs/ginger/GingerAgent.java
cd C:\TFS\GingerDev\GingerNextVer_Dev\GingerCore\Drivers\JavaDriverLib\GingerJavaAgent\bin
"c:\Program Files (x86)\java\jdk1.6.0_29\bin\jar.exe"  cvfm C:\TFS\GingerDev\GingerNextVer_Dev\Ginger\StaticDrivers\GingerAgent.jar C:\TFS\GingerDev\GingerNextVer_Dev\GingerCore\Drivers\JavaDriverLib\GingerJavaAgent\MANIFEST.MF . .
C:\eclipse\plugins\org.apache.ant_1.8.4.v201303080030\bin\ant -buildfile C:\TFS\GingerDev\GingerNextVer_Dev\Ginger\GingerAgentStarter.xml
