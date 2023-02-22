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

using Amdocs.Ginger.CoreNET.Run;
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

using System.Collections.Generic;

namespace GingerCore.Platforms
{

    /// <summary>
    ///  Use this struct to pack platform action before sending to Ginger node
    //  Fields must match exactly PlatformActionData
    /// </summary>
    public struct PlatformAction
    {        


        public string ActionType { get; }

        public Dictionary<string, object> InputParams;
        
        public PlatformAction(IActPluginExecution Action)
        {            

            ActionType = Action.GetName();
            InputParams = new Dictionary<string, object>();
        }
        public PlatformAction(IActPluginExecution Action, Dictionary<string, object> InputParameters)
        {

            ActionType = Action.GetName();
            InputParams = InputParameters;
        }

    }
}
