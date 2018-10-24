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
using Ginger.Repository;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Amdocs.Ginger.Repository;
using GingerWPF.TreeViewItemsLib;
using System.Windows;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedVariablesFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public enum eVariablesItemsShowMode { ReadWrite, ReadOnly }

        readonly RepositoryFolder<VariableBase> mVariablesFolder;
        private VariablesRepositoryPage mVariablesRepositoryPage;        
        private eVariablesItemsShowMode mShowMode;

        public SharedVariablesFolderTreeItem(RepositoryFolder<VariableBase> variablesFolder,  eVariablesItemsShowMode showMode = eVariablesItemsShowMode.ReadWrite)
        {
            mVariablesFolder = variablesFolder;
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mVariablesFolder;
        }
        override public string NodePath()
        {
            return mVariablesFolder.FolderFullPath;
        }
        override public Type NodeObjectType()
        {
            return typeof(VariableBase);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemFolderHeaderStyle(mVariablesFolder);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<VariableBase>(mVariablesFolder);
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is VariableBase)
            {
                return new SharedVariableTreeItem((VariableBase)item);
            }
            else if (item is RepositoryFolderBase)
            {
                return new SharedVariablesFolderTreeItem((RepositoryFolder<VariableBase>)item);
            }
            else
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error unknown item added to Variables folder");
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
            if (mVariablesRepositoryPage == null)
            {
                mVariablesRepositoryPage = new VariablesRepositoryPage(mVariablesFolder);
            }
            return mVariablesRepositoryPage;
        }


        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }


        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            if (mShowMode == eVariablesItemsShowMode.ReadWrite)
            {
                if (mVariablesFolder.IsRootFolder)
                { 
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.Variable), allowAddNew: false, allowRenameFolder: false, allowDeleteFolder: false, allowRefresh:false);
                }
                else
                { 
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.Variable), allowAddNew: false, allowRefresh: false);
                }
                AddSourceControlOptions(mContextMenu, false, false);
            }
            else
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, GingerDicser.GetTermResValue(eTermResKey.Variable), false, false, false, false, false, false, false, false, false, false);
            }
        }       
    }
}