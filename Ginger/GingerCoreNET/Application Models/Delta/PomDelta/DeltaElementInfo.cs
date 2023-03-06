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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCoreNET.Application_Models
{
    public class DeltaElementInfo: DeltaItemBase
    {
        public ElementInfo ElementInfo = null;

        public string ElementName { get { return ElementInfo.ElementName; } }

        public string Description { get { return ElementInfo.Description; } }

        public eElementType ElementTypeEnum { get { return ElementInfo.ElementTypeEnum; } }

        public eImageType ElementTypeImage { get { return ElementInfo.ElementTypeImage; } }

        public ElementInfo.eElementStatus ElementStatus { get { return ElementInfo.ElementStatus; } }

        public eImageType StatusIcon { get { return ElementInfo.StatusIcon; } }

        public string OptionalValuesObjectsListAsString { get { return ElementInfo.OptionalValuesObjectsListAsString; } }

        public bool IsAutoLearned { get { return ElementInfo.IsAutoLearned; } }

        public ObservableList<DeltaElementLocator> Locators = new ObservableList<DeltaElementLocator>();

        public ObservableList<DeltaElementLocator> FriendlyLocators = new ObservableList<DeltaElementLocator>();

        public ObservableList<DeltaControlProperty> Properties = new ObservableList<DeltaControlProperty>();

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


        private eMappingStatus mMappingElementStatus;
        public eMappingStatus MappingElementStatus
        {
            get
            {
                return mMappingElementStatus;
            }
            set
            {
                mMappingElementStatus = value;
                OnPropertyChanged(nameof(MappingElementStatus));
            }
        }

        public enum eMappingStatus
        {
            DeletedElement,
            ReplaceExistingElement
        }


    }
}
