using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Amdocs.Ginger.CoreNET.Execution;

namespace Amdocs.Ginger.CoreNET.Logger
{
    public class WebReportGenerator
    {
        private string browserPath = "chrome.exe";
        public WebReportGenerator()
        {

        }

        public WebReportGenerator(string browserNewPath)
        {
            this.browserPath = browserNewPath;
        }

        public bool RunNewHtmlReport(string runSetGuid = null, WebReportFilter openObject = null, bool shouldOpenBrowser = true)
        {
            bool response = false;
            try
            {
                string clientAppFolderPath = Path.Combine(WorkSpace.Instance.LocalUserApplicationDataFolderPath, "Reports", "Ginger-Web-Client");
                if (!Directory.Exists(clientAppFolderPath))
                    return false;
                DeleteFoldersData(Path.Combine(clientAppFolderPath, "assets", "Execution_Data"));
                DeleteFoldersData(Path.Combine(clientAppFolderPath, "assets", "screenshots"));
                LiteDbManager dbManager = new LiteDbManager(WorkSpace.Instance.Solution.LoggerConfigurations.ExecutionLoggerConfigurationExecResultsFolder);
                var result = dbManager.GetRunSetLiteData();
                List<LiteDbRunSet> filterData = null;
                if (!string.IsNullOrEmpty(runSetGuid))
                {
                    filterData = result.IncludeAll().Find(a => a._id.ToString() == runSetGuid).ToList();
                }
                else
                    filterData = dbManager.FilterCollection(result, Query.All());
                LiteDbRunSet lightDbRunSet = filterData.Last();
                PopulateMissingFields(lightDbRunSet, clientAppFolderPath);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(filterData.Last());
                response = RunClientApp(json, clientAppFolderPath, openObject, shouldOpenBrowser);
            }
            catch (Exception ex)
            {

            }
            return response;

        }

        private bool RunClientApp(string json, string clientAppFolderPath, WebReportFilter openObject, bool shouldOpenBrowser = true)
        {
            bool response = false;

            try
            {
                json = $"window.runsetData={json}";
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
                if (shouldOpenBrowser)
                    System.Diagnostics.Process.Start(@browserPath, taskCommand);
                response = true;
            }
            catch (Exception ec)
            {

            }
            return response;
        }

        private void DeleteFoldersData(string clientAppFolderPath)
        {
            DirectoryInfo dir = new DirectoryInfo(clientAppFolderPath);
            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }
        }

        //TODO move it to utils class
        private void PopulateMissingFields(LiteDbRunSet liteDbRunSet, string clientAppPath)
        {
            string imageFolderPath = Path.Combine(clientAppPath, "assets", "screenshots");

            int totalRunners = liteDbRunSet.RunnersColl.Count;
            int totalPassed = liteDbRunSet.RunnersColl.Where(runner => runner.RunStatus == eRunStatus.Passed.ToString()).Count();
            int totalExecuted = totalRunners - liteDbRunSet.RunnersColl.Where(runner => runner.RunStatus == eRunStatus.Pending.ToString() || runner.RunStatus == eRunStatus.Skipped.ToString() || runner.RunStatus == eRunStatus.Blocked.ToString()).Count();
            if (totalRunners != 0)
                liteDbRunSet.ExecutionRate = string.Format("{0:F1}", (totalExecuted * 100 / totalRunners).ToString());
            if (totalRunners != 0)
                liteDbRunSet.PassRate = string.Format("{0:F1}", (totalPassed * 100 / totalRunners).ToString());

            foreach (LiteDbRunner liteDbRunner in liteDbRunSet.RunnersColl)
            {

                int totalBFs = liteDbRunner.BusinessFlowsColl.Count;
                int totalPassedBFs = liteDbRunner.BusinessFlowsColl.Where(bf => bf.RunStatus == eRunStatus.Passed.ToString()).Count();
                int totalExecutedBFs = totalBFs - liteDbRunner.BusinessFlowsColl.Where(bf => bf.RunStatus == eRunStatus.Pending.ToString() || bf.RunStatus == eRunStatus.Skipped.ToString() || bf.RunStatus == eRunStatus.Blocked.ToString()).Count();
                if (totalBFs != 0)
                    liteDbRunner.ExecutionRate = string.Format("{0:F1}", (totalExecutedBFs * 100 / totalBFs).ToString());
                if (totalExecutedBFs != 0)
                    liteDbRunner.PassRate = string.Format("{0:F1}", (totalPassedBFs * 100 / totalExecutedBFs).ToString());
                if (liteDbRunner.Elapsed.HasValue)
                    liteDbRunner.Elapsed = Math.Round(liteDbRunner.Elapsed.Value, 2);
                foreach (LiteDbBusinessFlow liteDbBusinessFlow in liteDbRunner.BusinessFlowsColl)
                {
                    int totalActivities = liteDbBusinessFlow.ActivitiesColl.Count;
                    int totalPassedActivities = liteDbBusinessFlow.ActivitiesColl.Where(ac => ac.RunStatus == eRunStatus.Passed.ToString()).Count();
                    int totalExecutedActivities = totalActivities - liteDbBusinessFlow.ActivitiesColl.Where(ac => ac.RunStatus == eRunStatus.Pending.ToString() || ac.RunStatus == eRunStatus.Skipped.ToString() || ac.RunStatus == eRunStatus.Blocked.ToString()).Count();
                    if (totalActivities != 0)
                        liteDbBusinessFlow.ExecutionRate = string.Format("{0:F1}", (totalExecutedActivities * 100 / totalActivities).ToString());
                    if (totalExecutedActivities != 0)
                        liteDbBusinessFlow.PassRate = string.Format("{0:F1}", (totalPassedActivities * 100 / totalExecutedActivities).ToString());
                    if (liteDbBusinessFlow.Elapsed.HasValue)
                        liteDbBusinessFlow.Elapsed = Math.Round(liteDbBusinessFlow.Elapsed.Value, 2);
                    foreach (LiteDbActivity liteDbActivity in liteDbBusinessFlow.ActivitiesColl)
                    {
                        int totalActions = liteDbActivity.ActionsColl.Count;
                        int totalPassedActions = liteDbActivity.ActionsColl.Where(ac => ac.RunStatus == eRunStatus.Passed.ToString()).Count();
                        int totalExecutedActions = totalActions - liteDbActivity.ActionsColl.Where(ac => ac.RunStatus == eRunStatus.Pending.ToString() || ac.RunStatus == eRunStatus.Skipped.ToString() || ac.RunStatus == eRunStatus.Blocked.ToString()).Count();
                        if (totalActions != 0)
                            liteDbActivity.ExecutionRate = string.Format("{0:F1}", (totalExecutedActions * 100 / totalActions).ToString());
                        if (totalExecutedActions != 0)
                            liteDbActivity.PassRate = string.Format("{0:F1}", (totalPassedActions * 100 / totalExecutedActions).ToString());
                        if (liteDbActivity.Elapsed.HasValue)
                            liteDbActivity.Elapsed = Math.Round(liteDbActivity.Elapsed.Value, 2);
                        foreach (LiteDbAction liteDbAction in liteDbActivity.ActionsColl)
                        {
                            List<string> newScreenShotsList = new List<string>();
                            if (liteDbAction.Elapsed.HasValue)
                                liteDbAction.Elapsed = Math.Round(liteDbAction.Elapsed.Value, 2);
                            if ((!string.IsNullOrEmpty(liteDbAction.ExInfo)) && liteDbAction.ExInfo[liteDbAction.ExInfo.Length - 1] == '-')
                                liteDbAction.ExInfo = liteDbAction.ExInfo.Remove(liteDbAction.ExInfo.Length - 1);
                            foreach (string screenshot in liteDbAction.ScreenShots)
                            {
                                string fileName = Path.GetFileName(screenshot);
                                string newScreenshotPath = Path.Combine(imageFolderPath, fileName);
                                System.IO.File.Copy(screenshot, newScreenshotPath, true); //TODO - Replace with the real location under Ginger installation
                                newScreenShotsList.Add(fileName);
                            }
                            liteDbAction.ScreenShots = newScreenShotsList;
                        }
                    }

                }
            }
        }

    }

}
