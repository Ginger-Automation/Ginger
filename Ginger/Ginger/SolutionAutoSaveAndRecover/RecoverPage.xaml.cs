#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.UserControls;
using GingerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ginger.SolutionAutoSaveAndRecover
{
    /// <summary>
    /// Interaction logic for RecoverPage.xaml
    /// </summary>
    public partial class RecoverPage : Page, IDisposable
    {
        GenericWindow? _pageGenericWin = null;
        ObservableList<RecoveredItem> mRecoveredItems;
        bool selected = false;

        public RecoverPage()
        {
            InitializeComponent();
            SetGridView();
        }
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            mRecoveredItems = [];
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, null, false);
        }


        private async Task LoadFiles()
        {
            if (Directory.Exists(WorkSpace.Instance.AppSolutionRecover.RecoverFolderPath))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        this.ShowLoader();
                        NewRepositorySerializer serializer = new NewRepositorySerializer();

                        foreach (var directory in new DirectoryInfo(WorkSpace.Instance.AppSolutionRecover.RecoverFolderPath).GetDirectories())
                        {
                            string timestamp = directory.Name.Replace("AutoSave_", string.Empty);

                            IEnumerable<FileInfo> files = directory.GetFiles("*", SearchOption.AllDirectories);

                            foreach (var file in files)
                            {
                                try
                                {
                                    RecoveredItem recoveredItem = new RecoveredItem
                                    {
                                        RecoveredItemObject = serializer.DeserializeFromFile(file.FullName),
                                        RecoverDate = timestamp
                                    };
                                    recoveredItem.RecoveredItemObject.FileName = file.FullName;
                                    recoveredItem.RecoveredItemObject.ContainingFolder = file.FullName.Replace(directory.FullName, "~");
                                    recoveredItem.Status = eRecoveredItemStatus.PendingRecover;
                                    this.mRecoveredItems.Add(recoveredItem);
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "Failed to fetch recover item : " + file.FullName, ex);
                                }
                            }
                        }
                        this.HideLoader();
                        xRecoveredItemsGrid.DataSourceList = this.mRecoveredItems;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to Load files", ex);
                    }
                });

            }
        }

        private void HideLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xProcessingImage.Visibility = Visibility.Collapsed;
            });
        }

        private void ShowLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xProcessingImage.Visibility = Visibility.Visible;
            });
        }

        private void RecoverButton_Click(object sender, RoutedEventArgs e)
        {
            List<RecoveredItem> SelectedFiles = mRecoveredItems.Where(x => x.Selected == true && (x.Status == eRecoveredItemStatus.PendingRecover || x.Status == eRecoveredItemStatus.RecoveredFailed)).ToList();

            if (SelectedFiles == null || SelectedFiles.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.RecoverItemsMissingSelectionToRecover, "recover");
                return;
            }

            foreach (RecoveredItem ri in SelectedFiles)
            {
                RepositoryItemBase originalItem = null;

                try
                {
                    if (ri.RecoveredItemObject is BusinessFlow)
                    {
                        ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                        originalItem = businessFlows.FirstOrDefault(x => x.Guid == ri.RecoveredItemObject.Guid);
                    }
                    else if (ri.RecoveredItemObject is Run.RunSetConfig)
                    {
                        ObservableList<RunSetConfig> Runsets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                        originalItem = Runsets.FirstOrDefault(x => x.Guid == ri.RecoveredItemObject.Guid);
                    }
                    if (originalItem == null)
                    {
                        ri.Status = eRecoveredItemStatus.RecoveredFailed;
                        return;
                    }
                    File.Delete(originalItem.FileName);
                    File.Move(ri.RecoveredItemObject.FileName, originalItem.FileName);

                    ri.Status = eRecoveredItemStatus.Recovered;
                }
                catch (Exception ex)
                {
                    ri.Status = eRecoveredItemStatus.RecoveredFailed;
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to recover the original file '{0}' with the recovered file '{1}'", originalItem.FileName, ri.RecoveredItemObject.FileName), ex);
                }
            }

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (mRecoveredItems != null && mRecoveredItems.Any(x => x.Status == eRecoveredItemStatus.PendingRecover))
            {
                if (Reporter.ToUser(eUserMsgKey.DeleteRecoverFolderWarn) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                {
                    return;
                }
            }
            WorkSpace.Instance.AppSolutionRecover.CleanUpRecoverFolder();
            _pageGenericWin.Close();
        }

        private async Task SetGridView()
        {

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = [];
            view.GridColsView = viewCols;

            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.Selected), WidthWeight = 3, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.RecoverDate), Header = "Recovered DateTime", WidthWeight = 3, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.RecoveredItemType), Header = "Item Type", WidthWeight = 10, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.RecoverItemName), Header = "Item Name", WidthWeight = 10, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.RecoverPath), Header = "Item Original Path", WidthWeight = 15, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.Status), Header = "Status", WidthWeight = 15, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = "View Details", WidthWeight = 8, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.RecoveredItems.Resources["ViewDetailsButton"] });

            xRecoveredItemsGrid.SetAllColumnsDefaultView(view);
            xRecoveredItemsGrid.InitViewItems();
            xRecoveredItemsGrid.SetTitleLightStyle = true;
            xRecoveredItemsGrid.DataSourceList = this.mRecoveredItems;
            await LoadFiles();
            xRecoveredItemsGrid.Grid.MouseDoubleClick += xRecoveredItemsGrid_MouseDoubleClick;
            xRecoveredItemsGrid.AddToolbarTool("@CheckAllColumn_16x16.png", "Select All", new RoutedEventHandler(SelectAll));

        }
        private void ViewDetailsClicked(object sender, RoutedEventArgs e)
        {
            ShowPage();
        }
        private void xRecoveredItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowPage();
        }

        public void ShowPage()
        {
            if (xRecoveredItemsGrid.Grid.SelectedItem != null)
            {
                RecoveredItem selectedItem = (RecoveredItem)xRecoveredItemsGrid.Grid.SelectedItem;
                if (selectedItem.RecoveredItemObject is BusinessFlow)
                {
                    GingerWPF.BusinessFlowsLib.BusinessFlowViewPage w = new GingerWPF.BusinessFlowsLib.BusinessFlowViewPage((BusinessFlow)selectedItem.RecoveredItemObject, new Context(), General.eRIPageViewMode.View)
                    {
                        Width = 1000,
                        Height = 800
                    };
                    w.ShowAsWindow();
                }
                if (selectedItem.RecoveredItemObject is Run.RunSetConfig)
                {
                    Run.NewRunSetPage newRunSetPage = new Run.NewRunSetPage((Run.RunSetConfig)selectedItem.RecoveredItemObject, Run.NewRunSetPage.eEditMode.View)
                    {
                        Width = 1000,
                        Height = 800
                    };
                    newRunSetPage.ShowAsWindow();
                }
            }
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            if (mRecoveredItems == null)
            {
                return;
            }

            if (selected == false)
            {
                selected = true;
            }
            else
            {
                selected = false;
            }

            foreach (RecoveredItem RI in mRecoveredItems)
            {
                RI.Selected = selected;
            }
            xRecoveredItemsGrid.DataSourceList = mRecoveredItems;
            xRecoveredItemsGrid.DataSourceList = mRecoveredItems;
        }

        public void Dispose()
        {
            mRecoveredItems?.ClearAll();
            _pageGenericWin.ClearControlsBindings();
            _pageGenericWin = null;
            GC.SuppressFinalize(this);
        }

        ~RecoverPage()
        {
            Dispose();
        }
    }
}
