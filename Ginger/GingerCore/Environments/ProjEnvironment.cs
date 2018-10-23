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
using Amdocs.Ginger.Common;
using System;
using System.Linq;
using Amdocs.Ginger.Common.Repository;
using System.Collections.Generic;
using Amdocs.Ginger.Common.Enums;

namespace GingerCore.Environments
{
    public class ProjEnvironment : RepositoryItemBase
    {        

        public  static class Fields
        {
            public static string Name = "Name";
            public static string ReleaseVersion = "ReleaseVersion";
            public static string Notes = "Notes";
            public static string Active = "Active";
        }

        private string mName;
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { if (mName != value) { mName = value; OnPropertyChanged(nameof(Name)); } } }

        private string mReleaseVersion;
        [IsSerializedForLocalRepository]
        public string ReleaseVersion { get { return mReleaseVersion; } set { if (mReleaseVersion != value) { mReleaseVersion = value; OnPropertyChanged(nameof(ReleaseVersion)); } } }

        private string mNotes;
        [IsSerializedForLocalRepository]
        public string Notes { get { return mNotes; } set { if (mNotes != value) { mNotes = value; OnPropertyChanged(nameof(Notes)); } } }

        private bool mActive;
        [IsSerializedForLocalRepository]
        public bool Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(nameof(Active)); } } }

        [IsSerializedForLocalRepository]
        public ObservableList<EnvApplication> Applications = new ObservableList<EnvApplication>();
        
        [IsSerializedForLocalRepository]
        public ObservableList<Guid> Tags = new ObservableList<Guid>();

        public override bool FilterBy(eFilterBy filterType, object obj)
        {
            switch (filterType)
            {
                case eFilterBy.Tags:
                    foreach (Guid tagGuid in Tags)
                    {
                        Guid guid = ((List<Guid>)obj).Where(x => tagGuid.Equals(x) == true).FirstOrDefault();
                        if (!guid.Equals(Guid.Empty))
                            return true;
                    }
                    break;
            }
            return false;
        }

        public override string GetNameForFileName()
        {
            return Name;
        }

        public EnvApplication GetApplication(string AppName)
        {
            EnvApplication app = (from ap in Applications where ap.Name == AppName select ap).FirstOrDefault();
            return app;
        }

        public void CloseEnvironment()
        {
            if (Applications != null)
            {
                foreach (EnvApplication ea in Applications)
                {
                    foreach (Database db in ea.Dbs)
                    {
                        if(ea.Dbs!=null)

                        db.CloseConnection();
                    }
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

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.Environment;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }
    }
}
