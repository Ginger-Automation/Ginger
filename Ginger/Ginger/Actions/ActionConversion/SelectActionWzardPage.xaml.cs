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
using Amdocs.Ginger.CoreNET;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Environments;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using static Ginger.ExtensionMethods;

namespace Ginger.Actions.ActionConversion
{
    /// <summary>
    /// Interaction logic for SelectActionWzardPage.xaml
    /// </summary>
    public partial class SelectActionWzardPage : Page, IWizardPage
    {
        ActionsConversionWizard mWizard;
        
        public SelectActionWzardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (ActionsConversionWizard)WizardEventArgs.Wizard;
                    break;
                case EventType.Active:
                    Init();
                    break;
            }
        }
        
        private void Init()
        {
            // clearing the list of actions to be converted before clicking on Convertible Actions buttons again to reflect the fresh list of convertible actions
            mWizard.ActionToBeConverted.Clear();

            // fetching list of selected convertible activities from the first grid
            List<Activity> lstSelectedActivities = mWizard.Context.BusinessFlow.Activities.Where(x => x.SelectedForConversion).ToList();
            if (lstSelectedActivities.Count != 0)
            {
                ActionConversionUtils utils = new ActionConversionUtils();
                mWizard.ActionToBeConverted = utils.GetConvertableActivityActions(lstSelectedActivities);
                if (mWizard.ActionToBeConverted.Count != 0)
                {
                    xGridConvertibleActions.DataSourceList = mWizard.ActionToBeConverted;
                    SetGridView();
                    return;
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.NoConvertibleActionsFound);
                    return;
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoActivitySelectedForConversion);
            }
        }

        private void SetGridView()
        {
            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableActionDetails.Selected), Header = "Select", WidthWeight = 3.5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox, BindingMode = System.Windows.Data.BindingMode.TwoWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableActionDetails.SourceActionTypeName), WidthWeight = 15, Header = "Source Action Type" });
            view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableActionDetails.Activities), WidthWeight = 15, Header = "Source " + GingerDicser.GetTermResValue(eTermResKey.Activities) });
            view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableActionDetails.TargetActionTypeName), WidthWeight = 15, Header = "Target Action Type" });
            xGridConvertibleActions.SetAllColumnsDefaultView(view);
            xGridConvertibleActions.InitViewItems();
            xGridConvertibleActions.SetTitleLightStyle = true;
            xGridConvertibleActions.btnMarkAll.Visibility = System.Windows.Visibility.Visible;
            xGridConvertibleActions.MarkUnMarkAllActive += MarkUnMarkAllActions;
            xGridConvertibleActions.ValidationRules = new List<ucGrid.eUcGridValidationRules>()
            {
                ucGrid.eUcGridValidationRules.CheckedRowCount
            };
        }

        private void MarkUnMarkAllActions(bool ActiveStatus)
        {
            foreach (ConvertableActionDetails act in xGridConvertibleActions.DataSourceList)
            {
                act.Selected = ActiveStatus;
            }
        }
    }
}
