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
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Common.Expressions;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.BusinessFlowWindows;
using Ginger.Help;
using Ginger.Reports;
using Ginger.Repository;
using Ginger.UserControls;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Java;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Ginger.Actions
{
    enum eGridView {All,NonSimulation}

    public partial class ActionEditPage : Page
    {
        //static int ActionEditNum = 0;
        //static int LiveActionEditCounter = 0;
        //~ActionEditPage()
        //{
        //    LiveActionEditCounter--;
        //}

        private Act mAction;
        static public string sMultiLocatorVals = "";
        GenericWindow _pageGenericWin = null;
        //public ActionsPage ap;

        bool IsPageClosing = false;

        private static  readonly List<ComboEnumItem> OperatorList = GingerCore.General.GetEnumValuesForCombo(typeof(eOperator));

        ObservableList<DataSourceBase> mDSList = new ObservableList<DataSourceBase>();
        ObservableList<DataSourceTable> mDSTableList = new ObservableList<DataSourceTable>();
        List<string> mDSNames = new List<string>();
        private DataSourceTable mDSTable;
        private string mDataSourceName;
        List<String> mColNames = null;
        ObservableList<ActOutDataSourceConfig> aOutDSConfigParam = new ObservableList<ActOutDataSourceConfig>();

        private BusinessFlow mActParentBusinessFlow = null;
        private Activity mActParentActivity = null;

        Button mSimulateRunBtn = new Button();
        Button mRunActionBtn = new Button();
        Button mStopRunBtn = new Button();
        ComboBox dsOutputParamMapType = new ComboBox();
        private bool saveWasDone = false;

        Context mContext;

        public General.eRIPageViewMode EditMode { get; set; }

        public ActionEditPage(Act act, General.eRIPageViewMode editMode = General.eRIPageViewMode.Automation, BusinessFlow actParentBusinessFlow = null, Activity actParentActivity = null)
        {
            InitializeComponent();

            //ActionEditNum++;
            //LiveActionEditCounter++;

            mAction = act;
            if (editMode != General.eRIPageViewMode.View)
            {
                mAction.SaveBackup();
            }
            mContext = Context.GetAsContext(mAction.Context);
            RunDescritpion.Init(mContext, act, Act.Fields.RunDescription);

            if (actParentBusinessFlow != null)
            {
                mActParentBusinessFlow = actParentBusinessFlow;
            }
            else if (mAction.Context != null && mContext.BusinessFlow != null)
            {
                mActParentBusinessFlow = mContext.BusinessFlow;
            }

            if (actParentActivity != null)
            {
                mActParentActivity = actParentActivity;
            }
            else if (mAction.Context != null && mContext.Activity != null)
            {
                mActParentActivity = (Activity)mContext.Activity;
            }

            EditMode = editMode;
            mAction.PropertyChanged -= ActionPropertyChanged;
            mAction.PropertyChanged += ActionPropertyChanged;
            
            GingerHelpProvider.SetHelpString(this, act.ActionDescription);

            if (mAction.ConfigOutputDS == true && mAction.DSOutputConfigParams.Count > 0)
            {
                xDataSourceExpander.IsExpanded = true;
                mAction.OutDataSourceName = mAction.DSOutputConfigParams[0].DSName;
                mAction.OutDataSourceTableName = mAction.DSOutputConfigParams[0].DSTable;
                if (mAction.DSOutputConfigParams[0].OutParamMap == null)
                    mAction.OutDSParamMapType = Act.eOutputDSParamMapType.ParamToRow.ToString();
                else
                    mAction.OutDSParamMapType = mAction.DSOutputConfigParams[0].OutParamMap;
            }


            if (mActParentActivity != null && mActParentActivity.GetType() == typeof(ErrorHandler))
            {
                RetyrMechainsmTab.IsEnabled = false;
                ScreenShotTab.IsEnabled = false;
            }
            //Binding            
            txtDescription.BindControl(mAction, Act.Fields.Description);

            if (mAction.ObjectLocatorConfigsNeeded)
            {
                //List<eLocateBy> locateByList = act.AvailableLocateBy().Where(e => e != eLocateBy.POMElement).ToList();
                //cboLocateBy.BindControl(mAction, Act.Fields.LocateBy, act.AvailableLocateBy());
                cboLocateBy.BindControl(mAction, Act.Fields.LocateBy, act.AvailableLocateBy().Where(e => e != eLocateBy.POMElement).ToList());             
                txtLocateValue.BindControl(mContext, mAction, Act.Fields.LocateValue);
            }
            comboWindowsToCapture.BindControl(mAction, Act.Fields.WindowsToCapture);

            //Run Details binding
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RTStatusLabel, Label.ContentProperty, mAction, Act.Fields.Status, BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RTElapsedLabel, Label.ContentProperty, mAction, Act.Fields.ElapsedSecs, BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RTErrorLabel, TextBox.TextProperty, mAction, Act.Fields.Error, BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RTExInfoLabel, TextBox.TextProperty, mAction, Act.Fields.ExInfo, BindingMode.OneWay);

            //TODO: add tooltip on class level 
            //TODO: Add BindToolTip or use BindControl and supply DependecyProperty


            // !!!!!!!!!!!!!!!!!!!!!???????????????????????????????????
            if (mAction.ObjectLocatorConfigsNeeded)
            {
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtLocateValue, TextBox.ToolTipProperty, mAction, Act.Fields.LocateValue);
            }
            // TODO: create BindControl for 
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TakeScreenShotCheckBox, CheckBox.IsCheckedProperty, mAction, Act.Fields.TakeScreenShot);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(FailIgnoreCheckBox, CheckBox.IsCheckedProperty, mAction, Act.Fields.FailIgnored);

            comboFinalStatus.BindControl(mAction, Act.Fields.StatusConverter);
            xWaittxtWait.BindControl(mContext, mAction, nameof(Act.WaitVE));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtTimeout, TextBox.TextProperty, mAction, Act.Fields.Timeout);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(StatusLabel, Label.ContentProperty, mAction, Act.Fields.Status);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ErrorTextBlock, TextBlock.TextProperty, mAction, Act.Fields.Error);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ExtraInfoTextBlock, TextBlock.TextProperty, mAction, Act.Fields.ExInfo);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(EnableRetryMechanismCheckBox, CheckBox.IsCheckedProperty, mAction, Act.Fields.EnableRetryMechanism);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RetryMechanismIntervalTextBox, TextBox.TextProperty, mAction, Act.Fields.RetryMechanismInterval);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RetryMechanismMaxRetriesTextBox, TextBox.TextProperty, mAction, Act.Fields.MaxNumberOfRetries);

            dsOutputParamMapType = DataSourceConfigGrid.AddComboBox(typeof(Act.eOutputDSParamMapType), "Out Param Mapping", "", new RoutedEventHandler(OutDSParamType_SelectionChanged));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AddOutDS, CheckBox.IsCheckedProperty, mAction, Act.Fields.ConfigOutputDS);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(cmbDataSourceName, ComboBox.TextProperty, mAction, Act.Fields.OutDataSourceName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(cmbDataSourceTableName, ComboBox.TextProperty, mAction, Act.Fields.OutDataSourceTableName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(dsOutputParamMapType, ComboBox.SelectedValueProperty, mAction, Act.Fields.OutDSParamMapType);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(EnableActionLogConfigCheckBox, CheckBox.IsCheckedProperty, mAction, nameof(Act.EnableActionLogConfig));

            // Why we bind twice??
            if (mAction.ObjectLocatorConfigsNeeded)
            {
                txtLocateValue.BindControl(mContext, mAction, Act.Fields.LocateValue);
                txtLocateValue.ValueTextBox.Text = mAction.LocateValue;  // Why ?
            }

            SwitchingInputValueBoxAndGrid(mAction);
            LoadActionInfoPage(mAction);

            LoadActionFlowcontrols(mAction);
            TagsViewer.Init(mAction.Tags);

            mDSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            if (mDSList.Count == 0)
                AddOutDS.IsEnabled = false;

            OutputValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddReturnValue));
            OutputValuesGrid.AddSeparator();

            //allowing return values automatically in Edit Action window
            if (mAction.AddNewReturnParams == null && mAction.ReturnValues.Count() == 0)
                mAction.AddNewReturnParams = true;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(OutputValuesGrid.AddCheckBox("Add Parameters Automatically", null), CheckBox.IsCheckedProperty, mAction, Act.Fields.AddNewReturnParams);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(OutputValuesGrid.AddCheckBox("Support Simulation", new RoutedEventHandler(RefreshOutputColumns)), CheckBox.IsCheckedProperty, mAction, Act.Fields.SupportSimulation);
            OutputValuesGrid.AddToolbarTool("@Reset_16x16.png", "Clear Un-used Parameters", new RoutedEventHandler(ClearUnusedParameter));
            OutputValuesGrid.AllowHorizentalScroll = true;
            SetActReturnValuesGrid();
            SetActInputValuesGrid();

            OutputValuesGrid.DataSourceList = mAction.ReturnValues;
            InputValuesGrid.DataSourceList = mAction.InputValues;
            mAction.InputValues.CollectionChanged -= InputValues_CollectionChanged;
            mAction.InputValues.CollectionChanged += InputValues_CollectionChanged;

            ShowHideRunStopButtons();
            UpdatePassFailImages();

            if (mAction.ObjectLocatorConfigsNeeded == false)
                ActionLocatorPanel.Visibility = System.Windows.Visibility.Collapsed;

            UpdateTabsVisual();
            UpdateHelpTab();

            mAction.FlowControls.CollectionChanged -= FlowControls_CollectionChanged;
            mAction.FlowControls.CollectionChanged += FlowControls_CollectionChanged;

            mAction.ReturnValues.CollectionChanged -= ReturnValues_CollectionChanged;
            mAction.ReturnValues.CollectionChanged += ReturnValues_CollectionChanged;

            DataSourceConfigGrid.LostFocus += DataSourceConfigGrid_LostFocus;

            if (EditMode == General.eRIPageViewMode.Automation)
            {
                SharedRepoInstanceUC.Init(mAction, null);
            }
            else
            {
                SharedRepoInstanceUC.Visibility = Visibility.Collapsed;
                SharedRepoInstanceUC_Col.Width = new GridLength(0);
            }

            if (editMode == General.eRIPageViewMode.View)
            {
                SetViewMode();
            }

            if (mAction.Status == null || mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending || mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.NA)
                xRunStatusExpander.IsExpanded = false;

            InitActionLog();
            if (mContext != null && mContext.Runner != null)
            {
                mContext.Runner.PrepActionValueExpression(mAction, actParentBusinessFlow);
            }
        }

        public void StopEdit()
        {
            if (AFCP != null)
            {
                AFCP.FlowControlGrid.Grid.CommitEdit();
                AFCP.FlowControlGrid.Grid.CancelEdit();
            }
            if (InputValuesGrid != null)
            {
                InputValuesGrid.Grid.CommitEdit();
                InputValuesGrid.Grid.CancelEdit();
            }
            if (DataSourceConfigGrid != null)
            {
                DataSourceConfigGrid.Grid.CommitEdit();
                DataSourceConfigGrid.Grid.CancelEdit();
            }
            if (OutputValuesGrid != null)
            {
                OutputValuesGrid.Grid.CommitEdit();
                OutputValuesGrid.Grid.CancelEdit();
            }            
        }

        private void ReturnValues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateOutputTabVisual();
            mAction.OnPropertyChanged(nameof(Act.ReturnValuesCount));
        }

        private void FlowControls_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateFlowControlTabVisual();
            mAction.OnPropertyChanged(nameof(Act.FlowControlsInfo));
        }

        private void ClearUnusedParameter(object sender, RoutedEventArgs e)
        {
            mAction.ClearUnUsedReturnParams();
        }

        private void RefreshOutputColumns(object sender, RoutedEventArgs e)
        {
            if (mAction.SupportSimulation)
                OutputValuesGrid.ChangeGridView(eGridView.All.ToString());
            else
                OutputValuesGrid.ChangeGridView(eGridView.NonSimulation.ToString());
        }

        ActionFlowControlPage AFCP;
        private void LoadActionFlowcontrols(Act a)
        {
            FlowControlFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
            
            if(EditMode == General.eRIPageViewMode.View)
            {
                AFCP = new ActionFlowControlPage(a, mActParentBusinessFlow, mActParentActivity, General.eRIPageViewMode.View);
            }
            else if (EditMode == General.eRIPageViewMode.SharedReposiotry)
            {
                AFCP = new ActionFlowControlPage(a, mActParentBusinessFlow, mActParentActivity, General.eRIPageViewMode.SharedReposiotry);
            }
            else
            {
                AFCP = new ActionFlowControlPage(a, mActParentBusinessFlow, mActParentActivity);
            }
            FlowControlFrame.SetContent(AFCP);
        }
        
        private void InputValues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {         
            SwitchingInputValueBoxAndGrid(mAction);     
        }

        //FIXME - remove after moving to ActUIElement !!!!
        //TODO: Ugly code. Find a better way to do it
        // Split the flags in to two one for value field needed and other for grid needed
        // and grid needed flag will be set only for those actions where it is needed.
        // This way we will remove all below ugly else if conditions
        private void SwitchingInputValueBoxAndGrid(Act a)
        {
            if (IsPageClosing) return; // no need to update the UI since we are closing, when done in Undo changes/Cancel 
            // we do restore and don't want to raise events which will cause exception  (a.Value = ""  - is the messer)
            
            if (mAction.ValueConfigsNeeded == false)
            {
                this.Dispatcher.Invoke(() =>
                {
                    InputValuesEditControls.Visibility = System.Windows.Visibility.Collapsed;
                });
                return;
            }

            //TODO: Remove all if else and handle it dynamically based on if Input value grid is needed or not
            int minimumInputValuesToHideGrid = 1;
            if(mAction.ObjectLocatorConfigsNeeded)
            {
                //For actions with locator config needed, Locate by , locate value is also added to input value                
                minimumInputValuesToHideGrid = 3;
            }

            if (a.GetType() != typeof(ActDBValidation) && a.GetType() != typeof(ActTableElement) && 
                a.GetType() != typeof(ActLaunchJavaWSApplication) && a.GetType() != typeof(ActJavaEXE) && 
                a.GetType() != typeof(ActGenElement) && a.GetType() != typeof(ActScript) && a.GetType() != typeof(ActConsoleCommand))
            {
                if (a.InputValues.Count > minimumInputValuesToHideGrid)
                {
                    ValueGridPanel.Visibility = Visibility.Visible;
                    ValueBoxPanel.Visibility = Visibility.Collapsed;
                }
                else if (a.InputValues.Count == minimumInputValuesToHideGrid)
                {
                    
                    ValueGridPanel.Visibility = Visibility.Collapsed;
                    ValueBoxPanel.Visibility = Visibility.Visible;
                    ActInputValue inputValue = a.InputValues.Where(x => x.Param == "Value").FirstOrDefault();
                    if (inputValue!=null)
                    {                      
                        ValueUC.Init(mContext, inputValue, nameof(ActInputValue.Value));
                        ValueUC.ValueTextBox.Text = inputValue.Value;
                        ValueLabel.Content = inputValue.Param;
                    }                    
                }
                else
                {
                    ValueGridPanel.Visibility = Visibility.Collapsed;
                    ValueBoxPanel.Visibility = Visibility.Visible;
                    a.Value = "";
                    ActInputValue inputValue = a.InputValues.Where(x => x.Param == "Value").FirstOrDefault();
                    ValueUC.Init(Context.GetAsContext(a.Context), inputValue, nameof(ActInputValue.Value));
                }
            }
            else if (a.GetType() == typeof(ActGenElement)|| a.GetType() == typeof(ActTableElement))
            {

                ActInputValue inputValue = a.InputValues.Where(x => x.Param == "Value").FirstOrDefault();

                if(inputValue==null)
                {
                    a.AddOrUpdateInputParamValue("Value", "");
                    inputValue= a.InputValues.Where(x => x.Param == "Value").FirstOrDefault();
                }
                ValueGridPanel.Visibility = Visibility.Collapsed;
                ValueBoxPanel.Visibility = Visibility.Visible;
                ValueUC.Init(Context.GetAsContext(a.Context), inputValue, nameof(ActInputValue.Value));

            }
            else if (a.GetType() == typeof(ActLaunchJavaWSApplication) || a.GetType() == typeof(ActJavaEXE))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count <= 1)
                {
                    ValueGridPanel.Visibility = Visibility.Collapsed;
                    ValueBoxPanel.Visibility = Visibility.Collapsed;
                }
                else if (a.InputValues.Count >= 2)
                {
                    ValueGridPanel.Visibility = Visibility.Visible;
                    ValueBoxPanel.Visibility = Visibility.Collapsed;
                }
            }    
            else if (a.GetType() == typeof(ActDBValidation))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count == 1)
                {
                    ValueUC.Init(mContext, mAction.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                    ValueGridPanel.Visibility = Visibility.Collapsed;
                    ValueBoxPanel.Visibility = Visibility.Collapsed;
                    ValueUC.ValueTextBox.Text = mAction.InputValues.FirstOrDefault().Value;
                    ValueLabel.Content = a.InputValues.FirstOrDefault().Param;
                }
                else
                {
                    ValueGridPanel.Visibility = Visibility.Collapsed;
                    ValueBoxPanel.Visibility = Visibility.Collapsed;
                    ValueUC.Init(mContext, a.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                }
            }
            else if (a.GetType() == typeof(ActScript))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count > 1)
                {
                    ValueGridPanel.Visibility = Visibility.Visible;
                    ValueBoxPanel.Visibility = Visibility.Collapsed;
                }
                else if (a.InputValues.Count == 1)
                {
                    ValueUC.Init(mContext, mAction.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                    ValueGridPanel.Visibility = Visibility.Collapsed;
                    ValueBoxPanel.Visibility = Visibility.Visible;
                    ValueUC.ValueTextBox.Text = a.InputValues.FirstOrDefault().Value;
                    ValueLabel.Content = a.InputValues.FirstOrDefault().Param;
                }
            }
            else if (a.GetType() == typeof(ActConsoleCommand))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count == 1)
                {   
                    ValueGridPanel.Visibility = Visibility.Collapsed;
                    ValueBoxPanel.Visibility = Visibility.Visible;
                    ValueUC.Init(mContext, mAction.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                    ValueUC.ValueTextBox.Text = a.InputValues.FirstOrDefault().Value;
                    ValueLabel.Content = a.InputValues.FirstOrDefault().Param;
                }
                else if (a.InputValues.Count > 1)
                {
                    ValueGridPanel.Visibility = Visibility.Visible;
                    ValueBoxPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ValueGridPanel.Visibility = Visibility.Collapsed;
                    ValueBoxPanel.Visibility = Visibility.Collapsed;                    
                }
            }           
        }

        private void AddReturnValue(object sender, RoutedEventArgs e)
        {
            mAction.ReturnValues.Add(new ActReturnValue() { Active = true ,Operator=eOperator.Equals});
        }

        private void AddInputValue(object sender, RoutedEventArgs e)
        {
            mAction.InputValues.Add(new ActInputValue() { Param = "p" + mAction.InputValues.Count });
        }

        private void SetActDataSourceConfigGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = ActOutDataSourceConfig.Fields.Active, WidthWeight = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            viewCols.Add(new GridColView() { Field = ActOutDataSourceConfig.Fields.OutputType, Header = "Output Type", WidthWeight = 150, ReadOnly = true });
            viewCols.Add(new GridColView() { Field = ActOutDataSourceConfig.Fields.TableColumn, Header = "Table Column", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(ActOutDataSourceConfig.Fields.PossibleValues, ActOutDataSourceConfig.Fields.TableColumn) });
            if (DataSourceConfigGrid.SelectedViewName != null && DataSourceConfigGrid.SelectedViewName != "")
                DataSourceConfigGrid.updateAndSelectCustomView(view);
            else
                DataSourceConfigGrid.SetAllColumnsDefaultView(view);
            DataSourceConfigGrid.InitViewItems();
            DataSourceConfigGrid.SetTitleLightStyle = true;
        }

        private void SetActReturnValuesGrid()
        {
            GridViewDef SimView = new GridViewDef(eGridView.All.ToString());
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            SimView.GridColsView = viewCols;

            //Simulation view
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Active, WidthWeight = 50, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Param, Header = "Parameter", WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = "..", Header = "...", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ParamValueExpressionButton"] });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Path, WidthWeight = 100 });
            viewCols.Add(new GridColView() { Field = "...", WidthWeight = 30, MaxWidth = 30, Header = "...", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["PathValueExpressionButton"] });

            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.SimulatedActual, Header = "Simulated Value", WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = "....", Header = "...", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["SimulatedlValueExpressionButton"] });
            viewCols.Add(new GridColView() { Field = "<<", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["AddActualToSimulButton"] });

            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Actual, Header = "Actual Value", WidthWeight = 150, BindingMode = BindingMode.OneWay });
            viewCols.Add(new GridColView() { Field = ">>", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["AddActualToExpectButton"] });

            viewCols.Add(new GridColView() { Field = nameof(ActReturnValue.Operator), Header = "Operator", WidthWeight = 150, BindingMode = BindingMode.TwoWay, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = OperatorList });
            // viewCols.Add(new GridColView() { Field = ">>", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["AddActualToExpectButton"] });


            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Expected, Header = "Expected Value", WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = ".....", Header = "...", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ValueExpressionButton"] });
            viewCols.Add(new GridColView() { Field = "Clear Expected Value", Header = "X", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ClearExpectedValueBtnTemplate"] });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.ExpectedCalculated, Header = "Calculated Expected", WidthWeight = 150, BindingMode = BindingMode.OneWay });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Status, WidthWeight = 70, MaxWidth = 70, BindingMode = BindingMode.OneWay, PropertyConverter = (new ColumnPropertyConverter(new ActReturnValueStatusConverter(), TextBlock.ForegroundProperty)) });

            List<String> varsCollc;
            if (mActParentBusinessFlow != null)
            {
                varsCollc = mActParentBusinessFlow.GetAllVariables(mActParentActivity).Where(a => a.VariableType == "String").Select(a => a.Name).ToList();
            }
            else
            {
                varsCollc = WorkSpace.Instance.Solution.Variables.Where(a => a.VariableType == "String").Select(a => a.Name).ToList();
                if (mActParentActivity != null)
                {
                    foreach (GingerCore.Variables.VariableBase var in mActParentActivity.Variables)
                    {
                        varsCollc.Add(var.Name);
                    }
                }
            }
            varsCollc.Sort();
            if (varsCollc.Count > 0)
                varsCollc.Insert(0, string.Empty);//to be used for clearing selection

            ObservableList<GlobalAppModelParameter> appsModelsGlobalParamsList = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();

            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.StoreToValue, Header = "Store To ", WidthWeight = 300, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetStoreToTemplate(ActReturnValue.Fields.StoreTo, ActReturnValue.Fields.StoreToValue, varsCollc, mAppGlobalParamList: appsModelsGlobalParamsList) });

            //Default mode view
            GridViewDef defView = new GridViewDef(eGridView.NonSimulation.ToString());
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = ActReturnValue.Fields.SimulatedActual, Visible = false });
            defView.GridColsView.Add(new GridColView() { Field = "....", Visible = false });
            defView.GridColsView.Add(new GridColView() { Field = "<<", Visible = false });

            OutputValuesGrid.SetAllColumnsDefaultView(SimView);
            OutputValuesGrid.AddCustomView(defView);
            OutputValuesGrid.InitViewItems();

            if (mAction.SupportSimulation == true)
                OutputValuesGrid.ChangeGridView(eGridView.All.ToString());
            else
                OutputValuesGrid.ChangeGridView(eGridView.NonSimulation.ToString());

            OutputValuesGrid.ShowViewCombo = Visibility.Collapsed;
            OutputValuesGrid.ShowEdit = Visibility.Collapsed;
                    }

        private void SetActInputValuesGrid()
        {
            //Show/hide if needed
            InputValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddInputValue));//?? going to be hide in next line code

            InputValuesGrid.SetTitleLightStyle = true;
            InputValuesGrid.ClearTools();
            InputValuesGrid.ShowDelete = System.Windows.Visibility.Visible;

            //List<GridColView> view = new List<GridColView>();
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["InputValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Value ForDriver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            InputValuesGrid.SetAllColumnsDefaultView(view);
            InputValuesGrid.InitViewItems();
        }
        
        private void LoadActionInfoPage(Act a)
        {
            //Each Action need to implement ActionEditPage which return the name of the page for edit
            //TODO: check all action are working and showing the correct Edit Page

            if (a.ActionEditPage != null)
            {
                string classname = "Ginger.Actions." + a.ActionEditPage;
                Type t = Assembly.GetExecutingAssembly().GetType(classname);
                if (t == null)
                {
                    throw new Exception("Action edit page not found - " + classname);
                }
                Page p = (Page)Activator.CreateInstance(t, a);
                if (p != null)
                {
                    // For no driver actions we give the BF and env - used for example in set var value.
                    if (typeof(ActWithoutDriver).IsAssignableFrom(a.GetType()))
                    {
                        if (mContext != null && mContext.Runner != null)
                        {
                            ((ActWithoutDriver)a).RunOnBusinessFlow = (BusinessFlow)mContext.Runner.CurrentBusinessFlow;
                            ((ActWithoutDriver)a).RunOnEnvironment = (ProjEnvironment)mContext.Runner.ProjEnvironment;
                            ((ActWithoutDriver)a).DSList = mContext.Runner.DSList;
                        }
                    }

                    // Load the page
                    ActionPrivateConfigsFrame.SetContent(p);
                    ActionPrivateConfigsPanel.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                ActionPrivateConfigsPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        
        //private void NextActionButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

        //    if (ap == null)
        //    {
        //        Reporter.ToUser(eUserMsgKey.CurrentActionNotSaved);
        //    }
        //    else if (ap.grdActions.grdMain.Items.CurrentPosition < ap.grdActions.grdMain.Items.Count - 1)
        //    {
        //        ap.grdActions.grdMain.Items.MoveCurrentToNext();
        //        Act tempact = (Act)ap.grdActions.grdMain.Items.CurrentItem;
        //        if (tempact != null)
        //        {
        //            mAction = tempact;

        //            ActionEditPage actedit = new ActionEditPage(mAction);
        //            actedit.ap = ap;
        //            actedit.ShowAsWindow();
        //            _pageGenericWin.Close();
        //        }
        //    }
        //    else
        //    {
        //        Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Action to move to.");
        //    }

        //    Mouse.OverrideCursor = null;
        //}

        //private void PrevActionButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        //    if (ap == null)
        //    {
        //        Reporter.ToUser(eUserMsgKey.CurrentActionNotSaved);
        //    }
        //    else if (ap.grdActions.grdMain.Items.CurrentPosition >= 1)
        //    {
        //        ap.grdActions.grdMain.Items.MoveCurrentToPrevious();
        //        Act tempact = (Act)ap.grdActions.grdMain.Items.CurrentItem;
        //        if (tempact != null)
        //        {
        //            mAction = tempact;
        //            ActionEditPage actedit = new ActionEditPage(mAction);
        //            actedit.ap = ap;
        //            actedit.ShowAsWindow();
        //            _pageGenericWin.Close();
        //        }
        //    }
        //    else
        //    {
        //        Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Action to move to.");
        //    }

        //    Mouse.OverrideCursor = null;
        //}

        private async void RunActionInSimulationButton_Click(object sender, RoutedEventArgs e)
        {
            bool originalSimulationFlagValue = mContext.Runner.RunInSimulationMode;
            mContext.Runner.RunInSimulationMode = true;

            int res = await RunAction().ConfigureAwait(false);

            mContext.Runner.RunInSimulationMode = originalSimulationFlagValue;
        }

        private async void RunActionButton_Click(object sender, RoutedEventArgs e)
        {
            bool originalSimulationFlagValue = mContext.Runner.RunInSimulationMode;
            mContext.Runner.RunInSimulationMode = false;

            int res = await RunAction().ConfigureAwait(false);

            mContext.Runner.RunInSimulationMode = originalSimulationFlagValue;
        }

        private void StopRunBtn_Click(object sender, RoutedEventArgs e)
        {
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.StopRun, null);
        }

        private void ShowHideRunStopButtons()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running)
                {
                    mRunActionBtn.Visibility = Visibility.Collapsed;
                    mStopRunBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    mRunActionBtn.Visibility = Visibility.Visible;
                    mStopRunBtn.Visibility = Visibility.Collapsed;
                }
            });
        }

        private async Task<int> RunAction()
        {
            this.Dispatcher.Invoke(() =>
            {
                mAction.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                if (mAction.GetType() == typeof(ActLowLevelClicks))
                    App.MainWindow.WindowState = WindowState.Minimized;
                mAction.IsSingleAction = true;

                App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.SetupRunnerForExecution, null);

                //No need for agent for some actions like DB and read for excel. For other need agent   
                if (!(typeof(ActWithoutDriver).IsAssignableFrom(mAction.GetType())))
                {
                    mContext.Runner.SetCurrentActivityAgent();
                }

                mContext.Runner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;
            });
            
            var result = await mContext.Runner.RunActionAsync(mAction, false, true).ConfigureAwait(false);

            this.Dispatcher.Invoke(() =>
            {
                mAction.IsSingleAction = false;
                UpdateTabsVisual();
                UpdateScreenShotPage();
                Mouse.OverrideCursor = null;
            });
            return result;
        }

        private void UpdatePassFailImages()
        {
            PassImage.Visibility = System.Windows.Visibility.Collapsed;
            FailImage.Visibility = System.Windows.Visibility.Collapsed;
            WarningImage.Visibility = System.Windows.Visibility.Collapsed;
            ErrorTextBlock.Visibility = System.Windows.Visibility.Collapsed;
            ExtraInfoTextBlock.Visibility = System.Windows.Visibility.Collapsed;
            if (mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
            {
                PassImage.Visibility = System.Windows.Visibility.Visible;
                ExtraInfoTextBlock.Visibility = System.Windows.Visibility.Visible;
            }
            else if (mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
            {
                FailImage.Visibility = System.Windows.Visibility.Visible;
                ErrorTextBlock.Visibility = System.Windows.Visibility.Visible;
                ExtraInfoTextBlock.Visibility = System.Windows.Visibility.Visible;
            }
            else if (mAction.Status != null)
            {
                WarningImage.Visibility = System.Windows.Visibility.Visible;
                ExtraInfoTextBlock.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void GridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)OutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.Expected, mContext);
            VEEW.ShowAsWindow();
        }
        private void InputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)InputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), mContext);
            VEEW.ShowAsWindow();
        }
        private void GridAddActualToExpectButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)OutputValuesGrid.CurrentItem;
            ARV.Expected = ARV.Actual;
        }

        private void GridAddActualToSimulButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)OutputValuesGrid.CurrentItem;
            ARV.SimulatedActual = ARV.Actual;
        }

        private void SimulatedOutputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)OutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.SimulatedActual, mContext);
            VEEW.ShowAsWindow();
        }

        private string RemoveActionWord(string str)
        {
            return str.Replace("Action", "").Trim();
        }


        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free, bool startupLocationWithOffset = false)
        {
            //changed the style to be free - since many other windows get stuck and doesn't show
            // Need to find a solution if 2 windows show as Dialog...
            string title = "Edit " + RemoveActionWord(mAction.ActionDescription) + " Action";

            RoutedEventHandler closeHandler = CloseWinClicked;
            string closeContent = "Undo & Close";

            ObservableList<Button> winButtons = new ObservableList<Button>();
            Button okBtn = new Button();
            okBtn.Content = "Ok";
            okBtn.Click += new RoutedEventHandler(okBtn_Click);
            Button undoBtn = new Button();
            undoBtn.Content = "Undo & Close";
            undoBtn.Click += new RoutedEventHandler(undoBtn_Click);
            Button saveBtn = new Button();
            saveBtn.Content = "Save";
            switch (EditMode)
            {
                case General.eRIPageViewMode.Automation:                   
                    winButtons.Add(okBtn);                    
                    winButtons.Add(undoBtn);

                    //Button nextAction = new Button();
                    //nextAction.Content = "Next Action";
                    //nextAction.Click += new RoutedEventHandler(NextActionButton_Click);
                    //nextAction.Margin = new Thickness(0, 0, 60, 0);
                    //winButtons.Add(nextAction);
                    //Button prevAction = new Button();
                    //prevAction.Content = "Previous Action";
                    //prevAction.Click += new RoutedEventHandler(PrevActionButton_Click);
                    //winButtons.Add(prevAction);

                    mRunActionBtn.Content = "Run";
                    mRunActionBtn.Click += new RoutedEventHandler(RunActionButton_Click);
                    mRunActionBtn.Margin = new Thickness(0, 0, 60, 0);
                    winButtons.Add(mRunActionBtn);
                    mSimulateRunBtn.Content = "Simulate Run";
                    mSimulateRunBtn.Click += new RoutedEventHandler(RunActionInSimulationButton_Click);
                    ShowHideRunSimulation();
                    winButtons.Add(mSimulateRunBtn);

                    mStopRunBtn.Content = "Stop";
                    mStopRunBtn.Click += new RoutedEventHandler(StopRunBtn_Click);
                    mStopRunBtn.Margin = new Thickness(0, 0, 60, 0);
                    winButtons.Add(mStopRunBtn);
                    mStopRunBtn.Visibility = Visibility.Collapsed;
                    break;


                case General.eRIPageViewMode.SharedReposiotry:
                    title = "Edit Shared Repository " + RemoveActionWord(mAction.ActionDescription) + " Action";                   
                    saveBtn.Click += new RoutedEventHandler(SharedRepoSaveBtn_Click);
                    winButtons.Add(saveBtn);                    
                    winButtons.Add(undoBtn);
                    break;

                case General.eRIPageViewMode.Child:
                    title = "Edit " + RemoveActionWord(mAction.ActionDescription) + " Action";
                    winButtons.Add(okBtn);
                    winButtons.Add(undoBtn);
                    break;

                case General.eRIPageViewMode.ChildWithSave:
                    title = "Edit " + RemoveActionWord(mAction.ActionDescription) + " Action";
                    saveBtn.Click += new RoutedEventHandler(ParentSaveButton_Click);
                    winButtons.Add(saveBtn);
                    winButtons.Add(undoBtn);
                    break;

                case General.eRIPageViewMode.View:
                    title = "View " + RemoveActionWord(mAction.ActionDescription) + " Action";
                    winButtons.Add(okBtn);
                    closeHandler = new RoutedEventHandler(okBtn_Click);
                    closeContent = okBtn.Content.ToString();
                    break;
            }

            this.Height = 800;
            this.Width = 1000;
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, closeContent, closeHandler, startupLocationWithOffset: startupLocationWithOffset);
            SwitchingInputValueBoxAndGrid(mAction);
            return saveWasDone;
        }

        private void SetViewMode()
        {
            RetryMechanismStackPanel.IsEnabled = false;
            DataSourceConfigGrid.ToolsTray.Visibility = Visibility.Collapsed;
            DataSourceConfigGrid.DisableGridColoumns();
            ConfigDS.IsEnabled = false;
            OutputsToDsConfig.IsEnabled = false;
            OutputValuesGrid.ToolsTray.Visibility = Visibility.Collapsed;
            OutputValuesGrid.DisableGridColoumns();
            xScreenshotsSetting.IsEnabled = false;
            xActionRunDetails.IsEnabled = false;
            xActionsDetails.IsEnabled = false;
            xActionConfiguration.IsEnabled = false;
        }

        private void UndoChangesAndClose()
        {
            IsPageClosing = true;
            
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                mAction.RestoreFromBackup(true);
                _pageGenericWin.Close();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.AskIfToUndoChanges) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                UndoChangesAndClose();
            }
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            // cause prop change so grid will refresh the data
            mAction.OnPropertyChanged(Act.Fields.Details);
            mAction.OnPropertyChanged(Act.Fields.FlowControls);
            IsPageClosing = true;
            _pageGenericWin.Close();
        }
        //public void UpdateGrid()
        //{
        //    Act currentact = (Act)ap.grdActions.grdMain.Items.CurrentItem;
        //    if (currentact != null)
        //    {
        //        mAction = currentact;
        //        ap.UpdateActionGrid();
        //    }
        //}

        private void SharedRepoSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckIfUserWantToSave();
        }

        private void ParentSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if ((mActParentBusinessFlow != null && Reporter.ToUser(eUserMsgKey.SaveItemParentWarning, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow),mActParentBusinessFlow.Name) == Amdocs.Ginger.Common.eUserMsgSelection.Yes) 
                || (mActParentActivity != null && Reporter.ToUser(eUserMsgKey.SaveItemParentWarning, GingerDicser.GetTermResValue(eTermResKey.Activity), mActParentActivity.ActivityName) == Amdocs.Ginger.Common.eUserMsgSelection.Yes))
            {
                if(mActParentBusinessFlow != null)                    
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mActParentBusinessFlow);
                else
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mActParentActivity);
                
                saveWasDone = true;
            }

            IsPageClosing = true;
            _pageGenericWin.Close();
        }
        private void CheckIfUserWantToSave()
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mAction, "change") == true)
            {
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mAction);
                saveWasDone = true;
                IsPageClosing = true;
                _pageGenericWin.Close();
            }
        }

        private void cboLocateBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboLocateBy.SelectedItem.ToString() == "ByMulitpleProperties")
            {
                sMultiLocatorVals = txtLocateValue.ValueTextBox.Text;
                txtLocateValue.Background = System.Windows.Media.Brushes.LightGray;
                btnEditLocator.Width = 30;
                if (mAction.LocateBy != eLocateBy.ByMulitpleProperties)
                {
                    EditLocatorsWindow.sMultiLocatorVals = txtLocateValue.ValueTextBox.Text;
                    EditLocatorsWindow ELW = new EditLocatorsWindow(mActParentBusinessFlow);
                    ELW.ShowDialog();
                    txtLocateValue.ValueTextBox.Text = EditLocatorsWindow.sMultiLocatorVals;
                }
            }
            else
            {
                txtLocateValue.Background = System.Windows.Media.Brushes.White;
                btnEditLocator.Width = 0;
            }
        }

        private void btnEditLocator_Click(object sender, RoutedEventArgs e)
        {
            EditLocatorsWindow.sMultiLocatorVals = txtLocateValue.ValueTextBox.Text;
            EditLocatorsWindow ELW = new EditLocatorsWindow(mActParentBusinessFlow);
            ELW.ShowDialog();
            txtLocateValue.ValueTextBox.Text = EditLocatorsWindow.sMultiLocatorVals;
        }

        private void GridParamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)OutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.Param, mContext);
            VEEW.ShowAsWindow();
        }
        private void GridPathVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)OutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.Path, mContext);
            VEEW.ShowAsWindow();
        }

        private void actionInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            ActDescriptionPage desPage = new ActDescriptionPage(mAction);
            desPage.ShowAsWindow();
        }

        private void ActionTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set the selected tab text style
            try
            {
                if (ActionTab.SelectedItem != null)
                {
                    foreach (TabItem tab in ActionTab.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (ActionTab.SelectedItem == tab)
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$Color_DarkBlue");

                                ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Action Edit Page tabs style", ex);
            }

            if (ActionTab.SelectedItem == ScreenShotTab)
            {
                UpdateScreenShotPage();
                return;
            }
            else if (ActionTab.SelectedItem == HelpTab)
            {
                General.ShowGingerHelpWindow(mAction.ActionDescription);
            }
        }

        private void UpdateScreenShotPage()
        {

            ScreenShotsGrid.Children.Clear();
            ScreenShotsGrid.RowDefinitions.Clear();
            ScreenShotsGrid.ColumnDefinitions.Clear();

            if (!mAction.TakeScreenShot && mAction.ScreenShots.Count == 0)  // keep count check for visual testing
            {
                Label lbl = new Label();
                lbl.Content = "To take screen shot(s) after this action is executed select Take Screen Shot check box in General tab";
                lbl.FontSize = 16;
                ScreenShotsGrid.Children.Add(lbl);
                return;
            }

            if (mAction.TakeScreenShot && mAction.ScreenShots.Count == 0)
            {
                Label lbl = new Label();
                lbl.Content = "Run the action to see Screen shot(s)";
                lbl.FontSize = 16;
                ScreenShotsGrid.Children.Add(lbl);
                return;
            }

            if (mAction.ScreenShots.Count > 0)
            {
                // create grid row cols based on screen shots count, can be 1x1, 2x2, 3x3 etc.. 
                int rowcount = 1;
                int colsPerRow = 1;
                while (rowcount * colsPerRow < mAction.ScreenShots.Count)
                {
                    if (rowcount < colsPerRow)
                        rowcount++;    // enable 1 row 2 columns, 2x3, 3x4 etc.. - avoid showing empty row
                    else
                        colsPerRow++;
                    // we can limit cols if we want for example max 3 per row, and then the grid will have vertical scroll bar
                }

                for (int rows = 0; rows < rowcount; rows++)
                {
                    RowDefinition rf = new RowDefinition() { Height = new GridLength(50, GridUnitType.Star) };
                    ScreenShotsGrid.RowDefinitions.Add(rf);
                }

                for (int cols = 0; cols < colsPerRow; cols++)
                {
                    ColumnDefinition cf = new ColumnDefinition() { Width = new GridLength(50, GridUnitType.Star) };
                    ScreenShotsGrid.ColumnDefinitions.Add(cf);
                }

                // loop through the screen shot and create new frame per each to show and place in the grid

                int r = 0;
                int c = 0;

                for (int i = 0; i < mAction.ScreenShots.Count; i++)
                {
                    //TODO: clean me when Screenshots changed to class instead of list of strings
                    // just in case we don't have name, TOOD: fix all places where we add screen shots to include name
                    string Name = "";
                    if (mAction.ScreenShotsNames.Count > i)
                    {
                        Name = mAction.ScreenShotsNames[i];
                    }
                    ScreenShotViewPage p = new ScreenShotViewPage(Name, mAction.ScreenShots[i]);
                    Frame f = new Frame();
                    Grid.SetRow(f, r);
                    Grid.SetColumn(f, c);
                    f.HorizontalAlignment = HorizontalAlignment.Center;
                    f.VerticalAlignment = VerticalAlignment.Center;
                    f.SetContent(p);
                    ScreenShotsGrid.Children.Add(f);

                    c++;
                    if (c == colsPerRow)
                    {
                        c = 0;
                        r++;
                    }
                }
            }
        }

        private void HighLightElementButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: fixme - Currently working with first agent
            ApplicationAgent aa =(ApplicationAgent)mContext.Runner.ApplicationAgents[0];
            if (aa != null)
            {
                DriverBase driver =((Agent) aa.Agent).Driver;
                mContext.Runner.PrepActionValueExpression(mAction);
                if (driver != null)
                {
                    driver.HighlightActElement(mAction);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.MissingImplementation2);
            }
        }

        private void ControlSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationAgent aa =(ApplicationAgent)mContext.Runner.ApplicationAgents.Where(x => x.AppName == mActParentActivity.TargetApplication).FirstOrDefault();
            if (aa != null)
            {
                if (((Agent)aa.Agent).Driver == null)
                {
                    ((Agent)aa.Agent).DSList = mDSList;
                    ((Agent)aa.Agent).StartDriver();
                }
                DriverBase driver = ((Agent)aa.Agent).Driver;
                //Instead of check make it disabled ?
                if (driver is IWindowExplorer)
                {
                    WindowExplorerPage WEP = new WindowExplorerPage(aa, mContext,  mAction);
                    WEP.ShowAsWindow();
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Control selection is not available yet for driver - " + driver.GetType().ToString());
                }
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void txtTimeout_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtTimeout.Text == String.Empty || txtTimeout.Text == null)
            {
                txtTimeout.Text = "0";
                txtTimeout.CaretIndex = 1;
            }
        }

        private void TakeScreenShot_Checked(object sender, RoutedEventArgs e)
        {
            TakeScreenshotsConfigs.IsEnabled = true;
            UpdateScreenshotTabVisual();
        }

        private void TakeScreenShot_UnChecked(object sender, RoutedEventArgs e)
        {
            TakeScreenshotsConfigs.IsEnabled = false;
            UpdateScreenshotTabVisual();
        }

        void UpdateTabsVisual()
        {
            UpdateRetryMechanismTabVisual();
            UpdateFlowControlTabVisual();
            UpdateOutputTabVisual();
            UpdateScreenshotTabVisual();
        }

        //Output Tab
        void UpdateOutputTabVisual()
        {
            this.Dispatcher.Invoke(() =>
            {
                int count = mAction.ReturnValues.Count();
                if (count > 0)
                {
                    SetTabOnOffSign(OutputTab, true);
                    OutputCount.Text = "(" + count + ")";
                }
                else
                {
                    SetTabOnOffSign(OutputTab, false);
                    OutputCount.Text = "";
                }

                if (mAction.ConfigOutputDS)
                    SetTabOnOffSign(OutputTab, true);
            });
        }

        public class ActReturnValueStatusConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                    System.Globalization.CultureInfo culture)
            {
                string status = value.ToString();
                if (status.Equals(ActReturnValue.eStatus.Passed.ToString())) return System.Windows.Media.Brushes.Green;//System.Drawing.Brushes.Green;
                if (status.Equals(ActReturnValue.eStatus.Failed.ToString())) return System.Windows.Media.Brushes.Red;
                if (status.Equals(ActReturnValue.eStatus.Pending.ToString())) return System.Windows.Media.Brushes.Orange;
                if (status.Equals(ActReturnValue.eStatus.Skipped.ToString())) return System.Windows.Media.Brushes.Black;

                return System.Drawing.Brushes.Gray;
            }

            public object ConvertBack(object value, Type targetType,
                object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        void UpdateScreenshotTabVisual()
        {
            this.Dispatcher.Invoke(() =>
            {
                SetTabOnOffSign(ScreenShotTab, mAction.TakeScreenShot);

                int count = mAction.ScreenShots.Count;
                if (count > 0)
                {
                    ScreenShotCount.Text = "(" + count + ")";
                }
                else
                {
                    ScreenShotCount.Text = "";
                }
            });
        }

        // Retry Tab
        private void UpdateRetryMechanismTabVisual()
        {
            SetTabOnOffSign(RetyrMechainsmTab, mAction.EnableRetryMechanism);

            if (mAction.EnableRetryMechanism)
                RetryMechConfigsPnl.IsEnabled = true;
            else
                RetryMechConfigsPnl.IsEnabled = false;
        }


        private void UpdateFlowControlTabVisual()
        {
            this.Dispatcher.Invoke(() =>
            {
                bool b = mAction.FlowControls.Count() > 0;
                SetTabOnOffSign(FlowControlTab, b);
                if (b)
                {
                    FlowControlCountLabel.Text = "(" + mAction.FlowControls.Count() + ")";
                }
                else
                {
                    FlowControlCountLabel.Text = "";
                }
            });
        }

        private void UpdateHelpTab()
        {
            ActDescriptionPage desPage = new ActDescriptionPage(mAction);
            ActDescriptionFrm.SetContent(desPage);
        }

        void SetTabOnOffSign(TabItem tab, bool indicatorToShow)
        {
            try
            {
                //set the selected tab text style
                if (tab != null)
                {
                    foreach (object ctrl in ((StackPanel)(tab.Header)).Children)
                        if (ctrl.GetType() == typeof(System.Windows.Controls.Image))
                        {
                            System.Windows.Controls.Image img = (System.Windows.Controls.Image)ctrl;
                            if (img.Tag != null)
                            {
                                if (img.Tag.ToString() == "OffSignImage")
                                    if (indicatorToShow)
                                        img.Visibility = Visibility.Collapsed;
                                    else
                                        img.Visibility = Visibility.Visible;
                                else if (img.Tag.ToString() == "OnSignImage")
                                    if (indicatorToShow)
                                        img.Visibility = Visibility.Visible;
                                    else
                                        img.Visibility = Visibility.Collapsed;
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Action Edit Page tabs style", ex);
            }
        }

        private void EnableRetryMechanismCheckBox_CheckedUnChecked(object sender, RoutedEventArgs e)
        {
            UpdateRetryMechanismTabVisual();
        }

        private void GridDSVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)OutputValuesGrid.CurrentItem;
            ActDataSourcePage ADSP = new ActDataSourcePage(ARV, ActReturnValue.Fields.StoreToDataSource);
            ADSP.ShowAsWindow();
        }

        private void xDataSourceExpander_Expanded(object sender, RoutedEventArgs e)
        {
            SetDataSourceConfigTabView();
        }

        private void xDataSourceExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            DataSourceRow.Height = new GridLength(35);
        }

        private void AddOutDS_Checked(object sender, RoutedEventArgs e)
        {
            mDSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            if (mDSList.Count == 0)
                return;
            mDSNames.Clear();
            foreach (DataSourceBase ds in mDSList)
                mDSNames.Add(ds.Name);
            GingerCore.General.FillComboFromList(cmbDataSourceName, mDSNames);           
            if (mAction.OutDataSourceName != null && mAction.OutDataSourceTableName != null && mAction.OutDataSourceName != "" && mAction.OutDataSourceTableName != "")
            {
                cmbDataSourceName.SelectedValue = mAction.OutDataSourceName;
                cmbDataSourceTableName.SelectedValue = mAction.OutDataSourceTableName;
            }
            else
            {
                cmbDataSourceName.SelectedIndex = 0;
                mDataSourceName = mDSNames[0];
            }

            if (mAction.OutDSParamMapType == null || mAction.OutDSParamMapType == "")
            {
                dsOutputParamMapType.SelectedValue=Act.eOutputDSParamMapType.ParamToRow;
            }
            else
                dsOutputParamMapType.SelectedValue = mAction.OutDSParamMapType;

            SetDataSourceConfigTabView();

            UpdateOutputTabVisual();
        }

        private void AddOutDS_Unchecked(object sender, RoutedEventArgs e)
        {
            mAction.DSOutputConfigParams.Clear();            
            SetDataSourceConfigTabView();
            UpdateOutputTabVisual();
        }
        private void SetDataSourceConfigTabView()
        {
            if(xDataSourceExpander.IsExpanded)
            {
                if(AddOutDS.IsChecked == true)
                {
                        DataSourceRow.Height = new GridLength(270);
                }
                else
                {
                    DataSourceRow.Height = new GridLength(60);
                }
            }
            else
                DataSourceRow.Height = new GridLength(32);

            if (mAction.ConfigOutputDS)
            {
                OutputsToDsConfig.IsEnabled = true;
                DataSourceConfigGrid.IsEnabled = true;
            }
            else
            {
                OutputsToDsConfig.IsEnabled = false;
                DataSourceConfigGrid.IsEnabled = false;
            }        
        }

        private void updateDSOutGrid()
        {
            if (cmbDataSourceTableName == null || cmbDataSourceTableName.Items.Count == 0 || cmbDataSourceTableName.SelectedValue == null)
                return;            

            List<ActOutDataSourceConfig> DSConfigParam = mAction.DSOutputConfigParams.Where(x => x.DSName == mDataSourceName && x.DSTable == mDSTable.Name && x.OutParamMap == mAction.OutDSParamMapType).ToList();
            SetDataSourceConfigTabView();

            if (mDSTable.DSTableType == DataSourceTable.eDSTableType.Customized)
            {
                dsOutputParamMapType.IsEnabled = true;
                mColNames.Remove("GINGER_ID");
                if (mColNames.Contains("GINGER_LAST_UPDATED_BY"))
                    mColNames.Remove("GINGER_LAST_UPDATED_BY");
                if (mColNames.Contains("GINGER_LAST_UPDATE_DATETIME"))
                    mColNames.Remove("GINGER_LAST_UPDATE_DATETIME");
                if (mColNames.Contains("GINGER_USED"))
                    mColNames.Remove("GINGER_USED");

                if (mAction.OutDSParamMapType == Act.eOutputDSParamMapType.ParamToRow.ToString())
                {
                    mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter.ToString(), "Parameter", "", mColNames);
                    mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Path.ToString(), "Path", "", mColNames);
                    mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Actual.ToString(), "Actual", "", mColNames);
                }
                else
                {                    
                    foreach(ActOutDataSourceConfig oDSParam in DSConfigParam)
                    {
                        mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, oDSParam.OutputType, oDSParam.TableColumn, "", mColNames,mAction.OutDSParamMapType);
                    }
                }
            }
            else
            {                
                dsOutputParamMapType.IsEnabled = false;
                dsOutputParamMapType.SelectedValue = Act.eOutputDSParamMapType.ParamToRow;
                mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter.ToString(), "GINGER_KEY_NAME");
                mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter_Path.ToString(), "GINGER_KEY_NAME");
                mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Actual.ToString(), "GINGER_KEY_VALUE");

            }

            DSConfigParam = mAction.DSOutputConfigParams.Where(x => x.DSName == mDataSourceName && x.DSTable == mDSTable.Name && x.OutParamMap == mAction.OutDSParamMapType).ToList();
            aOutDSConfigParam.Clear();
            
            foreach (ActOutDataSourceConfig aOutDSConfig in DSConfigParam)
                aOutDSConfigParam.Add(aOutDSConfig);

            DataSourceConfigGrid.Visibility = Visibility.Visible;

            SetActDataSourceConfigGrid();
            mAction.DSOutputConfigParams = aOutDSConfigParam;
            DataSourceConfigGrid.DataSourceList = mAction.DSOutputConfigParams;
        }
        private void OutDSParamType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            updateDSOutGrid();
        }

        private void cmbDataSourceTableName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDataSourceTableName == null || cmbDataSourceTableName.Items.Count == 0 || cmbDataSourceTableName.SelectedValue == null)
                return;
            foreach (DataSourceTable dst in mDSTableList)
            {
                if (dst.Name == cmbDataSourceTableName.SelectedValue.ToString())
                {                    
                    mDSTable = dst;
                   
                    mColNames = mDSTable.DSC.GetColumnList(mDSTable.Name);
                    if (mAction.OutDSParamMapType == null)
                        mAction.OutDSParamMapType = Act.eOutputDSParamMapType.ParamToRow.ToString();
                    dsOutputParamMapType.SelectedValue = mAction.OutDSParamMapType;
                    updateDSOutGrid();
                    break;
                }               
            }
        }

        private void DataSourceConfigGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            if (mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
            {                
                if (e.OriginalSource.GetType() == typeof(CheckBox))
                {
                    DataGridCell cell = (DataGridCell)((CheckBox)e.OriginalSource).Parent;
                    ActOutDataSourceConfig currRow = (ActOutDataSourceConfig)cell.DataContext;                    
                    if (currRow.OutputType == "Actual")
                        mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Actual.ToString(), "GINGER_KEY_VALUE", "true");
                    else if(currRow.OutputType == "Parameter")
                        mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter_Path.ToString(), "GINGER_KEY_NAME", (!currRow.Active).ToString());
                    else
                        mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter.ToString(), "GINGER_KEY_NAME", (!currRow.Active).ToString());
                    DataSourceConfigGrid.DataSourceList = mAction.DSOutputConfigParams;
                }                
            }
        }               

        private void cmbDataSourceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDataSourceName == null || cmbDataSourceName.Items.Count == 0 || cmbDataSourceName.SelectedValue == null)
                return;
            foreach (DataSourceBase ds in mDSList)
            {
                if (ds.Name == cmbDataSourceName.SelectedValue.ToString())
                {
                    mDataSourceName = cmbDataSourceName.SelectedValue.ToString();
                    //if (ds.FilePath.StartsWith("~"))
                    //{
                    //    ds.FileFullPath = ds.FilePath.Replace(@"~\", "").Replace("~", "");
                    //    ds.FileFullPath = System.IO.Path.Combine( WorkSpace.Instance.Solution.Folder, ds.FileFullPath);
                    //}
                    ds.FileFullPath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(ds.FilePath);

                    List<string> dsTableNames = new List<string>();
                    mDSTableList.Clear();
                    mDSTableList = ds.GetTablesList();
                    foreach (DataSourceTable dst in mDSTableList)
                    {
                        dsTableNames.Add(dst.Name);
                        if (cmbDataSourceTableName.SelectedValue != null && cmbDataSourceTableName.SelectedValue.ToString() == dst.Name)
                            mDSTable = dst;
                    }
                                                
                    if (mDSTableList.Count == 0)
                        return;
                    GingerCore.General.FillComboFromList(cmbDataSourceTableName, dsTableNames);
                    if(cmbDataSourceTableName.SelectedValue == null)
                    {
                        mDSTable = mDSTableList[0];
                        cmbDataSourceTableName.SelectedIndex = 0;
                    }
                    break;
                }
            }
        }

        private void ShowHideRunSimulation()
        {
            if (mAction.SupportSimulation)
                mSimulateRunBtn.Visibility = Visibility.Visible;
            else
                mSimulateRunBtn.Visibility = Visibility.Collapsed;
        }

        private void ActionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {                       
            if (e.PropertyName == Act.Fields.SupportSimulation)
            {
                ShowHideRunSimulation();
            }
            else if (e.PropertyName == Act.Fields.Status)
            {
                PassImage.Dispatcher.Invoke(() =>
                {
                    UpdatePassFailImages();
                    ShowHideRunStopButtons();
                    xRunStatusExpander.IsExpanded = true;
                });
            }
        }

        private void xRunStatusExpander_Expanded(object sender, RoutedEventArgs e)
        {
            xRunStatusRow.Height = new GridLength(125);
        }

        private void xRunStatusExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            xRunStatusRow.Height = new GridLength(30);
        }

        private void InitActionLog()
        {
            if (mAction.EnableActionLogConfig)
            {
                ShowActionLogConfig();
            }            
        }

        private void ShowActionLogConfig()
        {
            if (mAction.ActionLogConfig == null)
            {
                mAction.ActionLogConfig = new ActionLogConfig();
            }                        
            ActionLogConfigFrame.SetContent(new ActionLogConfigPage(mAction.ActionLogConfig));
            ActionLogConfigExpander.IsExpanded = true;
        }

        private void EnableActionLogConfigCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mAction.EnableActionLogConfig = true;
            if (mAction.ActionLogConfig == null)
            {
                mAction.ActionLogConfig = new ActionLogConfig();
            }
            ResetActionLog();
            SetActionLogFrameView();
        }

        private void EnableActionLogConfigCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            mAction.EnableActionLogConfig = false;
            ResetActionLog();
            SetActionLogFrameView();
        }

        private void ActionLogConfigExpander_Expanded(object sender, RoutedEventArgs e)
        {
            SetActionLogFrameView();
        }

        private void ActionLogConfigExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ActionLogRow.Height = new GridLength(30);
        }

        private void ResetActionLog()
        {
            if (mAction.EnableActionLogConfig)
            {
                ShowActionLogConfig();
            }
            else
            {
                ActionLogConfigFrame.SetContent(null);
            }
        }

        private void SetActionLogFrameView()
        {
            if (ActionLogConfigExpander.IsExpanded)
            {
                if (EnableActionLogConfigCheckBox.IsChecked == true)
                {
                    ActionLogRow.Height = new GridLength(230);
                }
                else
                {
                    ActionLogRow.Height = new GridLength(60);
                }
            }
            else
                ActionLogRow.Height = new GridLength(32);

            if (mAction.EnableActionLogConfig)
            {
                ActionLogDetailsStackPanel.IsEnabled = true;
            }
            else
            {
                ActionLogDetailsStackPanel.IsEnabled = false;
            }
        }

        private void GridClearExpectedValueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (OutputValuesGrid.Grid.SelectedItem != null)
            {
                ((ActReturnValue)OutputValuesGrid.Grid.SelectedItem).Expected = null;
            }
        }

        public void ClearPageBindings()
        {
            StopEdit();
            BindingOperations.ClearAllBindings(txtDescription);
            BindingOperations.ClearAllBindings(cboLocateBy);
            BindingOperations.ClearAllBindings(comboWindowsToCapture);
            BindingOperations.ClearAllBindings(txtLocateValue);
            BindingOperations.ClearAllBindings(RTStatusLabel);
            BindingOperations.ClearAllBindings(RTElapsedLabel);
            BindingOperations.ClearAllBindings(RTErrorLabel);
            BindingOperations.ClearAllBindings(RTExInfoLabel);
            BindingOperations.ClearAllBindings(txtLocateValue);
            BindingOperations.ClearAllBindings(TakeScreenShotCheckBox);
            BindingOperations.ClearAllBindings(FailIgnoreCheckBox);
            BindingOperations.ClearAllBindings(comboFinalStatus);
            BindingOperations.ClearAllBindings(xWaittxtWait);
            BindingOperations.ClearAllBindings(txtTimeout);
            BindingOperations.ClearAllBindings(StatusLabel);
            BindingOperations.ClearAllBindings(ErrorTextBlock);
            BindingOperations.ClearAllBindings(ExtraInfoTextBlock);
            BindingOperations.ClearAllBindings(EnableRetryMechanismCheckBox);
            BindingOperations.ClearAllBindings(RetryMechanismIntervalTextBox);
            BindingOperations.ClearAllBindings(RetryMechanismMaxRetriesTextBox);
            AddOutDS.Unchecked -= AddOutDS_Unchecked;
            BindingOperations.ClearAllBindings(AddOutDS);
            BindingOperations.ClearAllBindings(cmbDataSourceName);
            BindingOperations.ClearAllBindings(cmbDataSourceTableName);
            BindingOperations.ClearAllBindings(dsOutputParamMapType);
            BindingOperations.ClearAllBindings(EnableActionLogConfigCheckBox);
            BindingOperations.ClearAllBindings(txtLocateValue);
            TagsViewer.ClearBinding();
            //this.ClearControlsBindings();
            if (mAction != null)
            {
                mAction.PropertyChanged -= ActionPropertyChanged;
                mAction.InputValues.CollectionChanged -= InputValues_CollectionChanged;
                mAction.FlowControls.CollectionChanged -= FlowControls_CollectionChanged;
                mAction.ReturnValues.CollectionChanged -= ReturnValues_CollectionChanged;
                mAction = null;
            }
            FlowControlFrame.NavigationService.RemoveBackEntry();
            ActionPrivateConfigsFrame.NavigationService.RemoveBackEntry();
            ActDescriptionFrm.NavigationService.RemoveBackEntry();
            ActionLogConfigFrame.NavigationService.RemoveBackEntry();
        }

        private void xActionDetailsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ActionDetailsRow.Height = new GridLength(220);
        }

        private void xActionDetailsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ActionDetailsRow.Height= new GridLength(30);
        }
    }
}
