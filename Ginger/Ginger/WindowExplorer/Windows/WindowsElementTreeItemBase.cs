#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System;
using System.Collections.Generic;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.WindowExplorer.Windows;
using Ginger.WindowExplorer;
using System.Windows.Controls;
using Ginger.Drivers.WindowsAutomation;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using Ginger.Drivers.UIA;
using GingerCore.Actions.UIAutomation;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;

namespace Ginger.Drivers.Windows
{
    public abstract class WindowsElementTreeItemBase : AutomationElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        public ITreeView TreeView
        {
            get;
            set;
        }

        Object ITreeViewItem.NodeObject()
        {
            return base.UIAElementInfo;
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            //TODO: Put better icon for generic control
            string ImageFileName = "@Agent_16x16.png";
            string Title = UIAElementInfo.ElementTitle;
                return TreeViewUtils.CreateItemHeader(Title, ImageFileName);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()        
        {
            List<ITreeViewItem> list = new List<ITreeViewItem>();
            {
                List<ElementInfo> Childrens = base.UIAElementInfo.WindowExplorer.GetElementChildren(base.UIAElementInfo);
                foreach (ElementInfo EI in Childrens)
                {
                    ITreeViewItem TVI = null;
                    if (EI.GetType() == typeof(UIAElementInfo))
                    {
                        //TODO: move convrter to here                
                         TVI = WindowsElementConverter.GetWindowsElementTreeItem(EI);
                        
                    }
                    else
                    {
                        TVI = WindowExplorer.HTMLCommon.HTMLElementInfoConverter.GetHTMLElementTreeItem(EI);
                    }
                    list.Add(TVI);
                }
            }
            return list;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return base.IsExpandable;          
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            return new UIAElementPage(base.UIAElementInfo);
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return null;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
        }

        ObservableList<ControlProperty> IWindowExplorerTreeItem.GetElementProperties()
        {
           return ((UIAutomationDriverBase) (UIAElementInfo.WindowExplorer)).mUIAutomationHelper.GetElementProperties(UIAElementInfo.ElementObject);
        }

        public ObservableList<ActInputValue> GetItemSpecificActionInputValues()
        {
            return null;
        }
    }
}