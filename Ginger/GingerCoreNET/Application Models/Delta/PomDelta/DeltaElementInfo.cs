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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCoreNET.Application_Models
{
    public class DeltaElementInfo : DeltaItemBase
    {
        public ElementInfo ElementInfo = null;

        public ElementInfo PredictedElementInfo { get; set; } = null;

        public string ElementName { get { return ElementInfo.ElementName; } }

        public string Description { get { return ElementInfo.Description; } }

        public eElementType ElementTypeEnum { get { return ElementInfo.ElementTypeEnum; } }

        public eImageType ElementTypeImage { get { return ElementInfo.ElementTypeImage; } }

        public ElementInfo.eElementStatus ElementStatus { get { return ElementInfo.ElementStatus; } }

        public eImageType StatusIcon { get { return ElementInfo.StatusIcon; } }

        public string OptionalValuesObjectsListAsString { get { return ElementInfo.OptionalValuesObjectsListAsString; } }

        public bool IsAutoLearned { get { return ElementInfo.IsAutoLearned; } }

        public ObservableList<DeltaElementLocator> Locators = [];

        public ObservableList<DeltaElementLocator> FriendlyLocators = [];

        public ObservableList<DeltaControlProperty> Properties = [];

        public string ScreenShotImage { get { return ElementInfo.ScreenShotImage; } }


        public object OriginalElementGroup { get { return ElementInfo.ElementGroup; } }

        public object mElementGroup = null;
        public object SelectedElementGroup
        {
            get
            {
                if (mElementGroup == null)
                {
                    mElementGroup = OriginalElementGroup;
                }
                return mElementGroup;
            }
            set
            {
                if (mElementGroup != value)
                {
                    mElementGroup = value;
                    OnPropertyChanged(nameof(SelectedElementGroup));
                }
            }
        }
        /// <summary>
        /// Map Deleted element with new added element
        /// </summary>
        /// 
        private string mMappedElementInfo;
        public string MappedElementInfo
        {
            get
            {
                return mMappedElementInfo;
            }
            set
            {
                mMappedElementInfo = value;
                OnPropertyChanged(nameof(MappedElementInfo));
            }
        }

        /// <summary>
        /// Map Deleted element with new added element
        /// </summary>
        /// 
        private string mMappedElementInfoName;
        public string MappedElementInfoName
        {
            get
            {
                return mMappedElementInfoName;
            }
            set
            {
                mMappedElementInfoName = value;
                OnPropertyChanged(nameof(MappedElementInfoName));
            }
        }


        private eMappingStatus mMappingElementStatus;
        public eMappingStatus MappingElementStatus
        {
            get
            {
                return mMappingElementStatus;
            }
            set
            {
                switch (value)
                {
                    case eMappingStatus.DeletedElement:
                        MappedElementInfo = string.Empty;
                        MappedElementInfoName = string.Empty;
                        break;
                    case eMappingStatus.ReplaceExistingElement:
                    case eMappingStatus.MergeExistingElement:
                        if (string.IsNullOrEmpty(MappedElementInfo))
                        {
                            mMappingElementStatus = eMappingStatus.DeletedElement;
                            return;//not valid operation
                        }
                        break;
                }
                mMappingElementStatus = value;
                OnPropertyChanged(nameof(MappingElementStatus));
            }
        }

        public enum eMappingStatus
        {
            DeletedElement,
            ReplaceExistingElement,
            MergeExistingElement
        }


    }
}
