﻿#region License
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
using static Ginger.Run.RunSetActions.RunSetActionBase;

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
        }
        public void Execute(IReportInfo RI)
        {
            string result = string.Empty;
            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            SetExportToALMConfig();
            foreach (BusinessFlowReport BFR in ((ReportInfo)RI).BusinessFlows)
            {
                bfs.Add((BusinessFlow)BFR.GetBusinessFlow());
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

    }
}
