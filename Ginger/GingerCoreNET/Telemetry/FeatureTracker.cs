#region License
/*
Copyright © 2014-2026 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Telemetry;
using Ginger.SolutionGeneral;
using System;

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
            Metadata = [];
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
