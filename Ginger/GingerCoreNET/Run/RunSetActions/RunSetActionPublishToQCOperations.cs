#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Reports;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCoreNET.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Ginger.Run.RunSetActions.RunSetActionBase;
using static GingerCore.ALM.PublishToALMConfig;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Ginger.Run.RunSetActions
{
    //Name of the class should be RunSetActionPublishToQC 
    //If we change the name, run set xml fails to find it because it look for name RunSetActionPublishToQC
    public class RunSetActionPublishToQCOperations : IRunSetActionPublishToQCOperations
    {
        public RunSetActionPublishToQC RunSetActionPublishToQC;
        public RunSetActionPublishToQCOperations(RunSetActionPublishToQC RunSetActionPublishToQC)
        {
            this.RunSetActionPublishToQC = RunSetActionPublishToQC;
            this.RunSetActionPublishToQC.RunSetActionPublishToQCOperations = this;
        }
        PublishToALMConfig PublishToALMConfig = new PublishToALMConfig();
        readonly ValueExpression mVE = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false);

        public void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            //Set flag for each BF to execute runset when BF execute finish
            SetExportToALMConfig();
            foreach (GingerRunner GR in WorkSpace.Instance.RunsetExecutor.Runners)
            {
                ((GingerExecutionEngine)GR.Executor).PublishToALMConfig = PublishToALMConfig;
            }
        }
        private void SetExportToALMConfig()
        {
            PublishToALMConfig.IsVariableInTCRunUsed = RunSetActionPublishToQC.isVariableInTCRunUsed;
            PublishToALMConfig.ToAttachActivitiesGroupReport = RunSetActionPublishToQC.toAttachActivitiesGroupReport;
            PublishToALMConfig.VariableForTCRunName = RunSetActionPublishToQC.VariableForTCRunName;
            PublishToALMConfig.CalculateTCRunName(mVE);
            PublishToALMConfig.FilterStatus = RunSetActionPublishToQC.FilterStatus;
            //check ALM type logic
            if (RunSetActionPublishToQC.PublishALMType.Equals(RunSetActionPublishToQC.AlmTypeDefault,StringComparison.CurrentCultureIgnoreCase))
            {
                //connect as connected till now to whatever default
            }
            else if (RunSetActionPublishToQC.PublishALMType != null)
            {
                eALMType almType = (eALMType)Enum.Parse(typeof(eALMType), RunSetActionPublishToQC.PublishALMType);
                //connect to the specific ALM type
                PublishToALMConfig.PublishALMType = almType;
            }

            PublishToALMConfig.ALMTestSetLevel = RunSetActionPublishToQC.ALMTestSetLevel;
            PublishToALMConfig.ExportType = RunSetActionPublishToQC.ExportType;
            PublishToALMConfig.AlmFields = RunSetActionPublishToQC.AlmFields;
            PublishToALMConfig.TestSetFolderDestination = RunSetActionPublishToQC.TestSetFolderDestination;
            PublishToALMConfig.TestCaseFolderDestination = RunSetActionPublishToQC.TestCaseFolderDestination;
            PublishToALMConfig.IsEntitySearchByName = RunSetActionPublishToQC.SearchALMEntityByName;
        }
        public void Execute(IReportInfo RI)
        {
            string result = string.Empty;
            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            SetExportToALMConfig();
            // ALM Test Set Level: if "Run Set" convert Run Set to Business flow
            if (PublishToALMConfig.ALMTestSetLevel == eALMTestSetLevel.RunSet)
            {
                bfs.Add(ConvertRunSetToBF(RI));
                // Export Type: if eExportType.EntitiesAndResults then export Business Flow to ALM.
                if (PublishToALMConfig.ExportType == eExportType.EntitiesAndResults)
                {
                    if (bfs.Count > 0)
                    {
                        if (!TargetFrameworkHelper.Helper.ExportVirtualBusinessFlowToALM(bfs[0], PublishToALMConfig, false, eALMConnectType.Silence, PublishToALMConfig.TestSetFolderDestination, PublishToALMConfig.TestCaseFolderDestination))
                        {
                            RunSetActionPublishToQC.Errors = result;
                            RunSetActionPublishToQC.Status = eRunSetActionStatus.Failed;
                        }
                        else
                        {
                            RunSetActionPublishToQC.Status = eRunSetActionStatus.Completed;
                        }
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.WARN, "Export Business Flow to ALM failed while publish results to ALM");
                    }
                }
                UpdateAlmIdtoRunset(bfs[0], RI);
            }
            else
            {
                BusinessFlow businessFlow;
                foreach (BusinessFlowReport BFR in ((ReportInfo)RI).BusinessFlows)
                {
                    businessFlow = (BusinessFlow)BFR.GetBusinessFlow();
                    if(businessFlow.RunStatus != eRunStatus.Pending)
                    {
                        bfs.Add(businessFlow);
                    }
                }
            }
            if (!TargetFrameworkHelper.Helper.ExportBusinessFlowsResultToALM(bfs, ref result, PublishToALMConfig))
            {
                RunSetActionPublishToQC.Errors = result;
                RunSetActionPublishToQC.Status = eRunSetActionStatus.Failed;
            }
            else
            {
                RunSetActionPublishToQC.Status = eRunSetActionStatus.Completed;
            }
        }

        private BusinessFlow ConvertRunSetToBF(IReportInfo reportInfo)
        {
            RunsetExecutor runSetExec = WorkSpace.Instance.RunsetExecutor;
            try
            {
                if (reportInfo == null)
                {
                    return null;
                }

                //Create Business Flow
                BusinessFlow virtualBF = new BusinessFlow();
                virtualBF.Name = runSetExec.RunSetConfig.Name;
                virtualBF.Description = runSetExec.RunSetConfig.Description;
                virtualBF.Status = BusinessFlow.eBusinessFlowStatus.Unknown;
                virtualBF.RunStatus = runSetExec.RunSetConfig.RunSetExecutionStatus;
                virtualBF.StartTimeStamp = runSetExec.RunSetConfig.StartTimeStamp;
                virtualBF.EndTimeStamp = runSetExec.RunSetConfig.EndTimeStamp;
                virtualBF.ALMTestSetLevel = PublishToALMConfig.ALMTestSetLevel.ToString();
                if (!string.IsNullOrEmpty(runSetExec.RunSetConfig.ExternalID))
                {
                    virtualBF.ExternalID = runSetExec.RunSetConfig.ExternalID;
                }
                else
                {
                    virtualBF.ExternalID = string.Empty;
                }

                virtualBF.Activities = new ObservableList<Activity>();
                foreach (GingerRunner runSetrunner in runSetExec.Runners)
                {
                    // if executor is null when run if from file
                    if (runSetrunner.Executor is null)
                    {
                        runSetrunner.Executor = new GingerExecutionEngine(runSetrunner);
                    }
                    foreach (BusinessFlow runSetBF in runSetrunner.Executor.BusinessFlows)
                    {
                        ActivitiesGroup virtualAG = new ActivitiesGroup();
                        virtualAG.Name = runSetBF.Name;
                        virtualAG.Description = runSetBF.Description;
                        ProjEnvironment projEnvironment = runSetExec.RunsetExecutionEnvironment;
                        if (projEnvironment != null)
                        {
                            IValueExpression magVE = new GingerCore.ValueExpression(projEnvironment, runSetBF, new ObservableList<GingerCore.DataSource.DataSourceBase>(), false, "", false);
                            runSetBF.CalculateExternalId(magVE);
                        }
                        virtualAG.ExternalID = !string.IsNullOrEmpty(runSetBF.ExternalIdCalCulated) ? runSetBF.ExternalIdCalCulated : string.Empty;
                        virtualAG.ParentGuid = runSetBF.Guid;//Business flow instance Guid passing in Parent Guid to Update External Id back
                        virtualAG.StartTimeStamp = runSetBF.StartTimeStamp;
                        virtualAG.EndTimeStamp = runSetBF.EndTimeStamp;
                        if (Enum.IsDefined(typeof(eActivitiesGroupRunStatus), runSetBF.RunStatus.ToString()))
                        {
                            virtualAG.RunStatus = (eActivitiesGroupRunStatus)Enum.Parse(typeof(eActivitiesGroupRunStatus), runSetBF.RunStatus.ToString());
                        }
                        else
                        {
                            virtualAG.RunStatus = eActivitiesGroupRunStatus.NA;
                        }
                        virtualBF.AddActivitiesGroup(virtualAG);
                        foreach (Activity runSetAct in runSetBF.Activities)
                        {
                            Activity activitycopy = (Activity)runSetAct.CreateCopy(false);
                            activitycopy.Status = runSetAct.Status;
                            virtualBF.AddActivity(activitycopy, virtualAG, -1, false);
                        }
                    }
                }
                return virtualBF;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert Run Set to BF for ALM Export", ex);
                return null;
            }
        }

        private void UpdateAlmIdtoRunset(BusinessFlow businessFlow, IReportInfo reportInfo)
        {
            RunsetExecutor runSetExec = WorkSpace.Instance.RunsetExecutor;
            try
            {
                if (reportInfo == null)
                {
                    return;
                }
                if (businessFlow != null && !string.IsNullOrEmpty(businessFlow.ExternalID))
                {
                    if(!General.isVariableUsed(runSetExec.RunSetConfig.ExternalID))
                    {
                        runSetExec.RunSetConfig.ExternalID = businessFlow.ExternalID;
                    }                
                }

                foreach (GingerRunner runSetrunner in runSetExec.Runners)
                {
                    // if executor is null when run if from file
                    if (runSetrunner.Executor is null)
                    {
                        runSetrunner.Executor = new GingerExecutionEngine(runSetrunner);
                    }

                    ObservableList<BusinessFlow> Bflist = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

                    foreach (BusinessFlow bFlow in Bflist)
                    {
                        ActivitiesGroup activitiesGroup = businessFlow.ActivitiesGroups.FirstOrDefault(x => x.ParentGuid == bFlow.Guid);
                        if(activitiesGroup != null)
                        {
                            if(!General.isVariableUsed(bFlow.ExternalID))
                            {
                                bFlow.ExternalID = activitiesGroup.ExternalID;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to update ALM ID in the runset", ex);
            }
        }
    }
}
