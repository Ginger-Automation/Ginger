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
using System;
using System.Windows.Controls;

namespace Ginger.TwoLevelMenuLib
{
    public class TopMenuItem: MenuItemBase
    {
        public ObservableList<SubMenuItem> SubItems = new ObservableList<SubMenuItem>();

        public SubMenuItem LastSubMenuItem;

        public TopMenuItem(eImageType iconType, string name, ConsoleKey key, string automationID, string toolTip="")
        {
            IconType = iconType;
            Key = key;
            Name = name;            
            Active = true;
            AutomationID = automationID;
            ToolTip = toolTip;
        }
        
        internal void Add(eImageType iconType, string name, Func<Page> pageFunc, ConsoleKey k, string toolTip, string AutomationID)
        {
            SubItems.Add(new SubMenuItem(iconType, name, pageFunc, k, toolTip, AutomationID));
        }
    }
}
