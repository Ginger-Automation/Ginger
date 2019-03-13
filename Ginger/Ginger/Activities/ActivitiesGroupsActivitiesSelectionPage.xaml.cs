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
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Activities
{
    /// <summary>
    /// Interaction logic for ActivitiesSelectionPage.xaml
    /// </summary>
    public partial class ActivitiesGroupsActivitiesSelectionPage : Page
    {
        BusinessFlow mBusinessFlow;
        ActivitiesGroup mActivitiesGroup;

        GenericWindow _pageGenericWin = null;

        public ActivitiesGroupsActivitiesSelectionPage(BusinessFlow businessFlow, ActivitiesGroup activitiesGroup)
        {
            InitializeComponent();

            this.Title = GingerDicser.GetTermResValue(eTermResKey.Activities) + " Selection";

            mBusinessFlow = businessFlow;
            mActivitiesGroup = activitiesGroup;

            SetActivitiesGridView();
            SetActivitiesGridData();
        }

        private void SetActivitiesGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();

            defView.GridColsView.Add(new GridColView() { Field = Activity.Fields.ActivityName, Header = "Name", WidthWeight = 30, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = Activity.Fields.Description, Header = "Description", WidthWeight = 32.5, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = Activity.Fields.AutomationStatus, Header = "Auto. Status", WidthWeight = 32.5, ReadOnly = true });
            grdActivities.SetAllColumnsDefaultView(defView);
            grdActivities.InitViewItems();
        }

        private void SetActivitiesGridData()
        {
            ObservableList<Activity> freeActivities = new ObservableList<Activity>();
            foreach (Activity activ in mBusinessFlow.Activities)
                if (activ.ActivitiesGroupID == null || activ.ActivitiesGroupID == string.Empty)
                    freeActivities.Add(activ);
            grdActivities.DataSourceList = freeActivities;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button okBtn = new Button();
            okBtn.Content = "Add Selected";
            okBtn.Click += new RoutedEventHandler(okBtn_Click);

            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, true, "Cancel");
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (grdActivities.Grid.SelectedItems.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }

            //Add selected activities to group
            foreach (Activity act in grdActivities.Grid.SelectedItems)
            {             
                mActivitiesGroup.AddActivityToGroup(act);
            }

            _pageGenericWin.Close();
        }
    }
}
