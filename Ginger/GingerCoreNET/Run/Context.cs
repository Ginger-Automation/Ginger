using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;

namespace Amdocs.Ginger.Common
{
    public class Context
    {
        public GingerRunner Runner { get; set; }

        public ProjEnvironment Environment { get; set; }

        public BusinessFlow BusinessFlow { get; set; }

        public Activity Activity { get; set; }
       
        public TargetBase Target { get; set; }

        public static Context GetAsContext(object contextObj)
        {
            if (contextObj != null && contextObj is Context)
            {
                return (Context)contextObj;
            }
            else
            {
                return null;
            }
        }
    }
}
