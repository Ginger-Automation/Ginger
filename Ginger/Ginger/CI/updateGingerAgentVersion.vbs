
Option Explicit
Dim fso,strGingerAgentFilename,strSearch,strReplace,objFile,oldContent,newContent, betaNum,HTMLFilename
 betaNum=""
 
 strGingerAgentFilename=WScript.Arguments(0)

 call updateFile(strGingerAgentFilename, "(v2.\d Beta )(\d+)",true)

 

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

    betaNum = ma.Submatches(1)+1
    
    newContent=re.replace(oldContent,ma.Submatches(0) & betaNum)
    set objFile=fso.OpenTextFile(fileName,2)	
    objFile.Write newContent   
    objFile.Close
    Set objFile =nothing
end function









