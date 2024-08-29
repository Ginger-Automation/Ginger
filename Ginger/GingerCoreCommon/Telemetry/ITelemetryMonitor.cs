using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Telemetry
{
    public interface ITelemetryMonitor : IDisposable
    {
        public void AddLog(eLogLevel level, string msg);

        public void AddLog(eLogLevel level, string msg, TelemetryMetadata metadata);

        public IFeatureTracker StartFeatureTracking(FeatureId featureId);
    }
}
