#region License
/*
Copyright © 2014-2023 European Support Limited

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

using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using amdocs.ginger.GingerCoreNET;
using ZephyrEntStdSDK.Models.Base;

namespace Ginger.ALM.ZephyrEnt.TreeViewItems
{
    public class ZephyrEntPhaseTreeItem : ZephyrEntTreeItem, ITreeViewItem
    {
        private bool isExpandable = false;
        public bool AlreadyImported { get; set; }
        public BusinessFlow MappedBusinessFlow { get; set; }
        public String Path { get; set; }
        public string Folder { get; set; }
        public string TestSetName { get; set; }
        public string TestSetID { get; set; }
        public string FatherId { get; set; }
        public List<string[]> TestSetStatuses { get; set; }
        public string MappedBusinessFlowPath { get; set; }
        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        public ZephyrEntPhaseTreeItem(BaseResponseItem node) : base(node)
        {
            this.Folder = node.TryGetItem("name").ToString();
            this.TestSetName = node.TryGetItem("name").ToString();
            this.TestSetID = node.TryGetItem("id").ToString();
            this.TestSetStatuses = new List<string[]>();
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, "@TestSet_16x16.png");
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return CurrentChildrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return isExpandable;
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
            // there is not tools needed at this stage
        }
        public void IsTestSetAlreadyImported()
        {
            AlreadyImported = false;
            MappedBusinessFlow = null;
            MappedBusinessFlowPath = "None";
            foreach (BusinessFlow bf in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>())
            {
                if (bf.ExternalID != null)
                {
                    if (bf.ExternalID == this.TestSetID)
                    {
                        AlreadyImported = true;
                        MappedBusinessFlow = bf;
                        MappedBusinessFlowPath = bf.ContainingFolder + '\\' + bf.Name;
                        break;
                    }
                }
            }
        }
    }
}