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
using Ginger.WindowExplorer;
using Ginger.WindowExplorer.ASCF;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.ASCF;
using GingerCore.Drivers;
using GingerCore.Drivers.ASCF;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;

namespace Ginger.Actions.Locators.ASCF
{
    class ASCFBrowserTreeItem : TreeViewItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        public ASCFControlInfo ASCFControlInfo {get; set;}

        public string Name { get; set; }
        public string Path { get; set; }

        private ASCFBrowserInfoPage mASCFBrowserInfoPage;

        Object ITreeViewItem.NodeObject()
        {            
            return ASCFControlInfo;
        }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Browser_16x16.png";            
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);                        
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            //TODO: fix if been used  
            //ASCFDriver d = (ASCFDriver)((Agent)App.AutomateTabGingerRunner.ApplicationAgents[0].Agent).Driver;
            //d.SetCurrentBrowserControl(eLocateBy.ByName, Path);            
            //List<ASCFBrowserElementInfo> lst = d.GetBrowserElements();
            List<ASCFBrowserElementInfo> lst = null;
            if (lst == null)
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, "Cannot list Browser elements");
                return null;
            }

            List<ITreeViewItem> list = new List<ITreeViewItem>();
            foreach(ASCFBrowserElementInfo c in lst)
            {
                //TODO: add more handlers
                switch (c.ControlType)
                {
                    case ASCFBrowserElementInfo.eControlType.TextBox:
                        list.Add(new ASCFBrowserTextBoxTreeItem() { ASCFBrowserElementInfo = c });                    
                        break;
                    case ASCFBrowserElementInfo.eControlType.Link:
                        list.Add(new ASCFBrowserLinkTreeItem() { ASCFBrowserElementInfo = c});
                        break;
                    case ASCFBrowserElementInfo.eControlType.Button:
                        list.Add(new ASCFBrowserButtonTreeItem() { ASCFBrowserElementInfo = c});
                        break;
                    case ASCFBrowserElementInfo.eControlType.Label:
                        list.Add(new ASCFBrowserLabelTreeItem() { ASCFBrowserElementInfo = c});
                        break;
                    case ASCFBrowserElementInfo.eControlType.DropDown:
                        list.Add(new ASCFBrowserDropDownTreeItem() { ASCFBrowserElementInfo = c});
                        break;
                    case ASCFBrowserElementInfo.eControlType.CheckBox:
                        list.Add(new ASCFBrowserCheckBoxTreeItem() { ASCFBrowserElementInfo = c});
                        break;
                    default:
                        // Add generic element - unknown type...
                        list.Add(new ASCFBrowserElementTreeItem() { ASCFBrowserElementInfo = c});
                        break;
                }
            }
            return list;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mASCFBrowserInfoPage == null)
            {
                mASCFBrowserInfoPage = new ASCFBrowserInfoPage(Path);
            }
            return mASCFBrowserInfoPage;            
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return null;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();

            ActASCFBrowserElement BC = new ActASCFBrowserElement();
            BC.Description = "Set Active Browser";
            BC.LocateBy = eLocateBy.ByName;
            BC.LocateValue = Path;
            BC.ControlAction = ActASCFBrowserElement.eControlAction.SetBrowserControl;
            list.Add(BC);

            return list;
        }

        ObservableList<ControlProperty> IWindowExplorerTreeItem.GetElementProperties()
        {
            //TODO: temp solution fix me hard coded [0[]
                ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();
                return list;
        }

        public ObservableList<ActInputValue> GetItemSpecificActionInputValues()
        {
            return null;
        }
    }
}
