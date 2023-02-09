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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.ApplicationModelsLib.APIModels;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;

namespace GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems
{
    public class AppApiModelTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public ApplicationAPIModel mApiModel;
        private APIModelPage mAPIModelPage;
        bool mShowEditInMenu = false;

        public AppApiModelTreeItem(ApplicationAPIModel apiModel, bool ShowEditInMenu = false)
        {
            mApiModel = apiModel;
            mShowEditInMenu = ShowEditInMenu;
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

        public Page EditPage(Context mContext)
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

            AddItemNodeBasicManipulationsOptions(mContextMenu, allowEdit: mShowEditInMenu);

            AddSourceControlOptions(mContextMenu);
        }

        public override void EditTreeItem(object item)
        {
            (EditPage(null) as APIModelPage).ShowAsWindow(Ginger.eWindowShowStyle.Dialog, e: APIModelPage.eEditMode.Edit);
        }

        public override void DuplicateTreeItem(object item)
        {
            RepositoryItemBase copiedItem = CopyTreeItemWithNewName((RepositoryItemBase)item);
            if (copiedItem != null)
            {
                copiedItem.DirtyStatus = eDirtyStatus.NoTracked;
                HandleGlobalModelParameters(item, copiedItem);          // avoid generating new GUIDs for Global Model Parameters associated to API Model being copied
                (WorkSpace.Instance.SolutionRepository.GetItemRepositoryFolder(((RepositoryItemBase)item))).AddRepositoryItem(copiedItem);
            }
        }

        // avoid generating new GUIDs for Global Model Parameters associated to API Model being copied
        public static void HandleGlobalModelParameters(object item, RepositoryItemBase copiedItem)
        {
            foreach (GlobalAppModelParameter gAMPara in (copiedItem as ApplicationAPIModel).GlobalAppModelParameters)
            {
                gAMPara.Guid = (item as ApplicationAPIModel).GlobalAppModelParameters.Where(m => m.ElementName == gAMPara.ElementName).FirstOrDefault().Guid;
            }
        }
    }
}
