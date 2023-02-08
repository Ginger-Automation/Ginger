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

namespace Ginger.ALM.QC.TreeViewItems
{
    public class QCTestLabTreeItem : TreeViewItemBase, ITreeViewItem
    {
        public BusinessFlow QCExplorer { get; set; }

        Object ITreeViewItem.NodeObject()
        {
            return QCExplorer;
        }

        StackPanel ITreeViewItem.Header()
        {
            StackPanel mHeader = null;
            if(mHeader != null) mHeader.Children.Add(new Label() { Content = "root QC" });
            return mHeader;
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return null;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
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
    }
}
