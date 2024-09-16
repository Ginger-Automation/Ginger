#region License
/*
Copyright © 2014-2024 European Support Limited

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

using System;
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
namespace GingerCore.Environments
{
    public class ProjEnvironment : RepositoryItemBase
    {

        public static class Fields
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

        //Ginger Analytics Region starts here, No Tresspassing allowed by Ginger Objects
        private bool mGAFlag;
        [IsSerializedForLocalRepository]
        public bool GAFlag { get { return mGAFlag; } set { if (mGAFlag != value) { mGAFlag = value; OnPropertyChanged(nameof(GAFlag)); } } }

        private Guid mGingerAnalyticsEnvId;
        [IsSerializedForLocalRepository]
        public Guid GingerAnalyticsEnvId { get { return mGingerAnalyticsEnvId; } set { if (mGingerAnalyticsEnvId != value) { mGingerAnalyticsEnvId = value; OnPropertyChanged(nameof(GingerAnalyticsEnvId)); } } }

        private Guid mGingerAnalyticsProjectId;
        [IsSerializedForLocalRepository]
        public Guid GingerAnalticsProjectId { get { return mGingerAnalyticsProjectId; } set { if (mGingerAnalyticsProjectId != value) { mGingerAnalyticsProjectId = value; OnPropertyChanged(nameof(GingerAnalticsProjectId)); } } }

        private Guid mGingerAnalyticsArchitectureId;
        [IsSerializedForLocalRepository]
        public Guid GingerAnalticsArchitectureId { get { return mGingerAnalyticsArchitectureId; } set { if (mGingerAnalyticsArchitectureId != value) { mGingerAnalyticsArchitectureId = value; OnPropertyChanged(nameof(GingerAnalticsArchitectureId)); } } }

        private string mGingerAnalyticsRelease;
        [IsSerializedForLocalRepository]
        public string GingerAnalyticsRelease { get { return mGingerAnalyticsRelease; } set { if (mGingerAnalyticsRelease != value) { mGingerAnalyticsRelease = value; OnPropertyChanged(nameof(GingerAnalyticsRelease)); } } }

        private bool mActive;
        [IsSerializedForLocalRepository]
        public bool Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(nameof(Active)); } } }

        [IsSerializedForLocalRepository]
        public ObservableList<EnvApplication> Applications { get; set; } = new ObservableList<EnvApplication>();

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> Tags = new ObservableList<Guid>();

        public override bool FilterBy(eFilterBy filterType, object obj)
        {
            switch (filterType)
            {
                case eFilterBy.Tags:
                    foreach (Guid tagGuid in Tags)
                    {
                        Guid guid = ((List<Guid>)obj).FirstOrDefault(x => tagGuid.Equals(x) == true);
                        if (!guid.Equals(Guid.Empty))
                        {
                            return true;
                        }
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
                    if (ea.Dbs != null)
                    {
                        foreach (Database db in ea.Dbs)
                        {
                            db?.DatabaseOperations.CloseConnection();
                        }
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
        public override string GetItemType()
        {
            return "Environment";
        }
        // object ProjEnvironment.Guid => throw new NotImplementedException();

        // Checks the relation between Environment Application and Application Platform in "Target Application".

        public bool CheckIfApplicationPlatformExists(Guid ParentGuid, string AppName)
        {
            return this.Applications.Any((app) => app.ParentGuid.Equals(ParentGuid) || app.Name.Equals(AppName));
        }

        public void AddApplications(IEnumerable<ApplicationPlatform> SelectedApplications)
        {
            
            foreach(ApplicationPlatform SelectedApplication in SelectedApplications)
            {
                EnvApplication envApplication = new ();
                envApplication.Name = SelectedApplication.AppName;
                envApplication.ParentGuid = SelectedApplication.Guid;
                envApplication.Description = SelectedApplication.Description;
                envApplication.Platform = SelectedApplication.Platform;
                envApplication.Active = true;
                this.Applications.Add(envApplication);
                OnPropertyChanged(nameof(Applications));
            }
        }

    }
}
