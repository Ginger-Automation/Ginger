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
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers;
using GingerCore.Drivers.AndroidADB;
using GingerCore.Drivers.Common;
using GingerWPF.UserControlsLib.UCTreeView;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;

namespace Ginger.WindowExplorer.Android
{
    public class AndroidElementTreeItemBase :  ITreeViewItem, IWindowExplorerTreeItem
    {
        public ITreeView TreeView
        {
            get;
            set;
        }

        public AndroidElementInfo AndroidElementInfo { get; set; }

        public string Name { get { return AndroidElementInfo.ElementTitle; } }

        Object ITreeViewItem.NodeObject()
        {
            return AndroidElementInfo;
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();
            
            list.Add(new ActUIElement(){
                 Description = "Click",
                 ElementAction = ActUIElement.eElementAction.Click
            });

            list.Add(new ActUIElement()
            {
                 Description = "Get Custom Attribute text",
                 ElementAction = ActUIElement.eElementAction.GetAttrValue,
                 Value = "text"
            });
            
            return list;
        }

        StackPanel ITreeViewItem.Header()
        {
            //TODO: Pput better icon for generic control
            string ImageFileName = "@Agent_16x16.png";
            string Title = AndroidElementInfo.ElementTitle;
            return TreeViewUtils.CreateItemHeader(Title, ImageFileName);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> list = new List<ITreeViewItem>();
            List<ElementInfo> Childrens = AndroidElementInfo.WindowExplorer.GetElementChildren(AndroidElementInfo);

            foreach (ElementInfo EI in Childrens)
            {
                ITreeViewItem TVI = AndroidElementInfoConverter.GetTreeViewItemFor(EI);                                
                list.Add(TVI);
            }
            return list;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return AndroidElementInfo.XmlNode.HasChildNodes;            
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            //TODO: currently return the default generic page, later on create for Android
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
            return AndroidElementInfo.WindowExplorer.GetElementProperties(AndroidElementInfo);
        }

        public ObservableList<ActInputValue> GetItemSpecificActionInputValues()
        {
            return null;
        }
    }
}
