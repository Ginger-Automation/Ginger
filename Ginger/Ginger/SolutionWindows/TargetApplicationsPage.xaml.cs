using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;


namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for TargetApplicationsPage.xaml
    /// </summary>
    public partial class TargetApplicationsPage : Page
    {
        Solution mSolution;

        public TargetApplicationsPage()
        {
            InitializeComponent();

            mSolution = App.UserProfile.Solution;
            App.UserProfile.PropertyChanged += UserProfile_PropertyChanged;

            LoadGridData();
            SetAppsGrid();
        }

        private void UserProfile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(UserProfile.Solution))
            {
                mSolution = App.UserProfile.Solution;
                LoadGridData();
            }
        }

        private void SetAppsGrid()
        {
            xTargetApplicationsGrid.SetGridEnhancedHeader(Amdocs.Ginger.Common.Enums.eImageType.Application, "Target Applications", saveAllHandler: SaveHandler, addHandler: AddApplication);
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.AppName), Header = "Name", WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.Description), Header = "Description", WidthWeight = 40 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.CoreVersion), Header = "Version", WidthWeight = 15 });
            List<string> platformesTypesList = GingerCore.General.GetEnumValues(typeof(ePlatformType));
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.Platform), WidthWeight = 15, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = platformesTypesList });
            xTargetApplicationsGrid.SetAllColumnsDefaultView(view);
            xTargetApplicationsGrid.InitViewItems();
            
            xTargetApplicationsGrid.Grid.PreparingCellForEdit += ApplicationGrid_PreparingCellForEdit;
            xTargetApplicationsGrid.Grid.CellEditEnding += ApplicationGrid_CellEditEnding;
        }

        private void LoadGridData()
        {
            if (mSolution != null)
            {
                if (mSolution.ApplicationPlatforms == null)
                {
                    mSolution.ApplicationPlatforms = new ObservableList<ApplicationPlatform>();
                }
                xTargetApplicationsGrid.DataSourceList = mSolution.ApplicationPlatforms;
            }
            else
                xTargetApplicationsGrid.DataSourceList = new ObservableList<ApplicationPlatform>();
        }

        private void SaveHandler(object sender, RoutedEventArgs e)
        {
            mSolution.SaveSolution(true, Solution.eSolutionItemToSave.TargetApplications);
        }

        private void AddApplication(object sender, RoutedEventArgs e)
        {
            AddApplicationPage AAP = new AddApplicationPage(App.UserProfile.Solution);
            AAP.ShowAsWindow();
        }

        private void ApplicationGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.DisplayIndex == 0)//App Name Column
            {
                ApplicationPlatform currentApp = (ApplicationPlatform)xTargetApplicationsGrid.CurrentItem;
                currentApp.NameBeforeEdit = currentApp.AppName;
            }
        }

        private void ApplicationGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //Validate the name of the App is unique
            if (e.Column.DisplayIndex == 0)//App Name Column
            {
                ApplicationPlatform currentApp = (ApplicationPlatform)xTargetApplicationsGrid.CurrentItem;
                mSolution.SetUniqueApplicationName(currentApp);

                if (currentApp.AppName != currentApp.NameBeforeEdit)
                    UpdateApplicationNameChangeInSolution(currentApp);
            }
        }

        private void UpdateApplicationNameChangeInSolution(ApplicationPlatform app)
        {
            int numOfAfectedBFs = 0;
            if (Reporter.ToUser(eUserMsgKeys.UpdateApplicationNameChangeInSolution) == MessageBoxResult.No) return;

            foreach (BusinessFlow bf in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>())
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
