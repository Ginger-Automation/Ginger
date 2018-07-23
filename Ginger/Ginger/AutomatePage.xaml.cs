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

using Ginger.Actions;
using Ginger.Activities;
using Ginger.BusinessFlowFolder;
using Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.Variables;
using GingerCore;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ginger
{
    public enum eAutomatePageViewStyles
    {
        Execution, 
        Design,
        Details
    }

    /// <summary>
    /// Interaction logic for AutomatePage.xaml
    /// </summary>
    public partial class AutomatePage : Page
    {                 
        BusinessFlowPage mCurrentBusPage;
        VariablesPage mVariablesPage;
        ActivitiesGroupsPage mActivitiesGroupsPage;
        ActivitiesPage mActivitiesPage;
        ActivitiesMiniViewPage mActivitiesMiniPage;        
        VariablesPage mActivityVariablesPage;
        public ActionsPage mActionsPage;
        RepositoryPage mReposiotryPage;

        GridLength mlastBusFlowsColWidth = new GridLength(225);
        GridLength mlastRepositoryColWidth = new GridLength(300);

        GridLength mlastBFVariablesRowHeight = new GridLength(200, GridUnitType.Star);
        GridLength mlastBFActivitiesGroupsRowHeight = new GridLength(200, GridUnitType.Star);
        GridLength mlastBFActivitiesRowHeight = new GridLength(300, GridUnitType.Star);
        GridLength mlastActivityVariablesRowHeight = new GridLength(200, GridUnitType.Star);
        GridLength mlastActivitiyActionsRowHeight = new GridLength(300, GridUnitType.Star);
        GridLength mMinRowsExpanderSize= new GridLength(35);
        GridLength mMinColsExpanderSize = new GridLength(35);

        public AutomatePage()
        {
            InitializeComponent();

            SetExpanders();
            //Bind between Menu expanders and actual grid expanders
            App.ObjFieldBinding(BFVariablesExpander, Expander.IsExpandedProperty, BFVariablesExpander2, "IsExpanded");
            App.ObjFieldBinding(BFActivitiesGroupsExpander, Expander.IsExpandedProperty, BFActivitiesGroupsExpander2, "IsExpanded");
            App.ObjFieldBinding(BFActivitiesExpander, Expander.IsExpandedProperty, BFActivitiesExpander2, "IsExpanded");
            App.ObjFieldBinding(ActivityVariablesExpander, Expander.IsExpandedProperty, ActivityVariablesExpander2, "IsExpanded");
            App.ObjFieldBinding(ActivityActionsExpander, Expander.IsExpandedProperty, ActivityActionsExpander2, "IsExpanded");
            BFVariablesExpander.IsExpanded = false;
            BFActivitiesGroupsExpander.IsExpanded = false;
            ActivityVariablesExpander.IsExpanded = false;
            mlastBFVariablesRowHeight = new GridLength(200, GridUnitType.Star);
            mlastActivityVariablesRowHeight = new GridLength(200, GridUnitType.Star);
            SetFramesContent();            
            LoadBizFlows();

            App.PropertyChanged += AppPropertychanged;
            App.UserProfile.PropertyChanged += UserProfilePropertyChanged;

            SetGridsView(eAutomatePageViewStyles.Design.ToString());
            SetGherkinOptions();
        }

        private void SetFramesContent()
        {
            mCurrentBusPage = new BusinessFlowPage(App.BusinessFlow, true);
            CurrentBusFrame.Content = mCurrentBusPage;
            CurrentBusExpander.IsExpanded = false;

            mVariablesPage = new VariablesPage(eVariablesLevel.BusinessFlow);
            mVariablesPage.grdVariables.ShowTitle = System.Windows.Visibility.Collapsed;
            BFVariablesFrame.Content = mVariablesPage;
            mActivitiesGroupsPage = new ActivitiesGroupsPage();
            mActivitiesGroupsPage.grdActivitiesGroups.ShowTitle = System.Windows.Visibility.Collapsed;
            BFActivitiesGroupsFrame.Content = mActivitiesGroupsPage;

            mActivitiesPage = new ActivitiesPage();
            mActivitiesPage.grdActivities.ShowTitle = System.Windows.Visibility.Collapsed;
            BFActivitiesFrame.Content = mActivitiesPage;

            mActivityVariablesPage = new VariablesPage(eVariablesLevel.Activity);
            mActivityVariablesPage.grdVariables.ShowTitle = System.Windows.Visibility.Collapsed;
            ActivityVariablesFrame.Content = mActivityVariablesPage;

            mActionsPage = new ActionsPage();
            mActionsPage.grdActions.ShowTitle = System.Windows.Visibility.Collapsed;
            ActivityActionsFrame.Content = mActionsPage;

            mReposiotryPage = new RepositoryPage();
            RepositoryFrame.Content = mReposiotryPage;
        }

        private void SetExpanders()
        {
            // We do UI changes using the dispatcher since it might trgigger from STA like IB
            App.MainWindow.Dispatcher.Invoke(() =>
                    {
                //set daynamic expanders titles
                if (App.BusinessFlow != null)
                {
                    App.BusinessFlow.PropertyChanged -= BusinessFlow_PropertyChanged; 
                    App.BusinessFlow.PropertyChanged += BusinessFlow_PropertyChanged;

                
                    CurrentBusExpanderLable.Content = "'" + App.BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);    
                
                    mCurrentBusPage = new BusinessFlowPage(App.BusinessFlow, true);
                    CurrentBusFrame.Content = mCurrentBusPage;

                    UpdateCurrentActiityExpander();
                }
                else
                    CurrentBusExpanderLable.Content = "No " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
            });
        }

        private void UpdateCurrentActiityExpander()
        {
            if (Dispatcher.CheckAccess())
            {
                if (App.BusinessFlow.CurrentActivity != null)
                {
                    ActivityVariablesExpanderLabel.Content = "'" + App.BusinessFlow.CurrentActivity.ActivityName + "' - " + GingerDicser.GetTermResValue(eTermResKey.Activity, suffixString: " ") + GingerDicser.GetTermResValue(eTermResKey.Variables);
                    ActivityActionsExpanderLabel.Content = "'" + App.BusinessFlow.CurrentActivity.ActivityName + "' - " + GingerDicser.GetTermResValue(eTermResKey.Activity, suffixString: " ") + "Actions";
                }
                else
                {
                    ActivityVariablesExpanderLabel.Content = GingerDicser.GetTermResValue(eTermResKey.Activity) + " - " + GingerDicser.GetTermResValue(eTermResKey.Activity, suffixString: " ") + GingerDicser.GetTermResValue(eTermResKey.Variables);
                    ActivityActionsExpanderLabel.Content = GingerDicser.GetTermResValue(eTermResKey.Activity) + " - " + GingerDicser.GetTermResValue(eTermResKey.Activity, suffixString: " ") + "Actions";
                }
            }
        }

        private void LoadBizFlows()
        {
            // When working with new SolutionRespository Create dedicated Recent BFs Folder tree item
            BusinessFlowsTreeView.TreeTitle = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows);
            BusinessFlowsTreeView.TreeTitleStyle = (Style)TryFindResource("@ucTitleStyle_2");

            BusinessFlowsFolderTreeItem BFTVIRecentlyUsed = new BusinessFlowsFolderTreeItem(eBusinessFlowsTreeViewMode.ReadOnly);
            BFTVIRecentlyUsed.Folder = "Recently Used";
            BusinessFlowsTreeView.Tree.ClearTreeItems();
            BusinessFlowsTreeView.Tree.AddItem(BFTVIRecentlyUsed);

            BusinessFlowsFolderTreeItem BFTVI = new BusinessFlowsFolderTreeItem(eBusinessFlowsTreeViewMode.ReadOnly);
            BFTVI.Folder = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString: "s");
            BFTVI.Path = App.UserProfile.Solution.BusinessFlowsMainFolder;
            BusinessFlowsTreeView.Tree.AddItem(BFTVI);

            BusinessFlowsTreeView.Tree.ItemSelected += BusinessFlowsTreeView_ItemSelected;
        }

        private void BusinessFlowsTreeView_ItemSelected(object sender, EventArgs e)
        {
            if (sender == null) return;

            TreeViewItem TVI = (TreeViewItem)sender;
            if (TVI == null) return;
            ITreeViewItem ITVI = (ITreeViewItem)TVI.Tag;
            if (ITVI.GetType() == typeof(BusinessFlowTreeItem))
            {
                App.MainWindow.AutomateBusinessFlow((BusinessFlow)ITVI.NodeObject());
            }
        }

        private void SetGherkinOptions()
        {
            if (App.BusinessFlow != null && App.BusinessFlow.Source == BusinessFlow.eSource.Gherkin)
            {
                App.MainWindow.Gherkin.Visibility = Visibility.Visible;
                App.MainWindow.GenerateScenario.Visibility = Visibility.Visible;
                App.MainWindow.CleanScenario.Visibility = Visibility.Visible;
                App.MainWindow.OpenFeatureFile.Visibility = Visibility.Visible;
            }
            else
            {
                App.MainWindow.Gherkin.Visibility = Visibility.Collapsed;
                App.MainWindow.GenerateScenario.Visibility = Visibility.Collapsed;
                App.MainWindow.CleanScenario.Visibility = Visibility.Collapsed;
                App.MainWindow.OpenFeatureFile.Visibility = Visibility.Collapsed;
            }
        }

        public void SetGridsView(string viewName)
        {
            if (mActionsPage != null)
                mActionsPage.grdActions.ChangeGridView(viewName.ToString());
            if (mActivitiesPage != null)
                mActivitiesPage.grdActivities.ChangeGridView(viewName.ToString());
        }

        private void UserProfilePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle Solution change
            //TODO: cleanup close current biz flow etc...
            if (e.PropertyName == "Solution")
            {
                if (App.UserProfile.Solution == null) return;
                LoadBizFlows();
                if (mReposiotryPage != null)
                    mReposiotryPage.RefreshCurrentRepo();
            }
        }

        private void CurrentBusExpander_Expanded(object sender, RoutedEventArgs e)
        {
            CurrentBusRow.Height = new GridLength(215, GridUnitType.Star);
        }

        private void CurrentBusExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            CurrentBusRow.Height = new GridLength(35);
        } 

        private void BusinessFlowsExpander_ExpandedCollapsed(object sender, RoutedEventArgs e)
        {
            if (BusinessFlowsExpander.IsExpanded)
            {
                BusinessFlowsExpanderLabel.Visibility = System.Windows.Visibility.Collapsed;
                BusinessFlowsColumn.Width = mlastBusFlowsColWidth;
            }
            else
            {
                mlastBusFlowsColWidth = BusinessFlowsColumn.Width;
                BusinessFlowsExpanderLabel.Visibility = System.Windows.Visibility.Visible;
                BusinessFlowsColumn.Width = mMinColsExpanderSize;
            }
        }

        private void BusinessFlowsExpander_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (BusinessFlowsExpander.IsExpanded == false && e.NewSize.Width > mMinColsExpanderSize.Value)
            {
                BusinessFlowsColumn.Width = mMinColsExpanderSize;
            }
        }

        private void BusinessFlowsTreeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (BusinessFlowsExpander.IsExpanded && e.NewSize.Width <= 50)
            {
                BusinessFlowsExpander.IsExpanded = false;
                mlastBusFlowsColWidth = new GridLength(225);
                BusinessFlowsColumn.Width = mMinColsExpanderSize;
            }
        }

        private void RepositoryExpander_ExpandedCollapsed(object sender, RoutedEventArgs e)
        {
            if (RepositoryExpander.IsExpanded)
            {
                RepositoryExpanderLabel.Visibility = System.Windows.Visibility.Collapsed;
                RepositoryGridColumn.Width = mlastRepositoryColWidth;
            }
            else
            {
                mlastRepositoryColWidth = RepositoryGridColumn.Width;
                RepositoryExpanderLabel.Visibility = System.Windows.Visibility.Visible;
                RepositoryGridColumn.Width = mMinColsExpanderSize;
            }
        }

        private void RepositoryExpander_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (RepositoryExpander.IsExpanded == false && e.NewSize.Width > mMinColsExpanderSize.Value)
            {
                RepositoryGridColumn.Width = mMinColsExpanderSize;
            }
        }

        private void RepositoryFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (RepositoryExpander.IsExpanded && e.NewSize.Width <= 50)
            {
                RepositoryExpander.IsExpanded = false;                
                mlastRepositoryColWidth = new GridLength(300);
                RepositoryGridColumn.Width = mMinColsExpanderSize;
            }
        }
      
        private void BFActivitiesFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height < 70)
            {
                if (mActivitiesMiniPage == null) mActivitiesMiniPage = new ActivitiesMiniViewPage();
                BFActivitiesFrame.Content = mActivitiesMiniPage;
            }
            else
            {
                BFActivitiesFrame.Content = mActivitiesPage;
            }
        }        

        private void Expanders_Changed(object sender, RoutedEventArgs e)
        {
            Expanders_Changed();
        }

        private void Expanders_Changed()
        {
            if (BFVariablesExpander == null || BFActivitiesExpander == null || ActivityVariablesExpander == null || ActivityActionsExpander == null) return;

            int rowIndex = 1;   //dynamic content starts from row # 2         
            if (BFVariablesExpander != null)
            {
                if (BFVariablesExpander.IsExpanded)
                {
                    if (BFVariablesFrame != null && BFVariablesFrame.ActualHeight != 0)
                        mlastBFVariablesRowHeight = new GridLength(BFVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    BFVariablesExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastBFVariablesRowHeight;
                    BFVariablesExpander.Visibility = System.Windows.Visibility.Visible;
                    BFVariablesExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (BFVariablesFrame != null && BFVariablesFrame.ActualHeight != 0)
                        mlastBFVariablesRowHeight = new GridLength(BFVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    BFVariablesExpander.Visibility = System.Windows.Visibility.Collapsed;
                    BFVariablesExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (BFActivitiesGroupsExpander != null)
            {
                if (BFActivitiesGroupsExpander.IsExpanded)
                {
                    if (BFActivitiesGroupsFrame != null && BFActivitiesGroupsFrame.ActualHeight != 0)
                        mlastBFActivitiesGroupsRowHeight = new GridLength(BFActivitiesGroupsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    BFActivitiesGroupsExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastBFActivitiesGroupsRowHeight;
                    BFActivitiesGroupsExpander.Visibility = System.Windows.Visibility.Visible;
                    BFActivitiesGroupsExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (BFActivitiesGroupsFrame != null && BFActivitiesGroupsFrame.ActualHeight != 0)
                        mlastBFActivitiesGroupsRowHeight = new GridLength(BFActivitiesGroupsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    BFActivitiesGroupsExpander.Visibility = System.Windows.Visibility.Collapsed;
                    BFActivitiesGroupsExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (BFActivitiesExpander != null)
            {
                if (BFActivitiesExpander.IsExpanded)
                {
                    if (BFActivitiesFrame != null && BFActivitiesFrame.ActualHeight != 0)
                        mlastBFActivitiesRowHeight = new GridLength(BFActivitiesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    BFActivitiesExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastBFActivitiesRowHeight;
                    BFActivitiesExpander.Visibility = System.Windows.Visibility.Visible;
                    BFActivitiesExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (BFActivitiesFrame != null && BFActivitiesFrame.ActualHeight != 0)
                        mlastBFActivitiesRowHeight = new GridLength(BFActivitiesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    BFActivitiesExpander.Visibility = System.Windows.Visibility.Collapsed;
                    BFActivitiesExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (ActivityVariablesExpander != null)
            {
                if (ActivityVariablesExpander.IsExpanded)
                {
                    if (ActivityVariablesFrame != null && ActivityVariablesFrame.ActualHeight != 0)
                        mlastActivityVariablesRowHeight = new GridLength(ActivityVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    ActivityVariablesExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastActivityVariablesRowHeight;
                    ActivityVariablesExpander.Visibility = System.Windows.Visibility.Visible;
                    ActivityVariablesExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (ActivityVariablesFrame != null && ActivityVariablesFrame.ActualHeight != 0)
                        mlastActivityVariablesRowHeight = new GridLength(ActivityVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    ActivityVariablesExpander.Visibility = System.Windows.Visibility.Collapsed;
                    ActivityVariablesExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (ActivityActionsExpander != null)
            {
                if (ActivityActionsExpander.IsExpanded)
                {
                    if (ActivityActionsFrame != null && ActivityActionsFrame.ActualHeight != 0)
                        mlastActivitiyActionsRowHeight = new GridLength(ActivityActionsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    ActivityActionsExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastActivitiyActionsRowHeight;
                    ActivityActionsExpander.Visibility = System.Windows.Visibility.Visible;
                    ActivityActionsExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (ActivityActionsFrame != null && ActivityActionsFrame.ActualHeight != 0)
                        mlastActivitiyActionsRowHeight = new GridLength(ActivityActionsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    ActivityActionsExpander.Visibility = System.Windows.Visibility.Collapsed;
                    ActivityActionsExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            //hide all unneeded rows
            for(int indx=6;indx>rowIndex;indx--)
            {
                PageMainGrid.RowDefinitions[indx].MinHeight = 0;
                PageMainGrid.RowDefinitions[indx].Height = new GridLength(0);
            }

            //Disable all unneeded splitters
            List<GridSplitter> gridSplitters = new List<GridSplitter>();
            gridSplitters.Add(Row1Splitter);
            gridSplitters.Add(Row2Splitter);
            gridSplitters.Add(Row3Splitter);
            gridSplitters.Add(Row4Splitter);
            gridSplitters.Add(Row5Splitter);
            for (int indx = 1; indx <= gridSplitters.Count; indx++)
            {
                if (gridSplitters[indx - 1] != null)
                {
                    if (indx < (rowIndex-1))
                        gridSplitters[indx - 1].IsEnabled = true;
                    else
                        gridSplitters[indx - 1].IsEnabled = false;
                }
            }

            //arange expanders menu look 
            if (rowIndex == 6)
                PageMainGrid.RowDefinitions[1].Height = new GridLength(0);
            else
                PageMainGrid.RowDefinitions[1].Height = new GridLength(35);

            //make sure at least one expander open
            if (rowIndex == 1)
                BFActivitiesExpander.IsExpanded = true;            
        }

        private void BusinessFlow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.PropertyName == "CurrentActivity")
                {
                    UpdateCurrentActiityExpander();
                }
                else if(e.PropertyName == BusinessFlow.Fields.Name)
                {
                    CurrentBusExpanderLable.Content = "'" + App.BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                }
            });
        }

        private void AppPropertychanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BusinessFlow")
            {
                SetExpanders();
                SetGherkinOptions();
            }
        }
    }
}
