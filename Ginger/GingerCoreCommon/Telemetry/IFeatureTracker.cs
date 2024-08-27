using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Telemetry
{
    public interface IFeatureTracker
    {
        public sealed class FeatureTrackingResult
        {
            public required FeatureId FeatureId { get; init; }

            public required TimeSpan Duration { get; init; }

            public Dictionary<string, string> Attributes { get; } = [];
        }

        public FeatureId FeatureId { get; }

        public void StartTracking();

        public FeatureTrackingResult StopTracking();
    }
}
