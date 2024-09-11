using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Telemetry;
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
        private readonly TelemetryQueue<TelemetryLogRecord> _logQueue;
        private readonly TelemetryQueue<TelemetryFeatureRecord> _featureQueue;

        internal TelemetryQueueManager()
        {
            _logQueue = CreateLogQueue();
            _featureQueue = CreateFeatureQueue();
        }

        private static TelemetryQueue<TelemetryLogRecord> CreateLogQueue()
        {
            TelemetryQueue<TelemetryLogRecord>.Config config = new()
            {
                BufferSize = 4,
                DB = NewTelemetryLiteDB(),
                Collector = new MockTelemetryCollector<TelemetryLogRecord>(async _ =>
                {
                    await Task.Delay(200);
                    return new ITelemetryCollector<TelemetryLogRecord>.AddResult()
                    {
                        Successful = false,
                    };
                }),
                RetryPollingInterval = TimeSpan.FromSeconds(30),
                RetryPollingSize = 10,
                Logger = new DebugLogger(),
            };
            return new TelemetryQueue<TelemetryLogRecord>(config);
        }

        private static TelemetryQueue<TelemetryFeatureRecord> CreateFeatureQueue()
        {
            TelemetryQueue<TelemetryFeatureRecord>.Config config = new()
            {
                BufferSize = 4,
                DB = NewTelemetryLiteDB(),
                Collector = new MockTelemetryCollector<TelemetryFeatureRecord>(async _ =>
                {
                    await Task.Delay(200);
                    return new ITelemetryCollector<TelemetryFeatureRecord>.AddResult()
                    {
                        Successful = false,
                    };
                }),
                RetryPollingInterval = TimeSpan.FromSeconds(30),
                RetryPollingSize = 10,
                Logger = new DebugLogger(),
            };
            return new TelemetryQueue<TelemetryFeatureRecord>(config);
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
            if (level != eLogLevel.ERROR)
            {
                return;
            }

            AddLog(new TelemetryLogRecord()
            {
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
            AddFeatureUsage(new TelemetryFeatureRecord()
            {
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
