using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.CoreNET.Telemetry.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryMonitor : ITelemetryMonitor
    {
        private readonly AddToLocalDBTelemetryStep<TelemetryLogRecord> _logPipelineEntry;
        private readonly AddToLocalDBTelemetryStep<TelemetryFeatureRecord> _featurePipelineEntry;

        internal TelemetryMonitor(
            ITelemetryCollector<TelemetryLogRecord> logCollector, ITelemetryCollector<TelemetryFeatureRecord> featureCollector, 
            ITelemetryDB<TelemetryLogRecord> logDB, ITelemetryDB<TelemetryFeatureRecord> featureDB, 
            ILogger? logger = null)
        {
            _logPipelineEntry = CreatePipeline(logCollector, logDB, logger);
            _featurePipelineEntry = CreatePipeline(featureCollector, featureDB, logger);
        }

        private static AddToLocalDBTelemetryStep<TRecord> CreatePipeline<TRecord>(ITelemetryCollector<TRecord> collector, ITelemetryDB<TRecord> db, ILogger? logger)
        {
            DeleteFromLocalDBTelemetryStep<TRecord> deleteFromLocalDBTelemetryStep = new(bufferSize: 4, db, logger);
            MarkUnsuccessfulInLocalDBTelemetryStep<TRecord> markUnsuccessfulInLocalDBTelemetryStep = new(bufferSize: 4, db, logger);
            SendToCollectorTelemetryStep<TRecord> sendToCollectorTelemetryStep = new(bufferSize: 4, collector, deleteFromLocalDBTelemetryStep, markUnsuccessfulInLocalDBTelemetryStep, logger);
            AddToLocalDBTelemetryStep<TRecord> addToLocalDBTelemetryStep = new(bufferSize: 4, db, sendToCollectorTelemetryStep, logger);

            deleteFromLocalDBTelemetryStep.StartConsumer();
            markUnsuccessfulInLocalDBTelemetryStep.StartConsumer();
            sendToCollectorTelemetryStep.StartConsumer();
            addToLocalDBTelemetryStep.StartConsumer();

            return addToLocalDBTelemetryStep;
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
            _logPipelineEntry.Process(logRecord);
        }

        private void AddFeatureUsage(TelemetryFeatureRecord featureRecord)
        {
            _featurePipelineEntry.Process(featureRecord);
        }
    }
}
