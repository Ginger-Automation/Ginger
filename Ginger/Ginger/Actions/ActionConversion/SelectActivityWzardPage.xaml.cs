#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Ginger.UserControls;
using GingerCore;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.Actions.ActionConversion
{
    /// <summary>
    /// Interaction logic for SelectActivityWzardPage.xaml
    /// </summary>
    public partial class SelectActivityWzardPage : Page, IWizardPage
    {
        ActionsConversionWizard mWizard;
        public SelectActivityWzardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (ActionsConversionWizard)WizardEventArgs.Wizard;
                    ((WizardWindow)mWizard.mWizardWindow).xFinishButton.IsEnabled = false;
                    SetGridsView();
                    break;
            }
        }

        private void SetGridsView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(Activity.SelectedForConversion), WidthWeight = 2.5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox, Header = "Select", BindingMode = System.Windows.Data.BindingMode.TwoWay });
            defView.GridColsView.Add(new GridColView() { Field = nameof(Activity.ActivityName), WidthWeight = 15, Header = "Name of " + GingerDicser.GetTermResValue(eTermResKey.Activity) });
            xGrdGroups.SetAllColumnsDefaultView(defView);
            xGrdGroups.InitViewItems();
            xGrdGroups.SetTitleLightStyle = true;
            xGrdGroups.btnMarkAll.Visibility = System.Windows.Visibility.Visible;
            xGrdGroups.btnMarkAll.ToolTip = "Mark All As Active";
            xGrdGroups.SetBtnImage(xGrdGroups.btnMarkAll, "@CheckAllColumn_16x16.png");
            ActionConversionUtils utils = new ActionConversionUtils();
            ObservableList<Activity> lst = utils.GetConvertableActivitiesFromBusinessFlow(mWizard.Context.BusinessFlow);
            xGrdGroups.DataSourceList = lst;
            xGrdGroups.RowChangedEvent += grdGroups_RowChangedEvent;
            xGrdGroups.Title = "Convert " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            xGrdGroups.MarkUnMarkAllActive += MarkUnMarkAllActivities;
            xGrdGroups.ValidationRules = new List<ucGrid.eUcGridValidationRules>()
            {
                ucGrid.eUcGridValidationRules.CheckedRowCount
            };
            xGrdGroups.ActiveStatus = true;
        }

        private void grdGroups_RowChangedEvent(object sender, EventArgs e)
        {
            if (mWizard.Context.BusinessFlow != null)
            {
                mWizard.Context.BusinessFlow.CurrentActivity = (Activity)xGrdGroups.CurrentItem;
            }
        }

        private void MarkUnMarkAllActivities(bool ActiveStatus)
        {
            foreach (Activity act in xGrdGroups.DataSourceList)
            {
                act.SelectedForConversion = ActiveStatus;
            }
        }
    }
}
