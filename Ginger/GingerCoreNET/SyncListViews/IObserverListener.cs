using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET
{
    public interface IObserverListener
    {
        void NotifyListener(object sender);
    }
}
