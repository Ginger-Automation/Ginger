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
using GingerCore.ALM.QC;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Amdocs.Ginger.Common;

namespace Ginger.ALM.QC.TreeViewItems
{
    public class QCTestPlanFolderTreeItem : TreeViewItemBase, ITreeViewItem
    {
        public string Folder { get; set; }
        public string Path { get; set; }
        public static bool IsCreateBusinessFlowFolder { get; set; }

        private new ContextMenu mContextMenu = new ContextMenu();

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            if (Folder == "Subject")
            {                
                return TreeViewUtils.CreateItemHeader(Folder, "@WorkFlow_16x16.png");            
            }
            else
            {
                return TreeViewUtils.CreateItemHeader(Folder, "@Folder_16x16.png");            
            }
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();
            List<string> strParentFolders = ALMIntegration.Instance.GetTestPlanExplorer(Path);
            //Add QC folders to tree children             
            foreach (string sFolder in strParentFolders)
                {
                    QCTestPlanFolderTreeItem pfn = new QCTestPlanFolderTreeItem();
                    pfn.Folder = sFolder;
                    pfn.Path = Path + @"\" + sFolder;
                    Childrens.Add(pfn);                    
            }

            return Childrens;
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
            return mContextMenu;            
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            //Set Context Menu
            mContextMenu= new ContextMenu();
        }
    }
}
