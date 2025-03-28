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
using GingerCore.Actions;
using GingerWPF.UserControlsLib.UCTreeView;
using Open3270.TN3270;
using System;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.Mainframe
{
    class MainframeControlTreeItem : MainframeTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, ElementInfo.GetElementTypeImage(eElementType.TextBox));
        }

        public XMLScreenField XSF { get; set; }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<GingerCore.Actions.Act> ACL = [];

            if (XSF.Attributes.FieldType != "Hidden")
            {
                GingerCore.Actions.MainFrame.ActMainframeGetDetails ACMGD = new GingerCore.Actions.MainFrame.ActMainframeGetDetails
                {
                    LocateBy = eLocateBy.ByCaretPosition,
                    DetailsToFetch = GingerCore.Actions.MainFrame.ActMainframeGetDetails.eDetailsToFetch.GetDetailsFromText,
                    LocateValue = XSF.Location.position.ToString()
                };
                ACL.Add(ACMGD);

                GingerCore.Actions.MainFrame.ActMainframeGetDetails ACMGD2 = new GingerCore.Actions.MainFrame.ActMainframeGetDetails
                {
                    LocateBy = eLocateBy.ByCaretPosition,
                    DetailsToFetch = GingerCore.Actions.MainFrame.ActMainframeGetDetails.eDetailsToFetch.GetText,
                    LocateValue = XSF.Location.ToString()
                };
                ACL.Add(ACMGD2);
            }

            if (XSF.Attributes.Protected == false)
            {
                GingerCore.Actions.MainFrame.ActMainframeSetText AMFT = new GingerCore.Actions.MainFrame.ActMainframeSetText
                {
                    LocateBy = eLocateBy.ByCaretPosition,
                    LocateValue = XSF.Location.position.ToString()
                };
                ACL.Add(AMFT);
            }
            return ACL;
        }

        ObservableList<ControlProperty> IWindowExplorerTreeItem.GetElementProperties()
        {
            ObservableList<ControlProperty> CPL = [];

            ControlProperty CP1 = new ControlProperty
            {
                Name = "Caret Position",
                Value = XSF.Location.position.ToString()
            };
            CPL.Add(CP1);

            ControlProperty CP2 = new ControlProperty
            {
                Name = "X/Y",
                Value = XSF.Location.left.ToString() + "/" + XSF.Location.top.ToString()
            };
            CPL.Add(CP2);


            ControlProperty CP3 = new ControlProperty
            {
                Name = "Read Only",
                Value = XSF.Attributes.Protected.ToString()
            };
            CPL.Add(CP3);
            return CPL;
        }

        Object ITreeViewItem.NodeObject()
        {
            ElementInfo EI = new ElementInfo
            {
                ElementTitle = XSF.Text
            };

            if (XSF.Attributes.Protected == true)
            {
                EI.ElementType = "ReadOnly";
            }
            else if (XSF.Attributes.FieldType == "Hidden")
            {
                EI.ElementType = "Password";
            }
            else
            {
                EI.ElementType = "Text Field";
            }

            EI.IsExpandable = false;
            EI.Path = XSF.Location.position.ToString();
            EI.XPath = "";
            return EI;
        }
    }
}
