Dim WshShell, strCurDir
Set WshShell = CreateObject("WScript.Shell")
strCurDir    = WshShell.CurrentDirectory
strCurDir=Replace(strCurDir,"\scripts","")



wscript.Echo"~~~~GINGER_RC_START~~~~~~"
wSCRIPT.ECHO "DocumentFolder="&strCurDir
wscript.Echo"~~~~GINGER_RC_END~~~~~~"  

