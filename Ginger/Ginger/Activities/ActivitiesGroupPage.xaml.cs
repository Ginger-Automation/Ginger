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

using Amdocs.Ginger.Common;
using Ginger.Repository;
using Ginger.TagsLib;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Ginger.BusinessFlowWindows;
using Ginger.BusinessFlowFolder;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.Activities
{
    /// <summary>
    /// Interaction logic for ActivitiesGroupPage.xaml
    /// </summary>
    public partial class ActivitiesGroupPage : Page
    {
        ActivitiesGroup mActivitiesGroup;
        GenericWindow _pageGenericWin = null;
        public bool OKButtonClicked = false;

        public enum eEditMode
        {
            ExecutionFlow = 0,
            SharedRepository = 1
        }

        public eEditMode mEditMode;

        public ActivitiesGroupPage(ActivitiesGroup activitiesGroup, eEditMode mode = eEditMode.ExecutionFlow)
        {
            InitializeComponent();
            mEditMode = mode;
            mActivitiesGroup = activitiesGroup;            
                mActivitiesGroup.SaveBackup();

            App.ObjFieldBinding(txtGroupName, TextBox.TextProperty, mActivitiesGroup, ActivitiesGroup.Fields.Name);
            App.ObjFieldBinding(txtGroupDescription, TextBox.TextProperty, mActivitiesGroup, ActivitiesGroup.Fields.Description);
            App.ObjFieldBinding(txtAutoPrecentage, TextBox.TextProperty, mActivitiesGroup, ActivitiesGroup.Fields.AutomationPrecentage, BindingMode.OneWay);

            SetGroupedActivitiesGridView();
            grdGroupedActivities.DataSourceList = mActivitiesGroup.ActivitiesIdentifiers;
            grdGroupedActivities.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGroupedActivities));
            AttachActivitiesGroupAndRepositoryActivities();
            RefreshGroupedActivities();

            TagsViewer.Init(mActivitiesGroup.Tags);

            if (mEditMode == eEditMode.ExecutionFlow)
                SharedRepoInstanceUC.Init(mActivitiesGroup, App.BusinessFlow);
            else
            {
                SharedRepoInstanceUC.Visibility = Visibility.Collapsed;
                SharedRepoInstanceUC_Col.Width = new GridLength(0);
            }
        }       

        private void SetGroupedActivitiesGridView()
        {
            grdGroupedActivities.SetTitleLightStyle = true;

            GridViewDef defView2 = new GridViewDef(GridViewDef.DefaultViewName);
            defView2.GridColsView = new ObservableList<GridColView>();

            defView2.GridColsView.Add(new GridColView() { Field = ActivityIdentifiers.Fields.ActivityName, Header = "Name", WidthWeight = 30, ReadOnly = true });
            defView2.GridColsView.Add(new GridColView() { Field = ActivityIdentifiers.Fields.ActivityDescription, Header = "Description", WidthWeight = 30, ReadOnly = true });
            defView2.GridColsView.Add(new GridColView() { Field = ActivityIdentifiers.Fields.ActivityAutomationStatus, Header = "Auto. Status", WidthWeight = 20, ReadOnly = true });
            if (mEditMode == eEditMode.SharedRepository)
                defView2.GridColsView.Add(new GridColView() { Field = ActivityIdentifiers.Fields.ExistInRepository, Header = "Exist In Repository", WidthWeight = 20, ReadOnly = true });
            grdGroupedActivities.SetAllColumnsDefaultView(defView2);
            grdGroupedActivities.InitViewItems();
        }

        public void AttachActivitiesGroupAndRepositoryActivities()
        {
            ObservableList<Activity> activitiesRepository = new ObservableList<Activity>();

            if (mEditMode == eEditMode.ExecutionFlow)
                activitiesRepository = App.BusinessFlow.Activities;
            else if (mEditMode == eEditMode.SharedRepository)
                activitiesRepository = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();

            foreach (ActivityIdentifiers actIdent in mActivitiesGroup.ActivitiesIdentifiers)
            {
                Activity repoAct = activitiesRepository.Where(x => x.ActivityName == actIdent.ActivityName && x.Guid == actIdent.ActivityGuid).FirstOrDefault();
                if (repoAct == null)
                    repoAct = activitiesRepository.Where(x => x.Guid == actIdent.ActivityGuid).FirstOrDefault();
                if (repoAct == null)
                    repoAct = activitiesRepository.Where(x => x.ActivityName == actIdent.ActivityName).FirstOrDefault();
                if (repoAct != null)
                {
                    actIdent.IdentifiedActivity = repoAct;
                    if (mEditMode == eEditMode.SharedRepository)
                        actIdent.ExistInRepository = true;
                    else
                        actIdent.ExistInRepository = false;
                }
                else
                    actIdent.ExistInRepository = false;
            }
        }

        private void RefreshGroupedActivities()
        {
            AttachActivitiesGroupAndRepositoryActivities();
            mActivitiesGroup.OnPropertyChanged(ActivitiesGroup.Fields.AutomationPrecentage);
        }

        private void RefreshGroupedActivities(object sender, RoutedEventArgs e)
        {
            RefreshGroupedActivities();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            string title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup);

            ObservableList<Button> winButtons = new ObservableList<Button>();
            switch (mEditMode)
            {
                case ActivitiesGroupPage.eEditMode.ExecutionFlow:
               
                    Button okBtn = new Button();
                    okBtn.Content = "Ok";
                    okBtn.Click += new RoutedEventHandler(okBtn_Click);
                    winButtons.Add(okBtn);
                    break;

                case ActivitiesGroupPage.eEditMode.SharedRepository:
                    title = "Edit Shared Repository " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup);
                    Button saveBtn = new Button();
                    saveBtn.Content = "Save";
                    saveBtn.Click += new RoutedEventHandler(saveBtn_Click);
                    winButtons.Add(saveBtn);
                    break;
            }

            Button undoBtn = new Button();
            undoBtn.Content = "Undo & Close";
            undoBtn.Click += new RoutedEventHandler(undoBtn_Click);
            winButtons.Add(undoBtn);

            this.Height = 800;
            this.Width = 800;
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, string.Empty, CloseWinClicked, startupLocationWithOffset: startupLocationWithOffset);
        }

        private void UndoChangesAndClose()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            mActivitiesGroup.RestoreFromBackup(true);
            Mouse.OverrideCursor = null;

            _pageGenericWin.Close();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            OKButtonClicked = true;
            _pageGenericWin.Close();
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckIfUserWantToSave();
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.ToSaveChanges) == MessageBoxResult.No)
            {
                UndoChangesAndClose();
            }
            else
            {
                if (mEditMode == eEditMode.SharedRepository)
                    CheckIfUserWantToSave();
                else
                    _pageGenericWin.Close();
            }
        }

        private void CheckIfUserWantToSave()
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mActivitiesGroup, "change") == true)
            {                
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mActivitiesGroup);
                _pageGenericWin.Close();
            }
        }
    }
}
