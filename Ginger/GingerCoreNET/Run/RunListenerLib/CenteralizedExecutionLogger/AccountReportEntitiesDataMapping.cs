using AccountReport.Contracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Run;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger
{
    public class AccountReportEntitiesDataMapping 
    {     
        public AccountReportAction MapActionStartData(GingerCore.Actions.Act action, Context context)
        {
            action.ExecutionId = Guid.NewGuid(); // check incase of retry / flow control 
            

            AccountReportAction accountReportAction = new AccountReportAction();     
            if(context.Activity!=null)
            {
                accountReportAction.Seq = context.Activity.ExecutionLogActionCounter;
                action.ParentExecutionId = context.Activity.ExecutionId;
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
            accountReportAction.StartTimeStamp = action.StartTimeStamp;         
            accountReportAction.InputValues = GetInputValues(action);            
            accountReportAction.CurrentRetryIteration = action.RetryMechanismCount;            
            accountReportAction.Wait = Convert.ToInt32(action.Wait);
            accountReportAction.TimeOut = action.Timeout;
                   

            return accountReportAction;
        }
        public AccountReportAction MapActionEndData(GingerCore.Actions.Act action, Context context)
        {
            AccountReportAction accountReportAction = new AccountReportAction();
            List<string> newScreenShotsList = new List<string>();
            accountReportAction.EndTimeStamp = action.EndTimeStamp;
            accountReportAction.ElapsedEndTimeStamp = action.Elapsed;
            accountReportAction.RunStatus = action.Status.ToString();           
            accountReportAction.OutputValues = action.ReturnValues.Select(a => a.Param + "_:_" + a.Actual + "_:_" + a.ExpectedCalculated + "_:_" + a.Status).ToList();
            accountReportAction.FlowControls = action.FlowControls.Select(a => a.Condition + "_:_" + a.ConditionCalculated + "_:_" + a.FlowControlAction + "_:_" + a.Status).ToList();            
            accountReportAction.Error = action.Error;
            accountReportAction.ExInfo = action.ExInfo;
            accountReportAction.Id = action.ExecutionId;
            foreach (string screenshot in action.ScreenShots)
            {               
                string newScreenshotPath = WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID.ToString() + "/" + Path.GetFileName(screenshot);

                newScreenShotsList.Add(newScreenshotPath);
            }
            accountReportAction.ScreenShots = newScreenShotsList;
            return accountReportAction;
        }
        public AccountReportActivity MapActivityStartData(Activity activity, Context context)
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
            accountReportActivity.StartTimeStamp = activity.StartTimeStamp;            
            accountReportActivity.VariablesBeforeExec = activity.VariablesBeforeExec;
            accountReportActivity.ActivityGroupName = activity.ActivitiesGroupID;           
            return accountReportActivity;
        }
        public AccountReportActivity MapActivityEndData(Activity activity, Context context)
        {
            AccountReportActivity accountReportActivity = new AccountReportActivity();                       
            accountReportActivity.EndTimeStamp = activity.EndTimeStamp;
            accountReportActivity.ElapsedEndTimeStamp = activity.Elapsed;
            accountReportActivity.RunStatus = activity.Status.ToString();
            accountReportActivity.VariablesAfterExec = activity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();                                    
            accountReportActivity.ChildExecutedItemsCount = activity.Acts.Count(x => x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored);
            accountReportActivity.ChildPassedItemsCount = activity.Acts.Count(x => x.Status == eRunStatus.Passed);
            accountReportActivity.ChildExecutableItemsCount = activity.Acts.Count(x => x.Active == true);

            accountReportActivity.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportActivity.ChildExecutedItemsCount, accountReportActivity.ChildExecutableItemsCount));
            accountReportActivity.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportActivity.ChildPassedItemsCount, accountReportActivity.ChildExecutedItemsCount));
            return accountReportActivity;
        }

        public AccountReportActivityGroup MapActivityGroupStartData(ActivitiesGroup activitiesGroup, Context context)
        {
            activitiesGroup.ExecutionId = Guid.NewGuid();
            activitiesGroup.ParentExecutionId = context.BusinessFlow.ExecutionId;

            AccountReportActivityGroup accountReportActivityGroup = new AccountReportActivityGroup();
            accountReportActivityGroup.Id = activitiesGroup.ExecutionId;
            accountReportActivityGroup.EntityId = activitiesGroup.Guid;
            accountReportActivityGroup.AccountReportDbBusinessFlowId = activitiesGroup.ParentExecutionId;
            accountReportActivityGroup.ExecutionId =  (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportActivityGroup.Seq = context.BusinessFlow.ActivitiesGroups.IndexOf(activitiesGroup) ;// context.BusinessFlow.ExecutionLogActivityGroupCounter;            
            accountReportActivityGroup.Name = activitiesGroup.Name;
            accountReportActivityGroup.Description = activitiesGroup.Description;
            accountReportActivityGroup.AutomationPrecentage = activitiesGroup.AutomationPrecentage;
            accountReportActivityGroup.StartTimeStamp = RunListenerBase.GetDateTime(activitiesGroup.StartTimeStamp);           
            accountReportActivityGroup.ExecutedActivitiesGUID = activitiesGroup.ExecutedActivities.Select(x => x.Key).ToList();           
            return accountReportActivityGroup;
        }

        public AccountReportActivityGroup MapActivityGroupEndData(ActivitiesGroup activitiesGroup, Context context)
        {
            AccountReportActivityGroup accountReportActivityGroup = new AccountReportActivityGroup();            
            accountReportActivityGroup.EndTimeStamp = RunListenerBase.GetDateTime(activitiesGroup.EndTimeStamp);
            accountReportActivityGroup.ElapsedEndTimeStamp = activitiesGroup.Elapsed;
            accountReportActivityGroup.RunStatus = activitiesGroup.RunStatus.ToString();            
            return accountReportActivityGroup;
        }

        public AccountReportBusinessFlow MapBusinessFlowStartData(BusinessFlow businessFlow, Context context)
        {
            businessFlow.ExecutionId = Guid.NewGuid();
            businessFlow.ParentExecutionId = context.Runner.ExecutionId;

            AccountReportBusinessFlow accountReportBusinessFlow = new AccountReportBusinessFlow();
            accountReportBusinessFlow.Id = businessFlow.ExecutionId;
            accountReportBusinessFlow.EntityId = businessFlow.Guid;
            accountReportBusinessFlow.AccountReportDbRunnerId = businessFlow.ParentExecutionId;
            accountReportBusinessFlow.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportBusinessFlow.Seq = context.Runner.ExecutionLogBusinessFlowsCounter;            
            accountReportBusinessFlow.InstanceGUID = businessFlow.InstanceGuid;
            accountReportBusinessFlow.Name = businessFlow.Name;
            accountReportBusinessFlow.Description = businessFlow.Description;
            accountReportBusinessFlow.RunDescription = GetCalculatedValue(context, businessFlow.RunDescription);
            accountReportBusinessFlow.Environment = businessFlow.Environment;
            accountReportBusinessFlow.StartTimeStamp = businessFlow.StartTimeStamp;           
            accountReportBusinessFlow.VariablesBeforeExec = businessFlow.VariablesBeforeExec;            
            accountReportBusinessFlow.SolutionVariablesBeforeExec = businessFlow.SolutionVariablesBeforeExec;            
            return accountReportBusinessFlow;
        }

        public AccountReportBusinessFlow MapBusinessFlowEndData(BusinessFlow businessFlow, Context context)
        {
            AccountReportBusinessFlow accountReportBusinessFlow = new AccountReportBusinessFlow();
          
            accountReportBusinessFlow.EndTimeStamp = businessFlow.EndTimeStamp;
            accountReportBusinessFlow.ElapsedEndTimeStamp = businessFlow.Elapsed;
            accountReportBusinessFlow.RunStatus = businessFlow.RunStatus.ToString();            
            accountReportBusinessFlow.VariablesAfterExec = businessFlow.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();            
            accountReportBusinessFlow.SolutionVariablesAfterExec = businessFlow.GetSolutionVariables().Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList(); ;
            accountReportBusinessFlow.BFFlowControlDT = businessFlow.BFFlowControls.Select(a => a.Condition + "_:_" + a.ConditionCalculated + "_:_" + a.BusinessFlowControlAction + "_:_" + a.Status).ToList(); ;

            int ChildExecutableItemsCountAction = 0;
            int ChildExecutedItemsCountAction = 0;
            int ChildPassedItemsCountAction = 0;
            foreach (Activity activity in businessFlow.Activities)
            {
                ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + activity.Acts.Count(x => x.Active == true);
                ChildExecutedItemsCountAction = ChildExecutedItemsCountAction + activity.Acts.Count(x => x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored);
                ChildPassedItemsCountAction = ChildPassedItemsCountAction + activity.Acts.Count(x => x.Status == eRunStatus.Passed);
            }
            string Actvities = HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString();
            string Actions = HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString();
            accountReportBusinessFlow.ChildExecutableItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportBusinessFlow.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = businessFlow.Activities.Count(x => x.Active == true) });
            accountReportBusinessFlow.ChildExecutedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportBusinessFlow.ChildExecutedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = businessFlow.Activities.Where(ac => ac.Status == eRunStatus.Failed || ac.Status == eRunStatus.Passed || ac.Status == eRunStatus.FailIgnored).Count() });
            accountReportBusinessFlow.ChildPassedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportBusinessFlow.ChildPassedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actvities, Value = businessFlow.Activities.Where(ac => ac.Status == eRunStatus.Passed).Count() });

            accountReportBusinessFlow.ChildExecutableItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportBusinessFlow.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutableItemsCountAction });
            accountReportBusinessFlow.ChildExecutedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportBusinessFlow.ChildExecutedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutedItemsCountAction });
            accountReportBusinessFlow.ChildPassedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportBusinessFlow.ChildPassedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildPassedItemsCountAction });

            accountReportBusinessFlow.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportBusinessFlow.ChildExecutedItemsCount.Count, accountReportBusinessFlow.ChildExecutableItemsCount.Count));

            accountReportBusinessFlow.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportBusinessFlow.ChildPassedItemsCount.Count, accountReportBusinessFlow.ChildExecutedItemsCount.Count));
            return accountReportBusinessFlow;
        }

        public AccountReportRunner MapRunnerStartData(GingerRunner gingerRunner, Context context)
        {
            gingerRunner.ExecutionId = Guid.NewGuid();
            gingerRunner.ParentExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;

            AccountReportRunner accountReportRunner = new AccountReportRunner();
            accountReportRunner.Id = gingerRunner.ExecutionId;
            accountReportRunner.EntityId = gingerRunner.Guid;
            accountReportRunner.AccountReportDbRunSetId = gingerRunner.ParentExecutionId;
            accountReportRunner.ExecutionId = (Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            accountReportRunner.Seq = gingerRunner.ExecutionLoggerManager.GingerData.Seq;
            accountReportRunner.Name = gingerRunner.Name;
            //accountReportRunner.Description = gingerRunner.Description;
            accountReportRunner.Environment = gingerRunner.ProjEnvironment.Name.ToString();
            accountReportRunner.EndTimeStamp = gingerRunner.EndTimeStamp;
            accountReportRunner.ApplicationAgentsMappingList = gingerRunner.ApplicationAgents.Select(a => a.AgentName + "_:_" + a.AppName).ToList();            
            return accountReportRunner;
        }

        public AccountReportRunner MapRunnerEndData(GingerRunner gingerRunner, Context context)
        {
            AccountReportRunner accountReportRunner = new AccountReportRunner();           
            accountReportRunner.ElapsedEndTimeStamp = gingerRunner.Elapsed;
            accountReportRunner.EndTimeStamp = gingerRunner.EndTimeStamp;
            //accountReportRunner.RunStatus = gingerRunner.Status.ToString();//SetStatus(BusinessFlowsColl); // check if need to calculate based on businessflows status data
            accountReportRunner.RunStatus = GetRunnerStatus(gingerRunner).ToString();
            SetRunnerChildCounts(gingerRunner, accountReportRunner);

            accountReportRunner.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunner.ChildExecutedItemsCount.Count, accountReportRunner.ChildExecutableItemsCount.Count));

            accountReportRunner.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunner.ChildPassedItemsCount.Count, accountReportRunner.ChildExecutedItemsCount.Count));
            return accountReportRunner;
        }
        
        public AccountReportRunSet MapRunsetStartData(RunSetConfig runSetConfig, Context context)
        {
            runSetConfig.ExecutionID = WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;

            AccountReportRunSet accountReportRunSet = new AccountReportRunSet();
            accountReportRunSet.Id = (Guid)runSetConfig.ExecutionID;
            accountReportRunSet.EntityId = runSetConfig.Guid;
            //accountReportRunSet.Seq = runSetConfig.Seq;
            accountReportRunSet.Name = runSetConfig.Name;
            accountReportRunSet.Description = runSetConfig.Description;
            accountReportRunSet.Environment = runSetConfig.GingerRunners[0].ProjEnvironment.ToString();
            accountReportRunSet.EndTimeStamp = runSetConfig.EndTimeStamp;
            accountReportRunSet.MachineName = System.Environment.MachineName.ToString();
            accountReportRunSet.ExecutedByUser = System.Environment.UserName.ToString();
            accountReportRunSet.GingerVersion = ApplicationInfo.ApplicationVersion;          
            return accountReportRunSet;
        }

        public AccountReportRunSet MapRunsetEndData(RunSetConfig runSetConfig, Context context)
        {
            AccountReportRunSet accountReportRunSet = new AccountReportRunSet();           
            accountReportRunSet.ElapsedEndTimeStamp = runSetConfig.Elapsed;
            accountReportRunSet.EndTimeStamp = runSetConfig.EndTimeStamp;
            //Calculate at runset end
            accountReportRunSet.RunStatus = (runSetConfig.RunSetExecutionStatus == eRunStatus.Automated)
                ? eRunStatus.Automated.ToString() : runSetConfig.RunSetExecutionStatus.ToString();
            SetRunSetChildCounts(runSetConfig, accountReportRunSet);
            accountReportRunSet.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunSet.ChildExecutedItemsCount.Count, accountReportRunSet.ChildExecutableItemsCount.Count));

            accountReportRunSet.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(accountReportRunSet.ChildPassedItemsCount.Count, accountReportRunSet.ChildExecutedItemsCount.Count));
            return accountReportRunSet;
        }

        public Amdocs.Ginger.CoreNET.Execution.eRunStatus GetRunnerStatus(GingerRunner gingerRunner)
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

        private void SetRunnerChildCounts(GingerRunner runner, AccountReportRunner accountReportRunner)
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
                int count = 0;
                
                ChildExecutableItemsCountActivity = ChildExecutableItemsCountActivity + businessFlow.Activities.Count(x => x.Active == true);
                
                ChildExecutedItemsCountActivity =  ChildExecutedItemsCountActivity + businessFlow.Activities.Where(ac => ac.Status == eRunStatus.Failed || ac.Status == eRunStatus.Passed || ac.Status == eRunStatus.FailIgnored).Count();
               
                ChildPassedItemsCountActivity = ChildPassedItemsCountActivity + businessFlow.Activities.Where(ac => ac.Status == eRunStatus.Passed).Count();

                foreach (Activity activity in businessFlow.Activities)
                {
                    ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + ChildExecutableItemsCountAction + activity.Acts.Count(x => x.Active == true);

                    ChildExecutedItemsCountAction = ChildExecutedItemsCountAction + ChildExecutedItemsCountAction + activity.Acts.Count(x => x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored); ;

                    ChildPassedItemsCountAction = ChildPassedItemsCountAction + activity.Acts.Count(x => x.Status == eRunStatus.Passed);
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
            { Key = Actvities, Value = ChildPassedItemsCountActivity } );
            accountReportRunner.ChildExecutableItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunner.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutableItemsCountAction } );
            accountReportRunner.ChildExecutedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunner.ChildExecutedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutedItemsCountAction });
            accountReportRunner.ChildPassedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunner.ChildPassedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildPassedItemsCountAction } );
        }

        private void SetRunSetChildCounts(RunSetConfig runSet, AccountReportRunSet accountReportRunSet)
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
                int count = 0;
                foreach (BusinessFlow businessFlow in runner.BusinessFlows)
                {                    
                    ChildExecutableItemsCountActivity = ChildExecutableItemsCountActivity + businessFlow.Activities.Count(x => x.Active == true);

                    ChildExecutedItemsCountActivity = ChildExecutedItemsCountActivity + businessFlow.Activities.Where(ac => ac.Status == eRunStatus.Failed || ac.Status == eRunStatus.Passed || ac.Status == eRunStatus.FailIgnored).Count();

                    ChildPassedItemsCountActivity = ChildPassedItemsCountActivity + businessFlow.Activities.Where(ac => ac.Status == eRunStatus.Passed).Count();

                    foreach (Activity activity in businessFlow.Activities)
                    {
                        ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + ChildExecutableItemsCountAction + activity.Acts.Count(x => x.Active == true);

                        ChildExecutedItemsCountAction = ChildExecutedItemsCountAction + ChildExecutedItemsCountAction + activity.Acts.Count(x => x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored); ;

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
            accountReportRunSet.ChildExecutableItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunSet.ChildExecutableItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutableItemsCountAction });
            accountReportRunSet.ChildExecutedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunSet.ChildExecutedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildExecutedItemsCountAction });
            accountReportRunSet.ChildPassedItemsCount = new List<AccountReport.Contracts.Helpers.DictObject>();
            accountReportRunSet.ChildPassedItemsCount.Add(new AccountReport.Contracts.Helpers.DictObject
            { Key = Actions, Value = ChildPassedItemsCountAction });
        }


        private string CalculateExecutionOrPassRate(int firstItem, int secondItem)
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

        private string GetCalculatedValue(Context context, string stringToCalculate)
        {
            IValueExpression mVE = new GingerCore.ValueExpression(context.Environment, context.BusinessFlow, new ObservableList<GingerCore.DataSource.DataSourceBase>(), false, "", false);           
            mVE.Value = stringToCalculate;
            return mVE.ValueCalculated;
        }

        public List<string> GetInputValues(Act mAction)
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
