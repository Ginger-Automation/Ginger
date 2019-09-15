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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ginger.SolutionAutoSaveAndRecover
{
    /// <summary>
    /// Interaction logic for RecoverPage.xaml
    /// </summary>
    public partial class RecoverPage : Page
    {
        GenericWindow _pageGenericWin = null;

        ObservableList<RecoveredItem> mRecoveredItems;
        bool selected = false;

        public RecoverPage(ObservableList<RecoveredItem> recoveredItems)
        {
            InitializeComponent();
            this.mRecoveredItems = recoveredItems;
            SetGridView();
        }
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, closeEventHandler: closeEventHandler);

        }

        private void closeEventHandler(object sender, EventArgs e)
        {
            //App.AppSolutionRecover.CleanUp();     //??why cleanup?       
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            List<RecoveredItem> SelectedFiles = mRecoveredItems.Where(x => x.Selected == true && (x.Status != eRecoveredItemStatus.Deleted && x.Status != eRecoveredItemStatus.Recovered)).ToList();

            if (SelectedFiles == null || SelectedFiles.Count == 0)
            {
                //TODO: please select valid Recover items to delete
                Reporter.ToUser(eUserMsgKey.RecoverItemsMissingSelectionToRecover, "delete");
                return;
            }

            foreach (RecoveredItem Ri in SelectedFiles)
            {
                try
                {
                    File.Delete(Ri.RecoveredItemObject.FileName);
                    Ri.Status = eRecoveredItemStatus.Deleted;
                }
                catch
                {
                    Ri.Status = eRecoveredItemStatus.DeleteFailed;
                }
            }

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
                        originalItem = businessFlows.Where(x => x.Guid == ri.RecoveredItemObject.Guid).FirstOrDefault();
                    }
                    else if (ri.RecoveredItemObject is Run.RunSetConfig)
                    {
                        ObservableList<RunSetConfig> Runsets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                        originalItem = Runsets.Where(x => x.Guid == ri.RecoveredItemObject.Guid).FirstOrDefault();
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

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.Selected), WidthWeight = 3, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.RecoverDate), Header = "Recovered DateTime", WidthWeight = 3, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.RecoveredItemType), Header = "Item Type", WidthWeight = 10, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.RecoverItemName), Header = "Item Name", WidthWeight = 10, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.RecoverPath), Header = "Item Original Path", WidthWeight = 15, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(RecoveredItem.Status), Header = "Status", WidthWeight = 15, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = "View Details", WidthWeight = 8, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.RecoveredItems.Resources["ViewDetailsButton"] });

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xDoNotAskAgainChkbox, CheckBox.IsCheckedProperty, WorkSpace.Instance.UserProfile, nameof(UserProfile.DoNotAskToRecoverSolutions));

            xRecoveredItemsGrid.SetAllColumnsDefaultView(view);
            xRecoveredItemsGrid.InitViewItems();
            xRecoveredItemsGrid.SetTitleLightStyle = true;
            xRecoveredItemsGrid.DataSourceList = this.mRecoveredItems;
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
                    GingerWPF.BusinessFlowsLib.BusinessFlowViewPage w = new GingerWPF.BusinessFlowsLib.BusinessFlowViewPage((BusinessFlow)selectedItem.RecoveredItemObject, new Context(), General.eRIPageViewMode.View);
                    w.Width = 1000;
                    w.Height = 800;
                    w.ShowAsWindow();
                }
                if (selectedItem.RecoveredItemObject is Run.RunSetConfig)
                {
                    Run.NewRunSetPage newRunSetPage = new Run.NewRunSetPage((Run.RunSetConfig)selectedItem.RecoveredItemObject, Run.NewRunSetPage.eEditMode.View);
                    newRunSetPage.Width = 1000;
                    newRunSetPage.Height = 800;
                    newRunSetPage.ShowAsWindow();
                }
            }
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            if (mRecoveredItems == null) return;
            if (selected == false)
                selected = true;
            else
                selected = false;
            foreach (RecoveredItem RI in mRecoveredItems)
            {
                RI.Selected = selected;
            }
            xRecoveredItemsGrid.DataSourceList = mRecoveredItems;
        }
    }
}
