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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Amdocs.Ginger.Repository
{
    public class ApplicationPOMModel : ApplicationModelBase
    {
        public ApplicationPOMModel()
        {
        }

        public const int cLearnScreenWidth= 1000;
        public const int cLearnScreenHeight = 1000;

        public static readonly List<ePlatformType> PomSupportedPlatforms = new List<ePlatformType>() { ePlatformType.Web , ePlatformType.Java , ePlatformType.Windows, ePlatformType.Mobile };
        
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
        public bool UnMappedUIElementsLazyLoad { get { return (mUnMappedElements != null) ? mUnMappedElements.LazyLoad : false; } }
        [IsLazyLoad (LazyLoadListConfig.eLazyLoadType.NodePath)]
        [IsSerializedForLocalRepository]
        public ObservableList<ElementInfo> UnMappedUIElements
        {
            get
            {
                if (mUnMappedElements == null)
                {
                    mUnMappedElements = new ObservableList<ElementInfo>();
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
        public bool MappedUIElementsLazyLoad { get { return (mMappedElements != null) ? mMappedElements.LazyLoad : false; } }
        [IsLazyLoad (LazyLoadListConfig.eLazyLoadType.NodePath)]
        [IsSerializedForLocalRepository]
        public ObservableList<ElementInfo> MappedUIElements
        {
            get
            {
                if (mMappedElements == null)
                {
                    mMappedElements = new ObservableList<ElementInfo>();
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
       
        [IsSerializedForLocalRepository]
        public ObservableList<CustomRelativeXpathTemplate> RelativeXpathTemplateList = new ObservableList<CustomRelativeXpathTemplate>();

        public ObservableList<ElementInfo> GetUnifiedElementsList()
        {
            ObservableList<ElementInfo> unifiedList = new ObservableList<ElementInfo>();
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
            switch (platformType)
            {
                case ePlatformType.Web:
                    eImageType = eImageType.Globe;
                    break;
                case ePlatformType.Java:
                    eImageType = eImageType.Java;
                    break;
                case ePlatformType.Windows:
                    eImageType = eImageType.Window;
                    break;
                case ePlatformType.Mobile:
                    eImageType = eImageType.Mobile;
                    break;
                default:
                    eImageType = eImageType.ApplicationPOMModel;
                break;
            }
            
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
                return this.ContainingFolder.Substring(this.ContainingFolder.IndexOf(@"\POM Models") + 11) + "\\" + Name;
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
        public bool ApplicationPOMMetaDataLazyLoad { get { return (mApplicationPOMMetaData != null) ? mApplicationPOMMetaData.LazyLoad : false; } }
        [IsLazyLoad(LazyLoadListConfig.eLazyLoadType.NodePath)]
        [IsSerializedForLocalRepository]
        public ObservableList<POMPageMetaData> ApplicationPOMMetaData
        {
            get
            {
                if (mApplicationPOMMetaData == null)
                {
                    mApplicationPOMMetaData = new ObservableList<POMPageMetaData>();
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
    }
}
