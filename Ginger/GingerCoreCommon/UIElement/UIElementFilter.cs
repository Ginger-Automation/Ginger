#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using System.ComponentModel;

namespace Amdocs.Ginger.Common.UIElement
{
    public class UIElementFilter : RepositoryItemBase //INotifyPropertyChanged
    {
        public UIElementFilter() { }

        public UIElementFilter(eElementType elementType, string elementExtraInfo, bool isSelected = false)
        {
            Selected = isSelected;
            ElementType = elementType;
            ElementExtraInfo = elementExtraInfo;
        }

        private bool mSelected = true;
        [IsSerializedForLocalRepository]
        public bool Selected
        {
            get { return mSelected;
            }
            set
            {
                if (mSelected != value)
                {
                    mSelected = value; OnPropertyChanged(nameof(Selected));
                }
            }
        }

        private eElementType mElementType;

        [IsSerializedForLocalRepository]
        public eElementType ElementType
        {
            get
            {
                return mElementType;
            }
            set
            {
                if (mElementType != value)
                {
                    mElementType = value;
                    OnPropertyChanged(nameof(ElementType));
                }
            }
        }

        private bool mIsCommonElementType;
        public bool IsCommonElementType { get { return mIsCommonElementType; } set { mIsCommonElementType = value; } }
        public string ElementExtraInfo { get; set; }
        public override string ItemName { get; set; }

    }
}
