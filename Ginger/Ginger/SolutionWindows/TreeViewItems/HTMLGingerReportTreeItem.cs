#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Ginger.Environments;
using Ginger.Reports;
using Ginger.Repository;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using GingerCore.SourceControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class HTMLGingerReportTreeItem : NewTreeViewItemBase, ITreeViewItem
    {       
        public HTMLReportConfiguration HTMLReportConfiguration { get; set; }
        HTMLReportTemplatePage mHTMLReportTemplatePage;

        public HTMLGingerReportTreeItem(HTMLReportConfiguration htmlConfig)
        {
            HTMLReportConfiguration = htmlConfig;
        }

        Object ITreeViewItem.NodeObject()
        {
            return HTMLReportConfiguration;
        }
        override public string NodePath()
        {
            return HTMLReportConfiguration.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(HTMLReportConfiguration);
        }

        StackPanel ITreeViewItem.Header()
        {            
            return NewTVItemHeaderStyle(HTMLReportConfiguration);
        }
        
        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        Page ITreeViewItem.EditPage()
        {   
            //TODO: to load page only once            
            mHTMLReportTemplatePage = new HTMLReportTemplatePage(HTMLReportConfiguration);
            return mHTMLReportTemplatePage;
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return null;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
                    
            TreeViewUtils.AddMenuItem(mContextMenu, "Set as Default Template", SetTemplateAsDefault, null, eImageType.Check);            
            AddItemNodeBasicManipulationsOptions(mContextMenu, allowCopy:false,allowCut:false);
            AddSourceControlOptions(mContextMenu);           
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            if(HTMLReportConfiguration.IsDefault==true)
            {
                Reporter.ToUser(eUserMsgKeys.DefaultTemplateCantBeDeleted);
                return false;
            }
            else
            {
                base.DeleteTreeItem(HTMLReportConfiguration, deleteWithoutAsking, refreshTreeAfterDelete);
                return true;
            }
        }

        private void SetTemplateAsDefault(object sender, RoutedEventArgs e)
        {
            if (HTMLReportConfiguration.IsDefault == true)
            {
                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Template is already defualt");
                return;
            }

            Ginger.Reports.GingerExecutionReport.ExtensionMethods.SetTemplateAsDefault(HTMLReportConfiguration);
        }

        public override void DuplicateTreeItem(object item)
        {
            HTMLReportConfiguration copiedItem = (HTMLReportConfiguration)CopyTreeItemWithNewName((RepositoryItemBase)item);
            
            //TODO: why below is needed??
            copiedItem.ID = App.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq + 1;
            App.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq = App.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq + 1;
            copiedItem.IsDefault = false;
            if (copiedItem != null)
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(copiedItem);
        }
    }
}
