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
using Amdocs.Ginger.CoreNET;
using Ginger.UserControls;
using GingerCore;
using GingerWPF.WizardLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

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
                case EventType.LeavingForNextPage:
                    SetSelectedActionsForConversion();
                    break;
            }
        }

        /// <summary>
        /// This method will update the selected actions for conversion
        /// </summary>
        private void SetSelectedActionsForConversion()
        {
            if (xGridConvertibleActions.DataSourceList != null)
            {
                foreach (ConvertableActionDetails act in xGridConvertibleActions.DataSourceList)
                {
                    if (act.Selected)
                    {
                        foreach (var convertableAction in mWizard.ActionToBeConverted)
                        {
                            if (act.SourceActionTypeName == convertableAction.SourceActionTypeName && act.TargetActionTypeName == convertableAction.TargetActionTypeName)
                            {
                                convertableAction.Selected = act.Selected;
                            }
                        }
                    }
                } 
            }
        }

        private void Init()
        {   
            if (mWizard.LstSelectedActivities == null || mWizard.LstSelectedActivities.Count <= 0)
            {
                // fetching list of selected convertible activities from the first grid
                if (mWizard.ConversionType == ActionsConversionWizard.eActionConversionType.SingleBusinessFlow)
                {
                    mWizard.LstSelectedActivities = mWizard.Context.BusinessFlow.Activities.Where(x => x.SelectedForConversion).ToList();
                }
                else
                {
                    mWizard.LstSelectedActivities = mWizard.ListOfBusinessFlow.Where(x => x.IsSelected).SelectMany(y => y.BusinessFlow.Activities).Where(z => z.Active).ToList();
                } 
            }

            if (mWizard.LstSelectedActivities.Count != 0)
            {
                xGridConvertibleActions.ValidationRules = new List<ucGrid.eUcGridValidationRules>()
                {
                    ucGrid.eUcGridValidationRules.CheckedRowCount
                };

                if (mWizard.ActionToBeConverted == null || mWizard.ActionToBeConverted.Count <= 0)
                {
                    ActionConversionUtils utils = new ActionConversionUtils();
                    mWizard.ActionToBeConverted = utils.GetConvertableActivityActions(mWizard.LstSelectedActivities); 
                }
                if (mWizard.ActionToBeConverted.Count != 0)
                {
                    xGridConvertibleActions.DataSourceList = GetDistinctList(mWizard.ActionToBeConverted);
                    SetGridView();
                    return;
                }
                else
                {                    
                    Reporter.ToLog(eLogLevel.INFO, "No Convertible Actions Found");
                    return;
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoActivitySelectedForConversion);
            }
        }

        /// <summary>
        /// This method is used to get the distinct list
        /// </summary>
        /// <param name="actionToBeConverted"></param>
        /// <returns></returns>
        private IObservableList GetDistinctList(ObservableList<ConvertableActionDetails> actionToBeConverted)
        {
            ObservableList<ConvertableActionDetails> list = new ObservableList<ConvertableActionDetails>();
            foreach (var act in actionToBeConverted)
            {
                ConvertableActionDetails det = list.Where(x => x.SourceActionTypeName == act.SourceActionTypeName && x.TargetActionTypeName == act.TargetActionTypeName).FirstOrDefault();
                if (det == null)
                {
                    list.Add(act);
                } 
            }
            return list;
        }

        private void SetGridView()
        {
            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableActionDetails.Selected), Header = "Select", WidthWeight = 3.5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox, BindingMode = System.Windows.Data.BindingMode.TwoWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableActionDetails.SourceActionTypeName), WidthWeight = 15, Header = "Source Action Type" });
            if (mWizard.ConversionType == ActionsConversionWizard.eActionConversionType.SingleBusinessFlow)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableActionDetails.Activities), WidthWeight = 15, Header = "Source " + GingerDicser.GetTermResValue(eTermResKey.Activities) }); 
            }
            view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableActionDetails.TargetActionTypeName), WidthWeight = 15, Header = "Target Action Type" });
            if (mWizard.ConversionType == ActionsConversionWizard.eActionConversionType.MultipleBusinessFlow)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableActionDetails.ActionCount), WidthWeight = 15, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Header = "Instances Count" });
            }
            xGridConvertibleActions.SetAllColumnsDefaultView(view);
            xGridConvertibleActions.InitViewItems();
            xGridConvertibleActions.SetTitleLightStyle = true;
            xGridConvertibleActions.btnMarkAll.ToolTip = "Mark All As Active";
            xGridConvertibleActions.SetBtnImage(xGridConvertibleActions.btnMarkAll, "@CheckAllColumn_16x16.png");
            xGridConvertibleActions.btnMarkAll.Visibility = System.Windows.Visibility.Visible;
            xGridConvertibleActions.MarkUnMarkAllActive += MarkUnMarkAllActions;
            xGridConvertibleActions.ActiveStatus = true;
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
