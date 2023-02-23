#region License
/*
Copyright © 2014-2023 European Support Limited

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

using AccountReport.Contracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger
{
    public static class AccountReportEntitiesDataMapping
    {
        //select template 
        static HTMLReportConfiguration _HTMLReportConfig = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>().Where(x => x.IsDefault).FirstOrDefault();
        static string _InProgressStatus = "In Progress";

        public static AccountReportAction MapActionStartData(GingerCore.Actions.Act action, Context context)
        {
            action.ExecutionId = Guid.NewGuid(); // check incase of retry / flow control             

            AccountReportAction accountReportAction = new AccountReportAction();

            if (context.BusinessFlow.CurrentActivity != null)
            {
                accountReportAction.Seq = context.BusinessFlow.CurrentActivity.ExecutionLogActionCounter;
                action.ParentExecutionId = context.BusinessFlow.CurrentActivity.ExecutionId;
            }
            else
            {
                accountReportAction.Seq = 1;
            }
            accountReportAction.Id = action.ExecutionId;
            accountReportAction.EntityId = action.Guid;
            accountReportAction.AccountReportDbActivityId = action.ParentExecutionId;
            accountReportAction.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportAction.Name = action.Description;            
            accountReportAction.ActionType = action.ActionType;
            accountReportAction.Description = action.Description;
            accountReportAction.RunDescription = GetCalculatedValue(context, action.RunDescription);    //must pass also BF to VE
            accountReportAction.Environment = ((GingerExecutionEngine)context.Runner).GingerRunner.ProjEnvironment.Name;
            accountReportAction.EnvironmentId = ((GingerExecutionEngine)context.Runner).GingerRunner.ProjEnvironment.Guid;
            accountReportAction.StartTimeStamp = action.StartTimeStamp;
            accountReportAction.InputValues = GetInputValues(action);
            accountReportAction.CurrentRetryIteration = action.RetryMechanismCount;
            accountReportAction.Wait = Convert.ToInt32(action.Wait);
            accountReportAction.TimeOut = action.Timeout;
            accountReportAction.RunStatus = _InProgressStatus;
            
            return accountReportAction;
        }
        public static AccountReportAction MapActionEndData(GingerCore.Actions.Act action, Context context)
        {
            AccountReportAction accountReportAction = new AccountReportAction();
            List<string> newScreenShotsList = new List<string>();
            accountReportAction.Id = action.ExecutionId;
            accountReportAction.EntityId = action.Guid;
            accountReportAction.AccountReportDbActivityId = action.ParentExecutionId;
            accountReportAction.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportAction.Name = action.Description;
            accountReportAction.EndTimeStamp = action.EndTimeStamp;
            accountReportAction.ElapsedEndTimeStamp = action.Elapsed;
            accountReportAction.RunStatus = action.Status.ToString();
            accountReportAction.OutputValues = action.ReturnValues.Select(a => a.Param + "_:_" + a.Actual + "_:_" + a.ExpectedCalculated + "_:_" + a.Status).ToList();
            accountReportAction.FlowControls = action.FlowControls.Select(a => a.Condition + "_:_" + a.ConditionCalculated + "_:_" + a.FlowControlAction + "_:_" + a.Status).ToList();
            accountReportAction.Error = action.Error;
            accountReportAction.ExInfo = action.ExInfo;
            foreach (string screenshot in action.ScreenShots)
            {
                string newScreenshotPath = WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID.ToString() + "/" + Path.GetFileName(screenshot);
                newScreenShotsList.Add(newScreenshotPath);
            }
            accountReportAction.ScreenShots = newScreenShotsList;

            return accountReportAction;
        }
        public static AccountReportActivity MapActivityStartData(Activity activity, Context context)
        {
            activity.ExecutionId = Guid.NewGuid();// check incase of rerun / flow control 
            activity.ParentExecutionId = context.BusinessFlow.CurrentActivitiesGroup.ExecutionId;

            AccountReportActivity accountReportActivity = new AccountReportActivity();
            accountReportActivity.Id = activity.ExecutionId;
            accountReportActivity.EntityId = activity.Guid;
            accountReportActivity.AccountReportDbActivityGroupId = activity.ParentExecutionId;
            accountReportActivity.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportActivity.Seq = context.BusinessFlow.ExecutionLogActivityCounter;
            accountReportActivity.Name = activity.ActivityName;
            accountReportActivity.Description = activity.Description;
            accountReportActivity.RunDescription = GetCalculatedValue(context, activity.RunDescription);
            accountReportActivity.Environment = ((GingerExecutionEngine)context.Runner).GingerRunner.ProjEnvironment.Name;
            accountReportActivity.EnvironmentId = ((GingerExecutionEngine)context.Runner).GingerRunner.ProjEnvironment.Guid;
            accountReportActivity.StartTimeStamp = activity.StartTimeStamp;
            accountReportActivity.VariablesBeforeExec = activity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description + "_:_" + a.Guid + "_:_" + a.SetAsInputValue + "_:_" + a.SetAsOutputValue + "_:_" + a.Publish).ToList();
            accountReportActivity.ActivityGroupName = activity.ActivitiesGroupID;
            accountReportActivity.ChildExecutableItemsCount = activity.Acts.Count(x => x.Active);
            accountReportActivity.RunStatus = _InProgressStatus;
            accountReportActivity.IsPublished = activity.Publish;
            return accountReportActivity;
        }
        public static AccountReportActivity MapActivityEndData(Activity activity, Context context)
        {
            AccountReportActivity accountReportActivity = new AccountReportActivity();
            accountReportActivity.Id = activity.ExecutionId;
            accountReportActivity.EntityId = activity.Guid;
            accountReportActivity.AccountReportDbActivityGroupId = activity.ParentExecutionId;
            accountReportActivity.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportActivity.Name = activity.ActivityName;
            accountReportActivity.EndTimeStamp = activity.EndTimeStamp;
            accountReportActivity.ElapsedEndTimeStamp = activity.Elapsed;
            accountReportActivity.RunStatus = activity.Status.ToString();
            accountReportActivity.VariablesAfterExec = activity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description + "_:_" + a.Guid + "_:_" + a.SetAsInputValue + "_:_" + a.SetAsOutputValue + "_:_" + a.Publish).ToList();

            accountReportActivity.ChildExecutedItemsCount = activity.Acts.Count(x => x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Stopped || x.Status == eRunStatus.Completed);
            accountReportActivity.ChildPassedItemsCount = activity.Acts.Count(x => x.Status == eRunStatus.Passed);
            accountReportActivity.ChildExecutableItemsCount = activity.Acts.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Blocked));

            accountReportActivity.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate((int)accountReportActivity.ChildExecutedItemsCount, (int)accountReportActivity.ChildExecutableItemsCount));
            accountReportActivity.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate((int)accountReportActivity.ChildPassedItemsCount, (int)accountReportActivity.ChildExecutableItemsCount));
            return accountReportActivity;
        }

        public static AccountReportActivityGroup MapActivityGroupStartData(ActivitiesGroup activitiesGroup, Context context)
        {
            activitiesGroup.ExecutionId = Guid.NewGuid();
            activitiesGroup.ParentExecutionId = context.BusinessFlow.ExecutionId;

            AccountReportActivityGroup accountReportActivityGroup = new AccountReportActivityGroup();
            accountReportActivityGroup.Id = activitiesGroup.ExecutionId;
            accountReportActivityGroup.EntityId = activitiesGroup.Guid;
            accountReportActivityGroup.AccountReportDbBusinessFlowId = activitiesGroup.ParentExecutionId;
            accountReportActivityGroup.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportActivityGroup.Seq = context.BusinessFlow.ActivitiesGroups.IndexOf(activitiesGroup) + 1;// context.BusinessFlow.ExecutionLogActivityGroupCounter;            
            accountReportActivityGroup.Name = activitiesGroup.Name;
            accountReportActivityGroup.Description = activitiesGroup.Description;
            accountReportActivityGroup.Environment = ((GingerExecutionEngine)context.Runner).GingerRunner.ProjEnvironment.Name;
            accountReportActivityGroup.EnvironmentId = ((GingerExecutionEngine)context.Runner).GingerRunner.ProjEnvironment.Guid;
            accountReportActivityGroup.AutomationPrecentage = activitiesGroup.AutomationPrecentage;
            accountReportActivityGroup.StartTimeStamp = activitiesGroup.StartTimeStamp;
            accountReportActivityGroup.ExecutedActivitiesGUID = activitiesGroup.ExecutedActivities.Select(x => x.Key).ToList();
            accountReportActivityGroup.RunStatus = _InProgressStatus;
            accountReportActivityGroup.IsPublished = activitiesGroup.Publish;
            return accountReportActivityGroup;
        }

        public static AccountReportActivityGroup MapActivityGroupEndData(ActivitiesGroup activitiesGroup, Context context)
        {
            AccountReportActivityGroup accountReportActivityGroup = new AccountReportActivityGroup();
            accountReportActivityGroup.Id = activitiesGroup.ExecutionId;
            accountReportActivityGroup.EntityId = activitiesGroup.Guid;
            accountReportActivityGroup.AccountReportDbBusinessFlowId = activitiesGroup.ParentExecutionId;
            accountReportActivityGroup.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportActivityGroup.Name = activitiesGroup.Name;
            accountReportActivityGroup.EndTimeStamp = activitiesGroup.EndTimeStamp;
            accountReportActivityGroup.ElapsedEndTimeStamp = activitiesGroup.Elapsed;
            accountReportActivityGroup.RunStatus = activitiesGroup.RunStatus.ToString();
            return accountReportActivityGroup;
        }

        public static AccountReportBusinessFlow MapBusinessFlowStartData(BusinessFlow businessFlow, Context context)
        {
            businessFlow.ExecutionId = Guid.NewGuid();
            businessFlow.ParentExecutionId = context.Runner.ExecutionId;

            AccountReportBusinessFlow accountReportBusinessFlow = new AccountReportBusinessFlow();
            accountReportBusinessFlow.Id = businessFlow.ExecutionId;
            accountReportBusinessFlow.EntityId = businessFlow.Guid;
            accountReportBusinessFlow.AccountReportDbRunnerId = businessFlow.ParentExecutionId;
            accountReportBusinessFlow.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportBusinessFlow.Seq = context.Runner.ExecutionLogBusinessFlowsCounter + 1;
            accountReportBusinessFlow.InstanceGUID = businessFlow.InstanceGuid;
            accountReportBusinessFlow.Name = businessFlow.Name;
            accountReportBusinessFlow.Description = businessFlow.Description;
            accountReportBusinessFlow.RunDescription = GetCalculatedValue(context, businessFlow.RunDescription);
            accountReportBusinessFlow.Environment = ((GingerExecutionEngine)context.Runner).GingerRunner.ProjEnvironment.Name;
            accountReportBusinessFlow.EnvironmentId = ((GingerExecutionEngine)context.Runner).GingerRunner.ProjEnvironment.Guid;
            accountReportBusinessFlow.StartTimeStamp = businessFlow.StartTimeStamp;
            accountReportBusinessFlow.VariablesBeforeExec = businessFlow.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description + "_:_" + a.Guid + "_:_" + a.SetAsInputValue + "_:_" + a.SetAsOutputValue + "_:_" + a.Publish).ToList();
            accountReportBusinessFlow.SolutionVariablesBeforeExec = businessFlow.GetSolutionVariables().Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
            accountReportBusinessFlow.RunStatus = _InProgressStatus;
            accountReportBusinessFlow.IsPublished = businessFlow.Publish;
            int ChildExecutableItemsCountAction = 0;
            string Actions = HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString();
            string Actvities = HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString();
            foreach (Activity activity in businessFlow.Activities)
            {
                ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + activity.Acts.Count(x => x.Active);
            }
            accountReportBusinessFlow.ChildExecutableItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportBusinessFlow.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = businessFlow.Activities.Count(x => x.Active) });

            accountReportBusinessFlow.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutableItemsCountAction });
            return accountReportBusinessFlow;
        }

        public static AccountReportBusinessFlow MapBusinessFlowEndData(BusinessFlow businessFlow, Context context)
        {
            AccountReportBusinessFlow accountReportBusinessFlow = new AccountReportBusinessFlow();
            accountReportBusinessFlow.Id = businessFlow.ExecutionId;
            accountReportBusinessFlow.EntityId = businessFlow.Guid;
            accountReportBusinessFlow.AccountReportDbRunnerId = businessFlow.ParentExecutionId;
            accountReportBusinessFlow.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportBusinessFlow.Name = businessFlow.Name;
            accountReportBusinessFlow.EndTimeStamp = businessFlow.EndTimeStamp;
            accountReportBusinessFlow.ElapsedEndTimeStamp = businessFlow.Elapsed;
            accountReportBusinessFlow.RunStatus = businessFlow.RunStatus.ToString();
            accountReportBusinessFlow.VariablesAfterExec = businessFlow.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description + "_:_" + a.Guid + "_:_" + a.SetAsInputValue + "_:_" + a.SetAsOutputValue + "_:_" + a.Publish).ToList();
            accountReportBusinessFlow.SolutionVariablesAfterExec = businessFlow.GetSolutionVariables().Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList(); ;
            accountReportBusinessFlow.BFFlowControlDT = businessFlow.BFFlowControls.Select(a => a.Condition + "_:_" + a.ConditionCalculated + "_:_" + a.BusinessFlowControlAction + "_:_" + a.Status).ToList(); ;
            accountReportBusinessFlow.AutomationPercent = businessFlow.AutomationPrecentage;
            int ChildExecutableItemsCountAction = 0;
            int ChildExecutedItemsCountAction = 0;
            int ChildPassedItemsCountAction = 0;
            foreach (Activity activity in businessFlow.Activities)
            {
                ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + activity.Acts.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Blocked));
                ChildExecutedItemsCountAction = ChildExecutedItemsCountAction + activity.Acts.Count(x => x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Stopped || x.Status == eRunStatus.Completed);
                ChildPassedItemsCountAction = ChildPassedItemsCountAction + activity.Acts.Count(x => x.Status == eRunStatus.Passed);
            }
            string Actvities = HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString();
            string Actions = HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString();

            accountReportBusinessFlow.ChildExecutableItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportBusinessFlow.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = businessFlow.Activities.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Blocked)) });

            accountReportBusinessFlow.ChildExecutedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportBusinessFlow.ChildExecutedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = businessFlow.Activities.Count(ac => ac.Status == eRunStatus.Failed || ac.Status == eRunStatus.Passed || ac.Status == eRunStatus.FailIgnored || ac.Status == eRunStatus.Stopped || ac.Status == eRunStatus.Completed) });

            accountReportBusinessFlow.ChildPassedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportBusinessFlow.ChildPassedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = businessFlow.Activities.Count(ac => ac.Status == eRunStatus.Passed) });


            accountReportBusinessFlow.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutableItemsCountAction });

            accountReportBusinessFlow.ChildExecutedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutedItemsCountAction });

            accountReportBusinessFlow.ChildPassedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildPassedItemsCountAction });

            accountReportBusinessFlow.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportBusinessFlow.ChildExecutedItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value, accountReportBusinessFlow.ChildExecutableItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value));

            accountReportBusinessFlow.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportBusinessFlow.ChildPassedItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value, accountReportBusinessFlow.ChildExecutableItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value));
            return accountReportBusinessFlow;
        }

        public static AccountReportRunner MapRunnerStartData(GingerRunner gingerRunner, Context context)
        {
            gingerRunner.Executor.ExecutionId = Guid.NewGuid();
            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID != null)
            {
                gingerRunner.Executor.ParentExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            }
            AccountReportRunner accountReportRunner = new AccountReportRunner();
            accountReportRunner.Id = gingerRunner.Executor.ExecutionId;
            accountReportRunner.EntityId = gingerRunner.Guid;
            accountReportRunner.AccountReportDbRunSetId = gingerRunner.Executor.ParentExecutionId;
            accountReportRunner.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportRunner.Seq = gingerRunner.Executor.ExecutionLoggerManager.GingerData.Seq;
            accountReportRunner.Name = gingerRunner.Name;
            //accountReportRunner.Description = gingerRunner.Description;
            accountReportRunner.Environment = gingerRunner.ProjEnvironment.Name.ToString();
            accountReportRunner.EnvironmentId = gingerRunner.ProjEnvironment.Guid;
            accountReportRunner.StartTimeStamp = gingerRunner.Executor.StartTimeStamp;
            accountReportRunner.ApplicationAgentsMappingList = gingerRunner.ApplicationAgents.Select(a => a.AgentName + "_:_" + a.AppName).ToList();
            SetRunnerChildCounts((GingerExecutionEngine)gingerRunner.Executor, accountReportRunner, true);
            accountReportRunner.RunStatus = _InProgressStatus;
            accountReportRunner.IsPublished = gingerRunner.Publish;
            return accountReportRunner;
        }

        public static AccountReportRunner MapRunnerEndData(GingerRunner gingerRunner, Context context)
        {
            AccountReportRunner accountReportRunner = new AccountReportRunner();
            accountReportRunner.Id = gingerRunner.Executor.ExecutionId;
            accountReportRunner.Name = gingerRunner.Name;
            accountReportRunner.EntityId = gingerRunner.Guid;
            accountReportRunner.AccountReportDbRunSetId = gingerRunner.Executor.ParentExecutionId;
            accountReportRunner.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportRunner.Name = gingerRunner.Name;
            accountReportRunner.ElapsedEndTimeStamp = gingerRunner.Executor.Elapsed;
            accountReportRunner.EndTimeStamp = gingerRunner.Executor.EndTimeStamp;
            //accountReportRunner.RunStatus = gingerRunner.Status.ToString();//SetStatus(BusinessFlowsColl); // check if need to calculate based on businessflows status data
            accountReportRunner.RunStatus = GetRunnerStatus((GingerExecutionEngine)gingerRunner.Executor).ToString();
            SetRunnerChildCounts((GingerExecutionEngine)gingerRunner.Executor, accountReportRunner);

            accountReportRunner.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunner.ChildExecutedItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value, accountReportRunner.ChildExecutableItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value));

            accountReportRunner.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunner.ChildPassedItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value, accountReportRunner.ChildExecutableItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value));
            return accountReportRunner;
        }

        public static AccountReportRunSet MapRunsetStartData(RunSetConfig runSetConfig, Context context)
        {

            AccountReportRunSet accountReportRunSet = new AccountReportRunSet();
            accountReportRunSet.Id = (Guid)runSetConfig.ExecutionID;
            accountReportRunSet.ExecutionId = (Guid)runSetConfig.ExecutionID;
            accountReportRunSet.EntityId = runSetConfig.Guid;
            accountReportRunSet.GingerSolutionGuid = WorkSpace.Instance.Solution.Guid;
            accountReportRunSet.Seq = 1;
            accountReportRunSet.Name = runSetConfig.Name;
            accountReportRunSet.Description = runSetConfig.Description;
            accountReportRunSet.Environment = runSetConfig.GingerRunners[0].ProjEnvironment.ToString();
            accountReportRunSet.EnvironmentId = runSetConfig.GingerRunners[0].ProjEnvironment.Guid;
            accountReportRunSet.StartTimeStamp = runSetConfig.StartTimeStamp;
            accountReportRunSet.MachineName = System.Environment.MachineName.ToString();
            accountReportRunSet.ExecutedByUser = System.Environment.UserName.ToString();
            accountReportRunSet.GingerVersion = ApplicationInfo.ApplicationVersion;
            accountReportRunSet.Account = WorkSpace.Instance.Solution.Account;
            accountReportRunSet.Product = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.Where(x => x.Category == SolutionCategory.eSolutionCategories.Product).FirstOrDefault());
            accountReportRunSet.Release = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.Where(x => x.Category == SolutionCategory.eSolutionCategories.Release).FirstOrDefault());
            accountReportRunSet.Iteration = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.Where(x => x.Category == SolutionCategory.eSolutionCategories.Iteration).FirstOrDefault());
            accountReportRunSet.TestType = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.Where(x => x.Category == SolutionCategory.eSolutionCategories.TestType).FirstOrDefault());
            accountReportRunSet.UserCategory1 = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.Where(x => x.Category == SolutionCategory.eSolutionCategories.UserCategory1).FirstOrDefault());
            accountReportRunSet.UserCategory2 = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.Where(x => x.Category == SolutionCategory.eSolutionCategories.UserCategory2).FirstOrDefault());
            accountReportRunSet.UserCategory3 = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.Where(x => x.Category == SolutionCategory.eSolutionCategories.UserCategory3).FirstOrDefault());
            accountReportRunSet.RunStatus = _InProgressStatus;
            accountReportRunSet.IsPublished = runSetConfig.Publish;
            SetRunSetChildCounts(runSetConfig, accountReportRunSet, true);
            return accountReportRunSet;
        }

        public static AccountReportRunSet MapRunsetEndData(RunSetConfig runSetConfig, Context context)
        {
            AccountReportRunSet accountReportRunSet = new AccountReportRunSet();
            accountReportRunSet.Id = (Guid)runSetConfig.ExecutionID;
            accountReportRunSet.ExecutionId = (Guid)runSetConfig.ExecutionID;
            accountReportRunSet.EntityId = runSetConfig.Guid;
            accountReportRunSet.GingerSolutionGuid = WorkSpace.Instance.Solution.Guid;
            accountReportRunSet.Name = runSetConfig.Name;
            accountReportRunSet.ElapsedEndTimeStamp = runSetConfig.Elapsed;
            accountReportRunSet.EndTimeStamp = runSetConfig.EndTimeStamp;
            //Calculate at runset end
            accountReportRunSet.RunStatus = (runSetConfig.RunSetExecutionStatus == eRunStatus.Automated)
                ? eRunStatus.Automated.ToString() : runSetConfig.RunSetExecutionStatus.ToString();
            SetRunSetChildCounts(runSetConfig, accountReportRunSet);
            accountReportRunSet.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunSet.ChildExecutedItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value, accountReportRunSet.ChildExecutableItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value));

            accountReportRunSet.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunSet.ChildPassedItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value, accountReportRunSet.ChildExecutableItemsCount[(int)_HTMLReportConfig.ExecutionStatisticsCountBy].Value));
            return accountReportRunSet;
        }

        public static Amdocs.Ginger.CoreNET.Execution.eRunStatus GetRunnerStatus(GingerExecutionEngine gingerRunner)
        {

            if (gingerRunner.BusinessFlows != null && gingerRunner.BusinessFlows.Count > 0)
            {
                if ((from x in gingerRunner.BusinessFlows.ToList() where x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed select x).Count() > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                }
                else if ((from x in gingerRunner.BusinessFlows.ToList() where x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked select x).Count() > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                }
                else if ((from x in gingerRunner.BusinessFlows.ToList() where x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped select x).Count() > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                }
                else if ((from x in gingerRunner.BusinessFlows.ToList()
                          where (x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed ||
                            x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped)
                          select x).Count() == gingerRunner.BusinessFlows.Count)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                }
                else
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                }
            }
            else if (gingerRunner != null)
            {
                return gingerRunner.Status;
            }
            else
            {
                return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
            }
        }

        private static void SetRunnerChildCounts(GingerExecutionEngine runner, AccountReportRunner accountReportRunner, bool IsStart = false)
        {
            int ChildExecutableItemsCountActivity = 0;
            int ChildExecutedItemsCountActivity = 0;
            int ChildPassedItemsCountActivity = 0;
            int ChildExecutableItemsCountAction = 0;
            int ChildExecutedItemsCountAction = 0;
            int ChildPassedItemsCountAction = 0;
            string Actvities = HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString();
            string Actions = HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString();

            foreach (BusinessFlow businessFlow in runner.BusinessFlows)
            {
                if(IsStart)
                {
                    ChildExecutableItemsCountActivity = ChildExecutableItemsCountActivity + businessFlow.Activities.Count(x=> x.Active);
                }
                else
                {
                    ChildExecutableItemsCountActivity = ChildExecutableItemsCountActivity + businessFlow.Activities.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Failed || x.Status == eRunStatus.Blocked));
                }                

                ChildExecutedItemsCountActivity = ChildExecutedItemsCountActivity + businessFlow.Activities.Count(ac => ac.Status == eRunStatus.Failed || ac.Status == eRunStatus.Passed || ac.Status == eRunStatus.FailIgnored || ac.Status == eRunStatus.Stopped || ac.Status == eRunStatus.Completed);

                ChildPassedItemsCountActivity = ChildPassedItemsCountActivity + businessFlow.Activities.Count(ac => ac.Status == eRunStatus.Passed);

                foreach (Activity activity in businessFlow.Activities)
                {
                    if(IsStart)
                    {
                        ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + activity.Acts.Count(x => x.Active);
                    }
                    else
                    {
                        ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + activity.Acts.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Failed || x.Status == eRunStatus.Blocked));
                    }                    

                    ChildExecutedItemsCountAction = ChildExecutedItemsCountAction + activity.Acts.Count(x => x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Stopped || x.Status == eRunStatus.Completed);

                    ChildPassedItemsCountAction = activity.Acts.Count(x => x.Status == eRunStatus.Passed);
                }
            }
            accountReportRunner.ChildExecutableItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunner.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = ChildExecutableItemsCountActivity });

            accountReportRunner.ChildExecutedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunner.ChildExecutedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = ChildExecutedItemsCountActivity });

            accountReportRunner.ChildPassedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunner.ChildPassedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = ChildPassedItemsCountActivity });

            accountReportRunner.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutableItemsCountAction });

            accountReportRunner.ChildExecutedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutedItemsCountAction });

            accountReportRunner.ChildPassedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildPassedItemsCountAction });
        }

        private static void SetRunSetChildCounts(RunSetConfig runSet, AccountReportRunSet accountReportRunSet, bool IsStart = false)
        {
            int ChildExecutableItemsCountActivity = 0;
            int ChildExecutedItemsCountActivity = 0;
            int ChildPassedItemsCountActivity = 0;
            int ChildExecutableItemsCountAction = 0;
            int ChildExecutedItemsCountAction = 0;
            int ChildPassedItemsCountAction = 0;
            string Actvities = HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString();
            string Actions = HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString();
            foreach (GingerRunner runner in runSet.GingerRunners)
            {
                foreach (BusinessFlow businessFlow in runner.Executor.BusinessFlows)
                {
                    if(IsStart)
                    {
                        ChildExecutableItemsCountActivity = ChildExecutableItemsCountActivity + businessFlow.Activities.Count(x => x.Active);
                    }
                    else
                    {
                        ChildExecutableItemsCountActivity = ChildExecutableItemsCountActivity + businessFlow.Activities.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Failed || x.Status == eRunStatus.Blocked));
                    }                    

                    ChildExecutedItemsCountActivity = ChildExecutedItemsCountActivity + businessFlow.Activities.Count(ac => ac.Status == eRunStatus.Failed || ac.Status == eRunStatus.Passed || ac.Status == eRunStatus.FailIgnored || ac.Status == eRunStatus.Stopped || ac.Status == eRunStatus.Completed);

                    ChildPassedItemsCountActivity = ChildPassedItemsCountActivity + businessFlow.Activities.Count(ac => ac.Status == eRunStatus.Passed);

                    foreach (Activity activity in businessFlow.Activities)
                    {
                        if (IsStart)
                        {
                            ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + activity.Acts.Count(x => x.Active);
                        }
                        else
                        {
                            ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + activity.Acts.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Failed || x.Status == eRunStatus.Blocked));
                        }

                        ChildExecutedItemsCountAction = ChildExecutedItemsCountAction + activity.Acts.Count(x => x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Stopped || x.Status == eRunStatus.Completed);

                        ChildPassedItemsCountAction = ChildPassedItemsCountAction + activity.Acts.Count(x => x.Status == eRunStatus.Passed);
                    }
                }
            }
            accountReportRunSet.ChildExecutableItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunSet.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = ChildExecutableItemsCountActivity });

            accountReportRunSet.ChildExecutedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunSet.ChildExecutedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = ChildExecutedItemsCountActivity });

            accountReportRunSet.ChildPassedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunSet.ChildPassedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = ChildPassedItemsCountActivity });

            accountReportRunSet.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutableItemsCountAction });

            accountReportRunSet.ChildExecutedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutedItemsCountAction });

            accountReportRunSet.ChildPassedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildPassedItemsCountAction });
        }


        private static string CalculateExecutionOrPassRate(int firstItem, int secondItem)
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

        private static string GetCalculatedValue(Context context, string stringToCalculate)
        {
            IValueExpression mVE = new GingerCore.ValueExpression(context.Environment, context.BusinessFlow, new ObservableList<GingerCore.DataSource.DataSourceBase>(), false, "", false);
            mVE.Value = stringToCalculate;
            return mVE.ValueCalculated;
        }

        public static List<string> GetInputValues(Act mAction)
        {
            List<string> inputValues = new List<string>();
            inputValues = mAction.InputValues.Select(a => OverrideHTMLRelatedCharacters(a.Param + "_:_" + a.Value + "_:_" + a.ValueForDriver)).ToList();

            if ((mAction.GetInputValueListForVEProcessing() != null) && (mAction.GetInputValueListForVEProcessing().Count > 0))
            {
                mAction.GetInputValueListForVEProcessing().ForEach(x => x.Select(a => OverrideHTMLRelatedCharacters(a.Param + "_:_" + a.Value + "_:_" + a.ValueForDriver)).ToList().ForEach(z => inputValues.Add(z)));
            }
            return inputValues;
        }

        public static string OverrideHTMLRelatedCharacters(string text)
        {
            try
            {
                text = text.Replace(@"<", "&#60;");
                text = text.Replace(@">", "&#62;");
                text = text.Replace(@"$", "&#36;");
                text = text.Replace(@"%", "&#37;");
                return text;
            }
            catch
            {
                return text;
            }
        }
    }
}
