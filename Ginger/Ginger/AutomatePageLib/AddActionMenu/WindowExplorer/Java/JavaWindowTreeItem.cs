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
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Drivers.JavaDriverLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.Java
{
    class JavaWindowTreeItem : JavaElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        Object ITreeViewItem.NodeObject()
        {
            return base.JavaElementInfo;
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, ElementInfo.GetElementTypeImage(eElementType.Window));
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
            ObservableList<Act> list =
            [
                new ActSwitchWindow()
                {
                    Description = "Switch Window - " + Name
                },
                new ActWindow()
                {
                    Description = "Check If  " + Name + " Exist",
                    WindowActionType = ActWindow.eWindowActionType.IsExist
                },
                new ActWindow()
                {
                    Description = "Close Window-  " + Name,
                    WindowActionType = ActWindow.eWindowActionType.Close
                },
            ];
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
                string ErrMSG = Response.GetErrorValue();
                throw new Exception(ErrMSG);
            }

            if (Response.Name == "ControlProperties")
            {
                ObservableList<ControlProperty> list = [];
                List<PayLoad> props = Response.GetListPayLoad();
                foreach (PayLoad prop in props)
                {
                    string PropName = prop.GetValueString();
                    string PropValue = String.Empty;
                    if (PropName != "Value")
                    {
                        PropValue = prop.GetValueString();
                    }
                    else
                    {
                        List<String> valueList = prop.GetListString();
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