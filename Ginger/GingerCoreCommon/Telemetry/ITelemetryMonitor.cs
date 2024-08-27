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

        public void AddLog(eLogLevel level, string msg, Dictionary<string, string> attributes);

        public IFeatureTracker StartFeatureTracking(FeatureId featureId);
    }
}
