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
        private Dictionary<string,string> Attributes { get; }

        internal FeatureTracker(FeatureId featureId, Action<TelemetryFeatureRecord> onStop)
        {
            _startTime = DateTime.UtcNow.Ticks;
            _onStop = onStop;
            FeatureId = featureId;
            Attributes = [];
        }

        public void StopTracking()
        {
            _onStop(new TelemetryFeatureRecord()
            {
                AppVersion = "1.2.3",
                CreationTimestamp = DateTime.UtcNow,
                LastUpdateTimestamp = DateTime.UtcNow,
                UserId = "abcd",
                FeatureId = FeatureId.ToString(),
                Duration = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _startTime),
                Attributes = Attributes,
            });
        }

        public void Dispose()
        {
            StopTracking();
        }
    }
}
