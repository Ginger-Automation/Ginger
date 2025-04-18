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
using Ginger.Repository;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Activities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Ginger.Activities
{
    /// <summary>
    /// Interaction logic for ActivitiesGroupPage.xaml
    /// </summary>
    public partial class ActivitiesGroupPage : GingerUIPage
    {
        ActivitiesGroup mActivitiesGroup;
        BusinessFlow mBusinessFlow = null;
        GenericWindow _pageGenericWin = null;
        public bool OKButtonClicked = false;

        public enum eEditMode
        {
            ExecutionFlow = 0,
            SharedRepository = 1
        }

        public eEditMode mEditMode;

        public ActivitiesGroupPage(ActivitiesGroup activitiesGroup, BusinessFlow parentBusinessFlow = null, eEditMode mode = eEditMode.ExecutionFlow, Context mContext = null)
        {
            InitializeComponent();
            mEditMode = mode;
            mActivitiesGroup = activitiesGroup;
            mActivitiesGroup.SaveBackup();
            mBusinessFlow = parentBusinessFlow;

            xShowIDUC.Init(mActivitiesGroup);
            xAGExternalId.Init(mContext, mActivitiesGroup, nameof(ActivitiesGroup.ExternalID));
            if (WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures)
            {
                xAGExternalId.Init(mContext, mActivitiesGroup, nameof(ActivitiesGroup.ExternalID));
            }
            else
            {
                xPnlAGExternalId.Visibility = Visibility.Collapsed;
            }
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtGroupName, TextBox.TextProperty, mActivitiesGroup, nameof(ActivitiesGroup.Name));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtGroupDescription, TextBox.TextProperty, mActivitiesGroup, nameof(ActivitiesGroup.Description));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtAutoPrecentage, TextBox.TextProperty, mActivitiesGroup, nameof(ActivitiesGroup.AutomationPrecentage), BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xPublishcheckbox, CheckBox.IsCheckedProperty, mActivitiesGroup, nameof(RepositoryItemBase.Publish));
            SetGroupedActivitiesGridView();
            grdGroupedActivities.DataSourceList = mActivitiesGroup.ActivitiesIdentifiers;
            grdGroupedActivities.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGroupedActivities));
            AttachActivitiesGroupAndRepositoryActivities();
            RefreshGroupedActivities();

            TagsViewer.Init(mActivitiesGroup.Tags);

            if (mEditMode == eEditMode.ExecutionFlow)
            {
                SharedRepoInstanceUC.Init(mActivitiesGroup, mBusinessFlow);
            }
            else
            {
                SharedRepoInstanceUC.Visibility = Visibility.Collapsed;
                SharedRepoInstanceUC_Col.Width = new GridLength(0);
            }
        }

        void UpdateSharedRepositorySupportedOperations()
        {
            grdGroupedActivities.ShowUpDown = Visibility.Visible;
            grdGroupedActivities.ShowDelete = Visibility.Visible;

            grdGroupedActivities.AddToolbarTool(Amdocs.Ginger.Common.Enums.eImageType.Add, "Add Another " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " to this Group", BtnAdd_Click);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ActivitiesRepositoryPage ActivitiesRepoPage = new ActivitiesRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>(), null, null, GroupActivitiesHandler, ActivitiesRepositoryPage.ePageViewMode.Selection);
            ActivitiesRepoPage.ShowAsWindow(Window.GetWindow(this));
        }

        private void GroupActivitiesHandler(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                ObservableList<Amdocs.Ginger.Repository.RepositoryItemBase> selectedActivitiesList = null;
                if (sender is ucGrid)
                {
                    ucGrid senderGrid = sender as ucGrid;
                    selectedActivitiesList = senderGrid.GetSelectedItems();
                }
                else if (sender is UcListView)
                {
                    UcListView senderLst = sender as UcListView;
                    selectedActivitiesList = senderLst.GetSelectedItems();
                }

                if (selectedActivitiesList != null && selectedActivitiesList.Count > 0)
                {
                    foreach (Activity sharedActivity in selectedActivitiesList)
                    {
                        Activity newInstance = sharedActivity.CreateInstance(true) as Activity;
                        mActivitiesGroup.AddActivityToGroup(newInstance);
                    }
                }
            }
        }

        private void SetGroupedActivitiesGridView()
        {
            grdGroupedActivities.SetTitleLightStyle = true;

            GridViewDef defView2 = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ActivityIdentifiers.ActivityName), Header = "Name", WidthWeight = 30, ReadOnly = true },
                new GridColView() { Field = nameof(ActivityIdentifiers.ActivityDescription), Header = "Description", WidthWeight = 30, ReadOnly = true },
                new GridColView() { Field = nameof(ActivityIdentifiers.ActivityAutomationStatus), Header = "Auto. Status", WidthWeight = 20, ReadOnly = true },
            ]
            };
            if (mEditMode == eEditMode.SharedRepository)
            {
                defView2.GridColsView.Add(new GridColView() { Field = nameof(ActivityIdentifiers.ExistInRepository), Header = "Exist In Repository", WidthWeight = 20, ReadOnly = true });
            }

            grdGroupedActivities.SetAllColumnsDefaultView(defView2);
            grdGroupedActivities.InitViewItems();
        }

        public void AttachActivitiesGroupAndRepositoryActivities()
        {
            ObservableList<Activity> activitiesRepository = [];

            if (mEditMode == eEditMode.ExecutionFlow)
            {
                activitiesRepository = mBusinessFlow.Activities;
            }
            else if (mEditMode == eEditMode.SharedRepository)
            {
                activitiesRepository = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            }

            foreach (ActivityIdentifiers actIdent in mActivitiesGroup.ActivitiesIdentifiers)
            {
                Activity repoAct = activitiesRepository.FirstOrDefault(x => x.ActivityName == actIdent.ActivityName && x.Guid == actIdent.ActivityGuid);
                if (repoAct == null)
                {
                    repoAct = activitiesRepository.FirstOrDefault(x => x.Guid == actIdent.ActivityGuid);
                }

                if (repoAct == null)
                {
                    repoAct = activitiesRepository.FirstOrDefault(x => x.ActivityName == actIdent.ActivityName);
                }

                if (repoAct != null)
                {
                    actIdent.IdentifiedActivity = repoAct;
                    if (mEditMode == eEditMode.SharedRepository)
                    {
                        actIdent.ExistInRepository = true;
                    }
                    else
                    {
                        actIdent.ExistInRepository = false;
                    }
                }
                else
                {
                    actIdent.ExistInRepository = false;
                }
            }
        }

        private void RefreshGroupedActivities()
        {
            AttachActivitiesGroupAndRepositoryActivities();
            mActivitiesGroup.OnPropertyChanged(nameof(ActivitiesGroup.AutomationPrecentage));
        }

        private void RefreshGroupedActivities(object sender, RoutedEventArgs e)
        {
            RefreshGroupedActivities();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            string title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup);

            ObservableList<Button> winButtons = [];
            switch (mEditMode)
            {
                case ActivitiesGroupPage.eEditMode.ExecutionFlow:
                    Button okBtn = new Button
                    {
                        Content = "Ok"
                    };
                    WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: okBtn, eventName: nameof(ButtonBase.Click), handler: okBtn_Click);


                    winButtons.Add(okBtn);
                    break;

                case ActivitiesGroupPage.eEditMode.SharedRepository:
                    title = "Edit Shared Repository " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup);
                    Button saveBtn = new Button
                    {
                        Content = "Save"
                    };
                    WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: saveBtn, eventName: nameof(ButtonBase.Click), handler: saveBtn_Click);



                    winButtons.Add(saveBtn);

                    UpdateSharedRepositorySupportedOperations();

                    break;
            }

            Button undoBtn = new Button
            {
                Content = "Undo & Close"
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: undoBtn, eventName: nameof(ButtonBase.Click), handler: undoBtn_Click);



            winButtons.Add(undoBtn);

            this.Height = 800;
            this.Width = 800;
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, "Undo & Close", CloseWinClicked, startupLocationWithOffset: startupLocationWithOffset);
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
            if (Reporter.ToUser(eUserMsgKey.AskIfToUndoChanges) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                UndoChangesAndClose();
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

        protected override void IsVisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (mEditMode == eEditMode.SharedRepository && mActivitiesGroup != null && !String.IsNullOrEmpty(mActivitiesGroup.ContainingFolder))
            {
                CurrentItemToSave = mActivitiesGroup;
                base.IsVisibleChangedHandler(sender, e);
            }
        }
    }
}
