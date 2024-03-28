using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET.Run.RemoteExecution;
using Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib;
using Amdocs.Ginger.Repository;
using Ginger.ExecuterService.Contracts.V1.ExecuterHandler.Responses;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Reports
{
    public sealed class RunsetFromReportLoader : IDisposable
    {
        private readonly IExecutionHandlerAPIClient _executionHandlerAPIClient;

        public RunsetFromReportLoader()
        {
            _executionHandlerAPIClient = new ExecutionHandlerAPIClient();
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
            string handlerAPIUrl = WorkSpace.Instance.Solution.LoggerConfigurations.ExecutionHandlerURL;
            if (string.IsNullOrEmpty(handlerAPIUrl))
            {
                //case: URL is null or empty
                throw new InvalidOperationException($"Please make sure that the Execution Handler URL on the 'Execution Logger Configurations' page under 'Reports' tab is entered correctly.");
            }

            _executionHandlerAPIClient.URL = handlerAPIUrl;
            ExecutionHandlerAPIClient.ExecutionDetailsOptions options = new()
            {
                IncludeRequestDetails = true
            };
            ExecutionDetailsResponse? response;
            try
            {
                response = await _executionHandlerAPIClient.GetExecutionDetailsAsync(executionId, options);
            }
            catch(InvalidOperationException)
            {
                //case: URL is invalid like, someDummyText
                throw new InvalidOperationException($"Please make sure that the Execution Handler URL on the 'Execution Logger Configurations' page under 'Reports' tab is entered correctly.");
            }
            catch(Exception)
            {
                //case: URL has invalid host name like, https://someDummyHost/ExecuterHandlerService
                //default case: Any other form of exception
                throw new InvalidOperationException($"Unable to get details from Execution Handler.");
            }

            if (response == null)
            {
                return null;
            }

            GingerExecConfig executionConfig = JsonSerializer.Deserialize<GingerExecConfig>(
                response.RequestDetails.ExecutionConfigurations, 
                new JsonSerializerOptions()
                {
                    Converters = { new JsonStringEnumConverter() }
                })!;
            
            return executionConfig;
        }

        private RunSetConfig CreateVirtualRunset(GingerExecConfig executionConfig)
        {
            RunSetConfig runset = DynamicExecutionManager.LoadRunsetFromExecutionConfig(executionConfig);
            runset.IsVirtual = true;

            RepositoryFolderBase bfFolder = GetRootRepositoryFolder<BusinessFlow>();
            RepositoryFolderBase bfCacheFolder = GetOrCreateRepositoryFolder(ISolution.CacheDirectoryName, bfFolder);
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
            RepositoryFolderBase runsetCacheFolder = GetOrCreateRepositoryFolder(ISolution.CacheDirectoryName, runsetFolder);
            runsetCacheFolder.AddRepositoryItem(runset, doNotSave: false);

            runset.DynamicPostSaveHandler = () =>
            {
                MoveRepositoryItemToFolder(runset, runsetFolder.FolderFullPath);
                runset.IsVirtual = false;
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

        public void Dispose()
        {
            _executionHandlerAPIClient.Dispose();
        }
    }
}
