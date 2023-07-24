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

namespace GingerCore.ALM.RQM
{
    public class RQMExecutionRecord
    {
        public static class Fields
        {
            public static string RQMID = "RQMID";
            public static string RelatedTestScriptGUID = "RelatedTestScriptRqmID";
            public static string RelatedTestCaseGUID = "RelatedTestCaseRqmID";
        }

        public RQMExecutionRecord(string rQMID, string relatedTestScriptRqmID, string relatedTestCaseRqmID)
        {
            RQMID = rQMID;
            RelatedTestScriptRqmID = relatedTestScriptRqmID;
            RelatedTestCaseRqmID = relatedTestCaseRqmID;
        }

        public string RQMID { get; set; }

        public string RelatedTestScriptRqmID { get; set; }

        public string RelatedTestCaseRqmID { get; set; }
    }
}
