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
using Amdocs.Ginger.CoreNET;
using Ginger.UserControls;
using GingerCore;
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
                    SetGridsView();
                    break;
            }
        }

        private void SetGridsView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = Activity.Fields.SelectedForConversion, WidthWeight = 2.5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox, Header = "Select" });
            defView.GridColsView.Add(new GridColView() { Field = Activity.Fields.ActivityName, WidthWeight = 15, Header = "Name of " + GingerDicser.GetTermResValue(eTermResKey.Activity) });
            xGrdGroups.SetAllColumnsDefaultView(defView);
            xGrdGroups.InitViewItems();
            xGrdGroups.SetTitleLightStyle = true;

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
            if (xGrdGroups.DataSourceList.Count <= 0)
            {
                return;
            }
            if (xGrdGroups.DataSourceList.Count > 0)
            {
                ObservableList<Activity> lstMarkUnMarkActivities = (ObservableList<Activity>)xGrdGroups.DataSourceList;
                foreach (Activity act in lstMarkUnMarkActivities)
                {
                    act.SelectedForConversion = ActiveStatus;
                }
                xGrdGroups.DataSourceList = lstMarkUnMarkActivities;
            }
        }
    }
}
