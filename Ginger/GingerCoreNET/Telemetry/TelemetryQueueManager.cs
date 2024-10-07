#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Telemetry;
using Ginger.SolutionGeneral;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryQueueManager : ITelemetryQueueManager
    {
        private readonly eLogLevel _minLogTrackingLevel;
        private readonly TelemetryQueue<TelemetryLogRecord> _logQueue;
        private readonly TelemetryQueue<TelemetryFeatureRecord> _featureQueue;

        internal TelemetryQueueManager(ITelemetryQueueManager.Config config)
        {
            _minLogTrackingLevel = config.MinLogLevel;
            _logQueue = CreateLogQueue(config);
            _featureQueue = CreateFeatureQueue(config);
        }

        private static TelemetryQueue<TelemetryLogRecord> CreateLogQueue(ITelemetryQueueManager.Config config)
        {
            TelemetryCollector? collector = null;
            try
            {
                collector = new(config.CollectorURL, new DebugLogger());
            }
            catch { }

            TelemetryQueue<TelemetryLogRecord>.Config queueConfig = new()
            {
                BufferSize = config.BufferSize,
                DB = NewTelemetryLiteDB(),
                Collector = collector != null ? collector : NewMockCollector<TelemetryLogRecord>(),
                RetryPollingInterval = TimeSpan.FromSeconds(config.RetryIntervalInSeconds),
                RetryPollingSize = config.RetryPollingSize,
                Logger = new DebugLogger(),
            };
            return new TelemetryQueue<TelemetryLogRecord>(queueConfig);
        }

        private static TelemetryQueue<TelemetryFeatureRecord> CreateFeatureQueue(ITelemetryQueueManager.Config config)
        {
            TelemetryCollector? collector = null;
            try
            {
                collector = new(config.CollectorURL, new DebugLogger());
            }
            catch { }

            TelemetryQueue<TelemetryFeatureRecord>.Config queueConfig = new()
            {
                BufferSize = config.BufferSize,
                DB = NewTelemetryLiteDB(),
                Collector = collector != null ? collector : NewMockCollector<TelemetryFeatureRecord>(),
                RetryPollingInterval = TimeSpan.FromSeconds(config.RetryIntervalInSeconds),
                RetryPollingSize = config.RetryPollingSize,
                Logger = new DebugLogger(),
            };
            return new TelemetryQueue<TelemetryFeatureRecord>(queueConfig);
        }

        private static MockTelemetryCollector<TRecord> NewMockCollector<TRecord>()
        {
            return new MockTelemetryCollector<TRecord>(async _ =>
            {
                await Task.Delay(200);
                return new ITelemetryCollector<TRecord>.AddResult()
                {
                    Successful = false,
                };
            });
        }

        private static TelemetryLiteDB NewTelemetryLiteDB()
        {
            string userProfileAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string telemetryDBFilePath = Path.Combine(userProfileAppDataDir, "Amdocs", "Ginger", "telemetry.db");
            return new TelemetryLiteDB(telemetryDBFilePath);
        }

        public void Dispose()
        {
            _logQueue.Dispose();
            _featureQueue.Dispose();
        }

        public void AddLog(eLogLevel level, string msg)
        {
            AddLog(level, msg, metadata: []);
        }

        public void AddLog(eLogLevel level, string msg, TelemetryMetadata metadata)
        {
            if (level < _minLogTrackingLevel)
            {
                return;
            }

            Solution? solution = WorkSpace.Instance?.Solution;
            string? userId = WorkSpace.Instance?.UserProfile.UserName;

            TelemetryLogRecord logRecord = new()
            {
                SolutionId = solution != null ? solution.Guid.ToString() : "",
                Account = solution != null && solution.Account != null ? solution.Account : "",
                AppVersion = ApplicationInfo.ApplicationBackendVersion,
                UserId = userId,
                CreationTimestamp = DateTime.UtcNow,
                LastUpdateTimestamp = DateTime.UtcNow,
                Level = level.ToString(),
                Message = msg,
                Metadata = metadata.ToJSON(),
            };

            _logQueue.Enqueue(logRecord);
        }

        public void AddFeatureUsage(FeatureId featureId)
        {
            AddFeatureUsage(featureId, duration: null, metadata: []);
        }

        public void AddFeatureUsage(FeatureId featureId, TelemetryMetadata metadata)
        {
            AddFeatureUsage(featureId, duration: null, metadata);
        }

        private void AddFeatureUsage(FeatureId featureId, TimeSpan? duration, TelemetryMetadata metadata)
        {
            Solution? solution = WorkSpace.Instance.Solution;

            TelemetryFeatureRecord featureRecord = new()
            {
                SolutionId = solution != null ? solution.Guid.ToString() : "",
                Account = solution != null && solution.Account != null ? solution.Account : "",
                AppVersion = ApplicationInfo.ApplicationBackendVersion,
                UserId = WorkSpace.Instance.UserProfile.UserName,
                CreationTimestamp = DateTime.UtcNow,
                LastUpdateTimestamp = DateTime.UtcNow,
                FeatureId = featureId.ToString(),
                Duration = duration,
                Metadata = metadata.ToJSON(),
            };

            _featureQueue.Enqueue(featureRecord);
        }

        public IFeatureTracker StartFeatureTracking(FeatureId featureId)
        {
            return new FeatureTracker(
                featureId,
                onStop: (featureId, duration, metadata) => AddFeatureUsage(featureId, duration, metadata));
        }

        private sealed class DebugLogger : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                System.Diagnostics.Debug.WriteLine($"[{logLevel}]: {formatter(state, exception)}");
            }
        }
    }
}
