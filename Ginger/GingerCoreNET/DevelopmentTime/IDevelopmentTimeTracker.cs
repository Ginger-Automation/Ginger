using System;

namespace Amdocs.Ginger.CoreNET.DevelopmentTime
{
    public interface IDevelopmentTimeTracker
    {
        TimeSpan DevelopmentTime { get; }
        void StartTimer();
        void StopTimer();

    }
}
