using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IDispatcher
    {        
        void Invoke(Action callback);        
    }        
}
