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

        public string ParentItemPath // ? needs to be relativy
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