#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.ApplicationModelsLib.APIModels;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems
{
    public class AppApiModelTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private ApplicationAPIModel mApiModel;
        private APIModelPage mAPIModelPage;

        public AppApiModelTreeItem(ApplicationAPIModel apiModel)
        {
            mApiModel = apiModel;
        }

        public object NodeObject()
        {
            return mApiModel;
        }

        override public string NodePath()
        {
            return mApiModel.FilePath;
        }

        override public Type NodeObjectType()
        {
            return typeof(ApplicationAPIModel);
        }

        public StackPanel Header()
        {
            return NewTVItemHeaderStyle(mApiModel);
        }

        public Page EditPage()
        {
            if(mAPIModelPage == null)
                mAPIModelPage = new APIModelPage(mApiModel);            
            else
                mAPIModelPage.BindUiControls();

            return mAPIModelPage;
        }

        public bool IsExpandable()
        {
            return false;
        }

        public List<ITreeViewItem> Childrens()
        {
            return null;
        }

        public ContextMenu Menu()
        {
            return mContextMenu;
        }

        public void SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            AddItemNodeBasicManipulationsOptions(mContextMenu);
            
            AddSourceControlOptions(mContextMenu);
        }
    }
}
