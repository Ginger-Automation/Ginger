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

namespace Amdocs.Ginger.Repository
{
    public class ExternalItemFieldBase : RepositoryItemBase
    {
        public static class Fields
        {
            public static string ToUpdate = "ToUpdate";
            public static string ID = "ID";
            public static string Name = "Name";
            public static string ItemType = "ItemType";
            public static string Type = "Type";
            public static string Mandatory = "Mandatory";
            public static string PossibleValues = "PossibleValues";
            public static string SelectedValue = "SelectedValue";
        }

        [IsSerializedForLocalRepository]
        public bool ToUpdate { get; set; }

        [IsSerializedForLocalRepository]
        public string ID { get; set; }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }

        [IsSerializedForLocalRepository]
        public string ItemType { get; set; }

        [IsSerializedForLocalRepository]
        public string Type { get; set; }

        [IsSerializedForLocalRepository]
        public bool Mandatory { get; set; }

        [IsSerializedForLocalRepository]
        public bool SystemFieled { get; set; }
        public bool IsMultiple { get; set; } = false;

        ObservableList<string> mPossibleValues = new ObservableList<string>();
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
                if (mSelectedValue != value)
                {
                    mSelectedValue = value;
                    OnPropertyChanged(Fields.SelectedValue);
                }
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
