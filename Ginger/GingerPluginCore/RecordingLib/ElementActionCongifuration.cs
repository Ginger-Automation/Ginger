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
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public class ElementActionCongifuration
    {
        public object LocateBy { get; set; }
        public string LocateValue { get; set; }
        public string ElementValue { get; set; }
        public string Operation { get; set; }
        public object Type { get; set; }
        public string Description { get; set; }
        public object LearnedElementInfo { get; set; }

        public bool AddPOMToAction { get; set; }
        public string POMGuid { get; set; }
        public string ElementGuid { get; set; }
        
        #region java driver table element specific property
        public string WhereColumnValue { get; set; }
        public string RowValue { get; set; }
        public string ControlAction { get; set; }
        public string LocateRowType { get; set; }

        public string LocateColTitle { get; set; }
        public string ColSelectorValue { get; set; }

        #endregion
    }
}
