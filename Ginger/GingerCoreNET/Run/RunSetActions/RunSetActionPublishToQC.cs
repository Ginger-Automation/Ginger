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


namespace Ginger.Run.RunSetActions
{
    //Name of the class should be RunSetActionPublishToQC 
    //If we change the name, run set xml fails to find it because it look for name RunSetActionPublishToQC
    public class RunSetActionPublishToQC : RunSetActionBase
    {
        public new static class Fields
        {
            public static string VariableForTCRunName = "VariableForTCRunName";
            public static string isVariableInTCRunUsed = "isVariableInTCRunUsed";
            public static string toAttachActivitiesGroupReport = "toAttachActivitiesGroupReport";
        }
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
            get { return mFilterStatus; }
            set { mFilterStatus = value; }
        }
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
        }
        public override void Execute(ReportInfo RI)
        {
            string result = string.Empty;
            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            SetExportToALMConfig();
            foreach (BusinessFlowReport BFR in RI.BusinessFlows)
            {
                bfs.Add((BusinessFlow)BFR.GetBusinessFlow());
            }
            
            if (!RepositoryItemHelper.RepositoryItemFactory.ExportBusinessFlowsResultToALM(bfs, result, PublishToALMConfig))
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

        

        public override string Type { get { return "Publish to ALM"; } }
    }
}
