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
        
        public List<RunsetOperationBase> RunsetOperations { get; set; } = new List<RunsetOperationBase>();
    }
}
