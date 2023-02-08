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

using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;
using GingerCore.ALM.Qtest;

namespace Ginger.ALM.Qtest.TreeViewItems
{
    public class QtestCycleTreeItem : TreeViewItemBase, ITreeViewItem
    {
        public BusinessFlow QCExplorer { get; set; }
        public List<ITreeViewItem> currentChildrens = null;

        public string ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        public QtestCycleTreeItem(List<QTestAPIStdModel.TestCycleResource> childrenCycles = null, List<QTestAPIStdModel.TestSuiteWithCustomFieldResource> childrenSuites = null)
        {
            currentChildrens = new List<ITreeViewItem>();

            if (childrenCycles != null)
            {
                foreach (QTestAPIStdModel.TestCycleResource cycle in childrenCycles)
                {
                    QtestCycleTreeItem pfn = new QtestCycleTreeItem(cycle.TestCycles, cycle.TestSuites);
                    pfn.Name = cycle.Name.ToString();
                    pfn.ID = cycle.Id.ToString();
                    pfn.Path = cycle.Path.ToString();
                    currentChildrens.Add(pfn);
                }
            }

            if (childrenSuites != null)
            {
                foreach (QTestAPIStdModel.TestSuiteWithCustomFieldResource suite in childrenSuites)
                {
                    QtestSuiteTreeItem pfn = new QtestSuiteTreeItem();
                    pfn.Name = suite.Name.ToString();
                    pfn.ID = suite.Id.ToString();
                    currentChildrens.Add(pfn);
                }
            }
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, "@WorkFlow_16x16.png");          
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return currentChildrens;
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
            // there is not tools needed at this stage
        }
    }
}
