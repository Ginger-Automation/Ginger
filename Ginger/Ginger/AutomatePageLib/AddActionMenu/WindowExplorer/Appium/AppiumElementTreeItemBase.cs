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
using Ginger.Drivers.Common;
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerWPF.UserControlsLib.UCTreeView;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;

namespace Ginger.WindowExplorer.Appium
{
    public class AppiumElementTreeItemBase :  ITreeViewItem, IWindowExplorerTreeItem
    {
        public ITreeView TreeView
        {
            get;
            set;
        }

        public AppiumElementInfo AppiumElementInfo { get; set; }

        public string Name { get { return AppiumElementInfo.ElementTitle; } }

        Object ITreeViewItem.NodeObject()
        {
            return AppiumElementInfo;
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();
            
            list.Add(new ActGenElement(){
                 Description = "Click",
                 GenElementAction = ActGenElement.eGenElementAction.Click
            });

            list.Add(new ActGenElement(){
                 Description = "Get Custom Attribute text",
                 GenElementAction = ActGenElement.eGenElementAction.GetCustomAttribute,
                 Value = "text"
            });
            
            return list;
        }

        StackPanel ITreeViewItem.Header()
        {
            //TODO: Put better icon for generic control
            string ImageFileName = "@Agent_16x16.png";
            string Title = AppiumElementInfo.ElementTitle;
            return TreeViewUtils.CreateItemHeader(Title, ImageFileName);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> list = new List<ITreeViewItem>();
            List<ElementInfo> Childrens = AppiumElementInfo.WindowExplorer.GetElementChildren(AppiumElementInfo);

            foreach (ElementInfo EI in Childrens)
            {
                ITreeViewItem TVI = AppiumElementInfoConverter.GetTreeViewItemFor(EI);                                
                list.Add(TVI);
            }
            return list;



        }

        bool ITreeViewItem.IsExpandable()
        {
            return AppiumElementInfo.XmlNode.HasChildNodes;            
        }



        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            //TODO: currently return the default generic page, later on create for Appium elem
            return new ElementInfoPage(AppiumElementInfo);
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


            return AppiumElementInfo.WindowExplorer.GetElementProperties(AppiumElementInfo);
            
        }

        public ObservableList<ActInputValue> GetItemSpecificActionInputValues()
        {
            return null;
        }
    }
}
