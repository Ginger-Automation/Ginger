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

namespace Amdocs.Ginger.CoreNET.RunLib
{

    // This is the readonly platform action data executed by the Ginger node
    // This struct must match exectly to PlatformAction - so json deserialize witl match

    public struct NodePlatformAction
    {
        public string Platform;
        public string ActionHandler;
        public string ActionType;

        // TODO: use List<NodeActionOutputValue> !!!!!!!!!!!!!!!!!
        public Dictionary<string, object> InputParams;
        public string error;
        public NodeActionOutput Output;
        public string exInfo;


        public void addError(string err)
        {
            exInfo += err;
        }
        
    }
}