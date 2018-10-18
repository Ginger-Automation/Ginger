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

using Ginger.Repository;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using Ginger.SourceControl;
using GingerCore.SourceControl;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SourceControl;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;

namespace Ginger.SolutionWindows.TreeViewItems
{

    // class is obsolete !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public class TreeViewItemBase : TreeViewItemGenericBase
    {
        public SourceControlFileInfo.eRepositoryItemStatus ItemSourceControlStatus;

        public override void AddSourceControlOptions(ContextMenu CM, bool addDetailsOption = true, bool addLocksOption = true)
        {
            if (App.UserProfile.Solution != null && App.UserProfile.Solution.SourceControl != null)
            {
                MenuItem sourceControlMenu = TreeViewUtils.CreateSubMenu(CM, "Source Control");
                if (addDetailsOption)
                    TreeViewUtils.AddSubMenuItem(sourceControlMenu, "Get Info", SourceControlGetInfo, null, "@Info_16x16.png");
                TreeViewUtils.AddSubMenuItem(sourceControlMenu, "Check-In Changes", SourceControlCheckIn, null, "@CheckIn2_16x16.png");
                if (App.UserProfile.Solution.SourceControl.IsSupportingGetLatestForIndividualFiles)
                    TreeViewUtils.AddSubMenuItem(sourceControlMenu, "Get Latest Version", SourceControlGetLatestVersion, null, "@GetLatest2_16x16.png");
                if (App.UserProfile.Solution.ShowIndicationkForLockedItems && App.UserProfile.Solution.SourceControl.IsSupportingLocks && addLocksOption)
                    if (ItemSourceControlStatus == SourceControlFileInfo.eRepositoryItemStatus.LockedByAnotherUser || ItemSourceControlStatus == SourceControlFileInfo.eRepositoryItemStatus.LockedByMe)
                    {
                        TreeViewUtils.AddSubMenuItem(sourceControlMenu, "UnLock Item", SourceControlUnlock, null, "@Unlock_16x16.png");
                    }
                    else
                    {
                        TreeViewUtils.AddSubMenuItem(sourceControlMenu, "Lock Item", SourceControlLock, null, "@Lock_16x16.png");
                    }
                TreeViewUtils.AddSubMenuItem(sourceControlMenu, "Undo Changes", SourceControlUndoChanges, null, "@Undo_16x16.png");
            }
        }

        private void SourceControlGetInfo(object sender, RoutedEventArgs e)
        {
            SourceControlItemInfoDetails SCIID = SourceControlIntegration.GetInfo(App.UserProfile.Solution.SourceControl, this.NodePath());
            SourceControlItemInfoPage SCIIP = new SourceControlItemInfoPage(SCIID);
            SCIIP.ShowAsWindow();
        }

        private void SourceControlUnlock(object sender, RoutedEventArgs e)
        {
            SourceControlIntegration.UnLock(App.UserProfile.Solution.SourceControl, this.NodePath());
            mTreeView.Tree.RefreshHeader((ITreeViewItem)this);
        }

        private void SourceControlLock(object sender, RoutedEventArgs e)
        {
            string lockComment = string.Empty;
            if (GingerCore.General.GetInputWithValidation("Lock", "Lock Comment:", ref lockComment, System.IO.Path.GetInvalidFileNameChars()))
            {
                SourceControlIntegration.Lock(App.UserProfile.Solution.SourceControl, this.NodePath(), lockComment);
                mTreeView.Tree.RefreshHeader((ITreeViewItem)this);
            }
        }

        public void SourceControlCheckIn(object sender, System.Windows.RoutedEventArgs e)
        { 
            App.CheckIn(this.NodePath());
            mTreeView.Tree.RefreshHeader(((ITreeViewItem)this));
        }

        public void SourceControlUndoChanges(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.SureWantToDoRevert) == MessageBoxResult.Yes)
            {
                Reporter.ToGingerHelper(eGingerHelperMsgKey.RevertChangesFromSourceControl);
                SourceControlIntegration.Revert(App.UserProfile.Solution.SourceControl, this.NodePath());                
                mTreeView.Tree.RefreshSelectedTreeNodeParent();
                Reporter.CloseGingerHelper();
            }
        }

        public void SourceControlGetLatestVersion(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.LoseChangesWarn) == MessageBoxResult.No) return;
            
            Reporter.ToGingerHelper(eGingerHelperMsgKey.GetLatestFromSourceControl);
            if (string.IsNullOrEmpty(this.NodePath()))
                Reporter.ToUser(eUserMsgKeys.SourceControlUpdateFailed, "Invalid Path provided");
            else
                SourceControlIntegration.GetLatest(this.NodePath(), App.UserProfile.Solution.SourceControl);
            
            mTreeView.Tree.RefreshSelectedTreeNodeParent();
            Reporter.CloseGingerHelper();
        }

        

        public override bool SaveTreeItem(object item, bool saveOnlyIfDirty = false)
        {
            if (item is RepositoryItem)
            {
                RepositoryItem RI = (RepositoryItem)item;
                if (saveOnlyIfDirty && RI.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified)
                {
                    return false;//no need to Save because not Dirty
                }

                Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, RI.GetNameForFileName(), "item");
                RI.Save();                
                Reporter.CloseGingerHelper();

                //refresh node header
                mTreeView.Tree.RefreshHeader((ITreeViewItem)this); //needed?
                mTreeView.Tree.SelectParentItem((ITreeViewItem)this);//to allow catch isDirty again when user will select this item again so we move to parent                

                return true;
            }
            else
            {
                //implement for other item types
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Save operation for this item type was not implemented yet.");
                return false;
            }
        }

        public override bool SaveBackup(object item)
        {
            if (item is RepositoryItem)
            {
                RepositoryItem RI = (RepositoryItem)item;
                RI.SaveBackup();
                return true;
            }

            return false;
        }

        public override bool ItemIsDirty(object item)
        {
            if (item is RepositoryItem && ((RepositoryItem)item).DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified)
            {
                return true;
            }

            return false;
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            bool deleteWasDone = false;
            if (item is RepositoryItem)
            {
                if (!deleteWithoutAsking)
                    if (Reporter.ToUser(eUserMsgKeys.DeleteRepositoryItemAreYouSure, ((RepositoryItem)item).GetNameForFileName()) == MessageBoxResult.No)
                        return false;

                WorkSpace.Instance.SolutionRepository.DeleteRepositoryItem((RepositoryItem)item);                
                deleteWasDone = true;
            }
            else
            {
                //implement for other item types              
                string filePath = string.Empty;
                if (item == null)
                    filePath = this.NodePath();
                else
                    filePath = ((TreeViewItemBase)item).NodePath();
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    deleteWasDone = true;
                }
            }

            if (deleteWasDone && refreshTreeAfterDelete)
            {
                //refresh parent
                mTreeView.Tree.DeleteItemAndSelectParent((ITreeViewItem)this);
            }

            return deleteWasDone;
        }

        public override void DuplicateTreeItem(object item)
        {
            if (item is RepositoryItem)
            {
                //create copy
                //RepositoryItemBase copy = ((RepositoryItemBase)item).CreateCopy();
                //copy.ItemName = copy.GetNameForFileName() + "_Copy";
                //if (App.CurrentSelectedTreeItem is Ginger.SolutionWindows.TreeViewItems.BusinessFlowTreeItem)
                //{
                //    var currentFolderPath = ((RepositoryItemBase)item).ContainingFolderFullPath.EndsWith("\\") ? ((RepositoryItemBase)item).ContainingFolderFullPath : ((RepositoryItemBase)item).ContainingFolderFullPath + "\\";
                //    if (!SetNewFileName(currentFolderPath, copy))
                //        return;
                //}
                RepositoryItemBase copiedItem = CopyTreeItemWithNewName((RepositoryItemBase)item);
                if (copiedItem != null)
                {
                    //Save copy to target folder
                    // check me                    
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(copiedItem);

                    //refresh target folder node
                    mTreeView.Tree.RefreshSelectedTreeNodeParent();
                }
            }
            else
            {
                //implement for other item types
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Duplicae operation for this item type was not implemented yet.");
            }
        }

        public override bool PasteCopiedTreeItem(object nodeItemToCopy, TreeViewItemGenericBase targetFolderNode, bool toRefreshFolder = true)
        {
            if (nodeItemToCopy is RepositoryItemBase)
            {
                //create copy
                //RepositoryItemBase copy = ((RepositoryItemBase)nodeItemToCopy).CreateCopy();
                //string targetFolderNodePath = Path.GetFullPath(targetFolderNode.NodePath()).TrimEnd('\\');
                //handle copy-past to same folder
                //if (string.Equals(targetFolderNodePath, Path.GetFullPath(((RepositoryItem)nodeItemToCopy).ContainingFolderFullPath), StringComparison.OrdinalIgnoreCase))
                //    copy.ItemName = copy.GetNameForFileName() + "_Copy";
                //else if(File.Exists(targetFolderNodePath + "\\"+ copy.GetNameForFileName()+ "." + ((RepositoryItemBase)nodeItemToCopy).ObjFileExt + ".xml"))
                //    copy.ItemName = copy.GetNameForFileName() + "_Copy";
                //if (App.CurrentSelectedTreeItem is Ginger.SolutionWindows.TreeViewItems.BusinessFlowsFolderTreeItem)
                //{
                //    if (!SetNewFileName(Path.GetFullPath(targetFolderNode.NodePath()), copy))
                //        return false;
                //}
                RepositoryItemBase copiedItem = CopyTreeItemWithNewName((RepositoryItemBase)nodeItemToCopy);
                if (copiedItem != null)
                {
                    //Save copy to target folder                    
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(copiedItem);

                    //refresh target folder node
                    if (toRefreshFolder)
                        mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                    return true;
                }
                return false;
            }
            else
            {
                //implement for other item types
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "The " + mCurrentFolderNodePastOperations.ToString() + " operation for this item type was not implemented yet.");
                return false;
            }
        }

        ///// <summary>
        ///// Set new file name for copy and duplication.
        ///// </summary>
        ///// <param name="targetFolderPath"></param>
        ///// <param name="copyBFFile"></param>
        ///// <returns></returns>
        //private bool SetNewFileName(string targetFolderPath, RepositoryItemBase copyBFFile)
        //{
        //    string defaultBFFileName = Repository.LocalRepository.GetRepoItemFileName(copyBFFile, targetFolderPath);
        //    string newFileName = Path.GetFileNameWithoutExtension(defaultBFFileName);
        //    string fileExtention = "." + copyBFFile.ObjFileExt;
        //    if (newFileName.EndsWith(fileExtention))
        //        newFileName = newFileName.Substring(0, newFileName.LastIndexOf(fileExtention));
        //    string fileNameBackup = newFileName;
        //    bool tryAgain = true;
        //    while (tryAgain)
        //    {
        //        tryAgain = false;
        //        newFileName = fileNameBackup;
        //        if (GingerCore.GeneralLib.InputBoxWindow.GetInputWithValidation("Enter Business Flow Name", "Name:", ref newFileName, System.IO.Path.GetInvalidPathChars()))
        //        {
        //            tryAgain = false;
        //            // Validation unique file name.
        //            if (File.Exists(targetFolderPath + newFileName + fileExtention + ".xml"))
        //            {
        //                MessageBox.Show("Business Flow file with same name already exists. Please choose a different name.", "File Creation Failed", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
        //                mTreeView.Tree.RefreshSelectedTreeNodeParent();
        //                tryAgain = true;
        //                continue;
        //            }
        //            // Validation unique business name in ginger.
        //            Amdocs.Ginger.Common.ObservableList<BusinessFlow> allBizFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
        //            foreach (BusinessFlow bf in allBizFlows)
        //            {
        //                if (bf.ContainingFolderFullPath.Equals(targetFolderPath.TrimEnd('\\')) && bf.Name.Equals(newFileName))
        //                {
        //                    MessageBox.Show("Ginger business flow with same name already exists. Please choose a different name.", "Business Flow name Creation Failed", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
        //                    mTreeView.Tree.RefreshSelectedTreeNodeParent();
        //                    tryAgain = true;
        //                    break;
        //                }
        //                if (tryAgain) continue;
        //            }
        //            if (!(newFileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0))
        //            {
        //                System.Text.StringBuilder builder = new System.Text.StringBuilder();
        //                MessageBox.Show("File name contain Invalid character, Please choose a different name for the File. ", "Value Issue", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
        //                tryAgain = true;
        //            }
        //            if (!tryAgain) break;
        //        }
        //        else
        //            return false;
        //    }
        //    copyBFFile.ItemName = newFileName;
        //    return true;

        //}

        public override void PasteCopiedTreeItems()
        {
            bool refreshNeeded = false;
            List<ITreeViewItem> childNodes = mTreeView.Tree.GetTreeNodeChilds(mNodeManipulationsSource);
            foreach (ITreeViewItem node in childNodes)
            {
                if (node == null || node.NodeObject() == null) continue;
                if (PasteCopiedTreeItem(node.NodeObject(), this, false))
                    refreshNeeded = true;
            }
            if (refreshNeeded)
            {
                //refresh target folder node
                mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
            }
        }

        public override bool PasteCutTreeItem(object nodeItemToCut, TreeViewItemGenericBase targetFolderNode, bool toRefreshFolder = true)
        {
            if (nodeItemToCut is RepositoryItem)
            {  
                //refresh source and target folder nodes
                if (toRefreshFolder)
                {
                    mTreeView.Tree.RefreshTreeNodeParent(mNodeManipulationsSource);
                    mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                }
                return true;
            }
            else
            {
                //implement for other item types
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "The " + mCurrentFolderNodePastOperations.ToString() + " operation for this item type was not implemented yet.");
                return false;
            }
        }

        public override void PasteCutTreeItems()
        {
            bool refreshNeeded = false;
            List<ITreeViewItem> childNodes = mTreeView.Tree.GetTreeNodeChilds(mNodeManipulationsSource);
            foreach (ITreeViewItem node in childNodes)
            {
                if (node == null || node.NodeObject() == null) continue;
                if (PasteCutTreeItem(node.NodeObject(), this))
                    refreshNeeded = true;
            }
            if (refreshNeeded)
            {
                //refresh source and target folder nodes
                mTreeView.Tree.RefresTreeNodeChildrens(mNodeManipulationsSource);
                mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
            }
        }

        public override void SaveAllTreeFolderItems()
        {
            List<ITreeViewItem> childNodes = mTreeView.Tree.GetTreeNodeChildsIncludingSubChilds((ITreeViewItem)this);
            int itemsSavedCount = 0;
            foreach (ITreeViewItem node in childNodes)
            {
                if (node != null && node.NodeObject() != null)
                {
                    if (ItemIsDirty(node.NodeObject()))
                    {
                        if (SaveTreeItem(node.NodeObject(), true))
                            itemsSavedCount++;
                    }
                }
            }
            if (itemsSavedCount == 0)
            {                
                Reporter.ToUser(eUserMsgKeys.SaveAll, "Nothing found to Save.");
            }
            else
            {
                mTreeView.Tree.SelectItem((ITreeViewItem)this);//in case the event was called from diffrent class                                                             
            }
        }

        public override void RefreshTreeFolder(Type itemType, string path)
        {
            try
            {                
                if (Reporter.ToUser(eUserMsgKeys.RefreshFolder) == MessageBoxResult.Yes)
                {                    
                    //refresh Tree
                    mTreeView.Tree.RefreshHeader((ITreeViewItem)this);
                    mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to refresh the item type cache for the folder: '" + path + "'", ex);
            }
        }

        public override ITreeViewItem AddSubFolder(Type typeOfFolder, string newFolderName, string newFolderPath)
        {
            object folderItem;
            try
            {
                folderItem = Activator.CreateInstance(this.GetType());
            }
            catch (Exception ex)
            {
                folderItem = Activator.CreateInstance(this.GetType(),
                                                                BindingFlags.CreateInstance |
                                                                BindingFlags.Public |
                                                                BindingFlags.Instance |
                                                                BindingFlags.OptionalParamBinding, null, new object[] { Type.Missing }, CultureInfo.CurrentCulture);
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
            if (folderItem == null)
            {
                return null;
            }

            try
            {
                this.GetType().GetProperty("Folder").SetValue(folderItem, newFolderName);
                this.GetType().GetProperty("Path").SetValue(folderItem, newFolderPath);
            }
            catch (Exception ex)
            {
                //return null;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }

            
            if (folderItem != null)
            {
                System.IO.Directory.CreateDirectory(newFolderPath);
                mTreeView.Tree.AddChildItemAndSelect((ITreeViewItem)this, (ITreeViewItem)folderItem);
            }
            else
            {                
                Reporter.ToUser(eUserMsgKeys.FolderOperationError, "Folder Creation Failed");
            }

            return (ITreeViewItem)folderItem;
        }

        public override bool RenameTreeFolder(string originalName, string newFolderName, string newPath)
        {
            try
            {
                string sourcePath = this.NodePath();
                if (sourcePath != newPath)
                {
                    if (originalName.ToUpper() == newFolderName.ToUpper())//user just changed the name letters case
                    {
                        //move to temp folder
                        string tempTargetPath = Path.Combine(Path.GetTempPath(), originalName);
                        Directory.Move(sourcePath, tempTargetPath);
                        sourcePath = tempTargetPath;
                    }

                    Directory.Move(sourcePath, newPath);
                    if (System.IO.Directory.Exists(newPath) == false)
                        return false;
                }
                else
                {
                    return false;
                }

                this.GetType().GetProperty("Folder").SetValue(this, newFolderName);
                this.GetType().GetProperty("Path").SetValue(this, newPath);

                //refresh header and childerns (to get new File name)
                RefreshTreeFolder(this.NodeObjectType(), Path.GetDirectoryName(this.NodePath()));
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                return false;
            }

            return true;
        }

        public override void DeleteTreeFolder()
        {
            if (Reporter.ToUser(eUserMsgKeys.DeleteTreeFolderAreYouSure, mTreeView.Tree.GetSelectedTreeNodeName()) == MessageBoxResult.Yes)
            {
                List<ITreeViewItem> childNodes = mTreeView.Tree.GetTreeNodeChildsIncludingSubChilds((ITreeViewItem)this);
                childNodes.Reverse();
                foreach (ITreeViewItem node in childNodes)
                {
                    if (node == null) continue;
                    if (node.NodeObject() != null)
                    {
                        DeleteTreeItem(node.NodeObject(), true, false);
                    }
                    else
                    {
                        if (Directory.Exists(((TreeViewItemBase)node).NodePath()))
                           Directory.Delete(((TreeViewItemBase)node).NodePath(), true);
                    }
                }

                //delete root and refresh tree                    
                Directory.Delete(this.NodePath(), true);
                mTreeView.Tree.RefreshSelectedTreeNodeParent();                
            }
        }

        public override void AddTreeItem()
        {
            Reporter.ToUser(eUserMsgKeys.MissingImplementation2);
        }
    }
}
