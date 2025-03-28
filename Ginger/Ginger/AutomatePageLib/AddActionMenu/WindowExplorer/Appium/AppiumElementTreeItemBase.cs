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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Drivers.Common;
using GingerCore.Actions;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml;

namespace Ginger.WindowExplorer.Appium
{
    public class AppiumElementTreeItemBase : ITreeViewItem, IWindowExplorerTreeItem
    {
        public ITreeView TreeView
        {
            get;
            set;
        }

        public ElementInfo ElementInfo { get; set; }

        public string Name { get { return ElementInfo.ElementTitle; } }

        Object ITreeViewItem.NodeObject()
        {
            return ElementInfo;
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list =
            [
                new ActGenElement()
                {
                    Description = "Click",
                    GenElementAction = ActGenElement.eGenElementAction.Click
                },
                new ActGenElement()
                {
                    Description = "Get Custom Attribute text",
                    GenElementAction = ActGenElement.eGenElementAction.GetCustomAttribute,
                    Value = "text"
                },
            ];

            return list;
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(ElementInfo.ElementTitle, Amdocs.Ginger.Common.Enums.eImageType.Agent);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> list = [];
            List<ElementInfo> Childrens = ElementInfo.WindowExplorer.GetElementChildren(ElementInfo);

            foreach (ElementInfo EI in Childrens)
            {
                ITreeViewItem TVI = AppiumElementInfoConverter.GetTreeViewItemFor(EI);
                list.Add(TVI);
            }
            return list;



        }

        bool ITreeViewItem.IsExpandable()
        {
            return (ElementInfo.ElementObject as XmlNode).HasChildNodes;
        }



        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            //TODO: currently return the default generic page, later on create for Appium elem
            return new ElementInfoPage(ElementInfo);
        }


        ContextMenu ITreeViewItem.Menu()
        {
            //ContextMenu CM = new ContextMenu();
            //base.AddRefreshMenuItem(CM, Path);
            //TreeView2.CreateMenuItem(CM, "Add New Agent", AddNewAgent, this);
            //base.AddCheckInMenuItem(CM, Path);
            //base.AddViewFolderFilesMenuItem(CM, Path);
            //return CM;
            return null;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            //mTV = TV;

            // TV.AddToolbarTool("@Refresh_16x16.png", "Refresh", TV.RefreshTreeNodeChildren);
            //TV.AddToolbarTool("@Add_16x16.png", "Add New Agent", AddNewAgent);
            //TV.AddToolbarTool("@CheckIn_16x16.png", "Check-In", CheckIn);
            //TV.AddToolbarTool("@Glass_16x16.png", "Open folder in File Explorer", ViewFolderFilesFromTool, System.Windows.Visibility.Visible, Path);
        }


        ObservableList<ControlProperty> IWindowExplorerTreeItem.GetElementProperties()
        {


            return ElementInfo.WindowExplorer.GetElementProperties(ElementInfo);

        }

        public ObservableList<ActInputValue> GetItemSpecificActionInputValues()
        {
            return null;
        }
    }
}
