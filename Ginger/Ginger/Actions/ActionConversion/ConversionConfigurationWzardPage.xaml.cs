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
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.UserControls;
using GingerCore.Actions.Common;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.ActionConversion
{
    /// <summary>
    /// Interaction logic for ConversionConfigurationWzardPage.xaml
    /// </summary>
    public partial class ConversionConfigurationWzardPage : Page, IWizardPage
    {
        ActionsConversionWizard mWizard;

        public ConversionConfigurationWzardPage()
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
                    Init(WizardEventArgs);
                    break;
            }
        }
        
        private void Init(WizardEventArgs WizardEventArgs)
        {
            //mWizard.DefaultTargetAppChecked = Convert.ToBoolean(xChkDefaultTargetApp.IsChecked);
            
            LocateByPOMElementPage locateByPOMElementPage = new LocateByPOMElementPage(mWizard.Context, null, nameof(ActUIElement.ElementType), mWizard, nameof(mWizard.SelectedPOMObjectName), true);
            xLocateValueEditFrame.Content = locateByPOMElementPage;
            DataContext = mWizard;
            SetGridView();
        }

        private void SetGridView()
        {
            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableTargetApplicationDetails.Selected), Header = "Select", WidthWeight = 3.5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox, BindingMode = System.Windows.Data.BindingMode.TwoWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableTargetApplicationDetails.SourceTargetApplicationName), WidthWeight = 15, Header = "Source - Taret Application" });
            view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableTargetApplicationDetails.TargetTargetApplicationName), WidthWeight = 15, Header = "Target - Target Application" });
            xTargetApplication.SetAllColumnsDefaultView(view);
            xTargetApplication.InitViewItems();

            ObservableList<ConvertableTargetApplicationDetails> lst = GetTargetApplication();
            xTargetApplication.DataSourceList = lst;

            xTargetApplication.SetTitleLightStyle = true;
            xTargetApplication.btnMarkAll.Visibility = System.Windows.Visibility.Visible;
            xTargetApplication.MarkUnMarkAllActive += MarkUnMarkAllTargetApplication;
            xTargetApplication.ValidationRules = new List<ucGrid.eUcGridValidationRules>()
            {
                ucGrid.eUcGridValidationRules.CheckedRowCount
            };
        }

        private void MarkUnMarkAllTargetApplication(bool Status)
        {
            
        }

        private void ControlsViewsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ControlsViewRow.Height = new GridLength(200, GridUnitType.Star);
            ControlsViewRow.MaxHeight = Double.PositiveInfinity;
            //if (Row2Splitter != null)
            //    Row2Splitter.IsEnabled = true;
        }

        private void ControlsViewsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ControlsViewRow.Height = new GridLength(35);
            ControlsViewRow.MaxHeight = 35;
            //if (Row2Splitter != null)
            //    Row2Splitter.IsEnabled = false;
        }

        //private void btnRefresh_Click(object sender, RoutedEventArgs e)
        //{
        //    SetGridView();
        //}

        private ObservableList<ConvertableTargetApplicationDetails> GetTargetApplication()
        {
            ObservableList<ConvertableTargetApplicationDetails> lstTA = new ObservableList<ConvertableTargetApplicationDetails>();
            // fetching list of selected convertible activities from the first grid
            if (mWizard.ConversionType == ActionsConversionWizard.eActionConversionType.SingleBusinessFlow)
            {
                foreach (var targetBase in mWizard.Context.BusinessFlow.TargetApplications)
                {
                    lstTA.Add(new ConvertableTargetApplicationDetails() { SourceTargetApplicationName = targetBase.ItemName, TargetTargetApplicationName = targetBase.ItemName });
                }
            }
            else
            {
                foreach (var targetBase in mWizard.ListOfBusinessFlow.Where(x => x.SelectedForConversion).SelectMany(y => y.TargetApplications))
                {
                    lstTA.Add(new ConvertableTargetApplicationDetails() { SourceTargetApplicationName = targetBase.ItemName, TargetTargetApplicationName = targetBase.ItemName });
                }
            }

            return lstTA;
        }
    }
}
