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

using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Amdocs.Ginger.Common.Functionalities
{
    public class FoundItem : INotifyPropertyChanged
    {

        public enum eStatus
        {
            [EnumValueDescription("Pending Replace")]
            PendingReplace,
            Replaced,
            ReplaceFailed,
            Saved,
            SavedFailed
        }

        private eStatus mStatus;
        public eStatus Status { get { return mStatus; } set { mStatus = value; OnPropertyChanged(nameof(Status)); } }

        private bool mIsSelected = false;
        // [IsSerializedForLocalRepository]
        public bool IsSelected { get { return mIsSelected; } set { mIsSelected = value; OnPropertyChanged(nameof(IsSelected)); } }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public RepositoryItemBase OriginObject { get; set; }

        public string OriginObjectType
        {
            get
            {
                return OriginObject.ObjFileExt.Substring(OriginObject.ObjFileExt.IndexOf("Ginger.") + 7);
            }
        }

        public string OriginObjectName
        {
            get
            {
                return OriginObject.ItemName;
            }
        }


        public RepositoryItemBase ItemObject { get; set; }


        public string ItemObjectType
        {
            get
            {
                return ItemObject.GetType().ToString();
            }
        }

        public string ItemObjectName
        {
            get
            {
                return ItemObject.ItemName;
            }
        }

        public string ItemParent { get; set; }


        private string mFoundField;
        public string FoundField { get { return mFoundField; } set { mFoundField = value; OnPropertyChanged(nameof(FoundField)); } }




        public RepositoryItemBase ParentItemToSave { get; set; }
        public string ParentItemName
        {
            get
            {
                return ParentItemToSave.ItemName;
            }
        }

        public string ParentItemType
        {
            get
            {
                return ParentItemToSave.GetType().ToString(); 
            }
                
        }

        public static string SolutionFolder { get; set; }

        public string ParentItemPath // ? needs to be relative
        {
            get
            {
                string path = Path.GetDirectoryName(ParentItemToSave.FileName);
                string s = path.Replace(SolutionFolder.TrimEnd(new char[] { '\\','/'}), string.Empty);
                return s;
            }
            
        }


        public string FieldName { get; set; }


        private string mFieldValue;
        public string FieldValue
        {
            get
            {
                return mFieldValue;
            }
            set
            {
                mFieldValue = value;
                OnPropertyChanged(nameof(FieldValue));
            }
        }

        public List<string> OptionalValuesToRepalce = new List<string>();

        //public string OriginalFieldValue { get; set; }

        //public object ValueToReplaceTo { get; set; }


    }
}
