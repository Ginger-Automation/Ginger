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

using Ginger.SolutionWindows.TreeViewItems;
using Ginger.SourceControl;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems
{
    public class ApplicationDBTableTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        
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
            string ImageFile;
            if (IsGingerDefualtFolder)
            {
                ImageFile = "@Documents_16x16.png";
            }
            else
            {
                ImageFile = "@Folder2_16x16.png";
            }

            return TreeViewUtils.CreateItemHeader(Name, ImageFile, SourceControlUI.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
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

            if (IsGingerDefualtFolder)
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document", allowSaveAll: false, allowAddNew: false, allowCopyItems: false, allowCutItems: false, allowPaste: false, allowRenameFolder: false, allowDeleteFolder: false, allowDeleteAllItems: true);
            else
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document", allowSaveAll: false, allowAddNew: false, allowCopyItems: false, allowCutItems: false, allowPaste: false);
            AddSourceControlOptions(mContextMenu, false, false);
        }
    }
}
