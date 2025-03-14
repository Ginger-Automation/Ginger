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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.Actions.ActionConversion
{
    /// <summary>
    /// Interaction logic for ActionConverterPage.xaml
    /// </summary>
    public partial class ActionConverterPage : Page
    {
        private Solution mSolution;
        private BusinessFlow mBusinessFlow;
        private bool isGridSet = false;
        ObservableList<ConvertableActionDetails> lstActionToBeConverted = [];

        GenericWindow _pageGenericWin = null;

        public ActionConverterPage(BusinessFlow businessFlow)
        {
            InitializeComponent();
            mBusinessFlow = businessFlow;
            SetGridsView();

            if (mBusinessFlow.Activities.Any(x => x.SelectedForConversion == true))
            {
                mBusinessFlow.Activities.Where(x => x.SelectedForConversion == true).ToList().ForEach(x => { x.SelectedForConversion = false; });
            }
            grdGroups.DataSourceList = GingerCore.General.ConvertListToObservableList(businessFlow.Activities.Where(x => x.Active == true).ToList());
            WeakEventManager<ucGrid, EventArgs>.AddHandler(source: grdGroups, eventName: nameof(ucGrid.RowChangedEvent), handler: grdGroups_RowChangedEvent);
            grdGroups.Title = "Name of " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " in '" + mBusinessFlow.Name + "'";
            grdGroups.btnMarkAll.Visibility = Visibility.Visible;

            gridConvertibleActions.btnMarkAll.Visibility = Visibility.Visible;
            gridConvertibleActions.Visibility = Visibility.Collapsed;

            conversionConfigLblPanel.Visibility = Visibility.Collapsed;
            conversionConfigRadBtnPanel.Visibility = Visibility.Collapsed;
            conversionBtnPanel.Visibility = Visibility.Collapsed;

            btnConvert.Visibility = Visibility.Collapsed;

            cmbTargetApp.BindControl(mBusinessFlow.TargetApplications.Select(x => x.Name).ToList());
            if ((cmbTargetApp != null) && (cmbTargetApp.Items.Count > 0))
            {
                cmbTargetApp.SelectedIndex = 0;
            }

            grdGroups.MarkUnMarkAllActive += MarkUnMarkAllActivities;
            gridConvertibleActions.MarkUnMarkAllActive += MarkUnMarkAllActions;
        }
        private void MarkUnMarkAllActivities(bool ActiveStatus)
        {
            if (grdGroups.DataSourceList.Count <= 0)
            {
                return;
            }

            if (grdGroups.DataSourceList.Count > 0)
            {
                ObservableList<Activity> lstMarkUnMarkActivities = (ObservableList<Activity>)grdGroups.DataSourceList;
                foreach (Activity act in lstMarkUnMarkActivities)
                {
                    act.SelectedForConversion = ActiveStatus;
                }
                grdGroups.DataSourceList = lstMarkUnMarkActivities;
            }
        }
        private void MarkUnMarkAllActions(bool ActiveStatus)
        {
            if (gridConvertibleActions.DataSourceList.Count <= 0)
            {
                return;
            }

            if (gridConvertibleActions.DataSourceList.Count > 0)
            {
                ObservableList<ConvertableActionDetails> lstMarkUnMarkActions = (ObservableList<ConvertableActionDetails>)gridConvertibleActions.DataSourceList;
                foreach (ConvertableActionDetails act in lstMarkUnMarkActions)
                {
                    act.Selected = ActiveStatus;
                }
                gridConvertibleActions.DataSourceList = lstMarkUnMarkActions;
            }
        }
        public void Init(Solution Solution, BusinessFlow BusinessFlow)
        {
            mSolution = Solution;
            mBusinessFlow = BusinessFlow;
        }

        private void SetGridsView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(Activity.SelectedForConversion), WidthWeight = 2.5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox, Header = "Select" },
                new GridColView() { Field = nameof(Activity.ActivityName), WidthWeight = 15, Header = "Name of " + GingerDicser.GetTermResValue(eTermResKey.Activity) },
            ]
            };
            grdGroups.SetAllColumnsDefaultView(defView);
            grdGroups.InitViewItems();
            grdGroups.SetTitleLightStyle = true;
        }

        private void SetGridView()
        {
            if (isGridSet)
            {
                return;
            }
            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ConvertableActionDetails.Selected), Header = "Select", WidthWeight = 3.5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(ConvertableActionDetails.SourceActionTypeName), WidthWeight = 15, Header = "Source Action Type" },
                new GridColView() { Field = nameof(ConvertableActionDetails.Activities), WidthWeight = 15, Header = "Source " + GingerDicser.GetTermResValue(eTermResKey.Activities) },
                new GridColView() { Field = nameof(ConvertableActionDetails.TargetActionTypeName), WidthWeight = 15, Header = "Target Action Type" },
            ]
            };
            gridConvertibleActions.SetAllColumnsDefaultView(view);
            gridConvertibleActions.InitViewItems();
            gridConvertibleActions.SetTitleLightStyle = true;
        }

        private void grdGroups_RowChangedEvent(object sender, EventArgs e)
        {
            if (mBusinessFlow != null)
            {
                mBusinessFlow.CurrentActivity = (Activity)grdGroups.CurrentItem;
                if (mBusinessFlow.CurrentActivity != null)
                {
                    string allProperties = string.Empty;
                    PropertyChangedEventManager.AddHandler(source: mBusinessFlow.CurrentActivity, handler: CurrentActivity_PropertyChanged, propertyName: allProperties);
                }
            }
        }

        private void CurrentActivity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HandlerType")
            {
                grdGroups.setDefaultView();
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Actions Conversion", this);
        }

        private bool DoExistingPlatformCheck(ObservableList<ConvertableActionDetails> lstActionToBeConverted)
        {
            // fetch list of existing platforms in the business flow
            List<ePlatformType> lstExistingPlatform = mSolution.ApplicationPlatforms.Where(x => mBusinessFlow.TargetApplications
                                               .Any(a => a.Name == x.AppName)).Select(x => x.Platform).ToList();
            Dictionary<ePlatformType, string> lstMissingPlatform = [];
            // create list of missing platforms
            foreach (ConvertableActionDetails ACH in lstActionToBeConverted)
            {
                if (ACH.Selected && !lstExistingPlatform.Contains(ACH.TargetPlatform)
                    && !lstMissingPlatform.ContainsKey(ACH.TargetPlatform))
                {
                    lstMissingPlatform.Add(ACH.TargetPlatform, ACH.TargetActionTypeName);
                }
            }

            // if there are any missing platforms
            if (lstMissingPlatform.Count > 0)
            {
                foreach (var item in lstMissingPlatform)
                {
                    // ask the user if he wants to continue with the conversion, if there are missing target platforms
                    if (Reporter.ToUser(eUserMsgKey.MissingTargetPlatformForConversion, item.Value, item.Key) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void btnConvertAction_Click(object sender, RoutedEventArgs e)
        {
            // Check if no actions are selected to be converted
            if (lstActionToBeConverted.All(act => act.Selected == false))
            {
                Reporter.ToUser(eUserMsgKey.NoConvertibleActionSelected);
                return;
            }

            if (!DoExistingPlatformCheck(lstActionToBeConverted))
            {
                //missing target application so stop the conversion
                return;
            }
            else
            {
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                    // setting the conversion status label as visible
                    lblConversionStatus.Visibility = Visibility.Visible;
                    Reporter.ToStatus(eStatusMsgKey.BusinessFlowConversion, null, mBusinessFlow.Name);

                    // create a new converted activity
                    if ((bool)radNewActivity.IsChecked)
                    {
                        Activity newActivity = new Activity() { Active = true };
                        foreach (Activity oldActivity in mBusinessFlow.Activities.ToList())
                        {
                            // check if the activity is selected for conversion and it contains actions that are obsolete (of type, IObsolete)
                            if (oldActivity.SelectedForConversion && oldActivity.Acts.OfType<IObsoleteAction>().ToList().Count > 0)
                            {
                                newActivity = (Activity)oldActivity.CreateCopy(false);
                                newActivity.ActivityName = "New - " + oldActivity.ActivityName;
                                mBusinessFlow.Activities.Add(newActivity);
                                mBusinessFlow.Activities.Move(mBusinessFlow.Activities.Count - 1, mBusinessFlow.Activities.IndexOf(oldActivity) + 1);

                                foreach (Act oldAct in oldActivity.Acts.ToList())
                                {
                                    if (oldAct is IObsoleteAction && lstActionToBeConverted.FirstOrDefault(act => act.SourceActionType == oldAct.GetType() && act.Selected && act.TargetActionType == ((IObsoleteAction)oldAct).TargetAction()) != null)
                                    {
                                        // convert the old action
                                        Act newAct = ((IObsoleteAction)oldAct).GetNewAction();
                                        int oldActionIndex = newActivity.Acts.IndexOf(newActivity.Acts.FirstOrDefault(x => x.Guid == oldAct.Guid));
                                        newActivity.Acts.RemoveAt(oldActionIndex);
                                        newActivity.Acts.Add(newAct);
                                        newActivity.Acts.Move(newActivity.Acts.Count - 1, oldActionIndex);
                                    }
                                }

                                // check if the old activity was active or not and accordingly set Active field for the new activity
                                if (!oldActivity.Active)
                                {
                                    newActivity.Active = false;
                                }
                                else
                                {
                                    newActivity.Active = true;
                                }

                                // by default, set the old activity as inactive
                                oldActivity.Active = false;

                                // if the user has not chosen any target application in the combo box then, we set it as empty
                                if ((Boolean)chkDefaultTargetApp.IsChecked && cmbTargetApp.SelectedIndex != -1)
                                {
                                    newActivity.TargetApplication = cmbTargetApp.SelectedValue.ToString();
                                }
                                else
                                {
                                    newActivity.TargetApplication = string.Empty;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (Activity activity in mBusinessFlow.Activities)
                        {
                            if (activity.SelectedForConversion && activity.Acts.OfType<IObsoleteAction>().ToList().Count > 0)
                            {

                                foreach (Act act in activity.Acts.ToList())
                                {
                                    if (act.Active && act is IObsoleteAction && lstActionToBeConverted.FirstOrDefault(a => a.SourceActionType == act.GetType() && a.Selected && a.TargetActionType == ((IObsoleteAction)act).TargetAction()) != null)
                                    {
                                        // get the index of the action that is being converted 
                                        int selectedActIndex = activity.Acts.IndexOf(act);

                                        // convert the old action
                                        activity.Acts.Add(((IObsoleteAction)act).GetNewAction());

                                        if (selectedActIndex >= 0)
                                        {
                                            activity.Acts.Move(activity.Acts.Count - 1, selectedActIndex + 1);
                                        }

                                        // set obsolete action in the activity as inactive
                                        act.Active = false;
                                    }
                                }

                                // if the user has not chosen any target application in the combo box then, we set it as empty
                                if ((Boolean)chkDefaultTargetApp.IsChecked && cmbTargetApp.SelectedIndex != -1)
                                {
                                    activity.TargetApplication = cmbTargetApp.SelectedValue.ToString();
                                }
                                else
                                {
                                    activity.TargetApplication = string.Empty;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " - ", ex);
                    Reporter.ToUser(eUserMsgKey.ActivitiesConversionFailed);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
            lblConversionStatus.Visibility = Visibility.Hidden;
            Reporter.HideStatusMessage();

            // ask the user if he wants to convert more actions once the conversion is done successfully                       
            if (Reporter.ToUser(eUserMsgKey.SuccessfulConversionDone) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                _pageGenericWin.Close();
            }
        }

        private void btnShowConvertibleActionTypes_Click(object sender, RoutedEventArgs e)
        {
            // clearing the list of actions to be converted before clicking on Convertible Actions buttons again to reflect the fresh list of convertible actions
            lstActionToBeConverted.Clear();

            // fetching list of selected convertible activities from the first grid
            List<Activity> lstSelectedActivities = mBusinessFlow.Activities.Where(x => x.SelectedForConversion).ToList();

            if (lstSelectedActivities.Count != 0)
            {
                foreach (Activity convertibleActivity in lstSelectedActivities)
                {
                    foreach (Act act in convertibleActivity.Acts)
                    {
                        if ((act is IObsoleteAction) && (((IObsoleteAction)act).IsObsoleteForPlatform(act.Platform)) &&
                            (act.Active))
                        {
                            ConvertableActionDetails existingConvertibleActionType = lstActionToBeConverted.FirstOrDefault(x => x.SourceActionType == act.GetType() && x.TargetActionTypeName == ((IObsoleteAction)act).TargetActionTypeName());
                            if (existingConvertibleActionType == null)
                            {
                                ConvertableActionDetails newConvertibleActionType = new ConvertableActionDetails
                                {
                                    SourceActionTypeName = act.ActionDescription.ToString(),
                                    SourceActionType = act.GetType(),
                                    TargetActionType = ((IObsoleteAction)act).TargetAction()
                                };
                                if (newConvertibleActionType.TargetActionType == null)
                                {
                                    continue;
                                }

                                newConvertibleActionType.TargetActionTypeName = ((IObsoleteAction)act).TargetActionTypeName();
                                newConvertibleActionType.ActionCount = 1;
                                newConvertibleActionType.Actions.Add(act);
                                newConvertibleActionType.ActivityList.Add(convertibleActivity.ActivityName);
                                lstActionToBeConverted.Add(newConvertibleActionType);
                            }
                            else
                            {
                                if (!existingConvertibleActionType.Actions.Contains(act))
                                {
                                    existingConvertibleActionType.ActionCount++;
                                    existingConvertibleActionType.Actions.Add(act);
                                    existingConvertibleActionType.ActivityList.Add(convertibleActivity.ActivityName);
                                }
                            }
                        }
                    }
                }
                if (lstActionToBeConverted.Count != 0)
                {
                    gridConvertibleActions.DataSourceList = lstActionToBeConverted;
                    SetGridView();
                    gridConvertibleActions.Visibility = Visibility.Visible;
                    btnConvert.Visibility = Visibility.Visible;
                    conversionConfigLblPanel.Visibility = Visibility.Visible;
                    conversionConfigRadBtnPanel.Visibility = Visibility.Visible;
                    conversionBtnPanel.Visibility = Visibility.Visible;
                    return;
                }
                else
                {
                    gridConvertibleActions.Visibility = Visibility.Collapsed;
                    btnConvert.Visibility = Visibility.Collapsed;
                    conversionConfigLblPanel.Visibility = Visibility.Collapsed;
                    conversionConfigRadBtnPanel.Visibility = Visibility.Collapsed;
                    Reporter.ToUser(eUserMsgKey.NoConvertibleActionsFound);
                    return;
                }
            }
            else
            {
                gridConvertibleActions.Visibility = Visibility.Collapsed;
            }

            btnConvert.Visibility = Visibility.Collapsed;
            conversionConfigLblPanel.Visibility = Visibility.Collapsed;
            conversionConfigRadBtnPanel.Visibility = Visibility.Collapsed;
            Reporter.ToUser(eUserMsgKey.NoActivitySelectedForConversion);


        }

        private void chkDefaultTargetApp_Checked(object sender, RoutedEventArgs e)
        {
            if ((cmbTargetApp != null) && (cmbTargetApp.Items.Count > 0))
            {
                cmbTargetApp.IsEnabled = true;
                btnRefresh.IsEnabled = true;
            }
        }

        private void chkDefaultTargetApp_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((cmbTargetApp != null) && (cmbTargetApp.Items.Count > 0))
            {
                cmbTargetApp.IsEnabled = false;
                btnRefresh.IsEnabled = false;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            cmbTargetApp.BindControl(mBusinessFlow.TargetApplications.Select(x => x.Name).ToList());
            if ((cmbTargetApp != null) && (cmbTargetApp.Items.Count > 0))
            {
                cmbTargetApp.SelectedIndex = 0;
            }
        }
    }
}
