#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
using GingerCore.Actions;

namespace GingerCore.GeneralLib
{
    public class VBS
    {       
        public static string ExecuteVBSEval(string Expr)
        {
            //Create temp vbs file
            string fileName="";
            try
            {
                fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".vbs";             
                
            }catch(Exception)
            {
                   //this to overcome speical IT settings which doesn't allow local personal folders
                fileName = "" + Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\" + Guid.NewGuid().ToString() + ".vbs" + "";
            }
            string s = "Dim v" + Environment.NewLine;
            s += "v=" + Expr.Replace("\r\n", "vbCrLf").Replace("\n", "vbCrLf").Replace("\r", "vbCrLf") + Environment.NewLine;            
            s += "Wscript.echo v" + Environment.NewLine;
            System.IO.File.WriteAllText(fileName, s);
            //Execute
            ActScript scr = new ActScript();
            scr.AddNewReturnParams = true;
            scr.ScriptCommand = ActScript.eScriptAct.Script;
            scr.ScriptName = fileName;
            
            scr.Execute();

            //Delete the temp vbs file
            System.IO.File.Delete(fileName);

            string result = string.Empty;
            if (Expr.StartsWith("IsDate(CDate"))
            {
                //Return the result
                result = scr.ReturnValues[0].Actual;
                if (result != null) result = result.Trim() == "-1" ? "true" : "false";
            }
            else
            {
                //Return the result
                result = scr.ReturnValues[0].Actual;
                if (result != null) result = result.Trim();
            }

            return result;
        }       
    }
}
