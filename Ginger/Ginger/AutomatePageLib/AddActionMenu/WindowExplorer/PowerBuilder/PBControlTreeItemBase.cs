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
using Ginger.Drivers.UIA;
using Ginger.Drivers.WindowsAutomation;
using Ginger.WindowExplorer;
using Ginger.WindowExplorer.PowerBuilder;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers.Common;
using GingerCore.Actions.Java;
using Ginger.WindowExplorer.Java;
using Ginger.WindowExplorer.HTMLCommon;
using GingerCore.Drivers.PBDriver;
using GingerCore.Drivers.WindowsLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;

namespace Ginger.Drivers.PowerBuilder
{
    public class PBControlTreeItemBase : AutomationElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
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

        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Agent_16x16.png";
            string Title = UIAElementInfo.ElementTitle;
            return TreeViewUtils.CreateItemHeader(Title, ImageFileName);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> list = new List<ITreeViewItem>();
            List<ElementInfo> Childrens = base.UIAElementInfo.WindowExplorer.GetElementChildren(base.UIAElementInfo);

            foreach (ElementInfo EI in Childrens)
            {
                ITreeViewItem TVI=null;
                if (EI.GetType() == typeof(UIAElementInfo))
                {
                    EI.WindowExplorer = UIAElementInfo.WindowExplorer;
                    PBControlTreeItemBase treeItem = GetMatchingPBTreeItem(EI);
                    treeItem.UIAElementInfo.WindowExplorer = UIAElementInfo.WindowExplorer;
                   
                    double XOffset =
                        double.Parse(((UIAutomationDriverBase)UIAElementInfo.WindowExplorer).mUIAutomationHelper
                            .GetControlPropertyValue(EI.ElementObject, "XOffset"));
                    double YOffset =
                        double.Parse(((UIAutomationDriverBase)UIAElementInfo.WindowExplorer).mUIAutomationHelper
                            .GetControlPropertyValue(EI.ElementObject, "YOffset"));
                    treeItem.UIAElementInfo.XCordinate = XOffset - base.UIAElementInfo.XCordinate;
                    treeItem.UIAElementInfo.YCordinate = YOffset - base.UIAElementInfo.YCordinate;

                    TVI = treeItem;
                }
                else
                {
                    TVI = HTMLElementInfoConverter.GetHTMLElementTreeItem(EI);
                }
                list.Add(TVI);
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
            TV.AddToolbarTool(eImageType.Refresh, "Refresh", TV.Tree.RefreshSelectedTreeNodeChildrens);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            // Base class returns empty list. sub classed to return application action list
            ObservableList<Act> list = new ObservableList<Act>();
            return list;
        }

        ObservableList<ControlProperty> IWindowExplorerTreeItem.GetElementProperties()
        {
            return ((UIAutomationDriverBase)(UIAElementInfo.WindowExplorer)).mUIAutomationHelper.GetElementProperties(UIAElementInfo.ElementObject);
        }

        public static PBControlTreeItemBase GetMatchingPBTreeItem(ElementInfo elementInfo)
        {
            if (elementInfo==null || elementInfo.ElementObject == null) return null;

            string elementControlType = ((PBDriver)elementInfo.WindowExplorer).mUIAutomationHelper.GetElementControlType(elementInfo.ElementObject);
            string elmentClass =
                ((PBDriver) elementInfo.WindowExplorer).mUIAutomationHelper.GetControlPropertyValue(
                    elementInfo.ElementObject, "ClassName");

            PBControlTreeItemBase treeItem;
            if (General.CompareStringsIgnoreCase(elementControlType,"button"))
            {
                treeItem = new PBButtonTreeItem();             
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "edit"))
            {
                treeItem = new PBTextBoxTreeItem();
                
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "title bar"))
            {
                treeItem = new PBTitleBarTreeItem();
                
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "pane") && General.CompareStringsIgnoreCase(elmentClass, "SysDateTimePick32"))
            {
                treeItem = new PBDatePickerTreeItem();
                
            }

             //TODO:For grids need to implement generic way, independent of class name.This is breaking other pane controls which are not actually Grids. 
            //else if (elementNode.Current.LocalizedControlType == "pane" && elementNode.Current.ClassName == "pbdw126")
            //{
            //    PBDataGridTreeItem DGTI = new PBDataGridTreeItem();
            //    DGTI.AEControl = elementNode;
            //    Childrens.Add(DGTI);
            //}

            else if (General.CompareStringsIgnoreCase(elementControlType, "pane") && General.CompareStringsIgnoreCase(elmentClass, "PBTabControl32_100"))
            {
                treeItem = new PBTabTreeItem();
            }
            //TODO: Find a Better way to distinguish a grid control
            else if (General.CompareStringsIgnoreCase(elementControlType,"pane") && (elmentClass.StartsWith("pbd")))// && (elementNode.Current.Name=="") 
            {
                treeItem = new PBDataGridTreeItem();
            }
            else if(elmentClass.Equals("Internet Explorer_Server"))
            {               
                treeItem = new PBBrowserTreeItem();
            }                
            else if (General.CompareStringsIgnoreCase(elementControlType, "text"))
            {
                treeItem = new PBTextTreeItem();
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "scroll bar"))
            {
                treeItem = new PBScrollBarTreeItem();
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "menu bar"))
            {
                treeItem = new PBMenuBarTreeItem();
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "menu item"))
            {
                treeItem = new PBMenuItemTreeItem();
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "check box"))
            {
                treeItem = new PBCheckBoxTreeItem();
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "radio button"))
            {
                treeItem = new PBRadioButtonTreeItem();
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "combo box"))
            {
                treeItem = new PBComboBoxTreeItem();
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "list"))
            {
                treeItem = new PBListBoxTreeItem();
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "list item"))
            {
                treeItem = new PBListItemTreeItem();
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "Dialog"))
            {
                treeItem = new PBDialogBoxTreeItem();
            }
            //TODO: Add special handling for table controls. Don't display table fields as child, instead create similar table on Explorer pages using these values
            else if (General.CompareStringsIgnoreCase(elementControlType, "table"))
            {
                treeItem = new PBTableTreeItem();
            }
            else if (General.CompareStringsIgnoreCase(elementControlType, "document") || (elementControlType == ""))
            {
                treeItem = new PBDocumentTreeItem();
            }
            else
            {
                treeItem = new PBControlTreeItemBase();
            }

            // we need to put all the minimal attr so calc when needed
            UIAElementInfo EI = new UIAElementInfo();          
            EI.ElementObject = elementInfo.ElementObject;  // The most important, the rest will be lazy loading
            treeItem.UIAElementInfo = EI;

            return treeItem;
        }

        public ObservableList<ActInputValue> GetItemSpecificActionInputValues()
        {
            return null;
        }
    }
}
