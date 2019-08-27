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
using GingerCore.Drivers;
using GingerCore.Drivers.ASCF;
using GingerWPF.UserControlsLib.UCTreeView;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;

namespace Ginger.Actions.Locators.ASCF
{
    //Base class for ASCF controls 
    class ASCFControlTreeItem : TreeViewItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        public ASCFControlInfo ASCFControlInfo {get; set;}

        public string Name { get; set; }
        public string Path {get; set;}

        private ASCFControlInfoPage mASCFControlInfoPage;

        Object ITreeViewItem.NodeObject()
        {            
            return ASCFControlInfo;
        }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "ASCF16x16.png";            
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);                        
        }


        List<ITreeViewItem> ITreeViewItem.Childrens()
        {            
            return null;            
        }
             

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {            
            if (mASCFControlInfoPage == null)
            {
                mASCFControlInfoPage = new ASCFControlInfoPage(ASCFControlInfo);
            }
            return mASCFControlInfoPage;            
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
            // Will be overridden in derived class
             ObservableList<Act> list = new ObservableList<Act>();
            return list;
        }

        ObservableList<ControlProperty> IWindowExplorerTreeItem.GetElementProperties()
        {
            //TODO: fix if needed
            //ASCFDriver d = (ASCFDriver)((Agent)App.AutomateTabGingerRunner.ApplicationAgents[0].Agent).Driver;
            //string RC = d.Send("GetControlInfo", "ByName" + "", ASCFControlInfo.Path, " ", " ", false);
            string RC = string.Empty;

            if (RC.StartsWith("OK"))
            {
                string vals = RC.Substring(3);
                string[] valArr = vals.Split('~');
                ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();
                foreach (string pv in valArr)
                {
                    if (pv.Length > 0)
                    {
                        string[] prop = pv.Split('=');
                        list.Add(new ControlProperty() { Name = prop[0], Value = prop[1] });
                    }
                }
                return list;
            }
            else
            {
                return null;
            }
        }

        public ObservableList<ActInputValue> GetItemSpecificActionInputValues()
        {
            return null;
        }
    }
}
