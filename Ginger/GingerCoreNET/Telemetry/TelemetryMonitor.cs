using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryMonitor : ITelemetryMonitor
    {
        public void AddLog(eLogLevel level, string msg)
        {
            AddLog(level, msg, attributes: []);
        }

        public void AddLog(eLogLevel level, string msg, Dictionary<string,string> attributes)
        {
            AddLog(new TelemetryLogRecord()
            {
                AppVersion = "1.2.3",
                CreationTimestamp = DateTime.UtcNow,
                LastUpdateTimestamp = DateTime.UtcNow,
                Level = level.ToString(),
                UserId = "abcd",
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

        }

        private void AddFeatureUsage(TelemetryFeatureRecord featureRecord)
        {

        }
    }
}
