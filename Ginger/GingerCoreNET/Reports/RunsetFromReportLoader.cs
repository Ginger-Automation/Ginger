#region License
/*
Copyright © 2014-2025 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.GenAIServices;
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

        public async Task<RunSetConfig?> LoadAsync(RunSetReport runsetReport, Uri? executionConfigurationSourceUrl = null)
        {
            string executionId = runsetReport.GUID;
            RunSetConfig? runset = null;
            Guid runsetId = Guid.Empty;
            if (runsetReport.RunSetGuid != Guid.Empty)
            {
                runsetId = runsetReport.RunSetGuid;
                runset = GetRunsetFromSolutionRepository(runsetId);
            }

            if (runset == null)
            {
                runset = await GetRunsetFromSourceAsync(executionId, executionConfigurationSourceUrl);
            }

            if (runset != null && runset.IsVirtual)
            {
                runset.Guid = runsetId;
                string exectionTime = "";
                if (runsetReport.StartTimeStamp != default(DateTime))
                {
                    exectionTime = $"\nExecutionTime: {runsetReport.StartTimeStamp}";
                }
                runset.Description = $"ExecutionId: {runsetReport.GUID}{exectionTime}";
            }

            return runset;
        }

        private RunSetConfig? GetRunsetFromSolutionRepository(Guid runsetId)
        {
            return WorkSpace
                .Instance
                .SolutionRepository
                .GetRepositoryItemByGuid<RunSetConfig>(runsetId);
        }

        private async Task<RunSetConfig?> GetRunsetFromSourceAsync(string executionId, Uri? executionConfigSourceUrl)
        {
            GingerExecConfig? executionConfig = executionConfigSourceUrl != null
                ? await GetExecutionConfigurationFromUrlAsync(executionConfigSourceUrl)
                : await GetExecutionConfigurationFromExecutionHandlerAsync(executionId);

            if (executionConfig == null)
            {
                return null;
            }

            return CreateVirtualRunset(executionConfig);
        }

        private async Task<GingerExecConfig?> GetExecutionConfigurationFromUrlAsync(Uri execConfigSourceUrl)
        {
            try
            {
                var (responseContent, statusCode) = await HttpUtilities.GetAsync(execConfigSourceUrl);

                if (string.IsNullOrEmpty(responseContent))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<GingerExecConfig>(responseContent, new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving execution configuration from {execConfigSourceUrl}");
            }
        }

        private async Task<GingerExecConfig?> GetExecutionConfigurationFromExecutionHandlerAsync(string executionId)
        {
            string handlerAPIUrl = GingerPlayEndPointManager.GetExecutionServiceUrl();
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
            catch (InvalidOperationException)
            {
                //case: URL is invalid like, someDummyText
                throw new InvalidOperationException($"Please make sure that the Execution Handler URL on the 'Execution Logger Configurations' page under 'Reports' tab is entered correctly.");
            }
            catch (Exception)
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
            executionConfig.ExecutionID = response.Id;

            return executionConfig;
        }

        private RunSetConfig CreateVirtualRunset(GingerExecConfig executionConfig)
        {
            RunSetConfig runset = DynamicExecutionManager.LoadRunsetFromExecutionConfig(executionConfig);
            runset.IsVirtual = true;
            runset.AllowAutoSave = false;

            runset.Name = GetUniqueRunsetName(runset.Name);

            RepositoryFolderBase bfFolder = GetRootRepositoryFolder<BusinessFlow>();

            IEnumerable<BusinessFlowRun> bfRuns = runset
                .GingerRunners
                .SelectMany(runner => runner.BusinessFlowsRunList);

            foreach (BusinessFlowRun bfRun in bfRuns)
            {
                BusinessFlow bf = GetBusinessFlowById(bfRun.BusinessFlowGuid);
                if (bf.IsVirtual)
                {
                    RepositoryFolderBase bfCacheFolder = GetOrCreateRepositoryFolder(ISolution.CacheDirectoryName, bfFolder);
                    RepositoryFolderBase bfCacheRunsetFolder = GetOrCreateRepositoryFolder(runset.Name, bfCacheFolder);
                    bf.AllowAutoSave = false;
                    MoveRepositoryItemToFolder(bf, bfCacheRunsetFolder.FolderFullPath);
                    bf.DynamicPostSaveHandler = () =>
                    {
                        RepositoryFolderBase bfRunsetFolder = GetOrCreateRepositoryFolder(runset.Name, bfFolder);
                        bf.AllowAutoSave = true;
                        MoveRepositoryItemToFolder(bf, bfRunsetFolder.FolderFullPath);
                    };
                    bf.DirtyStatus = eDirtyStatus.Modified;
                }

            }
            RepositoryFolderBase runsetFolder = GetRootRepositoryFolder<RunSetConfig>();
            RepositoryFolderBase runsetCacheFolder = GetOrCreateRepositoryFolder(ISolution.CacheDirectoryName, runsetFolder);
            runsetCacheFolder.AddRepositoryItem(runset, doNotSave: false);

            runset.DynamicPostSaveHandler = () =>
            {
                MoveRepositoryItemToFolder(runset, runsetFolder.FolderFullPath);
                runset.IsVirtual = false;
                runset.AllowAutoSave = true;
            };
            runset.DirtyStatus = eDirtyStatus.Modified;

            return runset;
        }

        private string GetUniqueRunsetName(string runsetName)
        {
            bool RunsetExist(string runsetName)
            {
                return WorkSpace
                    .Instance
                    .SolutionRepository
                    .GetAllRepositoryItems<RunSetConfig>()
                    .Any(runset => string.Equals(runset.Name, runsetName));
            }

            int copyCount = 0;
            string copyIdentifier = string.Empty;
            const int MaxAttempts = 10_000;
            while (RunsetExist($"{runsetName}{copyIdentifier}") && copyCount < MaxAttempts)
            {
                copyCount++;
                if (copyCount == 1)
                {
                    copyIdentifier = "-Copy";
                }
                else
                {
                    copyIdentifier = $"-Copy{copyCount}";
                }
            }

            if (copyCount >= MaxAttempts)
            {
                throw new Exception($"Too many {GingerDicser.GetTermResValue(eTermResKey.RunSets)} with similar name, remove/delete them first.");
            }

            return $"{runsetName}{copyIdentifier}";
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
