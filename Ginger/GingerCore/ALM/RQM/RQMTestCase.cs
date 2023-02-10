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
using System.Linq;
using System.ComponentModel;

namespace GingerCore.ALM.RQM
{
    public class RQMTestCase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static class Fields
        {
            public static string Seq = "Seq";
            public static string Name = "Name";
            public static string RQMID = "RQMID";
            public static string CreatedBy = "CreatedBy";
            public static string CreationDate = "CreationDate";
            public static string Steps = "Steps";
            public static string TestScriptsNamesList = "TestScriptsNamesList";
            public static string TestScriptsQuantity = "TestScriptsQuantity";
            public static string SelectedTestScriptName = "SelectedTestScriptName";
            public static string TestSuiteTitle = "TestSuiteTitle";
            public static string TestSuiteId = "TestSuiteId";
        }

        public RQMTestCase(string name, string rQMID, string description, string createdBy, DateTime creationDate, string testSuiteTitle, string testSuiteId)
        {
            Name = name;
            RQMID = rQMID;
            CreatedBy = createdBy;
            CreationDate = creationDate;
            Description = description;
            TestScripts = new ObservableList<RQMTestScript>();
            Parameters = new ObservableList<RQMTestParameter>();
            BTSID = string.Empty;
            TestSuiteTitle = testSuiteTitle;
            TestSuiteId = testSuiteId;
    }
        public string Seq { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string RQMID { get; set; }

        public string BTSID { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string TestSuiteTitle { get; set; }

        public string TestSuiteId { get; set; }

        public ObservableList<RQMTestScript> TestScripts { get; set; }

        public int TestScriptsQuantity
        {
            get
            {
                if (TestScripts != null)
                {
                    return TestScripts.Count;
                }
                else
                {
                    return 0;
                }
            }
            set { }
        }

        public ObservableList<RQMTestParameter> Parameters { get; set; }

        string mSelectedTestScripts = string.Empty;
        ObservableList<string> mTestScriptsStr = new ObservableList<string>();
        public ObservableList<string> TestScriptsNamesList
        {
            get
            {
                mTestScriptsStr = new ObservableList<string>();
                TestScripts.ToList().ForEach(y => mTestScriptsStr.Add(y.Name));
                return mTestScriptsStr;
            }
            set
            {
                mTestScriptsStr = value;
                OnPropertyChanged(Fields.TestScriptsNamesList);
            }
        }

        public string SelectedTestScriptName
        {
            get
            {
                return mSelectedTestScripts;
            }
            set
            {
                mSelectedTestScripts = value;
                OnPropertyChanged(Fields.SelectedTestScriptName);
            }
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
