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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GingerCore.Annotations;

namespace GingerCore.ALM.Rally
{
    /// <summary>
    /// </summary>
    public class RallyTestCase : INotifyPropertyChanged
    {
        public static class Fields
        {
            public static string Seq = "Seq";
            public static string Name = "Name";
            public static string RallyID = "RallyID";
            public static string CreatedBy = "CreatedBy";
            public static string CreationDate = "CreationDate";
            public static string TestSteps = "TestSteps";
        }

        public RallyTestCase(string name, string rallyID, string description, string createdBy, DateTime creationDate)
        {
            Name = name;
            RallyID = rallyID;
            CreatedBy = createdBy;
            CreationDate = creationDate;
            Description = description;
            TestSteps = new ObservableList<RallyTestStep>();
            Parameters = new ObservableList<RallyTestParameter>();
            BTSID = string.Empty;
        }
        public string Seq { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string RallyID { get; set; }

        public string BTSID { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public ObservableList<RallyTestStep> TestSteps { get; set; }

        public ObservableList<RallyTestParameter> Parameters { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
