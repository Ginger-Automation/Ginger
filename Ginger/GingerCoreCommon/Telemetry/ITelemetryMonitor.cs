using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Telemetry
{
    public interface ITelemetryMonitor
    {
        public void AddLog(eLogLevel level, string msg);

        public void AddFeatureUsage(IFeatureTracker.FeatureTrackingResult usage);

        public IFeatureTracker NewFeatureTracker(FeatureId featureId);
    }
}
