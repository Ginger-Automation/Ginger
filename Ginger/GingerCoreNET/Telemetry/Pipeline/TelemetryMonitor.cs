using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry.Pipeline
{
    internal sealed class TelemetryMonitor : ITelemetryMonitor
    {
        public void AddLog(eLogLevel level, string msg)
        {
            TelemetryLogRecord logRecord = new()
            {
                AppVersion = "1.2.3",
                CreationTimestamp = DateTime.UtcNow,
                LastUpdateTimestamp = DateTime.UtcNow,
                Level = level.ToString(),
                UserId = "abcd",
                Message = msg,
            };
        }

        public void AddFeatureUsage(IFeatureTracker.FeatureTrackingResult usage)
        {
            TelemetryFeatureRecord featureRecord = new()
            {
                AppVersion = "1.2.3",
                CreationTimestamp = DateTime.UtcNow,
                LastUpdateTimestamp = DateTime.UtcNow,
                UserId = "abcd",
                FeatureId = usage.FeatureId.ToString(),
                Duration = usage.Duration,
            };
        }

        public IFeatureTracker NewFeatureTracker(FeatureId featureId)
        {
            return new FeatureTracker(featureId);
        }
    }
}
