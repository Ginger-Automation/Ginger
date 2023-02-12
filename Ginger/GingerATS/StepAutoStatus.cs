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

namespace GingerATS
{
    /// <summary>
    /// This class is been used for storing the Ginger automation status of a QC Test Case Step
    /// </summary>
    public class StepAutoStatus
    {
        /// <summary>
        /// QC Test Case Step ID (DS_ID is required)
        /// </summary>
        public long ID { get; set; }
        /// <summary>
        /// QC Test Case Step Parameters (ATS will store unique key of parameter name-value combination in the dictionary)
        /// </summary>
        public Dictionary<string, StepParamAutoStatus> Parameters { get; set; }
        /// <summary>
        /// 'False' value means the TC Step is not exist in Ginger repository or it is not automated- all step parameters automation status will be False as well,
        /// 'True' value means the TC Step is automated but each step parameter automation status need to be checked separately,
        /// 'Null' value means the TC Step automation status was not checked
        /// </summary>
        public bool? AutomatedByGinger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">QC Test Case Step ID</param>
        public StepAutoStatus(long id)
        {
            this.ID = id;
            this.Parameters = new Dictionary<string, StepParamAutoStatus>();
            this.AutomatedByGinger = null;
        }
    }
}
