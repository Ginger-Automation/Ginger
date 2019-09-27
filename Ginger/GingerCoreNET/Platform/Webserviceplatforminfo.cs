using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Run;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Platform
{
    class Webserviceplatforminfo : IPlatformPluginPostRun
    {
       private bool SaveRequest;
        private bool SaveResponse;
        private string PathToSave;
        public void PostExecute(Agent agent, Act actPlugin)
        {


            foreach (DriverConfigParam DCP in agent.DriverConfiguration.Where(x => x.IsPlatformParameter))
           {
                switch (DCP.Parameter)
                {

                    case "Save Request":
                        Boolean.TryParse(DCP.Value,out SaveRequest);
                        break;
                    case "Save Response":
                        Boolean.TryParse(DCP.Value, out SaveResponse);
                        break;
                    case "Path To Save":
                        PathToSave = DCP.Value;
                        break;
                }
            }
            if(PathToSave.StartsWith(@"~\"))
            {
                PathToSave = Path.Combine(WorkSpace.Instance.Solution.ContainingFolderFullPath, PathToSave.Remove(0, 2));
            }



        }





    }
}
