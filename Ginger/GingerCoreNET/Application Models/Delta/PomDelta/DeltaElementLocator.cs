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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCoreNET.Application_Models
{
    public class DeltaElementLocator: DeltaItemBase
    {        
        ElementLocator mElementLocator = null;        
        public ElementLocator ElementLocator
        {
            get
            {
                return mElementLocator;
            }
            set
            {
                mElementLocator = value;
                mElementLocator.PropertyChanged += MOriginalElementLocator_PropertyChanged;
            }
        }
        public bool Active { get { return ElementLocator.Active; } }
        public eLocateBy LocateBy { get { return ElementLocator.LocateBy; } }
        public string LocateValue { get { return ElementLocator.LocateValue; } }
        
        public string ReferanceElement { get { return ElementLocator.ReferanceElement; } }
        public bool IsAutoLearned { get { return ElementLocator.IsAutoLearned; } }
        public ElementLocator.eLocateStatus LocateStatus { get { return ElementLocator.LocateStatus; } }
        public eImageType StatusIcon { get { return ElementLocator.StatusIcon; } } 

        public ePosition Position { get { return ElementLocator.Position; } }

        public bool EnableFriendlyLocator { get { return ElementLocator.EnableFriendlyLocator; } }

        private void MOriginalElementLocator_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElementLocator.StatusIcon))
            {
                OnPropertyChanged(nameof(StatusIcon));
            }
        }
    }
}
