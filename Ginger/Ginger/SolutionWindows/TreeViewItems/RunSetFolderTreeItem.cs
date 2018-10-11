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

using Amdocs.Ginger.Common;
using Ginger.Run;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Ginger.Repository;
using GingerCore.GeneralLib;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger;
using System.Windows;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class RunSetFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private readonly RepositoryFolder<RunSetConfig> mRunSetConfigFolder;

        public RunSetFolderTreeItem(RepositoryFolder<RunSetConfig> runSetConfigFolder)
        {
            mRunSetConfigFolder = runSetConfigFolder;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mRunSetConfigFolder;
        }
        override public string NodePath()
        {
            return mRunSetConfigFolder.FolderFullPath;
        }
        override public Type NodeObjectType()
        {
            return typeof(RunSetConfig);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemFolderHeaderStyle(mRunSetConfigFolder);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<RunSetConfig>(mRunSetConfigFolder);
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is RunSetConfig)
            {
                return new RunSetTreeItem((RunSetConfig)item);
            }
            else if (item is RepositoryFolderBase)
            {
                return new RunSetFolderTreeItem((RepositoryFolder<RunSetConfig>)item);
            }
            else
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error unknown item added to Run Sets folder");
                throw new NotImplementedException();
            }
        }

        internal void AddItemHandler(object sender, RoutedEventArgs e)
        {
            AddTreeItem();
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
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

            if (mRunSetConfigFolder.IsRootFolder)
            { 
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.RunSet), allowRenameFolder: false, allowDeleteFolder: false, allowRefresh:false);
            }
            else
            { 
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.RunSet), allowRefresh: false);
            }

            AddSourceControlOptions(mContextMenu, false, false);
        }

        public override void AddTreeItem()
        {
            RunSetOperations.CreateNewRunset(runSetsFolder: mRunSetConfigFolder);
        }

    }
}