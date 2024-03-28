using Ginger.ExecuterService.Contracts.V1.ExecuterHandler.Requests;
using Ginger.ExecuterService.Contracts.V1.ExecuterHandler.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
