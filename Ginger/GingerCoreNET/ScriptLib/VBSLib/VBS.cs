#region License
/*
Copyright © 2014-2023 European Support Limited

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

using Amdocs.Ginger.Common;
using GingerCoreNET.RosLynLib;
using System;
using System.Diagnostics;
using System.Threading;

namespace GingerCore.GeneralLib
{
    public class VBS
    {

        string DataBuffer = "";
        string ErrorBuffer = "";
        string result;


        static CodeProcessor mCodeProcessor = new CodeProcessor();
        public static string ExecuteVBSEval(string Expr)
        {
            VBS vbs = new VBS();
            return vbs.Execute(Expr);
            //string result =mCodeProcessor.EvalExpression(Expr);
            //return result;
        }

        public string Execute(string Expr)
        {
            if (Environment.OSVersion.Platform.ToString().StartsWith("Win"))
            {


                Stopwatch st = Stopwatch.StartNew();
                //Create temp vbs file
                string fileName = "";

                try
                {
                    fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".vbs";
                }
                catch (Exception)
                {
                    //this to overcome speical IT settings which doesn't allow local personal folders
                    fileName = "" + Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\" + Guid.NewGuid().ToString() + ".vbs" + "";
                }
                string s = "Dim v" + Environment.NewLine;
                s += "v=" + Expr.Replace("\r\n", "vbCrLf").Replace("\n", "vbCrLf").Replace("\r", "vbCrLf") + Environment.NewLine;
                s += "Wscript.echo v" + Environment.NewLine;
                System.IO.File.WriteAllText(fileName, s);
                //Execute
                string result = RunVBS(fileName);

                //Delete the temp vbs file
                System.IO.File.Delete(fileName);

                if (result != null)
                {
                    result = result.Trim();
                }
                st.Stop();
                Reporter.ToLog(eLogLevel.DEBUG, "Executed VBS - Elapsed: " + st.ElapsedMilliseconds + " ,Expression: " + Expr + " ,Result: " + result);
                return result;
            }
            else
            {
                return Expr;
            }

        }


        string RunVBS(string fileName)
        {
            DataBuffer = "";
            ErrorBuffer = "";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            //if (File.Exists(GetSystemDirectory() + @"\cscript.exe"))
            //p.StartInfo.FileName = GetSystemDirectory() + @"\cscript.exe";
            //else
            p.StartInfo.FileName = @"cscript";


            p.OutputDataReceived += (proc, outLine) => { AddData(outLine.Data); };
            p.ErrorDataReceived += (proc, outLine) => { AddError(outLine.Data); };
            p.Exited += Process_Exited;

            p.StartInfo.Arguments = "\"" + fileName + "\" /B /Nologo";
            p.Start();

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            while (!p.HasExited)
            {
                Thread.Sleep(50);
            }
            p.Close();

            //catch (Exception e)
            //{
            //    // Reporter.ToLog(eAppReporterLogLevel.ERROR, e.Message);
            //    // this.Error = "Failed to execute the script. Details: " + e.Message;
            //}
            //if (!string.IsNullOrEmpty(ErrorBuffer))
            //{
            //    Error += ErrorBuffer;
            //}
            return result;
        }




        protected void AddError(string outLine)
        {
            if (!string.IsNullOrEmpty(outLine))
            {
                ErrorBuffer += outLine + "\n";
            }
        }

        protected void AddData(string outLine)
        {
            DataBuffer += outLine + "\n";
        }

        protected void Process_Exited(object sender, EventArgs e)
        {
            string[] s = DataBuffer.Split('\n');
            result = s[3];
        }


    }
}


