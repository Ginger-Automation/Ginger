#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Plugin.Core;
using System;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class ActionHandler
    {
        public string ServiceActionId { get; set; }
        public IGingerAction NodeGingerAction { get; set; }

        //TODO: remove - not used
        public Action<GingerAction> Action { get; set; }
        public string Name { get; internal set; }
        public MethodInfo MethodInfo { get; set; }
        public object Instance { get; set; }  // instance of the class which contain the method to invoke
    }
}

