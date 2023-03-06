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

using GingerCore.Variables;

namespace Ginger.Reports
{
    public class VariableReport
    {
        private VariableBase mVariable;

        public VariableReport(VariableBase Variable)
        {
            mVariable = Variable;
        }

        // Put here everything we want to make public for users customizing the reports, never give direct access to the activity iteself.
        // serve as facade to expose only what we want
        // must not change as it will break existing reports, no compile check on XAML
        public int Seq { get; set; }
        public string Name { get { return mVariable.Name; } }
        public string Value { get { return mVariable.Value; } }        
    }
}
