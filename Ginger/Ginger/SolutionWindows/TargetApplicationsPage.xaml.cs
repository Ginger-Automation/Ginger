#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for TargetApplicationsPage.xaml
    /// </summary>
    public partial class TargetApplicationsPage : GingerUIPage
    {
        Solution mSolution;
        string AppName;

        public delegate void OnActivityTargetApplicationUpdate();

        public static event OnActivityTargetApplicationUpdate OnActivityUpdate;

        public TargetApplicationsPage()
        {
            InitializeComponent();

            mSolution = WorkSpace.Instance.Solution;
            string allProperties = string.Empty;
            WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;
            CurrentItemToSave = mSolution;

            LoadGridData();
            SetAppsGrid();
        }

        private void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                mSolution = WorkSpace.Instance.Solution;
                LoadGridData();
            }
        }

        private void SetAppsGrid()
        {
            xTargetApplicationsGrid.SetGridEnhancedHeader(Amdocs.Ginger.Common.Enums.eImageType.Application, $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}s", saveAllHandler: SaveHandler, addHandler: AddApplication, true);
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ApplicationPlatform.PlatformImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16, Style = FindResource("@DataGridColumn_Image") as Style },
                new GridColView() { Field = nameof(ApplicationPlatform.AppName), Header = "Name", WidthWeight = 30 },
                new GridColView() { Field = nameof(ApplicationPlatform.Description), Header = "Description", WidthWeight = 40 },
                // show description (friendly text) instead of raw enum value
                new GridColView() { Field = nameof(ApplicationPlatform.PlatformDescription), Header="Platform", WidthWeight = 15, ReadOnly = true },
                new GridColView() { Field = nameof(ApplicationPlatform.Guid), Header = "ID", WidthWeight = 15, ReadOnly = true },
    ]
            };

            xTargetApplicationsGrid.SetAllColumnsDefaultView(view);
            xTargetApplicationsGrid.InitViewItems();

            WeakEventManager<DataGrid, DataGridPreparingCellForEditEventArgs>.AddHandler(source: xTargetApplicationsGrid.Grid, eventName: nameof(DataGrid.PreparingCellForEdit), handler: ApplicationGrid_PreparingCellForEdit);
            WeakEventManager<DataGrid, DataGridCellEditEndingEventArgs>.AddHandler(source: xTargetApplicationsGrid.Grid, eventName: nameof(DataGrid.CellEditEnding), handler: ApplicationGrid_CellEditEnding);

            xTargetApplicationsGrid.AddToolbarTool(Amdocs.Ginger.Common.Enums.eImageType.ID, "Copy selected item ID", CopySelectedItemID);

            xTargetApplicationsGrid.SetbtnDeleteHandler(btnDelete_Click);
            xTargetApplicationsGrid.SetbtnClearAllHandler(btnClearAll_Click);
        }
        public bool NameAlreadyExists(string value)
        {
            if (WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(obj => obj.AppName == value) == null)
            {
                return false; //no name like it in the group 
            }
            List<ApplicationPlatform> sameNameObjList = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(obj => obj.AppName == value).ToList<ApplicationPlatform>();
            if (sameNameObjList.Count == 1 && sameNameObjList[0].AppName == value)
            {
                return false; //Same internal object 
            }
            return true;
        }
        private void LoadGridData()
        {
            if (mSolution != null)
            {
                if (mSolution.ApplicationPlatforms == null)
                {
                    mSolution.ApplicationPlatforms = [];
                }
                xTargetApplicationsGrid.DataSourceList = mSolution.ApplicationPlatforms;
            }
            else
            {
                xTargetApplicationsGrid.DataSourceList = new ObservableList<ApplicationPlatform>();
            }

        }

        private void CopySelectedItemID(object sender, RoutedEventArgs e)
        {
            if (xTargetApplicationsGrid.Grid.SelectedItem != null)
            {
                GingerCore.General.SetClipboardText(((RepositoryItemBase)xTargetApplicationsGrid.Grid.SelectedItem).Guid.ToString());
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void SaveHandler(object sender, RoutedEventArgs e)
        {
            mSolution.SolutionOperations.SaveSolution(true, Solution.eSolutionItemToSave.TargetApplications);
        }

        private void AddApplication(object sender, RoutedEventArgs e)
        {
            AddApplicationPage AAP = new AddApplicationPage(WorkSpace.Instance.Solution, false);
            AAP.ShowAsWindow();
        }

        private void ApplicationGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.DisplayIndex == 1)//App Name Column 
            {
                ApplicationPlatform currentApp = (ApplicationPlatform)xTargetApplicationsGrid.CurrentItem;
                currentApp.NameBeforeEdit = currentApp.AppName;
                AppName = currentApp.NameBeforeEdit;
            }
        }
        private ObservableList<ApplicationPlatform> Applications;
        private void ApplicationGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //Validate the name of the App is unique 
            if (e.Column.DisplayIndex == 1)//App Name Column 
            {
                ApplicationPlatform currentApp = (ApplicationPlatform)xTargetApplicationsGrid.CurrentItem;
                currentApp.NameBeforeEdit = AppName;
                mSolution.SetUniqueApplicationName(currentApp);

                if (currentApp.AppName != currentApp.NameBeforeEdit)
                {
                    if (string.IsNullOrEmpty(currentApp.AppName.ToString()) || string.IsNullOrWhiteSpace(currentApp.AppName.ToString()))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Name Cannot be empty");
                        return;
                    }
                    if (NameAlreadyExists(currentApp.AppName))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} with same name already exists");
                        return;
                    }
                    UpdateApplicationNameChangeInSolution(currentApp);
                }
            }
        }

        private void UpdateApplicationNameChangeInSolution(ApplicationPlatform app)
        {
            int numOfAfectedItems = 0;
            bool startDirtyTrackOfBF = false;
            if (Reporter.ToUser(eUserMsgKey.UpdateApplicationNameChangeInSolution) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                return;
            }

            foreach (BusinessFlow bf in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>())
            {
                //update the BF target applications 
                startDirtyTrackOfBF = false;
                foreach (var activity in bf.Activities)
                {
                    //donot check for TargetPlugins, only for TargetApplications 

                    if (string.Equals(activity.TargetApplication, app.NameBeforeEdit))
                    {
                        activity.StartDirtyTracking();
                        activity.TargetApplication = app.AppName;

                        numOfAfectedItems++;
                    }

                    // For changing the name of DB type
                    foreach (ActDBValidation dbAction in activity.Acts.OfType<ActDBValidation>())
                    {
                        if (string.Equals(dbAction.AppName, app.NameBeforeEdit))
                        {
                            dbAction.StartDirtyTracking();
                            dbAction.AppName = app.AppName;
                            numOfAfectedItems++;
                        }  
                    }
                }

                foreach (TargetApplication bfTargetApp in bf.TargetApplications.Where((targetApp) => targetApp is TargetApplication))
                {
                    if (string.Equals(bfTargetApp.AppName, app.NameBeforeEdit))
                    {
                        bfTargetApp.StartDirtyTracking();
                        bfTargetApp.AppName = app.AppName;
                        startDirtyTrackOfBF = true;
                    }
                }
                if (startDirtyTrackOfBF)
                {
                    bf.StartDirtyTracking();
                }
            }


            foreach (Activity activity in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>())
            {
                //update the shared repository activities 
                if (activity.TargetApplication == app.NameBeforeEdit)
                {
                    activity.StartDirtyTracking();
                    activity.TargetApplication = app.AppName;
                    numOfAfectedItems++;
                }
                // For changing the name of DB type
                foreach (ActDBValidation dbAction in activity.Acts.OfType<ActDBValidation>())
                {
                    if (string.Equals(dbAction.AppName, app.NameBeforeEdit))
                    {
                        dbAction.StartDirtyTracking();
                        dbAction.AppName = app.AppName;
                        numOfAfectedItems++;
                    }
                }
            }

            foreach (ApplicationAPIModel applicationAPIModel in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>())
            {
                //update the shared repository activities 
                if (applicationAPIModel.TargetApplicationKey?.ItemName == app.NameBeforeEdit)
                {
                    applicationAPIModel.StartDirtyTracking();
                    applicationAPIModel.TargetApplicationKey.ItemName = app.AppName;
                    numOfAfectedItems++;
                }
            }

            foreach (ApplicationPOMModel applicationPOMModel in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>())
            {
                //update the shared repository activities 
                if (applicationPOMModel.TargetApplicationKey?.ItemName == app.NameBeforeEdit)
                {
                    applicationPOMModel.StartDirtyTracking();
                    applicationPOMModel.TargetApplicationKey.ItemName = app.AppName;
                    numOfAfectedItems++;
                }
            }

            foreach (ProjEnvironment projEnv in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>())
            {
                foreach (EnvApplication envApplication in projEnv.Applications)
                {

                    if (string.Equals(envApplication.Name, app.NameBeforeEdit))
                    {
                        projEnv.StartDirtyTracking();
                        envApplication.Name = app.AppName;
                        numOfAfectedItems++;
                    }
                }
            }

            if (numOfAfectedItems > 0 && OnActivityUpdate != null)
            {
                OnActivityUpdate();
            }
            Reporter.ToUser(eUserMsgKey.StaticInfoMessage, string.Format("{0} items were updated successfully, please remember to Save All change.", numOfAfectedItems));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (xTargetApplicationsGrid.grdMain.SelectedItems.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.SelectItemToDelete);
                return;
            }

            DeleteApplication((ApplicationPlatform)xTargetApplicationsGrid.grdMain.SelectedItem);
        }

        public void DeleteApplication(ApplicationPlatform applicationPlatform)
        {
            if (applicationPlatform == null)
            {
                return;
            }

            bool doesAppExistInBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Any(x => x.TargetApplications.Any(y => y.Name == applicationPlatform.AppName));
            bool doesAppExistInEnv = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().SelectMany((projEnv) => projEnv.Applications).Any((x) => string.Equals(x.Name, applicationPlatform.AppName));
            if (doesAppExistInBF || doesAppExistInEnv)
            {
                string messageToUser = $"Cannot remove {applicationPlatform.AppName} as it is being used by ";

                if (doesAppExistInBF)
                {
                    messageToUser += "Businessflows";
                }
                if (doesAppExistInEnv)
                {
                    if (messageToUser.EndsWith("Businessflows"))
                    {
                        messageToUser += " and Environments";
                    }

                    else
                    {
                        messageToUser += "Environments";
                    }
                }

                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, messageToUser);
            }
            else
            {
                WorkSpace.Instance.Solution.ApplicationPlatforms.Remove(applicationPlatform);
            }
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (ApplicationPlatform applicationPlatform in WorkSpace.Instance.Solution.ApplicationPlatforms.ToList())
            {
                DeleteApplication(applicationPlatform);
            }
        }
    }
}
