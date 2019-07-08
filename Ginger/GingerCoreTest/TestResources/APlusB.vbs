'GINGER_Description Sample Script to calculate a + b
'GINGER_$1 A
'GINGER_$2 B

Option Explicit

if WScript.Arguments.Count = 0 then
    WScript.Echo "Missing parameters"        
end if

' Your code here
Dim A
Dim B
Dim T
Dim txt 

A = WScript.Arguments(0)
B = WScript.Arguments(1)
T = CInt(A) + CInt(B)

txt = A + " + " + B + " = " + CStr(T)

' output results to Ginger
Wscript.echo "~~~GINGER_RC_START~~~" 
Wscript.echo "TXT=" + txt 
Wscript.echo "Total=" + CStr(T)
Wscript.echo "~~~GINGER_RC_END~~~"
