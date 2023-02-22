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

namespace GingerCore.ALM.QC
{
    public class ALMTSTest
    {
        public ALMTSTest()
        {
            this.Parameters = new List<ALMTSTestParameter>();
            this.Steps = new List<ALMTSTestStep>();
            this.Runs = new List<ALMTSTestRun>();
        }

        public string TestName { get; set; }
        public string TestID { get; set; }
        public string LinkedTestID { get; set; }
        public string Description { get; set; }
        //Called Test Parameters
        public List<ALMTSTestParameter> Parameters { get; set; }
        public List<ALMTSTestStep> Steps { get; set; }
        public List<ALMTSTestRun> Runs { get; set; }
    }
}
