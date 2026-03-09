#region License
/*
Copyright © 2014-2026 European Support Limited

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

using AccountReport.Contracts.ResponseModels;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using Amdocs.Ginger.CoreNET.External.GingerPlay;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger;
using GingerCore;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ginger.Reports.ExecutionLoggerConfiguration;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN.Exportation
{
    public sealed class RunSetExecutionHistoryToBPMNExporter
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RunSetExecutionHistoryToBPMNExporter() { }

        /// <summary>
        /// Retrieve a collection of <see cref="ExecutedBusinessFlow"/> based on the specified execution ID and data repository source.<br/>
        /// <b>NOTE:</b> This method is not supported for <see cref="DataRepositoryMethod.TextFile"/>
        /// </summary>
        /// <param name="executionId">The execution ID of the runset for which to retrieve executed business flows.</param>
        /// <param name="source">The source from where to fetch the executed business flows. Can be either <see cref="DataRepositoryMethod.LiteDB"/> or <see cref="DataRepositoryMethod.Remote"/>.</param>
        /// <returns>Collection of <see cref="ExecutedBusinessFlow"/> for the specific runset execution.</returns>
        /// <exception cref="ArgumentException">If the provided <see cref="DataRepositoryMethod"/> is not supported.</exception>
        public Task<IEnumerable<ExecutedBusinessFlow>> GetExecutedBusinessFlowsAsync(string executionId, DataRepositoryMethod source)
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
                throw new ArgumentException($"Generation of BPMN from {GingerDicser.GetTermResValue(eTermResKey.RunSet)} execution history with report method '{source}' is not supported.");
            }
        }

        /// <summary>
        /// Export a given <see cref="ExecutedBusinessFlow"/> to a specified path. 
        /// </summary>
        /// <param name="executedBusinessFlow">The <see cref="ExecutedBusinessFlow"/> to be exported.</param>
        /// <param name="exportPath">The file system path where the BPMN file should be exported.</param>
        /// <exception cref="BPMNExportationException">If any <see cref="Activity"/> is not available in Shared Repository.</exception>
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

        /// <summary>
        /// Retrieve collection of <see cref="ExecutedBusinessFlow"/> from LiteDB based on the <paramref name="executionId"/>.
        /// </summary>
        /// <param name="executionId">The execution ID for which to retrieve collection of <see cref="ExecutedBusinessFlow"/> from LiteDB.</param>
        /// <returns>Collection of <see cref="ExecutedBusinessFlow"/> for the given <paramref name="executionId"/>.</returns>
        private IEnumerable<ExecutedBusinessFlow> GetExecutedBusinessFlowsFromLiteDb(string executionId)
        {
            LiteDbManager liteDbManager = new(new ExecutionLoggerHelper().GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder));
            LiteDbRunSet liteRunSet = liteDbManager.GetLatestExecutionRunsetData(executionId);
            IEnumerable<LiteDbBusinessFlow> liteBFs = liteRunSet
                .RunnersColl
                .SelectMany(liteRunner => liteRunner.AllBusinessFlowsColl);

            List<ExecutedBusinessFlow> executedBusinessFlows = [];

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

                executedBusinessFlows.Add(new(bf, activities));
            }

            return executedBusinessFlows;
        }

        /// <summary>
        /// Retrieve collection of <see cref="ExecutedBusinessFlow"/> from a remote source based on the <paramref name="executionId"/>.
        /// </summary>
        /// <param name="executionId">The execution ID for which to retrieve collection of <see cref="ExecutedBusinessFlow"/> from a remote.</param>
        /// <returns>Collection of <see cref="ExecutedBusinessFlow"/> for the given <paramref name="executionId"/>.</returns>
        private async Task<IEnumerable<ExecutedBusinessFlow>> GetExecutedBusinessFlowsFromRemoteAsync(string executionId)
        {
            List<ExecutedBusinessFlow> executedBusinessFlows = [];

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

                    executedBusinessFlows.Add(new(bf, executedActivities));
                }
            }

            return executedBusinessFlows;
        }

        /// <summary>
        /// Retrieve a <see cref="BusinessFlow"/> by it's name from the solution.
        /// </summary>
        /// <param name="name">The name of the <see cref="BusinessFlow"/> to retrieve.</param>
        /// <returns><see cref="BusinessFlow"/> matching the given <paramref name="name"/>.</returns>
        /// <exception cref="BPMNExportationException">If no matching <see cref="BusinessFlow"/> is found.</exception>
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

        /// <summary>
        /// Retrieve an <see cref="Activity"/> by it's name from the shared repository.
        /// </summary>
        /// <param name="name">The name of the <see cref="Activity"/> to retrieve.</param>
        /// <returns><see cref="Activity"/> matching the given <paramref name="name"/> or <see langword="null"/> if no match is found.</returns>
        private Activity? GetActivityFromSharedRepository(string name)
        {
            return WorkSpace
                .Instance
                .SolutionRepository
                .GetAllRepositoryItems<GingerCore.Activity>()
                .FirstOrDefault(activity => string.Equals(activity.ActivityName, name));
        }

        /// <summary>
        /// Retrieve an <see cref="Activity"/> by it's name from the given <see cref="BusinessFlow"/>.
        /// </summary>
        /// <param name="bf">The <see cref="BusinessFlow"/> from which to retrieve the <see cref="Activity"/>.</param>
        /// <param name="activityName">The name of the <see cref="Activity"/> to retrieve.</param>
        /// <returns><see cref="Activity"/> matching the given <paramref name="name"/> or <see langword="null"/> if no match is found.</returns>
        private Activity? GetActivityFromBusinessFlow(BusinessFlow bf, string activityName)
        {
            return bf
                .Activities
                .FirstOrDefault(activity => string.Equals(activity.ActivityName, activityName));
        }

        /// <summary>
        /// Create a new <see cref="BusinessFlow"/> instance based on the execution data of an <see cref="ExecutedBusinessFlow"/>.
        /// </summary>
        /// <param name="executedBusinessFlow">The <see cref="ExecutedBusinessFlow"/> from which to create a new <see cref="BusinessFlow"/> instance.</param>
        /// <returns><see cref="BusinessFlow"/> based on the given <paramref name="executedBusinessFlow"/>.</returns>
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

        /// <summary>
        /// Retrieve execution data from the account report API based on the <paramref name="executionId"/>.
        /// </summary>
        /// <param name="executionId">The execution ID for which to retrieve execution data from the account report.</param>
        /// <returns><see cref="AccountReportRunSetClient"/> containing the execution details.</returns>
        private Task<AccountReportRunSetClient> GetExecutionDataFromAccountReportAsync(string executionId)
        {
            AccountReportApiHandler handler = new(GingerPlayEndPointManager.GetAccountReportServiceUrl());
            return handler.GetAccountHTMLReportAsync(Guid.Parse(executionId));
        }

        /// <summary>
        /// Export a given <see cref="BusinessFlow"/> to a specified path.
        /// </summary>
        /// <param name="businessFlow">The <see cref="BusinessFlow"/> to be exported.</param>
        /// <param name="exportPath">The file system path where the BPMN file should be exported.</param>
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