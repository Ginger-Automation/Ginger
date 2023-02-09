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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Ast;

namespace Ginger.GherkinLib
{
    public class GherkinStep
    {
        public bool Selected { get; set; }
        public string Text { get; set; }
        public int Counter { get; set; }
        public string AutomationStatus { get; set; }
        public int ColorIndex { get; set; }
        public Step Step { get; internal set; }        

        public static class Fields
        {
            public static string Text = "Text";
            public static string Counter = "Counter";
            public static string AutomationStatus = "AutomationStatus";
        }
    }
}
