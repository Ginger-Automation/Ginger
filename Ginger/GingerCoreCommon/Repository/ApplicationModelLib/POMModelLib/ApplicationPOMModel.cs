#region License
/*
Copyright © 2014-2025 European Support Limited

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
using System.Diagnostics;
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common.WorkSpaceLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace Amdocs.Ginger.Repository
{
    public enum ePomElementCategory
    {
        Web, Java, Android, iOS, Windows, PowerBuilder, Mainframe
    }

    public class ApplicationPOMModel : ApplicationModelBase
    {
        public ApplicationPOMModel()
        {
            this.OnDirtyStatusChanged += POM_OnDirtyStatusChanged;
        }
        private void POM_OnDirtyStatusChanged(object sender, EventArgs e)
        {
            if (DirtyStatus == eDirtyStatus.Modified)
            {
                this.StartTimer();
            }
        }
        /// <summary>
        /// This method is called before saving the ApplicationPOMModel object.
        /// It stops the timer and returns false.
        /// </summary>
        /// <returns>False</returns>
        public override bool PreSaveHandler()
        {
            this.StopTimer();
            return false;
        }

        public const int cLearnScreenWidth = 1000;
        public const int cLearnScreenHeight = 1000;
        private Stopwatch _stopwatch;

        public static readonly List<ePlatformType> PomSupportedPlatforms = [ePlatformType.Web, ePlatformType.Java, ePlatformType.Windows, ePlatformType.Mobile];

        public bool IsLearning { get; set; }

        private string mPageURL = string.Empty;

        ePageLoadFlowType mPageLoadFlow;
        [IsSerializedForLocalRepository]
        public ePageLoadFlowType PageLoadFlow
        {
            get
            {
                return mPageLoadFlow;
            }
            set
            {
                if (mPageLoadFlow != value)
                {
                    mPageLoadFlow = value;
                    OnPropertyChanged(nameof(this.PageLoadFlow));
                }
            }
        }

        private TimeSpan mDevelopmentTime;
        [IsSerializedForLocalRepository]
        public TimeSpan DevelopmentTime
        {
            get
            {
                return mDevelopmentTime;
            }
            set
            {
                if (mDevelopmentTime != value)
                {
                    mDevelopmentTime = value;
                }
            }
        }
        public void StartTimer()
        {
            if (_stopwatch == null)
            {
                _stopwatch = new Stopwatch();
            }

            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
            }
            else
            {
                _stopwatch.Restart();
            }
        }
        public bool IsTimerRunning()
        {
            return _stopwatch != null && _stopwatch.IsRunning;
        }
        public void StopTimer()
        {
            if (_stopwatch != null && _stopwatch.IsRunning)
            {
                _stopwatch.Stop();
                TimeSpan elapsedTime = new TimeSpan(_stopwatch.Elapsed.Hours, _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds);
                DevelopmentTime = DevelopmentTime.Add(elapsedTime);
                _stopwatch.Reset();
            }
        }

        [IsSerializedForLocalRepository]
        public string PageURL //OperationName 
        {
            get
            {
                return mPageURL;
            }
            set
            {
                if (mPageURL != value)
                {
                    mPageURL = value;
                    OnPropertyChanged(nameof(this.PageURL));
                }
            }
        }

        RepositoryItemKey mMappedBusinessFlow;
        [IsSerializedForLocalRepository]
        public RepositoryItemKey MappedBusinessFlow
        {
            get
            {
                return mMappedBusinessFlow;
            }
            set
            {
                if (mMappedBusinessFlow != value)
                {
                    mMappedBusinessFlow = value;
                    OnPropertyChanged(nameof(this.MappedBusinessFlow));
                }
            }
        }

        private ObservableList<ElementInfo> mUnMappedElements;
        /// <summary>
        /// Been used to identify if UnMappedUIElements were lazy loaded already or not
        /// </summary>
        public bool UnMappedUIElementsLazyLoad { get { return (mUnMappedElements != null) && mUnMappedElements.LazyLoad; } }
        [IsLazyLoad(LazyLoadListConfig.eLazyLoadType.NodePath)]
        [IsSerializedForLocalRepository]
        public ObservableList<ElementInfo> UnMappedUIElements
        {
            get
            {
                if (mUnMappedElements == null)
                {
                    mUnMappedElements = [];
                }
                if (mUnMappedElements.LazyLoad)
                {
                    mUnMappedElements.LoadLazyInfo();
                    if (this.DirtyStatus != eDirtyStatus.NoTracked)
                    {
                        this.TrackObservableList(mUnMappedElements);
                    }
                }
                return mUnMappedElements;
            }
            set
            {
                mUnMappedElements = value;
            }
        }

        private ObservableList<ElementInfo> mMappedElements;
        /// <summary>
        /// Been used to identify if MappedUIElements were lazy loaded already or not
        /// </summary>
        public bool MappedUIElementsLazyLoad { get { return (mMappedElements != null) && mMappedElements.LazyLoad; } }
        [IsLazyLoad(LazyLoadListConfig.eLazyLoadType.NodePath)]
        [IsSerializedForLocalRepository]
        public ObservableList<ElementInfo> MappedUIElements
        {
            get
            {
                if (mMappedElements == null)
                {
                    mMappedElements = [];
                }
                if (mMappedElements.LazyLoad)
                {
                    mMappedElements.LoadLazyInfo();
                    if (this.DirtyStatus != eDirtyStatus.NoTracked)
                    {
                        this.TrackObservableList(mMappedElements);
                    }
                }
                return mMappedElements;
            }
            set
            {
                mMappedElements = value;
            }
        }

        public ObservableList<CustomRelativeXpathTemplate> RelativeXpathTemplateList = [];
        public override void PostDeserialization()
        {
            base.PostDeserialization();
            if (PomSetting == null)
            {
                PomSetting = new PomSetting();
            }
            if (RelativeXpathTemplateList != null && RelativeXpathTemplateList.Count > 0)
            {
                PomSetting.RelativeXpathTemplateList = RelativeXpathTemplateList;
            }
        }
        public ObservableList<ElementInfo> GetUnifiedElementsList()
        {
            ObservableList<ElementInfo> unifiedList = [];
            foreach (ElementInfo element in MappedUIElements)
            {
                element.ElementGroup = ApplicationPOMModel.eElementGroup.Mapped;
                unifiedList.Add(element);
            }
            foreach (ElementInfo element in UnMappedUIElements)
            {
                element.ElementGroup = ApplicationPOMModel.eElementGroup.Unmapped;
                unifiedList.Add(element);
            }
            return unifiedList;
        }

        public enum eElementGroup
        {
            Mapped,
            Unmapped
        }

        public enum ePageLoadFlowType
        {
            PageURL,
            BusinessFlow
        }

        string mScreenShotImage;
        [IsSerializedForLocalRepository]
        public string ScreenShotImage { get { return mScreenShotImage; } set { if (mScreenShotImage != value) { mScreenShotImage = value; OnPropertyChanged(nameof(ScreenShotImage)); } } }

        private eImageType eImageType;
        public override eImageType ItemImageType
        {
            get
            {
                return eImageType;
            }
        }

        public void SetItemImageType(ePlatformType platformType)
        {
            eImageType = platformType switch
            {
                ePlatformType.Web => eImageType.Globe,
                ePlatformType.Java => eImageType.Java,
                ePlatformType.Windows => eImageType.Window,
                ePlatformType.Mobile => eImageType.Mobile,
                _ => eImageType.ApplicationPOMModel,
            };
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }

        Guid mLastUsedAgent;
        [IsSerializedForLocalRepository]
        public Guid LastUsedAgent { get { return mLastUsedAgent; } set { if (mLastUsedAgent != value) { mLastUsedAgent = value; OnPropertyChanged(nameof(LastUsedAgent)); } } }

        public string NameWithRelativePath
        {
            get
            {
                return this.ContainingFolder[(this.ContainingFolder.IndexOf(@"\POM Models") + 11)..] + "\\" + Name;
            }

        }
        public override string GetItemType()
        {
            return nameof(ApplicationPOMModel);
        }

        private bool mIsCollapseDetailsExapander;
        public bool IsCollapseDetailsExapander //OperationName 
        {
            get
            {
                return mIsCollapseDetailsExapander;
            }
            set
            {
                mIsCollapseDetailsExapander = value;
                OnPropertyChanged(nameof(this.IsCollapseDetailsExapander));
            }
        }

        private ObservableList<POMPageMetaData> mApplicationPOMMetaData;
        /// <summary>
        /// Been used to identify if POMMetaData were lazy loaded already or not
        /// </summary>
        public bool ApplicationPOMMetaDataLazyLoad { get { return (mApplicationPOMMetaData != null) && mApplicationPOMMetaData.LazyLoad; } }
        [IsLazyLoad(LazyLoadListConfig.eLazyLoadType.NodePath)]
        [IsSerializedForLocalRepository]
        public ObservableList<POMPageMetaData> ApplicationPOMMetaData
        {
            get
            {
                if (mApplicationPOMMetaData == null)
                {
                    mApplicationPOMMetaData = [];
                }
                if (mApplicationPOMMetaData.LazyLoad)
                {
                    mApplicationPOMMetaData.LoadLazyInfo();
                    if (this.DirtyStatus != eDirtyStatus.NoTracked)
                    {
                        this.TrackObservableList(mApplicationPOMMetaData);
                    }
                }
                return mApplicationPOMMetaData;
            }
            set
            {
                mApplicationPOMMetaData = value;
            }
        }

        private PomSetting mPomSetting;
        [IsSerializedForLocalRepository]
        public PomSetting PomSetting
        {
            get
            {
                return mPomSetting;
            }
            set
            {
                mPomSetting = value;
                OnPropertyChanged(nameof(this.PomSetting));
            }
        }

    }
}
