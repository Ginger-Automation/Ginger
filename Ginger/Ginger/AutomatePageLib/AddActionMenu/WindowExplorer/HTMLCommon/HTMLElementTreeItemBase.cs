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
using System.Windows.Controls;
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using GingerWPF.UserControlsLib.UCTreeView;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;

namespace Ginger.WindowExplorer.HTMLCommon
{
    public class HTMLElementTreeItemBase : TreeViewItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {        
        public ElementInfo ElementInfo { get; set; }     
        
        Object ITreeViewItem.NodeObject()
        {
            return ElementInfo;
        }

        StackPanel ITreeViewItem.Header()
        {
            // TODO: put ? or unknown
            string ImageFileName = "@Folder_16x16.png";
            return TreeViewUtils.CreateItemHeader(ElementInfo.ElementTitle, ImageFileName);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> list = new List<ITreeViewItem>();

            List<ElementInfo> ChildrenList = ElementInfo.WindowExplorer.GetElementChildren(this.ElementInfo);
            if(ChildrenList != null)
            {
                foreach (ElementInfo EI in ChildrenList)
                {
                    //TODO: move converter to here
                    ITreeViewItem TVI = HTMLElementInfoConverter.GetHTMLElementTreeItem(EI);
                    list.Add(TVI);
                }
            }
            
            return list;          
        }
        
        bool ITreeViewItem.IsExpandable()
        {
            return this.ElementInfo.IsExpandable;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            return null;
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
            return ElementInfo.GetElementProperties();
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            return null;
        }

        public void AddGeneralHTMLActions(ObservableList<Act> list)
        {
            list.Add(new ActGenElement()
            {
                Description = "Click '" + this.ElementInfo.ElementTitle + "' ",
                GenElementAction = ActGenElement.eGenElementAction.Click,
                Value = ""
            });
            
            list.Add(new ActGenElement()
            {
                Description = "Check item is visible '" + this.ElementInfo.ElementTitle + "' ",
                GenElementAction = ActGenElement.eGenElementAction.Visible,
                Value = ""
            });
        }

        public ObservableList<ActInputValue> GetItemSpecificActionInputValues()
        {
            return null;
        }
    }
}
