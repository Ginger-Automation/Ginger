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
        public string Folder { get; set; }
        public string Path { get; set; }

        POMEditPage mPOMEditPage;

        public ApplicationPOMModel ApplicationPOM;

        public ApplicationPOMTreeItem(ApplicationPOMModel pom)
        {
            ApplicationPOM = pom;
        }

        Object ITreeViewItem.NodeObject()
        {       
            return null;
        }
        override public string NodePath()
        {
            return Path + @"\";
        }
        override public Type NodeObjectType()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(ApplicationPOM, nameof(ApplicationPOM.Name), eImageType.Application , eImageType.Null, true, nameof(ApplicationPOM.IsDirty));            
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            AddsubFolders(Path, Childrens);

            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            return Childrens;
        }

        private void AddsubFolders(string sDir, List<ITreeViewItem> Childrens)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(Path))
                {
                    DocumentsFolderTreeItem FolderItem = new DocumentsFolderTreeItem();
                    string FolderName = System.IO.Path.GetFileName(d);

                    FolderItem.Folder = FolderName;
                    FolderItem.Path = d;

                    Childrens.Add(FolderItem);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        Page ITreeViewItem.EditPage()
        {            
            if (mPOMEditPage == null)
            {                
                mPOMEditPage = new POMEditPage(ApplicationPOM);
            }
            return mPOMEditPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            
            TreeViewUtils.AddMenuItem(mContextMenu, "Save", SavePOM, null, "@Save_16x16.png");
            mTreeView.AddToolbarTool("@Save_16x16.png", "Save", SavePOM);

            if (IsGingerDefualtFolder)
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document", allowSaveAll: false, allowAddNew: false, allowCopyItems: false, allowCutItems: false, allowPaste: false, allowRenameFolder: false, allowDeleteFolder: false);
            else
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document", allowSaveAll: false, allowAddNew: false, allowCopyItems: false, allowCutItems: false, allowPaste: false);
            AddSourceControlOptions(mContextMenu);
            AddSourceControlOptions(mContextMenu, false, false);
        }

        private void SavePOM(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(ApplicationPOM);            
        }
    }
}
