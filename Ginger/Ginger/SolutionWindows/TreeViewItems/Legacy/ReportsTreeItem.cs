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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Ginger.Reports;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class ReportsTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private ReportTemplatesPage mReportTemplatesPage;

        string Path =  WorkSpace.Instance.Solution.Folder;

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }
        override public string NodePath()
        {
            return Path + @"\";
        }
        override public Type NodeObjectType()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader("Report Templates", "@Report2_16x16.png", Ginger.SourceControl.SourceControlUI.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
        }
        
        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            throw new NotImplementedException();            
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            return mReportTemplatesPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        internal void AddItemHandler(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            TreeViewUtils.AddMenuItem(mContextMenu, "Refresh", RefreshItems, null, eImageType.Refresh);
            mTreeView.AddToolbarTool(eImageType.Refresh, "Refresh", RefreshItems);
        }

        private void RefreshItems(object sender, System.Windows.RoutedEventArgs e)
        {
            //App.LocalRepository.RefreshSolutionHTMLReportConfigurationsCache(specificFolderPath: Path);
            //mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
        }
    }
}
