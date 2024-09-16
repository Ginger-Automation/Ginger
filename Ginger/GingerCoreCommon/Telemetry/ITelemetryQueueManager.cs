using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Telemetry
{
    public interface ITelemetryQueueManager : IDisposable
    {
        public sealed class Config : RepositoryItemBase
        {
            [IsSerializedForLocalRepository]
            public string CollectorURL { get; set; } = string.Empty;
            
            [IsSerializedForLocalRepository(DefaultValue: 10)]
            public int BufferSize { get; set; } = 10;

            [IsSerializedForLocalRepository(DefaultValue: 1800)]
            public int RetryIntervalInSeconds { get; set; } = 1800;

            [IsSerializedForLocalRepository(DefaultValue: 10)]
            public int RetryPollingSize { get; set; } = 10;

            [IsSerializedForLocalRepository(DefaultValue: eLogLevel.ERROR)]
            public eLogLevel MinLogLevel { get; set; } = eLogLevel.ERROR;

            public override string ItemName { get; set; }
        }

        public void AddLog(eLogLevel level, string msg);

        public void AddLog(eLogLevel level, string msg, TelemetryMetadata metadata);

        public void AddFeatureUsage(FeatureId featureId);

        public void AddFeatureUsage(FeatureId featureId, TelemetryMetadata metadata);

        public IFeatureTracker StartFeatureTracking(FeatureId featureId);
    }
}
