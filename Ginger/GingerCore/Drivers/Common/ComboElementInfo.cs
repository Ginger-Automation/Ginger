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

using Amdocs.Ginger.Common.UIElement;
using System;
using System.Collections.Generic;

namespace GingerCore.Drivers.Common
{
    /// <summary>
    /// Base class for different Control type for each driver, enable to show unified list in Window Explorer Grid
    /// </summary>
    public class ComboElementInfo : ElementInfo
    {
        public static class Fields
        {
            public static string ItemList = "ItemList";                      
        }
        private List<String> mItemList = null;
        public List<String> ItemList
        {
            get
            {
                if (mItemList == null) mItemList = GetItemList();
                return mItemList;
            }
            set { mItemList = value; }
        }
       
        // Used for Lazy loading when possible
        public virtual List<String> GetItemList()
        {
            // we return ElementType unless it was overridden as expected
            // So we keep backward compatibility until all drivers do it correctly
            return mItemList;
        }
    }
}
