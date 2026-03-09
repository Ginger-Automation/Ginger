#region License
/*
Copyright © 2014-2026 European Support Limited

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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
namespace GingerCore.Actions.RobotFramework
{
    public class ActRobot : ActWithoutDriver
    {

        public ActInputValue PythonExecutable => GetOrCreateInputParam(nameof(PythonExecutable));

        public ActInputValue RobotExecutable => GetOrCreateInputParam(nameof(RobotExecutable));

        public ActInputValue RobotFileName => GetOrCreateInputParam(nameof(RobotFileName));

        public ActInputValue RobotLibraries => GetOrCreateInputParam(nameof(RobotLibraries));

        public override string ActionDescription { get { return "Robot File Action"; } }

        public override eImageType Image { get { return eImageType.Robot; } }

        public override string ActionUserDescription { get { return "Description for Robot File Action"; } }

        public override string ActionType { get { return "Robot"; } }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

        public override bool ValueConfigsNeeded { get { return false; } }

        public override string ActionEditPage { get { return "RobotFramework.ActRobotEditPage"; } }

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to call Robot script from Ginger");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a Robot action, enter robot libraries location and the Robot File Location");
        }

        public List<Variables.VariableBase> CreateBusinessAndActivityVariablesToList()
        {
            List<Variables.VariableBase> lstVarBase =
            [
                // business flow variables
                .. this.RunOnBusinessFlow.Variables,
                // activity variables
                .. this.RunOnBusinessFlow.CurrentActivity.Variables,
            ];

            return lstVarBase;
        }

        public void WriteVariablesToJSONFile_V2(string fileName, List<GingerParam> gingerParamsLst)
        {
            StringBuilder sbr = new StringBuilder();
            int recNum = 0;

            sbr.Append("{");
            foreach (GingerParam paramLine in gingerParamsLst)
            {
                if (recNum > 0)
                {
                    sbr.Append(", ");
                }

                sbr.Append("\"");
                sbr.Append(paramLine.key);
                sbr.Append("\"");
                sbr.Append(":");
                sbr.Append("\"");
                sbr.Append(paramLine.value);
                sbr.Append("\"");
                recNum++;
            }
            sbr.Append("}");

            //open file stream
            using (StreamWriter file = File.CreateText(fileName))
            {
                file.WriteLine(JSONHelper.FormatJSON(sbr.ToString()));
            }
        }

        public List<GingerParam> GetCreateBusinessAndActivityVariablesToJSONList()
        {
            List<GingerParam> lstGingerParam = [];

            // solution variables except password variables
            foreach (Variables.VariableBase varBase in this.RunOnBusinessFlow.GetSolutionVariables().Where(vrb => vrb is not VariablePasswordString))
            {
                GingerParam gingerParam = new GingerParam
                {
                    key = varBase.Name != null ? varBase.Name.ToString() : "",
                    value = varBase.Value != null ? varBase.Value.ToString() : ""
                };
                lstGingerParam.Add(gingerParam);
            }

            // business flow variables except password variables
            foreach (Variables.VariableBase varBase in this.RunOnBusinessFlow.Variables.Where(vrb => vrb is not VariablePasswordString))
            {
                GingerParam gingerParam = new GingerParam
                {
                    key = varBase.Name != null ? varBase.Name.ToString() : "",
                    value = varBase.Value != null ? varBase.Value.ToString() : ""
                };
                if (!checkVariableAlreadyExists(lstGingerParam, gingerParam.key))
                {
                    lstGingerParam.Add(gingerParam);
                }
            }

            // activity variables except password variables
            foreach (Variables.VariableBase varBase in this.RunOnBusinessFlow.CurrentActivity.Variables.Where(vrb => vrb is not VariablePasswordString))
            {
                GingerParam gingerParam = new GingerParam
                {
                    key = varBase.Name != null ? varBase.Name.ToString() : "",
                    value = varBase.Value != null ? varBase.Value.ToString() : ""
                };
                if (!checkVariableAlreadyExists(lstGingerParam, gingerParam.key))
                {
                    lstGingerParam.Add(gingerParam);
                }
            }
            return lstGingerParam;
        }


        private bool checkVariableAlreadyExists(List<GingerParam> lstGingerParams, string key)
        {
            bool alreadyExists = false;
            foreach (GingerParam ginParam in lstGingerParams)
            {
                if (ginParam.key.Equals(key))
                {
                    alreadyExists = true; break;
                }
            }
            return alreadyExists;
        }

        private void ResetValues()
        {
            DataBuffer = "";
            ErrorBuffer = "";
        }

        public override void Execute()
        {
            try
            {
                // reset values 
                ResetValues();

                List<GingerParam> lstGingerVars = GetCreateBusinessAndActivityVariablesToJSONList();
                string fileName = System.IO.Path.GetTempFileName().Replace(".tmp", ".json");
                WriteVariablesToJSONFile_V2(fileName, lstGingerVars);

                string robotFileName = RobotFileName.ValueForDriver;
                if (!File.Exists(robotFileName))
                {
                    Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    Error = "Robot File not found at: - " + robotFileName + @", Make sure to include full path";
                    return;
                }

                //overriding to hardcode for now
                // robot with library path

                string robotLibraries = RobotLibraries.ValueForDriver;
                StringBuilder sbr = new StringBuilder();
                sbr.Append("call robot");
                sbr.Append(" ");

                // adding libraries
                if (robotLibraries != null && robotLibraries != string.Empty)
                {
                    sbr.Append("--pythonpath ");
                    sbr.Append(robotLibraries);
                }

                // adding variable filename as command line parameter
                sbr.Append(" ");
                sbr.Append("--variable fileName:");
                sbr.Append(fileName);
                sbr.Append(" ");
                sbr.Append(@robotFileName);

                string commandString = sbr.ToString();

                ExecuteCommandSync(commandString); // make run and wait 
            }
            catch (Exception ex)
            {
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                Error = ex.Message;
            }
        } // end of Execute


        public void ExecuteCommandSync(string command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                        new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command)
                        {
                            // The following commands are needed to redirect the standard output.
                            // This means that it will be redirected to the Process.StandardOutput StreamReader.
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,       // redirect standard error
                            UseShellExecute = false,

                            // Do not create the black window.
                            CreateNoWindow = true
                        };

                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process procss = new System.Diagnostics.Process
                {
                    StartInfo = procStartInfo
                };

                procss.OutputDataReceived += (proc, outLine) => { AddData(outLine.Data + "\n"); };
                procss.ErrorDataReceived += (proc, outLine) => { AddError(outLine.Data + "\n"); };
                procss.Exited += Process_Exited;

                procss.Start();

                procss.BeginOutputReadLine();
                procss.BeginErrorReadLine();
                procss.WaitForExit();
                while (!procss.HasExited)
                {
                    Thread.Sleep(100);
                }

                // Get the output into a string
                ExInfo = command + Environment.NewLine + DataBuffer;

                string pwd = Directory.GetCurrentDirectory().Replace("\\", "/");
                string outputFile = pwd + "/output.xml";

                // execution details in the list and need to process them as needed
                List<RobotTestCase> lstRobotTCs = ReadXML_GetRobotTestExecutionDetails(@outputFile);

                string reportOutput = FormatRobotStatsListAsPipeSeparated(lstRobotTCs);

                // appending the execution stats
                ExInfo += reportOutput;

                // add output variables 
                AddRobotStatsListToOutputVariables(lstRobotTCs);
            }
            catch (Exception ex)
            {
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                Error = ex.Message;
            }
        } // end of ExecuteCommandSync

        string DataBuffer = "";
        string ErrorBuffer = "";

        protected void AddData(string outLine)
        {
            DataBuffer += outLine;
        }
        protected void AddError(string outLine)
        {
            ErrorBuffer += outLine;
        }

        protected void Process_Exited(object sender, EventArgs e)
        {
            ParseRC(DataBuffer);
        }

        public int ReadXML_GetRobotTestExecutionStatus(string fileName)
        {
            int testStatus = 0;
            int statPass = 0;
            int statFail = 0;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            XmlNodeList nodeList = xmlDoc.SelectNodes("//statistics/total/stat");
            foreach (XmlNode statNode in nodeList)
            {
                if (statNode.InnerText.Equals("All Tests"))
                {
                    statPass = int.Parse(statNode.Attributes["pass"].Value);
                    statFail = int.Parse(statNode.Attributes["fail"].Value);
                    break;
                }
            }

            if (statPass > 0 || statFail > 0)
            {
                if (statFail > 0) { testStatus = 0; }
                else { testStatus = 1; }
            }

            return testStatus;
        } // end of ReadXML_GetRobotTestExecutionStatus

        private string DuplicateToTempFile(string fileName)
        {
            string tmpFile = fileName + ".tmp";
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }
            File.Copy(fileName, tmpFile);

            return tmpFile;
        }

        public List<RobotTestCase> ReadXML_GetRobotTestExecutionDetails(string fileName)
        {
            List<RobotTestCase> lstRobotTCs = [];

            XmlDocument xmlDoc = new XmlDocument();

            if (File.Exists(fileName))
            {
                string tmpFile = DuplicateToTempFile(fileName);
                xmlDoc.Load(tmpFile);

                XmlNodeList nodeList = xmlDoc.SelectNodes("//suite/test");
                foreach (XmlNode testNode in nodeList)
                {
                    RobotTestCase robotTC = new RobotTestCase
                    {
                        test_id = testNode.Attributes["id"].Value,
                        test_name = testNode.Attributes["name"].Value
                    };

                    XmlNode xNodeStatus = testNode.SelectSingleNode("status");
                    if (xNodeStatus != null)
                    {
                        robotTC.test_starttime = xNodeStatus.Attributes["starttime"].Value;
                        robotTC.test_endtime = xNodeStatus.Attributes["endtime"].Value;
                        robotTC.test_status = xNodeStatus.Attributes["status"].Value;
                    }

                    if (robotTC != null)
                    {
                        lstRobotTCs.Add(robotTC);
                    }
                }
            }

            return lstRobotTCs;

        } // end of ReadXML_GetRobotTestExecutionDetails


        private string FormatRobotStatsListAsPipeSeparated(List<RobotTestCase> lstRobotTCs)
        {
            StringBuilder sRptContent = new StringBuilder();
            sRptContent.Append("| Test ID ");
            sRptContent.Append("| Test Name ");
            sRptContent.Append("| Start Time ");
            sRptContent.Append("| End Time ");
            sRptContent.Append("| Status ");
            sRptContent.AppendLine("|");

            foreach (RobotTestCase roboTC in lstRobotTCs)
            {
                sRptContent.Append("| " + roboTC.test_id + " ");
                sRptContent.Append("| " + roboTC.test_name + " ");
                sRptContent.Append("| " + roboTC.test_starttime + " ");
                sRptContent.Append("| " + roboTC.test_endtime + " ");
                sRptContent.Append("| " + roboTC.test_status + " ");
                sRptContent.AppendLine("|");
            }

            return sRptContent.ToString();
        }

        private void AddRobotStatsListToOutputVariables(List<RobotTestCase> lstRobotTCs)
        {
            foreach (RobotTestCase roboTC in lstRobotTCs)
            {
                // allowing to add new TC's from Robot
                AddNewReturnParams = true;
                AddOrUpdateReturnParamActual(roboTC.test_name, roboTC.test_status, "PASS");
            }
        }
    } // end of class
} // end of namespace