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

using AccountReport.Contracts;
using AccountReport.Contracts.Enum;
using AccountReport.Contracts.RequestModels;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using static Ginger.Reports.ExecutionLoggerConfiguration;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger
{
    public static class AccountReportEntitiesDataMapping
    {
        //select template 
        static HTMLReportConfiguration _HTMLReportConfig = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>().FirstOrDefault(x => x.IsDefault);      
        public static List<AccountReportStatistics>  accountReportStatistics= new List<AccountReportStatistics>();
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
            accountReportAction.RunStatus = eExecutionStatus.InProgress;

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
            accountReportAction.RunStatus = (eExecutionStatus)action.Status;
            accountReportAction.OutputValues = action.ReturnValues.Select(a => a.Param + "_:_" + a.Actual + "_:_" + a.ExpectedCalculated + "_:_" + a.Status + "_:_" + a.Description).ToList();
            accountReportAction.FlowControls = action.FlowControls.Select(a => a.Condition + "_:_" + a.ConditionCalculated + "_:_" + a.FlowControlAction + "_:_" + a.Status).ToList();
            accountReportAction.Error = action.Error;
            accountReportAction.ExInfo = action.ExInfo;
            accountReportAction.ExternalID = action.ExternalID;
            accountReportAction.ExternalID2 = action.ExternalID2;
            if(action.ParentGuid != Guid.Empty)
            {
                accountReportAction.ParentID = action.ParentGuid;
            }
            foreach (string screenshot in action.ScreenShots)
            {
                string newScreenshotPath = WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID.ToString() + "/" + Path.GetFileName(screenshot);
                newScreenShotsList.Add(newScreenshotPath);
            }
            accountReportAction.ScreenShots = newScreenShotsList;
            
            accountReportAction.Artifacts = new List<AccountReport.Contracts.Helpers.DictObject>();

            if(WorkSpace.Instance.Solution.LoggerConfigurations.UploadArtifactsToCentralizedReport == eUploadExecutionArtifactsToCentralizedReport.Yes)
            {
                string basePath = WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID.ToString() + "/";
                foreach (ArtifactDetails artifact in action.Artifacts)
                {
                    string newArtifactPath = basePath +  Path.GetFileName(artifact.ArtifactReportStoragePath);                    
                    accountReportAction.Artifacts.Add(new AccountReport.Contracts.Helpers.DictObject
                    { Key = artifact.ArtifactOriginalName, Value = newArtifactPath });
                }
            }
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
            accountReportActivity.Seq = context.BusinessFlow.ExecutionLogActivityCounter -1;
            accountReportActivity.Name = activity.ActivityName;
            accountReportActivity.Description = activity.Description;
            accountReportActivity.RunDescription = GetCalculatedValue(context, activity.RunDescription);
            accountReportActivity.Environment = ((GingerExecutionEngine)context.Runner).GingerRunner.ProjEnvironment.Name;
            accountReportActivity.EnvironmentId = ((GingerExecutionEngine)context.Runner).GingerRunner.ProjEnvironment.Guid;
            accountReportActivity.StartTimeStamp = activity.StartTimeStamp;
            accountReportActivity.VariablesBeforeExec = activity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description + "_:_" + a.Guid + "_:_" + a.SetAsInputValue + "_:_" + a.SetAsOutputValue + "_:_" + a.Publish).ToList();
            accountReportActivity.ActivityGroupName = activity.ActivitiesGroupID;
            accountReportActivity.ChildsExecutionStatistics = new Dictionary<AccountReport.Contracts.Enum.eEntityType, StatisticsBase>();
            StatisticsBase actionStatisticsBase = new StatisticsBase();
            actionStatisticsBase.TotalExecutable = activity.Acts.Count(x => x.Active);
            accountReportActivity.ChildsExecutionStatistics.Add(AccountReport.Contracts.Enum.eEntityType.Action, actionStatisticsBase);                  
            accountReportActivity.RunStatus = eExecutionStatus.InProgress;
            accountReportActivity.IsPublished = activity.Publish;
            accountReportActivity.ExternalID = activity.ExternalID;
            accountReportActivity.ExternalID2 = activity.ExternalID2;            
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
            accountReportActivity.RunStatus = (eExecutionStatus)activity.Status;
            accountReportActivity.ExternalID = GetCalculatedValue(context, activity.ExternalID);
            accountReportActivity.ExternalID2 = activity.ExternalID2;
            accountReportActivity.VariablesAfterExec = activity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description + "_:_" + a.Guid + "_:_" + a.SetAsInputValue + "_:_" + a.SetAsOutputValue + "_:_" + a.Publish).ToList();

            accountReportActivity.ChildsExecutionStatistics = new Dictionary<AccountReport.Contracts.Enum.eEntityType, StatisticsBase>();
            StatisticsBase actionStatisticsBase = new StatisticsBase();
            actionStatisticsBase.StatusCount = new Dictionary<eExecutionStatus, int>();

            actionStatisticsBase.TotalExecuted = activity.Acts.Count(x => x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Stopped || x.Status == eRunStatus.Completed);
            actionStatisticsBase.TotalPassed = activity.Acts.Count(x => x.Status == eRunStatus.Passed);
            actionStatisticsBase.TotalExecutable = activity.Acts.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Blocked));

            var actionsStat = (from a in activity.Acts.GroupBy(x => x.Status)
                              select new KeyValuePair<eRunStatus, int>((eRunStatus)a.First().Status, a.Count())).ToList();
            foreach (var astat in actionsStat)
            {
                actionStatisticsBase.StatusCount[(eExecutionStatus)astat.Key] = astat.Value;
            }

            accountReportActivity.ChildsExecutionStatistics.Add(eEntityType.Action, actionStatisticsBase);

            accountReportStatistics.Add(new AccountReportStatistics()
            {
                ChildsExecutionStatistics = accountReportActivity.ChildsExecutionStatistics,
                EntityId = activity.Guid,
                Type = activity.GetType().Name
            });
            accountReportActivity.ExternalID = activity.ExternalID;
            accountReportActivity.ExternalID2 = activity.ExternalID2;
            if(activity.ParentGuid != Guid.Empty)
            {
                accountReportActivity.ParentID = activity.ParentGuid;
            }
            accountReportActivity.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate((int)actionStatisticsBase.TotalExecuted, (int)actionStatisticsBase.TotalExecutable));
            accountReportActivity.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate((int)actionStatisticsBase.TotalPassed, (int)actionStatisticsBase.TotalExecutable));
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
            accountReportActivityGroup.RunStatus = eExecutionStatus.InProgress;
            accountReportActivityGroup.IsPublished = activitiesGroup.Publish;
            accountReportActivityGroup.ExternalID = GetCalculatedValue(context, activitiesGroup.ExternalID);
            accountReportActivityGroup.ExternalID2 = activitiesGroup.ExternalID2;
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
            accountReportActivityGroup.RunStatus = (eExecutionStatus)activitiesGroup.RunStatus;
            accountReportActivityGroup.ExternalID = GetCalculatedValue(context, activitiesGroup.ExternalID);
            accountReportActivityGroup.ExternalID2 = activitiesGroup.ExternalID2;
            if(activitiesGroup.ParentGuid != Guid.Empty)
            {
                accountReportActivityGroup.ParentID = activitiesGroup.ParentGuid;
            }
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
            accountReportBusinessFlow.RunStatus = eExecutionStatus.InProgress;
            accountReportBusinessFlow.IsPublished = businessFlow.Publish;
            accountReportBusinessFlow.ExternalID = GetCalculatedValue(context, businessFlow.ExternalID);
            accountReportBusinessFlow.ExternalID2 = businessFlow.ExternalID2;
       
            int ChildExecutableItemsCountAction = 0;
            foreach (Activity activity in businessFlow.Activities)
            {
                ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + activity.Acts.Count(x => x.Active);
            }
            accountReportBusinessFlow.ChildsExecutionStatistics = new Dictionary<eEntityType, StatisticsBase>();
            StatisticsBase actionStatisticsBase = new StatisticsBase();
            StatisticsBase activityStatisticsBase = new StatisticsBase();

            actionStatisticsBase.TotalExecutable = ChildExecutableItemsCountAction;
            activityStatisticsBase.TotalExecutable = businessFlow.Activities.Count(x => x.Active);

            accountReportBusinessFlow.ChildsExecutionStatistics.Add(eEntityType.Action, actionStatisticsBase);
            accountReportBusinessFlow.ChildsExecutionStatistics.Add(eEntityType.Activity, activityStatisticsBase);

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
            accountReportBusinessFlow.RunStatus = (eExecutionStatus)businessFlow.RunStatus;
            accountReportBusinessFlow.VariablesAfterExec = businessFlow.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description + "_:_" + a.Guid + "_:_" + a.SetAsInputValue + "_:_" + a.SetAsOutputValue + "_:_" + a.Publish).ToList();
            accountReportBusinessFlow.SolutionVariablesAfterExec = businessFlow.GetSolutionVariables().Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList(); ;
            accountReportBusinessFlow.BFFlowControlDT = businessFlow.BFFlowControls.Select(a => a.Condition + "_:_" + a.ConditionCalculated + "_:_" + a.BusinessFlowControlAction + "_:_" + a.Status).ToList(); ;
            accountReportBusinessFlow.AutomationPercent = businessFlow.AutomationPrecentage;
            accountReportBusinessFlow.ExternalID = GetCalculatedValue(context, businessFlow.ExternalID);
            accountReportBusinessFlow.ExternalID2 = businessFlow.ExternalID2;
            if(businessFlow.ParentGuid != Guid.Empty)
            {
                accountReportBusinessFlow.ParentID = businessFlow.ParentGuid;
            }
            accountReportBusinessFlow.ChildsExecutionStatistics = new Dictionary<eEntityType, StatisticsBase>();
            StatisticsBase actionStatisticsBase = new StatisticsBase();
            StatisticsBase activityStatisticsBase = new StatisticsBase();
            actionStatisticsBase.StatusCount = new Dictionary<eExecutionStatus, int>();
            activityStatisticsBase.StatusCount = new Dictionary<eExecutionStatus, int>();
            

            var activitiesStat = (from a in businessFlow.Activities.GroupBy(x => x.Status)
                                select new KeyValuePair<eRunStatus, int>((eRunStatus)a.First().Status, a.Count())).ToList();
           
            foreach (var astat in activitiesStat)
            {
                activityStatisticsBase.StatusCount[(eExecutionStatus)astat.Key] = astat.Value;
            }

            activityStatisticsBase.TotalExecuted = businessFlow.Activities.Count(ac => ac.Status == eRunStatus.Failed || ac.Status == eRunStatus.Passed || ac.Status == eRunStatus.FailIgnored || ac.Status == eRunStatus.Stopped || ac.Status == eRunStatus.Completed);
            activityStatisticsBase.TotalPassed = businessFlow.Activities.Count(ac => ac.Status == eRunStatus.Passed);
            activityStatisticsBase.TotalExecutable = businessFlow.Activities.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Blocked));

            foreach (Activity activity in businessFlow.Activities)
            {
                if(activity.Active)
                {                    
                    var stat = accountReportStatistics.FirstOrDefault(x=> x.EntityId == activity.Guid);
                    if (stat!=null && stat.ChildsExecutionStatistics!=null)
                    {
                        StatisticsBase historyActionStatisticsBase = new StatisticsBase();
                        stat.ChildsExecutionStatistics.TryGetValue(eEntityType.Action, out historyActionStatisticsBase);
                        actionStatisticsBase.TotalExecutable =
                          actionStatisticsBase.TotalExecutable + historyActionStatisticsBase.TotalExecutable;

                        actionStatisticsBase.TotalExecuted =
                            actionStatisticsBase.TotalExecuted + historyActionStatisticsBase.TotalExecuted;

                        actionStatisticsBase.TotalPassed =
                           actionStatisticsBase.TotalPassed + historyActionStatisticsBase.TotalPassed;

                        foreach (var dicObj in historyActionStatisticsBase.StatusCount)
                        {
                            if (actionStatisticsBase.StatusCount.ContainsKey(dicObj.Key))
                            {
                                actionStatisticsBase.StatusCount[dicObj.Key] = actionStatisticsBase.StatusCount[dicObj.Key] + dicObj.Value;
                            }
                            else
                            {
                                actionStatisticsBase.StatusCount[dicObj.Key] = dicObj.Value;
                            }
                        }  
                    }
                }
            }

            accountReportBusinessFlow.ChildsExecutionStatistics.Add(eEntityType.Action, actionStatisticsBase);
            accountReportBusinessFlow.ChildsExecutionStatistics.Add(eEntityType.Activity, activityStatisticsBase);

            accountReportStatistics.Add(new AccountReportStatistics()
            {
                ChildsExecutionStatistics = accountReportBusinessFlow.ChildsExecutionStatistics,
                EntityId = businessFlow.Guid,
                Type = businessFlow.GetType().Name
            });

            if((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 1)
            {
                accountReportBusinessFlow.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(activityStatisticsBase.TotalExecuted, activityStatisticsBase.TotalExecutable));

                accountReportBusinessFlow.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(activityStatisticsBase.TotalPassed, activityStatisticsBase.TotalExecutable));
            }
            else if ((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 0)
            {
                accountReportBusinessFlow.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(actionStatisticsBase.TotalExecuted, actionStatisticsBase.TotalExecutable));

                accountReportBusinessFlow.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(actionStatisticsBase.TotalPassed, actionStatisticsBase.TotalExecutable));
            }
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
            SetRunnerChildCountsAtStart(gingerRunner, accountReportRunner);
            accountReportRunner.RunStatus = eExecutionStatus.InProgress;
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
            accountReportRunner.ExternalID = gingerRunner.ExternalID;
            accountReportRunner.ExternalID2 = gingerRunner.ExternalID2;
            if(gingerRunner.ParentGuid != Guid.Empty)
            {
                accountReportRunner.ParentID = gingerRunner.ParentGuid;
            }
            //accountReportRunner.RunStatus = gingerRunner.Status.ToString();//SetStatus(BusinessFlowsColl); // check if need to calculate based on businessflows status data
            accountReportRunner.RunStatus = (eExecutionStatus)GetRunnerStatus((GingerExecutionEngine)gingerRunner.Executor);
            SetRunnerChildCountsAtEnd(gingerRunner, accountReportRunner);

            StatisticsBase activityStatisticsBase = new StatisticsBase();
            accountReportRunner.ChildsExecutionStatistics.TryGetValue(eEntityType.Activity, out activityStatisticsBase);

            StatisticsBase actionStatisticsBase = new StatisticsBase();
            accountReportRunner.ChildsExecutionStatistics.TryGetValue(eEntityType.Action, out actionStatisticsBase);

            if ((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 1)
            {
                accountReportRunner.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(activityStatisticsBase.TotalExecuted, activityStatisticsBase.TotalExecutable));

                accountReportRunner.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(activityStatisticsBase.TotalPassed, activityStatisticsBase.TotalExecutable));
            }
            else if ((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 0)
            {
                accountReportRunner.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(actionStatisticsBase.TotalExecuted, actionStatisticsBase.TotalExecutable));

                accountReportRunner.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(actionStatisticsBase.TotalPassed, actionStatisticsBase.TotalExecutable));
            }           
            return accountReportRunner;
        }

        public static AccountReportRunSet MapRunsetStartData(RunSetConfig runSetConfig, Context context)
        {            
            GingerCore.ValueExpression valueExpression = new(context.Environment, context, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>());

            AccountReportRunSet accountReportRunSet = new AccountReportRunSet();

            //updating source application and user
            accountReportRunSet.SourceApplication = runSetConfig.SourceApplication;
            accountReportRunSet.SourceApplicationUser = runSetConfig.SourceApplicationUser;
            
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
            accountReportRunSet.GingerVersion = ApplicationInfo.ApplicationUIversion;
            accountReportRunSet.Account = WorkSpace.Instance.Solution.Account;
            accountReportRunSet.Product = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.FirstOrDefault(x => x.Category == SolutionCategory.eSolutionCategories.Product));
            accountReportRunSet.Release = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.FirstOrDefault(x => x.Category == SolutionCategory.eSolutionCategories.Release));
            accountReportRunSet.Iteration = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.FirstOrDefault(x => x.Category == SolutionCategory.eSolutionCategories.Iteration));
            accountReportRunSet.TestType = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.FirstOrDefault(x => x.Category == SolutionCategory.eSolutionCategories.TestType));
            accountReportRunSet.UserCategory1 = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.FirstOrDefault(x => x.Category == SolutionCategory.eSolutionCategories.UserCategory1));
            accountReportRunSet.UserCategory2 = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.FirstOrDefault(x => x.Category == SolutionCategory.eSolutionCategories.UserCategory2));
            accountReportRunSet.UserCategory3 = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.FirstOrDefault(x => x.Category == SolutionCategory.eSolutionCategories.UserCategory3));
            accountReportRunSet.BusinessProcessTag = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.FirstOrDefault(x => x.Category == SolutionCategory.eSolutionCategories.BusinessProcessTag));
            accountReportRunSet.SubBusinessProcessTag = GingerCoreNET.GeneralLib.General.GetSolutionCategoryValue(runSetConfig.CategoriesDefinitions.FirstOrDefault(x => x.Category == SolutionCategory.eSolutionCategories.SubBusinessProcessTag));
            accountReportRunSet.RunStatus = eExecutionStatus.InProgress;           
            valueExpression.Value = runSetConfig.RunDescription;
            accountReportRunSet.RunDescription = valueExpression.ValueCalculated;
            accountReportRunSet.IsPublished = runSetConfig.Publish;
            accountReportRunSet.ExternalID = runSetConfig.ExternalID;            
            SetRunSetChildCountsAtStart(runSetConfig, accountReportRunSet);            
            return accountReportRunSet;
        }

        public static AccountReportRunSet MapRunsetEndData(RunSetConfig runSetConfig)
        {
            AccountReportRunSet accountReportRunSet = new AccountReportRunSet();
            accountReportRunSet.Id = (Guid)runSetConfig.ExecutionID;
            accountReportRunSet.ExecutionId = (Guid)runSetConfig.ExecutionID;
            accountReportRunSet.EntityId = runSetConfig.Guid;
            accountReportRunSet.GingerSolutionGuid = WorkSpace.Instance.Solution.Guid;
            accountReportRunSet.Name = runSetConfig.Name;
            accountReportRunSet.ElapsedEndTimeStamp = runSetConfig.Elapsed;
            accountReportRunSet.EndTimeStamp = runSetConfig.EndTimeStamp;
            accountReportRunSet.ExternalID = runSetConfig.ExternalID;
            accountReportRunSet.ExternalID2 = runSetConfig.ExternalID2;
            if (runSetConfig.ParentGuid != Guid.Empty)
            {
                accountReportRunSet.ParentID = runSetConfig.ParentGuid;
            }
            //Calculate at runset end
            accountReportRunSet.RunStatus = (runSetConfig.RunSetExecutionStatus == eRunStatus.Automated)
                ? eExecutionStatus.Automated : (eExecutionStatus)runSetConfig.RunSetExecutionStatus;
            SetRunSetChildCountsAtEnd(runSetConfig, accountReportRunSet);

            StatisticsBase activityStatisticsBase = new StatisticsBase();
            accountReportRunSet.ChildsExecutionStatistics.TryGetValue(eEntityType.Activity, out activityStatisticsBase);

            StatisticsBase actionStatisticsBase = new StatisticsBase();
            accountReportRunSet.ChildsExecutionStatistics.TryGetValue(eEntityType.Action, out actionStatisticsBase);

            if ((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 1)
            {
                accountReportRunSet.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(activityStatisticsBase.TotalExecuted, activityStatisticsBase.TotalExecutable));

                accountReportRunSet.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(activityStatisticsBase.TotalPassed, activityStatisticsBase.TotalExecutable));
            }
            else if ((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 0)
            {
                accountReportRunSet.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(actionStatisticsBase.TotalExecuted, actionStatisticsBase.TotalExecutable));

                accountReportRunSet.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(actionStatisticsBase.TotalPassed, actionStatisticsBase.TotalExecutable));
            }
            return accountReportRunSet;
        }

        public static Amdocs.Ginger.CoreNET.Execution.eRunStatus GetRunnerStatus(GingerExecutionEngine gingerRunner)
        {

            if (gingerRunner.BusinessFlows != null && gingerRunner.BusinessFlows.Count > 0)
            {
                if (gingerRunner.BusinessFlows.Any(x=> x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed))
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                }
                else if (gingerRunner.BusinessFlows.Any(x => x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked))
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                }
                else if (gingerRunner.BusinessFlows.Any(x => x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped))
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                }
                else if (gingerRunner.BusinessFlows.Count(x=> x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed ||
                            x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped)== gingerRunner.BusinessFlows.Count)
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

        private static void SetRunnerChildCountsAtStart(GingerRunner gingerRunner, AccountReportRunner accountReportRunner)
        {
            accountReportRunner.ChildsExecutionStatistics = new Dictionary<eEntityType, StatisticsBase>();

            StatisticsBase actionStatisticsBase = new StatisticsBase();
            StatisticsBase activityStatisticsBase = new StatisticsBase();
            StatisticsBase businessFlowStatisticsBase = new StatisticsBase();

            businessFlowStatisticsBase.TotalExecutable = ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows.Count(bf => bf.Active);

            foreach (BusinessFlow businessFlow in ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows)
            {
                if (businessFlow.Active)
                {
                    activityStatisticsBase.TotalExecutable =
                               activityStatisticsBase.TotalExecutable + businessFlow.Activities.Count(ac=>ac.Active);
                                                        
                    foreach (Activity activity in businessFlow.Activities)
                    {
                        if (activity.Active)
                        {
                            actionStatisticsBase.TotalExecutable =
                                 actionStatisticsBase.TotalExecutable + activity.Acts.Count(a=>a.Active);                                                             
                        }
                    }
                }
            }

            accountReportRunner.ChildsExecutionStatistics.Add(eEntityType.Action, actionStatisticsBase);
            accountReportRunner.ChildsExecutionStatistics.Add(eEntityType.Activity, activityStatisticsBase);
            accountReportRunner.ChildsExecutionStatistics.Add(eEntityType.BusinessFlow, businessFlowStatisticsBase);            
        }
        private static void SetRunSetChildCountsAtStart(RunSetConfig runSet, AccountReportRunSet accountReportRunSet)
        {
            accountReportRunSet.ChildsExecutionStatistics = new Dictionary<eEntityType, StatisticsBase>();
            StatisticsBase actionStatisticsBase = new StatisticsBase();
            StatisticsBase activityStatisticsBase = new StatisticsBase();
            StatisticsBase businessFlowStatisticsBase = new StatisticsBase();
            StatisticsBase runnerStatisticsBase = new StatisticsBase();

            runnerStatisticsBase.TotalExecutable = runSet.GingerRunners.Count(r => r.Active);

            foreach (GingerRunner runner in runSet.GingerRunners)
            {
                if (runner.Active)
                {
                    businessFlowStatisticsBase.TotalExecutable =
                    businessFlowStatisticsBase.TotalExecutable + ((GingerExecutionEngine)runner.Executor).BusinessFlows.Count(bf => bf.Active);
                    foreach (BusinessFlow businessFlow in runner.Executor.BusinessFlows)
                    {
                        if (businessFlow.Active)
                        {
                            activityStatisticsBase.TotalExecutable =
                                   activityStatisticsBase.TotalExecutable + businessFlow.Activities.Count(ac => ac.Active);

                            foreach (Activity activity in businessFlow.Activities)
                            {
                                if (activity.Active)
                                {
                                    actionStatisticsBase.TotalExecutable =
                                     actionStatisticsBase.TotalExecutable + activity.Acts.Count(a => a.Active);
                                }
                            }
                        }
                    }
                }
            }

            accountReportRunSet.ChildsExecutionStatistics.Add(eEntityType.Action, actionStatisticsBase);
            accountReportRunSet.ChildsExecutionStatistics.Add(eEntityType.Activity, activityStatisticsBase);
            accountReportRunSet.ChildsExecutionStatistics.Add(eEntityType.BusinessFlow, businessFlowStatisticsBase);
            accountReportRunSet.ChildsExecutionStatistics.Add(eEntityType.Runner, runnerStatisticsBase);
        }

        private static void SetRunnerChildCountsAtEnd(GingerRunner gingerRunner, AccountReportRunner accountReportRunner)
        {
            accountReportRunner.ChildsExecutionStatistics = new Dictionary<eEntityType, StatisticsBase>();
            StatisticsBase actionStatisticsBase = new StatisticsBase();
            StatisticsBase activityStatisticsBase = new StatisticsBase();
            StatisticsBase businessFlowStatisticsBase = new StatisticsBase();

            actionStatisticsBase.StatusCount = new Dictionary<eExecutionStatus, int>();
            activityStatisticsBase.StatusCount = new Dictionary<eExecutionStatus, int>();
            businessFlowStatisticsBase.StatusCount = new Dictionary<eExecutionStatus, int>();

            var businessFlowStat = (from a in ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows.GroupBy(x => x.RunStatus)
                                    select new KeyValuePair<eRunStatus, int>((eRunStatus)a.First().RunStatus, a.Count())).ToList();

            foreach (var bstat in businessFlowStat)
            {
                businessFlowStatisticsBase.StatusCount[(eExecutionStatus)bstat.Key] = bstat.Value;
            }
            businessFlowStatisticsBase.TotalExecuted = ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows.Count(bf => bf.RunStatus == eRunStatus.Failed || bf.RunStatus == eRunStatus.Passed || bf.RunStatus == eRunStatus.FailIgnored || bf.RunStatus == eRunStatus.Stopped || bf.RunStatus == eRunStatus.Completed);
            businessFlowStatisticsBase.TotalPassed = ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows.Count(bf => bf.RunStatus == eRunStatus.Passed);
            businessFlowStatisticsBase.TotalExecutable = ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows.Count(bf => bf.Active && (bf.RunStatus == eRunStatus.Passed || bf.RunStatus == eRunStatus.Failed || bf.RunStatus == eRunStatus.FailIgnored || bf.RunStatus == eRunStatus.Blocked));
                        

            foreach (BusinessFlow businessFlow in ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows)
            {
                if (businessFlow.Active)
                {                    
                    var bfStat = accountReportStatistics.FirstOrDefault(x => x.EntityId == businessFlow.Guid);
                    if (bfStat!=null && bfStat.ChildsExecutionStatistics!=null)
                    {
                        StatisticsBase historyActivityStatisticsBase = new StatisticsBase();
                        bfStat.ChildsExecutionStatistics.TryGetValue(eEntityType.Activity, out historyActivityStatisticsBase);

                        activityStatisticsBase.TotalExecutable =
                                   activityStatisticsBase.TotalExecutable + historyActivityStatisticsBase.TotalExecutable;

                        activityStatisticsBase.TotalExecuted =
                                   activityStatisticsBase.TotalExecuted + historyActivityStatisticsBase.TotalExecuted;

                        activityStatisticsBase.TotalPassed =
                               activityStatisticsBase.TotalPassed + historyActivityStatisticsBase.TotalPassed;

                            foreach (var dicObj in historyActivityStatisticsBase.StatusCount)
                            {
                                if (activityStatisticsBase.StatusCount.ContainsKey(dicObj.Key))
                                {
                                    activityStatisticsBase.StatusCount[dicObj.Key] = activityStatisticsBase.StatusCount[dicObj.Key] + dicObj.Value;
                                }
                                else
                                {
                                    activityStatisticsBase.StatusCount[dicObj.Key] = dicObj.Value;
                                }
                            }                        
                    }

                    foreach (Activity activity in businessFlow.Activities)
                    {
                        if(activity.Active)
                        {
                            var stat = accountReportStatistics.FirstOrDefault(x => x.EntityId == activity.Guid);
                           
                            if (stat != null && stat.ChildsExecutionStatistics!=null)
                            {
                                StatisticsBase historyActionStatisticsBase = new StatisticsBase();
                                stat.ChildsExecutionStatistics.TryGetValue(eEntityType.Action, out historyActionStatisticsBase);

                                actionStatisticsBase.TotalExecutable =
                                 actionStatisticsBase.TotalExecutable + historyActionStatisticsBase.TotalExecutable;

                                actionStatisticsBase.TotalExecuted =
                                actionStatisticsBase.TotalExecuted + historyActionStatisticsBase.TotalExecuted;

                                actionStatisticsBase.TotalPassed =
                                    actionStatisticsBase.TotalPassed + historyActionStatisticsBase.TotalPassed;

                                foreach (var dicObj in historyActionStatisticsBase.StatusCount)
                                {
                                    if (actionStatisticsBase.StatusCount.ContainsKey(dicObj.Key))
                                    {
                                        actionStatisticsBase.StatusCount[dicObj.Key] = actionStatisticsBase.StatusCount[dicObj.Key] + dicObj.Value;
                                    }
                                    else
                                    {
                                        actionStatisticsBase.StatusCount[dicObj.Key] = dicObj.Value;
                                    }
                                }
                                
                            } 
                        }
                    } 
                }
            }

            accountReportRunner.ChildsExecutionStatistics.Add(eEntityType.Action, actionStatisticsBase);
            accountReportRunner.ChildsExecutionStatistics.Add(eEntityType.Activity, activityStatisticsBase);
            accountReportRunner.ChildsExecutionStatistics.Add(eEntityType.BusinessFlow, businessFlowStatisticsBase);

            accountReportStatistics.Add(new AccountReportStatistics()
            {
                ChildsExecutionStatistics = accountReportRunner.ChildsExecutionStatistics,
                EntityId = gingerRunner.Guid,
                Type = gingerRunner.GetType().Name
            });
            
        }

        private static void SetRunSetChildCountsAtEnd(RunSetConfig runSet, AccountReportRunSet accountReportRunSet)
        {
            accountReportRunSet.ChildsExecutionStatistics = new Dictionary<eEntityType, StatisticsBase>();
            StatisticsBase actionStatisticsBase = new StatisticsBase();
            StatisticsBase activityStatisticsBase = new StatisticsBase();
            StatisticsBase businessFlowStatisticsBase = new StatisticsBase();
            StatisticsBase runnerStatisticsBase = new StatisticsBase();

            actionStatisticsBase.StatusCount = new Dictionary<eExecutionStatus, int>();
            activityStatisticsBase.StatusCount = new Dictionary<eExecutionStatus, int>();
            businessFlowStatisticsBase.StatusCount = new Dictionary<eExecutionStatus, int>();
            runnerStatisticsBase.StatusCount = new Dictionary<eExecutionStatus, int>();

            runnerStatisticsBase.TotalExecuted = runSet.GingerRunners.Count(rs => rs.Status == eRunStatus.Failed || rs.Status == eRunStatus.Passed || rs.Status == eRunStatus.FailIgnored || rs.Status == eRunStatus.Stopped || rs.Status == eRunStatus.Completed);
            runnerStatisticsBase.TotalPassed = runSet.GingerRunners.Count(rs => rs.Status == eRunStatus.Passed);

            runnerStatisticsBase.TotalExecutable = runSet.GingerRunners.Count(rs => rs.Active && (rs.Status == eRunStatus.Passed || rs.Status == eRunStatus.Failed || rs.Status == eRunStatus.FailIgnored || rs.Status == eRunStatus.Blocked));
            
            var runnerStat = (from a in runSet.GingerRunners.GroupBy(x => x.Status)
                                select new KeyValuePair<eRunStatus, int>((eRunStatus)a.First().Status, a.Count())).ToList();

            foreach (var rstat in runnerStat)
            {
                runnerStatisticsBase.StatusCount[(eExecutionStatus)rstat.Key] = rstat.Value;
            }
                                   
            foreach (GingerRunner runner in runSet.GingerRunners)
            {
                if (runner.Active)
                {
                    var rStat = accountReportStatistics.FirstOrDefault(x => x.EntityId == runner.Guid);
                    if (rStat != null && rStat.ChildsExecutionStatistics != null)
                    {
                        StatisticsBase historyBusinessflowStatisticsBase = new StatisticsBase();
                        rStat.ChildsExecutionStatistics.TryGetValue(eEntityType.BusinessFlow, out historyBusinessflowStatisticsBase);

                        businessFlowStatisticsBase.TotalExecutable =
                           businessFlowStatisticsBase.TotalExecutable + historyBusinessflowStatisticsBase.TotalExecutable;

                        businessFlowStatisticsBase.TotalExecuted =
                            businessFlowStatisticsBase.TotalExecuted + historyBusinessflowStatisticsBase.TotalExecuted;

                        businessFlowStatisticsBase.TotalPassed =
                            businessFlowStatisticsBase.TotalPassed + historyBusinessflowStatisticsBase.TotalPassed;

                        foreach (var dicObj in historyBusinessflowStatisticsBase.StatusCount)
                        {
                            if (businessFlowStatisticsBase.StatusCount.ContainsKey(dicObj.Key))
                            {
                                businessFlowStatisticsBase.StatusCount[dicObj.Key] = businessFlowStatisticsBase.StatusCount[dicObj.Key] + dicObj.Value;
                            }
                            else
                            {
                                businessFlowStatisticsBase.StatusCount[dicObj.Key] = dicObj.Value;
                            }
                        }                         
                    }

                    foreach (BusinessFlow businessFlow in runner.Executor.BusinessFlows)
                    {
                        if (businessFlow.Active)
                        {
                            var bfStat = accountReportStatistics.FirstOrDefault(x => x.EntityId == businessFlow.Guid);
                            if (bfStat != null && bfStat.ChildsExecutionStatistics!=null)
                            {
                                StatisticsBase historyActivityStatisticsBase = new StatisticsBase();
                                bfStat.ChildsExecutionStatistics.TryGetValue(eEntityType.Activity, out historyActivityStatisticsBase);

                                activityStatisticsBase.TotalExecutable =
                                 activityStatisticsBase.TotalExecutable + historyActivityStatisticsBase.TotalExecutable;

                                activityStatisticsBase.TotalExecuted =
                                                    activityStatisticsBase.TotalExecuted + historyActivityStatisticsBase.TotalExecuted;

                                activityStatisticsBase.TotalPassed =
                                    activityStatisticsBase.TotalPassed + historyActivityStatisticsBase.TotalPassed;
                                foreach (var dicObj in historyActivityStatisticsBase.StatusCount)
                                {
                                    if (activityStatisticsBase.StatusCount.ContainsKey(dicObj.Key))
                                    {
                                        activityStatisticsBase.StatusCount[dicObj.Key] = activityStatisticsBase.StatusCount[dicObj.Key] + dicObj.Value;
                                    }
                                    else
                                    {
                                        activityStatisticsBase.StatusCount[dicObj.Key] = dicObj.Value;
                                    }
                                }                                 
                            }

                            foreach (Activity activity in businessFlow.Activities)
                            {
                                if (activity.Active)
                                {
                                    var stat = accountReportStatistics.FirstOrDefault(x => x.EntityId == activity.Guid);
                                    if (stat != null && stat.ChildsExecutionStatistics!=null)
                                    {
                                        StatisticsBase historyActionStatisticsBase = new StatisticsBase();
                                        stat.ChildsExecutionStatistics.TryGetValue(eEntityType.Action, out historyActionStatisticsBase);

                                        actionStatisticsBase.TotalExecutable =
                                          actionStatisticsBase.TotalExecutable + historyActionStatisticsBase.TotalExecutable;


                                        actionStatisticsBase.TotalExecuted =
                                                                        actionStatisticsBase.TotalExecuted + historyActionStatisticsBase.TotalExecuted;

                                        actionStatisticsBase.TotalPassed =
                                            actionStatisticsBase.TotalPassed + historyActionStatisticsBase.TotalPassed;

                                        foreach (var dicObj in historyActionStatisticsBase.StatusCount)
                                        {
                                            if (actionStatisticsBase.StatusCount.ContainsKey(dicObj.Key))
                                            {
                                                actionStatisticsBase.StatusCount[dicObj.Key] = actionStatisticsBase.StatusCount[dicObj.Key] + dicObj.Value;
                                            }
                                            else
                                            {
                                                actionStatisticsBase.StatusCount[dicObj.Key] = dicObj.Value;
                                            }
                                        }                                         
                                    }
                                }
                            }
                        }
                    }
                }
            }

            accountReportRunSet.ChildsExecutionStatistics.Add(eEntityType.Action, actionStatisticsBase);
            accountReportRunSet.ChildsExecutionStatistics.Add(eEntityType.Activity, activityStatisticsBase);
            accountReportRunSet.ChildsExecutionStatistics.Add(eEntityType.BusinessFlow, businessFlowStatisticsBase);
            accountReportRunSet.ChildsExecutionStatistics.Add(eEntityType.Runner, runnerStatisticsBase);

            accountReportStatistics.Add(new AccountReportStatistics()
            {
                ChildsExecutionStatistics = accountReportRunSet.ChildsExecutionStatistics,
                EntityId = runSet.Guid,
                Type = runSet.GetType().Name
            });             
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

            List<string> inputValues = mAction.InputValues.Select(a => OverrideHTMLRelatedCharacters(a.Param + "_:_" + a.Value + "_:_" + a.ValueForDriver)).ToList();

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
