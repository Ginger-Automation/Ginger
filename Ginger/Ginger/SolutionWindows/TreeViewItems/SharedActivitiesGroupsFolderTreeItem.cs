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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.ALM;
using Ginger.Repository;
using GingerCore;
using GingerCore.Activities;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedActivitiesGroupsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public enum eActivitiesGroupsItemsShowMode { ReadWrite, ReadOnly }

        readonly RepositoryFolder<ActivitiesGroup> mActivitiesGroupFolder;
        private ActivitiesGroupsRepositoryPage mActivitiesGroupsRepositoryPage;
        private eActivitiesGroupsItemsShowMode mShowMode;

        public SharedActivitiesGroupsFolderTreeItem(RepositoryFolder<ActivitiesGroup> activitiesGroupFolder, eActivitiesGroupsItemsShowMode showMode = eActivitiesGroupsItemsShowMode.ReadWrite)
        {
            mActivitiesGroupFolder = activitiesGroupFolder;
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mActivitiesGroupFolder;
        }
        override public string NodePath()
        {
            return mActivitiesGroupFolder.FolderFullPath;
        }
        override public Type NodeObjectType()
        {
            return typeof(ActivitiesGroup);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemFolderHeaderStyle(mActivitiesGroupFolder);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<ActivitiesGroup>(mActivitiesGroupFolder);
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is ActivitiesGroup)
            {
                return new SharedActivitiesGroupTreeItem((ActivitiesGroup)item);
            }
            else if (item is RepositoryFolderBase)
            {
                return new SharedActivitiesGroupsFolderTreeItem((RepositoryFolder<ActivitiesGroup>)item);
            }
            else
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error unknown item added to Activities Group folder");
                throw new NotImplementedException();
            }
        }

        internal void AddItemHandler(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mActivitiesGroupsRepositoryPage == null)
            {
                mActivitiesGroupsRepositoryPage = new ActivitiesGroupsRepositoryPage(mActivitiesGroupFolder);
            }
            return mActivitiesGroupsRepositoryPage;
        }


        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }


        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            if (mShowMode == eActivitiesGroupsItemsShowMode.ReadWrite)
            {
                if (mActivitiesGroupFolder.IsRootFolder)
                { 
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), allowAddNew: false, allowRenameFolder: false, allowDeleteFolder: false, allowRefresh: false);
                }
                else
                { 
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), allowAddNew: false, allowRefresh: false);
                }

                MenuItem exportMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Export");
                TreeViewUtils.AddSubMenuItem(exportMenu, "Export All to ALM", ExportAllToALM, null, "@ALM_16x16.png");
                
                AddSourceControlOptions(mContextMenu, false, false);
            }
            else
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), false, false, false, false, false, false, false, false, false,false);
            }
        }

        private void ExportAllToALM(object sender, System.Windows.RoutedEventArgs e)
        {
            ObservableList<ActivitiesGroup> agToExport = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            if (agToExport.Count > 0)
            {
                if (agToExport.Count == 1)
                    ALMIntegration.Instance.ExportActivitiesGroupToALM(agToExport[0], true);
                else
                {

                    foreach (ActivitiesGroup ag in agToExport)
                        ALMIntegration.Instance.ExportActivitiesGroupToALM(ag, true);
                }
            }
        }
    }
}
