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
using System.Linq;
using System.Windows.Controls;
using Ginger.Drivers.Common;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Drivers.JavaDriverLib;
using GingerWPF.UserControlsLib.UCTreeView;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;

namespace Ginger.WindowExplorer.Java
{
    // Will serve as base class for Java Element, or will default to this class when there is no specific control type yet
    public class JavaElementTreeItem : JavaElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        Object ITreeViewItem.NodeObject()
        {
            return base.JavaElementInfo;
        }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "ASCF16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }
      
        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return base.Childrens();
        }

        bool ITreeViewItem.IsExpandable()
        {
            return IsExpandable;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            //Override if needed with specific Element Info Page
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
            // Will be overridden in derived class
            ObservableList<Act> list = new ObservableList<Act>();
            ActUIElement act = new ActUIElement();
            act.AddNewReturnParams = true;
            act.Description = "Get Control Property for " + Name;
            act.ElementType = eElementType.Unknown;
            act.ElementAction = ActUIElement.eElementAction.GetControlProperty;
            act.AddOrUpdateInputParamValue(ActUIElement.Fields.ValueToSelect, ActUIElement.eElementProperty.Value.ToString());
            list.Add(act);
            return list;
        }

        ObservableList<ControlProperty> IWindowExplorerTreeItem.GetElementProperties()
        {
            PayLoad Request = new PayLoad(JavaDriver.CommandType.WindowExplorerOperation.ToString());
            Request.AddEnumValue(JavaDriver.WindowExplorerOperationType.GetProperties);
            Request.AddValue("ByXPath");
            Request.AddValue(JavaElementInfo.XPath);
            Request.ClosePackage();
            
            JavaDriver d = (JavaDriver)JavaElementInfo.WindowExplorer;    
            PayLoad Response = d.Send(Request);
            if (Response.IsErrorPayLoad())
            {
                string ErrMSG = Response.GetValueString();
                return null;  
            }
            
            if (Response.Name == "ControlProperties")
            {
                ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();
                List<PayLoad> props = Response.GetListPayLoad();
                foreach(PayLoad prop in props)
                {
                    string PropName = prop.GetValueString();
                    string PropValue = String.Empty;
                    if(PropName!="Value")
                    {
                        PropValue = prop.GetValueString();                        
                    }
                    else
                    {
                        List<String> valueList = prop.GetListString();
                        if(valueList.Count!=0)
                            PropValue = valueList.ElementAt(0);
                    }
                   
                    list.Add(new ControlProperty() { Name = PropName, Value = PropValue });
                }
                return list;
            }
            else
            {
                //TODO: handle err
                return null;
            }
        }

        public ObservableList<ActInputValue> GetItemSpecificActionInputValues()
        {
            return null;
        }
    }
}
