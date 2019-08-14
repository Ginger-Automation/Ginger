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
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.Actions.ActionConversion
{
    /// <summary>
    /// Interaction logic for ConversionStatusReportPage.xaml
    /// </summary>
    public partial class ConversionStatusReportPage : Page, IWizardPage
    {
        ActionsConversionWizard mWizard;
        public ObservableList<BusinessFlowConversionStatus> ListOfBusinessFlow = null;
        List<string> SelectedBusinessFlow = new List<string>();

        /// <summary>
        /// Constructor for configuration page
        /// </summary>
        public ConversionStatusReportPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Wizard events
        /// </summary>
        /// <param name="WizardEventArgs"></param>
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

        /// <summary>
        /// This method is used to init the configuration settings page
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        private void Init(WizardEventArgs WizardEventArgs)
        {
            Dispatcher.Invoke(() =>
            {
                mWizard.ProcessStarted();
                DataContext = mWizard;
                SetBusinessFlowConversionStatusGridView();
                SetButtonsVisibility(false);
                mWizard.ConverToActions(ListOfBusinessFlow);
                mWizard.ProcessEnded();
            });
        }

        /// <summary>
        /// This method is used to set the columns for BusinessFlow Conversion Status GridView
        /// </summary>
        private void SetBusinessFlowConversionStatusGridView()
        {
            Dispatcher.Invoke(() =>
            {
                xBusinessFlowGrid.SetTitleLightStyle = true;
                //Set the Data Grid columns
                GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
                view.GridColsView = new ObservableList<GridColView>();

                view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlowConversionStatus.IsSelected), WidthWeight = 5, StyleType = GridColView.eGridColStyleType.CheckBox, Header = "Select" });
                view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlowConversionStatus.RelativeFilePath), WidthWeight = 20, ReadOnly = true, Header = "Folder" });
                view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlowConversionStatus.BusinessFlowName), WidthWeight = 25, ReadOnly = true, Header = "Name" });
                view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlowConversionStatus.StatusIcon), WidthWeight = 15, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"], ReadOnly = true, Header = "Conversion Status" });
                view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlowConversionStatus.SaveStatus), WidthWeight = 10, ReadOnly = true, Header = "Save Status" });

                xBusinessFlowGrid.SetAllColumnsDefaultView(view);
                xBusinessFlowGrid.InitViewItems();
                xBusinessFlowGrid.btnMarkAll.Visibility = System.Windows.Visibility.Visible;
                xBusinessFlowGrid.MarkUnMarkAllActive += MarkUnMarkAllActions;
                xBusinessFlowGrid.DataSourceList = GetBusinessFlowList();
                xBusinessFlowGrid.ShowTitle = Visibility.Collapsed;
            });
        }

        /// <summary>
        /// This method is used to get the businessFlowList
        /// </summary>
        /// <returns></returns>
        private ObservableList<BusinessFlowConversionStatus> GetBusinessFlowList()
        {
            ListOfBusinessFlow = new ObservableList<BusinessFlowConversionStatus>();
            foreach (BusinessFlow bf in mWizard.ListOfBusinessFlow)
            {
                if (bf.Selected && (SelectedBusinessFlow == null || SelectedBusinessFlow.Count == 0 || SelectedBusinessFlow.Contains(bf.Name)))
                {
                    BusinessFlow destinationBf = new BusinessFlow();
                    BusinessFlowConversionStatus flowConversion = new BusinessFlowConversionStatus();
                    flowConversion.BusinessFlow = bf;
                    flowConversion.ActivitiesCount = bf.Activities.Count;
                    flowConversion.CurrentActivityIndex = 0;
                    flowConversion.ConversionStatus = eConversionStatus.Pending;
                    ListOfBusinessFlow.Add(flowConversion);
                }
            }
            return ListOfBusinessFlow;
        }

        /// <summary>
        /// This method is used to Stop the conversion process in between conversion process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            SetButtonsVisibility(true);
            mWizard.StopConversion();
        }

        /// <summary>
        /// This method is used to convert the action in case of Continue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContinueButtonClicked(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                mWizard.ProcessStarted();
                SetButtonsVisibility(false);
                mWizard.ProcessConversion(ListOfBusinessFlow, false);
                mWizard.ProcessEnded();
            });
        }

        /// <summary>
        /// This method is used to convert the action in case of Re-Convert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReConvertButtonClicked(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                mWizard.ProcessStarted();
                SetButtonsVisibility(false);
                GetSelectedBusinessFlows();
                mWizard.ProcessConversion(ListOfBusinessFlow, true);
                mWizard.ProcessEnded();
            });
        }

        /// <summary>
        /// This method will get the names of selected businessflows
        /// </summary>
        private void GetSelectedBusinessFlows()
        {
            SelectedBusinessFlow = new List<string>();
            foreach (var bfs in ListOfBusinessFlow)
            {
                if(bfs.IsSelected)
                {
                    SelectedBusinessFlow.Add(bfs.BusinessFlowName);
                }
            }
        }

        /// <summary>
        /// This method is used to save the converted actions and save the businessflow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                    mWizard.ProcessStarted();

                    foreach (var bf in ListOfBusinessFlow)
                    {
                        if (bf.IsSelected)
                        {
                            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(bf.BusinessFlow);
                            bf.SaveStatus = eConversionSaveStatus.Saved;
                        }
                    }

                    mWizard.ProcessEnded();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to Save - ", ex);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            });
        }

        private void MarkUnMarkAllActions(bool status)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (BusinessFlowConversionStatus bf in xBusinessFlowGrid.DataSourceList)
                {
                    bf.IsSelected = status;
                }
            });
        }

        public void SetButtonsVisibility(bool saveVisible)
        {
            xSaveButton.Visibility = saveVisible ? Visibility.Visible : Visibility.Collapsed;
            xStopButton.Visibility = saveVisible ? Visibility.Collapsed : Visibility.Visible;
            xContinue.Visibility = saveVisible ? Visibility.Visible : Visibility.Collapsed;
            xReConvert.Visibility = saveVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
