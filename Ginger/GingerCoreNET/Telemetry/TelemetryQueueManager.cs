using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Telemetry;
using Ginger.SolutionGeneral;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            AddLog(level, msg, metadata: new());
        }

        public void AddLog(eLogLevel level, string msg, TelemetryMetadata metadata)
        {
            if (level < _minLogTrackingLevel)
            {
                return;
            }

            Solution? solution = WorkSpace.Instance.Solution;

            AddLog(new TelemetryLogRecord()
            {
                SolutionId = solution != null ? solution.Guid.ToString() : "",
                Account = solution != null ? solution.Account : "",
                AppVersion = ApplicationInfo.ApplicationBackendVersion,
                UserId = WorkSpace.Instance.UserProfile.UserName,
                CreationTimestamp = DateTime.UtcNow,
                LastUpdateTimestamp = DateTime.UtcNow,
                Level = level.ToString(),
                Message = msg,
                Metadata = metadata.ToJSON(),
            });
        }

        public void AddFeatureUsage(FeatureId featureId)
        {
            AddFeatureUsage(featureId, metadata: new());
        }

        public void AddFeatureUsage(FeatureId featureId, TelemetryMetadata metadata)
        {

            Solution? solution = WorkSpace.Instance.Solution;

            AddFeatureUsage(new TelemetryFeatureRecord()
            {
                SolutionId = solution != null ? solution.Guid.ToString() : "",
                Account = solution != null ? solution.Account : "",
                AppVersion = ApplicationInfo.ApplicationBackendVersion,
                UserId = WorkSpace.Instance.UserProfile.UserName,
                CreationTimestamp = DateTime.UtcNow,
                LastUpdateTimestamp = DateTime.UtcNow,
                FeatureId = featureId.ToString(),
                Metadata = metadata.ToJSON(),
            });
        }

        public IFeatureTracker StartFeatureTracking(FeatureId featureId)
        {
            return new FeatureTracker(featureId, onStop: AddFeatureUsage);
        }

        private void AddLog(TelemetryLogRecord logRecord)
        {
            _logQueue.Enqueue(logRecord);
        }

        private void AddFeatureUsage(TelemetryFeatureRecord featureRecord)
        {
            _featureQueue.Enqueue(featureRecord);
        }

        private sealed class DebugLogger : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                throw new NotImplementedException();
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
