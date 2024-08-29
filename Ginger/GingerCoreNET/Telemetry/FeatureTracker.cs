using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class FeatureTracker : IFeatureTracker
    {
        private readonly long _startTime;
        private readonly Action<TelemetryFeatureRecord> _onStop;

        public FeatureId FeatureId { get; }
        
        public TelemetryMetadata Metadata { get; }

        internal FeatureTracker(FeatureId featureId, Action<TelemetryFeatureRecord> onStop)
        {
            _startTime = DateTime.UtcNow.Ticks;
            _onStop = onStop;
            FeatureId = featureId;
            Metadata = new();
        }

        public void StopTracking()
        {
            _onStop(new TelemetryFeatureRecord()
            {
                AppVersion = ApplicationInfo.ApplicationBackendVersion,
                CreationTimestamp = DateTime.UtcNow,
                LastUpdateTimestamp = DateTime.UtcNow,
                UserId = WorkSpace.Instance.UserProfile.UserName,
                FeatureId = FeatureId.ToString(),
                Duration = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _startTime),
                Metadata = Metadata.ToJSON(),
            });
        }

        public void Dispose()
        {
            StopTracking();
        }
    }
}
