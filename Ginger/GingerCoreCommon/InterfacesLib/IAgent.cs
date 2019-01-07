using System;
using System.Collections.Generic;
using System.Text;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IAgent
    {
        string Name { get; set; }
        ePlatformType Platform { get; }
        bool UsedForAutoMapping { get; set; }

        void StartDriver();
        
    }
}
