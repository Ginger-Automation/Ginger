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
using GingerWPF.WizardLib;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.ActionConversion
{
    /// <summary>
    /// Interaction logic for ConversionStatusReportPage.xaml
    /// </summary>
    public partial class ConversionStatusReportPage : Page, IWizardPage
    {
        ActionsConversionWizard mWizard;
        public ObservableList<BusinessFlowToConvert> ListOfBusinessFlow = null;

        /// <summary>
        /// Constructor for configuration page
        /// </summary>
        public ConversionStatusReportPage(ObservableList<BusinessFlowToConvert> listOfBusinessFlow)
        {
            InitializeComponent();

            ListOfBusinessFlow = listOfBusinessFlow;
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
                DataContext = mWizard;
                SetBusinessFlowConversionStatusGridView();
                SetButtonsVisibility(false);
                xContinue.Visibility = Visibility.Collapsed;
                mWizard.BusinessFlowsActionsConversion(ListOfBusinessFlow);
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

                view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlowToConvert.IsSelected), WidthWeight = 5, StyleType = GridColView.eGridColStyleType.CheckBox, Header = "Select" });
                view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlowToConvert.BusinessFlowName), WidthWeight = 23, ReadOnly = true, Header = "Name" });
                view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlowToConvert.ConvertedActionsCount), WidthWeight = 13, ReadOnly = true, HorizontalAlignment= HorizontalAlignment.Center, Header = "Converted Actions" });
                view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlowToConvert.TotalProcessingActionsCount), WidthWeight = 13, ReadOnly = true,
                                                          HorizontalAlignment = HorizontalAlignment.Center, Header = "Convertible Actions" });
                view.GridColsView.Add(new GridColView()
                {
                    Field = nameof(BusinessFlowToConvert.StatusIcon),
                    WidthWeight = 15,
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = (DataTemplate)this.PageGrid.Resources["xConversionStatusIconTemplate"],
                    ReadOnly = true,
                    Header = "Conversion Status",
                    BindingMode = System.Windows.Data.BindingMode.OneWayToSource
                });
                view.GridColsView.Add(new GridColView()
                {
                    Field = nameof(BusinessFlowToConvert.SaveStatusIcon),
                    WidthWeight = 10,
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = (DataTemplate)this.PageGrid.Resources["xConversionSaveStatusIconTemplate"],
                    ReadOnly = true,
                    Header = "Save Status",
                    BindingMode = System.Windows.Data.BindingMode.OneWayToSource
                });

                xBusinessFlowGrid.SetAllColumnsDefaultView(view);
                xBusinessFlowGrid.InitViewItems();
                xBusinessFlowGrid.btnMarkAll.Visibility = System.Windows.Visibility.Visible;
                xBusinessFlowGrid.MarkUnMarkAllActive += MarkUnMarkAllActions;
                xBusinessFlowGrid.DataSourceList = GetBusinessFlowList();
                xBusinessFlowGrid.ShowTitle = Visibility.Collapsed;
                xBusinessFlowGrid.ActiveStatus = false;
            });
        }

        /// <summary>
        /// This method is used to get the businessFlowList
        /// </summary>
        /// <returns></returns>
        private ObservableList<BusinessFlowToConvert> GetBusinessFlowList()
        {
            ListOfBusinessFlow = new ObservableList<BusinessFlowToConvert>();
            foreach (BusinessFlowToConvert bf in mWizard.ListOfBusinessFlow)
            {
                if (bf.IsSelected)
                {
                    BusinessFlowToConvert flowConversion = new BusinessFlowToConvert();
                    flowConversion.BusinessFlow = bf.BusinessFlow;
                    flowConversion.IsSelected = true;
                    flowConversion.ConversionStatus = eConversionStatus.Pending;
                    flowConversion.TotalProcessingActionsCount = bf.TotalProcessingActionsCount;
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
            xContinue.Visibility = Visibility.Visible;
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
                SetButtonsVisibility(false);
                xContinue.Visibility = Visibility.Collapsed;
                mWizard.ProcessConversion(ListOfBusinessFlow, false);
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
                SetButtonsVisibility(false);
                xContinue.Visibility = Visibility.Collapsed;
                ObservableList<BusinessFlowToConvert> lst = GetListToReConvert(ListOfBusinessFlow);
                mWizard.ProcessConversion(lst, true);
            });
        }

        /// <summary>
        /// This method is used to get the businessFlowList to reconvert
        /// </summary>
        /// <param name="listOfBusinessFlow"></param>
        /// <returns></returns>
        private ObservableList<BusinessFlowToConvert> GetListToReConvert(ObservableList<BusinessFlowToConvert> listOfBusinessFlow)
        {
            ObservableList<BusinessFlowToConvert> lst = new ObservableList<BusinessFlowToConvert>();
            foreach (BusinessFlowToConvert bf in listOfBusinessFlow)
            {
                if (bf.IsSelected)
                {
                    bf.ConversionStatus = eConversionStatus.Pending;
                    lst.Add(bf);
                }
            }
            return lst;
        }

        /// <summary>
        /// This method is used to save the converted actions and save the businessflow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            mWizard.ProcessStarted();

            await Task.Run(() =>
            {
                foreach (BusinessFlowToConvert bf in xBusinessFlowGrid.DataSourceList)
                {
                    try
                    {
                        if (bf.IsSelected && bf.SaveStatus != eConversionSaveStatus.NA)
                        {
                            if (bf.ConvertedActionsCount > 0)
                            {
                                bf.SaveStatus = eConversionSaveStatus.Saving;
                                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(bf.BusinessFlow);
                                bf.SaveStatus = eConversionSaveStatus.Saved; 
                            }
                            else
                            {
                                bf.SaveStatus = eConversionSaveStatus.NA;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        bf.SaveStatus = eConversionSaveStatus.Failed;
                        Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to Save - ", ex);
                    }
                }
            });

            mWizard.ProcessEnded();
        }

        private void MarkUnMarkAllActions(bool status)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (BusinessFlowToConvert bf in xBusinessFlowGrid.DataSourceList)
                {
                    bf.IsSelected = status;
                }
            });
        }

        public void SetButtonsVisibility(bool saveVisible)
        {
            xSaveButton.Visibility = saveVisible ? Visibility.Visible : Visibility.Collapsed;
            xStopButton.Visibility = saveVisible ? Visibility.Collapsed : Visibility.Visible;
            xReConvert.Visibility = saveVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
