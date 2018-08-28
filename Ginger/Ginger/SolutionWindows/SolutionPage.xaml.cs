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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Environments;
using Ginger.TagsLib;
using Ginger.UserControls;
using Ginger.Variables;
using GingerCore;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for SolutionPage.xaml
    /// </summary>
    public partial class SolutionPage : Page
    {
        Solution mSolution;
        ucGrid ApplicationGrid;

        public SolutionPage(Solution s)
        {
            InitializeComponent();
            mSolution = s;
            Init();
        }

        private void Init()
        {
            App.ObjFieldBinding(SolutionNameTextBox, TextBox.TextProperty, mSolution, Solution.Fields.Name);
            App.ObjFieldBinding(SolutionFolderTextBox, TextBox.TextProperty, mSolution, Solution.Fields.Folder);
            App.ObjFieldBinding(AccountTextBox, TextBox.TextProperty, mSolution, Solution.Fields.Account);

            ApplicationGrid = new ucGrid();
            ApplicationGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddApplication));
            ApplicationGrid.Grid.PreparingCellForEdit += ApplicationGrid_PreparingCellForEdit;
            ApplicationGrid.Grid.CellEditEnding += ApplicationGrid_CellEditEnding;
            SetAppsGridView();
            ApplicationGrid.ShowTitle = System.Windows.Visibility.Collapsed;
            if (mSolution.ApplicationPlatforms == null)
            {
                mSolution.ApplicationPlatforms = new ObservableList<ApplicationPlatform>();
            }
            ApplicationGrid.DataSourceList = mSolution.ApplicationPlatforms;
            ApplicationsFrame.Content = ApplicationGrid;

            VariablesPage varbsPage = new VariablesPage(eVariablesLevel.Solution);
            varbsPage.grdVariables.ShowTitle = System.Windows.Visibility.Collapsed;
            VariablesFrame.Content = varbsPage;

            infoImage.ToolTip = "The first application in the list of Target Application(s) is consider to be the Solution Main Application."
                                + Environment.NewLine +
                                "Application: The local name of the application to be automated i.e.: CRM, select the name you use to call this app and how it is known to all people in the project"
                                + Environment.NewLine +
                                "Core: The core product on which this application is built on i.e: Amdocs CRM, this name is used to search packages in the global repository"
                                + Environment.NewLine +
                                "Core Version: The version of the core product i.e.: for CRM we have v8 or v9";


            SolutionTagsEditorPage p = new SolutionTagsEditorPage(mSolution.Tags);
            TagsFrame.Content = p;
        }

        private void AddApplication(object sender, RoutedEventArgs e)
        {
            AddApplicationPage AAP = new AddApplicationPage(App.UserProfile.Solution);
            AAP.ShowAsWindow();
        }

        private void SetAppsGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.AppName), Header = "Application", WidthWeight = 60 });

            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.Core), WidthWeight = 60 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.CoreVersion), WidthWeight = 20 });


            List<string> platformesTypesList = GingerCore.General.GetEnumValues(typeof(ePlatformType));
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.Platform), WidthWeight = 40, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = platformesTypesList });

            ApplicationGrid.SetAllColumnsDefaultView(view);
            ApplicationGrid.InitViewItems();
        }

        private void ApplicationGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.DisplayIndex == 0)//App Name Column
            {
                ApplicationPlatform currentApp = (ApplicationPlatform)ApplicationGrid.CurrentItem;
                currentApp.NameBeforeEdit = currentApp.AppName;
            }
        }

        private void ApplicationGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //Validate the name of the App is unique
            if (e.Column.DisplayIndex == 0)//App Name Column
            {
                ApplicationPlatform currentApp = (ApplicationPlatform)ApplicationGrid.CurrentItem;
                mSolution.SetUniqueApplicationName(currentApp);

                if (currentApp.AppName != currentApp.NameBeforeEdit)
                    UpdateApplicationNameChangeInSolution(currentApp);
            }
        }

        private void UpdateApplicationNameChangeInSolution(ApplicationPlatform app)
        {
            int numOfAfectedBFs = 0;
            if (Reporter.ToUser(eUserMsgKeys.UpdateApplicationNameChangeInSolution) == MessageBoxResult.No) return;


            foreach(BusinessFlow bf in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>())
            {
                //update the BF target applications
                foreach (TargetApplication bfApp in bf.TargetApplications)
                {
                    if (bfApp.AppName == app.NameBeforeEdit)
                    {
                        App.AddItemToSaveAll(bf);
                        bfApp.AppName = app.AppName;

                        //update the bf activities
                        foreach (Activity activity in bf.Activities)
                        {
                            if (activity.TargetApplication == app.NameBeforeEdit)
                                activity.TargetApplication = app.AppName;
                        }

                        numOfAfectedBFs++;
                        break;
                    }
                }
            }
            Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, string.Format("{0} {1} were updated successfully, please remember to Save All change.", numOfAfectedBFs, GingerDicser.GetTermResValue(eTermResKey.BusinessFlows)));
        }
    }
}
