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
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for ModifiedRepositoryFilesPage.xaml
    /// </summary>
    public partial class ModifiedRepositoryFilesPage : Page
    {

        private GenericWindow genWin = null;
        private ImageMakerControl loaderElement = new ImageMakerControl();
        private bool isSaving;
        private ObservableList<ModifiedRepositoryFileInfo> mModifiedFilesInfo;

        public ModifiedRepositoryFilesPage()
        {
            InitializeComponent();
            Init();
            SetGridView();

            UnsavedItemsGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            UnsavedItemsGrid.AddToolbarTool("@CheckAllColumn_16x16.png", "Select All", new RoutedEventHandler(SelectAll));
            UnsavedItemsGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Unselect All", new RoutedEventHandler(UnselectAll));
            UnsavedItemsGrid.AddToolbarTool("@Filter16x16.png", "Filter Items By Item Type", new RoutedEventHandler(FilterItemsByType));
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = nameof(ModifiedRepositoryFileInfo.Selected), Header = "Selected", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.CheckBox, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(ModifiedRepositoryFileInfo.Name), Header = "Item Name", WidthWeight = 20, ReadOnly = true, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(ModifiedRepositoryFileInfo.FileType), Header = "Item Type", WidthWeight = 20, ReadOnly = false, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(ModifiedRepositoryFileInfo.Path), Header = "Item File Name", WidthWeight = 40, ReadOnly = true, AllowSorting = true });

            UnsavedItemsGrid.SetAllColumnsDefaultView(view);
            UnsavedItemsGrid.InitViewItems();
        }

        private void Init()
        {
            try
            {
                loaderElement.Visibility = Visibility.Visible;
                mModifiedFilesInfo = GetModifiedFilesInfo();
                UnsavedItemsGrid.DataSourceList = mModifiedFilesInfo;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Init ViewModifiedRepositoryFilesPage error - " + e.Message, e);
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    loaderElement.Visibility = Visibility.Collapsed;
                });
            }
        }

        internal string SetFileType(RepositoryItemBase file)
        {
            if (file.FilePath.Contains("Ginger.Solution.xml"))
            {
                return nameof(Ginger.SolutionGeneral.Solution);
            }
            SolutionRepositoryItemInfoBase SRII = WorkSpace.Instance.SolutionRepository.GetSolutionRepositoryItemInfo(file.GetType());
            if (String.IsNullOrEmpty(SRII.DisplayName))
            {
                return file.GetType().Name;
            }
            return SRII.DisplayName;
        }
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.OnlyDialog)
        {
            Button saveBtn = new Button();
            saveBtn.Content = "Save";
            saveBtn.Click += SaveButton_Click;
            saveBtn.ToolTip = "Save Selected Files";

            Button undoBtn = new Button();
            undoBtn.Content = "Undo";
            undoBtn.Click += UndoButton_Click;
            undoBtn.ToolTip = "Undo Changes In Selected Files";

            loaderElement.Name = "xProcessingImage";
            loaderElement.Height = 30;
            loaderElement.Width = 30;
            loaderElement.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing;
            loaderElement.Visibility = Visibility.Collapsed;

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Modified Solution Items", this, new ObservableList<Button> { saveBtn }, loaderElement: loaderElement);
        }

        private ObservableList<ModifiedRepositoryFileInfo> GetModifiedFilesInfo()
        {
            ObservableList<ModifiedRepositoryFileInfo> list = new ObservableList<ModifiedRepositoryFileInfo>();

            foreach (RepositoryItemBase RIB in WorkSpace.Instance.SolutionRepository.ModifiedFiles)
            {
                ModifiedRepositoryFileInfo MRFI = new ModifiedRepositoryFileInfo();
                MRFI.Name = RIB.ItemName;
                MRFI.Selected = true;
                MRFI.Path = RIB.FilePath.Substring(RIB.FilePath.LastIndexOf('\\') + 1);
                MRFI.FileType = SetFileType(RIB);
                MRFI.guid = RIB.Guid;
                MRFI.item = RIB;
                list.Add(MRFI);
            }

            return list;
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSaving)
            {
                return;
            }
            isSaving = true;
            await SaveModifiedFilesAsync();
            isSaving = false;
        }
        private async Task SaveModifiedFilesAsync()
        {
            try
            {
                loaderElement.Visibility = Visibility.Visible;
                List<ModifiedRepositoryFileInfo> selectedFiles = mModifiedFilesInfo.Where(x => x.Selected).ToList();
                if (selectedFiles == null || selectedFiles.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.AskToSelectItem);
                    return;
                }
                if (Reporter.ToUser(eUserMsgKey.SaveAllModifiedItems, $"Are you sure you want to save {selectedFiles.Count} Item(s)") == eUserMsgSelection.Yes)
                {
                    await Task.Run(() =>
                   {
                       Parallel.ForEach(selectedFiles.GroupBy(g => g.FileType), fileToSave =>
                       {
                           foreach (var file in fileToSave)
                           {
                               SaveHandler.Save(file.item);
                           }
                       });
                   });
                    Init();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error while saving - " + ex.Message, ex);
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    loaderElement.Visibility = Visibility.Collapsed;
                    Reporter.HideStatusMessage();
                });
            }
        }
        private void SelectAll(object sender, RoutedEventArgs e)
        {
            if (mModifiedFilesInfo == null)
            {
                return;
            }
            foreach (ModifiedRepositoryFileInfo file in mModifiedFilesInfo)
            {
                file.Selected = true;
            }
            UnsavedItemsGrid.DataSourceList = mModifiedFilesInfo;

        }

        private void UnselectAll(object sender, RoutedEventArgs e)
        {
            if (mModifiedFilesInfo == null)
            {
                return;
            }
            foreach (ModifiedRepositoryFileInfo file in mModifiedFilesInfo)
            {
                file.Selected = false;
            }
            UnsavedItemsGrid.DataSourceList = mModifiedFilesInfo;
        }
        private void FilterItemsByType(object sender, RoutedEventArgs e)
        {
            // TODO
        }
        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            Init();
        }
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            // if in mid of save, don't allow undo
            if (isSaving)
            {
                return;
            }
            UndoModifiedFiles();

        }

        private void UndoModifiedFiles()
        {
            try
            {
                loaderElement.Visibility = Visibility.Visible;
                List<ModifiedRepositoryFileInfo> selectedFiles = mModifiedFilesInfo.Where(x => x.Selected).ToList();
                if (selectedFiles == null || selectedFiles.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.AskToSelectItem);
                    return;
                }
                if (Reporter.ToUser(eUserMsgKey.AskIfSureWantToUndoChange, $"Are you sure you want to undo {selectedFiles.Count} Item(s)") == eUserMsgSelection.Yes)
                {
                    foreach (ModifiedRepositoryFileInfo fileToUndo in selectedFiles)
                    {
                        if (fileToUndo.FileType.Equals(nameof(Ginger.SolutionGeneral.Solution)))
                        {
                            // to implement
                        }
                        else
                        {
                            Reporter.ToStatus(eStatusMsgKey.UndoChanges, null, fileToUndo.Name);
                            Ginger.General.UndoChangesInRepositoryItem(fileToUndo.item, true);
                        }
                    }
                    Init();
                }
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    loaderElement.Visibility = Visibility.Collapsed;
                    Reporter.HideStatusMessage();
                });
            }
        }
    }
}