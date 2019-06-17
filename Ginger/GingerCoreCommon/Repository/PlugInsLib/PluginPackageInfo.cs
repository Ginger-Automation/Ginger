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
using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.Repository
{
    public class PluginPackageInfo
    {
        public static string cInfoFile = "Ginger.PluginPackage.json";

        public string Id { get; set; }
        public string Version { get; set; }
        public string ProjectUrl { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public string StartupDLL { get; set; }
        public string UIDLL { get; set; }

        public List<string> Implementations{get;set;}

        

    }
}
