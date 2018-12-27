using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public class DriverDispatcher
    {
        //FIXME temp using dynamic
        public dynamic Object { get; set; }

        public void Invoke(Action callback)
        {            
            Object.Invoke(callback);
        }

        public void Run()
        {
            Object.Run();
        }
    }
}
