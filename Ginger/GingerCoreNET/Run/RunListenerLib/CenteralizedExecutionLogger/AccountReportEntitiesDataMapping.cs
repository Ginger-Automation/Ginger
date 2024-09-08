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
        static string _InProgressStatus = "In Progress";
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
            accountReportActivity.ChildsExecutionStatistics = new AccountReport.Contracts.RequestModels.ExecutionStatistics();
            accountReportActivity.ChildsExecutionStatistics.Action = new ActionsStatistics();
            accountReportActivity.ChildsExecutionStatistics.Action.TotalExecutable = activity.Acts.Count(x => x.Active);            
            accountReportActivity.RunStatus = _InProgressStatus;
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
            accountReportActivity.RunStatus = activity.Status.ToString();
            accountReportActivity.ExternalID = GetCalculatedValue(context, activity.ExternalID);
            accountReportActivity.ExternalID2 = activity.ExternalID2;
            accountReportActivity.VariablesAfterExec = activity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description + "_:_" + a.Guid + "_:_" + a.SetAsInputValue + "_:_" + a.SetAsOutputValue + "_:_" + a.Publish).ToList();
          
            accountReportActivity.ChildsExecutionStatistics = new AccountReport.Contracts.RequestModels.ExecutionStatistics();
            accountReportActivity.ChildsExecutionStatistics.Action = new ActionsStatistics();
            accountReportActivity.ChildsExecutionStatistics.Action.StatusCount = new Dictionary<string, int>();

            accountReportActivity.ChildsExecutionStatistics.Action.TotalExecuted = activity.Acts.Count(x => x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Stopped || x.Status == eRunStatus.Completed);
            accountReportActivity.ChildsExecutionStatistics.Action.TotalPassed = activity.Acts.Count(x => x.Status == eRunStatus.Passed);
            accountReportActivity.ChildsExecutionStatistics.Action.TotalExecutable = activity.Acts.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Blocked));

            var actionsStat = (from a in activity.Acts.GroupBy(x => x.Status)
                              select new KeyValuePair<string, int>(a.First().Status.ToString(), a.Count())).ToList();
            foreach (var astat in actionsStat)
            {
                accountReportActivity.ChildsExecutionStatistics.Action.StatusCount[astat.Key] = astat.Value;
            }
                
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
            accountReportActivity.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate((int)accountReportActivity.ChildsExecutionStatistics.Action.TotalExecuted, (int)accountReportActivity.ChildsExecutionStatistics.Action.TotalExecutable));
            accountReportActivity.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate((int)accountReportActivity.ChildsExecutionStatistics.Action.TotalPassed, (int)accountReportActivity.ChildsExecutionStatistics.Action.TotalExecutable));
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
            accountReportActivityGroup.RunStatus = activitiesGroup.RunStatus.ToString();
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
            accountReportBusinessFlow.RunStatus = _InProgressStatus;
            accountReportBusinessFlow.IsPublished = businessFlow.Publish;
            accountReportBusinessFlow.ExternalID = GetCalculatedValue(context, businessFlow.ExternalID);
            accountReportBusinessFlow.ExternalID2 = businessFlow.ExternalID2;
       
            int ChildExecutableItemsCountAction = 0;
            foreach (Activity activity in businessFlow.Activities)
            {
                ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + activity.Acts.Count(x => x.Active);
            }
            accountReportBusinessFlow.ChildsExecutionStatistics = new AccountReport.Contracts.RequestModels.ExecutionStatistics();
            accountReportBusinessFlow.ChildsExecutionStatistics.Action = new ActionsStatistics();
            accountReportBusinessFlow.ChildsExecutionStatistics.Activity = new ActivitiesStatistics();
            accountReportBusinessFlow.ChildsExecutionStatistics.Action.TotalExecutable = ChildExecutableItemsCountAction;
            accountReportBusinessFlow.ChildsExecutionStatistics.Activity.TotalExecutable = businessFlow.Activities.Count(x => x.Active);

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
            accountReportBusinessFlow.ExternalID = GetCalculatedValue(context, businessFlow.ExternalID);
            accountReportBusinessFlow.ExternalID2 = businessFlow.ExternalID2;
            if(businessFlow.ParentGuid != Guid.Empty)
            {
                accountReportBusinessFlow.ParentID = businessFlow.ParentGuid;
            }            
            accountReportBusinessFlow.ChildsExecutionStatistics = new AccountReport.Contracts.RequestModels.ExecutionStatistics();
            accountReportBusinessFlow.ChildsExecutionStatistics.Action = new ActionsStatistics();
            accountReportBusinessFlow.ChildsExecutionStatistics.Activity = new ActivitiesStatistics();
            accountReportBusinessFlow.ChildsExecutionStatistics.Action.StatusCount = new Dictionary<string, int>();
            accountReportBusinessFlow.ChildsExecutionStatistics.Activity.StatusCount = new Dictionary<string, int>();
            

            var activitiesStat = (from a in businessFlow.Activities.GroupBy(x => x.Status)
                                select new KeyValuePair<string, int>(a.First().Status.ToString(), a.Count())).ToList();
           
            foreach (var astat in activitiesStat)
            {
                accountReportBusinessFlow.ChildsExecutionStatistics.Activity.StatusCount[astat.Key] = astat.Value;
            }

            accountReportBusinessFlow.ChildsExecutionStatistics.Activity.TotalExecuted = businessFlow.Activities.Count(ac => ac.Status == eRunStatus.Failed || ac.Status == eRunStatus.Passed || ac.Status == eRunStatus.FailIgnored || ac.Status == eRunStatus.Stopped || ac.Status == eRunStatus.Completed);
            accountReportBusinessFlow.ChildsExecutionStatistics.Activity.TotalPassed = businessFlow.Activities.Count(ac => ac.Status == eRunStatus.Passed);
            accountReportBusinessFlow.ChildsExecutionStatistics.Activity.TotalExecutable = businessFlow.Activities.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Blocked));

            foreach (Activity activity in businessFlow.Activities)
            {
                if(activity.Active)
                {
                    var stat = accountReportStatistics.Where(x => x.EntityId == activity.Guid).FirstOrDefault();
                    if (stat!=null)
                    {
                        accountReportBusinessFlow.ChildsExecutionStatistics.Action.TotalExecutable =
                          accountReportBusinessFlow.ChildsExecutionStatistics.Action.TotalExecutable + stat.ChildsExecutionStatistics.Action.TotalExecutable;

                        accountReportBusinessFlow.ChildsExecutionStatistics.Action.TotalExecuted =
                            accountReportBusinessFlow.ChildsExecutionStatistics.Action.TotalExecuted + stat.ChildsExecutionStatistics.Action.TotalExecuted;
                        
                        accountReportBusinessFlow.ChildsExecutionStatistics.Action.TotalPassed =
                           accountReportBusinessFlow.ChildsExecutionStatistics.Action.TotalPassed + stat.ChildsExecutionStatistics.Action.TotalPassed;

                        foreach (var dicObj in stat.ChildsExecutionStatistics.Action.StatusCount)
                        {
                            if (accountReportBusinessFlow.ChildsExecutionStatistics.Action.StatusCount.ContainsKey(dicObj.Key))
                            {
                                accountReportBusinessFlow.ChildsExecutionStatistics.Action.StatusCount[dicObj.Key] = accountReportBusinessFlow.ChildsExecutionStatistics.Action.StatusCount[dicObj.Key] + dicObj.Value;
                            }
                            else
                            {
                                accountReportBusinessFlow.ChildsExecutionStatistics.Action.StatusCount[dicObj.Key] = dicObj.Value;
                            }
                        }  
                    }
                }
            }

            accountReportStatistics.Add(new AccountReportStatistics()
            {
                ChildsExecutionStatistics = accountReportBusinessFlow.ChildsExecutionStatistics,
                EntityId = businessFlow.Guid,
                Type = businessFlow.GetType().Name
            });

            if((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 1)
            {
                accountReportBusinessFlow.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportBusinessFlow.ChildsExecutionStatistics.Activity.TotalExecuted, accountReportBusinessFlow.ChildsExecutionStatistics.Activity.TotalExecutable));

                accountReportBusinessFlow.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportBusinessFlow.ChildsExecutionStatistics.Activity.TotalPassed, accountReportBusinessFlow.ChildsExecutionStatistics.Activity.TotalExecutable));
            }
            else if ((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 0)
            {
                accountReportBusinessFlow.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportBusinessFlow.ChildsExecutionStatistics.Action.TotalExecuted, accountReportBusinessFlow.ChildsExecutionStatistics.Action.TotalExecutable));

                accountReportBusinessFlow.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportBusinessFlow.ChildsExecutionStatistics.Action.TotalPassed, accountReportBusinessFlow.ChildsExecutionStatistics.Action.TotalExecutable));
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
            accountReportRunner.ExternalID = gingerRunner.ExternalID;
            accountReportRunner.ExternalID2 = gingerRunner.ExternalID2;
            if(gingerRunner.ParentGuid != Guid.Empty)
            {
                accountReportRunner.ParentID = gingerRunner.ParentGuid;
            }
            //accountReportRunner.RunStatus = gingerRunner.Status.ToString();//SetStatus(BusinessFlowsColl); // check if need to calculate based on businessflows status data
            accountReportRunner.RunStatus = GetRunnerStatus((GingerExecutionEngine)gingerRunner.Executor).ToString();
            SetRunnerChildCountsAtEnd(gingerRunner, accountReportRunner);

            if ((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 1)
            {
                accountReportRunner.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunner.ChildsExecutionStatistics.Activity.TotalExecuted, accountReportRunner.ChildsExecutionStatistics.Activity.TotalExecutable));

                accountReportRunner.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunner.ChildsExecutionStatistics.Activity.TotalPassed, accountReportRunner.ChildsExecutionStatistics.Activity.TotalExecutable));
            }
            else if ((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 0)
            {
                accountReportRunner.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunner.ChildsExecutionStatistics.Action.TotalExecuted, accountReportRunner.ChildsExecutionStatistics.Action.TotalExecutable));

                accountReportRunner.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunner.ChildsExecutionStatistics.Action.TotalPassed, accountReportRunner.ChildsExecutionStatistics.Action.TotalExecutable));
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
            accountReportRunSet.RunStatus = _InProgressStatus;           
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
                ? eRunStatus.Automated.ToString() : runSetConfig.RunSetExecutionStatus.ToString();
            SetRunSetChildCountsAtEnd(runSetConfig, accountReportRunSet);         

            if ((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 1)
            {
                accountReportRunSet.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunSet.ChildsExecutionStatistics.Activity.TotalExecuted, accountReportRunSet.ChildsExecutionStatistics.Activity.TotalExecutable));

                accountReportRunSet.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunSet.ChildsExecutionStatistics.Activity.TotalPassed, accountReportRunSet.ChildsExecutionStatistics.Activity.TotalExecutable));
            }
            else if ((int)_HTMLReportConfig.ExecutionStatisticsCountBy == 0)
            {
                accountReportRunSet.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunSet.ChildsExecutionStatistics.Action.TotalExecuted, accountReportRunSet.ChildsExecutionStatistics.Action.TotalExecutable));

                accountReportRunSet.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunSet.ChildsExecutionStatistics.Action.TotalPassed, accountReportRunSet.ChildsExecutionStatistics.Action.TotalExecutable));
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
            accountReportRunner.ChildsExecutionStatistics = new AccountReport.Contracts.RequestModels.ExecutionStatistics();
            accountReportRunner.ChildsExecutionStatistics.Action = new ActionsStatistics();
            accountReportRunner.ChildsExecutionStatistics.Activity = new ActivitiesStatistics();
            accountReportRunner.ChildsExecutionStatistics.BusinessFlow = new BusinessFlowStatistics();

            accountReportRunner.ChildsExecutionStatistics.BusinessFlow.TotalExecutable = ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows.Count(bf => bf.Active);

            foreach (BusinessFlow businessFlow in ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows)
            {
                if (businessFlow.Active)
                {                   
                        accountReportRunner.ChildsExecutionStatistics.Activity.TotalExecutable =
                               accountReportRunner.ChildsExecutionStatistics.Activity.TotalExecutable + businessFlow.Activities.Count(ac=>ac.Active);
                                                        
                    foreach (Activity activity in businessFlow.Activities)
                    {
                        if (activity.Active)
                        {                          
                                accountReportRunner.ChildsExecutionStatistics.Action.TotalExecutable =
                                 accountReportRunner.ChildsExecutionStatistics.Action.TotalExecutable + activity.Acts.Count(a=>a.Active);                                                             
                        }
                    }
                }
            }
        }
        private static void SetRunSetChildCountsAtStart(RunSetConfig runSet, AccountReportRunSet accountReportRunSet)
        {
            accountReportRunSet.ChildsExecutionStatistics = new AccountReport.Contracts.RequestModels.ExecutionStatistics();
            accountReportRunSet.ChildsExecutionStatistics.Action = new ActionsStatistics();
            accountReportRunSet.ChildsExecutionStatistics.Activity = new ActivitiesStatistics();
            accountReportRunSet.ChildsExecutionStatistics.BusinessFlow = new BusinessFlowStatistics();
            accountReportRunSet.ChildsExecutionStatistics.Runner = new RunnersStatistics();

            accountReportRunSet.ChildsExecutionStatistics.Runner.TotalExecutable = runSet.GingerRunners.Count(r => r.Active);

            foreach (GingerRunner runner in runSet.GingerRunners)
            {
                if (runner.Active)
                {
                    accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.TotalExecutable =
                    accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.TotalExecutable + ((GingerExecutionEngine)runner.Executor).BusinessFlows.Count(bf => bf.Active);
                    foreach (BusinessFlow businessFlow in runner.Executor.BusinessFlows)
                    {
                        if (businessFlow.Active)
                        {
                            accountReportRunSet.ChildsExecutionStatistics.Activity.TotalExecutable =
                                   accountReportRunSet.ChildsExecutionStatistics.Activity.TotalExecutable + businessFlow.Activities.Count(ac => ac.Active);

                            foreach (Activity activity in businessFlow.Activities)
                            {
                                if (activity.Active)
                                {
                                    accountReportRunSet.ChildsExecutionStatistics.Action.TotalExecutable =
                                     accountReportRunSet.ChildsExecutionStatistics.Action.TotalExecutable + activity.Acts.Count(a => a.Active);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void SetRunnerChildCountsAtEnd(GingerRunner gingerRunner, AccountReportRunner accountReportRunner)
        {
            accountReportRunner.ChildsExecutionStatistics = new AccountReport.Contracts.RequestModels.ExecutionStatistics();
            accountReportRunner.ChildsExecutionStatistics.Action = new ActionsStatistics();
            accountReportRunner.ChildsExecutionStatistics.Activity = new ActivitiesStatistics();
            accountReportRunner.ChildsExecutionStatistics.BusinessFlow = new BusinessFlowStatistics();
            
                accountReportRunner.ChildsExecutionStatistics.Action.StatusCount = new Dictionary<string, int>();
                accountReportRunner.ChildsExecutionStatistics.Activity.StatusCount = new Dictionary<string, int>();
                accountReportRunner.ChildsExecutionStatistics.BusinessFlow.StatusCount = new Dictionary<string, int>();

                var businessFlowStat = (from a in ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows.GroupBy(x => x.RunStatus)
                                        select new KeyValuePair<string, int>(a.First().RunStatus.ToString(), a.Count())).ToList();

                foreach (var bstat in businessFlowStat)
                {
                    accountReportRunner.ChildsExecutionStatistics.BusinessFlow.StatusCount[bstat.Key] = bstat.Value;
                }
                accountReportRunner.ChildsExecutionStatistics.BusinessFlow.TotalExecuted = ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows.Count(bf => bf.RunStatus == eRunStatus.Failed || bf.RunStatus == eRunStatus.Passed || bf.RunStatus == eRunStatus.FailIgnored || bf.RunStatus == eRunStatus.Stopped || bf.RunStatus == eRunStatus.Completed);
                accountReportRunner.ChildsExecutionStatistics.BusinessFlow.TotalPassed = ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows.Count(bf => bf.RunStatus == eRunStatus.Passed);
                accountReportRunner.ChildsExecutionStatistics.BusinessFlow.TotalExecutable = ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows.Count(bf => bf.Active && (bf.RunStatus == eRunStatus.Passed || bf.RunStatus == eRunStatus.Failed || bf.RunStatus == eRunStatus.FailIgnored || bf.RunStatus == eRunStatus.Blocked));
                        

            foreach (BusinessFlow businessFlow in ((GingerExecutionEngine)gingerRunner.Executor).BusinessFlows)
            {
                if (businessFlow.Active)
                {                   
                    var bfStat = accountReportStatistics.Where(x => x.EntityId == businessFlow.Guid).FirstOrDefault();
                    if (bfStat!=null)
                    {                             
                            accountReportRunner.ChildsExecutionStatistics.Activity.TotalExecutable =
                                   accountReportRunner.ChildsExecutionStatistics.Activity.TotalExecutable + bfStat.ChildsExecutionStatistics.Activity.TotalExecutable;

                            accountReportRunner.ChildsExecutionStatistics.Activity.TotalExecuted =
                                   accountReportRunner.ChildsExecutionStatistics.Activity.TotalExecuted + bfStat.ChildsExecutionStatistics.Activity.TotalExecuted;

                            accountReportRunner.ChildsExecutionStatistics.Activity.TotalPassed =
                               accountReportRunner.ChildsExecutionStatistics.Activity.TotalPassed + bfStat.ChildsExecutionStatistics.Activity.TotalPassed;

                            foreach (var dicObj in bfStat.ChildsExecutionStatistics.Activity.StatusCount)
                            {
                                if (accountReportRunner.ChildsExecutionStatistics.Activity.StatusCount.ContainsKey(dicObj.Key))
                                {
                                    accountReportRunner.ChildsExecutionStatistics.Activity.StatusCount[dicObj.Key] = accountReportRunner.ChildsExecutionStatistics.Activity.StatusCount[dicObj.Key] + dicObj.Value;
                                }
                                else
                                {
                                    accountReportRunner.ChildsExecutionStatistics.Activity.StatusCount[dicObj.Key] = dicObj.Value;
                                }
                            }                        
                    }

                    foreach (Activity activity in businessFlow.Activities)
                    {
                        if(activity.Active)
                        {
                            var stat = accountReportStatistics.Where(x => x.EntityId == activity.Guid).FirstOrDefault();
                           
                            if (stat != null)
                            {
                                accountReportRunner.ChildsExecutionStatistics.Action.TotalExecutable =
                                 accountReportRunner.ChildsExecutionStatistics.Action.TotalExecutable + stat.ChildsExecutionStatistics.Action.TotalExecutable;
                                                            
                                accountReportRunner.ChildsExecutionStatistics.Action.TotalExecuted =
                                accountReportRunner.ChildsExecutionStatistics.Action.TotalExecuted + stat.ChildsExecutionStatistics.Action.TotalExecuted;

                                accountReportRunner.ChildsExecutionStatistics.Action.TotalPassed =
                                    accountReportRunner.ChildsExecutionStatistics.Action.TotalPassed + stat.ChildsExecutionStatistics.Action.TotalPassed;

                                foreach (var dicObj in stat.ChildsExecutionStatistics.Action.StatusCount)
                                {
                                    if (accountReportRunner.ChildsExecutionStatistics.Action.StatusCount.ContainsKey(dicObj.Key))
                                    {
                                        accountReportRunner.ChildsExecutionStatistics.Action.StatusCount[dicObj.Key] = accountReportRunner.ChildsExecutionStatistics.Action.StatusCount[dicObj.Key] + dicObj.Value;
                                    }
                                    else
                                    {
                                        accountReportRunner.ChildsExecutionStatistics.Action.StatusCount[dicObj.Key] = dicObj.Value;
                                    }
                                }
                                
                            } 
                        }
                    } 
                }
            }
            
            accountReportStatistics.Add(new AccountReportStatistics()
            {
                ChildsExecutionStatistics = accountReportRunner.ChildsExecutionStatistics,
                EntityId = gingerRunner.Guid,
                Type = gingerRunner.GetType().Name
            });
            
        }

        private static void SetRunSetChildCountsAtEnd(RunSetConfig runSet, AccountReportRunSet accountReportRunSet)
        {
            accountReportRunSet.ChildsExecutionStatistics = new AccountReport.Contracts.RequestModels.ExecutionStatistics();
            accountReportRunSet.ChildsExecutionStatistics.Action = new ActionsStatistics();
            accountReportRunSet.ChildsExecutionStatistics.Activity = new ActivitiesStatistics();
            accountReportRunSet.ChildsExecutionStatistics.BusinessFlow = new BusinessFlowStatistics();
            accountReportRunSet.ChildsExecutionStatistics.Runner = new RunnersStatistics();
           
            accountReportRunSet.ChildsExecutionStatistics.Action.StatusCount = new Dictionary<string, int>();
            accountReportRunSet.ChildsExecutionStatistics.Activity.StatusCount = new Dictionary<string, int>();
            accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.StatusCount = new Dictionary<string, int>();
            accountReportRunSet.ChildsExecutionStatistics.Runner.StatusCount = new Dictionary<string, int>();
            accountReportRunSet.ChildsExecutionStatistics.Runner.TotalExecuted = runSet.GingerRunners.Count(rs => rs.Status == eRunStatus.Failed || rs.Status == eRunStatus.Passed || rs.Status == eRunStatus.FailIgnored || rs.Status == eRunStatus.Stopped || rs.Status == eRunStatus.Completed);
            accountReportRunSet.ChildsExecutionStatistics.Runner.TotalPassed = runSet.GingerRunners.Count(rs => rs.Status == eRunStatus.Passed);
           
            accountReportRunSet.ChildsExecutionStatistics.Runner.TotalExecutable = runSet.GingerRunners.Count(rs => rs.Active && (rs.Status == eRunStatus.Passed || rs.Status == eRunStatus.Failed || rs.Status == eRunStatus.FailIgnored || rs.Status == eRunStatus.Blocked));
            
            var runnerStat = (from a in runSet.GingerRunners.GroupBy(x => x.Status)
                                select new KeyValuePair<string, int>(a.First().Status.ToString(), a.Count())).ToList();

            foreach (var rstat in runnerStat)
            {
                accountReportRunSet.ChildsExecutionStatistics.Runner.StatusCount[rstat.Key] = rstat.Value;
            }
                                   
            foreach (GingerRunner runner in runSet.GingerRunners)
            {
                if (runner.Active)
                {
                    var rStat = accountReportStatistics.Where(x => x.EntityId == runner.Guid).FirstOrDefault();
                    if (rStat != null)
                    {
                       
                        accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.TotalExecutable =
                           accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.TotalExecutable + rStat.ChildsExecutionStatistics.BusinessFlow.TotalExecutable; 
                                            
                        accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.TotalExecuted =
                            accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.TotalExecuted + rStat.ChildsExecutionStatistics.BusinessFlow.TotalExecuted;

                        accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.TotalPassed =
                            accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.TotalPassed + rStat.ChildsExecutionStatistics.BusinessFlow.TotalPassed;

                        foreach (var dicObj in rStat.ChildsExecutionStatistics.BusinessFlow.StatusCount)
                        {
                            if (accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.StatusCount.ContainsKey(dicObj.Key))
                            {
                                accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.StatusCount[dicObj.Key] = accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.StatusCount[dicObj.Key] + dicObj.Value;
                            }
                            else
                            {
                                accountReportRunSet.ChildsExecutionStatistics.BusinessFlow.StatusCount[dicObj.Key] = dicObj.Value;
                            }
                        }                         
                    }

                    foreach (BusinessFlow businessFlow in runner.Executor.BusinessFlows)
                    {
                        if (businessFlow.Active)
                        {
                            var bfStat = accountReportStatistics.Where(x => x.EntityId == businessFlow.Guid).FirstOrDefault();
                            if (bfStat != null)
                            {
                                accountReportRunSet.ChildsExecutionStatistics.Activity.TotalExecutable =
                                 accountReportRunSet.ChildsExecutionStatistics.Activity.TotalExecutable + bfStat.ChildsExecutionStatistics.Activity.TotalExecutable;

                                accountReportRunSet.ChildsExecutionStatistics.Activity.TotalExecuted =
                                                    accountReportRunSet.ChildsExecutionStatistics.Activity.TotalExecuted + bfStat.ChildsExecutionStatistics.Activity.TotalExecuted;

                                accountReportRunSet.ChildsExecutionStatistics.Activity.TotalPassed =
                                    accountReportRunSet.ChildsExecutionStatistics.Activity.TotalPassed + bfStat.ChildsExecutionStatistics.Activity.TotalPassed;
                                foreach (var dicObj in bfStat.ChildsExecutionStatistics.Activity.StatusCount)
                                {
                                    if (accountReportRunSet.ChildsExecutionStatistics.Activity.StatusCount.ContainsKey(dicObj.Key))
                                    {
                                        accountReportRunSet.ChildsExecutionStatistics.Activity.StatusCount[dicObj.Key] = accountReportRunSet.ChildsExecutionStatistics.Activity.StatusCount[dicObj.Key] + dicObj.Value;
                                    }
                                    else
                                    {
                                        accountReportRunSet.ChildsExecutionStatistics.Activity.StatusCount[dicObj.Key] = dicObj.Value;
                                    }
                                }                                 
                            }

                            foreach (Activity activity in businessFlow.Activities)
                            {
                                if (activity.Active)
                                {
                                    var stat = accountReportStatistics.Where(x => x.EntityId == activity.Guid).FirstOrDefault();
                                    if (stat != null)
                                    {
                                        accountReportRunSet.ChildsExecutionStatistics.Action.TotalExecutable =
                                          accountReportRunSet.ChildsExecutionStatistics.Action.TotalExecutable + stat.ChildsExecutionStatistics.Action.TotalExecutable;

                                        
                                        accountReportRunSet.ChildsExecutionStatistics.Action.TotalExecuted =
                                                                        accountReportRunSet.ChildsExecutionStatistics.Action.TotalExecuted + stat.ChildsExecutionStatistics.Action.TotalExecuted;

                                        accountReportRunSet.ChildsExecutionStatistics.Action.TotalPassed =
                                            accountReportRunSet.ChildsExecutionStatistics.Action.TotalPassed + stat.ChildsExecutionStatistics.Action.TotalPassed;

                                        foreach (var dicObj in stat.ChildsExecutionStatistics.Action.StatusCount)
                                        {
                                            if (accountReportRunSet.ChildsExecutionStatistics.Action.StatusCount.ContainsKey(dicObj.Key))
                                            {
                                                accountReportRunSet.ChildsExecutionStatistics.Action.StatusCount[dicObj.Key] = accountReportRunSet.ChildsExecutionStatistics.Action.StatusCount[dicObj.Key] + dicObj.Value;
                                            }
                                            else
                                            {
                                                accountReportRunSet.ChildsExecutionStatistics.Action.StatusCount[dicObj.Key] = dicObj.Value;
                                            }
                                        }                                         
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
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
