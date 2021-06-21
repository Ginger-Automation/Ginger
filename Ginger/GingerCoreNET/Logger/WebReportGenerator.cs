#region License
/*
Copyright © 2014-2021 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Ginger.Reports;
using Ginger.Reports.GingerExecutionReport;
using Amdocs.Ginger.CoreNET.Utility;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.Logger
{
    public class WebReportGenerator
    {
        private string browserPath = "chrome.exe";
        public WebReportGenerator()
        {

        }

        // TODO: Remove  browserNewPath
        public WebReportGenerator(string browserNewPath)
        {
            this.browserPath = browserNewPath;
        }

        // TODO: Make this function to just generate the report folder !!!
        public LiteDbRunSet RunNewHtmlReport(string reportResultsFolderPath = "", string runSetGuid = null, WebReportFilter openObject = null, bool shouldDisplayReport = true)
        {
            //Copy folder to reportResultsFolderPath or Execution logger
            string reportsResultFolder = string.Empty;
            if (!string.IsNullOrEmpty(reportResultsFolderPath))
            {
                reportsResultFolder = reportResultsFolderPath;
            }
            else 
            {
                HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
                reportsResultFolder = Path.Combine(ExtensionMethods.GetReportDirectory(currentConf.HTMLReportsFolder), "Reports", "Ginger-Web-Client");
            }
            try
            {
                string clientAppFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "Ginger-Web-Client");
                Reporter.ToLog(eLogLevel.INFO, "Copying web report folder from: " + clientAppFolderPath);
                
                Reporter.ToLog(eLogLevel.INFO, "Copying web report folder to: " + reportsResultFolder);
                if (Directory.Exists(clientAppFolderPath))
                {
                    string rootFolder = Path.Combine(reportsResultFolder);
                    if (Directory.Exists(rootFolder))
                    {
                        IoHandler.Instance.TryFolderDelete(rootFolder);
                    }
                    IoHandler.Instance.CopyFolderRec(clientAppFolderPath, reportsResultFolder, true);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Check WebReportFolder Error: " + ex.Message, ex);
            }

            //get exeution data and replace
            LiteDbRunSet lightDbRunSet = new LiteDbRunSet();
            bool response = false;
            try
            {
                if (!Directory.Exists(reportsResultFolder))
                {
                    return lightDbRunSet;
                }
                IoHandler.Instance.DeleteFoldersData(Path.Combine(reportsResultFolder, "assets", "Execution_Data"));
                IoHandler.Instance.DeleteFoldersData(Path.Combine(reportsResultFolder, "assets", "screenshots"));
                LiteDbManager dbManager = new LiteDbManager(new ExecutionLoggerHelper().GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder));              
                lightDbRunSet = dbManager.GetLatestExecutionRunsetData(runSetGuid);
                PopulateMissingFields(lightDbRunSet, reportsResultFolder);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(lightDbRunSet);
                response = RunClientApp(json, reportsResultFolder, openObject, shouldDisplayReport);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "RunNewHtmlReport,error :"+ex.ToString());
            }
            return lightDbRunSet;
        }


        // TODO: Remove from here as this class is WebReportGenerator - not viewer
        private bool RunClientApp(string json, string clientAppFolderPath, WebReportFilter openObject, bool shouldDisplayReport)
        {
            bool response = false;


            try
            {
                json = $"window.runsetData={json};";

#warning Report Fix MEN not stable approach 
                StringBuilder pageDataSb = new StringBuilder();
                pageDataSb.Append("file:///");
                pageDataSb.Append(clientAppFolderPath.Replace('\\', '/'));
                pageDataSb.Append("/");
                pageDataSb.Append("index.html");
                if (openObject != null)
                {
                    pageDataSb.Append("#/?Routed_Guid=");
                    pageDataSb.Append(openObject.Guid);
                }
                string taskCommand = $"\"{pageDataSb.ToString()}\"";
                System.IO.File.WriteAllText(Path.Combine(clientAppFolderPath, "assets", "Execution_Data", "executiondata.js"), json);

                if (shouldDisplayReport && !Assembly.GetEntryAssembly().FullName.ToUpper().Contains("CONSOLE"))
                {
                    System.Diagnostics.Process.Start(@browserPath, taskCommand);
                    System.Diagnostics.Process.Start(clientAppFolderPath);
                }
                response = true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in RunClientApp", ex);
            }

            return response;
        }

        
        //TODO move it to utils class
        // Create test class

        private void PopulateMissingFields(LiteDbRunSet liteDbRunSet, string clientAppPath)
        {
            //select template 
            HTMLReportConfiguration _HTMLReportConfig = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>().Where(x => (x.IsDefault == true)).FirstOrDefault();

            //populate data based on level
            if (string.IsNullOrEmpty(_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()))
            {
                _HTMLReportConfig.ExecutionStatisticsCountBy = HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions;
            }

            string imageFolderPath = Path.Combine(clientAppPath, "assets", "screenshots");
            List<string> runSetEnv = new List<string>();

            liteDbRunSet.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbRunSet.ChildExecutedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbRunSet.ChildExecutableItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

            liteDbRunSet.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbRunSet.ChildPassedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbRunSet.ChildExecutedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

            if (liteDbRunSet.Elapsed.HasValue)
            {
                liteDbRunSet.Elapsed = Math.Round(liteDbRunSet.Elapsed.Value, 2);
            }
            foreach (LiteDbRunner liteDbRunner in liteDbRunSet.RunnersColl)
            {
                if (!runSetEnv.Contains(liteDbRunner.Environment))
                {
                    runSetEnv.Add(liteDbRunner.Environment);
                }
                
                liteDbRunner.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbRunner.ChildExecutedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbRunner.ChildExecutableItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

                liteDbRunner.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbRunner.ChildPassedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbRunner.ChildExecutedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

                if (liteDbRunner.Elapsed.HasValue)
                {
                    liteDbRunner.Elapsed = Math.Round(liteDbRunner.Elapsed.Value, 2);
                }
                else { liteDbRunner.Elapsed = 0; }
                foreach (LiteDbBusinessFlow liteDbBusinessFlow in liteDbRunner.BusinessFlowsColl)
                {
                   
                    liteDbBusinessFlow.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbBusinessFlow.ChildExecutedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbBusinessFlow.ChildExecutableItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

                    liteDbBusinessFlow.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbBusinessFlow.ChildPassedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbBusinessFlow.ChildExecutedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

                    if (liteDbBusinessFlow.Elapsed.HasValue)
                    {
                        liteDbBusinessFlow.Elapsed = Math.Round(liteDbBusinessFlow.Elapsed.Value, 2);
                    }
                    else { liteDbBusinessFlow.Elapsed = 0; }
                    foreach (LiteDbActivity liteDbActivity in liteDbBusinessFlow.ActivitiesColl)
                    {
                        
                        liteDbActivity.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbActivity.ChildExecutedItemsCount, liteDbActivity.ChildExecutableItemsCount));

                        liteDbActivity.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbActivity.ChildPassedItemsCount, liteDbActivity.ChildExecutedItemsCount));

                        
                        if (liteDbActivity.Elapsed.HasValue)
                        {
                            liteDbActivity.Elapsed = Math.Round(liteDbActivity.Elapsed.Value / 1000, 4);
                        }
                        else { liteDbActivity.Elapsed = 0; }
                        foreach (LiteDbAction liteDbAction in liteDbActivity.ActionsColl)
                        {
                            List<string> newScreenShotsList = new List<string>();
                            if (liteDbAction.Elapsed.HasValue)
                            {
                                liteDbAction.Elapsed = Math.Round(liteDbAction.Elapsed.Value / 1000, 4);
                            }
                            else { liteDbAction.Elapsed = 0; }
                            if ((!string.IsNullOrEmpty(liteDbAction.ExInfo)) && liteDbAction.ExInfo[liteDbAction.ExInfo.Length - 1] == '-')
                                liteDbAction.ExInfo = liteDbAction.ExInfo.Remove(liteDbAction.ExInfo.Length - 1);
                            foreach (string screenshot in liteDbAction.ScreenShots)
                            {
                                string fileName = Path.GetFileName(screenshot);
                                string newScreenshotPath = Path.Combine(imageFolderPath, fileName);
                                if (File.Exists(screenshot))
                                {
                                    System.IO.File.Copy(screenshot, newScreenshotPath, true); //TODO - Replace with the real location under Ginger installation
                                    newScreenShotsList.Add(fileName);
                                }
                            }
                            liteDbAction.ScreenShots = newScreenShotsList;
                        }
                    }
                }
            }

            if (runSetEnv.Count > 0)
            {
                liteDbRunSet.Environment = string.Join(",", runSetEnv);
            }
        }
        private string CalculateExecutionOrPassRate(int firstItem, int secondItem)
        {
            if (secondItem != 0)
            {
                return (firstItem * 100 / secondItem).ToString();
            }
            else
            {
                return "0";
            }
        }

    }

}
