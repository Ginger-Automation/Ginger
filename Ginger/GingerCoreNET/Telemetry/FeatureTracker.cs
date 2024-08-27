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
        private long? _startTime;

        public FeatureId FeatureId { get; }

        internal FeatureTracker(FeatureId featureId)
        {
            _startTime = null;
            FeatureId = featureId;
        }

        public void StartTracking()
        {
            _startTime = DateTime.Now.Ticks;
        }

        public IFeatureTracker.FeatureTrackingResult StopTracking()
        {
            if (_startTime == null)
            {
                throw new InvalidOperationException("Tracking must be started first");
            }

            return new IFeatureTracker.FeatureTrackingResult()
            {
                FeatureId = FeatureId,
                Duration = TimeSpan.FromTicks(DateTime.Now.Ticks - _startTime.Value),
            };
        }
    }
}
