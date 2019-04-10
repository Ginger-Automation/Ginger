using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.Drivers
{
  public  interface IPluginDriver
    {

        void StartDriver();
        void CloseDriver();
    }
}
