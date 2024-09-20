using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Telemetry;
using Ginger.SolutionGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class FeatureTracker : IFeatureTracker
    {
        internal delegate void OnStopHandler(FeatureId featureId, TimeSpan duration, TelemetryMetadata metadata);

        private readonly long _startTime;
        private readonly OnStopHandler _onStop;
        private bool _stopped = false;

        public FeatureId FeatureId { get; }
        
        public TelemetryMetadata Metadata { get; }

        internal FeatureTracker(FeatureId featureId, OnStopHandler onStop)
        {
            _startTime = DateTime.UtcNow.Ticks;
            _onStop = onStop;
            FeatureId = featureId;
            Metadata = new();
        }

        public void StopTracking()
        {
            if (_stopped)
            {
                return;
            }
            _stopped = true;

            Solution? solution = WorkSpace.Instance.Solution;

            _onStop(FeatureId, duration: TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _startTime), Metadata);
        }

        public void Dispose()
        {
            StopTracking();
        }
    }
}
