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
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using Open3270.TN3270;
using System;
using System.Windows.Controls;
using GingerWPF.UserControlsLib.UCTreeView;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.WindowExplorer.Mainframe
{
    class MainframeControlTreeItem:MainframeTreeItemBase,ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@TextBox_16x16.png";
            return TreeViewUtils.CreateItemHeader (Name, ImageFileName);
        }

        public XMLScreenField XSF { get; set; }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<GingerCore.Actions.Act> ACL = new ObservableList<GingerCore.Actions.Act> ();

            if (XSF.Attributes.FieldType != "Hidden")
            {
                GingerCore.Actions.MainFrame.ActMainframeGetDetails ACMGD = new GingerCore.Actions.MainFrame.ActMainframeGetDetails ();
                ACMGD.LocateBy = eLocateBy.ByCaretPosition;
                ACMGD.DetailsToFetch = GingerCore.Actions.MainFrame.ActMainframeGetDetails.eDetailsToFetch.GetDetailsFromText;
                ACMGD.LocateValue = XSF.Location.position.ToString ();
                ACL.Add (ACMGD);

                GingerCore.Actions.MainFrame.ActMainframeGetDetails ACMGD2 = new GingerCore.Actions.MainFrame.ActMainframeGetDetails ();
                ACMGD2.LocateBy = eLocateBy.ByCaretPosition;
                ACMGD2.DetailsToFetch = GingerCore.Actions.MainFrame.ActMainframeGetDetails.eDetailsToFetch.GetText;
                ACMGD2.LocateValue = XSF.Location.ToString ();
                ACL.Add (ACMGD2);
            }

            if (XSF.Attributes.Protected == false)
            {
                GingerCore.Actions.MainFrame.ActMainframeSetText AMFT = new GingerCore.Actions.MainFrame.ActMainframeSetText ();
                AMFT.LocateBy = eLocateBy.ByCaretPosition;
                AMFT.LocateValue = XSF.Location.position.ToString ();
                ACL.Add (AMFT);
            }
            return ACL;
        }

         ObservableList<ControlProperty> IWindowExplorerTreeItem.GetElementProperties()
        {
            ObservableList<ControlProperty> CPL = new ObservableList<ControlProperty> ();

            ControlProperty CP1 = new ControlProperty ();
            CP1.Name = "Caret Position";
            CP1.Value = XSF.Location.position.ToString();
            CPL.Add (CP1);

            ControlProperty CP2 = new ControlProperty ();
            CP2.Name = "X/Y";
            CP2.Value = XSF.Location.left.ToString () + "/" + XSF.Location.top.ToString ();
            CPL.Add (CP2);


            ControlProperty CP3 = new ControlProperty ();
            CP3.Name = "Read Only";
            CP3.Value = XSF.Attributes.Protected.ToString();
            CPL.Add (CP3);
            return CPL;
         }

         Object ITreeViewItem.NodeObject()
         {
            ElementInfo EI = new ElementInfo();
             EI.ElementTitle=XSF.Text;
             
             if(XSF.Attributes.Protected==true)
             {
                 EI.ElementType="ReadOnly";
             }
             else if (XSF.Attributes.FieldType=="Hidden")
             {
                 EI.ElementType="Password";
             }
             else 
             {
                 EI.ElementType="Text Field";
             }

             EI.IsExpandable=false;
             EI.Path=XSF.Location.position.ToString();
             EI.XPath = "";
             return EI;
         }
    }
}
