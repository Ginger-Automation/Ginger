#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerWeb.UsersLib;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GingerWeb.Controllers
{
    [Route("api/[controller]")]
    public class BusinessFlowController : Controller
    {
        // temp remove from here !!!!!!!!!!!
        static bool bDone;

        public class RunBusinessFlowRequest
        {
            public string name { get; set; }
        }

        public class RunBusinessFlowResult
        {            
            public string Status { get; internal set; }

            public int Elapsed { get; internal set; }

            public string Report { get; internal set; }
        }

        [HttpGet("[action]")]
        public IEnumerable<object> BusinessFlows()
        {
            if (!bDone)
            {
                General.init();
                bDone = true;
            }

            IEnumerable<BusinessFlow> BusinessFlows = General.SR.GetAllRepositoryItems<BusinessFlow>().OrderBy(x => x.Name);
            var data = BusinessFlows.Select(x =>
                                    new
                                    {
                                        name = x.Name,
                                        description = x.Description,
                                        fileName = x.FileName,
                                        status = x.RunStatus.ToString()
                                    });

            return data;
        }


        [HttpPost("[action]")]
        public RunBusinessFlowResult RunBusinessFlow([FromBody] RunBusinessFlowRequest runBusinessFlowRequest)
        {
            Directory.Delete(jsonDumpFolder, true);
            Directory.CreateDirectory(jsonDumpFolder);
            RunBusinessFlowResult runBusinessFlowResult = new RunBusinessFlowResult();

            if (string.IsNullOrEmpty(runBusinessFlowRequest.name))
            {
                runBusinessFlowResult.Status = "Name cannot be null";
                return runBusinessFlowResult;
            }

            BusinessFlow BF = (from x in General.SR.GetAllRepositoryItems<BusinessFlow>() where x.Name == runBusinessFlowRequest.name select x).SingleOrDefault();
            if (BF == null)
            {
                runBusinessFlowResult.Status = "Name cannot be null";
                return runBusinessFlowResult;
            }

            RunFlow(BF);

            runBusinessFlowResult.Status = BF.RunStatus.ToString();
            runBusinessFlowResult.Elapsed = (int)BF.Elapsed;
            runBusinessFlowResult.Report = "ahhhh !!!!!!!!!!!!";



            return runBusinessFlowResult;
        }


        string jsonDumpFolder = @"c:\temp\Ginger\Dump\";  // !!!!!!!!!!!!!!!!!!!temp FIXME
        void GenerateReport(BusinessFlow businessFlow)    // temp remove BF from param
        {

            string BusinessFlowReportFolder = jsonDumpFolder + "1 " + businessFlow.GetNameForFileName();   // !!!!!!!!!!!!!!!! temp remove
            ReportInfo RI = new ReportInfo(BusinessFlowReportFolder);
            //Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(RI);

            string templatesFolder = @"C:\Users\yaronwe\source\repos\Ginger\Ginger\Ginger\Reports\GingerExecutionReport\"; // !!!!!!!!!!!!!!!!!!!!!!! temp fix me
            HTMLReportConfiguration selectedHTMLReportConfiguration = new HTMLReportConfiguration("DefaultTemplate", true);

            HTMLReportsConfiguration hTMLReportsConfiguration = new HTMLReportsConfiguration();

            string hTMLOutputFolder = @"C:\Temp\Ginger\Report"; // !!!!!!!!!!!!!!!!!!!!!!! temp fix me
            Directory.Delete(hTMLOutputFolder,true);
            Directory.CreateDirectory(hTMLOutputFolder);

            string report = Ginger.Reports.GingerExecutionReport.ExtensionMethods.NewFunctionCreateGingerExecutionReport(RI, true, selectedHTMLReportConfiguration, templatesFolder: templatesFolder, hTMLReportsConfiguration: hTMLReportsConfiguration, hTMLOutputFolder: hTMLOutputFolder);



            //ExecutionLoggerConfiguration executionLoggerConfiguration = new ExecutionLoggerConfiguration();
            //// HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            ////get logger files
            //string exec_folder = @"c:\temp\hh"; // Ginger.Run.ExecutionLogger.GetLoggerDirectory(executionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + Ginger.Run.ExecutionLogger.defaultAutomationTabLogName);string templatesFolder = @"C:\Users\yaronwe\source\repos\Ginger\Ginger\Ginger\Reports\GingerExecutionReport\";
            //string templatesFolder = @"C:\Users\yaronwe\source\repos\Ginger\Ginger\Ginger\Reports\GingerExecutionReport\";
            ////create the report
            //HTMLReportConfiguration hTMLReportConfiguration = new HTMLReportConfiguration("DefaultTemplate", true); 
            //string reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.NewFunctionCreateGingerExecutionReport(new ReportInfo(exec_folder), true, hTMLReportConfiguration, templatesFolder, false, 100000);

            //if (reportsResultFolder == string.Empty)
            //{
            //    Reporter.ToUser(eUserMsgKey.AutomationTabExecResultsNotExists);
            //}
            //else
            //{
            //    foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
            //    {
            //        string fileName = Path.GetFileName(txt_file);
            //        if (fileName.Contains(".html"))
            //        {
            //            System.Diagnostics.Process.Start(reportsResultFolder);
            //            System.Diagnostics.Process.Start(reportsResultFolder + "\\" + fileName);
            //        }
            //    }
            //}
        }

        //ExecutionLogger executionLogger;
        void RunFlow(BusinessFlow businessFlow)
        {
            GingerRunner gingerRunner = new GingerRunner();            
            ExecutionLogger ex = (ExecutionLogger)gingerRunner.RunListeners[0];  // temp until we remove it from GR constructor and add manually
            ex.ExecutionLogfolder = jsonDumpFolder;
            ex.Configuration.ExecutionLoggerConfigurationIsEnabled = true;
            //ex.exec
            // ex.Configuration.exe
            // TODO: add dumper

            ProjEnvironment projEnvironment = new ProjEnvironment();
            //ExecutionDumperListener executionDumperListener = new ExecutionDumperListener(@"c:\temp\dumper");   // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!! temp
            //gingerRunner.RunListeners.Add(executionDumperListener);


            // executionLogger = new ExecutionLogger(projEnvironment, eExecutedFrom.Automation);
            // executionLogger.Configuration.ExecutionLoggerConfigurationIsEnabled = true;
            // gingerRunner.RunListeners.Add(executionLogger);
            gingerRunner.BusinessFlows.Clear();
            gingerRunner.BusinessFlows.Add(businessFlow);
            gingerRunner.CurrentBusinessFlow = businessFlow;
            gingerRunner.RunRunner();

            Console.WriteLine("Execution Completed");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Elapsed: " + businessFlow.Elapsed);
            Console.WriteLine("Business Flow: " + businessFlow.Name);
            Console.WriteLine("Business Flow Description: " + businessFlow.Description);
            Console.WriteLine("Business Flow Status: " + businessFlow.RunStatus);
            Console.WriteLine("Activities Count: " + businessFlow.Activities.Count);
            Console.WriteLine("----------------------------");

            foreach (Activity activity in businessFlow.Activities)
            {
                Console.WriteLine("Activity: " + activity.ActivityName + " Status: " + activity.Status);

                Console.WriteLine("Actions Found:" + activity.Acts.Count);
                foreach (Act act in activity.Acts)
                {
                    Console.WriteLine("--");
                    Console.WriteLine("Action:" + act.Description);
                    Console.WriteLine("Description:" + act.ActionDescription);
                    Console.WriteLine("Type:" + act.ActionType);
                    Console.WriteLine("Class:" + act.ActClass);
                    Console.WriteLine("Status:" + act.Status);
                    Console.WriteLine("Error:" + act.Error);
                    Console.WriteLine("ExInfo:" + act.ExInfo);
                }
                Console.WriteLine("----------");
            }

            GenerateReport(businessFlow);

            //TODO: create report

        }


    }
}
