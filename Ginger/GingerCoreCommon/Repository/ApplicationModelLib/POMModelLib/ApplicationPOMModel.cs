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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
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

        public static readonly List<ePlatformType> PomSupportedPlatforms = new List<ePlatformType>() { ePlatformType.Web };

        private string mPageURL = string.Empty;

        [IsSerializedForLocalRepository]
        public string PageURL //OperationName 
        {
            get
            {
                return mPageURL;
            }
            set
            {
                mPageURL = value;
                OnPropertyChanged(nameof(this.PageURL));
            }
        }




        [IsSerializedForLocalRepository]
        public ObservableList<ElementInfo> UnMappedUIElements = new ObservableList<ElementInfo>();

        [IsSerializedForLocalRepository]
        public ObservableList<ElementInfo> MappedUIElements = new ObservableList<ElementInfo>();

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

        string mScreenShotImage;
        [IsSerializedForLocalRepository]
        public string ScreenShotImage { get { return mScreenShotImage; } set { if (mScreenShotImage != value) { mScreenShotImage = value; OnPropertyChanged(nameof(ScreenShotImage)); } } }

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.ApplicationPOMModel;
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
