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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using Ginger.Reports;
using GingerCore;
using GingerCore.ALM;
using amdocs.ginger.GingerCoreNET;
using GingerCore.DataSource;
using Amdocs.Ginger.Common.InterfacesLib;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;
using GingerCore.Activities;
using System.ComponentModel;

namespace Ginger.Run.RunSetActions
{
    //Name of the class should be RunSetActionPublishToQC 
    //If we change the name, run set xml fails to find it because it look for name RunSetActionPublishToQC
    public class RunSetActionPublishToQC : RunSetActionBase
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public new static class Fields
        {
            public static string VariableForTCRunName = "VariableForTCRunName";
            public static string isVariableInTCRunUsed = "isVariableInTCRunUsed";
            public static string toAttachActivitiesGroupReport = "toAttachActivitiesGroupReport";
        }
        public static String AlmTypeDefault = "Default";

        PublishToALMConfig PublishToALMConfig = new PublishToALMConfig();
        readonly ValueExpression mVE = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false);                

        private string mVariableForTCRunName;
        [IsSerializedForLocalRepository]
        public string VariableForTCRunName { get { return mVariableForTCRunName; } set { if (mVariableForTCRunName != value) { mVariableForTCRunName = value; OnPropertyChanged(Fields.VariableForTCRunName); } } }

        private bool mIsVariableInTCRunUsed;
        [IsSerializedForLocalRepository]
        public bool isVariableInTCRunUsed { get { return mIsVariableInTCRunUsed; } set { if (mIsVariableInTCRunUsed != value) { mIsVariableInTCRunUsed = value; OnPropertyChanged(Fields.isVariableInTCRunUsed); } } }

        private bool mtoAttachActivitiesGroupReport;
        [IsSerializedForLocalRepository]
        public bool toAttachActivitiesGroupReport { get { return mtoAttachActivitiesGroupReport; } set { if (mtoAttachActivitiesGroupReport != value) { mtoAttachActivitiesGroupReport = value; OnPropertyChanged(Fields.toAttachActivitiesGroupReport); } } }        

        private FilterByStatus mFilterStatus;
        [IsSerializedForLocalRepository]
        public FilterByStatus FilterStatus
        {
            get 
            { 
                return mFilterStatus; 
            }
            set 
            { 
                mFilterStatus = value; 
            }
        }
        private string mPublishALMType = AlmTypeDefault;
        [IsSerializedForLocalRepository]
        public string PublishALMType 
        {
            get
            {
                return mPublishALMType;
            }
            set
            {
                if (mPublishALMType != value)
                {
                    mPublishALMType = value;
                    OnPropertyChanged(nameof(PublishALMType));
                }
            }
        }

        private eALMTestSetLevel mALMTestSetLevel;
        [IsSerializedForLocalRepository]
        public eALMTestSetLevel ALMTestSetLevel
        {
            get
            {
                return mALMTestSetLevel;
            }
            set
            {
                if (mALMTestSetLevel != value)
                {
                    mALMTestSetLevel = value;
                    OnPropertyChanged(nameof(ALMTestSetLevel));
                    OnPropertyChanged(nameof(PublishToALMConfig.ALMTestSetLevel));
                }
            }
        }
        private eExportType mExportType;
        [IsSerializedForLocalRepository]
        public eExportType ExportType
        {
            get
            {
                return mExportType;
            }
            set
            {
                mExportType = value;
            }
        }
        [IsSerializedForLocalRepository]
        public ObservableList<ExternalItemFieldBase> AlmFields { get; set; }
        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = new List<RunSetActionBase.eRunAt>();
            list.Add(RunSetActionBase.eRunAt.ExecutionEnd);
            list.Add(RunSetActionBase.eRunAt.DuringExecution);
            return list;
        }

        public override bool SupportRunOnConfig
        {
            get { return false; }
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            //Set flag for each BF to execute runset when BF execute finish
            SetExportToALMConfig();
            foreach (GingerRunner GR in WorkSpace.Instance.RunsetExecutor.Runners)
            {
                GR.PublishToALMConfig = PublishToALMConfig;
            }               
        }
        private void SetExportToALMConfig()
        {
            PublishToALMConfig.IsVariableInTCRunUsed = isVariableInTCRunUsed;
            PublishToALMConfig.ToAttachActivitiesGroupReport = toAttachActivitiesGroupReport;
            PublishToALMConfig.VariableForTCRunName = VariableForTCRunName;
            PublishToALMConfig.CalculateTCRunName(mVE);
            PublishToALMConfig.FilterStatus = FilterStatus;
            //check ALM type logic
            if (PublishALMType == AlmTypeDefault)
            {
                //connect as connected till now to whatever default
            }
            else if (PublishALMType != null)
            {
                eALMType almType = (eALMType)Enum.Parse(typeof(eALMType), PublishALMType);
                //connect to the specific ALM type

                PublishToALMConfig.PublishALMType = almType; // bind?
            }

            PublishToALMConfig.ALMTestSetLevel = ALMTestSetLevel; // bind?
            PublishToALMConfig.ExportType = ExportType; // bind?
        }
        public override void Execute(ReportInfo RI)
        {
            string result = string.Empty;
            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            SetExportToALMConfig();
            // ALM Test Set Level: if "Run Set" convert Run Set to Business flow
            if(PublishToALMConfig.ALMTestSetLevel == eALMTestSetLevel.RunSet)
            {
                bfs.Add(ConvertRunSetToBF(RI));
                // Export Type: if eExportType.EntitiesAndResults then export Business Flow to ALM.
                if (PublishToALMConfig.ExportType == eExportType.EntitiesAndResults)
                {
                    if (bfs.Count > 0)
                    {
                        TargetFrameworkHelper.Helper.ExportVirtualBusinessFlowToALM(bfs[0], false, eALMConnectType.Silence);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.WARN, "Export Business Flow to ALM failed while publish results to ALM");
                    }
                }
            }
            else
            {
                foreach (BusinessFlowReport BFR in RI.BusinessFlows)
                {
                    bfs.Add((BusinessFlow)BFR.GetBusinessFlow());
                }
            }
            if (!TargetFrameworkHelper.Helper.ExportBusinessFlowsResultToALM(bfs,ref result, PublishToALMConfig))
            {
                Errors= result;
                Status = eRunSetActionStatus.Failed;
            }
            else
            {
                Status = eRunSetActionStatus.Completed;
            }
        }
        
        public override string GetEditPage()
        {
            //return new ExportResultsToALMConfigPage(this);
            return "ExportResultsToALMConfigPage";
        }

        

        public override string Type { get { return "Publish Execution Results to ALM"; } }
        public BusinessFlow ConvertRunSetToBF(ReportInfo reportInfo)
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
                virtualBF.Status = BusinessFlow.eBusinessFlowStatus.Development;
                virtualBF.RunStatus = runSetExec.RunSetConfig.RunSetExecutionStatus;
                virtualBF.Activities = new ObservableList<Activity>();
                foreach(GingerRunner runSetrunner in runSetExec.Runners)
                {
                    foreach(BusinessFlow runSetBF in runSetrunner.BusinessFlows)
                    {
                        ActivitiesGroup virtualAG = new ActivitiesGroup();
                        virtualAG.Name = runSetBF.Name;
                        virtualAG.Description = runSetBF.Description;
                        if (Enum.IsDefined(typeof(eActivitiesGroupRunStatus), runSetBF.RunStatus.ToString()))
                        {
                            virtualAG.RunStatus = (eActivitiesGroupRunStatus)Enum.Parse(typeof(eActivitiesGroupRunStatus), runSetBF.RunStatus.ToString());
                        }
                        else
                        {
                            virtualAG.RunStatus = eActivitiesGroupRunStatus.NA;
                        }
                        virtualBF.AddActivitiesGroup(virtualAG);
                        foreach(Activity runSetAct in runSetBF.Activities)
                        {
                            virtualBF.AddActivity(runSetAct, virtualAG);
                        }
                    }
                }
                return virtualBF;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to import QC test set and convert it into " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }
    }
}
