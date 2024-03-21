using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run.RemoteExecution;
using Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib;
using Amdocs.Ginger.Repository;
using Ginger.ExecuterService.Contracts.V1.ExecuterHandler.Responses;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.Reports;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Ginger.ExecuterService.Contracts.V1.GingerParser.ParserApiRoutes;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Reports
{
    public sealed class RunsetFromReportLoader
    {
        private readonly JsonSerializerOptions GingerExecConfigSerializationOptions;

        public RunsetFromReportLoader()
        {
            GingerExecConfigSerializationOptions = new();
            GingerExecConfigSerializationOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<RunSetConfig?> LoadAsync(RunSetReport runsetReport)
        {
            string runsetName = runsetReport.Name;
            RunSetConfig? runset = GetRunsetFromSolutionRepository(runsetName);
            
            if (runset == null)
            {
                string executionId = runsetReport.GUID;
                runset = await GetRunsetFromExecutionHandler(executionId);
            }

            return runset;
        }

        private RunSetConfig? GetRunsetFromSolutionRepository(string runsetName)
        {
            return WorkSpace
                .Instance
                .SolutionRepository
                .GetAllRepositoryItems<RunSetConfig>()
                .FirstOrDefault(r => string.Equals(r.Name, runsetName));
        }

        private async Task<RunSetConfig?> GetRunsetFromExecutionHandler(string executionId)
        {
            GingerExecConfig? executionConfig = await GetExecutionConfigurationAsync(executionId);
            if (executionConfig == null)
            {
                return null;
            }
            RunSetConfig runset = CreateVirtualRunset(executionConfig);
            return runset;
        }

        private async Task<GingerExecConfig?> GetExecutionConfigurationAsync(string executionId)
        {
            ExecutionHandlerAPIClient apiClient = new(WorkSpace.Instance.Solution.LoggerConfigurations.ExecutionHandlerURL);
            ExecutionHandlerAPIClient.ExecutionDetailsOptions options = new()
            {
                IncludeRequestDetails = true
            };
            ExecutionDetailsResponse? response = await apiClient.GetExecutionDetailsAsync(executionId, options);

            if (response == null)
            {
                return null;
            }

            GingerExecConfig executionConfig = JsonSerializer.Deserialize<GingerExecConfig>(
                response.RequestDetails.ExecutionConfigurations, 
                GingerExecConfigSerializationOptions)!;
            
            return executionConfig;
        }

        private RunSetConfig CreateVirtualRunset(GingerExecConfig executionConfig)
        {
            RunSetConfig runset = DynamicExecutionManager.LoadRunsetFromExecutionConfig(executionConfig);

            RepositoryFolderBase bfFolder = GetRootRepositoryFolder<BusinessFlow>();
            RepositoryFolderBase bfCacheFolder = GetOrCreateRepositoryFolder(Solution.CacheDirectoryName, bfFolder);
            RepositoryFolderBase bfCacheRunsetFolder = GetOrCreateRepositoryFolder(runset.Name, bfCacheFolder);

            IEnumerable<BusinessFlowRun> bfRuns = runset
                .GingerRunners
                .SelectMany(runner => runner.BusinessFlowsRunList);

            foreach (BusinessFlowRun bfRun in bfRuns)
            {
                BusinessFlow bf = GetBusinessFlowById(bfRun.BusinessFlowGuid);
                MoveRepositoryItemToFolder(bf, bfCacheRunsetFolder.FolderFullPath);
                bf.DynamicPostSaveHandler = () =>
                {
                    RepositoryFolderBase bfRunsetFolder = GetOrCreateRepositoryFolder(runset.Name, bfFolder);
                    MoveRepositoryItemToFolder(bf, bfRunsetFolder.FolderFullPath);
                };
                bf.DirtyStatus = eDirtyStatus.Modified;
            }

            RepositoryFolderBase runsetFolder = GetRootRepositoryFolder<RunSetConfig>();
            RepositoryFolderBase runsetCacheFolder = GetOrCreateRepositoryFolder(Solution.CacheDirectoryName, runsetFolder);
            runsetCacheFolder.AddRepositoryItem(runset, doNotSave: false);

            runset.DynamicPostSaveHandler = () =>
            {
                MoveRepositoryItemToFolder(runset, runsetFolder.FolderFullPath);
            };
            runset.DirtyStatus = eDirtyStatus.Modified;

            return runset;
        }

        private RepositoryFolderBase GetRootRepositoryFolder<T>() where T : RepositoryItemBase
        {
            return WorkSpace
                .Instance
                .SolutionRepository
                .GetSolutionRepositoryItemInfo(typeof(T))
                .ItemRootRepositoryFolder;
        }

        private RepositoryFolderBase GetOrCreateRepositoryFolder(string name, RepositoryFolderBase parentFolder)
        {
            RepositoryFolderBase folder = parentFolder.GetSubFolderByName(name);
            if (folder == null)
            {
                folder = parentFolder.AddSubFolder(name);
            }
            return folder;
        }

        private void MoveRepositoryItemToFolder(RepositoryItemBase itemToMove, string targetFolderFullPath)
        {
            //item already in target folder
            if (string.Equals(itemToMove.ContainingFolderFullPath, targetFolderFullPath))
            {
                return;
            }

            WorkSpace.Instance.SolutionRepository.MoveItem(itemToMove, targetFolderFullPath);
        }

        private BusinessFlow GetBusinessFlowById(Guid id)
        {
            return WorkSpace
                .Instance
                .SolutionRepository
                .GetRepositoryItemByGuid<BusinessFlow>(id);
        }
    }
}
