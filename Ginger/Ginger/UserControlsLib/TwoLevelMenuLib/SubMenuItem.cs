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

using Amdocs.Ginger.Common.Enums;
using System;
using System.Windows.Controls;

namespace Ginger.TwoLevelMenuLib
{
    public class SubMenuItem: MenuItemBase
    {       
        Page mItemPage = null;
        public Page ItemPage
        {
            get
            {
                if (mItemPage==null)
                {
                    mItemPage = GetMenuItemPage();
                }
                return mItemPage;
            }
        }

        public bool IsPageLoaded
        {
            get
            {
                if (mItemPage != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal void ResetPage()
        {
            mItemPage = null;
        }

        public Func<Page> GetMenuItemPage { get; }               

        public SubMenuItem(eImageType iconType, string name, Func<Page> action, ConsoleKey key, string toolTip, string automationID)
        {
            IconType = iconType;
            Key = key;
            Name = name;
            GetMenuItemPage = action;
            ToolTip = toolTip;
            AutomationID = automationID;
            Active = true;
        }       
    }
}
