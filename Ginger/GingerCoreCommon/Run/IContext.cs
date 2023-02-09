#region License
/*
Copyright © 2014-2023 European Support Limited

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

using Amdocs.Ginger.Common.Repository;
using Ginger.Run;
//using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.ComponentModel;

namespace Amdocs.Ginger.Common
{
    public interface IContext
    {
        Activity Activity { get; set; }
        //IAgent Agent { get; set; }
        string AgentStatus { get; set; }
        BusinessFlow BusinessFlow { get; set; }
        ProjEnvironment Environment { get; set; }
        eExecutedFrom ExecutedFrom { get; set; }
        ePlatformType Platform { get; set; }
        IGingerExecutionEngine Runner { get; set; }
        //RunSetActionBase RunsetAction { get; set; }
        TargetBase Target { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name);
    }
}
