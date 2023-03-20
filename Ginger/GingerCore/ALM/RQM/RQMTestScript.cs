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

using Amdocs.Ginger.Common;
using System;

namespace GingerCore.ALM.RQM
{
    public class RQMTestScript
    {
        public static class Fields
        {
            public static string Seq = "Seq";
            public static string Name = "Name";
            public static string RQMID = "RQMID";
            public static string CreatedBy = "CreatedBy";
            public static string CreationDate = "CreationDate";
            public static string Steps = "Steps";
        }

        public RQMTestScript(string name, string rQMID, string description, string createdBy, DateTime creationDate)
        {
            Name = name;
            RQMID = rQMID;
            Description = description;
            CreatedBy = createdBy;
            CreationDate = creationDate;
            Steps = new ObservableList<RQMStep>();
            Parameters = new ObservableList<RQMTestParameter>();
            BTSStepsIDs = string.Empty;
        }

        public string Seq { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string RQMID { get; set; }

        public string BTSStepsIDs { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public ObservableList<RQMStep> Steps { get; set; }

        public ObservableList<RQMTestParameter> Parameters { get; set; }
    }
}
