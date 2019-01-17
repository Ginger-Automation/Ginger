using Amdocs.Ginger.Common;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerWeb.RepositoryLib;
using GingerWeb.UsersLib;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GingerWeb.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        // temp remove from here !!!!!!!!!!!
        static bool bDone;

        [HttpGet("[action]")]
        public IEnumerable<object> WeatherForecasts()        
        {
            if (!bDone)
            {
                General.init();
                bDone = true;
            }

            ObservableList<BusinessFlow> Bfs = General.SR.GetAllRepositoryItems<BusinessFlow>();
            var v2 = Bfs.Select(x => 
                                    new
                                    {
                                        name = x.Name,
                                        description = x.Description
                                    });

            return v2;            
        }
        

        [HttpPost("[action]")]
        public BusinessFlowWrapper RunBusinessFlow(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "String Service List Concat";
                //return null;
            }

            BusinessFlow BF = (from x in General.SR.GetAllRepositoryItems<BusinessFlow>() where x.Name == name select x).SingleOrDefault();
            RunFlow(BF);
            // GenerateReport();

            BusinessFlowWrapper businessFlowWrapper = new BusinessFlowWrapper(BF);
            return businessFlowWrapper;
        }

        void GenerateReport()
        {
            ExecutionLoggerConfiguration executionLoggerConfiguration = new ExecutionLoggerConfiguration();            
            // HTMLReportsConfiguration currentConf = WorkSpace.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            //get logger files
            string exec_folder = Ginger.Run.ExecutionLogger.GetLoggerDirectory(executionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + Ginger.Run.ExecutionLogger.defaultAutomationTabLogName);
            //create the report
            string reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), true, null, null, false, 100000);

            if (reportsResultFolder == string.Empty)
            {
                Reporter.ToUser(eUserMsgKey.AutomationTabExecResultsNotExists);
            }
            else
            {
                foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
                {
                    string fileName = Path.GetFileName(txt_file);
                    if (fileName.Contains(".html"))
                    {
                        System.Diagnostics.Process.Start(reportsResultFolder);
                        System.Diagnostics.Process.Start(reportsResultFolder + "\\" + fileName);
                    }
                }
            }
        }

        ExecutionLogger executionLogger;
        void RunFlow(BusinessFlow businessFlow)
        {
            GingerRunner gingerRunner = new GingerRunner();
            ProjEnvironment projEnvironment = new ProjEnvironment();
            executionLogger = new ExecutionLogger(projEnvironment, eExecutedFrom.Automation);
            executionLogger.Configuration.ExecutionLoggerConfigurationIsEnabled = true;
            gingerRunner.RunListeners.Add(executionLogger);
            gingerRunner.RunBusinessFlow(businessFlow, true);

            Console.WriteLine("Execution completed");
            Console.WriteLine("Business Flow Status: " + businessFlow.RunStatus);
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
            }
        }


    }
}
