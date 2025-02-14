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

using Ginger.ExecuterService.Contracts.V1.ExecuterHandler.Requests;
using Ginger.ExecuterService.Contracts.V1.ExecuterHandler.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Amdocs.Ginger.CoreNET.Run.RemoteExecution.ExecutionHandlerAPIClient;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Run.RemoteExecution
{
    public interface IExecutionHandlerAPIClient : IDisposable
    {
        public string URL { get; set; }

        public Task<ExecutionDetailsResponse?> GetExecutionDetailsAsync(string executionId, ExecutionDetailsOptions options);

        public Task<IEnumerable<ExecutionDetailsResponse?>> GetExecutionDetailsAsync(IEnumerable<string> executionIds, ExecutionDetailsOptions options);

        public Task<bool> StartExectuionAsync(AddExecutionRequest executionRequest);
    }
}
