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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using Ginger.UserControls;
using GingerCore;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.ActionConversion
{
    /// <summary>
    /// Interaction logic for SelectBusinessFlowWzardPage.xaml
    /// </summary>
    public partial class SelectBusinessFlowWzardPage : Page, IWizardPage
    {
        ActionsConversionWizard mWizard;
        public object BusinessFlowFolder { get; set; }
        SingleItemTreeViewSelectionPage mBFSelectionPage = null;
        ObservableList<BusinessFlow> ListOfBusinessFlow = null;
        Context mContext = null;

        public SelectBusinessFlowWzardPage(ObservableList<BusinessFlow> listOfBusinessFlow, Context context)
        {
            InitializeComponent();

            ListOfBusinessFlow = listOfBusinessFlow;
            mContext = context;
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (ActionsConversionWizard)WizardEventArgs.Wizard;
                    SetGridsView();
                    break;
                case EventType.LeavingForNextPage:
                    SetActivitiesSelected();
                    break;
            }
        }

        /// <summary>
        /// This method is used to set the acitvities for selected to conversion
        /// </summary>
        private void SetActivitiesSelected()
        {
            foreach (var businessFlow in ListOfBusinessFlow)
            {
                if (businessFlow.Selected)
                {
                    foreach (var act in businessFlow.Activities)
                    {
                        act.SelectedForConversion = true;
                    }
                }
            }
        }

        /// <summary>
        /// This method sets the gridview to display the selected businessflows
        /// </summary>
        private void SetGridsView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlow.Selected), WidthWeight = 5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox, Header = "Select", BindingMode = System.Windows.Data.BindingMode.TwoWay });
            defView.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlow.ContainingFolder), WidthWeight = 15, Header = "Folder" });
            defView.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlow.Name), WidthWeight = 15, Header = "Name" });
            defView.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlow.Description), WidthWeight = 15, Header = "Description" });

            xBusinessFlowGrid.SetAllColumnsDefaultView(defView);
            xBusinessFlowGrid.InitViewItems();
            xBusinessFlowGrid.SetTitleLightStyle = true;
            xBusinessFlowGrid.btnMarkAll.Visibility = System.Windows.Visibility.Visible;
            xBusinessFlowGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddBusinessFlow));

            xBusinessFlowGrid.DataSourceList = SetDefaultSelectedBusinessFlows();
            xBusinessFlowGrid.RowChangedEvent += grdGroups_RowChangedEvent;
            xBusinessFlowGrid.Title = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + " to Convert";
            xBusinessFlowGrid.MarkUnMarkAllActive += MarkUnMarkAllActivities;
            xBusinessFlowGrid.ValidationRules = new List<ucGrid.eUcGridValidationRules>()
            {
                ucGrid.eUcGridValidationRules.CheckedRowCount
            };
        }

        /// <summary>
        /// This method by default selects the businessflows to convert
        /// </summary>
        /// <returns></returns>
        private IObservableList SetDefaultSelectedBusinessFlows()
        {
            foreach (BusinessFlow bf in ListOfBusinessFlow)
            {
                bf.Selected = true;
            }
            return ListOfBusinessFlow;
        }

        /// <summary>
        /// This event is used to open the popup for selection of BusinessFlow and add it to the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddBusinessFlow(object sender, RoutedEventArgs e)
        {
            if (mBFSelectionPage == null)
            {
                RepositoryFolder<BusinessFlow> repositoryFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>();
                BusinessFlowsFolderTreeItem bfsRoot = new BusinessFlowsFolderTreeItem(repositoryFolder);
                mBFSelectionPage = new SingleItemTreeViewSelectionPage("Business Flow", eImageType.BusinessFlow, bfsRoot,
                                                                                        SingleItemTreeViewSelectionPage.eItemSelectionType.MultiStayOpenOnDoubleClick, false);
                mBFSelectionPage.SelectionDone += MBFSelectionPage_SelectionDone;
            }
            List<object> selectedBFs = mBFSelectionPage.ShowAsWindow();
            AddSelectedBF(selectedBFs);
        }

        /// <summary>
        /// This method is used to add the selected businessflow list which adds to grid
        /// </summary>
        /// <param name="selectedBFs"></param>
        private void AddSelectedBF(List<object> selectedBFs)
        {
            if (selectedBFs != null && selectedBFs.Count > 0)
            {
                foreach (var bf in selectedBFs)
                {
                    ((BusinessFlow)bf).Selected = true;
                    ListOfBusinessFlow.Add((BusinessFlow)bf);
                }
            }
        }

        /// <summary>
        /// This event is used to handle the selection of businessflow on the popup page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MBFSelectionPage_SelectionDone(object sender, SelectionTreeEventArgs e)
        {
            AddSelectedBF(e.SelectedItems);
        }

        private void grdGroups_RowChangedEvent(object sender, EventArgs e)
        {
            if (mContext.BusinessFlow != null)
            {
                mContext.BusinessFlow.CurrentActivity = (Activity)xBusinessFlowGrid.CurrentItem;
            }
        }

        private void MarkUnMarkAllActivities(bool businessFlowStatus)
        {
            foreach (BusinessFlow bf in xBusinessFlowGrid.DataSourceList)
            {
                bf.Selected = businessFlowStatus;
            }
        }
    }
}