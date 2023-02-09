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
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.POMsLib;
using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems
{
    public class ApplicationPOMTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private ApplicationPOMModel mApplicationPOM;
        private POMEditPage mPOMEditPage;

        public ApplicationPOMTreeItem(ApplicationPOMModel pom)
        {
            mApplicationPOM = pom;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mApplicationPOM;
        }
        override public string NodePath()
        {
            return mApplicationPOM.FilePath;
        }
        override public Type NodeObjectType()
        {
            return typeof(ApplicationPOMModel);
        }

        StackPanel ITreeViewItem.Header()
        {
            var appPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mApplicationPOM.TargetApplicationKey);
            mApplicationPOM.SetItemImageType(appPlatform);
            return NewTVItemHeaderStyle(mApplicationPOM);
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mPOMEditPage == null)
            {
                mPOMEditPage = new POMEditPage(mApplicationPOM);
            }
            return mPOMEditPage;
        }

        public List<ITreeViewItem> Childrens()
        {
            return null;
        }        

        bool ITreeViewItem.IsExpandable()
        {
            return false;
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
    }
}
