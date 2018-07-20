#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib;
using System;

namespace GingerCoreNET.ALMLib
{
    public class ExternalItemField : RepositoryItem
    {
        public new static class Fields
        {
            public static string ToUpdate = "ToUpdate";
            public static string ID = "ID";
            public static string Name = "Name";
            public static string ItemType = "ItemType";
            public static string Mandatory = "Mandatory";
            public static string PossibleValues = "PossibleValues";
            public static string SelectedValue = "SelectedValue";

            public static string This = "This";//for test
        }

        public ExternalItemField This//for test
        {
            get
            {
                return this;
            }
        }

        [IsSerializedForLocalRepository]
        public bool ToUpdate { get; set; }

        [IsSerializedForLocalRepository]
        public string ID { get; set; }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }

        [IsSerializedForLocalRepository]
        public String ItemType { get; set; }

        [IsSerializedForLocalRepository]
        public bool Mandatory { get; set; }

        ObservableList<string> mPossibleValues = new ObservableList<string>();
        [IsSerializedForLocalRepository]
        public ObservableList<string> PossibleValues
        {
            get
            {
                return mPossibleValues;
            }
            set
            {
                mPossibleValues = value;
                OnPropertyChanged(Fields.PossibleValues);
            }
        }

        string mSelectedValue;
        [IsSerializedForLocalRepository]
        public string SelectedValue
        {
            get
            {
                return mSelectedValue;
            }
            set
            {
                mSelectedValue = value;
                OnPropertyChanged(Fields.SelectedValue);
            }
        }

        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
    }
}