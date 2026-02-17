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
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using GingerCore.Platforms;
using GingerCoreNET.External.ZAP;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.ActionsLib.UI.Web
{
    public class ActSecurityTesting : Act, IActPluginExecution
    {
        public new static partial class Fields
        {
            public static string ScanType = "ScanType";
        }
        public override String ActionType
        {
            get
            {
                return "Security Testing";
            }
        }
        public override string ActionDescription { get { return "Security Testing"; } }

        public override string ActionUserDescription { get { return "Security Testing"; } }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

        public override bool ValueConfigsNeeded { get { return false; } }

        // Public property to set the type of rules to fetch
        public string CurrentRuleType { get; set; } = ePlatformType.Web.ToString(); // Default to "Web"

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                }
                return mPlatforms;
            }
        }

        public override string ActionEditPage { get { return "ActSecurityTestingEditPage"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Action to Test Security Testing on Web/API");
            TBH.AddLineBreak();
            TBH.AddText("Security testing action is using Zap Proxy tool");
            TBH.AddLineBreak();
            TBH.AddText("Security testing is the practice of making your web and API find vulnerabilities");
        }

        public PlatformAction GetAsPlatformAction()
        {
            PlatformAction platformAction = new PlatformAction(this);

            foreach (ActInputValue aiv in this.InputValues)
            {
                if (!platformAction.InputParams.ContainsKey(aiv.Param))
                {
                    platformAction.InputParams.Add(aiv.Param, aiv.ValueForDriver);
                }
            }
            return platformAction;
        }

        public string GetName()
        {
            return "Security Testing";
        }

        public enum eScanType
        {
            [EnumValueDescription("Perform Active Scan")]
            Active,
            [EnumValueDescription("Pull Passive Scan Report")]
            Passive
        }

        public enum eAlertTypes
        {
            [EnumValueDescription("High")]
            High,
            [EnumValueDescription("Medium")]
            Medium,
            [EnumValueDescription("Low")]
            Low,
            [EnumValueDescription("Informational")]
            Informational,
            [EnumValueDescription("False Positive")]
            FalsePositive,
        }
        private Dictionary<string, object> mAlertItems;

        public Dictionary<string, object> AlertItems
        {
            get
            {
                return mAlertItems;
            }
            set
            {
                mAlertItems = value;

            }
        }

        private ObservableList<OperationValues> mAlertList;
        [IsSerializedForLocalRepository]
        public ObservableList<OperationValues> AlertList
        {
            get
            {
                return mAlertList;
            }
            set
            {
                if (value != mAlertList)
                {
                    mAlertList = value;
                    OnPropertyChanged(nameof(AlertList));
                }
            }
        }
        eScanType mScanType;
        [IsSerializedForLocalRepository]
        public eScanType ScanType
        {
            get { return mScanType; }
            set
            {
                if (mScanType != value)
                {
                    mScanType = value;
                    OnPropertyChanged(nameof(ScanType));
                }
            }
        }

        private bool mPullReport;
        [IsSerializedForLocalRepository]
        public bool PullReport
        {
            get { return mPullReport; }
            set
            {
                if (mPullReport != value)
                {
                    mPullReport = value;
                    OnPropertyChanged(nameof(PullReport));
                }
            }
        }

        public override eImageType Image { get { return eImageType.Shield; } }


        private ZAPConfiguration zAPConfiguration;
        public ZAPConfiguration ZAPConfiguration
        {
            get
            {
                if (zAPConfiguration == null)
                {
                    zAPConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ZAPConfiguration>().Count == 0 ? new ZAPConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<ZAPConfiguration>();
                }
                return zAPConfiguration;
            }
            set
            {
                zAPConfiguration = value;
            }
        }
        //method to execute the scan
        public void ExecuteActiveZapScan(string testURL)
        {
            Status = eRunStatus.Running;
            ZapProxyService zapProxyService = new ZapProxyService();
            try
            {

                if (zapProxyService.IsZapRunning())
                {
                    zapProxyService.AddUrlToScanTree(testURL);
                    zapProxyService.PerformActiveScan(testURL);

                    ProcessResultAsync(zapProxyService, this, testURL);
                    AddZapAlertOutputValues(zapProxyService, this, testURL);
                    if (AlertList != null)
                    {
                        bool isPassed = zapProxyService.EvaluateScanResultWeb(testURL, AlertList);
                        if (isPassed)
                        {
                            Status = eRunStatus.Passed;
                            return;

                        }
                        else
                        {
                            Status = eRunStatus.Failed;
                            Error = "Vulnerability Issues Found, Please check the report in Output Values Artifact ";
                            return;
                        }
                    }

                    if (zapProxyService.EvaluateScanResult(testURL))
                    {
                        Status = eRunStatus.Passed;
                        return;
                    }
                    else
                    {
                        Status = eRunStatus.Failed;
                        Error = "Vulnerability Issues Found, Please check the report in Output Values Artifact ";
                        return;
                    }
                }
                else
                {
                    Status = eRunStatus.Failed;
                    Error = "ZAP Proxy is not running. Please start ZAP Proxy before executing the scan.";
                    return;
                }
            }
            catch (Exception ex)
            {
                Status = eRunStatus.Failed;
                Error = $"Error while executing ZAP scan: {ex.Message}";
                return;
            }

        }

        public void ExecutePassiveZapScan(string testURL, Act act)
        {
            bool isPassed = false;
            Status = eRunStatus.Running;
            ZapProxyService zapProxyService = new ZapProxyService();
            try
            {
                if (zapProxyService.IsZapRunning())
                {

                    zapProxyService.WaitTillPassiveScanCompleted();

                    ProcessResultAsync(zapProxyService, act, testURL);
                    AddZapAlertOutputValues(zapProxyService, act, testURL);
                    isPassed = zapProxyService.EvaluateScanResultWeb(testURL, AlertList);

                }
                else
                {
                    Status = eRunStatus.Failed;
                    Error = "ZAP Proxy is not running. Please start ZAP Proxy before executing the scan.";

                }
            }
            catch (Exception ex)
            {
                Status = eRunStatus.Failed;
                Error = $"Error while executing ZAP scan: {ex.Message}";
            }
            finally
            {
                if (isPassed)
                {
                    Status = eRunStatus.Passed;

                }
                else
                {
                    Status = eRunStatus.Failed;
                    Error = "Security testing Failed with some alerts .";
                    act.ExInfo = "Vulnerability Issues Found, Please check the report in Output Values Artifact ";
                }
            }
        }

        /// <summary>
        /// Since We are calling this method from Agent, giving the null optional to ActSecurity,
        /// need to change if calling from BF in future.
        /// this is for getting the alert list use set at agent level
        /// </summary>
        /// <param name="apiEndpointURL"></param>
        /// <param name="act"></param>
        public void ExecuteApiSecurityTesting(string apiEndpointURL, Act act, bool failAction = false)
        {
            Status = eRunStatus.Running;
            ZapProxyService zapProxyService = new ZapProxyService();
            try
            {
                if (zapProxyService.IsZapRunning())
                {
                    if (!string.IsNullOrEmpty(apiEndpointURL))
                    {

                        zapProxyService.ActiveScanAPI(apiEndpointURL);
                    }


                    ProcessResultAsync(zapProxyService, act, apiEndpointURL);
                    AddZapAlertOutputValues(zapProxyService, act, apiEndpointURL);
                    bool isPassed = zapProxyService.EvaluateScanResultAPI(apiEndpointURL, AlertList);

                    if (failAction)
                    {
                        act.Status = isPassed ? eRunStatus.Passed : eRunStatus.Failed;
                    }
                    if (!isPassed)
                    {
                        act.Error = "Vulnerability Issues Found, Please check the report in Output Values Artifact ";
                        act.ExInfo = "Vulnerability Issues Found, Please check the report in Output Values Artifact ";
                    }

                    if (act is ActWebAPIBase)
                    {

                        var alertSummary = zapProxyService.GetAlertSummary(apiEndpointURL);

                        int totalAlerts = alertSummary.Sum(a => a.Count);
                        if (totalAlerts > 0)
                        {
                            act.ExInfo = "Vulnerability Issues Found, Please check the report in Output Values Artifact ";
                        }
                    }

                }
                else
                {
                    Status = eRunStatus.Failed;
                    Error = "ZAP Proxy is not running. Please start ZAP Proxy before executing the scan.";
                }
            }
            catch (Exception ex)
            {
                Status = eRunStatus.Failed;
                Error = $"Error while executing ZAP API security scan: {ex.Message}";
            }
        }

        private void ProcessResultAsync(ZapProxyService zps, Act _act, string testURL)
        {

            string path = String.Empty;

            string solutionDir = WorkSpace.Instance.Solution.Folder;
            string folderPath = Path.Combine(solutionDir, "ExecutionResults", "Artifacts", "ZAPReport");
            // Ensure the directory exists
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }
            string DatetimeFormate = DateTime.Now.ToString("ddMMyyyy_HHmmssfff");
            string reportname = $"{_act.ItemName}_SecurityTestingReport_{DatetimeFormate}.html";
            path = Path.Combine(folderPath, reportname);

            zps.GenerateZapReport(testURL, folderPath, reportname);
            Act.AddArtifactToAction(Path.GetFileName(path), _act, path);
            _act.AddOrUpdateReturnParamActual(ParamName: "Security Testing Report", ActualValue: path);
        }

        private static void AddZapAlertOutputValues(ZapProxyService zapProxyService, Act act, string testURL)
        {
            var alertSummary = zapProxyService.GetAlertSummary(testURL);

            int totalAlerts = alertSummary.Sum(a => a.Count);

            // Build the summary string: "High=2,Medium=3,Low=2,FalsePositive=3,Informational=2"
            string vulnerabilitiesSummary = string.Join(
                ",",
                alertSummary.Select(a => $"{a.AlertName}={a.Count}")
            );
            // Add a single output value row for Vulnerabilities Summary
            act.AddNewReturnParams = true;

            act.AddOrUpdateReturnParamActualWithPath(
                "Vulnerabilities Summary",
                vulnerabilitiesSummary,
                totalAlerts.ToString()
            );
        }
    }
}
