using AccountReport.Contracts.ResponseModels;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Ginger.Reports;
using GingerCore.Activities;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using amdocs.ginger.GingerCoreNET;
using static Ginger.Reports.ExecutionLoggerConfiguration;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN.Exportation
{
    public sealed class RunSetExecutionHistoryToBPMNExporter
    {
        public RunSetExecutionHistoryToBPMNExporter() { }

        public Task<IEnumerable<ExecutedBusinessFlow>> GetExecutedBusinessFlows(string executionId, DataRepositoryMethod source)
        {
            if (source == DataRepositoryMethod.LiteDB)
            {
                return Task.FromResult(GetExecutedBusinessFlowsFromLiteDb(executionId));
            }
            else if (source == DataRepositoryMethod.Remote)
            {
                return GetExecutedBusinessFlowsFromRemoteAsync(executionId);
            }
            else
            {
                throw new BPMNExportationException($"Generation of BPMN from {GingerDicser.GetTermResValue(eTermResKey.RunSet)} execution history with report method '{source}' is not supported.");
            }
        }

        public void Export(ExecutedBusinessFlow executedBusinessFlow, string exportPath)
        {
            bool hasAnyNonSharedRepositoryActivity = executedBusinessFlow
                .ExecutedActivities
                .Any(activity => !activity.ExistInSharedRepository);

            if (hasAnyNonSharedRepositoryActivity)
            {
                throw new BPMNExportationException($"All {GingerDicser.GetTermResValue(eTermResKey.Activity)} must be added to shared repository for generating BPMN.");
            }

            BusinessFlow businessFlow = CreateBusinessFlowFromExecutionData(executedBusinessFlow);
            ExportBusinessFlow(businessFlow, exportPath);
        }

        private IEnumerable<ExecutedBusinessFlow> GetExecutedBusinessFlowsFromLiteDb(string executionId)
        {
            LiteDbManager liteDbManager = new(new ExecutionLoggerHelper().GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder));
            LiteDbRunSet liteRunSet = liteDbManager.GetLatestExecutionRunsetData(executionId);
            IEnumerable<LiteDbBusinessFlow> liteBFs = liteRunSet
                .RunnersColl
                .SelectMany(liteRunner => liteRunner.AllBusinessFlowsColl);

            List<ExecutedBusinessFlow> bfExecSequences = [];

            foreach (LiteDbBusinessFlow liteBF in liteBFs)
            {
                BusinessFlow bf = GetBusinessFlowByName(liteBF.Name);
                IEnumerable<ExecutedActivity> activities = liteBF
                    .AllActivitiesColl
                    .Select(seqActivity =>
                    {
                        bool existInSR, existInBF;
                        Activity? activity;

                        activity = GetActivityFromSharedRepository(seqActivity.Name);
                        existInSR = activity != null;
                        if (activity == null)
                        {
                            activity = GetActivityFromBusinessFlow(bf, seqActivity.Name);
                            existInBF = activity != null;
                        }
                        else
                        {
                            existInBF = GetActivityFromBusinessFlow(bf, seqActivity.Name) != null;
                        }

                        if (activity == null)
                        {
                            return null!;
                        }

                        return new ExecutedActivity(activity, seqActivity.ActivityGroupName, existInSR, existInBF);
                    })
                    .Where(executedActivity => executedActivity != null)
                    .ToArray();

                bfExecSequences.Add(new(bf, activities));
            }

            return bfExecSequences;
        }

        private async Task<IEnumerable<ExecutedBusinessFlow>> GetExecutedBusinessFlowsFromRemoteAsync(string executionId)
        {
            List<ExecutedBusinessFlow> bfExecSequences = [];

            AccountReportRunSetClient runset = await GetExecutionDataFromAccountReportAsync(executionId);
            foreach (AccountReportRunnerClient runner in runset.RunnersColl)
            {
                foreach (AccountReportBusinessFlowClient businessFlow in runner.BusinessFlowsColl)
                {
                    BusinessFlow bf = GetBusinessFlowByName(businessFlow.Name);
                    List<AccountReportActivityClient> activities = businessFlow.ActivitiesColl;

                    IEnumerable<ExecutedActivity> executedActivities = activities
                        .Select(seqActivity =>
                        {
                            bool existInSR, existInBF;
                            GingerCore.Activity? activity;

                            activity = GetActivityFromSharedRepository(seqActivity.Name);
                            existInSR = activity != null;
                            if (activity == null)
                            {
                                activity = GetActivityFromBusinessFlow(bf, seqActivity.Name);
                                existInBF = activity != null;
                            }
                            else
                            {
                                existInBF = GetActivityFromBusinessFlow(bf, seqActivity.Name) != null;
                            }

                            if (activity == null)
                            {
                                return null!;
                            }

                            return new ExecutedActivity(activity, seqActivity.ActivityGroupName, existInSR, existInBF);
                        })
                        .Where(executedActivity => executedActivity != null)
                        .ToArray();

                    bfExecSequences.Add(new(bf, executedActivities));
                }
            }

            return bfExecSequences;
        }

        private BusinessFlow GetBusinessFlowByName(string name)
        {
            BusinessFlow? bf = WorkSpace
                .Instance
                .SolutionRepository
                .GetAllRepositoryItems<BusinessFlow>()
                .FirstOrDefault(bf => string.Equals(bf.Name, name));

            if (bf == null)
            {
                throw new BPMNExportationException($"No {GingerDicser.GetTermResValue(eTermResKey.BusinessFlows)} found by name '{name}'.");
            }

            return bf;
        }

        private Activity? GetActivityFromSharedRepository(string name)
        {
            return WorkSpace
                .Instance
                .SolutionRepository
                .GetAllRepositoryItems<GingerCore.Activity>()
                .FirstOrDefault(activity => string.Equals(activity.ActivityName, name));
        }

        private Activity? GetActivityFromBusinessFlow(BusinessFlow bf, string activityName)
        {
            return bf
                .Activities
                .FirstOrDefault(activity => string.Equals(activity.ActivityName, activityName));
        }

        private BusinessFlow CreateBusinessFlowFromExecutionData(ExecutedBusinessFlow executedBusinessFlow)
        {
            IEnumerable<ActivitiesGroup> groups = executedBusinessFlow.BusinessFlow
                .ActivitiesGroups
                .Select(group => new ActivitiesGroup()
                {
                    Name = group.Name
                })
                .ToArray();

            foreach (ExecutedActivity executedActivity in executedBusinessFlow.ExecutedActivities)
            {
                ActivitiesGroup group = groups.First(g => string.Equals(g.Name, executedActivity.ExecutedGroupName));

                group.ActivitiesIdentifiers.Add(new()
                {
                    ActivityName = executedActivity.Name,
                });
            }

            IEnumerable<Activity> activities = executedBusinessFlow
                .ExecutedActivities
                .Select(executedActivity => executedActivity.Activity)
                .ToArray();

            return new BusinessFlow()
            {
                Name = executedBusinessFlow.Name,
                ActivitiesGroups = new(groups),
                Activities = new(activities)
            };
        }

        private Task<AccountReportRunSetClient> GetExecutionDataFromAccountReportAsync(string executionId)
        {
            AccountReportApiHandler handler = new(WorkSpace.Instance.Solution.LoggerConfigurations.CentralLoggerEndPointUrl);
            return handler.GetAccountHTMLReportAsync(Guid.Parse(executionId));
        }

        private void ExportBusinessFlow(BusinessFlow businessFlow, string exportPath)
        {
            string fullExportPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(exportPath);
            BusinessFlowToBPMNExporter bpmnExporter = new(
                businessFlow,
                new BusinessFlowToBPMNExporter.Options()
                {
                    ExportPath = fullExportPath,
                    IgnoreGroupWithNoValidActivity = true,
                    GroupConversionOptions = new()
                    {
                        IgnoreInterActivityFlowControls = true
                    }
                });
            bpmnExporter.Export();
        }

        public sealed class ExecutedBusinessFlow
        {
            public BusinessFlow BusinessFlow { get; }

            public string Name => BusinessFlow.Name;

            public IEnumerable<ExecutedActivity> ExecutedActivities { get; }

            public ExecutedBusinessFlow(BusinessFlow businessFlow, IEnumerable<ExecutedActivity> executedActivities)
            {
                BusinessFlow = businessFlow;
                ExecutedActivities = new List<ExecutedActivity>(executedActivities);
            }
        }

        public sealed class ExecutedActivity
        {
            public Activity Activity { get; }

            public string Name => Activity.ActivityName;

            public string ExecutedGroupName { get; }

            public bool ExistInSharedRepository { get; set; }

            public bool ExistInBusinessFlow { get; }

            public ExecutedActivity(GingerCore.Activity activity, string executedGroupName, bool existInSR, bool existInBF)
            {
                Activity = activity;
                ExecutedGroupName = executedGroupName;
                ExistInSharedRepository = existInSR;
                ExistInBusinessFlow = existInBF;
            }
        }
    }
}