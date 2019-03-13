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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Ginger.UserControls;
using GingerCore.Actions;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ActionLib
{
    /// <summary>
    /// Interaction logic for AddActionPage.xaml
    /// </summary>
    public partial class NewAddActionPage : Page
    {
        GenericWindow _pageGenericWin = null;
        ObservableList<IAct> mActionsList;

        public NewAddActionPage()
        {
            InitializeComponent();
            SetActionsGridsView();
            LoadGridData();
            LoadPlugInsActions();
            LoadGeneralActions();
        }

        private void LoadGeneralActions()
        {
            //ObservableList<StandAloneAction> actions = ActionFactory.GetAllStandAloneActions();
            //GeneralActionsGrid.DataSourceList = actions;
            //SetActionsGridView(GeneralActionsGrid);
        }

        private void LoadPlugInsActions()
        {
            //ObservableList<DriverAction> actions = ActionFactory.GetAllGenericActions();
            //PlatformActionsGrid.DataSourceList = actions;

            //PlugInsActionsGrid.DataSourceList = ActionFactory.GetAllPluginActions();
            //SetActionsGridView(PlugInsActionsGrid);
        }

        private void LoadGridData()
        {
        }

        private ObservableList<Act> GetPlatformsActions(bool ShowAll = false)
        {
            ObservableList<Act> Acts = new ObservableList<Act>();
            return Acts;
        }

        private void SetActionsGridsView()
        {
            //SetActionsGridView(PlatformActionsGrid);
        }

        private void SetActionsGridView(ucGrid actionsGrid)
        {
            //GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            //view.GridColsView = new ObservableList<GridColView>();
            //view.GridColsView.Add(new GridColView() { Field = nameof(DriverAction.ID), Header = "ID", AllowSorting = true, WidthWeight = 4 });
            //actionsGrid.SetAllColumnsDefaultView(view);
            //actionsGrid.InitViewItems();
            //actionsGrid.grdMain.SelectionMode = DataGridSelectionMode.Single;
            //actionsGrid.RowDoubleClick += ActionsGrid_MouseDoubleClick;
        }

        private void AddAction()
        {
            //Act DA = null;
            //if (ActionsTabs.SelectedItem == PlatformActionsTab)
            //{
            //    DA = (DriverAction)PlatformActionsGrid.CurrentItem;
            //}

            //if (ActionsTabs.SelectedItem == PlugInsActionsTab)
            //{
            //    DA = (DriverAction)PlugInsActionsGrid.CurrentItem;
            //}

            //if (ActionsTabs.SelectedItem == GeneralActionsTab)
            //{
            //    DA = (StandAloneAction)GeneralActionsGrid.CurrentItem;
            //}

            //if (DA != null)
            //{
            //    mActionsList.Add(DA);
            //}
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddAction();
            }
            catch (NullReferenceException)
            {
                //TODO: Enable adding new action in action grid after Run Flow
                //Fixes Bug 695. Prevents Ginger from crashing. 
            }
        }

        private void ActionsGrid_MouseDoubleClick(object sender, EventArgs e)
        {
            AddAction();
        }

        /// <summary>
        /// Open window to user to select an action
        /// User will be able to Edit the action properties after clicking add
        /// Add the selected action to ActionsList
        /// </summary>
        /// <param name="ActionsList">Existing list which the new action will be added to it if user click add</param>
        /// <param name="windowStyle"></param>
        public void ShowAsWindow(ObservableList<IAct> ActionsList, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            mActionsList = ActionsList;

            Button addActionBtn = new Button();
            addActionBtn.Content = "Add Action";
            addActionBtn.Click += new RoutedEventHandler(AddActionButton_Click);

            GenericWindow.LoadGenericWindow(ref _pageGenericWin, null, windowStyle, this.Title, this, new ObservableList<Button> { addActionBtn });
        }

        private void ActionsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            ShowSelectedActionDetails();
        }

        private void ActionsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Tab_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowSelectedActionDetails();
        }

        private void ShowSelectedActionDetails()
        {
        }
    }
}
