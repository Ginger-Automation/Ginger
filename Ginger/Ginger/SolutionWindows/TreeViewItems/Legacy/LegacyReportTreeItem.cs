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

using Ginger.Reports;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class LegacyReportTreeItem : TreeViewItemBase, ITreeViewItem
    {
        private ReportTemplatesPage mReportTemplatesPage;
        public string Folder { get; set; }
        public string Path { get; set; }
        
        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader("Legacy Report", "@Report_16x16.png", Ginger.SourceControl.SourceControlUI.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            //Add Reports
            ReportTemplatesTreeItem REPRI = new ReportTemplatesTreeItem(null);            
            Childrens.Add(REPRI);

            HTMLReportTemplatesTreeItem HTMLREPRI = new HTMLReportTemplatesTreeItem(null);            
            Childrens.Add(HTMLREPRI);           

            return Childrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        public Page EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            return mReportTemplatesPage;
        }

        public ContextMenu Menu()
        {
            ContextMenu CM = new ContextMenu();
            return CM;
        }

        public void SetTools(ITreeView TV)
        {
        }
    }
}
