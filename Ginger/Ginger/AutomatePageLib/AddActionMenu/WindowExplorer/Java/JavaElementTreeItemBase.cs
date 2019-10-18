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

using Ginger.SolutionWindows.TreeViewItems;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Drivers.JavaDriverLib;
using System;
using System.Collections.Generic;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.WindowExplorer.HTMLCommon;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common;

namespace Ginger.WindowExplorer.Java
{
    public abstract class JavaElementTreeItemBase : TreeViewItemBase
    {
        public JavaElementInfo JavaElementInfo { get; set; }
        public string Name { get { return this.JavaElementInfo.ElementTitle; } set { this.JavaElementInfo.ElementTitle = value; } }
        public Boolean IsExpandable { get { return this.JavaElementInfo.IsExpandable; } set { this.JavaElementInfo.IsExpandable = value; } }

       public List<ITreeViewItem> Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();
            try
            {
                PayLoad Response = getChilderns();
                List<PayLoad> controls = Response.GetListPayLoad();
               
                if (Response.Name == "ContainerComponents")
                    Childrens = GetControlsAsTreeItems(controls);

                if (Response.Name == "EditorChildrens")
                    Childrens = GetHTMLControlsAsTreeItems(controls);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Not able to get children node", ex);
            }
            return Childrens;

        }

       List<ITreeViewItem> GetControlsAsTreeItems(List<PayLoad> controls)
       {
           List<ITreeViewItem> items = new List<ITreeViewItem>();
           foreach (PayLoad pl in controls)
           {
               ElementInfo CI = JavaDriver.GetControlInfoFromPayLoad(pl);
               CI.WindowExplorer = JavaElementInfo.WindowExplorer;  // pass the driver down to elements to use
               ITreeViewItem tvi = JavaElementInfoConverter.GetTreeViewItemFor(CI);
               items.Add(tvi);
           }
           return items;
       }

       List<ITreeViewItem> GetHTMLControlsAsTreeItems(List<PayLoad> controls)
       {
           List<ITreeViewItem> items = new List<ITreeViewItem>();

           foreach (PayLoad pl in controls)
           {
               ElementInfo CI = JavaDriver.GetHTMLElementInfoFromPL(pl);
               CI.WindowExplorer = JavaElementInfo.WindowExplorer;  // pass the driver down to elements to use
               ITreeViewItem tvi = HTMLElementInfoConverter.GetHTMLElementTreeItem(CI);
               items.Add(tvi);
           }
           return items;
       }

       private PayLoad getChilderns()
       {
            //TODO: J.G: Move this to Java Driver. why here ?

           JavaDriver d = (JavaDriver)JavaElementInfo.WindowExplorer;
           PayLoad Request = null;
            if (JavaElementInfo.ElementTypeEnum== eElementType.Browser)
            {
                    d.InitializeBrowser(JavaElementInfo);        
                Request = new PayLoad("GetElementChildren");
                Request.AddValue("");
                Request.AddValue("/");
                Request.ClosePackage();
            } 
            else if (JavaElementInfo.ElementTypeEnum == eElementType.EditorPane)
            {
                d.InitializeJEditorPane(JavaElementInfo);
                Request = new PayLoad(JavaDriver.CommandType.WindowExplorerOperation.ToString());
                Request.AddEnumValue(JavaDriver.WindowExplorerOperationType.GetEditorChildrens);
                Request.AddValue(JavaElementInfo.XPath);        
                Request.ClosePackage();
            }
           else
           {
               Request = new PayLoad(JavaDriver.CommandType.WindowExplorerOperation.ToString());
               Request.AddEnumValue(JavaDriver.WindowExplorerOperationType.GetContainerControls);
                Request.AddValue(JavaElementInfo.XPath);
                Request.ClosePackage();
           }
           PayLoad Response = d.Send(Request);

           if (Response.Name == "ERROR")
           {
               string ErrMsg = Response.GetValueString();
               throw new Exception(ErrMsg);
           }
           else
               return Response;
       }
    }
}