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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using GingerCore.Drivers.MainFrame;
using GingerWPF.UserControlsLib.UCTreeView;
using Open3270.TN3270;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginger.WindowExplorer.Mainframe
{
    class MainframeTreeItemBase : TreeViewItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        public MainFrameDriver MFDriver { get; set; }

        public string Name { get; set; }
        public string Path { get; set; }

        public ObservableList<GingerCore.Actions.Act> GetElementActions()
        {
            ObservableList<GingerCore.Actions.Act> ACL = [];
            return ACL;
        }

        public ObservableList<ControlProperty> GetElementProperties()
        {
            throw new NotImplementedException();
        }

        public List<ITreeViewItem> Childrens()
        {
            List<ITreeViewItem> Childrens = [];
            //TODO: improve below to use really automate page used mainfram driver
            Agent agent = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().FirstOrDefault(x => x.DriverType == Agent.eDriverType.MainFrame3270 && ((AgentOperations)x.AgentOperations).Status == Agent.eStatus.Running);
            if (agent != null)
            {
                MFDriver = (MainFrameDriver)((AgentOperations)agent.AgentOperations).Driver;
                XMLScreen XMLS = MFDriver.GetRenderedScreen();
                foreach (XMLScreenField xf in XMLS.Fields)
                {
                    MainframeControlTreeItem MFTI = new MainframeControlTreeItem
                    {
                        Name = xf.Text,
                        XSF = xf,
                        Path = xf.Location.left + "/" + xf.Location.top
                    };
                    Childrens.Add(MFTI);
                }
            }
            return Childrens;
        }

        public System.Windows.Controls.Page EditPage(Context mContext)
        {
            return null;
        }

        public System.Windows.Controls.StackPanel Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, ElementInfo.GetElementTypeImage(eElementType.Window));
        }

        public bool IsExpandable()
        {
            return true;
        }

        public System.Windows.Controls.ContextMenu Menu()
        {
            throw new NotImplementedException();
        }

        public object NodeObject()
        {
            return null;
        }

        public void SetTools(ITreeView TV)
        {
            return;
        }

        public ObservableList<ActInputValue> GetItemSpecificActionInputValues()
        {
            return null;
        }
    }
}