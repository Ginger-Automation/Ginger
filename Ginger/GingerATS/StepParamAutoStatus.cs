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

namespace GingerATS
{
    /// <summary>
    /// This class is been used for storing the Ginger automation status of a QC Test Case Step Parameter Value
    /// </summary>
    public class StepParamAutoStatus
    {
        /// <summary>
        /// QC Test Case Step Parameter name as exist in QC
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// QC Test Case Step Parameter selected value for execution
        /// </summary>
        public string SelectedValue { get; set; }
        /// <summary>
        /// 'False' value means the TC Step Parameter and/or Parameter Value is not supported by Ginger,
        /// 'True' value means the TC Step Parameter and it's selected value is supported by Ginger,
        /// 'Null' value means the TC Step Parameter automation status was not checked
        /// </summary>
        public bool? AutomatedByGinger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">QC Test Case Step Parameter name as exist in QC</param>
        /// <param name="selectedValue">QC Test Case Step Parameter selected value for execution</param>
        public StepParamAutoStatus(string name, string selectedValue)
        {
            this.Name = name;
            this.SelectedValue = selectedValue;
            this.AutomatedByGinger = null;
        }
    }
}
