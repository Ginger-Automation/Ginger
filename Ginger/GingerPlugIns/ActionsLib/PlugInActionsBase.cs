#region License
/*
Copyright © 2014-2018 European Support Limited

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

using System.Collections.Generic;

namespace GingerPlugIns.ActionsLib
{
    // Base class for capability of type Actions, for plugin who want to add new actions to Ginger.
    public abstract class PlugInActionsBase : PlugInCapability
    {
        public override eCapabilityType CapabilityType { get { return eCapabilityType.Actions; } }  // Actions

        public abstract List<PlugInAction> Actions();   // Plugin which impl need to return a list of actions

        //public abstract void RunAction(GingerAction GA);  // The plugin will have to handle the actual run
    }
}
