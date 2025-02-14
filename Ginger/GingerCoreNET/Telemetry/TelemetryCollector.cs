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

using GingerTelemetryProto.V1;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryCollector : ITelemetryCollector<TelemetryLogRecord>, ITelemetryCollector<TelemetryFeatureRecord>
    {
        private readonly GrpcChannel _channel;
        private readonly ILogger? _logger;

        internal TelemetryCollector(string address, ILogger? logger = null)
        {
            _channel = GrpcChannel.ForAddress(address);
            _logger = logger;
        }

        public async Task<ITelemetryCollector<TelemetryLogRecord>.AddResult> AddAsync(IEnumerable<TelemetryLogRecord> logs)
        {
            try
            {
                LogCollector.LogCollectorClient client = new(_channel);
                var request = CreateAddLogsRequest(logs);
                var response = await client.CollectAsync(request);
                return new ITelemetryCollector<TelemetryLogRecord>.AddResult()
                {
                    Successful = true,
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError("unable to send logs to collector\n{ex}", ex);
                return new ITelemetryCollector<TelemetryLogRecord>.AddResult()
                {
                    Successful = false,
                };
            }
        }

        private AddLogsRequest CreateAddLogsRequest(IEnumerable<TelemetryLogRecord> logs)
        {
            AddLogsRequest request = new();
            foreach (var log in logs)
            {
                try
                {
                    request.Logs.Add(new LogRecord()
                    {
                        Id = log.Id,
                        SolutionId = log.SolutionId,
                        Account = log.Account,
                        AppVersion = log.AppVersion,
                        UserId = log.UserId,
                        CreationTimestamp = log.CreationTimestamp.ToString("O"),
                        Level = log.Level,
                        Message = log.Message,
                        Metadata = log.Metadata,
                    });
                }
                catch (Exception ex)
                {
                    _logger?.LogError("Error while creating {logRecord}\n{ex}", nameof(LogRecord), ex);
                }
            }
            return request;
        }

        public async Task<ITelemetryCollector<TelemetryFeatureRecord>.AddResult> AddAsync(IEnumerable<TelemetryFeatureRecord> features)
        {
            try
            {
                FeatureCollector.FeatureCollectorClient client = new(_channel);
                var request = CreateAddFeaturesRequest(features);
                var response = await client.CollectAsync(request);
                return new ITelemetryCollector<TelemetryFeatureRecord>.AddResult()
                {
                    Successful = true,
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError("unable to send logs to collector\n{ex}", ex);
                return new ITelemetryCollector<TelemetryFeatureRecord>.AddResult()
                {
                    Successful = false,
                };
            }
        }

        private AddFeaturesRequest CreateAddFeaturesRequest(IEnumerable<TelemetryFeatureRecord> features)
        {
            AddFeaturesRequest request = new();
            foreach (var feature in features)
            {
                try
                {
                    request.Features.Add(new FeatureRecord()
                    {
                        Id = feature.Id,
                        SolutionId = feature.SolutionId,
                        Account = feature.Account,
                        AppVersion = feature.AppVersion,
                        UserId = feature.UserId,
                        CreationTimestamp = feature.CreationTimestamp.ToString("O"),
                        FeatureId = feature.FeatureId,
                        Duration = feature.Duration != null ? feature.Duration.Value.ToString() : "",
                        Metadata = feature.Metadata,
                    });
                }
                catch (Exception ex)
                {
                    _logger?.LogError("Error while creating {featureRecord}\n{ex}", nameof(FeatureRecord), ex);
                }
            }
            return request;
        }
    }
}
