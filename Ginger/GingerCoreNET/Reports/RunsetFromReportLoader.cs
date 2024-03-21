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

#nullable enable
namespace Amdocs.Ginger.CoreNET.Reports
{
    public sealed class RunsetFromReportLoader
    {
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

            JsonSerializerOptions serializerOptions = new();
            serializerOptions.Converters.Add(new JsonStringEnumConverter());
            GingerExecConfig executionConfig = JsonSerializer.Deserialize<GingerExecConfig>(response.RequestDetails.ExecutionConfigurations, serializerOptions)!;

            RunSetConfig runset = DynamicExecutionManager.LoadRunsetFromExecutionConfig(executionConfig);

            RepositoryFolderBase bfFolder = WorkSpace.Instance.SolutionRepository.GetSolutionRepositoryItemInfo(typeof(BusinessFlow)).ItemRootRepositoryFolder;
            RepositoryFolderBase? bfCacheFolder = bfFolder.GetSubFolderByName(Solution.CacheDirectoryName);
            if (bfCacheFolder == null)
            {
                bfCacheFolder = bfFolder.AddSubFolder(Solution.CacheDirectoryName);
            }
            RepositoryFolderBase? runsetCacheBfFolder = bfCacheFolder.GetSubFolderByName(runset.Name);
            if (runsetCacheBfFolder == null)
            {
                runsetCacheBfFolder = bfCacheFolder.AddSubFolder(runset.Name);
            }
            foreach (BusinessFlowRun bfRun in runset.GingerRunners.SelectMany(runner => runner.BusinessFlowsRunList))
            {
                BusinessFlow bf = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(bfRun.BusinessFlowGuid);
                WorkSpace.Instance.SolutionRepository.MoveItem(bf, runsetCacheBfFolder.FolderFullPath);
                bf.DynamicPostSaveHandler = () =>
                {
                    RepositoryFolderBase? runsetBfFolder = bfFolder.GetSubFolderByName(runset.Name);
                    if (runsetBfFolder == null)
                    {
                        runsetBfFolder = bfFolder.AddSubFolder(runset.Name);
                    }
                    if (!string.Equals(bf.ContainingFolder, runsetBfFolder.FolderRelativePath))
                    {
                        WorkSpace.Instance.SolutionRepository.MoveItem(bf, runsetBfFolder.FolderFullPath);
                    }
                };
                bf.DirtyStatus = eDirtyStatus.Modified;
            }

            RepositoryFolderBase runsetFolder = WorkSpace.Instance.SolutionRepository.GetSolutionRepositoryItemInfo(typeof(RunSetConfig)).ItemRootRepositoryFolder;
            RepositoryFolderBase? runsetCacheFolder = runsetFolder.GetSubFolderByName(Solution.CacheDirectoryName, recursive: false);
            if (runsetCacheFolder == null)
            {
                runsetCacheFolder = runsetFolder.AddSubFolder(Solution.CacheDirectoryName);
            }
            runsetCacheFolder.AddRepositoryItem(runset, doNotSave: false);

            runset.DynamicPostSaveHandler = () =>
            {
                if (!string.Equals(runset.ContainingFolder, runsetFolder.FolderRelativePath))
                {
                    WorkSpace.Instance.SolutionRepository.MoveItem(runset, runsetFolder.FolderFullPath);
                }
            };
            runset.DirtyStatus = eDirtyStatus.Modified;

            return runset;
        }
    }
}
