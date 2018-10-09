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
using Ginger.Run.RunSetActions;
using GingerCore.GeneralLib;
using Amdocs.Ginger.Common.Enums;
using System;

namespace Ginger.Run
{
    public class RunSetConfig : RepositoryItemBase
    {        
        private string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        

        private string mDescription;
        [IsSerializedForLocalRepository]
        public string Description
        {
            get { return mDescription; }
            set
            {
                if (mDescription != value)
                {
                    mDescription = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<GingerRunner> GingerRunners = new ObservableList<GingerRunner>();

        [IsSerializedForLocalRepository]
        public ObservableList<RunSetActionBase> RunSetActions = new ObservableList<RunSetActionBase>();

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> Tags = new ObservableList<Guid>();

        public override string GetNameForFileName() { return Name; }

        public string LastRunsetLoggerFolder { get; set;}
        public bool RunsetExecLoggerPopulated
        {
            get
            {
                if(System.IO.Directory.Exists(LastRunsetLoggerFolder))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //TODO: remove make obsolete and use Run Set Actions only 
        /// <summary>
        /// DO_NOT_USE
        /// </summary>
        public bool SendEmail { get; set; }     
        /// <summary>
        /// DO_NOT_USE
        /// </summary>
        public Email Email{ get; set; }

        public bool mRunModeParallel = true;
        [IsSerializedForLocalRepository]
        public bool RunModeParallel
        {
            get
            {
                return mRunModeParallel;
            }
            set
            {
                mRunModeParallel = value;
                OnPropertyChanged(nameof(this.RunModeParallel));
            }
        }

        public bool mRunWithAnalyzer = true;
        [IsSerializedForLocalRepository]
        public bool RunWithAnalyzer
        {
            get
            {
                return mRunWithAnalyzer;
            }
            set
            {
                mRunWithAnalyzer = value;
                OnPropertyChanged(nameof(this.RunWithAnalyzer));
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
                return eImageType.RunSet;
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
