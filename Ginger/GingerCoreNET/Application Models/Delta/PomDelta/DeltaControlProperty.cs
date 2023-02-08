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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCoreNET.Application_Models
{
   
    public class DeltaControlProperty : DeltaItemBase 
    {
        public enum ePropertiesChangesToAvoid
        {
            All,
            OnlySizeAndLocationProperties,
            None
        }

        public ControlProperty ElementProperty = null;

        public string Name { get { return ElementProperty.Name; } }
        public string Value { get { return ElementProperty.Value; } }
        
        //public string UpdatedValue { get; set; }        

    }
}
