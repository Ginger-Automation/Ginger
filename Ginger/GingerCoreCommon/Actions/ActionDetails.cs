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

using Amdocs.Ginger.Common;

namespace GingerCore.Actions.Common
{
    public class ActionDetails
    {
        // This class is impl in each action and return info for use
        // Prep work for now to try it.
        // Can be Short Info - for Action Grid 
        public string Info { get; set; }

        public ObservableList<ActionParamInfo> Params { get; set; }

        // Can be display page - view only in action page when row is highlighted
        // Can be all the AIVs - for report

        public ObservableList<ActionParamInfo> GetParamsInfo()
        {
            return Params;
        }
    }
}
