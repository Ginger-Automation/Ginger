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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.Repository;
using GingerCore;
using GingerCore.GeneralLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.TreeViewItemsLib
{
    public abstract class TreeViewItemGenericBase
    {
        public virtual string NodePath()
        {
            return string.Empty;
        }
        public virtual Type NodeObjectType()
        {
            return null;
        }

        public ITreeView mTreeView;
        public ContextMenu mContextMenu;
        public static ITreeViewItem mNodeManipulationsSource = null;
        public bool IsGingerDefualtFolder = false;
        public enum eFolderNodePastOperations { None, Copy, Cut, CopyItems, CutItems }
        public static eFolderNodePastOperations mCurrentFolderNodePastOperations = eFolderNodePastOperations.None;

        public void AddItemNodeBasicManipulationsOptions(ContextMenu CM, bool allowSave = true, bool allowCopy = true, bool allowCut = true, bool allowDuplicate = true, bool allowDelete = true, bool allowViewXML = true, bool allowOpenContainingFolder = true)
        {
            if (allowSave)
            {
                TreeViewUtils.AddMenuItem(CM, "Save", SaveTreeItemHandler, null, "@Save_16x16.png");
                mTreeView.AddToolbarTool("@Save_16x16.png", "Save", SaveTreeItemHandler);
            }
            if (allowCopy)
            {
                TreeViewUtils.AddMenuItem(CM, "Copy", CopyTreeItemHandler, null, "@Copy_16x16.png");
                mTreeView.AddToolbarTool("@Copy_16x16.png", "Copy", CopyTreeItemHandler);
            }
            if (allowCut)
            {
                TreeViewUtils.AddMenuItem(CM, "Cut", CutTreeItemHandler, null, "@Cut_16x16.png");
                mTreeView.AddToolbarTool("@Cut_16x16.png", "Cut", CutTreeItemHandler);
            }
            if (allowDuplicate)
            {
                TreeViewUtils.AddMenuItem(CM, "Duplicate", DuplicateTreeItemHandler, null, "@Duplicate_16x16.png");
                mTreeView.AddToolbarTool("@Duplicate_16x16.png", "Duplicate", DuplicateTreeItemHandler);
            }
            if (allowDelete)
            {
                TreeViewUtils.AddMenuItem(CM, "Delete", DeleteTreeItemHandler, null, "@Trash_16x16.png");
                mTreeView.AddToolbarTool("@Trash_16x16.png", "Delete", DeleteTreeItemHandler);
            }
            if (allowViewXML)
            {
                TreeViewUtils.AddMenuItem(CM, "View XML", ViewTreeItemXMLHandler, null, "@XML_16x16.png");
                mTreeView.AddToolbarTool("@XML_16x16.png", "View XML", ViewTreeItemXMLHandler);
            }
            if (allowOpenContainingFolder)
            {
                TreeViewUtils.AddMenuItem(CM, "Open Containing Folder", OpenTreeItemFolderHandler, null, "@Folder_16x16.png");
                mTreeView.AddToolbarTool("@Folder_16x16.png", "Open Containing Folder", OpenTreeItemFolderHandler);
            }
        }

        public abstract bool SaveTreeItem(object item, bool saveOnlyIfDirty = false);
        public virtual void PostSaveTreeItemHandler()
        {
            //do in Folder node if needed
        }
        private void SaveTreeItemHandler(object sender, RoutedEventArgs e)
        {
            if (SaveTreeItem(((ITreeViewItem)this).NodeObject()))
                PostSaveTreeItemHandler();
        }
        
        private void CopyTreeItemHandler(object sender, RoutedEventArgs e)
        {
            mCurrentFolderNodePastOperations = eFolderNodePastOperations.Copy;
            mNodeManipulationsSource = (ITreeViewItem)this;
        }

        private void CutTreeItemHandler(object sender, RoutedEventArgs e)
        {
            mCurrentFolderNodePastOperations = eFolderNodePastOperations.Cut;
            mNodeManipulationsSource = (ITreeViewItem)this;
        }

        public abstract void DuplicateTreeItem(object item);
        private void DuplicateTreeItemHandler(object sender, RoutedEventArgs e)
        {
            DuplicateTreeItem(((ITreeViewItem)this).NodeObject());
        }

        public abstract bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true);
        public virtual void PostDeleteTreeItemHandler()
        {
            //do in Folder node if needed
        }
        public virtual void PreDeleteTreeItemHandler()
        {
            //do in Folder node if needed
        }        

        private void DeleteTreeItemHandler(object sender, RoutedEventArgs e)
        {
            if (DeleteTreeItem(((ITreeViewItem)this).NodeObject(), false, true))
                PostDeleteTreeItemHandler();
        }        
        
        private void ViewTreeItemXMLHandler(object sender, RoutedEventArgs e)
        {
            object item = ((ITreeViewItem)this).NodeObject();
            if (item is RepositoryItemBase)
            {
                XMLViewer(((RepositoryItemBase)item).FileName);
            }
            else
            {
                //implement for other item types
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "View item XML operation for this item type was not implemented yet.");
            }
        }

        public void XMLViewer(string Path)
        {
            RepositoryItemPage RIP = new RepositoryItemPage(Path);
            RIP.ShowAsWindow();
        }

        private void OpenTreeItemFolderHandler(object sender, RoutedEventArgs e)
        {
            string filePath = this.NodePath();
            string ContainingFolderFullPath = filePath.Substring(0, filePath.LastIndexOf('\\') + 1);
            ViewFolderFiles(ContainingFolderFullPath);
        }

        
        public void AddFolderNodeBasicManipulationsOptions(ContextMenu CM, string nodeItemTypeName, bool allowRefresh = true, bool allowAddNew = true, bool allowPaste = true, bool allowSaveAll = true, bool allowCutItems = true, bool allowCopyItems = true, bool allowRenameFolder = true, bool allowAddSubFolder = true, bool allowDeleteFolder = true, bool allowOpenFolder = true)
        {
            if (allowRefresh)
            {
                TreeViewUtils.AddMenuItem(CM, "Refresh", RefreshTreeFolderHandler, null, eImageType.Refresh);
                mTreeView.AddToolbarTool(eImageType.Refresh, "Refresh", RefreshTreeFolderHandler);
            }
            if (allowAddNew)
            {
                TreeViewUtils.AddMenuItem(CM, "Add New " + nodeItemTypeName, AddTreeItemHandler, null, "@Add_16x16.png");
                mTreeView.AddToolbarTool("@Add_16x16.png", "Add New " + nodeItemTypeName, AddTreeItemHandler);
            }
            if (allowPaste)
            {
                TreeViewUtils.AddMenuItem(CM, "Paste", PastOnTreeFolderHandler, null, "@Paste_16x16.png");
                mTreeView.AddToolbarTool("@Paste_16x16.png", "Paste", PastOnTreeFolderHandler);
            }
            if (allowCopyItems)
            {
                TreeViewUtils.AddMenuItem(CM, "Copy Items", CopyTreeFolderItemsHandler, null, "@Copy_16x16.png");
                mTreeView.AddToolbarTool("@Copy_16x16.png", "Copy Items", CopyTreeFolderItemsHandler);
            }
            if (allowCutItems)
            {
                TreeViewUtils.AddMenuItem(CM, "Cut Items", CutTreeFolderItemsHandler, null, "@Cut_16x16.png");
                mTreeView.AddToolbarTool("@Cut_16x16.png", "Cut Items", CutTreeFolderItemsHandler);
            }
            if (allowSaveAll)
            {
                TreeViewUtils.AddMenuItem(CM, "Save All", SaveAllTreeFolderItemsHandler, null, "@SaveAll_16x16.png");
                mTreeView.AddToolbarTool("@SaveAll_16x16.png", "Save All", SaveAllTreeFolderItemsHandler);
            }
            if (allowAddSubFolder)
            {
                TreeViewUtils.AddMenuItem(CM, "Add Sub Folder", AddSubTreeFolderHandler, null, "@AddFolder_16x16.png");
                mTreeView.AddToolbarTool("@AddFolder_16x16.png", "Add Sub Folder", AddSubTreeFolderHandler);
            }
            if (allowRenameFolder)
            {
                TreeViewUtils.AddMenuItem(CM, "Rename Folder", RenameTreeFolderHandler, null, "@Edit_16x16.png");
                mTreeView.AddToolbarTool("@Edit_16x16.png", "Rename Folder", RenameTreeFolderHandler);
            }
            if (allowDeleteFolder)
            {
                TreeViewUtils.AddMenuItem(CM, "Delete Folder", DeleteTreeFolderHandler, null, "@Trash_16x16.png");
                mTreeView.AddToolbarTool("@Trash_16x16.png", "Delete Folder", DeleteTreeFolderHandler);
            }
            if (allowOpenFolder)
            {
                TreeViewUtils.AddMenuItem(CM, "Open Folder in File Explorer", OpenTreeFolderHandler, null, "@Folder_16x16.png");
                mTreeView.AddToolbarTool("@Folder_16x16.png", "Open Folder in File Explorer", OpenTreeFolderHandler);
            }
        }

        public abstract bool PasteCopiedTreeItem(object nodeItemToCopy, TreeViewItemGenericBase targetFolderNode, bool toRefreshFolder = true);
        public abstract void PasteCopiedTreeItems();
        public abstract bool PasteCutTreeItem(object nodeItemToCut, TreeViewItemGenericBase targetFolderNode, bool toRefreshFolder = true);
        public abstract void PasteCutTreeItems();
        private void PastOnTreeFolderHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            if (mNodeManipulationsSource == null || mCurrentFolderNodePastOperations == eFolderNodePastOperations.None)
            {
                
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Please select Copy/Cut operation first.");
                return;
            }

            //make sure the source item and dest folder are from same item type
            if (this.NodeObjectType() != ((TreeViewItemGenericBase)mNodeManipulationsSource).NodeObjectType())
            {                
                Reporter.ToUser(eUserMsgKeys.DifferentItemType);
                return;
            }

            switch (mCurrentFolderNodePastOperations)
            {
                case eFolderNodePastOperations.Copy:
                    PasteCopiedTreeItem(mNodeManipulationsSource.NodeObject(), this);
                    break;
                case eFolderNodePastOperations.CopyItems:
                    PasteCopiedTreeItems();
                    break;
                case eFolderNodePastOperations.Cut:
                    PasteCutTreeItem(mNodeManipulationsSource.NodeObject(), this);
                    break;
                case eFolderNodePastOperations.CutItems:
                    PasteCutTreeItems();
                    break;
                default:                    
                    Reporter.ToUser(eUserMsgKeys.CopyCutOperation);
                    break;
            }

            mCurrentFolderNodePastOperations = eFolderNodePastOperations.None;
        }

        private void CopyTreeFolderItemsHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            mCurrentFolderNodePastOperations = eFolderNodePastOperations.CopyItems;
            mNodeManipulationsSource = (ITreeViewItem)this;
        }

        private void CutTreeFolderItemsHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            mCurrentFolderNodePastOperations = eFolderNodePastOperations.CutItems;
            mNodeManipulationsSource = (ITreeViewItem)this;
        }

        public abstract void SaveAllTreeFolderItems();
        public void SaveAllTreeFolderItemsHandler(object sender, System.Windows.RoutedEventArgs e)
        {            
            SaveAllTreeFolderItems();            
        }

        public abstract void DeleteTreeFolder();
        private void DeleteTreeFolderHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            PreDeleteTreeItemHandler();
            DeleteTreeFolder();
            PostDeleteTreeItemHandler();
        }

        public abstract void RefreshTreeFolder(Type itemType, string path);
        private void RefreshTreeFolderHandler(object sender, System.Windows.RoutedEventArgs e)
        {
                RefreshTreeFolder(this.NodeObjectType(), Path.GetDirectoryName(this.NodePath()));    
        }

        public abstract void AddTreeItem();
        private void AddTreeItemHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            AddTreeItem();
        }

        public abstract ITreeViewItem AddSubFolder(Type typeOfFolder, string newFolderName, string newFolderPath);
        private void AddSubTreeFolderHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            string folderName = string.Empty;
            if (InputBoxWindow.GetInputWithValidation("Add Sub Folder", "Folder Name:", ref folderName, System.IO.Path.GetInvalidPathChars()))
            {
                if (!String.IsNullOrEmpty(folderName))
                {
                    string path = Path.Combine(this.NodePath(), folderName);
                    if (System.IO.Directory.Exists(path) == true)
                    {                        
                        Reporter.ToUser(eUserMsgKeys.FolderExistsWithName);
                        mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                    }
                    else
                    {
                        object folderItem = AddSubFolder(this.GetType(), folderName, path);                       
                    }
                }
            }
        }

        public abstract bool RenameTreeFolder(string originalName, string newFolderName, string newPath);
        private void RenameTreeFolderHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            string originalName = mTreeView.Tree.GetSelectedTreeNodeName();
            string newFolderName = originalName;
            if (InputBoxWindow.GetInputWithValidation("Rename Folder", "New Folder Name:", ref newFolderName, System.IO.Path.GetInvalidPathChars()))
            {
                if (!String.IsNullOrEmpty(newFolderName))
                {
                    string path = Path.Combine(Path.GetDirectoryName(this.NodePath().TrimEnd('\\', '/')), newFolderName);
                    if (System.IO.Directory.Exists(path) == true && originalName.ToUpper() != newFolderName.ToUpper())
                    {                        
                        Reporter.ToUser(eUserMsgKeys.FolderExistsWithName);
                        mTreeView.Tree.RefreshSelectedTreeNodeParent();
                        return;
                    }
                    else
                    {
                        try
                        {
                            if (RenameTreeFolder(originalName, newFolderName, path) == false)
                            {                                
                                Reporter.ToUser(eUserMsgKeys.RenameItemError, path);
                                return;
                            }
                        }
                        catch (Exception ex)
                        {                            
                            Reporter.ToUser(eUserMsgKeys.RenameItemError, ex.Message);
                            Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                            return;
                        }
                    }
                }
            }
        }

        public void OpenTreeFolderHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.NodePath()))
                return;
            if (!Directory.Exists(this.NodePath()))
                Directory.CreateDirectory(this.NodePath());
            Process.Start(this.NodePath());
        }

        public virtual void AddSourceControlOptions(ContextMenu CM, bool addDetailsOption = true, bool addLocksOption = true)
        {
        }

        public bool RenameItem(string Title, Object obj, string Property)
        {
            // hard coded biz flow name... Check me
            if (InputBoxWindow.OpenDialog("Rename", Title, obj, Property))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public abstract bool SaveBackup(object item);

        public abstract bool ItemIsDirty(object item);

        private void SetNodeItemManipulationsSourceNodeFromMenu(object sender)
        {
            if (sender == null) return;
            MenuItem MI = (MenuItem)sender;
            ITreeViewItem treeItem = (ITreeViewItem)MI.CommandParameter;
            mNodeManipulationsSource = treeItem;
        }

        public void AddViewFolderFilesMenuItem(ContextMenu CM, string Path)
        {
            TreeViewUtils.AddMenuItem(CM, "Open Folder in File Explorer", ViewFolderFilesFromContextMenu, Path, eImageType.OpenFolder);
        }

        private void ViewFolderFilesFromContextMenu(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem MI = (MenuItem)sender;
            string path = (string)MI.CommandParameter;

            ViewFolderFiles(path);
        }

        public void ViewFolderFilesFromTool(object sender, System.Windows.RoutedEventArgs e)
        {
            Button b = (Button)sender;
            string path = (string)b.CommandParameter;

            ViewFolderFiles(path);
        }

        public void ViewFolderFiles(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            Process.Start(path);
        }

        public void ExploreFile(string Path)
        {
            Process.Start(Path);
        }

        public RepositoryItemBase CopyTreeItemWithNewName(RepositoryItemBase itemToCopy)
        {
            if (itemToCopy is RepositoryItemBase)
            {
                string newName = ((RepositoryItemBase)itemToCopy).ItemName + "_Copy";
                if (GingerCore.GeneralLib.InputBoxWindow.GetInputWithValidation("Copied/Duplicated Item Name", "New Name:", ref newName, System.IO.Path.GetInvalidPathChars()))
                {
                    RepositoryItemBase itemCopy = ((RepositoryItemBase)itemToCopy).CreateCopy();
                    itemCopy.ItemName = newName;
                    return itemCopy;
                }
            }

            return null;
        }
    }
}
