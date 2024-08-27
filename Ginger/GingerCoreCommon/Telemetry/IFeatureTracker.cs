using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Telemetry
{
    public interface IFeatureTracker : IDisposable
    {
        public FeatureId FeatureId { get; }

        public void StopTracking();
    }
}
