#region License
/*
Copyright © 2014-2025 European Support Limited

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
using GingerCore.Platforms;
using GingerCoreNET.External.ZAP;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;

#nullable enable
namespace Amdocs.Ginger.CoreNET.ActionsLib.UI.Web
{
    public class ActSecurityTesting : Act, IActPluginExecution
    {
        public new static partial class Fields
        {
            public static string ScanType = "ScanType";
            public static string Target = "Target";

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
                    mPlatforms.Add(ePlatformType.WebServices);
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
            [EnumValueDescription("Active")]
            Active
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


        public override eImageType Image { get { return eImageType.Shield; } }


        private ZAPConfiguration zAPConfiguration;
        private ZapProxyService zapProxyService;
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
            zapProxyService = new ZapProxyService();
            try
            {
                
                if (zapProxyService.IsZapRunningAsync())
                {
                    zapProxyService.AddUrlToScanTree(testURL);
                    zapProxyService.PerformActiveScan(testURL);

                    ProcessResultAsync(zapProxyService, this, testURL);
                    if (AlertList != null)
                    {
                        bool isPassed = zapProxyService.EvaluateScanResult(testURL, AlertList);
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

        public void ExecutePassiveZapScan(string testURL)
        {
            bool isPassed = false;
            Status = eRunStatus.Running;
            zapProxyService = new ZapProxyService();
            try
            {
                if (zapProxyService.IsZapRunningAsync())
                {

                    zapProxyService.WaitTillPassiveScanCompleted();
                    ProcessResultAsync(zapProxyService, (Act)this, testURL);
                    isPassed = zapProxyService.EvaluateScanResult(testURL, AlertList);

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
                }
            }
        }

        private void ProcessResultAsync(ZapProxyService zps, Act _act, string testURL)
        {

            string path = String.Empty;

            string solutionDir = WorkSpace.Instance.Solution.Folder;
            string folderPath = Path.Combine(solutionDir, "ExternalConfigurations", "ZAPConfiguration", "Report");
            // Ensure the directory exists
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }
            string DatetimeFormate = DateTime.Now.ToString("ddMMyyyy_HHmmssfff");
            string reportname = $"{_act.ItemName}_SecurityTestingReport_{DatetimeFormate}.html";
            path = $"{folderPath}{Path.DirectorySeparatorChar}{reportname}";

            zps.GenerateZapReport(testURL, folderPath, reportname);
            Act.AddArtifactToAction(Path.GetFileName(path), _act, path);
        }
    }
}
