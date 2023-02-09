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
    /// This class is been used for storing the Ginger automation status of a QC Test Case
    /// </summary>
    public class TestCaseAutoStatus
    {
        /// <summary>
        /// QC Test Case ID
        /// </summary>
        public long ID { get; set; }
        /// <summary>
        /// QC Test Case Steps
        /// </summary>
        public Dictionary<long, StepAutoStatus> Steps { get; set; }
        /// <summary>
        /// 'False' value means the TC is not exist in Ginger repository so it and all of it steps are not automated,
        /// 'True' value means the TC exist in Ginger repository but each step automation status need to be checked separately,
        /// 'Null' value means the TC automation status was not checked
        /// </summary>
        public bool? KnownByGinger { get; set; } //if 'False'- no need to check if steps are automated

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">QC Test Case ID</param>
        public TestCaseAutoStatus(long id)
        {
            this.ID = id;
            this.Steps = new Dictionary<long, StepAutoStatus>();
            this.KnownByGinger=null;
        }
    }
}
