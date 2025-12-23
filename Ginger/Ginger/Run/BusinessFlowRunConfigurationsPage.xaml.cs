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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using Ginger.Variables;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static Ginger.UserControlsLib.UCDataMapping;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for BusinessFlowRunVariablesPage.xaml
    /// </summary>
    public partial class BusinessFlowRunConfigurationsPage : Page
    {
        private enum eWindowMode
        {
            Configuration, SummaryView
        }
        private eWindowMode mWindowMode;

        private BusinessFlow mBusinessFlow;

        private BusinessFlowExecutionSummary mBusinessFlowExecSummary;

        public EventHandler EventRaiseVariableEdit;

        GenericWindow _pageGenericWin = null;

        public GingerRunner mGingerRunner;
        Context mContext;

        private ProcessInputVariableRule processInputVariable;

        private readonly General.eRIPageViewMode _viewMode;

        public BusinessFlowRunConfigurationsPage(GingerRunner runner, BusinessFlow businessFlow, General.eRIPageViewMode viewMode)
        {
            mBusinessFlow = businessFlow;
            InitializeComponent();

            _viewMode = viewMode;
            mWindowMode = eWindowMode.Configuration;

            mGingerRunner = runner;
            mContext = new Context() { BusinessFlow = businessFlow, Runner = runner.Executor };

            mBusinessFlow.SaveBackup();

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(MandatoryBusinessFlowCB, CheckBox.IsCheckedProperty, businessFlow, nameof(BusinessFlow.Mandatory), BindingMode.TwoWay);

            RunDescritpion.Init(mContext, businessFlow, nameof(BusinessFlow.RunDescription));
            MandatoryBusinessFlowCB.Click += MandatoryBusinessFlow_Clicked;

            RunDescritpion.ValueTextBox.TextChanged += RunDescription_TextChanged;
            grdVariables.btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditVar));
            grdVariables.AddToolbarTool("@Undo_16x16.png", "Reset " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " to Original Configurations", new RoutedEventHandler(ResetBusFlowVariables));
            grdVariables.AddToolbarTool("@Share_16x16.png", "Share Selected " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Value to all Similar " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " in " + GingerDicser.GetTermResValue(eTermResKey.RunSet), new RoutedEventHandler(CopyBusFlowVariables));
            grdVariables.AddToolbarTool(Amdocs.Ginger.Common.Enums.eImageType.Rules, "Rules page", new RoutedEventHandler(ShowRulesPage));
            // Removed: grdVariables.RowDoubleClick += VariablesGrid_grdMain_MouseDoubleClick;
            // Double-click to edit variables is disabled in Run Set Configurations to prevent unintended variable edits

            SetVariablesGridView();
            LoadGridData();
            UpdateEditVariablesTabVisual();

            LoadBusinessFlowcontrols(businessFlow);
            UpdateFlowControlTabVisual();
            CollectionChangedEventManager.AddHandler(source: mBusinessFlow.BFFlowControls, handler: BFFlowControls_CollectionChanged);

            bool editable = _viewMode is not General.eRIPageViewMode.View and not General.eRIPageViewMode.ViewAndExecute;
            SetViewMode(editable);
        }

        private void MandatoryBusinessFlow_Clicked(object sender, RoutedEventArgs e)
        {
            if (EventRaiseVariableEdit != null)
            {
                EventRaiseVariableEdit(null, null);
            }

        }

        private void RunDescription_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (EventRaiseVariableEdit != null)
            {
                EventRaiseVariableEdit(null, null);
            }
        }

        private void SetViewMode(bool editable)
        {
            btnAutoCreateDescription.IsEnabled = editable;
            RunDescritpion.IsEnabled = editable;
            MandatoryBusinessFlowCB.IsEnabled = editable;
            grdVariables.IsReadOnly = !editable;

            Visibility visibility = editable ? Visibility.Visible : Visibility.Collapsed;
            grdVariables.ToolsTray.Visibility = visibility;

            if (!editable)
            {
                grdVariables.DisableGridColoumns();
            }
        }

        private void LoadBusinessFlowcontrols(BusinessFlow businessFlow)
        {
            FlowControlFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
            BusinessFlowRunFlowControlPage BFCP = new BusinessFlowRunFlowControlPage(mGingerRunner, businessFlow, _viewMode);
            FlowControlFrame.ClearAndSetContent(BFCP);

        }
        public BusinessFlowRunConfigurationsPage(BusinessFlowExecutionSummary businessFlowExecSummary)
        {
            InitializeComponent();

            mWindowMode = eWindowMode.SummaryView;
            btnAutoCreateDescription.Visibility = System.Windows.Visibility.Collapsed;

            SetVariablesGridView();
            LoadGridData();
        }

        private void SetVariablesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(VariableBase.Image), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 2.5, MaxWidth = 20 },
                new GridColView() { Field = nameof(VariableBase.Name), Header = "Name", WidthWeight = 20, ReadOnly = true, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = nameof(VariableBase.MandatoryIndication), Header = " ", WidthWeight = 1, ReadOnly = true, BindingMode = BindingMode.OneWay, Style = FindResource("$GridColumnRedTextStyle") as Style },
                new GridColView() { Field = nameof(VariableBase.InitialValue), Header = "Initial Value", WidthWeight = 20, BindingMode = BindingMode.OneWay, ReadOnly = true },
            ]
            };
            if (mWindowMode == eWindowMode.Configuration)
            {
                view.GridColsView.Add(new GridColView()
                {
                    Field = nameof(VariableBase.MappedOutputValue),
                    Header = "Mapped Runtime Value",
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = UCDataMapping.GetTemplate(new UCDataMapping.TemplateOptions(
                        dataTypeProperty: nameof(VariableBase.MappedOutputType),
                        dataValueProperty: nameof(VariableBase.MappedOutputValue))
                    {
                        _EnableDataMappingProperty = nameof(VariableBase.SupportSetValue),
                        _VariabelsSourceProperty = nameof(VariableBase.PossibleVariables),
                        _OutputVariabelsSourceProperty = nameof(VariableBase.PossibleOutputVariables),

                        _RestrictedMappingTypes = new List<UCDataMapping.RestrictedMappingType>
                {
                    new UCDataMapping.RestrictedMappingType(nameof(eDataType.Variable), "Direct variables are not allowed in Runset configuration.")
                }

                    }),
                    WidthWeight = 40,

                });
            }
            else if (mWindowMode == eWindowMode.SummaryView)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.MappedOutputValue), Header = "Mapped Runtime Value", BindingMode = BindingMode.OneWay, ReadOnly = true, WidthWeight = 40 });
            }
            //view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.ParentType), Header = "Level", WidthWeight = 10, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.ParentName), Header = "Path", WidthWeight = 20, ReadOnly = true });
            grdVariables.SetAllColumnsDefaultView(view);
            grdVariables.InitViewItems();

            if (mWindowMode == eWindowMode.SummaryView)
            {
                grdVariables.ShowEdit = System.Windows.Visibility.Collapsed;
            }
        }

        private void LoadGridData()
        {
            //TODO: check if Summary view is used...
            switch (mWindowMode)
            {
                case eWindowMode.Configuration:
                    processInputVariable = new ProcessInputVariableRule(mBusinessFlow, mGingerRunner);
                    grdVariables.Title = "'" + mBusinessFlow.Name + "' Run " + GingerDicser.GetTermResValue(eTermResKey.Variables);
                    ObservableList<VariableBase> bfInputVariables = mBusinessFlow.GetBFandActivitiesVariabeles(true, true);

                    //**Legacy--- set the Variabels can be used- user should use Global Variabels/ Output Variabels instead
                    ObservableList<string> optionalVars =
                    [
                        string.Empty,//default value for clear selection
                    ];
                    foreach (VariableBase var in ((GingerExecutionEngine)mGingerRunner.Executor).GetPossibleOutputVariables(WorkSpace.Instance.RunsetExecutor.RunSetConfig, mBusinessFlow, includeGlobalVars: true, includePrevRunnersVars: false))
                    {
                        optionalVars.Add(var.Name);
                    }
                    //allow setting  vars options only to variables types which supports setting value
                    foreach (VariableBase inputVar in bfInputVariables)
                    {
                        if (inputVar.SupportSetValue)
                        {
                            inputVar.PossibleVariables = optionalVars;
                        }
                    }

                    //Set Output Variabels can be used
                    ObservableList<VariableBase> optionalOutputVars =
                    [
                        .. ((GingerExecutionEngine)mGingerRunner.Executor).GetPossibleOutputVariables(WorkSpace.Instance.RunsetExecutor.RunSetConfig, mBusinessFlow, includeGlobalVars: false, includePrevRunnersVars: true),
                    ];
                    //allow setting output vars options only to variables types which supports setting value
                    foreach (VariableBase inputVar in bfInputVariables)
                    {
                        if (inputVar.SupportSetValue)
                        {
                            inputVar.PossibleOutputVariables = optionalOutputVars;
                        }
                    }

                    processInputVariable.GetVariablesByRules(bfInputVariables);
                    grdVariables.DataSourceList = VariableBase.SortByMandatoryInput(new ObservableList<VariableBase>(bfInputVariables));
                    break;

                case eWindowMode.SummaryView:
                    grdVariables.Title = "'" + mBusinessFlowExecSummary.BusinessFlowName + "' Run " + GingerDicser.GetTermResValue(eTermResKey.Variables);
                    grdVariables.DataSourceList = mBusinessFlowExecSummary.ExecutionVariabeles;
                    break;
            }
        }


        private void CopyBusFlowVariables(object sender, RoutedEventArgs e)
        {
            if (grdVariables.CurrentItem != null)
            {
                if (Reporter.ToUser(eUserMsgKey.AskIfShareVaribalesInRunner) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    int countMatchingBfs = 0;

                    List<VariableBase> selectedVars = grdVariables.Grid.SelectedItems.Cast<VariableBase>().ToList();
                    foreach (GingerRunner runner in WorkSpace.Instance.RunsetExecutor.Runners)
                    {
                        List<BusinessFlow> matchingBfs = ((GingerExecutionEngine)runner.Executor).BusinessFlows.Where(x => x.Guid == mBusinessFlow.Guid).ToList();
                        countMatchingBfs += matchingBfs.Count;
                        foreach (BusinessFlow bf in matchingBfs)
                        {
                            foreach (VariableBase selectedVar in selectedVars)
                            {

                                VariableBase matchingVar = bf.GetBFandActivitiesVariabeles(true).FirstOrDefault(x => x.Guid == selectedVar.Guid);
                                if (matchingVar != null)
                                {
                                    String originalValue = matchingVar.Value;
                                    String originalFormula = matchingVar.Formula;

                                    VariableBase copiedVar = (VariableBase)selectedVar.CreateCopy(); ;
                                    if (selectedVar.ParentType == "Business Flow")
                                    {
                                        int indexSelected = bf.Variables.IndexOf(matchingVar);
                                        bf.Variables.Remove(matchingVar);
                                        bf.Variables.Insert(indexSelected, copiedVar);
                                    }
                                    if (selectedVar.ParentType == "Activity")
                                    {
                                        Activity a = bf.GetActivity(selectedVar.ParentGuid);
                                        int indexSelected = a.Variables.IndexOf(matchingVar);
                                        a.Variables.Remove(matchingVar);
                                        a.Variables.Insert(indexSelected, copiedVar);
                                    }
                                    if (copiedVar.Formula != originalFormula || copiedVar.Value != originalValue) //variable was changed
                                    {
                                        copiedVar.VarValChanged = true;
                                        copiedVar.DiffrentFromOrigin = true;
                                    }
                                }
                            }
                        }
                    }
                    Reporter.ToUser(eUserMsgKey.CopiedVariableSuccessfully, countMatchingBfs - 1);
                }

            }
            else
            {
                Reporter.ToUser(eUserMsgKey.ShareVariableNotSelected);
            }
        }

        private void ShowRulesPage(object sender, RoutedEventArgs e)
        {
            BusinessFlow cachedBusinessFlow = WorkSpace.Instance?.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(mBusinessFlow.Guid);
            InputVariablesRules inputVariableRule = new InputVariablesRules(cachedBusinessFlow, true);
            inputVariableRule.ShowAsWindow();
        }

        private void ResetBusFlowVariables(object sender, RoutedEventArgs e)
        {
            try
            {
                BusinessFlow originalBF = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(mBusinessFlow.Guid);
                if (originalBF == null)
                {
                    originalBF = (from bf in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>() where bf.Name == mBusinessFlow.Name select bf).FirstOrDefault();
                }

                if (originalBF == null)
                {
                    Reporter.ToUser(eUserMsgKey.ResetBusinessFlowRunVariablesFailed, "Original " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " was not found");
                    return;
                }
                else
                {
                    BusinessFlow originalBFCopy = (BusinessFlow)originalBF.CreateCopy(false);
                    mBusinessFlow.Variables = originalBFCopy.Variables;
                    mBusinessFlow.Activities = originalBFCopy.Activities;
                    LoadGridData();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.ResetBusinessFlowRunVariablesFailed, ex.Message);
            }
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            grdVariables.grdMain.ItemsSource = null;
            grdVariables.grdMain.ItemsSource = grdVariables.DataSourceList;
        }

        private void EditVar(object sender, RoutedEventArgs e)
        {
            if (grdVariables.CurrentItem != null && grdVariables.CurrentItem.ToString() != "{NewItemPlaceholder}")
            {
                EditVar();
                RefreshGrid(sender, e);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectVariable);
            }
        }

        private void EditVar()
        {
            VariableBase varToEdit = (VariableBase)grdVariables.CurrentItem;
            string originalFormula = varToEdit.Formula;
            string originalValue = varToEdit.Value;
            VariableEditPage w = new VariableEditPage(varToEdit, new Context() { BusinessFlow = mBusinessFlow }, true);
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            if (varToEdit.Formula != originalFormula || varToEdit.Value != originalValue)//variable was changed
            {
                varToEdit.VarValChanged = true;
                varToEdit.DiffrentFromOrigin = true;
                if (EventRaiseVariableEdit != null)
                {
                    EventRaiseVariableEdit(null, null);
                }

                processInputVariable = new ProcessInputVariableRule(mBusinessFlow, mGingerRunner);
                ObservableList<VariableBase> bfInputVariables = mBusinessFlow.GetBFandActivitiesVariabeles(true, true);
                ResetVariableValuesToDefault(bfInputVariables);
                processInputVariable.GetVariablesByRules(bfInputVariables);
                grdVariables.DataSourceList = VariableBase.SortByMandatoryInput(new ObservableList<VariableBase>(bfInputVariables));
            }

            UpdateEditVariablesTabVisual();
        }

        private void ResetVariableValuesToDefault(ObservableList<VariableBase> bfInputVariables)
        {
            //Revert selection list optional values to original list. 
            BusinessFlow cachedBusinessFlow = WorkSpace.Instance?.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(mBusinessFlow.Guid);
            ObservableList<VariableBase> cachedVariables = cachedBusinessFlow.GetBFandActivitiesVariabeles(true);
            foreach (VariableBase variable in bfInputVariables)
            {
                VariableBase vb = cachedVariables.FirstOrDefault(x => x.Guid == variable.Guid);
                if (vb != null && vb.GetType() == typeof(VariableSelectionList))
                {
                    ((VariableSelectionList)variable).OptionalValuesList = [];
                    foreach (OptionalValue values in ((VariableSelectionList)vb).OptionalValuesList)
                    {
                        ((VariableSelectionList)variable).OptionalValuesList.Add(new OptionalValue(values.Value));
                    }
                }
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            switch (mWindowMode)
            {
                case eWindowMode.Configuration:
                    Button okBtn = new Button
                    {
                        Content = "Ok"
                    };
                    WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: okBtn, eventName: nameof(ButtonBase.Click), handler: okBtn_Click);
                    Button undoBtn = new Button
                    {
                        Content = "Undo & Close"
                    };
                    WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: undoBtn, eventName: nameof(ButtonBase.Click), handler: undoBtn_Click);
                    ObservableList<Button> winButtons = [okBtn, undoBtn];

                    this.Width = 800;
                    this.Height = 800;
                    GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Edit " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Run Configurations", this, winButtons, false, "Undo & Close", CloseWinClicked);
                    break;

                case eWindowMode.SummaryView:
                    this.Width = 800;
                    this.Height = 800;
                    GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Run Configurations", this, null, true, "Close");
                    break;
            }
        }

        private void UndoChangesAndClose()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            mBusinessFlow.RestoreFromBackup(true);
            Mouse.OverrideCursor = null;

            _pageGenericWin.Close();
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
            if (mGingerRunner != null && mGingerRunner.Executor != null)
            {
                //commenting the below part out because causing issues with customized variables
                //mGingerRunner.Executor.UpdateBusinessFlowsRunList();
            }
            _pageGenericWin.Close();
        }

        private void btnAutoCreateDescription_Click(object sender, RoutedEventArgs e)
        {
            SetAutoDescription();
        }

        private void SetAutoDescription()
        {
            string autoDesc = string.Empty;
            List<VariableBase> bfVariables = (mBusinessFlow.GetBFandActivitiesVariabeles(true)).Where(var => var.GetType() == typeof(VariableSelectionList) || var.DiffrentFromOrigin == true || string.IsNullOrEmpty(var.MappedOutputVariable) == false || string.IsNullOrEmpty(var.MappedOutputValue) == false).ToList(); ;
            if (bfVariables != null && bfVariables.Count > 0)
            {
                autoDesc = "Running Configurations: ";
                foreach (VariableBase var in bfVariables)
                {
                    if (string.IsNullOrEmpty(var.MappedOutputVariable) == false)
                    {
                        autoDesc += "'" + var.Name + "' variable value mapped to '" + var.Value + "', ";
                    }
                    else
                    {
                        autoDesc += "'" + var.Name + "' variable value = '" + var.Value + "', ";
                    }
                }

                autoDesc = autoDesc.TrimEnd(new char[] { ',', ' ' });
            }
            else
            {
                autoDesc = "Default Run";
            }

            RunDescritpion.ValueTextBox.Text = autoDesc;
        }

        private void grdVariables_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void RunDescritpion_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void BusinessFlowTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set the selected tab text style
            try
            {
                if (BusinessFlowTab.SelectedItem != null)
                {
                    foreach (TabItem tab in BusinessFlowTab.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)
                        {
                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (BusinessFlowTab.SelectedItem == tab)
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                }
                                else
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$PrimaryColor_Black");
                                } ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Action Edit Page tabs style", ex);
            }


        }

        private void BFFlowControls_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateFlowControlTabVisual();
        }
        private void UpdateFlowControlTabVisual()
        {
            bool b = mBusinessFlow.BFFlowControls.Any();
            SetTabOnOffSign(FlowControlTab, b);
            if (b)
            {
                FlowControlCountLabel.Text = "(" + mBusinessFlow.BFFlowControls.Count + ")";
            }
            else
            {
                FlowControlCountLabel.Text = "";
            }
        }
        void UpdateEditVariablesTabVisual()
        {
            int count = 0;
            foreach (VariableBase var in mBusinessFlow.GetBFandActivitiesVariabeles(true, true))
            {
                if (var.DiffrentFromOrigin == true)
                {
                    count++;
                }
            }

            if (count > 0)
            {
                SetTabOnOffSign(EditVariablesTab, true);
                EditVariablesCountLabel.Text = "(" + count + ")";
            }
            else
            {
                SetTabOnOffSign(EditVariablesTab, false);
                EditVariablesCountLabel.Text = "";
            }
        }

        void SetTabOnOffSign(TabItem tab, bool indicatorToShow)
        {
            try
            {
                //set the selected tab text style
                if (tab != null)
                {
                    foreach (object ctrl in ((StackPanel)(tab.Header)).Children)
                    {
                        if (ctrl.GetType() == typeof(System.Windows.Controls.Image))
                        {
                            System.Windows.Controls.Image img = (System.Windows.Controls.Image)ctrl;
                            if (img.Tag != null)
                            {
                                if (img.Tag.ToString() == "OffSignImage")
                                {
                                    if (indicatorToShow)
                                    {
                                        img.Visibility = Visibility.Collapsed;
                                    }
                                    else
                                    {
                                        img.Visibility = Visibility.Visible;
                                    }
                                }
                                else if (img.Tag.ToString() == "OnSignImage")
                                {
                                    if (indicatorToShow)
                                    {
                                        img.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        img.Visibility = Visibility.Collapsed;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Configuration Page tabs style", ex);
            }
        }
    }
}
