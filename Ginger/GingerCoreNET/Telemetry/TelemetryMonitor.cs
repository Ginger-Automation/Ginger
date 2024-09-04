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
            TelemetryPipeline<TelemetryLogRecord>.Config<TelemetryLogRecord> options = new()
            {
                Collector = new MockTelemetryCollector<TelemetryLogRecord>(async _ =>
                {
                    await Task.Delay(200);
                    return new ITelemetryCollector<TelemetryLogRecord>.AddResult()
                    {
                        Successful = false,
                    };
                }),
                TelemetryDB = NewTelemetryLiteDB(),
                AddToLocalDBTelemetryStepBufferSize = 1,
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
            TelemetryPipeline<TelemetryFeatureRecord>.Config<TelemetryFeatureRecord> options = new()
            {
                Collector = new MockTelemetryCollector<TelemetryFeatureRecord>(async _ =>
                {
                    await Task.Delay(200);
                    return new ITelemetryCollector<TelemetryFeatureRecord>.AddResult()
                    {
                        Successful = false,
                    };
                }),
                TelemetryDB = NewTelemetryLiteDB(),
                AddToLocalDBTelemetryStepBufferSize = 1,
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
            _logPipeline.Process(logRecord);
        }

        private void AddFeatureUsage(TelemetryFeatureRecord featureRecord)
        {
            _featurePipeline.Process(featureRecord);
        }
    }
}
