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
using Ginger.Run;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class RunSetTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        readonly RunSetConfig mRunSetConfig;

        public RunSetTreeItem(RunSetConfig runSetConfig)
        {
            mRunSetConfig = runSetConfig;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mRunSetConfig;
        }
        override public string NodePath()
        {
            return mRunSetConfig.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(RunSetConfig);
        }

        StackPanel ITreeViewItem.Header()
        {           
            return NewTVItemHeaderStyle(mRunSetConfig);
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
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            AddItemNodeBasicManipulationsOptions(mContextMenu);
            AddSourceControlOptions(mContextMenu);            
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            if (base.DeleteTreeItem(mRunSetConfig, deleteWithoutAsking, refreshTreeAfterDelete))
            {
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.Equals(mRunSetConfig))//update Run tab in case the loaded run set was deleted
                {
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig = null;
                }

                return true;
            }
            return false;
        }                      
    }
}