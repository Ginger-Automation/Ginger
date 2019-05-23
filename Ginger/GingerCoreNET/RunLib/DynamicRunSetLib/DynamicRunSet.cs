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

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{    
    public class DynamicRunSet
    {
        public string SolutionSourceControlType { get; set; }
        public string SolutionSourceControlUrl { get; set; }
        public string SolutionSourceControlUser { get; set; }
        public string SolutionSourceControlPassword { get; set; }
        public string SolutionSourceControlPasswordEncrypted { get; set; }
        public string SolutionSourceControlProxyServer { get; set; }
        public string SolutionSourceControlProxyPort { get; set; }
        public string SolutionPath { get; set; }
        
        public bool ShowAutoRunWindow { get; set; }

        public string Name { get; set; }
        public string Environemnt { get; set; }        
        public bool RunAnalyzer { get; set; }
        public bool RunInParallel { get; set; }

        public List<Runner> Runners { get; set; } = new List<Runner>();  
        
        public MailReport MailReport { get; set; }
    }
}
