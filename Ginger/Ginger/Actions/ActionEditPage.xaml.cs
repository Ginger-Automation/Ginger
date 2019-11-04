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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Expressions;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.BusinessFlowWindows;
using Ginger.Help;
using Ginger.Repository;
using Ginger.UserControls;
using Ginger.UserControlsLib.UCListView;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ginger.Actions
{
    enum eGridView { All, NonSimulation }

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

        private static readonly List<ComboEnumItem> OperatorList = GingerCore.General.GetEnumValuesForCombo(typeof(eOperator));

        ObservableList<DataSourceBase> mDSList = new ObservableList<DataSourceBase>();
        ObservableList<DataSourceTable> mDSTableList = new ObservableList<DataSourceTable>();
        List<string> mDSNames = new List<string>();
        private DataSourceTable mDSTable;
        private string mDataSourceName;
        List<String> mColNames = null;
        ObservableList<ActOutDataSourceConfig> aOutDSConfigParam = new ObservableList<ActOutDataSourceConfig>();
        ObservableList<String> mStoreToVarsList = new ObservableList<string>();

        private BusinessFlow mActParentBusinessFlow = null;
        private Activity mActParentActivity = null;

        Button mSimulateRunBtn = new Button();
        Button mRunActionBtn = new Button();
        Button mStopRunBtn = new Button();
        ComboBox dsOutputParamMapType = new ComboBox();
        private bool saveWasDone = false;
        ActionFlowControlPage mAFCP;
        Context mContext;

        public int SelectedTabIndx
        {
            get
            {
                return xActionTabs.SelectedIndex;
            }
            set
            {
                xActionTabs.SelectedIndex = value;
            }
        }

        public General.eRIPageViewMode EditMode { get; set; }

        public ActionEditPage(Act act, General.eRIPageViewMode editMode = General.eRIPageViewMode.Automation, BusinessFlow actParentBusinessFlow = null, Activity actParentActivity = null)
        {
            InitializeComponent();

            //ActionEditNum++;
            //LiveActionEditCounter++;

            EditMode = editMode;

            mAction = act;
            if (editMode != General.eRIPageViewMode.View)
            {
                mAction.SaveBackup();
            }
            mAction.PropertyChanged -= ActionPropertyChanged;
            mAction.PropertyChanged += ActionPropertyChanged;
            mAction.InputValues.CollectionChanged -= InputValues_CollectionChanged;
            mAction.InputValues.CollectionChanged += InputValues_CollectionChanged;
            mAction.FlowControls.CollectionChanged -= FlowControls_CollectionChanged;
            mAction.FlowControls.CollectionChanged += FlowControls_CollectionChanged;
            mAction.ReturnValues.CollectionChanged -= ReturnValues_CollectionChanged;
            mAction.ReturnValues.CollectionChanged += ReturnValues_CollectionChanged;
            mAction.ScreenShots.CollectionChanged -= ScreenShots_CollectionChanged;
            mAction.ScreenShots.CollectionChanged += ScreenShots_CollectionChanged;

            mContext = Context.GetAsContext(mAction.Context);
            if (mContext != null && mContext.Runner != null)
            {
                mContext.Runner.PrepActionValueExpression(mAction, actParentBusinessFlow);
            }

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

            GingerHelpProvider.SetHelpString(this, act.ActionDescription);

            InitView();
        }

        private void InitView()
        {
            UpdateTabsHeaders();

            //allowing return values automatically in Edit Action window
            if (mAction.AddNewReturnParams == null && mAction.ReturnValues.Count() == 0)
            {
                mAction.AddNewReturnParams = true;
            }

            if (EditMode == General.eRIPageViewMode.Automation || EditMode == General.eRIPageViewMode.View)
            {
                BindingHandler.ObjFieldBinding(xExecutionStatusTabImage, UcItemExecutionStatus.StatusProperty, mAction, nameof(Act.Status));
            }
            else
            {
                xExecutionStatusTabImage.Visibility = Visibility.Collapsed;
            }

            if (EditMode == General.eRIPageViewMode.View)
            {
                SetViewMode();
            }

            if ((EditMode == General.eRIPageViewMode.Automation || EditMode == General.eRIPageViewMode.View) &&
                       (mAction.Status != null && mAction.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending))
            {
                xActionTabs.SelectedItem = xExecutionReportTab;
            }
            else
            {
                xActionTabs.SelectedItem = xOperationSettingsTab;
            }
        }

        private void InitDetailsTabView()
        {
            xDetailsTab.Tag = true;//marking that binding was done

            BindingHandler.ObjFieldBinding(xTypeLbl, Label.ContentProperty, mAction, nameof(Act.ActionType), BindingMode: BindingMode.OneWay);
            xDescriptionTextBox.BindControl(mAction, nameof(Act.Description));
            xRunDescritpionUC.Init(mContext, mAction, nameof(Act.RunDescription));
            xTagsViewer.Init(mAction.Tags);

            if (EditMode == General.eRIPageViewMode.Automation)
            {
                xSharedRepoInstanceUC.Init(mAction, null);
            }
            else
            {
                xSharedRepoInstanceUC.Visibility = Visibility.Collapsed;
                xSharedRepoInstanceUCCol.Width = new GridLength(0);
            }
        }

        private void InitOperationSettingsTabView()
        {
            xOperationSettingsTab.Tag = true;//marking that bindings were done

            if (mAction.ObjectLocatorConfigsNeeded)
            {
                List<eLocateBy> locatorsTypeList = mAction.AvailableLocateBy().Where(e => e != eLocateBy.POMElement).ToList();
                xLocateByCombo.BindControl(mAction, nameof(Act.LocateBy), locatorsTypeList);
                xLocateValueVE.BindControl(mContext, mAction, nameof(Act.LocateValue));
                BindingHandler.ObjFieldBinding(xLocateValueVE, TextBox.ToolTipProperty, mAction, nameof(Act.LocateValue));
            }
            else
            {
                xActionLocatorPnl.Visibility = System.Windows.Visibility.Collapsed;
            }

            SwitchingInputValueBoxAndGrid(mAction);
            SetActInputValuesGrid();

            LoadOperationSettingsEditPage(mAction);
        }

        private void InitFlowControlTabView()
        {
            xFlowControlTab.Tag = true;//marking that bindings were done

            //Wait/Timeout
            xWaitVeUC.BindControl(mContext, mAction, nameof(Act.WaitVE));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xTimeoutTextBox, TextBox.TextProperty, mAction, nameof(Act.Timeout));

            //Retry
            if (mActParentActivity != null && mActParentActivity.GetType() == typeof(ErrorHandler))
            {
                xRetryExpander.Visibility = Visibility.Collapsed;
            }
            else
            {
                BindingHandler.ObjFieldBinding(xEnableRetryMechanismCheckBox, CheckBox.IsCheckedProperty, mAction, nameof(Act.EnableRetryMechanism));
                BindingHandler.ObjFieldBinding(xRetryMechanismIntervalTextBox, TextBox.TextProperty, mAction, nameof(Act.RetryMechanismInterval));
                BindingHandler.ObjFieldBinding(xRetryMechanismMaxRetriesTextBox, TextBox.TextProperty, mAction, nameof(Act.MaxNumberOfRetries));

                SetRetryMechConfigsPnlView();
            }

            //Flow Controls Conditions
            if (EditMode == General.eRIPageViewMode.View)
            {
                mAFCP = new ActionFlowControlPage(mAction, mActParentBusinessFlow, mActParentActivity, General.eRIPageViewMode.View);
            }
            else if (EditMode == General.eRIPageViewMode.SharedReposiotry)
            {
                mAFCP = new ActionFlowControlPage(mAction, mActParentBusinessFlow, mActParentActivity, General.eRIPageViewMode.SharedReposiotry);
            }
            else
            {
                mAFCP = new ActionFlowControlPage(mAction, mActParentBusinessFlow, mActParentActivity);
            }
            xFlowControlConditionsFrame.SetContent(mAFCP);

            if (mAction.FlowControls.Count > 0)
            {
                xFlowControlConditionsExpander.IsExpanded = true;
            }
        }

        private void InitOutputValuesTabView()
        {
            xOutputValuesTab.Tag = true;//marking that bindings were done

            //Outputs to Data Source
            dsOutputParamMapType = xDataSourceConfigGrid.AddComboBox(typeof(Act.eOutputDSParamMapType), "Out Param Mapping", "", new RoutedEventHandler(OutDSParamType_SelectionChanged));
            BindingHandler.ObjFieldBinding(xAddOutToDSCheckbox, CheckBox.IsCheckedProperty, mAction, nameof(Act.ConfigOutputDS));
            BindingHandler.ObjFieldBinding(xDataSourceNameCombo, ComboBox.TextProperty, mAction, nameof(Act.OutDataSourceName));
            BindingHandler.ObjFieldBinding(xDataSourceTableNameCombo, ComboBox.TextProperty, mAction, nameof(Act.OutDataSourceTableName));
            BindingHandler.ObjFieldBinding(dsOutputParamMapType, ComboBox.SelectedValueProperty, mAction, nameof(Act.OutDSParamMapType));
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
            mDSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            if (mDSList.Count == 0)
            {
                xAddOutToDSCheckbox.IsEnabled = false;
            }
            xDataSourceConfigGrid.LostFocus += DataSourceConfigGrid_LostFocus;

            //Output Values
           

            SetActReturnValuesGrid();

            if (mAction.ActReturnValues.Count > 0)
            {
                xOutputValuesExpander.IsExpanded = true;
            }

            if (EditMode == General.eRIPageViewMode.View)
            {
                xOutputValuesGrid.DisableGridColoumns();
            }
        }

        private void InitExecutionReportTabView()
        {
            xExecutionReportTab.Tag = true;//marking that bindings were done           

            //configs section
            xStatusConvertorCombo.BindControl(mAction, nameof(Act.StatusConverter));
            BindingHandler.ObjFieldBinding(xFailIgnoreCheckBox, CheckBox.IsCheckedProperty, mAction, nameof(Act.FailIgnored));

            BindingHandler.ObjFieldBinding(xEnableActionLogConfigCheckBox, CheckBox.IsCheckedProperty, mAction, nameof(Act.EnableActionLogConfig));
            InitActionLog();

            //execution details section
            if (EditMode == General.eRIPageViewMode.Automation || EditMode == General.eRIPageViewMode.View)
            {
                BindingHandler.ObjFieldBinding(xExecutionStatusImage, UcItemExecutionStatus.StatusProperty, mAction, nameof(Act.Status));
                BindingHandler.ObjFieldBinding(xExecutionStatusLabel, UcItemExecutionStatus.StatusProperty, mAction, nameof(Act.Status));

                BindingHandler.ObjFieldBinding(xExecutionTimeLbl, Label.ContentProperty, mAction, nameof(Act.ElapsedSecs), BindingMode.OneWay);

                BindingHandler.ObjFieldBinding(xExecutionExtraInfoText, TextBox.TextProperty, mAction, nameof(Act.ExInfo), BindingMode.OneWay);
                BindingHandler.ObjFieldBinding(xExecutionExtraInfoPnl, StackPanel.VisibilityProperty, mAction, nameof(Act.ExInfo), bindingConvertor: new StringVisibilityConverter(), BindingMode: BindingMode.OneWay);

                BindingHandler.ObjFieldBinding(xExecutionErrorDetailsText, TextBox.TextProperty, mAction, nameof(Act.Error), BindingMode.OneWay);
                BindingHandler.ObjFieldBinding(xExecutionErrorDetailsPnl, StackPanel.VisibilityProperty, mAction, nameof(Act.Error), bindingConvertor: new StringVisibilityConverter(), BindingMode: BindingMode.OneWay);

                if (mActParentActivity != null && mActParentActivity.GetType() == typeof(ErrorHandler))
                {
                    xScreenshotsConfigsPnl.Visibility = Visibility.Collapsed;
                    xScreenShotsPnl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    BindingHandler.ObjFieldBinding(xTakeScreenShotCheckBox, CheckBox.IsCheckedProperty, mAction, nameof(Act.TakeScreenShot));
                    xWindowsToCaptureCombo.BindControl(mAction, nameof(Act.WindowsToCapture));
                    SetScreenshotsPnlView();
                    UpdateScreenShots();
                }
            }
            else
            {
                xExecutionDetailsExpander.Visibility = Visibility.Collapsed;
            }
        }

        private void InitHelpTabView()
        {
            xHelpTab.Tag = true;//marking that bindings were done

            ActDescriptionPage desPage = new ActDescriptionPage(mAction);
            xActionHelpDetailsFram.SetContent(desPage);
        }

        private void ScreenShots_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                UpdateScreenShots();
            });
        }

        public void StopEdit()
        {
            if (mAFCP != null)
            {
                mAFCP.FlowControlGrid.Grid.CommitEdit();
                mAFCP.FlowControlGrid.Grid.CancelEdit();
            }
            if (xInputValuesGrid != null)
            {
                xInputValuesGrid.Grid.CommitEdit();
                xInputValuesGrid.Grid.CancelEdit();
            }
            if (xDataSourceConfigGrid != null)
            {
                xDataSourceConfigGrid.Grid.CommitEdit();
                xDataSourceConfigGrid.Grid.CancelEdit();
            }
            if (xOutputValuesGrid != null)
            {
                xOutputValuesGrid.Grid.CommitEdit();
                xOutputValuesGrid.Grid.CancelEdit();
            }
        }

        private void ReturnValues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateOutputValuesTabHeader();
            mAction.OnPropertyChanged(nameof(Act.ReturnValuesCount));
            this.Dispatcher.Invoke(() =>
            {
                if (mAction.ActReturnValues.Count > 0)
                {
                    xOutputValuesExpander.IsExpanded = true;
                }
            });
        }

        private void FlowControls_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateFlowControlsTabHeader();
            mAction.OnPropertyChanged(nameof(Act.FlowControlsInfo));
        }

        private void ClearUnusedParameter(object sender, RoutedEventArgs e)
        {
            mAction.ClearUnUsedReturnParams();
        }

        private void RefreshOutputColumns(object sender, RoutedEventArgs e)
        {
            if (mAction.SupportSimulation)
                xOutputValuesGrid.ChangeGridView(eGridView.All.ToString());
            else
                xOutputValuesGrid.ChangeGridView(eGridView.NonSimulation.ToString());
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
                    xInputValuesEditControlsPnl.Visibility = System.Windows.Visibility.Collapsed;
                });
                return;
            }

            //TODO: Remove all if else and handle it dynamically based on if Input value grid is needed or not
            int minimumInputValuesToHideGrid = 1;
            if (mAction.ObjectLocatorConfigsNeeded)
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
                    xInputValuesGrid.Visibility = Visibility.Visible;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
                else if (a.InputValues.Count == minimumInputValuesToHideGrid)
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Visible;
                    ActInputValue inputValue = a.InputValues.Where(x => x.Param == "Value").FirstOrDefault();
                    if (inputValue != null)
                    {
                        xValueVE.Init(mContext, inputValue, nameof(ActInputValue.Value));
                        xValueVE.ValueTextBox.Text = inputValue.Value;
                        xValueLbl.Content = inputValue.Param;
                    }
                }
                else
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Visible;
                    a.Value = "";
                    ActInputValue inputValue = a.InputValues.Where(x => x.Param == "Value").FirstOrDefault();
                    xValueVE.Init(Context.GetAsContext(a.Context), inputValue, nameof(ActInputValue.Value));
                }
            }
            else if (a.GetType() == typeof(ActGenElement) || a.GetType() == typeof(ActTableElement))
            {

                ActInputValue inputValue = a.InputValues.Where(x => x.Param == "Value").FirstOrDefault();

                if (inputValue == null)
                {
                    a.AddOrUpdateInputParamValue("Value", "");
                    inputValue = a.InputValues.Where(x => x.Param == "Value").FirstOrDefault();
                }
                xInputValuesGrid.Visibility = Visibility.Collapsed;
                xValueBoxPnl.Visibility = Visibility.Visible;
                xValueVE.Init(Context.GetAsContext(a.Context), inputValue, nameof(ActInputValue.Value));

            }
            else if (a.GetType() == typeof(ActLaunchJavaWSApplication) || a.GetType() == typeof(ActJavaEXE))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count <= 1)
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
                else if (a.InputValues.Count >= 2)
                {
                    xInputValuesGrid.Visibility = Visibility.Visible;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
            }
            else if (a.GetType() == typeof(ActDBValidation))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count == 1)
                {
                    xValueVE.Init(mContext, mAction.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                    xValueVE.ValueTextBox.Text = mAction.InputValues.FirstOrDefault().Value;
                    xValueLbl.Content = a.InputValues.FirstOrDefault().Param;
                }
                else
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                    xValueVE.Init(mContext, a.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                }
            }
            else if (a.GetType() == typeof(ActScript))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count > 1)
                {
                    xInputValuesGrid.Visibility = Visibility.Visible;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
                else if (a.InputValues.Count == 1)
                {
                    xValueVE.Init(mContext, mAction.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Visible;
                    xValueVE.ValueTextBox.Text = a.InputValues.FirstOrDefault().Value;
                    xValueLbl.Content = a.InputValues.FirstOrDefault().Param;
                }
            }
            else if (a.GetType() == typeof(ActConsoleCommand))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count == 1)
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Visible;
                    xValueVE.Init(mContext, mAction.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                    xValueVE.ValueTextBox.Text = a.InputValues.FirstOrDefault().Value;
                    xValueLbl.Content = a.InputValues.FirstOrDefault().Param;
                }
                else if (a.InputValues.Count > 1)
                {
                    xInputValuesGrid.Visibility = Visibility.Visible;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void AddReturnValue(object sender, RoutedEventArgs e)
        {
            mAction.ReturnValues.Add(new ActReturnValue() { Active = true, Operator = eOperator.Equals });
        }

        private void RefreshOutputValuesGridElements(object sender, RoutedEventArgs e)
        {
            //refresh Variabels StoreTo options
            GenerateStoreToVarsList();
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
            if (xDataSourceConfigGrid.SelectedViewName != null && xDataSourceConfigGrid.SelectedViewName != "")
                xDataSourceConfigGrid.updateAndSelectCustomView(view);
            else
                xDataSourceConfigGrid.SetAllColumnsDefaultView(view);
            xDataSourceConfigGrid.InitViewItems();
            xDataSourceConfigGrid.SetTitleLightStyle = true;
        }

        private void SetActReturnValuesGrid()
        {
            GridViewDef SimView = new GridViewDef(eGridView.All.ToString());
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            SimView.GridColsView = viewCols;

            //Simulation view
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Active, WidthWeight = 50, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Param, Header = "Parameter", WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = "..", Header = "...", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["ParamValueExpressionButton"] });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Path, WidthWeight = 100 });
            viewCols.Add(new GridColView() { Field = "...", WidthWeight = 30, MaxWidth = 30, Header = "...", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["PathValueExpressionButton"] });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.SimulatedActual, Header = "Simulated Value", WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = "....", Header = "...", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["SimulatedlValueExpressionButton"] });
            viewCols.Add(new GridColView() { Field = "<<", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["AddActualToSimulButton"] });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Actual, Header = "Actual Value", WidthWeight = 150, BindingMode = BindingMode.OneWay });
            viewCols.Add(new GridColView() { Field = ">>", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["AddActualToExpectButton"] });
            viewCols.Add(new GridColView() { Field = nameof(ActReturnValue.Operator), Header = "Operator", WidthWeight = 150, BindingMode = BindingMode.TwoWay, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = OperatorList });
            // viewCols.Add(new GridColView() { Field = ">>", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["AddActualToExpectButton"] });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Expected, Header = "Expected Value", WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = ".....", Header = "...", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["ValueExpressionButton"] });
            viewCols.Add(new GridColView() { Field = "Clear Expected Value", Header = "X", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["ClearExpectedValueBtnTemplate"] });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.ExpectedCalculated, Header = "Calculated Expected", WidthWeight = 150, BindingMode = BindingMode.OneWay });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Status, WidthWeight = 70, MaxWidth = 70, BindingMode = BindingMode.OneWay, PropertyConverter = (new ColumnPropertyConverter(new ActReturnValueStatusConverter(), TextBlock.ForegroundProperty)) });
            GenerateStoreToVarsList();
            ObservableList<GlobalAppModelParameter> appsModelsGlobalParamsList = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.StoreToValue, Header = "Store To ", WidthWeight = 300, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetStoreToTemplate(ActReturnValue.Fields.StoreTo, ActReturnValue.Fields.StoreToValue, mStoreToVarsList, mAppGlobalParamList: appsModelsGlobalParamsList) });

            //Default mode view
            GridViewDef defView = new GridViewDef(eGridView.NonSimulation.ToString());
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = ActReturnValue.Fields.SimulatedActual, Visible = false });
            defView.GridColsView.Add(new GridColView() { Field = "....", Visible = false });
            defView.GridColsView.Add(new GridColView() { Field = "<<", Visible = false });

            xOutputValuesGrid.SetAllColumnsDefaultView(SimView);
            xOutputValuesGrid.AddCustomView(defView);
            xOutputValuesGrid.InitViewItems();

            if (mAction.SupportSimulation == true)
                xOutputValuesGrid.ChangeGridView(eGridView.All.ToString());
            else
                xOutputValuesGrid.ChangeGridView(eGridView.NonSimulation.ToString());

            xOutputValuesGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshOutputValuesGridElements));
            xOutputValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddReturnValue));
            xOutputValuesGrid.AddSeparator();

            xOutputValuesGrid.AddToolbarTool(eImageType.Reset, "Clear Un-used Parameters", new RoutedEventHandler(ClearUnusedParameter), imageSize: 14);
            BindingHandler.ObjFieldBinding(xOutputValuesGrid.AddCheckBox("Add Parameters Automatically", null), CheckBox.IsCheckedProperty, mAction, nameof(Act.AddNewReturnParams));
            BindingHandler.ObjFieldBinding(xOutputValuesGrid.AddCheckBox("Support Simulation", new RoutedEventHandler(RefreshOutputColumns)), CheckBox.IsCheckedProperty, mAction, nameof(Act.SupportSimulation));

            xOutputValuesGrid.ShowViewCombo = Visibility.Collapsed;
            xOutputValuesGrid.ShowEdit = Visibility.Collapsed;

            xOutputValuesGrid.AllowHorizentalScroll = true;
            xOutputValuesGrid.Grid.MaxHeight = 500;

            xOutputValuesGrid.DataSourceList = mAction.ReturnValues;
        }

        private void GenerateStoreToVarsList()
        {
            List<string> tempList = new List<string>();
            if (mActParentBusinessFlow != null)
            {
                tempList = mActParentBusinessFlow.GetAllVariables(mActParentActivity).Where(a => a.VariableType == "String").Select(a => a.Name).ToList();
            }
            else
            {
                tempList = WorkSpace.Instance.Solution.Variables.Where(a => a.VariableType == "String").Select(a => a.Name).ToList();
                if (mActParentActivity != null)
                {
                    foreach (GingerCore.Variables.VariableBase var in mActParentActivity.Variables)
                    {
                        tempList.Add(var.Name);
                    }
                }
            }
            tempList.Sort();
            if (tempList.Count > 0)
            {
                tempList.Insert(0, string.Empty);//to be used for clearing selection
            }

            //mStoreToVarsList.LoadDataFromList(tempList, true);

            //Add new
            foreach (string var in tempList)
            {
                if (mStoreToVarsList.Contains(var) == false)
                {
                    mStoreToVarsList.Add(var);
                }
            }

            //remove old
            for (int indx = 0; indx < mStoreToVarsList.Count; indx++)
            {
                if (tempList.Contains(mStoreToVarsList[indx]) == false)
                {
                    mStoreToVarsList.RemoveAt(indx);
                    indx--;
                }
            }

            if (mStoreToVarsList.Count > 0)
            {
                mStoreToVarsList.Move(mStoreToVarsList.IndexOf(string.Empty), 0);//making sure the empty option is first
            }
        }

        private void SetActInputValuesGrid()
        {
            //Show/hide if needed
            xInputValuesGrid.SetTitleLightStyle = true;
            xInputValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddInputValue));//?? going to be hide in next line code

            xInputValuesGrid.ClearTools();
            xInputValuesGrid.ShowDelete = System.Windows.Visibility.Visible;

            //List<GridColView> view = new List<GridColView>();
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["InputValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Value ForDriver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            xInputValuesGrid.SetAllColumnsDefaultView(view);
            xInputValuesGrid.InitViewItems();

            xInputValuesGrid.DataSourceList = mAction.InputValues;
        }

        private void LoadOperationSettingsEditPage(Act a)
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
                    xActionPrivateConfigsFrame.SetContent(p);
                    xActionPrivateConfigsFrame.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                xActionPrivateConfigsFrame.Visibility = System.Windows.Visibility.Collapsed;
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

        //private async void RunActionInSimulationButton_Click(object sender, RoutedEventArgs e)
        //{
        //    bool originalSimulationFlagValue = mContext.Runner.RunInSimulationMode;
        //    mContext.Runner.RunInSimulationMode = true;

        //    int res = await RunAction().ConfigureAwait(false);

        //    mContext.Runner.RunInSimulationMode = originalSimulationFlagValue;
        //}

        //private async void RunActionButton_Click(object sender, RoutedEventArgs e)
        //{
        //    bool originalSimulationFlagValue = mContext.Runner.RunInSimulationMode;
        //    mContext.Runner.RunInSimulationMode = false;

        //    int res = await RunAction().ConfigureAwait(false);

        //    mContext.Runner.RunInSimulationMode = originalSimulationFlagValue;
        //}

        //private void StopRunBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.StopRun, null);
        //}

        //private void ShowHideRunStopButtons()
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        if (mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running)
        //        {
        //            mRunActionBtn.Visibility = Visibility.Collapsed;
        //            mStopRunBtn.Visibility = Visibility.Visible;
        //        }
        //        else
        //        {
        //            mRunActionBtn.Visibility = Visibility.Visible;
        //            mStopRunBtn.Visibility = Visibility.Collapsed;
        //        }
        //    });
        //}

        //private async Task<int> RunAction()
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        mAction.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
        //        if (mAction.GetType() == typeof(ActLowLevelClicks))
        //            App.MainWindow.WindowState = WindowState.Minimized;
        //        mAction.IsSingleAction = true;

        //        App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.SetupRunnerForExecution, null);

        //        //No need for agent for some actions like DB and read for excel. For other need agent   
        //        if (!(typeof(ActWithoutDriver).IsAssignableFrom(mAction.GetType())))
        //        {
        //            mContext.Runner.SetCurrentActivityAgent();
        //        }

        //        mContext.Runner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;
        //    });

        //    var result = await mContext.Runner.RunActionAsync(mAction, false, true).ConfigureAwait(false);

        //    this.Dispatcher.Invoke(() =>
        //    {
        //        mAction.IsSingleAction = false;
        //        //UpdateTabsHeaders();
        //        //UpdateScreenShots();
        //        Mouse.OverrideCursor = null;
        //    });
        //    return result;
        //}

        private void GridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.Expected, mContext);
            VEEW.ShowAsWindow();
        }
        private void InputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)xInputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), mContext);
            VEEW.ShowAsWindow();
        }
        private void GridAddActualToExpectButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ARV.Expected = ARV.Actual;
        }

        private void GridAddActualToSimulButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ARV.SimulatedActual = ARV.Actual;
        }

        private void SimulatedOutputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
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
                    //winButtons.Add(undoBtn);

                    //mRunActionBtn.Content = "Run";
                    //mRunActionBtn.Click += new RoutedEventHandler(RunActionButton_Click);
                    //mRunActionBtn.Margin = new Thickness(0, 0, 60, 0);
                    //winButtons.Add(mRunActionBtn);
                    //mSimulateRunBtn.Content = "Simulate Run";
                    //mSimulateRunBtn.Click += new RoutedEventHandler(RunActionInSimulationButton_Click);
                    //ShowHideRunSimulation();
                    //winButtons.Add(mSimulateRunBtn);

                    //mStopRunBtn.Content = "Stop";
                    //mStopRunBtn.Click += new RoutedEventHandler(StopRunBtn_Click);
                    //mStopRunBtn.Margin = new Thickness(0, 0, 60, 0);
                    //winButtons.Add(mStopRunBtn);
                    //mStopRunBtn.Visibility = Visibility.Collapsed;
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
            //ShowHideRunStopButtons();
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, closeContent, closeHandler, startupLocationWithOffset: startupLocationWithOffset);
            SwitchingInputValueBoxAndGrid(mAction);
            return saveWasDone;
        }

        private void SetViewMode()
        {
            xActionsDetailsPnl.IsEnabled = false;

            xOperationSettingsPnl.IsEnabled = false;

            xWaitTimeoutPnl.IsEnabled = false;
            xRetryMechanismPnl.IsEnabled = false;

            xAddOutputToDataSourcePnl.IsEnabled = false;
            xDataSourceConfigGrid.ToolsTray.Visibility = Visibility.Collapsed;
            xDataSourceConfigGrid.DisableGridColoumns();
            xOutputValuesGrid.ToolsTray.Visibility = Visibility.Collapsed;
            xOutputValuesGrid.DisableGridColoumns();

            xExecutionReportConfigPnl.IsEnabled = false;
            //xActionRunDetailsPnl.IsEnabled = false;
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
            //mAction.OnPropertyChanged(Act.Fields.Details);
            //mAction.OnPropertyChanged(Act.Fields.FlowControls);
            IsPageClosing = true;
            _pageGenericWin.Close();
        }

        private void SharedRepoSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckIfUserWantToSave();
        }

        private void ParentSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if ((mActParentBusinessFlow != null && Reporter.ToUser(eUserMsgKey.SaveItemParentWarning, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), mActParentBusinessFlow.Name) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                || (mActParentActivity != null && Reporter.ToUser(eUserMsgKey.SaveItemParentWarning, GingerDicser.GetTermResValue(eTermResKey.Activity), mActParentActivity.ActivityName) == Amdocs.Ginger.Common.eUserMsgSelection.Yes))
            {
                if (mActParentBusinessFlow != null)
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
            if (xLocateByCombo.SelectedItem != null && xLocateByCombo.SelectedItem.ToString() == "ByMulitpleProperties")
            {
                sMultiLocatorVals = xLocateValueVE.ValueTextBox.Text;
                xLocateValueVE.Background = System.Windows.Media.Brushes.LightGray;
                xEditLocatorBtn.Width = 30;
                if (mAction.LocateBy != eLocateBy.ByMulitpleProperties)
                {
                    EditLocatorsWindow.sMultiLocatorVals = xLocateValueVE.ValueTextBox.Text;
                    EditLocatorsWindow ELW = new EditLocatorsWindow(mActParentBusinessFlow);
                    ELW.ShowDialog();
                    xLocateValueVE.ValueTextBox.Text = EditLocatorsWindow.sMultiLocatorVals;
                }
            }
            else
            {
                xLocateValueVE.Background = System.Windows.Media.Brushes.White;
                xEditLocatorBtn.Width = 0;
            }
        }

        private void btnEditLocator_Click(object sender, RoutedEventArgs e)
        {
            EditLocatorsWindow.sMultiLocatorVals = xLocateValueVE.ValueTextBox.Text;
            EditLocatorsWindow ELW = new EditLocatorsWindow(mActParentBusinessFlow);
            ELW.ShowDialog();
            xLocateValueVE.ValueTextBox.Text = EditLocatorsWindow.sMultiLocatorVals;
        }

        private void GridParamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.Param, mContext);
            VEEW.ShowAsWindow();
        }
        private void GridPathVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.Path, mContext);
            VEEW.ShowAsWindow();
        }

        private void actionInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            ActDescriptionPage desPage = new ActDescriptionPage(mAction);
            desPage.ShowAsWindow();
        }

        private void xActionTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mAction == null)
            {
                return;
            }

            if (xActionTabs.SelectedItem == xDetailsTab && bool.Parse(xDetailsTab.Tag.ToString()) != true)
            {
                InitDetailsTabView();
            }
            else if (xActionTabs.SelectedItem == xOperationSettingsTab && bool.Parse(xOperationSettingsTab.Tag.ToString()) != true)
            {
                InitOperationSettingsTabView();
            }
            else if (xActionTabs.SelectedItem == xFlowControlTab && bool.Parse(xFlowControlTab.Tag.ToString()) != true)
            {
                InitFlowControlTabView();
            }
            else if (xActionTabs.SelectedItem == xOutputValuesTab && bool.Parse(xOutputValuesTab.Tag.ToString()) != true)
            {
                InitOutputValuesTabView();
            }
            else if (xActionTabs.SelectedItem == xExecutionReportTab && bool.Parse(xExecutionReportTab.Tag.ToString()) != true)
            {
                InitExecutionReportTabView();
            }
            else if (xActionTabs.SelectedItem == xHelpTab && bool.Parse(xHelpTab.Tag.ToString()) != true)
            {
                InitHelpTabView();
            }
        }

        private void UpdateScreenShots()
        {
            xScreenShotsViewPnl.Children.Clear();

            if (mAction != null && mAction.ScreenShots.Count > 0)
            {
                xScreenShotsPnl.Visibility = Visibility.Visible;

                for (int i = 0; i < mAction.ScreenShots.Count; i++)
                {
                    //TODO: clean me when Screenshots changed to class instead of list of strings
                    // just in case we don't have name, TOOD: fix all places where we add screen shots to include name
                    string Name = "";
                    if (mAction.ScreenShotsNames.Count > i)
                    {
                        Name = mAction.ScreenShotsNames[i];
                    }
                    ScreenShotViewPage screenShotPage = new ScreenShotViewPage(Name, mAction.ScreenShots[i], 0.5);
                    Frame fram = new Frame();
                    fram.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
                    fram.NavigationService.RemoveBackEntry();
                    fram.Margin = new Thickness(20);
                    DockPanel.SetDock(fram, Dock.Top);
                    fram.HorizontalAlignment = HorizontalAlignment.Center;
                    fram.VerticalAlignment = VerticalAlignment.Center;
                    fram.SetContent(screenShotPage);
                    xScreenShotsViewPnl.Children.Add(fram);
                }
            }
            else
            {
                xScreenShotsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void HighLightElementButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: fixme - Currently working with first agent
            ApplicationAgent aa = (ApplicationAgent)mContext.Runner.ApplicationAgents[0];
            if (aa != null)
            {
                DriverBase driver = ((Agent)aa.Agent).Driver;
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
            ApplicationAgent aa = (ApplicationAgent)mContext.Runner.ApplicationAgents.Where(x => x.AppName == mActParentActivity.TargetApplication).FirstOrDefault();
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
                    WindowExplorerPage WEP = new WindowExplorerPage(aa, mContext, mAction);
                    WEP.ShowAsWindow();
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Control selection is not available yet for driver - " + driver.GetType().ToString());
                }
            }
        }

        private void txtTimeout_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (xTimeoutTextBox.Text == String.Empty || xTimeoutTextBox.Text == null)
            {
                xTimeoutTextBox.Text = "0";
                xTimeoutTextBox.CaretIndex = 1;
            }
        }

        void UpdateTabsHeaders()
        {
            UpdateFlowControlsTabHeader();
            UpdateOutputValuesTabHeader();
        }

        //Output Tab
        void UpdateOutputValuesTabHeader()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (mAction.ReturnValues.Count() > 0)
                {
                    xOutputValuesTabHeaderTextBlock.Text = string.Format("Output Values ({0})", mAction.ReturnValues.Count());
                }
                else
                {
                    xOutputValuesTabHeaderTextBlock.Text = "Output Values";
                }
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

        private void UpdateFlowControlsTabHeader()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (mAction.FlowControls.Count() > 0)
                {
                    xFlowControlTabHeaderTextBlock.Text = string.Format("Flow Control ({0})", mAction.FlowControls.Count());
                }
                else
                {
                    xFlowControlTabHeaderTextBlock.Text = "Flow Control";
                }
            });
        }

        private void GridDSVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ActDataSourcePage ADSP = new ActDataSourcePage(ARV, ActReturnValue.Fields.StoreToDataSource);
            ADSP.ShowAsWindow();
        }

        private void xDataSourceExpander_Expanded(object sender, RoutedEventArgs e)
        {
            SetDataSourceConfigTabView();
        }

        private void AddOutDS_Checked(object sender, RoutedEventArgs e)
        {
            mDSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            if (mDSList.Count == 0)
                return;
            mDSNames.Clear();
            foreach (DataSourceBase ds in mDSList)
                mDSNames.Add(ds.Name);
            GingerCore.General.FillComboFromList(xDataSourceNameCombo, mDSNames);
            if (mAction.OutDataSourceName != null && mAction.OutDataSourceTableName != null && mAction.OutDataSourceName != "" && mAction.OutDataSourceTableName != "")
            {
                xDataSourceNameCombo.SelectedValue = mAction.OutDataSourceName;
                xDataSourceTableNameCombo.SelectedValue = mAction.OutDataSourceTableName;
            }
            else
            {
                xDataSourceNameCombo.SelectedIndex = 0;
                mDataSourceName = mDSNames[0];
            }

            if (mAction.OutDSParamMapType == null || mAction.OutDSParamMapType == "")
            {
                dsOutputParamMapType.SelectedValue = Act.eOutputDSParamMapType.ParamToRow;
            }
            else
                dsOutputParamMapType.SelectedValue = mAction.OutDSParamMapType;

            SetDataSourceConfigTabView();

            UpdateOutputValuesTabHeader();
        }

        private void AddOutDS_Unchecked(object sender, RoutedEventArgs e)
        {
            mAction.DSOutputConfigParams.Clear();
            SetDataSourceConfigTabView();
            UpdateOutputValuesTabHeader();
        }
        private void SetDataSourceConfigTabView()
        {
            if (mAction.ConfigOutputDS)
            {
                xAddOutputToDataSourceConfigPnl.Visibility = Visibility.Visible;
            }
            else
            {
                xAddOutputToDataSourceConfigPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void updateDSOutGrid()
        {
            if (xDataSourceTableNameCombo == null || xDataSourceTableNameCombo.Items.Count == 0 || xDataSourceTableNameCombo.SelectedValue == null)
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
                    foreach (ActOutDataSourceConfig oDSParam in DSConfigParam)
                    {
                        mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, oDSParam.OutputType, oDSParam.TableColumn, "", mColNames, mAction.OutDSParamMapType);
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

            xDataSourceConfigGrid.Visibility = Visibility.Visible;

            SetActDataSourceConfigGrid();
            mAction.DSOutputConfigParams = aOutDSConfigParam;
            xDataSourceConfigGrid.DataSourceList = mAction.DSOutputConfigParams;
        }
        private void OutDSParamType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            updateDSOutGrid();
        }

        private void cmbDataSourceTableName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xDataSourceTableNameCombo == null || xDataSourceTableNameCombo.Items.Count == 0 || xDataSourceTableNameCombo.SelectedValue == null)
                return;
            foreach (DataSourceTable dst in mDSTableList)
            {
                if (dst.Name == xDataSourceTableNameCombo.SelectedValue.ToString())
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
                    else if (currRow.OutputType == "Parameter")
                        mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter_Path.ToString(), "GINGER_KEY_NAME", (!currRow.Active).ToString());
                    else
                        mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter.ToString(), "GINGER_KEY_NAME", (!currRow.Active).ToString());
                    xDataSourceConfigGrid.DataSourceList = mAction.DSOutputConfigParams;
                }
            }
        }

        private void cmbDataSourceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xDataSourceNameCombo == null || xDataSourceNameCombo.Items.Count == 0 || xDataSourceNameCombo.SelectedValue == null)
                return;
            foreach (DataSourceBase ds in mDSList)
            {
                if (ds.Name == xDataSourceNameCombo.SelectedValue.ToString())
                {
                    mDataSourceName = xDataSourceNameCombo.SelectedValue.ToString();
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
                        if (xDataSourceTableNameCombo.SelectedValue != null && xDataSourceTableNameCombo.SelectedValue.ToString() == dst.Name)
                            mDSTable = dst;
                    }

                    if (mDSTableList.Count == 0)
                        return;
                    GingerCore.General.FillComboFromList(xDataSourceTableNameCombo, dsTableNames);
                    if (xDataSourceTableNameCombo.SelectedValue == null)
                    {
                        mDSTable = mDSTableList[0];
                        xDataSourceTableNameCombo.SelectedIndex = 0;
                    }
                    break;
                }
            }
        }

        //private void ShowHideRunSimulation()
        //{
        //    if (mAction.SupportSimulation)
        //        mSimulateRunBtn.Visibility = Visibility.Visible;
        //    else
        //        mSimulateRunBtn.Visibility = Visibility.Collapsed;
        //}

        private void ActionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Act.Status))
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (mAction.Status== Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running)
                    {
                        mAction.ReturnValues.CollectionChanged -= ReturnValues_CollectionChanged;
                        xOutputValuesGrid.DataSourceList = null;
                    }
                    else
                    {
                        mAction.ReturnValues.CollectionChanged -= ReturnValues_CollectionChanged;
                        mAction.ReturnValues.CollectionChanged += ReturnValues_CollectionChanged;
                        xOutputValuesGrid.DataSourceList = mAction.ReturnValues;
                        ReturnValues_CollectionChanged(null, null);
                    }
                });
            }
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
            xActionLogConfigFrame.SetContent(new ActionLogConfigPage(mAction.ActionLogConfig));
        }

        private void EnableActionLogConfigCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mAction.EnableActionLogConfig = true;
            if (mAction.ActionLogConfig == null)
            {
                mAction.ActionLogConfig = new ActionLogConfig();
            }
            ResetActionLog();
        }

        private void EnableActionLogConfigCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            mAction.EnableActionLogConfig = false;
            ResetActionLog();
        }

        private void ResetActionLog()
        {
            if (mAction.EnableActionLogConfig)
            {
                ShowActionLogConfig();
            }
            else
            {
                xActionLogConfigFrame.SetContent(null);
            }
        }

        private void GridClearExpectedValueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xOutputValuesGrid.Grid.SelectedItem != null)
            {
                ((ActReturnValue)xOutputValuesGrid.Grid.SelectedItem).Expected = null;
            }
        }

        public void ClearPageBindings()
        {
            StopEdit();
            BindingOperations.ClearAllBindings(xDescriptionTextBox);
            BindingOperations.ClearAllBindings(xLocateByCombo);
            BindingOperations.ClearAllBindings(xWindowsToCaptureCombo);
            BindingOperations.ClearAllBindings(xLocateValueVE);
            BindingOperations.ClearAllBindings(xExecutionTimeLbl);
            BindingOperations.ClearAllBindings(xExecutionErrorDetailsText);
            BindingOperations.ClearAllBindings(xExecutionExtraInfoText);
            BindingOperations.ClearAllBindings(xLocateValueVE);
            BindingOperations.ClearAllBindings(xTakeScreenShotCheckBox);
            BindingOperations.ClearAllBindings(xFailIgnoreCheckBox);
            BindingOperations.ClearAllBindings(xStatusConvertorCombo);
            BindingOperations.ClearAllBindings(xWaitVeUC);
            BindingOperations.ClearAllBindings(xTimeoutTextBox);
            BindingOperations.ClearAllBindings(xEnableRetryMechanismCheckBox);
            BindingOperations.ClearAllBindings(xRetryMechanismIntervalTextBox);
            BindingOperations.ClearAllBindings(xRetryMechanismMaxRetriesTextBox);
            xAddOutToDSCheckbox.Unchecked -= AddOutDS_Unchecked;
            BindingOperations.ClearAllBindings(xAddOutToDSCheckbox);
            BindingOperations.ClearAllBindings(xDataSourceNameCombo);
            BindingOperations.ClearAllBindings(xDataSourceTableNameCombo);
            BindingOperations.ClearAllBindings(dsOutputParamMapType);
            BindingOperations.ClearAllBindings(xEnableActionLogConfigCheckBox);
            BindingOperations.ClearAllBindings(xLocateValueVE);
            xTagsViewer.ClearBinding();
            //this.ClearControlsBindings();
            if (mAction != null)
            {
                mAction.PropertyChanged -= ActionPropertyChanged;
                mAction.InputValues.CollectionChanged -= InputValues_CollectionChanged;
                mAction.FlowControls.CollectionChanged -= FlowControls_CollectionChanged;
                mAction.ReturnValues.CollectionChanged -= ReturnValues_CollectionChanged;
                mAction.ScreenShots.CollectionChanged -= ScreenShots_CollectionChanged;
                mAction = null;
            }
            xFlowControlConditionsFrame.NavigationService.RemoveBackEntry();
            xActionPrivateConfigsFrame.NavigationService.RemoveBackEntry();
            xActionHelpDetailsFram.NavigationService.RemoveBackEntry();
            xActionLogConfigFrame.NavigationService.RemoveBackEntry();
        }

        private void XEnableRetryMechanismCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            SetRetryMechConfigsPnlView();
        }

        private void SetRetryMechConfigsPnlView()
        {
            if (xEnableRetryMechanismCheckBox.IsChecked == true)
            {
                xRetryMechConfigsPnl.Visibility = Visibility.Visible;
                xRetryExpander.IsExpanded = true;
            }
            else
            {
                xRetryMechConfigsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void XTakeScreenShotCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            SetScreenshotsPnlView();
        }

        private void SetScreenshotsPnlView()
        {
            if (xTakeScreenShotCheckBox.IsChecked == true)
            {
                xScreenshotsCaptureTypeConfigsPnl.Visibility = Visibility.Visible;
            }
            else
            {
                xScreenshotsCaptureTypeConfigsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void XHelpButton_Click(object sender, RoutedEventArgs e)
        {
            xActionTabs.SelectedItem = xHelpTab;
        }
    }
}
