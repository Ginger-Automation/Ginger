#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
