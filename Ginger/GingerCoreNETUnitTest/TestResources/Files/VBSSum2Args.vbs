'GINGER_Description Calcultor
'GINGER_$ Var1
'GINGER_$ Var2

'Get Inputs from CLI
Var1=WScript.Arguments(0)
Var2=WScript.Arguments(1)

Dim sum
sum= CInt(Var1) + CInt(Var2)

Wscript.Echo "~~~GINGER_RC_START~~~"
WScript.Echo "Var1=" & Var1
WScript.Echo "Var2=" & Var2
WScript.Echo "sum=" & sum
Wscript.Echo "~~~GINGER_RC_END~~~"