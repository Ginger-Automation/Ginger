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
using Ginger.Drivers.WindowsAutomation;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerCore.Drivers.ASCF;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;

namespace Ginger.Actions.Locators.ASCF
{
    class ASCFFormTreeItem : TreeViewItemBase, ITreeViewItem , IWindowExplorerTreeItem
    {
        public ASCFDriver ASCFDriver { get; set; }

        public string Name { get; set; }
        public string Path { get; set; }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Window_16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }


        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            //TODO: fix if needed
            //ASCFDriver = (ASCFDriver)((Agent)App.AutomateTabGingerRunner.ApplicationAgents[0].Agent).Driver;
            //string rc = ASCFDriver.Send("GetFormControls", " ", Path, " ", " ", false);

            string rc = string.Empty;
            List <ITreeViewItem> Childrens = new List<ITreeViewItem>();            
            string controls;
            if (rc.StartsWith("OK"))
            {
                controls = rc.Substring(3);
            }
            else
            {
                //TODO: messagebox err
                return null;
            }
            string[] a = controls.Split('~');
            foreach(string control in a)
            {
                if (control.Length > 0)
                {
                    string[] ControlInfo = control.Split('@');

                    if (ControlInfo[1] == "class com.amdocs.uif.widgets.TextFieldNative")
                    {
                        ASCFTextBoxTreeItem ACTI = new ASCFTextBoxTreeItem();
                        ACTI.Name = ControlInfo[0];
                        ACTI.Path = ControlInfo[2];
                        ACTI.ASCFControlInfo = new ASCFControlInfo() { Name = ACTI.Name, Path = ACTI.Path, ControlType = ASCFControlInfo.eControlType.TextBox };
                        Childrens.Add(ACTI);
                    }
                    else if (ControlInfo[1] == "class com.amdocs.uif.widgets.UifForm")
                    {
                        ASCFFormTreeItem ACFI = new ASCFFormTreeItem();
                        ACFI.Name = ControlInfo[0];
                        ACFI.Path = ControlInfo[2];                       
                        ACFI.ASCFDriver = ASCFDriver;
                        Childrens.Add(ACFI);
                    }
                    else if (ControlInfo[1] == "SubForm")
                    {
                        ASCFFormTreeItem ACFF = new ASCFFormTreeItem();
                        ACFF.Name = ControlInfo[0];
                        ACFF.Path = ControlInfo[2];
                        ACFF.ASCFDriver = ASCFDriver;
                        Childrens.Add(ACFF);
                    }
                    else if (ControlInfo[1] == "class com.amdocs.uif.widgets.BrowserNative")
                    {
                        ASCFBrowserTreeItem ABTT = new ASCFBrowserTreeItem();
                        ABTT.Name = ControlInfo[0];
                        ABTT.Path = ControlInfo[2];
                        ABTT.ASCFControlInfo = new ASCFControlInfo() { Name = ABTT.Name, Path = ABTT.Path, ControlType = ASCFControlInfo.eControlType.Browser };
                        Childrens.Add(ABTT);
                    }
                    else if (ControlInfo[1] == "class com.amdocs.uif.widgets.LabelNative")
                    {
                        ASCFLabelTreeItem ACTI = new ASCFLabelTreeItem();
                        ACTI.Name = ControlInfo[0];
                        ACTI.Path = ControlInfo[2];
                        ACTI.ASCFControlInfo = new ASCFControlInfo() { Name = ACTI.Name, Path = ACTI.Path, ControlType = ASCFControlInfo.eControlType.Label };
                        Childrens.Add(ACTI);
                    }
                    else if (ControlInfo[1] == "class com.amdocs.uif.widgets.SearchGridNative")
                    {
                        ASCFGridTreeItem ACTI = new ASCFGridTreeItem();
                        ACTI.Name = ControlInfo[0];
                        ACTI.Path = ControlInfo[2];
                        ACTI.ASCFControlInfo = new ASCFControlInfo() { Name = ACTI.Name, Path = ACTI.Path, ControlType = ASCFControlInfo.eControlType.Grid };
                        Childrens.Add(ACTI);
                    }
                    // TODO: add other types...    

                    else
                    {
                        ASCFControlTreeItem ACTI = new ASCFControlTreeItem();
                        ACTI.Name = ControlInfo[0];
                        ACTI.Path = ControlInfo[2];
                        ACTI.ASCFControlInfo = new ASCFControlInfo() { Name = ACTI.Name, Path = ACTI.Path };
                        Childrens.Add(ACTI);
                    }
                }
            }
            return Childrens;
        }


        bool ITreeViewItem.IsExpandable()
        {
            return true;
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

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();

            ActWindow a1 = new ActWindow();
            a1.Description = "Check window exist " + Path;
            a1.LocateBy = eLocateBy.ByTitle;
            a1.LocateValue = Path;
            a1.WindowActionType = ActWindow.eWindowActionType.IsExist;
            list.Add(a1);

            ActWindow a2 = new ActWindow();
            a2.Description = "Close window " + Path;
            a2.LocateBy = eLocateBy.ByTitle;
            a2.LocateValue = Path;
            a2.WindowActionType = ActWindow.eWindowActionType.Close;
            list.Add(a2);

            ActWindow a3 = new ActWindow();
            a3.Description = "Switch to window " + Path;
            a3.LocateBy = eLocateBy.ByTitle;
            a3.LocateValue = Path;
            a3.WindowActionType = ActWindow.eWindowActionType.Switch;
            list.Add(a3);

            ActWindow a4 = new ActWindow();
            a4.Description = "Minimize window " + Path;
            a4.LocateBy = eLocateBy.ByTitle;
            a4.LocateValue = Path;
            a4.WindowActionType = ActWindow.eWindowActionType.Minimize;
            list.Add(a4);

            ActWindow a5 = new ActWindow();
            a5.Description = "Maximize window " + Path;
            a5.LocateBy = eLocateBy.ByTitle;
            a5.LocateValue = Path;
            a5.WindowActionType = ActWindow.eWindowActionType.Maximize;
            list.Add(a5);

            return list;
        }

        ObservableList<ControlProperty> IWindowExplorerTreeItem.GetElementProperties()
        {
            return null;
        }

        public ObservableList<ActInputValue> GetItemSpecificActionInputValues()
        {
            return null;
        }
    }
}
