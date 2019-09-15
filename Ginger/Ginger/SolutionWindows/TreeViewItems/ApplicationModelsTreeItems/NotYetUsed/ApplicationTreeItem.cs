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

using Ginger.SourceControl;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems
{
    public class ApplicationTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        ApplicationPlatform mApplicationPlatform;
        //ApplicationEditPage mApplicationEditPage = null;

        public ApplicationTreeItem(ApplicationPlatform ApplicationModel)
        {
            this.mApplicationPlatform = ApplicationModel;
        }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }
        override public string NodePath()
        {
            return null;
        }

        override public Type NodeObjectType()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFile = "@Folder2_16x16.png";  // TODO: find icon for Apps
            string Path = "?";
            return TreeViewUtils.CreateItemHeader(mApplicationPlatform.AppName, ImageFile, SourceControlUI.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
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
            //if (mApplicationEditPage == null)
            //{
            //    mApplicationEditPage =  new ApplicationEditPage(mApplicationPlatform);
            //}
            //return mApplicationEditPage;
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
            AddSourceControlOptions(mContextMenu, false, false);
            AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Application", allowSaveAll: false, allowAddNew: false, allowCopyItems: false, allowCutItems: false, allowPaste: false, allowAddSubFolder: false);
        }
    }
}