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
using Ginger.Actions.Locators.ASCF;
using Ginger.WindowExplorer.ASCF.ASCFBrowserElements;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.ASCF;
using GingerCore.Drivers;
using GingerCore.Drivers.ASCF;
using HtmlAgilityPack;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.WindowExplorer.ASCF
{
    class ASCFBrowserElementTreeItem : ASCFControlTreeItem, ITreeViewItem, IWindowExplorerTreeItem
    {
        public ASCFBrowserElementInfo ASCFBrowserElementInfo { get; set; }

        ASCFBrowserElementInfoPage mASCFBrowserElementInfoPage;

        public new string Name { get { return ASCFBrowserElementInfo.Name; } }

        StackPanel ITreeViewItem.Header()
        {
            //TODO: put text icon
            string ImageFileName = "@Info_16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }
        
        Object ITreeViewItem.NodeObject()
        {
            return ASCFBrowserElementInfo;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return null;
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();

            if (!string.IsNullOrEmpty(ASCFBrowserElementInfo.GetProperty("id")))
            {
                list.Add(new ActASCFBrowserElement()
                {
                    Description = "Click",
                    LocateBy = eLocateBy.ByID,
                    LocateValue = ASCFBrowserElementInfo.GetProperty("id"),
                    ControlAction = ActASCFBrowserElement.eControlAction.Click
                });
            }

            if (!string.IsNullOrEmpty(ASCFBrowserElementInfo.GetProperty("name")))
            {
                list.Add(new ActASCFBrowserElement()
                {
                    Description = "Click",
                    LocateBy = eLocateBy.ByName,
                    LocateValue = ASCFBrowserElementInfo.GetProperty("name"),
                    ControlAction = ActASCFBrowserElement.eControlAction.Click
                });
            }

            list.Add(new ActASCFBrowserElement()
            {
                Description = "Click",
                LocateBy = eLocateBy.ByXPath,
                LocateValue = ASCFBrowserElementInfo.GetProperty("XPath"),
                ControlAction = ActASCFBrowserElement.eControlAction.Click
            });
            return list;
        }

        ObservableList<ControlProperty> IWindowExplorerTreeItem.GetElementProperties()
        {
            return ASCFBrowserElementInfo.Properties;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mASCFBrowserElementInfoPage == null)
            {
                mASCFBrowserElementInfoPage = new ASCFBrowserElementInfoPage(ASCFBrowserElementInfo);
            }
            return mASCFBrowserElementInfoPage;
        }
    }
}
