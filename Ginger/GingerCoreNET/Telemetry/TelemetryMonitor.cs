using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.CoreNET.Telemetry.Pipeline;
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
    internal sealed class TelemetryMonitor : ITelemetryMonitor
    {
        private readonly TelemetryPipeline<TelemetryLogRecord> _logPipeline;
        private readonly TelemetryPipeline<TelemetryFeatureRecord> _featurePipeline;

        internal TelemetryMonitor()
        {
            _logPipeline = CreateLogPipeline();
            _featurePipeline = CreateFeaturePipeline();
        }

        private static TelemetryPipeline<TelemetryLogRecord> CreateLogPipeline()
        {
            TelemetryPipeline<TelemetryLogRecord>.Options<TelemetryLogRecord> options = new()
            {
                Collector = new MockTelemetryCollector<TelemetryLogRecord>(_ => Task.FromResult(new ITelemetryCollector<TelemetryLogRecord>.AddResult()
                {
                    Successful = true,
                })),
                TelemetryDB = NewTelemetryLiteDB(),
                AddToLocalDBTelemetryStepBufferSize = 4,
                SendToCollectorTelemetryStepBufferSize = 4,
                DeleteFromLocalDBTelemetryStepBufferSize = 4,
                MarkUnsuccessfulInLocalDBTelemetryStepBufferSize = 4,
                RetryServicePollingInterval = TimeSpan.FromSeconds(30),
                RetryServicePollingSize = 10,
                Logger = null,
            };
            return new TelemetryPipeline<TelemetryLogRecord>(options);
        }

        private static TelemetryPipeline<TelemetryFeatureRecord> CreateFeaturePipeline()
        {
            TelemetryPipeline<TelemetryFeatureRecord>.Options<TelemetryFeatureRecord> options = new()
            {
                Collector = new MockTelemetryCollector<TelemetryFeatureRecord>(_ => Task.FromResult(new ITelemetryCollector<TelemetryFeatureRecord>.AddResult()
                {
                    Successful = true,
                })),
                TelemetryDB = NewTelemetryLiteDB(),
                AddToLocalDBTelemetryStepBufferSize = 4,
                SendToCollectorTelemetryStepBufferSize = 4,
                DeleteFromLocalDBTelemetryStepBufferSize = 4,
                MarkUnsuccessfulInLocalDBTelemetryStepBufferSize = 4,
                RetryServicePollingInterval = TimeSpan.FromSeconds(30),
                RetryServicePollingSize = 10,
                Logger = null,
            };
            return new TelemetryPipeline<TelemetryFeatureRecord>(options);
        }

        private static TelemetryLiteDB NewTelemetryLiteDB()
        {
            string userProfileAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string telemetryDBFilePath = Path.Combine(userProfileAppDataDir, "Amdocs", "Ginger", "telemetry.db");
            return new TelemetryLiteDB(telemetryDBFilePath);
        }

        public void Dispose()
        {
            _logPipeline.Dispose();
            _featurePipeline.Dispose();
        }

        public void AddLog(eLogLevel level, string msg)
        {
            AddLog(level, msg, attributes: []);
        }

        public void AddLog(eLogLevel level, string msg, Dictionary<string,string> attributes)
        {
            if (level != eLogLevel.ERROR)
            {
                return;
            }

            AddLog(new TelemetryLogRecord()
            {
                AppVersion = ApplicationInfo.ApplicationBackendVersion,
                CreationTimestamp = DateTime.UtcNow,
                LastUpdateTimestamp = DateTime.UtcNow,
                Level = level.ToString(),
                UserId = WorkSpace.Instance.UserProfile.UserName,
                Message = msg,
                Attributes = attributes,
            });
        }

        public IFeatureTracker StartFeatureTracking(FeatureId featureId)
        {
            return new FeatureTracker(featureId, AddFeatureUsage);
        }

        private void AddLog(TelemetryLogRecord logRecord)
        {
            _logPipeline.Process(logRecord);
        }

        private void AddFeatureUsage(TelemetryFeatureRecord featureRecord)
        {
            _featurePipeline.Process(featureRecord);
        }
    }
}
