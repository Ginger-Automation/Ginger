using GingerTelemetryProto.V1;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryCollector : ITelemetryCollector<TelemetryLogRecord>, ITelemetryCollector<TelemetryFeatureRecord>
    {
        private static readonly GrpcChannel CollectorGRPCChannel = GrpcChannel.ForAddress("");
        private readonly ILogger? _logger;

        internal TelemetryCollector(ILogger? logger = null)
        {
            _logger = logger;
        }

        public async Task<ITelemetryCollector<TelemetryLogRecord>.AddResult> AddAsync(IEnumerable<TelemetryLogRecord> logs)
        {
            try
            {
                LogCollector.LogCollectorClient client = new(CollectorGRPCChannel);
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
                request.LogCount++;
            }
            return request;
        }

        public async Task<ITelemetryCollector<TelemetryFeatureRecord>.AddResult> AddAsync(IEnumerable<TelemetryFeatureRecord> features)
        {
            try
            {
                FeatureCollector.FeatureCollectorClient client = new(CollectorGRPCChannel);
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
                request.FeatureCount++;
            }
            return request;
        }
    }
}
