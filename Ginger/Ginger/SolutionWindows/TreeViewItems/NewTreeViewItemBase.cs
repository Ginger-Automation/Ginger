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
using Amdocs.Ginger.UserControls;
using Ginger;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.SourceControl;
using GingerCore;
using GingerCoreNET.SourceControl;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.TreeViewItemsLib
{
    public class NewTreeViewItemBase : TreeViewItemGenericBase
    {
        public SourceControlFileInfo.eRepositoryItemStatus ItemSourceControlStatus;//TODO: combine it with GingerCore one      
        static bool mBulkOperationIsInProcess = false;
        public override bool SaveTreeItem(object item, bool saveOnlyIfDirty = false)
        {
            if (item is RepositoryItemBase)
            {
                RepositoryItemBase RI = (RepositoryItemBase)item;
                if (saveOnlyIfDirty && RI.DirtyStatus != eDirtyStatus.Modified)
                {
                    return false;//no need to Save because not Dirty
                }
                Reporter.ToStatus(eStatusMsgKey.SaveItem, null, RI.ItemName, "item");
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(RI);
                Reporter.HideStatusMessage();

                //refresh node header                               
                PostSaveTreeItemHandler();
                return true;
            }
            else
            {
                //implement for other item types
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Save operation for this item type was not implemented yet.");
                return false;
            }
        }

        public override bool SaveBackup(object item)
        {
            if (item is RepositoryItemBase)
            {
                RepositoryItemBase RI = (RepositoryItemBase)item;
                RI.SaveBackup();
                return true;
            }

            return false;
        }

        public override bool ItemIsDirty(object item)
        {
            if (item is RepositoryItemBase && ((RepositoryItemBase)item).DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified)
            {
                return true;
            }

            return false;
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            var repoItem = item as RepositoryItemBase;
            if (repoItem != null)
            {
                if (!deleteWithoutAsking)
                {
                    if (Reporter.ToUser(eUserMsgKey.DeleteItem, repoItem.GetNameForFileName()) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                    {
                        return false;
                    }
                }

                WorkSpace.Instance.SolutionRepository.DeleteRepositoryItem((RepositoryItemBase)item);
                return true;
            }
            else
            {
                //implement for other item types              
                string filePath = string.Empty;
                if (item == null)
                {
                    filePath = this.NodePath();
                }
                else
                {
                    filePath = ((RepositoryItemBase)item).FilePath;
                }

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return true;
                }
            }

            return false;
        }

        public override void DuplicateTreeItem(object item)
        {
            if (item is RepositoryItemBase)
            {
                Reporter.ToStatus(eStatusMsgKey.DuplicateItem, null, ((RepositoryItemBase)item).ItemName);

                try
                {
                    RepositoryItemBase copiedItem = CopyTreeItemWithNewName((RepositoryItemBase)item);
                    if (copiedItem != null)
                    {
                        copiedItem.DirtyStatus = eDirtyStatus.NoTracked;
                        (WorkSpace.Instance.SolutionRepository.GetItemRepositoryFolder(((RepositoryItemBase)item))).AddRepositoryItem(copiedItem);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Duplicating tree item", ex);
                }
                finally
                {
                    Reporter.HideStatusMessage();
                }
            }
            else
            {
                //implement for other item types
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Item type " + item.GetType().Name + " - operation for this item type was not implemented yet.");
            }
        }


        public override bool PasteCopiedTreeItem(object nodeItemToCopy, TreeViewItemGenericBase targetFolderNode, bool toRefreshFolder = true)
        {
            if (nodeItemToCopy is RepositoryItemBase)
            {
                RepositoryItemBase copiedItem = CopyTreeItemWithNewName((RepositoryItemBase)nodeItemToCopy);
                if (copiedItem != null)
                {
                    copiedItem.DirtyStatus = eDirtyStatus.NoTracked;
                    ((RepositoryFolderBase)(((ITreeViewItem)targetFolderNode).NodeObject())).AddRepositoryItem(copiedItem);
                    return true;
                }
                return false;
            }
            else
            {
                //implement for other item types
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "The " + mCurrentFolderNodePastOperations.ToString() + " operation for this item type was not implemented yet.");
                return false;
            }
        }

        public override void PasteCopiedTreeItems()
        {
            foreach (RepositoryItemBase childItemToCopy in ((RepositoryFolderBase)(((ITreeViewItem)mNodeManipulationsSource).NodeObject())).GetFolderRepositoryItems())
            {
                RepositoryItemBase copiedItem = CopyTreeItemWithNewName((RepositoryItemBase)childItemToCopy);
                if (copiedItem != null)
                {
                    copiedItem.DirtyStatus = eDirtyStatus.NoTracked;
                    ((RepositoryFolderBase)(((ITreeViewItem)this).NodeObject())).AddRepositoryItem(copiedItem);
                }
            }
        }

        public override bool PasteCutTreeItem(object nodeItemToCut, TreeViewItemGenericBase targetFolderNode, bool toRefreshFolder = true)
        {
            if (nodeItemToCut is RepositoryItemBase)
            {
                WorkSpace.Instance.SolutionRepository.MoveItem((RepositoryItemBase)nodeItemToCut, targetFolderNode.NodePath());
                return true;
            }
            else
            {
                //implement for other item types
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "The " + mCurrentFolderNodePastOperations.ToString() + " operation for this item type was not implemented yet.");
                return false;
            }
        }

        public override void PasteCutTreeItems()
        {
            foreach (RepositoryItemBase childItemToCut in ((RepositoryFolderBase)(((ITreeViewItem)mNodeManipulationsSource).NodeObject())).GetFolderRepositoryItems())
            {
                PasteCutTreeItem((RepositoryItemBase)childItemToCut, this);
            }
        }

        public override void SaveAllTreeFolderItems()
        {
            List<ITreeViewItem> childNodes = mTreeView.Tree.GetTreeNodeChildsIncludingSubChilds((ITreeViewItem)this);
            bool showConfirmation = true;
            int itemsSavedCount = 0;
            foreach (ITreeViewItem node in childNodes)
            {
                if (node != null && node.NodeObject() is RepositoryItemBase)
                {
                    RepositoryItemBase RI = (RepositoryItemBase)node.NodeObject();
                    if (RI != null)
                    {
                        if (RI.DirtyStatus == eDirtyStatus.Modified)
                        {
                            // Try to save only items with file name = standalone xml, avoid items like env app
                            if (RI is Activity && showConfirmation && Reporter.ToUser(eUserMsgKey.AskIfWantsToUpdateAllLinkedRepoItem) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                            {
                                return;
                            }
                            showConfirmation = false;
                            if (!string.IsNullOrEmpty(RI.ContainingFolder))
                            {
                                if (SaveTreeItem(node.NodeObject(), true))
                                {
                                    itemsSavedCount++;
                                    if (RI is Activity)
                                    {
                                        ((SharedActivityTreeItem)node).PostSaveTreeItemHandler();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (itemsSavedCount == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Nothing found to Save.");
            }
        }

        public override void RefreshTreeFolder(Type itemType, string path)
        {
            try
            {
                if (Reporter.ToUser(eUserMsgKey.RefreshFolder) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    mBulkOperationIsInProcess = true;
                    //refresh cache
                    RepositoryFolderBase repoFolder = (RepositoryFolderBase)(((ITreeViewItem)this).NodeObject());
                    if (repoFolder != null)
                        repoFolder.ReloadItems(); // .RefreshFolderCache();

                    //refresh tree
                    mTreeView.Tree.RefresTreeNodeChildrens((ITreeViewItem)this);

                    mBulkOperationIsInProcess = false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.RefreshFailed, "Failed to refresh the item type cache for the folder: " + path);
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                mBulkOperationIsInProcess = false;
            }
        }

        public override ITreeViewItem AddSubFolder(Type typeOfFolder, string newFolderName, string newFolderPath)
        {
            try
            {
                RepositoryFolderBase repoFolder = (RepositoryFolderBase)(((ITreeViewItem)this).NodeObject());
                repoFolder.AddSubFolder(newFolderName);

                return null;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return null;
            }
        }

        public override bool RenameTreeFolder(string originalName, string newFolderName, string newPath)
        {
            try
            {
                RepositoryFolderBase repoFolder = (RepositoryFolderBase)((ITreeViewItem)this).NodeObject();
                repoFolder.RenameFolder(newFolderName);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return false;
            }
            return true;
        }

        public override void DeleteTreeFolder()
        {
            try
            {
                if (Reporter.ToUser(eUserMsgKey.DeleteTreeFolderAreYouSure, mTreeView.Tree.GetSelectedTreeNodeName()) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    WorkSpace.Instance.SolutionRepository.DeleteRepositoryItemFolder((RepositoryFolderBase)((ITreeViewItem)this).NodeObject());
                }
            }
            finally
            {
                mBulkOperationIsInProcess = false;
            }
        }

        public override void AddTreeItem()
        {
            Reporter.ToUser(eUserMsgKey.MissingImplementation);
        }

        public virtual ITreeViewItem GetFolderTreeItem(RepositoryFolderBase folder)
        {
            throw new NotImplementedException();
        }

        public virtual ITreeViewItem GetTreeItem(object item)
        {
            throw new NotImplementedException();
        }

        public void TreeFolderItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (mTreeView == null || e == null) return;

            if (mBulkOperationIsInProcess) return;

            // Since refresh of tree items can be triggered from FileWatcher running on separate thread, all TV handling is done on the TV.Dispatcher
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null && e.NewItems.Count > 0)
                    {
                        mTreeView.Tree.Dispatcher.Invoke(() =>
                        {
                            mTreeView.Tree.AddChildItemAndSelect((ITreeViewItem)this, GetTreeItem((dynamic)e.NewItems[0]));
                        });
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null && e.OldItems.Count > 0)
                    {
                        mTreeView.Tree.Dispatcher.Invoke(() =>
                        {
                            mTreeView.Tree.DeleteItemByObjectAndSelectParent(e.OldItems[0], (ITreeViewItem)this);
                        });
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    mTreeView.Tree.Dispatcher.Invoke(() =>
                    {
                        mTreeView.Tree.RefresTreeNodeChildrens((ITreeViewItem)this);
                    });
                    break;
            }
        }
        public override void AddSourceControlOptions(ContextMenu CM, bool addDetailsOption = true, bool addLocksOption = true)
        {
            if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.SourceControl != null)
            {
                MenuItem sourceControlMenu = TreeViewUtils.CreateSubMenu(CM, "Source Control");
                if (addDetailsOption)
                    TreeViewUtils.AddSubMenuItem(sourceControlMenu, "Get Info", SourceControlGetInfo, null, "@Info_16x16.png");
                TreeViewUtils.AddSubMenuItem(sourceControlMenu, "Check-In Changes", SourceControlCheckIn, null, "@CheckIn2_16x16.png");
                if (WorkSpace.Instance.Solution.SourceControl.IsSupportingGetLatestForIndividualFiles)
                    TreeViewUtils.AddSubMenuItem(sourceControlMenu, "Get Latest Version", SourceControlGetLatestVersion, null, "@GetLatest2_16x16.png");
                if (WorkSpace.Instance.Solution.ShowIndicationkForLockedItems && WorkSpace.Instance.Solution.SourceControl.IsSupportingLocks && addLocksOption)
                {
                    TreeViewUtils.AddSubMenuItem(sourceControlMenu, "Lock Item", SourceControlLock, null, "@Lock_16x16.png");
                    TreeViewUtils.AddSubMenuItem(sourceControlMenu, "UnLock Item", SourceControlUnlock, null, "@Unlock_16x16.png");
                }
                TreeViewUtils.AddSubMenuItem(sourceControlMenu, "Undo Changes", SourceControlUndoChanges, null, "@Undo_16x16.png");
            }
        }

        private void SourceControlGetInfo(object sender, RoutedEventArgs e)
        {
            SourceControlItemInfoDetails SCIID = SourceControlIntegration.GetInfo(WorkSpace.Instance.Solution.SourceControl, this.NodePath());
            if (SCIID != null)
            {
                SourceControlItemInfoPage SCIIP = new SourceControlItemInfoPage(SCIID);
                SCIIP.ShowAsWindow();
            }
        }

        private void SourceControlUnlock(object sender, RoutedEventArgs e)
        {
            RepositoryItemBase RI = ((ITreeViewItem)this).NodeObject() as RepositoryItemBase;

            if (RI != null && RI.SourceControlStatus != eImageType.SourceControlLockedByMe && RI.SourceControlStatus != eImageType.SourceControlLockedByAnotherUser)
            {
                Reporter.ToUser(eUserMsgKey.SoruceControlItemAlreadyUnlocked);
                return;
            }
            SourceControlIntegration.UnLock(WorkSpace.Instance.Solution.SourceControl, this.NodePath());
            mTreeView.Tree.RefreshHeader((ITreeViewItem)this);
        }

        private void SourceControlLock(object sender, RoutedEventArgs e)
        {
            RepositoryItemBase RI = ((ITreeViewItem)this).NodeObject() as RepositoryItemBase;

            if (RI != null && (RI.SourceControlStatus == eImageType.SourceControlLockedByMe || RI.SourceControlStatus == eImageType.SourceControlLockedByAnotherUser))
            {
                Reporter.ToUser(eUserMsgKey.SourceControlItemAlreadyLocked);
                return;
            }
            string lockComment = string.Empty;
            if (GingerCore.General.GetInputWithValidation("Lock", "Lock Comment:", ref lockComment, null, false, RI))
            {
                SourceControlIntegration.Lock(WorkSpace.Instance.Solution.SourceControl, this.NodePath(), lockComment);
                mTreeView.Tree.RefreshHeader((ITreeViewItem)this);
            }
        }

        public void SourceControlCheckIn(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckInPage CIW = new CheckInPage(this.NodePath());
            CIW.ShowAsWindow();
        }

        public void SourceControlUndoChanges(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SureWantToDoRevert) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                Reporter.ToStatus(eStatusMsgKey.RevertChangesFromSourceControl);
                SourceControlIntegration.Revert(WorkSpace.Instance.Solution.SourceControl, this.NodePath());
                Reporter.HideStatusMessage();
            }
        }

        public void SourceControlGetLatestVersion(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.LoseChangesWarn) == Amdocs.Ginger.Common.eUserMsgSelection.No) return;

            Reporter.ToStatus(eStatusMsgKey.GetLatestFromSourceControl);
            if (string.IsNullOrEmpty(this.NodePath()))
                Reporter.ToUser(eUserMsgKey.SourceControlUpdateFailed, "Invalid Path provided");
            else
                SourceControlUI.GetLatest(this.NodePath(), WorkSpace.Instance.Solution.SourceControl);
            Reporter.HideStatusMessage();
        }


        protected eImageType GetSourceControlImage(RepositoryItemBase repositoryItem)
        {
            return GetSourceControlImageByPath(repositoryItem.FilePath);
        }


        protected eImageType GetSourceControlImage(RepositoryFolderBase repositoryFolderBase)
        {
            return GetSourceControlImageByPath(repositoryFolderBase.FolderFullPath);
        }

        protected eImageType GetSourceControlImageByPath(string path)
        {
            return SourceControlIntegration.GetFileImage(path);
        }

        protected List<ITreeViewItem> GetChildrentGeneric<T>(RepositoryFolder<T> RF)
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            ObservableList<RepositoryFolder<T>> subFolders = RF.GetSubFolders();
            foreach (RepositoryFolder<T> subFolder in subFolders)
            {
                Childrens.Add(GetTreeItem(subFolder));
            }
            subFolders.CollectionChanged -= TreeFolderItems_CollectionChanged; // track sub folders
            subFolders.CollectionChanged += TreeFolderItems_CollectionChanged; // track sub folders

            //Add direct children's        
            ObservableList<T> folderItems = RF.GetFolderItems();
            // why we need -? in case we did refresh and reloaded the item TODO: research, make children called once
            folderItems.CollectionChanged -= TreeFolderItems_CollectionChanged;
            folderItems.CollectionChanged += TreeFolderItems_CollectionChanged;//adding event handler to add/remove tree items automatically based on folder items collection changes

            if (folderItems.Count > 0)
            {
                object sampleItem = folderItems[0];
                foreach (T item in folderItems.OrderBy(((RepositoryItemBase)sampleItem).ItemNameField))
                {
                    ITreeViewItem tvi = GetTreeItem(item);
                    Childrens.Add(tvi);
                }
            }
            return Childrens;
        }

        /// <summary>
        /// The function creates the tree node item header
        /// </summary>
        /// <param name="repoItem">The repository item which the tree nodes represents</param>
        /// <param name="imageType">The image type which associated with the repository item- should be pulled from the repoItem</param>
        /// <param name="NameProperty">The field of the item which holds the item name or static name in case the repository item is null</param>
        /// <returns></returns>
        protected StackPanel NewTVItemHeaderStyle(RepositoryItemBase repoItem, eImageType imageType = eImageType.Null, string NameProperty = "")
        {
            //The new item style with Source control
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            if (WorkSpace.Instance.SourceControl != null)
            {
                // Source control image
                ImageMakerControl sourceControlImage = new ImageMakerControl();
                sourceControlImage.BindControl(repoItem, nameof(RepositoryItemBase.SourceControlStatus));
                sourceControlImage.Width = 8;
                sourceControlImage.Height = 8;
                stack.Children.Add(sourceControlImage);

                // Since it might take time to get the item status from SCM server 
                // we run it on task so update will happen when status come back and we do not block the UI
                Task.Factory.StartNew(() =>
                {
                    repoItem.RefreshSourceControlStatus();
                });
            }

            // Add Item Image            
            ImageMakerControl NodeImageType = new ImageMakerControl();
            if (imageType == eImageType.Null)
            {
                NodeImageType.ImageType = repoItem.ItemImageType;
            }
            else
            {
                NodeImageType.ImageType = imageType;
            }

            NodeImageType.Width = 16;
            NodeImageType.Height = 16;
            stack.Children.Add(NodeImageType);

            // Add Item header text 
            Label itemHeaderLabel = new Label();

            string nameFieldProperty;
            if (string.IsNullOrEmpty(NameProperty))
            {
                nameFieldProperty = repoItem.ItemNameField;
            }
            else
            {
                nameFieldProperty = NameProperty;
            }
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(itemHeaderLabel, Label.ContentProperty, repoItem, nameFieldProperty, BindingMode: System.Windows.Data.BindingMode.OneWay);


            stack.Children.Add(itemHeaderLabel);

            // add icon of dirty status            
            ImageMakerControl dirtyStatusImage = new ImageMakerControl();
            dirtyStatusImage.BindControl(repoItem, nameof(RepositoryItemBase.DirtyStatusImage));
            dirtyStatusImage.Width = 6;
            dirtyStatusImage.Height = 6;
            dirtyStatusImage.VerticalAlignment = VerticalAlignment.Top;
            dirtyStatusImage.Margin = new Thickness(0, 10, 10, 0);
            stack.Children.Add(dirtyStatusImage);

            return stack;
        }

        /// <summary>
        /// The function creates the folder tree node header
        /// </summary>
        /// <param name="repoItemFolder">the Repository Folder Base</param>      
        /// <param name="imageType">Only if need different icon than default one then require to provide it</param> 
        /// <returns></returns>
        protected StackPanel NewTVItemFolderHeaderStyle(RepositoryFolderBase repoItemFolder, eImageType imageType = eImageType.Null)
        {
            //The new item style with Source control
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            if (WorkSpace.Instance.SourceControl != null)
            {
                // Source control image
                ImageMakerControl sourceControlImage = new ImageMakerControl();
                sourceControlImage.BindControl(repoItemFolder, nameof(RepositoryFolderBase.SourceControlStatus));
                sourceControlImage.Width = 8;
                sourceControlImage.Height = 8;
                sourceControlImage.Margin = new Thickness(0, 0, 2, 0);
                stack.Children.Add(sourceControlImage);

                // Since it might take time to get the item status from SCM server 
                // we run it on task so update will happen when status come back and we do not block the UI
                Task.Factory.StartNew(() =>
                {
                    repoItemFolder.RefreshFolderSourceControlStatus();
                });
            }

            // Add Item Image            
            ImageMakerControl NodeImageType = new ImageMakerControl();
            if (imageType == eImageType.Null)
            {
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(NodeImageType, ImageMakerControl.ImageTypeProperty, repoItemFolder, nameof(RepositoryFolderBase.FolderImageType), BindingMode: System.Windows.Data.BindingMode.OneWay);
            }
            else
            {
                NodeImageType.ImageType = imageType;
            }

            NodeImageType.Width = 16;
            NodeImageType.Height = 16;
            stack.Children.Add(NodeImageType);

            // Add Item header text 
            Label itemHeaderLabel = new Label();
            itemHeaderLabel.BindControl(repoItemFolder, "DisplayName");
            stack.Children.Add(itemHeaderLabel);


            return stack;
        }

        public override void DeleteAllTreeItems()
        {
            if (Reporter.ToUser(eUserMsgKey.DeleteTreeFolderAreYouSure, mTreeView.Tree.GetSelectedTreeNodeName()) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                List<ITreeViewItem> childNodes = mTreeView.Tree.GetTreeNodeChildsIncludingSubChilds((ITreeViewItem)this);
                childNodes.Reverse();
                foreach (ITreeViewItem node in childNodes)
                {
                    if (node == null) continue;
                    if (node.NodeObject() != null)
                    {
                        if (node.NodeObject() is RepositoryFolderBase)
                        {
                            WorkSpace.Instance.SolutionRepository.DeleteRepositoryItemFolder((RepositoryFolderBase)node.NodeObject());
                        }
                        else if (node.NodeObject() is RepositoryItemBase)
                        {
                            ((NewTreeViewItemBase)node).DeleteTreeItem(node.NodeObject(), true, false);
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, "Exception while deleting" + node.NodeObject());
                        }
                    }
                    else
                    {
                        if (Directory.Exists(this.NodePath()))
                        {
                            String[] DocFolderChildItems = Directory.GetDirectories(this.NodePath());
                            foreach (String path in DocFolderChildItems)
                            {
                                Directory.Delete(path, true);
                            }
                        }
                        break;
                    }
                }
            }
            mTreeView.Tree.RefreshSelectedTreeNodeParent();
        }
    }
}
