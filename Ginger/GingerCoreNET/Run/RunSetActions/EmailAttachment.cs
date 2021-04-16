#region License
/*
Copyright © 2014-2021 European Support Limited

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
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.Run.RunSetActions
{
    public class EmailAttachment : RepositoryItemBase
    {
        public enum eAttachmentType
        {
            [EnumValueDescription("Report")]
            Report,
            [EnumValueDescription("File")]
            File
        }

        public  static class Fields
        {
            public static string AttachmentType = "AttachmentType";
            public static string Name = "Name";
            public static string CalculatedName = "CalculatedName";
            public static string ExtraInformation = "ExtraInformation";
            public static string CalculatedExtraInformation = "CalculatedExtraInformation";
            public static string ZipIt = "ZipIt";
        }

        [IsSerializedForLocalRepository]
        public eAttachmentType AttachmentType { get; set; }

        string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
                OnPropertyChanged(Fields.Name);
            }
        }

        public static string CalculatedName { get; set; }
        public static string CalculatedExtraInformation { get; set; }

        //TODO: Check if it is need to serialize or not.
        string mExtraInformation = string.Empty;
        [IsSerializedForLocalRepository]
        public string ExtraInformation
        {
            get
            {
                return mExtraInformation;
            }
            set
            {
                mExtraInformation = value;
                OnPropertyChanged(Fields.ExtraInformation);
            }
        }

        bool mZipit;
        [IsSerializedForLocalRepository]
        public bool ZipIt
        {
            get
            {
                if (AttachmentType == eAttachmentType.Report)
                    return true;
                else
                    return mZipit;
            }
            set
            {
                if (mZipit != value)
                {
                    mZipit = value;
                    OnPropertyChanged(Fields.ZipIt);
                }
            }
        }

        public bool IsReport
        {
            get
            {
                if (AttachmentType == eAttachmentType.Report)
                    return false;
                else
                    return true;
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