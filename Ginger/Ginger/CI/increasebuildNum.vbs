
Option Explicit
Dim fso,strFilename,strSearch,strReplace,objFile,oldContent,newContent, betaNum,HTMLFilename
 betaNum=""
 
 strFilename=WScript.Arguments(0)
 HTMLFilename=WScript.Arguments(1)

 call updateFile(strFilename, "(\d\.\d\.\d\.)(\d+)",false)
 call updateFile(HTMLFilename, "(>Ginger v2.\d beta )(\d+)",true)

 objFile.Close 

function updateFile(fileName, pattern,isSite)
'Does file exist?
    Dim re,Matches,ma
    Set fso=CreateObject("Scripting.FileSystemObject")
    if fso.FileExists(fileName)=false then
       wscript.echo "file "& fileName &" not found!"
       wscript.Quit
    end if
 
    'Read file
    set objFile=fso.OpenTextFile(fileName,1)
    oldContent=objFile.ReadAll
    
    Set re = New RegExp
    re.IgnoreCase = True
    re.Global = True
    re.Pattern = pattern'"(\d\.\d\.\d\.)(\d+)"
    'Write file
    Set Matches = re.Execute(oldContent)
    set ma=Matches(0)
    if betaNum="" then
        betaNum = ma.Submatches(1)+1
    end if
    if isSite then
        betaNum=betaNum-1
    end if 
    newContent=re.replace(oldContent,ma.Submatches(0) & betaNum)
    set objFile=fso.OpenTextFile(fileName,2)
    objFile.Write newContent

end function









