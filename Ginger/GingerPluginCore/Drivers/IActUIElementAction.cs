using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.Drivers
{
    public interface IActUIElementAction
    {


        void RunActUi(IGingerAction gingerAction, string message, int sleep);
    }
}
