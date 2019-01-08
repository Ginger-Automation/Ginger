using Amdocs.Ginger.Common.InterfacesLib;
using System;
using System.Windows.Threading;

namespace GingerCore.Drivers
{
    public class DriverWindowDispatcher : IDispatcher
    {
        Dispatcher mDispatcher;        

        public DriverWindowDispatcher(Dispatcher dispatcher)
        {
            mDispatcher = dispatcher;
        }

        public void BeginInvokeShutdown(dynamic dispatherPriority)
        {
            mDispatcher.BeginInvokeShutdown(dispatherPriority);
        }

        public void Invoke(Action callback)
        {
            mDispatcher.Invoke(callback);
        }

    }
}
